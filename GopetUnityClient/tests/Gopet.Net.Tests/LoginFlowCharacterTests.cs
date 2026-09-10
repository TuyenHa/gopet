using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Nhánh tạo nhân vật. Chỗ dễ sai nhất của luồng đăng nhập: server ĐÓNG kết nối
    /// ngay sau khi tạo nhân vật (<c>GameController.cs:740</c>), nên cú đóng ấy là
    /// bước bình thường chứ không phải lỗi mạng.
    /// </summary>
    public sealed class LoginFlowCharacterTests
    {
        private readonly LoginFlowHarness _h = new LoginFlowHarness();

        [Fact]
        public void ChuaCoNhanVat_ChuyenSangManTao()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();

            Assert.Equal(LoginStage.CreatingCharacter, _h.Stage);
        }

        /// <summary>
        /// Cú đóng SAU khi tạo nhân vật phải thành "đang nối lại", không phải
        /// "mất kết nối". Đây là ca duy nhất phân biệt được hai cách xử lý.
        /// </summary>
        [Fact]
        public void TaoNhanVatXong_ServerDong_ThiNoiLaiChuKhongBaoLoi()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();

            Assert.True(_h.Flow.SubmitCharacter("gopet01", 0));
            Assert.Contains(GopetCmd.CREATE_CHAR, _h.Sent);

            var before = _h.Connects.Count;
            _h.Flow.OnDisconnected("server closed");

            Assert.Equal(LoginStage.Connecting, _h.Stage);
            Assert.Equal(before + 1, _h.Connects.Count);
        }

        /// <summary>Nối lại xong thì tự đăng nhập bằng tài khoản đã nhập, không hỏi lại.</summary>
        [Fact]
        public void NoiLaiSauKhiTaoNhanVat_TuDangNhapLai()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();
            _h.Flow.SubmitCharacter("gopet01", 0);
            _h.Flow.OnDisconnected("server closed");

            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);

            Assert.Equal(LoginStage.LoggingIn, _h.Stage);
            Assert.Equal(2, _h.CountSent(GopetCmd.LOGIN));
        }

        [Theory]
        [InlineData("abcd")]                    // 4 ký tự, cần >= 5
        [InlineData("abcdefghijklmnopqrstu")]   // 21 ký tự, tối đa 20
        [InlineData("GoPet01")]                 // chữ hoa
        [InlineData("go pet")]                  // khoảng trắng
        public void TenNhanVatSai_KhongGuiGoiNao(string name)
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();

            Assert.False(_h.Flow.SubmitCharacter(name, 0));
            Assert.DoesNotContain(GopetCmd.CREATE_CHAR, _h.Sent);
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }

        /// <summary>Mất kết nối KHÔNG kèm thông điệp vẫn phải có câu để hiện.</summary>
        [Fact]
        public void MatKetNoiKhongLyDo_VanCoCauDeHien()
        {
            _h.LogIn();
            _h.Flow.OnDisconnected(null);

            Assert.Equal(LoginStage.Disconnected, _h.Stage);
            Assert.False(string.IsNullOrEmpty(_h.Notice));
        }

        [Fact]
        public void ThuLai_NoiLaiToiMayChuDaChon()
        {
            _h.LogIn();
            _h.Flow.OnDisconnected(null);

            var before = _h.Connects.Count;
            _h.Flow.Retry();

            Assert.Equal(LoginStage.Connecting, _h.Stage);
            Assert.Equal(before + 1, _h.Connects.Count);
        }

        /// <summary>
        /// Tên trùng là thứ CHỈ server biết (<c>GameController.cs:726-732</c>): nó gửi
        /// dialog đỏ rồi đóng kết nối. Câu đó phải hiện NGAY TRÊN màn đang gõ tên —
        /// đẩy sang màn khác rồi xoá đi thì người chơi gõ lại đúng cái tên cũ mãi.
        /// </summary>
        [Fact]
        public void TenNhanVatTrung_HienLyDoNgayTrenManTaoNhanVat()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();
            _h.Flow.SubmitCharacter("gopet01", 0);

            _h.Flow.OnLoginRejected("Tên nhân vật đã có người dùng");

            Assert.Equal(LoginStage.CreatingCharacter, _h.Stage);
            Assert.Equal("Tên nhân vật đã có người dùng", _h.Notice);
        }

        /// <summary>Và câu đó phải sống sót qua cả cú đóng lẫn lần đăng nhập lại.</summary>
        [Fact]
        public void TenNhanVatTrung_LyDoSongSotQuaLanDangNhapLai()
        {
            _h.LogIn();
            _h.Flow.OnCharacterRequired();
            _h.Flow.SubmitCharacter("gopet01", 0);
            _h.Flow.OnLoginRejected("Tên nhân vật đã có người dùng");
            _h.Flow.OnDisconnected(null);

            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);
            _h.Flow.SubmitCredentials("gopettest", "abc12345");
            _h.Flow.OnCharacterRequired();

            Assert.Equal(LoginStage.CreatingCharacter, _h.Stage);
            Assert.Equal("Tên nhân vật đã có người dùng", _h.Notice);
        }

        /// <summary>Rớt mạng bình thường: bấm Thử lại thì đăng nhập luôn, không bắt gõ lại.</summary>
        [Fact]
        public void RotMang_ThuLai_ThiTuDangNhapLai()
        {
            _h.LogIn();
            _h.Flow.OnDisconnected(null);

            _h.Flow.Retry();
            _h.Flow.OnConnected();
            _h.Flow.OnClientAccepted(true);

            Assert.Equal(LoginStage.LoggingIn, _h.Stage);
        }
    }
}
