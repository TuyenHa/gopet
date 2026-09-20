using Gopet.Net.Guider;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Nút bình máu bấm một phát là uống: client tự chọn hộ dòng trong menu vật phẩm
    /// hỗ trợ mà server trả về.
    /// </summary>
    public sealed class BattleSupportItemsTests
    {
        private static MenuScreen Screen(params MenuItemInfo[] items) =>
            new MenuScreen { ListId = 1016, Title = "Chọn vật phẩm", Items = items };

        private static MenuItemInfo Item(string title, string description = "", bool canSelect = true) =>
            new MenuItemInfo { Title = title, Description = description, CanSelect = canSelect };

        [Fact]
        public void PickHealing_TuiRong_TraVeNone()
        {
            Assert.Equal(BattleSupportItems.None, BattleSupportItems.PickHealing(Screen()));
            Assert.Equal(BattleSupportItems.None, BattleSupportItems.PickHealing(null));
        }

        [Fact]
        public void PickHealing_UuTienBinhMau_BoQuaBinhMana()
        {
            var screen = Screen(
                Item("Bình mana nhỏ", "Hồi phục 100 MP"),
                Item("Bình máu nhỏ", "Hồi phục 500 HP"));

            Assert.Equal(1, BattleSupportItems.PickHealing(screen));
        }

        /// <summary>Tên không nhắc máu nhưng mô tả có — vẫn phải nhận ra.</summary>
        [Fact]
        public void PickHealing_NhanRaQuaMoTa()
        {
            var screen = Screen(
                Item("Bình mana nhỏ", "Hồi phục 100 MP"),
                Item("Tiên đan", "Hồi phục 30% HP tối đa"));

            Assert.Equal(1, BattleSupportItems.PickHealing(screen));
        }

        /// <summary>Không nhận ra dòng nào là bình máu thì vẫn dùng vật phẩm đầu tiên,
        /// còn hơn báo "hết bình" trong khi túi vẫn có đồ dùng được.</summary>
        [Fact]
        public void PickHealing_KhongCoBinhMau_DungVatPhamDauTien()
        {
            var screen = Screen(Item("Bình mana nhỏ", "Hồi phục 100 MP"));

            Assert.Equal(0, BattleSupportItems.PickHealing(screen));
        }

        [Fact]
        public void PickHealing_BoQuaDongKhongChonDuoc()
        {
            var screen = Screen(
                Item("Bình máu lớn", "Hồi phục 5000 HP", canSelect: false),
                Item("Bình máu nhỏ", "Hồi phục 500 HP"));

            Assert.Equal(1, BattleSupportItems.PickHealing(screen));
        }
    }
}
