using Gopet.Net.Auth;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Nhánh <c>REGISTER</c> (35) của <see cref="LoginFlow"/> — tính năng MỚI, server
    /// vừa mở khoá đăng ký (trước đó luôn trả "Chức năng này bị khóa" bất kể client
    /// gửi gì). Xem <c>LoginFlowTests.cs</c> cho luồng đăng nhập/tạo nhân vật.
    /// </summary>
    public sealed class LoginFlowRegisterTests
    {
        [Fact]
        public void HopLe_GuiGoiRegister_KhongDoiStage()
        {
            var harness = new LoginFlowHarness();
            harness.ReachCredentials();

            var ok = harness.Flow.SubmitRegistration("newuser123", "pass1234");

            Assert.True(ok);
            Assert.Equal(LoginStage.EnteringCredentials, harness.Stage);
            Assert.Equal(1, harness.CountSent(GopetCmd.REGISTER));
        }

        [Fact]
        public void TenSai_KhongGuiGoi()
        {
            var harness = new LoginFlowHarness();
            harness.ReachCredentials();

            var ok = harness.Flow.SubmitRegistration("Hoa-Hoe!", "pass1234");

            Assert.False(ok);
            Assert.NotNull(harness.Notice);
            Assert.Equal(0, harness.CountSent(GopetCmd.REGISTER));
        }

        [Fact]
        public void MatKhauNgan_KhongGuiGoi()
        {
            var harness = new LoginFlowHarness();
            harness.ReachCredentials();

            var ok = harness.Flow.SubmitRegistration("newuser123", "abc");

            Assert.False(ok);
            Assert.Equal(0, harness.CountSent(GopetCmd.REGISTER));
        }

        [Fact]
        public void DangChoHoiAm_BamLai_KhongGuiTrung()
        {
            var harness = new LoginFlowHarness();
            harness.ReachCredentials();
            harness.Flow.SubmitRegistration("newuser123", "pass1234");

            var secondOk = harness.Flow.SubmitRegistration("khactaikhoan", "khacmatkhau");

            Assert.False(secondOk);
            Assert.Equal(1, harness.CountSent(GopetCmd.REGISTER));
        }

        [Fact]
        public void HoiAm_BanRegisterReplyReceived_KhongDongKetNoiLaiVaKhongDoiStage()
        {
            var harness = new LoginFlowHarness();
            harness.ReachCredentials();
            harness.Flow.SubmitRegistration("newuser123", "pass1234");

            string received = null;
            harness.Flow.RegisterReplyReceived += text => received = text;
            var connectsBefore = harness.Connects.Count;

            harness.Flow.OnDialog("Đăng ký tài khoản thành công.");

            Assert.Equal("Đăng ký tài khoản thành công.", received);
            Assert.Equal(LoginStage.EnteringCredentials, harness.Stage);
            Assert.Equal(connectsBefore, harness.Connects.Count); // KHÔNG Reconnect() như OnLoginRejected vẫn làm
        }

        /// <summary>Không có REGISTER nào đang chờ thì <c>OnDialog</c> phải đi đúng đường cũ (OnLoginRejected) — không được đổi hành vi.</summary>
        [Fact]
        public void KhongCoRegisterDangCho_OnDialogVanXuLyNhuLoiTuChoiDangNhap()
        {
            var harness = new LoginFlowHarness();
            harness.LogIn(); // Stage == LoggingIn, IsConnected == true
            var connectsBefore = harness.Connects.Count;

            harness.Flow.OnDialog("sai mật khẩu");

            Assert.Equal(LoginStage.EnteringCredentials, harness.Stage);
            Assert.Equal("sai mật khẩu", harness.Notice);
            Assert.Equal(connectsBefore + 1, harness.Connects.Count); // OnLoginRejected nối lại NGẦM
        }

        [Fact]
        public void MatKetNoiDangChoHoiAm_KhongKetTreoVinhVien()
        {
            var harness = new LoginFlowHarness();
            harness.ReachCredentials();
            harness.Flow.SubmitRegistration("newuser123", "pass1234");

            harness.Flow.OnDisconnected("mất mạng");
            harness.ReachCredentials();

            var ok = harness.Flow.SubmitRegistration("newuser123", "pass1234");

            Assert.True(ok);
        }
    }
}
