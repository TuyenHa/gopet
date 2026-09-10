using Gopet.Net.Auth;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Server từ chối đăng nhập theo HAI kiểu khác nhau, và client phải sống được với
    /// cả hai:
    ///
    /// <list type="bullet">
    /// <item>Sai mật khẩu → <c>LOGIN_FAILED</c> rồi <b>đóng kết nối</b> (<c>Player.cs:655-658</c>)</item>
    /// <item>Sai OTP → chỉ dialog đỏ, <b>giữ kết nối</b> (<c>MenuController.inputDialog.cs:731</c>)</item>
    /// </list>
    /// </summary>
    public sealed class LoginFlowRejectionTests
    {
        private const string WrongPassword = "Sai tài khoản hoặc mật khẩu";

        private readonly LoginFlowHarness _h = new LoginFlowHarness();

        /// <summary>
        /// Cú đóng theo sau lời từ chối KHÔNG được đè màn "mất kết nối" lên: người chơi
        /// sẽ chỉ thấy lỗi mạng và không biết mình gõ sai mật khẩu.
        /// </summary>
        [Fact]
        public void SaiMatKhau_RoiServerDong_VanGiuCauCuaServer()
        {
            _h.LogIn();
            _h.Flow.OnLoginRejected(WrongPassword);
            _h.Flow.OnDisconnected(null);

            Assert.NotEqual(LoginStage.Disconnected, _h.Stage);

            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);

            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);
            Assert.Equal(WrongPassword, _h.Notice);
        }

        /// <summary>
        /// Và tuyệt đối KHÔNG tự gửi lại: server đếm số lần thử và khoá ở lần thứ 10
        /// trong 5 phút (<c>Player.cs:630</c>). Tự thử lại là tự khoá mình sau vài giây.
        /// </summary>
        [Fact]
        public void SauKhiBiTuChoi_KhongTuGuiLaiLogin()
        {
            _h.LogIn();
            var before = _h.CountSent(GopetCmd.LOGIN);

            _h.Flow.OnLoginRejected(WrongPassword);
            _h.Flow.OnDisconnected(null);
            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);

            Assert.Equal(before, _h.CountSent(GopetCmd.LOGIN));
        }

        /// <summary>Gõ lại rồi bấm Đăng nhập thì câu từ chối cũ phải biến mất.</summary>
        [Fact]
        public void GuiLaiTaiKhoan_XoaCauTuChoiCu()
        {
            _h.LogIn();
            _h.Flow.OnLoginRejected(WrongPassword);

            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);
            Assert.True(_h.Flow.SubmitCredentials("gopettest", "matkhaumoi"));

            Assert.Null(_h.Notice);
        }

        /// <summary>
        /// Sai OTP thì server GIỮ kết nối — nhưng lúc đó nó đã có <c>user</c>, nên gói
        /// <c>LOGIN</c> thứ hai bị <c>Player.cs:587</c> bỏ qua <b>không một lời hồi
        /// âm</b>. Gửi lại trên chính kết nối ấy là treo màn "đang đăng nhập" vĩnh
        /// viễn, mà màn đó không có nút nào. Phải nối lại.
        /// </summary>
        [Fact]
        public void SaiOtp_PhaiNoiLaiChuKhongGuiLaiTrenKetNoiCu()
        {
            _h.LogIn();
            var connectsBefore = _h.Connects.Count;

            _h.Flow.OnLoginRejected("Mã OTP không đúng");

            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);
            Assert.Equal("Mã OTP không đúng", _h.Notice);
            Assert.Equal(connectsBefore + 1, _h.Connects.Count);
        }

        /// <summary>Và trong lúc chưa nối lại xong thì bấm Đăng nhập KHÔNG được gửi gói vào hư không.</summary>
        [Fact]
        public void DangNoiLai_BamDangNhap_ThiKhongGuiGoiNao()
        {
            _h.LogIn();
            _h.Flow.OnLoginRejected("Mã OTP không đúng");

            var before = _h.CountSent(GopetCmd.LOGIN);

            Assert.False(_h.Flow.SubmitCredentials("gopettest", "abc12345"));
            Assert.Equal(before, _h.CountSent(GopetCmd.LOGIN));
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }

        /// <summary>Nối lại xong thì gửi lại được, và lần này server sẽ trả lời.</summary>
        [Fact]
        public void NoiLaiXong_ThiGuiLaiDuoc()
        {
            _h.LogIn();
            _h.Flow.OnLoginRejected("Mã OTP không đúng");

            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);

            Assert.True(_h.Flow.SubmitCredentials("gopettest", "abc12345"));
            Assert.Equal(LoginStage.LoggingIn, _h.Stage);
        }

        /// <summary>Nối lại ngầm KHÔNG được đá người chơi sang màn "đang kết nối" — câu server sẽ mất.</summary>
        [Fact]
        public void NoiLaiNgam_GiuNguyenManHinhVaCau()
        {
            _h.LogIn();
            _h.Flow.OnLoginRejected(WrongPassword);
            _h.Flow.OnDisconnected(null);
            _h.Flow.OnConnected();

            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);
            Assert.Equal(WrongPassword, _h.Notice);
        }

        /// <summary>
        /// Dialog của server giữa lúc chơi KHÔNG được kéo người chơi về màn đăng nhập.
        /// <c>DialogShown</c> nối thẳng vào <c>OnLoginRejected</c> nên chặn phải nằm ở đây.
        /// </summary>
        [Fact]
        public void DialogSauKhiVaoGame_KhongDayVeManDangNhap()
        {
            _h.LogIn();
            _h.Flow.OnLoginSucceeded(new LoginSuccess { UserId = 1, Name = "a" });

            _h.Flow.OnLoginRejected("Bạn vừa nhận được vật phẩm");

            Assert.Equal(LoginStage.Ready, _h.Stage);
        }

        /// <summary>Đăng nhập được rồi thì lần rớt mạng sau đó phải hiện đúng là rớt mạng.</summary>
        [Fact]
        public void DangNhapDuocRoi_RotMangSauDo_VanBaoMatKetNoi()
        {
            _h.LogIn();
            _h.Flow.OnLoginSucceeded(new LoginSuccess { UserId = 1, Name = "a" });

            _h.Flow.OnDisconnected(null);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
        }
    }
}
