using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Chuyển đổi toạ độ jar → world. Loại lỗi dễ mắc: quên đảo trục Y, sai công thức
    /// sortingOrder. Cả hai đều khó chẩn đoán trong Editor vì mọi ô "trông có vẻ" đúng.
    /// </summary>
    public sealed class MapPlacementTests
    {
        [Fact]
        public void JarToWorld_TrenTraiThanhDuoiTrai_YDuocDaoNguoc()
        {
            // Map cao 480 pixel: jarY = 0 (đỉnh) → world Y = 480 (đáy world khi camera nhìn từ dưới lên).
            var (x, y) = MapPlacement.JarToWorld(0, 0, 480);
            Assert.Equal(0f, x);
            Assert.Equal(480f, y);

            // jarY = 480 (đáy) → world Y = 0 (đáy map). Đúng gốc dưới-trái.
            var (x2, y2) = MapPlacement.JarToWorld(0, 480, 480);
            Assert.Equal(0f, x2);
            Assert.Equal(0f, y2);
        }

        [Fact]
        public void JarToWorld_GiuNguyenX()
        {
            var (x, _) = MapPlacement.JarToWorld(123, 456, 1000);
            Assert.Equal(123f, x);
        }

        [Fact]
        public void ObjectSortingOrder_JarYLonHonThiOrderLonHon()
        {
            // Cây gần đáy (jarY lớn) phải vẽ sau cây xa đáy — sortingOrder cao hơn.
            var near = MapPlacement.ObjectSortingOrder(300);
            var far = MapPlacement.ObjectSortingOrder(100);
            Assert.True(near > far, $"near={near} phải > far={far}");
        }

        [Fact]
        public void ObjectSortingOrder_LuonTrenLopNen()
        {
            // Base = 1000: vật thể ở y=0 vẫn cao hơn LayerSortingOrder cao nhất.
            var lowest = MapPlacement.ObjectSortingOrder(0);
            var topLayer = MapPlacement.LayerSortingOrder(99);
            Assert.True(lowest > topLayer);
        }

        [Fact]
        public void LayerSortingOrder_LayerCaoHonThiVeSau()
        {
            Assert.True(MapPlacement.LayerSortingOrder(1) > MapPlacement.LayerSortingOrder(0));
        }

        [Fact]
        public void WorldToJar_LaNghichDaoCuaJarToWorld()
        {
            const int mapH = 480;
            var (wx, wy) = MapPlacement.JarToWorld(123, 200, mapH);
            var (jx, jy) = MapPlacement.WorldToJar(wx, wy, mapH);
            Assert.Equal(123, jx);
            Assert.Equal(200, jy);
        }

        [Theory]
        [InlineData(10, 0, 0)]    // đông
        [InlineData(-10, 0, 1)]   // tây
        [InlineData(0, 10, 2)]    // nam trên trục jar
        [InlineData(0, -10, 3)]   // bắc trên trục jar
        [InlineData(5, 3, 0)]     // dx trội → đông
        [InlineData(3, 5, 2)]     // dy trội → nam
        public void Direction4_ChonHuongTheoTrucTroi(int dx, int dy, int expected)
        {
            Assert.Equal(expected, MapPlacement.Direction4(dx, dy));
        }
    }
}
