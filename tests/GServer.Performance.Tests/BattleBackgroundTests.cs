using Gopet.Data.BattleBackground;

static class BattleBackgroundTests
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }

    /// <summary>Ví vàng giả: trừ nếu đủ, giống Player.checkGold + mineGold.</summary>
    static Func<long, bool> Wallet(PlayerData pd, Action onSpend = null) => price =>
    {
        if (pd.gold < price) return false;
        pd.gold -= price;
        onSpend?.Invoke();
        return true;
    };

    public static void BuyAndSelect()
    {
        var pd = new PlayerData { gold = 50_000 };
        Check(BattleBackgroundRules.EffectiveSelected(pd) == 0, "default scene is not forest");
        Check(BattleBackgroundRules.Buy(pd, 3, Wallet(pd)) == BattleBackgroundResult.Ok, "buy failed");
        Check(pd.gold == 50_000 - BattleBackgroundCatalog.Find(3).PriceGold, "wrong gold charged");
        Check(BattleBackgroundRules.Owns(pd, 3) && pd.BattleBgSelected == 3, "bought scene not owned/selected");
        Check(BattleBackgroundRules.Buy(pd, 3, Wallet(pd)) == BattleBackgroundResult.AlreadyOwned, "re-buy allowed");
        Check(pd.gold == 38_000, "re-buy charged gold");
        Check(BattleBackgroundRules.Select(pd, 0) == BattleBackgroundResult.Ok && pd.BattleBgSelected == 0, "select default failed");
        Check(BattleBackgroundRules.Select(pd, 3) == BattleBackgroundResult.Ok && pd.BattleBgSelected == 3, "select owned failed");
    }

    public static void Rejections()
    {
        var pd = new PlayerData { gold = 1_000 };
        Check(BattleBackgroundRules.Buy(pd, 5, Wallet(pd)) == BattleBackgroundResult.NotEnoughGold, "bought without gold");
        Check(pd.gold == 1_000 && !BattleBackgroundRules.Owns(pd, 5), "failed buy changed state");
        Check(BattleBackgroundRules.Buy(pd, 0, Wallet(pd)) == BattleBackgroundResult.InvalidId, "bought default scene");
        Check(BattleBackgroundRules.Buy(pd, 99, Wallet(pd)) == BattleBackgroundResult.InvalidId, "bought unknown id");
        Check(BattleBackgroundRules.Buy(pd, -1, Wallet(pd)) == BattleBackgroundResult.InvalidId, "bought negative id");
        Check(BattleBackgroundRules.Select(pd, 2) == BattleBackgroundResult.NotOwned, "selected unowned scene");
        Check(BattleBackgroundRules.Select(pd, 99) == BattleBackgroundResult.InvalidId, "selected unknown id");
        pd.BattleBgSelected = 4; // cột DB lệch: chọn cảnh chưa sở hữu
        Check(BattleBackgroundRules.EffectiveSelected(pd) == 0, "unowned stored selection leaked");
        pd.BattleBgOwned = null; // cột NULL từ DB cũ
        Check(!BattleBackgroundRules.Owns(pd, 2) && BattleBackgroundRules.Owns(pd, 0), "null owned list crashed");
    }

    public static void ConcurrentBuyChargesOnce()
    {
        var pd = new PlayerData { gold = 1_000_000 };
        int spends = 0;
        var wallet = Wallet(pd, () => Interlocked.Increment(ref spends));
        Parallel.For(0, 32, _ => BattleBackgroundRules.Buy(pd, 1, wallet));
        Check(spends == 1, $"concurrent buys charged {spends} times");
        Check(pd.gold == 1_000_000 - BattleBackgroundCatalog.Find(1).PriceGold, "wrong gold after concurrent buys");
    }
}
