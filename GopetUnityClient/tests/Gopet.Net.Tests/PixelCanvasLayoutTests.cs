using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Khung tham chiếu 320×240 neo theo chiều cao. Các cỡ máy dùng trong test là
    /// đúng những cỡ đã liệt kê ở phase-05-1 (điện thoại, iPad, PC cửa sổ).
    /// </summary>
    public sealed class PixelCanvasLayoutTests
    {
        [Theory]
        [InlineData(2400, 1080, 4, 600)]   // điện thoại 2400x1080
        [InlineData(1920, 1080, 4, 480)]   // điện thoại / PC 1920x1080
        [InlineData(2732, 2048, 8, 341.5f)]   // iPad
        public void CacCoMayDaBiet_RaDungSoTrongPlan(float w, float h, int expectedScale, float expectedLogicalWidth)
        {
            var layout = PixelCanvasLayout.Compute(w, h);

            Assert.Equal(expectedScale, layout.Scale);
            Assert.Equal(expectedLogicalWidth, layout.LogicalWidth, 0);
        }

        /// <summary>N phải luôn là số nguyên — đây là toàn bộ lý do PixelCanvas tồn tại.</summary>
        [Theory]
        [InlineData(100, 999)]
        [InlineData(1, 1234)]
        [InlineData(1920, 1)]
        public void N_LuonLaSoNguyen(float w, float h)
        {
            var layout = PixelCanvasLayout.Compute(w, h);

            Assert.True(layout.Scale >= 1);
            Assert.Equal(layout.Scale, (int)layout.Scale);
        }

        /// <summary>Chiều cao đã phóng không bao giờ vượt quá chiều cao thật của màn hình.</summary>
        [Theory]
        [InlineData(2400, 1080)]
        [InlineData(1920, 1080)]
        [InlineData(2732, 2048)]
        [InlineData(1000, 241)]
        public void CaoDaPhong_KhongVuotQuaManHinh(float w, float h)
        {
            var layout = PixelCanvasLayout.Compute(w, h);

            Assert.True(layout.ScaledHeight <= h);
            Assert.Equal(h - layout.ScaledHeight, layout.Letterbox, 3);
        }

        /// <summary>Chiều ngang logic phải tràn hết màn hình khi phóng lại đúng N lần.</summary>
        [Theory]
        [InlineData(2400, 1080)]
        [InlineData(1920, 1080)]
        [InlineData(2732, 2048)]
        public void RongLogic_PhongLaiDungBangChieuRongManHinh(float w, float h)
        {
            var layout = PixelCanvasLayout.Compute(w, h);

            Assert.Equal(w, layout.LogicalWidth * layout.Scale, 2);
        }

        /// <summary>Màn hình rộng hơn 4:3 (mọi máy nằm ngang hiện đại) thì rộng logic phải ≥ 320 — dư lề, không cụt bố cục jar.</summary>
        [Theory]
        [InlineData(2400, 1080)]
        [InlineData(1920, 1080)]
        [InlineData(2732, 2048)]
        public void ManRongHonJar_ThiRongLogicKhongDuoi320(float w, float h)
        {
            var layout = PixelCanvasLayout.Compute(w, h);

            Assert.True(layout.LogicalWidth >= 320f, $"Rộng logic {layout.LogicalWidth} < 320 — bố cục jar sẽ bị cụt.");
        }

        /// <summary>Màn thấp hơn khung tham chiếu (test runner -nographics, cửa sổ Editor thu nhỏ) không được chia cho 0.</summary>
        [Theory]
        [InlineData(400, 100)]
        [InlineData(50, 50)]
        public void ManThapHonKhungThamChieu_KhongNemVaScaleLaMot(float w, float h)
        {
            var layout = PixelCanvasLayout.Compute(w, h);

            Assert.Equal(1, layout.Scale);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(100, 0)]
        [InlineData(-1, 100)]
        public void KichThuocVoLy_ThiNem(float w, float h)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => PixelCanvasLayout.Compute(w, h));
        }
    }
}
