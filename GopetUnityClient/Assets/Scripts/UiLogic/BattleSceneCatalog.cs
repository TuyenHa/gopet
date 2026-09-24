namespace Gopet.UiLogic
{
    /// <summary>Kiểu hiệu ứng động của một khung cảnh màn đấu.</summary>
    public enum BattleAmbientKind { None, Canopy, Sakura, Snow, Cave, Fire }

    /// <summary>
    /// Ánh xạ id khung cảnh (server gửi trong gói <c>TYPE_BATTLE_BG_STATE</c>) sang ảnh nền và
    /// kiểu hiệu ứng. Id phải khớp <c>BattleBackgroundCatalog</c> phía server; id lạ (server mới
    /// hơn client) rơi về rừng mặc định thay vì để màn đấu trống nền.
    /// </summary>
    public static class BattleSceneCatalog
    {
        public const int DefaultId = 0;

        private static readonly string[] Backgrounds =
        {
            "Battle/bg-forest",
            "Battle/bg/bg-canopy",
            "Battle/bg/bg-sakura",
            "Battle/bg/bg-snow",
            "Battle/bg/bg-cave",
            "Battle/bg/bg-fire",
        };

        private static readonly BattleAmbientKind[] Ambients =
        {
            BattleAmbientKind.None,
            BattleAmbientKind.Canopy,
            BattleAmbientKind.Sakura,
            BattleAmbientKind.Snow,
            BattleAmbientKind.Cave,
            BattleAmbientKind.Fire,
        };

        public static int Count => Backgrounds.Length;

        public static int Normalize(int id) => id >= 0 && id < Backgrounds.Length ? id : DefaultId;

        /// <summary>Đường dẫn Resources của ảnh nền (không đuôi).</summary>
        public static string BackgroundPath(int id) => Backgrounds[Normalize(id)];

        public static BattleAmbientKind Ambient(int id) => Ambients[Normalize(id)];
    }
}
