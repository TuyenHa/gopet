using Gopet.Net.Auth;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Thứ tự các bước đăng nhập. Đối chiếu lượt chạy thật của
    /// <c>LiveSmoke/LoginChecks</c>: nối → CLIENT_INFO → SERVER_LIST → chọn máy chủ
    /// → LOGIN → LOGIN_SUCCES.
    /// </summary>
    public sealed class LoginFlowTests
    {
        private readonly LoginFlowHarness _h = new LoginFlowHarness();

        [Fact]
        public void DuongThuan_TuNoiToiVaoGame()
        {
            _h.Flow.Start("127.0.0.1", 19180);
            Assert.Equal(LoginStage.Connecting, _h.Stage);
            Assert.Equal(new[] { "127.0.0.1:19180" }, _h.Connects);

            _h.Flow.OnConnected();
            Assert.Equal(LoginStage.Handshaking, _h.Stage);
            Assert.Equal(new[] { GopetCmd.CLIENT_INFO }, _h.Sent);

            _h.Flow.OnClientAccepted(true);
            Assert.Equal(GopetCmd.SERVER_LIST, _h.Sent[1]);

            // Đúng một máy chủ (trường hợp thật hiện nay) thì khỏi qua màn chọn.
            _h.Flow.OnServerList(LoginFlowHarness.OneServer());
            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);

            Assert.True(_h.Flow.SubmitCredentials("gopettest", "abc12345"));
            Assert.Equal(LoginStage.LoggingIn, _h.Stage);
            Assert.Equal(GopetCmd.LOGIN, _h.Sent[2]);

            _h.Flow.OnLoginSucceeded(new LoginSuccess { UserId = 7, Name = "abc" });
            Assert.Equal(LoginStage.Ready, _h.Stage);
        }

        /// <summary>Chọn máy chủ khác thì phải nối lại tới ĐÚNG địa chỉ nó cho.</summary>
        [Fact]
        public void ChonMayChuKhac_NoiLaiToiDiaChiDo()
        {
            _h.Flow.Start("127.0.0.1", 19180);
            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);
            // Đúng một máy chủ thì OnServerList tự chọn luôn — không cần gọi
            // ChooseServer tay, và gọi thêm ở đây sẽ đá luồng đi sai (xem ghi chú
            // trong LoginFlow.ServerList.cs).
            _h.Flow.OnServerList(LoginFlowHarness.OneServer("10.0.0.9", 20000));

            Assert.Equal(LoginStage.Connecting, _h.Stage);
            Assert.Equal("10.0.0.9:20000", _h.Connects[1]);
        }

        /// <summary>Bắt tay lần hai KHÔNG được hỏi lại danh sách máy chủ.</summary>
        [Fact]
        public void SauKhiDaChonMayChu_KhongXinLaiDanhSach()
        {
            _h.ReachCredentials("10.0.0.9");

            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);
            Assert.Equal(1, _h.CountSent(GopetCmd.SERVER_LIST));
        }

        [Fact]
        public void MayChuTuChoiClient_KhongTreoManHinh()
        {
            _h.Flow.Start("127.0.0.1", 19180);
            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(false);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }

        [Fact]
        public void DanhSachMayChuRong_KhongTreoManHinh()
        {
            _h.Flow.Start("127.0.0.1", 19180);
            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);
            _h.Flow.OnServerList(new ServerEntry[0]);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }

        /// <summary>Nhiều máy chủ (chưa dùng hiện nay, để sẵn cho tương lai) vẫn phải qua màn chọn — chỉ đúng một mới được tự động.</summary>
        [Fact]
        public void NhieuMayChu_VanQuaManChon()
        {
            _h.Flow.Start("127.0.0.1", 19180);
            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);

            _h.Flow.OnServerList(new[]
            {
                new ServerEntry { Name = "Máy chủ 1", Address = "127.0.0.1", Port = 19180 },
                new ServerEntry { Name = "Máy chủ 2", Address = "10.0.0.9", Port = 20000 }
            });

            Assert.Equal(LoginStage.ChoosingServer, _h.Stage);
        }

        [Fact]
        public void TenTaiKhoanSai_KhongGuiGoiNao()
        {
            _h.ReachCredentials();

            Assert.False(_h.Flow.SubmitCredentials("Gopet Test", "abc12345"));
            Assert.Equal(LoginStage.EnteringCredentials, _h.Stage);
            Assert.Equal(0, _h.CountSent(GopetCmd.LOGIN));
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }
    }
}
