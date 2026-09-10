using Gopet.Net;
using Gopet.Net.Auth;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Mỗi chặng của <see cref="LoginFlow"/> phải ra đúng một màn hình, và bấm trên
    /// màn hình đó phải đẩy luồng đi tiếp.
    ///
    /// <para>Không có màn 2FA ở đây là CỐ Ý: server hỏi OTP bằng
    /// <c>TYPE_DIALOG_INPUT</c> thường nên nó đi qua <c>UiRoot</c>, cùng đường với
    /// mọi hộp nhập khác. <c>UiRootDialogTests</c> đã phủ đường đó.</para>
    /// </summary>
    public sealed class LoginScreensTests
    {
        private GameObject _canvas;
        private LoginFlow _flow;
        private LoginScreens _screens;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            _flow = new LoginFlow();

            _screens = LoginScreens.Create(_canvas.transform, null);
            _screens.Initialize(_flow);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
        }

        private void ReachCredentials()
        {
            _flow.Start("127.0.0.1", 19180);
            _flow.OnConnected();
            _flow.OnClientAccepted(true);
            _flow.OnServerList(new[] { new ServerEntry { Name = "Máy chủ 1", Address = "127.0.0.1", Port = 19180 } });

            // Đúng một máy chủ thì OnServerList tự chọn luôn (xem LoginFlow.ServerList.cs).
            if (_flow.Stage == LoginStage.ChoosingServer) _flow.ChooseServer(0);
        }

        [Test]
        public void DangNoi_KhongDungManHinhTrungGian()
        {
            _flow.Start("127.0.0.1", 19180);

            Assert.IsNull(_screens.Form);
            Assert.IsNull(_screens.ServerPicker);
        }

        [Test]
        public void ManDangNhap_CoHaiONhapVaOMatKhauBiChe()
        {
            ReachCredentials();

            Assert.AreEqual(2, _screens.Form.Fields.Count);
            Assert.AreEqual(UnityEngine.UI.InputField.ContentType.Password, _screens.Form.Fields[1].contentType);
        }

        [Test]
        public void BamDangNhap_DayLuongSangDangGui()
        {
            ReachCredentials();

            _screens.Form.Fields[0].text = "gopettest";
            _screens.Form.Fields[1].text = "abc12345";
            _screens.Form.Press(0);

            Assert.AreEqual(LoginStage.LoggingIn, _flow.Stage);
        }

        /// <summary>Tên sai thì ở lại màn đăng nhập VÀ hiện lý do, không im lặng.</summary>
        [Test]
        public void TenTaiKhoanSai_HienLyDoNgayTrenManHinh()
        {
            ReachCredentials();

            _screens.Form.Fields[0].text = "Sai Ten";
            _screens.Form.Press(0);

            Assert.AreEqual(LoginStage.EnteringCredentials, _flow.Stage);
            Assert.IsNotEmpty(_screens.Form.NoticeText);
        }

        /// <summary>Mất kết nối không kèm thông điệp vẫn phải có chữ và có nút thoát ra.</summary>
        [Test]
        public void MatKetNoiKhongLyDo_VanCoChuVaNutThuLai()
        {
            ReachCredentials();
            _flow.OnDisconnected(null);

            Assert.AreEqual(1, _screens.Form.Buttons.Count);
            Assert.IsNotEmpty(_screens.Form.NoticeText);

            _screens.Form.Press(0);
            Assert.AreEqual(LoginStage.Connecting, _flow.Stage);
        }

        /// <summary>
        /// Nút nào thì giới tính đó. Kiểm BYTE THẬT trong gói: mọi assert về chặng đều
        /// xanh với cả hai nút, nên chúng không phân biệt được gì cả.
        /// </summary>
        [TestCase(0)]
        [TestCase(1)]
        public void ManTaoNhanVat_NutNaoThiGioiTinhDo(int button)
        {
            ReachCredentials();
            _flow.SubmitCredentials("gopettest", "abc12345");
            _flow.OnCharacterRequired();

            Assert.AreEqual(1, _screens.Form.Fields.Count);
            Assert.AreEqual(2, _screens.Form.Buttons.Count);

            sbyte? gender = null;
            _flow.SendRequested += m =>
            {
                if (m.Id != GopetCmd.CREATE_CHAR) return;

                // CREATE_CHAR = UTF tên + sbyte giới tính; giới tính là byte cuối.
                var wire = m.ToWire();
                gender = unchecked((sbyte)wire[wire.Length - 1]);
            };

            _screens.Form.Fields[0].text = "gopet01";
            _screens.Form.Press(button);

            Assert.AreEqual((sbyte)button, gender, "Nút bấm không khớp giới tính gửi lên.");
        }

        [Test]
        public void DangNhapXong_BanSuKienVaDonManHinh()
        {
            ReachCredentials();
            _flow.SubmitCredentials("gopettest", "abc12345");

            var finished = false;
            _screens.Finished += () => finished = true;

            _flow.OnLoginSucceeded(new LoginSuccess { UserId = 1, Name = "abc" });

            // Runtime gọi hàm này sau khi VerticalSplitRevealTransition đã chụp frame login.
            _screens.CompleteReadyPresentation();

            Assert.IsTrue(finished);
            Assert.IsNull(_screens.Form);
        }

        [Test]
        public void KhoiTaoHaiLan_Nem()
        {
            Assert.Throws<System.InvalidOperationException>(() => _screens.Initialize(_flow));
        }
    }
}
