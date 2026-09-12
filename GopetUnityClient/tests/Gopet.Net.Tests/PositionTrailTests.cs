using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class PositionTrailTests
    {
        [Fact]
        public void PointBehind_DiThang_GiuKhoangCachTheoDuongDi()
        {
            var trail = new PositionTrail();
            trail.Reset(0f, 0f);
            for (var sampleX = 10; sampleX <= 50; sampleX += 10) trail.Add(sampleX, 0f);

            var (x, y) = trail.PointBehind(20f);

            Assert.Equal(30f, x, 3);
            Assert.Equal(0f, y, 3);
        }

        [Fact]
        public void PointBehind_QuaGocVuong_DiTheoQuyDaoCuThayViCatGoc()
        {
            var trail = new PositionTrail();
            trail.Reset(0f, 0f);
            trail.Add(40f, 0f);
            trail.Add(40f, 30f);

            var (x, y) = trail.PointBehind(40f);

            Assert.Equal(30f, x, 3);
            Assert.Equal(0f, y, 3);
        }

        [Fact]
        public void PointBehind_ChuaDiDuKhoangCach_GiuTaiDiemBatDau()
        {
            var trail = new PositionTrail();
            trail.Reset(5f, 7f);
            trail.Add(15f, 7f);

            var (x, y) = trail.PointBehind(40f);

            Assert.Equal(5f, x, 3);
            Assert.Equal(7f, y, 3);
        }

        [Fact]
        public void PointBehind_PetBanDauBenPhai_KhongNhayKhiChuDiSangTrai()
        {
            var trail = new PositionTrail();
            trail.Reset(28f, 0f); // initial pet ground position
            trail.Add(0f, 0f);    // owner position
            trail.Add(-12f, 0f);  // owner has just begun moving left

            var (x, y) = trail.PointBehind(40f);

            Assert.Equal(28f, x, 3);
            Assert.Equal(0f, y, 3);
        }
    }
}
