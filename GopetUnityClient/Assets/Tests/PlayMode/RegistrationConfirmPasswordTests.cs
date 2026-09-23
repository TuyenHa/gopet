using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Form đăng ký có ô nhập lại mật khẩu: không khớp thì báo lỗi ngay trên form, KHÔNG
    /// phát yêu cầu đăng ký (tránh tạo tài khoản với mật khẩu gõ nhầm). Form đăng nhập
    /// không có ô này.
    /// </summary>
    public sealed class RegistrationConfirmPasswordTests
    {
        private GameObject _root;
        private GameObject _eventSystem;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root", typeof(RectTransform), typeof(Canvas));
            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_eventSystem);
        }

        [Test]
        public void FormDangNhap_KhongCoONhapLai()
        {
            var view = LoginFormView.Create(_root.transform, null);
            Assert.IsNull(view.transform.Find("Panel/Content/Field_NhapLaiMatKhau"));
        }

        [Test]
        public void NhapLaiSai_BaoLoi_KhongGuiDangKy()
        {
            var view = LoginFormView.CreateRegistration(_root.transform, null);
            var sent = 0;
            view.SubmitRegistrationRequested += (_, __) => sent++;
            Assert.IsNotNull(view.transform.Find("Panel/Content/Field_NhapLaiMatKhau"), "Thiếu ô nhập lại mật khẩu.");

            view.SetCredentials("newuser123", "pass1234");
            view.SetConfirmPassword("pass12345");
            Press(view, "Button_DangKy");

            Assert.AreEqual(0, sent, "Mật khẩu nhập lại sai mà vẫn gửi đăng ký.");
            Assert.AreEqual(LoginFormView.PasswordMismatchNotice, view.NoticeText);
        }

        [Test]
        public void NhapLaiDung_GuiDangKy()
        {
            var view = LoginFormView.CreateRegistration(_root.transform, null);
            string user = null, pass = null;
            view.SubmitRegistrationRequested += (u, p) => { user = u; pass = p; };

            view.SetCredentials("newuser123", "pass1234");
            view.SetConfirmPassword("pass1234");
            Press(view, "Button_DangKy");

            Assert.AreEqual("newuser123", user);
            Assert.AreEqual("pass1234", pass);
        }

        [Test]
        public void KhungDangKy_CaoHon_CungBeNgang_LogoKhongBiCat()
        {
            var login = (RectTransform)LoginFormView.Create(_root.transform, null).transform.Find("Panel");
            var register = (RectTransform)LoginFormView.CreateRegistration(_root.transform, null).transform.Find("Panel");
            float Height(RectTransform r) => r.anchorMax.y - r.anchorMin.y;
            float Width(RectTransform r) => Height(r) * r.GetComponent<AspectRatioFitter>().aspectRatio;

            Assert.Greater(Height(register), Height(login), "Khung đăng ký phải cao hơn khung đăng nhập.");
            Assert.AreEqual(Width(login), Width(register), 0.001f, "Bề ngang hai khung phải bằng nhau.");
            // Logo treo từ đỉnh panel lên thêm 30% chiều cao panel (MakeLogo).
            Assert.LessOrEqual(register.anchorMax.y + 0.30f * Height(register), 1f, "Logo nhô khỏi mép trên màn hình.");
            Assert.GreaterOrEqual(register.anchorMin.y, 0f, "Khung đăng ký lọt khỏi mép dưới màn hình.");
        }

        private static void Press(LoginFormView view, string name) =>
            view.transform.Find("Panel/Content/" + name).GetComponent<Button>().onClick.Invoke();
    }
}
