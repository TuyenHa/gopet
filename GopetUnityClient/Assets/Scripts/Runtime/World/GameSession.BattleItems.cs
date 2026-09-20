using Gopet.Net.Guider;
using Gopet.UiLogic;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Nút bình máu trong màn đánh quái: bấm MỘT phát là uống luôn.
    ///
    /// <para>Server không có lệnh "uống bình máu". Bấm nút gửi
    /// <c>PET_BATTLE_USE_ITEM</c>, server trả về MENU vật phẩm hỗ trợ
    /// (<c>MENU_SELECT_ITEM_SUPPORT_PET</c> = 1016) rồi mới chờ người chơi chọn dòng;
    /// chọn xong <c>PetBattle.useItem</c> mới cộng HP và trừ túi đồ. Ở đây client tự
    /// chọn hộ luôn dòng bình máu — giữa trận mà phải mở menu, cuộn, rồi xác nhận thì
    /// một lượt đánh mất mấy giây.</para>
    ///
    /// <para>Menu này đi chung đường <c>SHOW_MENU_ITEM</c> với mọi menu khác nên phải
    /// chặn ở <see cref="TryConsumeHudMenu"/>, và CHỈ chặn khi màn đấu đang mở — ngoài
    /// trận thì cùng listId đó vẫn phải hiện menu bình thường.</para>
    /// </summary>
    public sealed partial class GameSession
    {
        /// <summary><c>MenuController.MENU_SELECT_ITEM_SUPPORT_PET</c>.</summary>
        private const int SupportItemMenuId = 1016;

        /// <summary>Câu báo khi túi đồ sạch bình máu.</summary>
        private const string NoPotionMessage = "Hết bình máu rồi!";

        private bool TryConsumeBattleItemMenu(MenuScreen screen)
        {
            if (screen == null || screen.ListId != SupportItemMenuId) return false;
            var view = _battle?.View;
            if (view == null) return false;

            var index = BattleSupportItems.PickHealing(screen);
            if (index == BattleSupportItems.None)
            {
                // Toast của HUD nằm ở canvas 35, dưới màn đấu (40) — báo phải mọc
                // TRONG màn đấu, không thì người chơi bấm mãi mà tưởng nút hỏng.
                view.ShowNotice(NoPotionMessage);
                return true;
            }

            _guider.Select(screen, index);
            return true;
        }

    }
}
