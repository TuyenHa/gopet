using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Thống kê fps của <c>MenuBench</c>. Đo được ngoài Unity vì nó chỉ là số học —
    /// còn con số thật thì phải lấy trên thiết bị.
    /// </summary>
    public sealed class FpsSamplerTests
    {
        private static FpsSampler Warmed()
        {
            var sampler = new FpsSampler();
            for (var i = 0; i < FpsSampler.WarmupFrames; i++) sampler.Add(1f);

            return sampler;
        }

        [Fact]
        public void FrameDauTien_BiBoQua()
        {
            var sampler = new FpsSampler();
            for (var i = 0; i < FpsSampler.WarmupFrames; i++) sampler.Add(1f);

            Assert.Equal(0, sampler.SampleCount);

            sampler.Add(1f / 60f);
            Assert.Equal(1, sampler.SampleCount);
        }

        [Fact]
        public void TrungBinh_TinhTrenToanBoMau()
        {
            var sampler = Warmed();
            for (var i = 0; i < 100; i++) sampler.Add(1f / 50f);

            Assert.Equal(50f, sampler.AverageFps(), 1);
        }

        /// <summary>
        /// 1% thấp phải bắt được cú khựng mà trung bình che mất: 99 frame 60fps +
        /// 1 frame 10fps thì trung bình vẫn ~57, còn 1% thấp phải là 10.
        /// </summary>
        [Fact]
        public void MotPhanTramThap_BatDuocCuKhung()
        {
            var sampler = Warmed();
            for (var i = 0; i < 99; i++) sampler.Add(1f / 60f);
            sampler.Add(1f / 10f);

            Assert.True(sampler.AverageFps() > 50f);
            Assert.Equal(10f, sampler.OnePercentLowFps(), 1);
        }

        [Fact]
        public void ChuaCoMau_TraVeKhong()
        {
            var sampler = new FpsSampler();

            Assert.Equal(0f, sampler.AverageFps());
            Assert.Equal(0f, sampler.OnePercentLowFps());
        }

        [Fact]
        public void DeltaKhongHopLe_BiBoQua()
        {
            var sampler = Warmed();
            sampler.Add(0f);
            sampler.Add(-1f);

            Assert.Equal(0, sampler.SampleCount);
        }

        [Fact]
        public void Reset_XoaCaMauLanDemWarmup()
        {
            var sampler = Warmed();
            sampler.Add(1f / 60f);

            sampler.Reset();

            Assert.Equal(0, sampler.SampleCount);
            sampler.Add(1f / 60f);
            Assert.Equal(0, sampler.SampleCount);
        }
    }
}
