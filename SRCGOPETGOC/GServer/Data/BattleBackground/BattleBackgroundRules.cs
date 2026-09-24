using System;
using Gopet.Data.Collections;

namespace Gopet.Data.BattleBackground
{
    public enum BattleBackgroundResult { Ok, InvalidId, AlreadyOwned, NotOwned, NotEnoughGold }

    /// <summary>
    /// Luật mua/chọn khung cảnh, thuần trên <see cref="PlayerData"/> để test được không cần
    /// session. Trừ vàng đi qua <c>trySpend</c> do tầng gọi cấp (Player.checkGold + mineGold).
    /// </summary>
    public static class BattleBackgroundRules
    {
        public static bool Owns(PlayerData pd, int id) =>
            id == BattleBackgroundCatalog.DefaultId || (pd.BattleBgOwned?.Contains(id) ?? false);

        /// <summary>Khung cảnh đang dùng; cột DB lệch (id lạ, chưa sở hữu) thì về mặc định.</summary>
        public static sbyte EffectiveSelected(PlayerData pd)
        {
            int id = pd.BattleBgSelected;
            return BattleBackgroundCatalog.Find(id) != null && Owns(pd, id)
                ? (sbyte)id
                : BattleBackgroundCatalog.DefaultId;
        }

        /// <summary>
        /// Mua rồi chọn luôn. Khoá theo người chơi để bấm liên tiếp (hoặc gói trùng) không trừ
        /// vàng hai lần: lần sau thấy đã sở hữu và trả <see cref="BattleBackgroundResult.AlreadyOwned"/>.
        /// </summary>
        public static BattleBackgroundResult Buy(PlayerData pd, int id, Func<long, bool> trySpend)
        {
            var def = BattleBackgroundCatalog.Find(id);
            if (def == null || id == BattleBackgroundCatalog.DefaultId) return BattleBackgroundResult.InvalidId;
            lock (pd.BattleBgLock)
            {
                if (Owns(pd, id)) return BattleBackgroundResult.AlreadyOwned;
                if (!trySpend(def.PriceGold)) return BattleBackgroundResult.NotEnoughGold;
                pd.BattleBgOwned ??= new CopyOnWriteArrayList<int>();
                pd.BattleBgOwned.addIfAbsent(id);
                pd.BattleBgSelected = id;
            }
            return BattleBackgroundResult.Ok;
        }

        public static BattleBackgroundResult Select(PlayerData pd, int id)
        {
            if (BattleBackgroundCatalog.Find(id) == null) return BattleBackgroundResult.InvalidId;
            lock (pd.BattleBgLock)
            {
                if (!Owns(pd, id)) return BattleBackgroundResult.NotOwned;
                pd.BattleBgSelected = id;
            }
            return BattleBackgroundResult.Ok;
        }
    }
}
