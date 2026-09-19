using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Tên NPC trong DB là HOA TOÀN BỘ không dấu ('BANH SINH NHAT') vì bitmap font
    /// jar thiếu chữ hoa có dấu. Client dùng font TTF rồi nên hạ về chữ hoa đầu từ.</summary>
    public sealed class NpcDisplayNamesTests
    {
        [Theory]
        [InlineData("BANH SINH NHAT", "Banh Sinh Nhat")]
        [InlineData("GIAN THUONG", "Gian Thuong")]
        [InlineData("WINDY", "Windy")]
        [InlineData("BAC SI THIEN THAN", "Bac Si Thien Than")]
        public void TenHoaToanBoThiHaXuongChuHoaDauTu(string raw, string expected)
        {
            Assert.Equal(expected, NpcDisplayNames.Prettify(raw));
        }

        /// <summary>Tên do người đặt (người chơi, pet) phải giữ NGUYÊN — đụng vào là làm hỏng.</summary>
        [Theory]
        [InlineData("kzhd9x")]
        [InlineData("Rua Test")]
        [InlineData("Bánh Sinh Nhật")]
        [InlineData("iPhone")]
        public void TenDaCoChuThuongThiGiuNguyen(string raw)
        {
            Assert.Equal(raw, NpcDisplayNames.Prettify(raw));
        }

        [Fact]
        public void DauCachVaDauChamDeuMoTuMoi()
        {
            // Chữ và số nối liền là một từ, nên "LV.3" giữ được chữ hoa sau dấu chấm.
            Assert.Equal("Ke Thu Thap Lv.3", NpcDisplayNames.Prettify("KE THU THAP LV.3"));
            Assert.Equal("Npc-Test", NpcDisplayNames.Prettify("NPC-TEST"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RongHoacNullThiTraVeNguyenTrang(string raw)
        {
            Assert.Equal(raw, NpcDisplayNames.Prettify(raw));
        }
    }
}
