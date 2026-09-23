using System;
using System.IO;
using Gopet.Net;
using Gopet.Net.Auth;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nối nút "Tạo tài khoản" + ô "Ghi nhớ tài khoản đăng nhập" của
    /// <see cref="LoginFormView"/> vào <see cref="LoginFlow"/> qua <see cref="LoginScreens"/>.
    /// Xem <c>LoginFlowRegisterTests.cs</c> (Gopet.Net.Tests) cho phần logic thuần,
    /// và <c>LoginFormViewTests.cs</c> cho phần chỉ UI.
    /// </summary>
    public sealed class LoginScreensRegisterTests
    {
        private GameObject _canvas;
        private LoginFlow _flow;
        private LoginScreens _screens;
        private CredentialStore _store;
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));

            _tempDir = Path.Combine(Path.GetTempPath(), "GopetTest_" + Guid.NewGuid());
            _store = new CredentialStore(_tempDir);

            _flow = new LoginFlow();
            _screens = LoginScreens.Create(_canvas.transform, null);
            _screens.Initialize(_flow, _store);

            _flow.Start("127.0.0.1", 19180);
            _flow.OnConnected();
            _flow.OnClientAccepted(true);
            _flow.OnServerList(new[] { new ServerEntry { Name = "s1", Address = "127.0.0.1", Port = 19180 } });
            if (_flow.Stage == LoginStage.ChoosingServer) _flow.ChooseServer(0);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) UnityEngine.Object.DestroyImmediate(_canvas);
            if (_tempDir != null && Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
        }

        [Test]
        public void BamTaoTaiKhoan_MoFormDangKy_ChuaGuiRegister()
        {
            var loginForm = _screens.LoginForm;
            loginForm.SetCredentials("newuser123", "pass1234");
            var registerPackets = 0;
            _flow.SendRequested += message =>
            {
                if (message.Id == GopetCmd.REGISTER) registerPackets++;
            };

            RaiseRegister(loginForm);

            Assert.AreEqual(LoginStage.EnteringCredentials, _flow.Stage);
            Assert.AreEqual(0, registerPackets, "Mở form không được gửi REGISTER ngay.");
            Assert.IsNull(_screens.LoginForm, "Màn đăng nhập phải được thay bằng form đăng ký.");
            Assert.IsNull(_screens.Form, "Form đăng ký phải dùng skin login, không dùng FormView chung.");
            Assert.IsNotNull(_screens.RegistrationForm);
            Assert.IsNotNull(_screens.RegistrationForm.GetComponentInChildren<LoginBackground>(true));
            Assert.AreEqual("newuser123", _screens.RegistrationForm.Username);
            Assert.AreEqual("pass1234", _screens.RegistrationForm.Password);
            var registerButton = FindButton(_screens.RegistrationForm, "Button_DangKy");
            var backButton = FindButton(_screens.RegistrationForm, "Button_TroLai");
            Assert.AreEqual(LoginSkin.Get(LoginSkin.ButtonRegister), registerButton.GetComponent<Image>().sprite);
            Assert.AreEqual(LoginSkin.Get(LoginSkin.ButtonLogin), backButton.GetComponent<Image>().sprite);
            Assert.AreEqual("Đăng ký", registerButton.GetComponentInChildren<Text>().text);
            Assert.AreEqual("Trở lại", backButton.GetComponentInChildren<Text>().text);
            Assert.IsNull(_screens.RegistrationForm.transform.Find("Panel/Content/RegistrationTitle"),
                "Form đăng ký không cần thêm dòng tiêu đề ở giữa.");
        }

        [Test]
        public void TrenFormDangKy_BamDangKy_MoiGuiRegister()
        {
            var registerPackets = 0;
            _flow.SendRequested += message =>
            {
                if (message.Id == GopetCmd.REGISTER) registerPackets++;
            };
            RaiseRegister(_screens.LoginForm);
            _screens.RegistrationForm.SetCredentials("newuser123", "pass1234");
            _screens.RegistrationForm.SetConfirmPassword("pass1234");

            FindButton(_screens.RegistrationForm, "Button_DangKy").onClick.Invoke();

            Assert.AreEqual(LoginStage.EnteringCredentials, _flow.Stage,
                "Gửi REGISTER không được đổi chặng đăng nhập.");
            Assert.AreEqual(1, registerPackets);
            Assert.IsNotNull(_screens.RegistrationForm, "Form đăng ký phải ở lại để hiện hồi âm của server.");
        }

        [Test]
        public void TrenFormDangKy_BamTroVe_KhoiPhucLoginVaDuLieuDangNhap()
        {
            var loginForm = _screens.LoginForm;
            loginForm.SetCredentials("newuser123", "pass1234");
            RaiseRegister(loginForm);

            FindButton(_screens.RegistrationForm, "Button_TroLai").onClick.Invoke();

            Assert.IsNull(_screens.Form);
            Assert.IsNull(_screens.RegistrationForm);
            Assert.IsNotNull(_screens.LoginForm);
            Assert.AreEqual("newuser123", _screens.LoginForm.Username);
            Assert.AreEqual("pass1234", _screens.LoginForm.Password);
        }

        [Test]
        public void HoiAmRegister_CapNhatNoticeTrenManDangHien()
        {
            RaiseRegister(_screens.LoginForm);
            var registerForm = _screens.RegistrationForm;
            registerForm.SetCredentials("newuser123", "pass1234");
            registerForm.SetConfirmPassword("pass1234");
            FindButton(registerForm, "Button_DangKy").onClick.Invoke();

            _flow.OnDialog("Đăng ký tài khoản thành công.");

            Assert.AreEqual("Đăng ký tài khoản thành công.", registerForm.NoticeText);
        }

        /// <summary>Bỏ chọn "ghi nhớ" rồi đăng nhập thành công thì KHÔNG được lưu, và phải xoá phần đã lưu trước đó.</summary>
        [Test]
        public void BoChonGhiNho_DangNhapThanhCong_KhongLuuVaXoaCaiCu()
        {
            _store.Save("cu", "matkhaucu");

            var loginForm = _screens.LoginForm;
            loginForm.SetCredentials("newuser123", "pass1234");
            SetRemember(loginForm, false);
            loginForm.SubmitDefault();

            _flow.OnLoginSucceeded(new LoginSuccess { UserId = 1, Name = "newuser123" });

            var saved = _store.Load();
            Assert.AreEqual(string.Empty, saved.Username, "Bỏ ghi nhớ mà tài khoản cũ vẫn còn — chưa xoá.");
        }

        [Test]
        public void GiuGhiNho_DangNhapThanhCong_LuuTaiKhoanVuaGui()
        {
            var loginForm = _screens.LoginForm;
            loginForm.SetCredentials("newuser123", "pass1234");
            SetRemember(loginForm, true);
            loginForm.SubmitDefault();

            _flow.OnLoginSucceeded(new LoginSuccess { UserId = 1, Name = "newuser123" });

            var saved = _store.Load();
            Assert.AreEqual("newuser123", saved.Username);
        }

        /// <summary>
        /// Không có API bắn thẳng sự kiện từ ngoài — bấm đúng nút thật, giống người dùng.
        /// Tìm theo TÊN chứ không theo chữ hiện trên nút: chữ nằm sẵn trong sprite của
        /// bộ art, nút không có <see cref="Text"/> nào để đọc.
        /// </summary>
        private static void RaiseRegister(LoginFormView view)
        {
            foreach (var button in view.GetComponentsInChildren<Button>(true))
            {
                if (button.name == "Button_TaoTaiKhoan")
                {
                    button.onClick.Invoke();
                    return;
                }
            }

            Assert.Fail("Không tìm thấy nút 'Tạo tài khoản'.");
        }

        private static void SetRemember(LoginFormView view, bool value)
        {
            view.GetComponentInChildren<Toggle>().isOn = value;
        }

        private static Button FindButton(LoginFormView view, string name)
        {
            foreach (var button in view.GetComponentsInChildren<Button>(true))
            {
                if (button.name == name) return button;
            }

            Assert.Fail($"Không tìm thấy nút '{name}'.");
            return null;
        }
    }
}
