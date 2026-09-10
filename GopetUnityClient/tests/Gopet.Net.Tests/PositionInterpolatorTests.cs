using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Nội suy exponential — client làm mượt bước nhảy vị trí server gửi.</summary>
    public sealed class PositionInterpolatorTests
    {
        [Fact]
        public void Snap_DatCaHienTaiVaTarget()
        {
            var p = new PositionInterpolator();
            p.Snap(10, 20);
            Assert.Equal(10, p.X);
            Assert.Equal(20, p.Y);
            Assert.Equal(10, p.TargetX);
            Assert.Equal(20, p.TargetY);
        }

        [Fact]
        public void Tick_TienDenTargetNhungKhongVuot()
        {
            var p = new PositionInterpolator { RatePerSecond = 10f };
            p.Snap(0, 0);
            p.SetTarget(100, 100);
            p.Tick(0.1f);

            // Sau 100ms với rate=10, đã đi ~63% quãng đường.
            Assert.InRange(p.X, 50, 80);
            Assert.InRange(p.Y, 50, 80);

            // Không bao giờ vượt qua target.
            Assert.True(p.X <= 100);
            Assert.True(p.Y <= 100);
        }

        [Fact]
        public void Tick_HoiTuVeTargetSauNhieuBuoc()
        {
            var p = new PositionInterpolator { RatePerSecond = 10f };
            p.Snap(0, 0);
            p.SetTarget(50, 50);
            for (var i = 0; i < 100; i++) p.Tick(0.05f);

            Assert.Equal(50, p.X, 1);
            Assert.Equal(50, p.Y, 1);
        }

        [Fact]
        public void SetTargetGiuaChung_KhongGiat()
        {
            var p = new PositionInterpolator { RatePerSecond = 10f };
            p.Snap(0, 0);
            p.SetTarget(100, 0);
            p.Tick(0.1f);
            var mid = p.X;

            // Đổi target giữa chừng — không được nhảy về đâu cả.
            p.SetTarget(200, 0);
            Assert.Equal(mid, p.X);
        }

        [Fact]
        public void TickAmHoacBang0_KhongLamGi()
        {
            var p = new PositionInterpolator();
            p.Snap(5, 5);
            p.SetTarget(10, 10);
            p.Tick(0);
            p.Tick(-1);
            Assert.Equal(5, p.X);
        }
    }
}
