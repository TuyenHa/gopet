using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nút tắt/bật âm thanh góc trên-phải — tính năng MỚI, không có trong jar gốc.
    /// Xem <see cref="SoundToggleButton"/> cho lý do dùng nhãn chữ thay vì icon.
    /// </summary>
    public sealed class SoundToggleButtonTests
    {
        private GameObject _root;
        private GameObject _eventSystem;
        private SoundManager _sound;
        private SoundToggleButton _button;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));

            _sound = SoundManager.Create(_root.transform);
            _button = SoundToggleButton.Create(_root.transform, _sound, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
            if (_eventSystem != null) Object.DestroyImmediate(_eventSystem);
            PlayerPrefs.DeleteKey("Gopet.SoundEnabled");
        }

        [Test]
        public void CoDinh_GocTrenPhaiManHinhThat()
        {
            // Neo theo TỈ LỆ màn hình (không phải pixel cố định) nên nút giữ nguyên
            // kích thước tương đối trên mọi cỡ máy; chỉ cần chắc nó nằm sát góc
            // trên-phải và có bề rộng thật.
            var rect = (RectTransform)_button.transform;

            Assert.Greater(rect.anchorMax.x, 0.9f, "Nút phải sát mép PHẢI.");
            Assert.Greater(rect.anchorMax.y, 0.9f, "Nút phải sát mép TRÊN.");
            Assert.Less(rect.anchorMin.x, rect.anchorMax.x, "Nút rộng 0 thì không bấm được.");
            Assert.Less(rect.anchorMin.y, rect.anchorMax.y);
        }

        [Test]
        public void Bam_TatTieng()
        {
            Click(_button.GetComponent<Button>());

            Assert.IsFalse(_sound.Enabled);
        }

        [Test]
        public void BamHaiLan_BatLai()
        {
            Click(_button.GetComponent<Button>());
            Click(_button.GetComponent<Button>());

            Assert.IsTrue(_sound.Enabled);
        }

        /// <summary>
        /// Hai trạng thái phải hiện HAI icon khác nhau. Quên gọi Refresh sau khi bấm
        /// thì âm tắt thật nhưng nút vẫn vẽ icon loa đang kêu — người chơi bấm lại,
        /// hoá ra là bật lên.
        /// </summary>
        [Test]
        public void BatVaTat_DoiIcon()
        {
            var image = _button.GetComponent<Image>();
            var whenOn = image.sprite;

            Click(_button.GetComponent<Button>());
            var whenOff = image.sprite;

            if (whenOn == null && whenOff == null)
            {
                // Chưa có bộ art: nút lùi về nhãn chữ, kiểm nhãn thay cho icon.
                Assert.AreEqual("TẮT", _button.GetComponentInChildren<Text>().text);
                return;
            }

            Assert.AreNotSame(whenOn, whenOff, "Bật và tắt đang dùng chung một icon.");
        }

        /// <summary>Nút phải luôn có Graphic, nếu không Button không nhận được cú chạm nào.</summary>
        [Test]
        public void Nut_LuonCoGraphicDeBatCuCham()
        {
            var image = _button.GetComponent<Image>();

            Assert.IsNotNull(image);
            Assert.IsTrue(image.raycastTarget);
        }

        private static void Click(Button button)
        {
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
        }
    }
}
