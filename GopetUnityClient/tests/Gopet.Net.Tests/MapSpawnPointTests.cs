using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class MapSpawnPointTests
    {
        /// <summary>Mốc của map 23 (Băng động 1) — xếp đúng thứ tự kind 0..3.</summary>
        private static JarMapLayout IceCave1() => new JarMapLayout
        {
            Waypoints = new[]
            {
                new JarMapWaypoint { Kind = 0, X = 550, Y = 514 },
                new JarMapWaypoint { Kind = 1, X = 667, Y = 412 },
                new JarMapWaypoint { Kind = 2, X = 117, Y = 86 },
                new JarMapWaypoint { Kind = 3, X = 98, Y = 311 },
            }
        };

        /// <summary>Map 11 xếp lệch: mảng là [kind 2, kind 1, kind 0].</summary>
        private static JarMapLayout BeastCity() => new JarMapLayout
        {
            Waypoints = new[]
            {
                new JarMapWaypoint { Kind = 2, X = 35, Y = 241 },
                new JarMapWaypoint { Kind = 1, X = 482, Y = 243 },
                new JarMapWaypoint { Kind = 0, X = 250, Y = 412 },
            }
        };

        [Theory]
        // Cổng map 24 → Băng động 1 dùng index 2 và 3: hai bờ bắc/nam, đều trên cạn.
        [InlineData(2, 117, 86)]
        [InlineData(3, 98, 311)]
        // Menu dịch chuyển luôn gửi index 0.
        [InlineData(0, 550, 514)]
        public void Warp_DatNhanVatVaoMocTheoIndex(int index, int x, int y)
        {
            int px = MapSpawnPoint.WarpSentinel, py = MapSpawnPoint.WarpSentinel;
            MapSpawnPoint.Resolve(IceCave1(), index, ref px, ref py);
            Assert.Equal((x, y), (px, py));
        }

        [Fact]
        public void KhopTheoKindTruocThuTuMang()
        {
            int x = MapSpawnPoint.WarpSentinel, y = MapSpawnPoint.WarpSentinel;
            MapSpawnPoint.Resolve(BeastCity(), 0, ref x, ref y);
            Assert.Equal((250, 412), (x, y)); // kind 0, KHÔNG phải phần tử [0] = (35,241)
        }

        [Fact]
        public void ToaDoThat_GiuNguyen()
        {
            int x = 412, y = 96;
            MapSpawnPoint.Resolve(IceCave1(), 2, ref x, ref y);
            Assert.Equal((412, 96), (x, y));
        }

        [Fact]
        public void KhongCoMocKhop_GiuNguyenToaDoServer()
        {
            int x = MapSpawnPoint.WarpSentinel, y = MapSpawnPoint.WarpSentinel;
            MapSpawnPoint.Resolve(new JarMapLayout(), 0, ref x, ref y);
            Assert.Equal((MapSpawnPoint.WarpSentinel, MapSpawnPoint.WarpSentinel), (x, y));
        }
    }
}
