using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Toán camera map — offset theo faceDir + clamp cạnh map. Khớp ew.java:308-312, 545-581.</summary>
    public sealed class CameraClampTests
    {
        [Fact]
        public void EffectiveViewport_LayMin_MapVsScreen()
        {
            // Map nhỏ hơn màn hình → viewport = map size (không lộ ngoài).
            var (w, h) = CameraClamp.EffectiveViewport(200, 150, 320, 240);
            Assert.Equal(200, w);
            Assert.Equal(150, h);
        }

        [Fact]
        public void TopLeftFor_FacePhai_PlayerO2Phan3ViewNgang()
        {
            // Face phải: player ở 2/3 viewport ngang → camX = playerX - viewW*2/3
            var (camX, camY) = CameraClamp.TopLeftFor(playerX: 300, playerY: 200, viewW: 320, viewH: 240, faceRight: true);
            Assert.Equal(300 - 320 / 3 * 2, camX); // 300 - 212 = 88
            Assert.Equal(200 - 240 / 3 * 2, camY); // 200 - 160 = 40
        }

        [Fact]
        public void TopLeftFor_FaceTrai_PlayerO1Phan3ViewNgang()
        {
            var (camX, _) = CameraClamp.TopLeftFor(playerX: 300, playerY: 200, viewW: 320, viewH: 240, faceRight: false);
            Assert.Equal(300 - 320 / 3, camX); // 300 - 106 = 194
        }

        [Theory]
        [InlineData(-50, 100, 500, 0)]  // âm → 0
        [InlineData(50, 100, 500, 50)]  // trong khoảng → giữ nguyên
        [InlineData(450, 100, 500, 400)] // quá mép phải (500-100) → clamp
        [InlineData(600, 100, 500, 400)] // quá xa → clamp
        public void ClampAxis_GioiHanTrong0_VaMapSizeTruView(int c, int viewSize, int mapSize, int expected)
        {
            Assert.Equal(expected, CameraClamp.ClampAxis(c, viewSize, mapSize));
        }

        [Fact]
        public void ClampAxis_MapNhoHonView_LuonTraVe0()
        {
            Assert.Equal(0, CameraClamp.ClampAxis(50, 320, 200));
            Assert.Equal(0, CameraClamp.ClampAxis(-10, 320, 200));
        }

    }
}
