using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Bản jar xài chung bộ tile mùa đông cho nhiều map. Test chốt từng map được đổi
    /// những dải nào — đổi nhầm là cả map lệch tông (cỏ xanh viền tuyết, hoặc ngược lại).
    /// </summary>
    public sealed class MapSkinOverridesTests
    {
        private const int SnowGrassA = 161, SnowGrassB = 162, SnowBorder = 3;
        private const int GrassA = 11161, GrassB = 11162, StoneBorder = 11003;

        [Fact]
        public void ThanhPhoLinhThu_DoiCaCoLanVien()
        {
            var map = MapSkinOverrides.BeastCityMapId;

            Assert.Equal(GrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(GrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
            Assert.Equal(StoneBorder, MapSkinOverrides.ResolveImageId(map, SnowBorder));
        }

        /// <summary>Đấu trường mặc cùng bộ áo với thành phố: cỏ xanh + viền đá.</summary>
        [Fact]
        public void DauTruong_DoiCaCoLanVien()
        {
            var map = MapSkinOverrides.ArenaMapId;

            Assert.Equal(StoneBorder, MapSkinOverrides.ResolveImageId(map, SnowBorder));
            Assert.Equal(GrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(GrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
        }

        /// <summary>
        /// Đường lên đỉnh núi: mặt tuyết thành thảm cỏ, vách tuyết thành vách đá lộ cỏ,
        /// cỏ-lẫn-tuyết thành cỏ, viền tuyết thành viền đá — cả map hết tuyết chứ không
        /// nửa nọ nửa kia.
        /// </summary>
        [Fact]
        public void DuongLenDinhNui_HetTuyet()
        {
            const int snowGround = 180, snowCliff = 179;
            const int greenGround = 11180, greenCliff = 11179;
            var map = MapSkinOverrides.MountainPathMapId;

            Assert.Equal(greenGround, MapSkinOverrides.ResolveImageId(map, snowGround));
            Assert.Equal(greenCliff, MapSkinOverrides.ResolveImageId(map, snowCliff));
            Assert.Equal(GrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(GrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
            Assert.Equal(StoneBorder, MapSkinOverrides.ResolveImageId(map, SnowBorder));
        }

        /// <summary>Dải 180/179 chỉ đổi ở map núi — map khác không có dải này.</summary>
        [Fact]
        public void MatNui_ChiApDungChoMapNui()
        {
            const int snowGround = 180;

            Assert.Equal(snowGround,
                MapSkinOverrides.ResolveImageId(MapSkinOverrides.BeastCityMapId, snowGround));
        }

        /// <summary>Map không khai báo thì giữ nguyên mọi dải — không tự ý đổi tông.</summary>
        [Fact]
        public void MapKhac_GiuNguyenBoTileGoc()
        {
            const int loiDaiMapId = 20;

            Assert.Equal(SnowBorder, MapSkinOverrides.ResolveImageId(loiDaiMapId, SnowBorder));
            Assert.Equal(SnowGrassA, MapSkinOverrides.ResolveImageId(loiDaiMapId, SnowGrassA));
        }

        /// <summary>Dải không nằm trong bảng đổi thì trả về chính nó.</summary>
        [Fact]
        public void DaiKhongKhaiBao_TraVeChinhNo()
        {
            const int someStrip = 158;

            Assert.Equal(someStrip,
                MapSkinOverrides.ResolveImageId(MapSkinOverrides.ArenaMapId, someStrip));
        }
    }
}
