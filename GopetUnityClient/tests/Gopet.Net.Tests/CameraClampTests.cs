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

        /// <summary>Map 12 (ải) rộng 360 trên màn 16:9 từng lộ hai dải nền xanh navy hai bên:
        /// khung nhìn cao 240 × aspect 1.95 = rộng 468 > 360. Xem <c>FitOrthographicSize</c>.</summary>
        [Fact]
        public void FitOrthographicSize_MapHepHonKhungNhin_ThuTamNhinLai()
        {
            var fit = CameraClamp.FitOrthographicSize(120f, 360, 360, 1.95f);

            Assert.True(fit < 120f, "phải thu nhỏ, nếu không hai mép vẫn hở nền");
            // Thu vừa đủ để bề ngang khung nhìn bằng đúng bề ngang map.
            Assert.Equal(360f, fit * 2f * 1.95f, 3);
        }

        [Fact]
        public void FitOrthographicSize_MapRongHonKhungNhin_GiuNguyen()
        {
            // 768×576 ở aspect 1.95: khung nhìn 468×240, map trùm hết → không đụng vào zoom.
            Assert.Equal(120f, CameraClamp.FitOrthographicSize(120f, 768, 576, 1.95f));
        }

        [Fact]
        public void FitOrthographicSize_MapThapHonKhungNhin_ThuTheoChieuCao()
        {
            // Map 900×180: đủ rộng nhưng thấp hơn 240 → cạnh trên/dưới mới là chỗ hở.
            Assert.Equal(90f, CameraClamp.FitOrthographicSize(120f, 900, 180, 1.95f));
        }

        [Theory]
        [InlineData(0, 360, 1.95f)]   // map rỗng
        [InlineData(360, 0, 1.95f)]
        [InlineData(360, 360, 0f)]    // aspect 0: màn chưa dựng xong
        public void FitOrthographicSize_DuLieuVoLy_GiuNguyenThayViTraVe0(int w, int h, float aspect)
        {
            // orthographicSize 0 làm camera không vẽ nổi gì — thà giữ giá trị mong muốn.
            Assert.Equal(120f, CameraClamp.FitOrthographicSize(120f, w, h, aspect));
        }

        /// <summary>1080 / 240 = 4.5 pixel màn cho mỗi pixel nguồn — tròn lên 5, tầm nhìn
        /// còn 216 đơn vị. Đây là ca hay gặp nhất (màn 1080p, khung nhìn 240).</summary>
        [Fact]
        public void SnapOrthographicSize_TiLeLe_TronLenThanhSoNguyen()
        {
            Assert.Equal(108f, CameraClamp.SnapOrthographicSize(120f, 1080));
        }

        [Theory]
        [InlineData(120f, 960)]    // 960/240 = 4 chẵn
        [InlineData(120f, 1200)]   // 1200/240 = 5 chẵn
        [InlineData(90f, 720)]     // 720/180 = 4 chẵn
        public void SnapOrthographicSize_DaChan_GiuNguyen(float size, int screenHeight)
        {
            Assert.Equal(size, CameraClamp.SnapOrthographicSize(size, screenHeight));
        }

        /// <summary>Chỉ được THU tầm nhìn: nới ra sẽ lộ ra ngoài biên map mà
        /// FitOrthographicSize vừa kẹp xong.</summary>
        [Theory]
        [InlineData(1080)]
        [InlineData(1050)]
        [InlineData(800)]
        [InlineData(1440)]
        public void SnapOrthographicSize_KhongBaoGioNoiRongTamNhin(int screenHeight)
        {
            Assert.True(CameraClamp.SnapOrthographicSize(120f, screenHeight) <= 120f);
        }

        /// <summary>Màn thấp hơn cả khung nhìn thì không có tỉ lệ nguyên nào dùng được —
        /// giữ nguyên cỡ, thà đúng khung còn hơn vỡ bố cục.</summary>
        [Theory]
        [InlineData(120f, 200)]
        [InlineData(120f, 0)]
        [InlineData(0f, 1080)]
        public void SnapOrthographicSize_KhongChotDuoc_GiuNguyen(float size, int screenHeight)
        {
            Assert.Equal(size, CameraClamp.SnapOrthographicSize(size, screenHeight));
        }
    }
}
