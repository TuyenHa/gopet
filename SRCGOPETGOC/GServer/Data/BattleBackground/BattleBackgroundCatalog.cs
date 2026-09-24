using System.Collections.Generic;

namespace Gopet.Data.BattleBackground
{
    /// <summary>Một khung cảnh màn đấu. Giá chỉ nằm ở server; client nhận qua gói STATE.</summary>
    public sealed record BattleBackgroundDef(sbyte Id, string Name, long PriceGold);

    /// <summary>
    /// Danh mục khung cảnh màn đấu (PvE, PvP, đấu trường). Id là chỉ số trong mảng và phải
    /// khớp <c>Gopet.UiLogic.BattleSceneCatalog</c> phía client (ảnh nền + hiệu ứng).
    /// Id 0 là rừng mặc định, luôn sở hữu. Thêm khung cảnh mới: nối vào CUỐI mảng.
    /// </summary>
    public static class BattleBackgroundCatalog
    {
        public const sbyte DefaultId = 0;

        public static readonly IReadOnlyList<BattleBackgroundDef> All = new[]
        {
            new BattleBackgroundDef(0, "Rừng", 0),
            new BattleBackgroundDef(1, "Rừng cây che", 7_000),
            new BattleBackgroundDef(2, "Hoa anh đào", 10_000),
            new BattleBackgroundDef(3, "Tuyết trắng", 12_000),
            new BattleBackgroundDef(4, "Hang động đá", 17_000),
            new BattleBackgroundDef(5, "Mưa lửa", 22_000),
        };

        /// <summary>Tra khung cảnh theo id; null nếu id nằm ngoài danh mục.</summary>
        public static BattleBackgroundDef Find(int id) => id >= 0 && id < All.Count ? All[id] : null;
    }
}
