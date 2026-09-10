using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class MapCollisionTests
    {
        [Fact]
        public void CanStand_ChanNgoaiMapVaMatNaDay()
        {
            var map = MapWithMask(15);
            Assert.False(MapCollision.CanStand(map, -1, 0));
            Assert.False(MapCollision.CanStand(map, 0, -1));
            Assert.False(MapCollision.CanStand(map, 24, 0));
            Assert.False(MapCollision.CanStand(map, 5, 5));
        }

        [Fact]
        public void CanStand_MatNaRongChoDiCaBonPhanTu()
        {
            var map = MapWithMask(0);
            Assert.True(MapCollision.CanStand(map, 1, 1));
            Assert.True(MapCollision.CanStand(map, 13, 1));
            Assert.True(MapCollision.CanStand(map, 1, 13));
            Assert.True(MapCollision.CanStand(map, 13, 13));
        }

        [Fact]
        public void CanStand_MatNaMotChanDungPhanTuDuoiPhai()
        {
            var map = MapWithMask(1);
            Assert.True(MapCollision.CanStand(map, 1, 1));
            Assert.True(MapCollision.CanStand(map, 13, 1));
            Assert.True(MapCollision.CanStand(map, 1, 13));
            Assert.False(MapCollision.CanStand(map, 13, 13));
        }

        private static JarMapLayout MapWithMask(byte mask) => new JarMapLayout
        {
            WidthTiles = 1,
            HeightTiles = 1,
            Collision = new[] { new[] { mask } }
        };
    }
}
