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

        [Fact]
        public void CanPlayerStand_GiuChanPlayerDuoiKhoangTrongPhiaTren()
        {
            var map = new JarMapLayout
            {
                WidthTiles = 10,
                HeightTiles = 10,
                Collision = EmptyCollision(10, 10)
            };

            Assert.False(MapCollision.CanPlayerStand(map, 100, 71));
            Assert.True(MapCollision.CanPlayerStand(map, 100, 72));
        }

        [Fact]
        public void CanPlayerStand_MapThapVanDungCollisionGoc()
        {
            var map = MapWithMask(0);
            Assert.True(MapCollision.CanPlayerStand(map, 1, 1));
        }

        [Fact]
        public void NearestVisiblePlayerY_DuaSelfKhoiMepTrenVaTonTrongCollision()
        {
            var collision = EmptyCollision(10, 10);
            collision[3][4] = 15; // Y 72..95 is blocked at X 100.
            var map = new JarMapLayout
            {
                WidthTiles = 10,
                HeightTiles = 10,
                Collision = collision
            };

            Assert.Equal(96, MapCollision.NearestVisiblePlayerY(map, 100, 0));
            Assert.Equal(100, MapCollision.NearestVisiblePlayerY(map, 100, 100));
        }

        private static byte[][] EmptyCollision(int width, int height)
        {
            var rows = new byte[height][];
            for (var y = 0; y < height; y++) rows[y] = new byte[width];
            return rows;
        }

        private static JarMapLayout MapWithMask(byte mask) => new JarMapLayout
        {
            WidthTiles = 1,
            HeightTiles = 1,
            Collision = new[] { new[] { mask } }
        };
    }
}
