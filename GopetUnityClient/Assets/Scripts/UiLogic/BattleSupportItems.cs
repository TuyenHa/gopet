using Gopet.Net.Guider;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Chọn hộ dòng bình máu trong menu vật phẩm hỗ trợ của màn đánh quái
    /// (<c>MenuController.MENU_SELECT_ITEM_SUPPORT_PET</c>).
    ///
    /// <para>Thuần C#, không dính UnityEngine để test headless được.</para>
    /// </summary>
    public static class BattleSupportItems
    {
        /// <summary>Không có dòng nào dùng được.</summary>
        public const int None = -1;

        /// <summary>
        /// Chỉ số dòng nên uống. Menu gom MỌI vật phẩm hỗ trợ (cả bình mana) mà client
        /// chỉ thấy tên + mô tả chứ không có chỉ số hồi phục, nên dò chữ: ưu tiên dòng
        /// nhắc tới máu/HP; không nhận ra dòng nào thì lấy vật phẩm dùng được đầu tiên —
        /// còn hơn báo "hết bình" trong khi túi vẫn có đồ.
        /// </summary>
        public static int PickHealing(MenuScreen screen)
        {
            var items = screen?.Items;
            if (items == null) return None;

            var fallback = None;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] == null || !items[i].CanSelect) continue;
                if (fallback == None) fallback = i;
                if (MentionsHealth(items[i].Title) || MentionsHealth(items[i].Description)) return i;
            }
            return fallback;
        }

        private static bool MentionsHealth(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            var lower = text.ToLowerInvariant();
            return lower.Contains("máu") || lower.Contains("hp");
        }
    }
}
