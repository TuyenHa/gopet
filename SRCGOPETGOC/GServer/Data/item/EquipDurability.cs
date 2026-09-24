using Gopet.Data.GopetItem;

namespace Gopet.Data.item
{
    public enum WearResult { None, Warned, Broke }

    /// <summary>
    /// Luật độ bền trang bị pet (nón, kiếm, giày, bao tay, giáp). Thuần trên <see cref="Item"/>
    /// để test được không cần DB. Chỉnh cân bằng ở các hằng số dưới đây.
    ///
    /// <para>Món hỏng (độ bền 0) chỉ mất chỉ số RIÊNG của nó — bonus set vẫn tính
    /// (<c>Pet.applyInfo</c>). Sửa ở Thợ Rèn (Thành phố Linh Thú) bằng Đá mài sửa chữa.</para>
    /// </summary>
    public static class EquipDurability
    {
        public const int Max = 80;
        public const int WearWin = 1;
        public const int WearLose = 2;
        /// <summary>Xuống tới mức này (10%) thì báo "sắp hỏng" một lần.</summary>
        public const int WarnAt = 8;

        public static bool Applies(Item item) => item?.Template?.IsEquip == true;

        public static bool IsBroken(Item item) => Applies(item) && item.durability <= 0;

        public static bool NeedsRepair(Item item) => Applies(item) && item.durability < Max;

        /// <summary>
        /// Trừ độ bền sau một trận. Trả mốc vừa VƯỢT QUA trong lần này (để báo đúng một lần),
        /// không phải trạng thái hiện tại.
        /// </summary>
        public static WearResult Wear(Item item, bool lost)
        {
            if (!Applies(item) || item.durability <= 0) return WearResult.None;
            int before = item.durability;
            item.durability = System.Math.Max(0, before - (lost ? WearLose : WearWin));
            if (item.durability == 0) return WearResult.Broke;
            if (before > WarnAt && item.durability <= WarnAt) return WearResult.Warned;
            return WearResult.None;
        }

        public static void Repair(Item item) => item.durability = Max;

        /// <summary>
        /// Chữ độ bền gắn vào tên/mô tả; rỗng với đồ không phải trang bị pet. Luôn có dạng
        /// "Độ bền: X/Max" (kể cả khi hỏng) — popup Thợ Rèn của client đọc số từ đây.
        /// </summary>
        public static string Describe(Item item)
        {
            if (!Applies(item)) return "";
            var text = $" Độ bền: {System.Math.Max(0, item.durability)}/{Max}";
            return item.durability <= 0 ? text + " (Hỏng - mang tới Thợ Rèn để sửa)" : text;
        }
    }
}
