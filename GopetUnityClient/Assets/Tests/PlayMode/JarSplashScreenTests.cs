using System.Collections;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>Màn splash: tranh nền + nhạc, tự đóng sau một khoảng thời gian tối thiểu hoặc khi chạm.</summary>
    public sealed class JarSplashScreenTests
    {
        private GameObject _root;
        private GameObject _eventSystem;
        private PixelCanvas _pixelCanvas;
        private SoundManager _sound;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));

            _pixelCanvas = PixelCanvas.Create(_root.transform);
            _pixelCanvas.ApplyLayout(PixelCanvasLayout.Compute(1920f, 1080f));

            _sound = SoundManager.Create(_root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
            PlayerPrefs.DeleteKey("Gopet.SoundEnabled");
        }

        /// <summary>
        /// Ảnh nằm giữa màn hình và dùng EnvelopeParent: luôn phủ kín khung mà không
        /// kéo méo. Trên màn rộng hơn ảnh gốc, chỉ mép trên/dưới được cắt đều.
        /// </summary>
        [Test]
        public void Create_AnhNenCanGiua_PhuKinManHinh()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 60f);

            var fitter = splash.GetComponentInChildren<AspectRatioFitter>(true);
            Assert.IsNotNull(fitter, "Không tìm thấy lớp ảnh splash.");
            Assert.AreEqual(AspectRatioFitter.AspectMode.EnvelopeParent, fitter.aspectMode);

            var image = fitter.GetComponent<Image>();
            Assert.IsNotNull(image.sprite, $"Không nạp được Resources/{JarSplashScreen.PictureResource}.");
            Assert.AreEqual(image.sprite.rect.width / image.sprite.rect.height, fitter.aspectRatio, 0.01f);

            var rect = (RectTransform)fitter.transform;
            Assert.AreEqual(new Vector2(0.5f, 0.5f), rect.pivot);
        }

        /// <summary>
        /// Nền phải phủ ĐÚNG kích thước màn hình thật (canvas.transform), không phải
        /// khung đã bị letterbox của canvas.Content — đây chính là yêu cầu "ảnh full
        /// màn hình", không để lộ sọc màu ở hai mép.
        /// </summary>
        [Test]
        public void Nen_PhuDungManHinhThat_KhongBiLetterbox()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 60f);

            var backgroundRect = (RectTransform)splash.transform;
            Assert.AreSame(_pixelCanvas.transform, backgroundRect.parent,
                "Nền phải là con trực tiếp của canvas.transform (toàn màn hình), không phải canvas.Content (bị letterbox).");
            Assert.AreEqual(Vector2.zero, backgroundRect.anchorMin);
            Assert.AreEqual(Vector2.one, backgroundRect.anchorMax);
        }

        /// <summary>
        /// Tranh splash không được tạo thêm lớp trong khung pixel-art; nếu đặt nhầm
        /// vào đó, ảnh sẽ bị thu nhỏ theo khung 320×240 và có viền ngoài ý muốn.
        /// </summary>
        [UnityTest]
        public IEnumerator Dismiss_KhongDeLaiAnhTrongPixelContent()
        {
            var contentChildrenBefore = _pixelCanvas.Content.childCount;

            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 60f);
            Assert.AreEqual(contentChildrenBefore, _pixelCanvas.Content.childCount,
                "Ảnh splash bị đặt nhầm vào PixelCanvas.Content.");

            splash.OnPointerClick(new PointerEventData(EventSystem.current));
            yield return null;

            Assert.AreEqual(contentChildrenBefore, _pixelCanvas.Content.childCount,
                "Splash để lại phần tử ngoài ý muốn trong PixelCanvas.Content.");
        }

        [UnityTest]
        public IEnumerator BatDauThi_PhatNhac()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 60f);

            yield return null;

            var source = GetLoopingSource(_sound);
            Assert.IsTrue(source.isPlaying);
        }

        [UnityTest]
        public IEnumerator HetThoiGianToiThieu_TuDongDong()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 1f);

            var finished = false;
            splash.Finished += () => finished = true;

            // Kiểm TRƯỚC hạn: bẫy đã dính một lần — AddComponent chạy OnEnable trước
            // khi kịp gán _minimumSeconds, nên giá trị thật luôn là 0 và màn tự đóng
            // ngay bất kể tham số truyền vào. Chờ 0.2s (< 1s) rồi khẳng định CHƯA
            // đóng là ca duy nhất phân biệt được "đóng đúng giờ" với "đóng ngay do lỗi".
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(finished, "Đóng sớm hơn minimumSeconds — tham số bị bỏ qua.");

            yield return new WaitForSeconds(1f);
            Assert.IsTrue(finished);
        }

        [UnityTest]
        public IEnumerator ChoKetNoi_XongMoiDuocDongSplash()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 0.05f, waitForSignal: true);
            var finished = false;
            splash.Finished += () => finished = true;

            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(finished, "Splash đóng khi kiểm tra kết nối chưa xong.");

            splash.AllowFinish();
            Assert.IsTrue(finished);
        }

        [Test]
        public void ChamManHinh_DongNgaySomHonThoiGianToiThieu()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 60f);

            var finished = false;
            splash.Finished += () => finished = true;

            splash.OnPointerClick(new PointerEventData(EventSystem.current));

            Assert.IsTrue(finished);
        }

        /// <summary>Đóng hai lần (chạm rồi hết giờ, hoặc chạm hai lần) không được bắn sự kiện hai lần.</summary>
        [UnityTest]
        public IEnumerator DongHaiLan_ChiBanSuKienMotLan()
        {
            var splash = JarSplashScreen.Create(_pixelCanvas, _sound, minimumSeconds: 0.05f);

            var count = 0;
            splash.Finished += () => count++;

            splash.OnPointerClick(new PointerEventData(EventSystem.current));
            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(1, count);
        }

        private static AudioSource GetLoopingSource(SoundManager manager)
        {
            foreach (var source in manager.GetComponents<AudioSource>())
            {
                if (source.loop) return source;
            }

            Assert.Fail("Không tìm thấy AudioSource nhạc nền.");
            return null;
        }
    }
}
