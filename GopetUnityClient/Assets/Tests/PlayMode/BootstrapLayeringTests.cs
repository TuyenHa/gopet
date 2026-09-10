using System.Collections;
using Gopet.Runtime;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Thứ tự lớp của <see cref="GopetBootstrap"/>.
    ///
    /// <para><b>Vì sao đáng một test riêng:</b> server hỏi OTP đúng lúc client đang ở
    /// chặng "đang đăng nhập" (<c>Player.cs:400-403</c>), và màn hình chặng đó là một
    /// <see cref="FormView"/> phủ kín màn hình, có <c>Image</c> chắn cả tia raycast.
    /// uGUI vẽ theo thứ tự anh em, nên nếu <c>LoginScreens</c> dựng SAU
    /// <see cref="UiRoot"/> thì hộp OTP nằm dưới nó: vừa không nhìn thấy vừa không bấm
    /// được, và người chơi không đăng nhập được nữa. Không nhìn ra bằng mắt được vì
    /// hai thứ trông giống hệt nhau khi chưa có server thật.</para>
    /// </summary>
    public sealed class BootstrapLayeringTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);

            foreach (var canvas in Object.FindObjectsByType<Canvas>())
            {
                if (canvas != null) Object.DestroyImmediate(canvas.gameObject);
            }

            foreach (var events in Object.FindObjectsByType<EventSystem>())
            {
                if (events != null) Object.DestroyImmediate(events.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator ManDangNhap_NamDUOI_ChongDialogCuaServer()
        {
            _host = new GameObject("Bootstrap");
            var bootstrap = _host.AddComponent<GopetBootstrap>();

            yield return null;

            var login = Object.FindAnyObjectByType<LoginScreens>();
            Assert.IsNotNull(login, "Bootstrap không dựng màn đăng nhập.");
            Assert.IsNotNull(bootstrap.Ui, "Bootstrap không dựng UiRoot.");

            Assert.AreSame(login.transform.parent, bootstrap.Ui.transform.parent,
                "Hai thứ phải cùng một Canvas thì so thứ tự anh em mới có nghĩa.");

            Assert.Less(login.transform.GetSiblingIndex(), bootstrap.Ui.transform.GetSiblingIndex(),
                "Màn đăng nhập vẽ ĐÈ lên UiRoot — hộp OTP của server sẽ bị che và không bấm được.");
        }

        /// <summary>
        /// Canvas chung (chứa <see cref="UiRoot"/>) phải THẮNG canvas riêng của
        /// <see cref="PixelCanvas"/> khi so <c>sortingOrder</c> — nếu không, hộp OTP
        /// gửi qua <c>UiRoot</c> đúng lúc màn splash đang hiện sẽ bị nó che
        /// mất. Đây là đúng loại lỗi layering đã dính ở test phía trên, chỉ khác là
        /// ở ranh giới Canvas thay vì ranh giới sibling — hai Canvas cùng
        /// <c>sortingOrder</c> mặc định (đều 0) thì thứ tự vẽ KHÔNG được đảm bảo.
        /// </summary>
        [UnityTest]
        public IEnumerator CanvasChung_ThangCanvasCuaPixelCanvas()
        {
            _host = new GameObject("Bootstrap");
            var bootstrap = _host.AddComponent<GopetBootstrap>();

            yield return null;

            var login = Object.FindAnyObjectByType<LoginScreens>();
            var generalCanvas = login.GetComponentInParent<Canvas>();
            var pixelCanvas = bootstrap.PixelCanvasInstance.GetComponent<Canvas>();

            Assert.AreNotSame(generalCanvas, pixelCanvas, "Hai canvas phải TÁCH RIÊNG để so sortingOrder có ý nghĩa.");
            Assert.Greater(generalCanvas.sortingOrder, pixelCanvas.sortingOrder,
                "Canvas chung phải vẽ ĐÈ lên PixelCanvas — nếu không, hộp OTP sẽ nằm dưới màn splash.");
        }

        [UnityTest]
        public IEnumerator NutAmThanh_AnKhiSplash_HienLaiSauKhiDong()
        {
            _host = new GameObject("Bootstrap");
            _host.AddComponent<GopetBootstrap>();

            yield return null;

            var soundToggle = Object.FindAnyObjectByType<SoundToggleButton>(FindObjectsInactive.Include);
            var splash = Object.FindAnyObjectByType<JarSplashScreen>();
            Assert.IsNotNull(soundToggle);
            Assert.IsNotNull(splash);
            Assert.IsFalse(soundToggle.gameObject.activeSelf, "Nút âm thanh đang đè lên tranh splash.");

            splash.OnPointerClick(new PointerEventData(EventSystem.current));

            Assert.IsTrue(soundToggle.gameObject.activeSelf, "Nút âm thanh không hiện lại sau splash.");
        }

        /// <summary>Không có EventSystem thì không cú chạm nào tới được uGUI.</summary>
        [UnityTest]
        public IEnumerator Bootstrap_DungLuonEventSystem()
        {
            _host = new GameObject("Bootstrap");
            _host.AddComponent<GopetBootstrap>();

            yield return null;

            Assert.IsNotNull(EventSystem.current, "Không có EventSystem: UI hiện ra nhưng bấm không ăn.");
        }
    }
}
