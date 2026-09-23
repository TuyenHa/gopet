using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Bộ art Thành phố Linh Thú: panel viền vàng 9-slice, nút mặt kính TRƠN có nhãn vẽ
    /// bằng code, splash đặt logo jar goPet lên tranh không chữ.
    /// </summary>
    public sealed class LoginStyleTests
    {
        private GameObject _root;
        private GameObject _eventSystem;
        private SoundManager _sound;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            _sound = SoundManager.Create(_root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_eventSystem);
            PlayerPrefs.DeleteKey("Gopet.SoundEnabled");
        }

        [Test]
        public void Panel_KhungVienVang9Slice()
        {
            var view = LoginFormView.Create(_root.transform, null, _sound);
            var panel = view.transform.Find("Panel").GetComponent<Image>();

            Assert.IsNotNull(panel.sprite, "Thiếu Resources/Ui/Login/panel.png.");
            Assert.AreEqual(Image.Type.Sliced, panel.type);
            Assert.Greater(panel.sprite.border.x, 0f, "Góc trang trí sẽ bị kéo méo nếu không có viền 9-slice.");
        }

        [Test]
        public void NutDangNhap_CoNhanVeBangCode()
        {
            var view = LoginFormView.Create(_root.transform, null, _sound);

            Assert.AreEqual("Đăng nhập", LabelOf(view, "Button_DangNhap"));
            Assert.AreEqual("Tạo tài khoản", LabelOf(view, "Button_TaoTaiKhoan"));
        }

        [Test]
        public void GhiNho_BoChon_VanGiuKhung_ChiAnDauTich()
        {
            var view = LoginFormView.Create(_root.transform, null, _sound);
            var toggle = view.GetComponentInChildren<Toggle>(true);
            var box = toggle.transform.Find("Box").GetComponent<Image>();

            toggle.isOn = false;

            Assert.AreNotSame(box, toggle.graphic, "Toggle ẩn chính khung ô khi bỏ chọn.");
            Assert.IsTrue(box.enabled && box.gameObject.activeInHierarchy, "Khung ô biến mất khi bỏ chọn.");
            Assert.AreEqual("Check", toggle.graphic.name);
        }

        [Test]
        public void NhanNut_KhongDungOutline_TranhRangCua()
        {
            var view = LoginFormView.Create(_root.transform, null, _sound);
            foreach (var button in view.GetComponentsInChildren<Button>(true))
                foreach (var outline in button.GetComponentsInChildren<Outline>(true))
                    Assert.Fail($"{button.name}: Outline vẽ chữ 4 lần lệch nhau → răng cưa.");
        }

        [Test]
        public void Splash_CoLogoJarTrenTranh()
        {
            var canvas = PixelCanvas.Create(_root.transform);
            canvas.ApplyLayout(PixelCanvasLayout.Compute(1920f, 1080f));
            var splash = JarSplashScreen.Create(canvas, _sound);

            var logo = splash.transform.Find("Logo").GetComponent<Image>();
            Assert.AreSame(LoginSkin.Get(LoginSkin.Logo), logo.sprite);
            Assert.Greater(logo.transform.GetSiblingIndex(), splash.transform.Find("Picture").GetSiblingIndex(),
                "Logo phải vẽ ĐÈ lên tranh nền.");
        }

        private static string LabelOf(LoginFormView view, string buttonName)
        {
            foreach (var button in view.GetComponentsInChildren<Button>(true))
                if (button.name == buttonName) return button.GetComponentInChildren<Text>().text;
            Assert.Fail($"Không tìm thấy nút {buttonName}.");
            return null;
        }
    }
}
