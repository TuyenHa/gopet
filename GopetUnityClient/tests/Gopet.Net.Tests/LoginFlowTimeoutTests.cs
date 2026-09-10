using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Hạn chờ hồi âm. Server nhận gói rồi im lặng là chuyện có thật
    /// (<c>Player.cs:587</c> return khi đã có <c>user</c>), và chặng
    /// <see cref="LoginStage.LoggingIn"/> là màn hình KHÔNG CÓ NÚT NÀO — kẹt ở đó là
    /// kẹt vĩnh viễn.
    /// </summary>
    public sealed class LoginFlowTimeoutTests
    {
        private readonly LoginFlowHarness _h = new LoginFlowHarness();

        private void Advance(long ms)
        {
            _h.NowMs += ms;
            _h.Flow.Tick();
        }

        [Fact]
        public void DangDangNhapMaServerImLang_ThiThoatRaDuoc()
        {
            _h.LogIn();
            Assert.Equal(LoginStage.LoggingIn, _h.Stage);

            Advance(LoginFlow.ReplyTimeoutMs + 1);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }

        /// <summary>Chưa tới hạn thì tuyệt đối không được tự bỏ cuộc.</summary>
        [Fact]
        public void ChuaToiHan_VanChoTiep()
        {
            _h.LogIn();

            Advance(LoginFlow.ReplyTimeoutMs - 1);

            Assert.Equal(LoginStage.LoggingIn, _h.Stage);
        }

        [Fact]
        public void BatTayMaServerImLang_CungThoatRaDuoc()
        {
            _h.Flow.Start("127.0.0.1", 19180);
            _h.Flow.OnConnected();
            Assert.Equal(LoginStage.Handshaking, _h.Stage);

            Advance(LoginFlow.ReplyTimeoutMs + 1);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
        }

        /// <summary>Server trả lời rồi thì đồng hồ phải tắt, không được nổ muộn.</summary>
        [Fact]
        public void ServerDaTraLoi_ThiDongHoTat()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();

            _h.NowMs += LoginFlow.ReplyTimeoutMs * 10;
            _h.Flow.Tick();

            Assert.Equal(LoginStage.CreatingCharacter, _h.Stage);
        }

        /// <summary>Đứng ở màn nhập tài khoản thì chờ bao lâu cũng không sao — chờ NGƯỜI, không chờ server.</summary>
        [Fact]
        public void DungOManNhapTaiKhoan_KhongCoHanChoNao()
        {
            _h.ReachCredentials();

            _h.NowMs += LoginFlow.ReplyTimeoutMs * 10;
            _h.Flow.Tick();

            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);
        }

        /// <summary>Gửi CREATE_CHAR rồi mà server không đóng kết nối cũng là hỏng.</summary>
        [Fact]
        public void TaoNhanVatMaServerImLang_ThiThoatRaDuoc()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();
            _h.Flow.SubmitCharacter("gopet01", 0);

            Advance(LoginFlow.ReplyTimeoutMs + 1);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
        }
    }
}
