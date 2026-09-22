using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class MapDirtSkinOverridesTests
    {
        [Theory]
        [InlineData(12)][InlineData(13)][InlineData(14)][InlineData(15)]
        [InlineData(17)][InlineData(18)][InlineData(20)][InlineData(22)]
        public void DuongDat_DungDungBoAnhLinhLam(int mapId)
        {
            Assert.Equal(12151, MapSkinOverrides.ResolveImageId(mapId, 151));
            Assert.Equal(12161, MapSkinOverrides.ResolveImageId(mapId, 161));
            Assert.Equal(12162, MapSkinOverrides.ResolveImageId(mapId, 162));
        }

        [Theory]
        [InlineData(17)][InlineData(18)][InlineData(21)][InlineData(22)]
        public void NuiVaHang_DoiCaNenVaMepTuyet(int mapId)
        {
            Assert.Equal(12179, MapSkinOverrides.ResolveImageId(mapId, 179));
            Assert.Equal(12180, MapSkinOverrides.ResolveImageId(mapId, 180));
        }

        [Theory]
        [InlineData(12)][InlineData(13)][InlineData(14)][InlineData(15)][InlineData(20)]
        public void MapRung_KhongNhanBoNui(int mapId)
        {
            Assert.Equal(179, MapSkinOverrides.ResolveImageId(mapId, 179));
            Assert.Equal(180, MapSkinOverrides.ResolveImageId(mapId, 180));
        }

        [Theory]
        [InlineData(23)][InlineData(24)][InlineData(25)][InlineData(26)]
        [InlineData(27)][InlineData(28)][InlineData(29)][InlineData(30)]
        [InlineData(31)][InlineData(32)][InlineData(33)][InlineData(34)]
        [InlineData(-1)][InlineData(0)][InlineData(999)]
        public void BangSongMayVaMapLa_KhongBiDoiLay(int mapId)
        {
            foreach (var id in new[] { 3, 151, 161, 162, 179, 180, 219, 220, 221, 222,
                241, 242, 243, 244, 245, 246, 295, 299, 334, 337, 371 })
                Assert.Equal(id, MapSkinOverrides.ResolveImageId(mapId, id));
        }

        [Theory]
        [InlineData(12)][InlineData(14)][InlineData(17)][InlineData(18)]
        [InlineData(20)][InlineData(21)][InlineData(22)]
        public void MapDoiDat_KhongDoiDaNuocMayHoacVatThe(int mapId)
        {
            foreach (var id in new[] { 3, 158, 163, 177, 219, 220, 221, 222,
                242, 243, 244, 245, 246, 334, 337, 371 })
                Assert.Equal(id, MapSkinOverrides.ResolveImageId(mapId, id));
            foreach (var id in new[] { 151, 161, 162, 179, 180, 158, 177 })
                Assert.Equal(id, MapSkinOverrides.ResolveObjectImageId(mapId, id));
        }

        [Fact]
        public void ThachDong_ChiDoiBoNui()
        {
            foreach (var id in new[] { 151, 161, 162, 163 })
                Assert.Equal(id, MapSkinOverrides.ResolveImageId(21, id));
        }
    }
}
