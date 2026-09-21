using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup cửa hàng: badge tiêu đề chờm lên mép trên, 4 tab (Vũ khí / Giáp / Mũ /
    /// Thức ăn), danh sách thẻ item có nút giá, và nút X đỏ ở góc trên-phải.
    ///
    /// <para><b>Không dùng <see cref="GenericMenuView"/>.</b> Bản trước nhúng menu
    /// generic vào, nhưng thẻ item của cửa hàng có khuôn riêng (tên + yêu cầu chỉ số,
    /// chip chỉ số, nút giá) mà dòng menu chung không mang nổi nếu không thêm nhánh
    /// riêng cho shop — đúng thứ <see cref="GenericMenuView"/> tồn tại để tránh. Danh
    /// sách cửa hàng chỉ vài chục dòng nên không cần virtualization; dựng thẳng
    /// <see cref="ShopItemRow"/>, cùng cách <see cref="PetGridView"/> đang làm.</para>
    ///
    /// <para>Cần <see cref="UiRoot"/> gọi <see cref="TryConsumeMenu"/> khi
    /// <see cref="GuiderHandler.MenuShown"/> bắn ra — nếu đúng của shop popup thì
    /// <b>swallow</b> để không đẻ thêm view đứng ngoài.</para>
    ///
    /// <para>Phần dựng hình nằm ở <c>ShopPopupView.Chrome.cs</c>.</para>
    /// </summary>
    public sealed partial class ShopPopupView : MonoBehaviour
    {
        // Server ID: MenuController.cs:423-435. GIỮ khớp với server, đừng đổi.
        public const sbyte ShopWeapon = 1;
        public const sbyte ShopArmour = 2;
        public const sbyte ShopHat = 3;
        public const sbyte ShopFood = 4;
        /// <summary>SHOP_SKIN. Server có quirk: SHOP_FOOD ở map 19 trả về SHOP_SKIN (GameController.cs:2367).</summary>
        public const sbyte ShopSkin = 7;

        private static readonly (sbyte Id, string Label)[] Tabs =
        {
            (ShopWeapon, "Vũ khí"),
            (ShopArmour, "Giáp"),
            (ShopHat,    "Mũ"),
            (ShopFood,   "Thức ăn"),
        };

        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private Font _font;
        private MenuScreen _screen;
        private sbyte _activeShopId = -1;

        public event Action Closed;

        /// <summary>Hỏi trước khi mua. <see cref="UiRoot"/> dựng hộp thoại đè lên popup.</summary>
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        /// <summary>Báo ngắn cho người chơi (thiếu tiền). <see cref="UiRoot"/> đẩy ra toast.</summary>
        public event Action<string> Message;

        public sbyte ActiveShopId => _activeShopId;

        /// <summary>Số thẻ item đang dựng — cho PlayMode test soi mà không phải đào GameObject.</summary>
        public int RowCount => _list.RowCount;

        /// <summary>
        /// Được gọi bởi <see cref="UiRoot"/> mỗi khi <see cref="GuiderHandler.MenuShown"/>
        /// bắn ra. Nếu <paramref name="screen"/> thuộc HỌ shop (bất kỳ shopId nào) thì
        /// popup <b>swallow</b> — kể cả khi listId KHÔNG khớp tab active. Lý do:
        /// <list type="bullet">
        /// <item>Chống race đổi tab: user bấm Vũ khí → server đang trả, user bấm tiếp Mũ.
        /// Gói Vũ khí về sau, khớp shop family nhưng khác <c>_activeShopId</c> — không
        /// nuốt thì <see cref="UiRoot"/> đẻ <see cref="GenericMenuView"/> đè popup.</item>
        /// <item>Server quirk: <c>SHOP_FOOD</c> ở map 19 trả về <c>SHOP_SKIN=7</c>
        /// (<c>GameController.cs:2367-2371</c>) — không có tab nhưng vẫn thuộc họ shop.</item>
        /// </list>
        /// Chỉ dựng lại danh sách khi listId KHỚP tab đang chọn; còn lại swallow im lặng.
        /// Server đặt <c>listId = shopId</c> khi <c>showShop(...)</c>
        /// (<c>MenuController.cs:875</c>).
        /// </summary>
        public bool TryConsumeMenu(MenuScreen screen)
        {
            if (screen == null) return false;
            if (!IsShopFamily(screen.ListId)) return false;

            if (screen.ListId == _activeShopId) Bind(screen);
            return true;
        }

        private static bool IsShopFamily(int listId) =>
            listId == ShopWeapon || listId == ShopArmour || listId == ShopHat
            || listId == ShopFood || listId == ShopSkin;

        private void Bind(MenuScreen screen)
        {
            _screen = screen;
            _list.Bind(screen, _assets);
            _frame.SetFooter(string.IsNullOrEmpty(screen.Title) ? DefaultFooter : screen.Title);
            if (screen.Items.Length == 0) _list.ShowPlaceholder("Cửa hàng chưa bày món nào.");
        }

        /// <summary>
        /// Gửi lệnh mua dòng <paramref name="index"/>. Dòng cửa hàng luôn có
        /// <c>showDialog</c> nên đi qua hộp xác nhận của server
        /// (<c>MenuController.showShop</c> đặt <c>DoYouWantBuyIt</c>); không ai nghe
        /// <see cref="ConfirmRequested"/> thì gửi thẳng còn hơn im lặng nuốt cú bấm.
        ///
        /// <para>Công khai để test gọi thẳng, khỏi phải mò <c>Button</c> trong cây
        /// GameObject — cùng lý do <see cref="GenericMenuView.OnRowClicked"/> công khai.</para>
        /// </summary>
        public void TryBuy(int index)
        {
            if (_screen == null || _guider == null) return;
            if (index < 0 || index >= _screen.Items.Length) return;

            var item = _screen.Items[index];
            if (!item.CanSelect) return;

            // Không đủ tiền thì báo tại chỗ chứ đừng gửi lên. Server CÓ trả lời thiếu
            // tiền cho vàng/ngọc/thỏi bạc… nhưng nhánh switch của nó
            // (selectMenu.cs:797-823) bỏ sót các loại tiền sự kiện như điểm hoa ngọc —
            // đúng loại cửa hàng thức ăn đang dùng — nên gửi lên là im lặng tuyệt đối.
            if (!CanAfford(item))
            {
                Message?.Invoke("Bạn cần " + ShopItemText.ShortMoney(item.PaymentOptions[0].MoneyText)
                                + " để thực hiện giao dịch");
                return;
            }

            var paymentIndex = item.PaymentOptions != null && item.PaymentOptions.Length > 0 ? 0 : -1;
            void Send() => _guider.Select(_screen, index, paymentIndex);

            if (item.ShowDialog && ConfirmRequested != null)
                ConfirmRequested(MenuSelection.PromptFor(_screen, index), Send);
            else
                Send();
        }

        private static bool CanAfford(MenuItemInfo item)
        {
            var options = item.PaymentOptions;
            return options == null || options.Length == 0 || options[0].IsEnabled != 0;
        }

        /// <summary>Chuyển tab. Công khai để test gọi thẳng, không phải qua <c>Button</c>.</summary>
        public void SelectTab(sbyte shopId)
        {
            if (_activeShopId == shopId) return;

            _activeShopId = shopId;
            // Gọi thẳng SelectTab (test, hoặc code khác) cũng phải kéo theo khay tab.
            // Rail bỏ qua khi trùng tab đang chọn nên không có vòng lặp.
            for (var i = 0; i < Tabs.Length; i++)
            {
                if (Tabs[i].Id == shopId) _rail.Select(i);
            }

            // Danh sách cũ thuộc tab khác — xoá ngay thay vì để nó đứng đó tới lúc gói
            // mới về, người chơi bấm mua nhầm món của tab trước.
            _screen = null;
            _list.Clear();
            _frame.SetFooter(DefaultFooter);
            _list.ShowPlaceholder("Đang tải…");

            // Server sẽ trả về MenuScreen, UiRoot forward vào TryConsumeMenu.
            _guider.RequestShop(shopId);
        }
    }
}
