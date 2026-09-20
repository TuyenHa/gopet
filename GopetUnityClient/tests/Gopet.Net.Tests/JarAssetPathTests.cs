using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class JarAssetPathTests
    {
        [Theory]
        // Dạng chuẩn giữ nguyên.
        [InlineData("npcs/giaNoel.png", "npcs/giaNoel.png")]
        // Backslash kiểu Windows trong dump DB.
        [InlineData(@"npcs\bac_hoc_dau_gau.png", "npcs/bac_hoc_dau_gau.png")]
        // Gạch đôi: đây là ca làm NPC mất sprite, chỉ còn nhãn tên.
        [InlineData(@"npcs\\giaNoel.png", "npcs/giaNoel.png")]
        [InlineData(@"npcs\\huong_dan choi.png", "npcs/huong_dan choi.png")]
        [InlineData("npcs//su_gia_thien_than.png", "npcs/su_gia_thien_than.png")]
        // Gạch mở đầu cũng hỏng tra cứu Resources.
        [InlineData("/npcs/arena.png", "npcs/arena.png")]
        public void Normalize_GomGachVaDoiBackslash(string input, string expected)
        {
            Assert.Equal(expected, JarAssetPath.Normalize(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Normalize_ChuoiRong_TraVeNguyenTrang(string input)
        {
            Assert.Equal(input, JarAssetPath.Normalize(input));
        }
    }
}
