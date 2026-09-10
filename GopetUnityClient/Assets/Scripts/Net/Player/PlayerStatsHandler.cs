using System;

namespace Gopet.Net.Player
{
    /// <summary>
    /// Nhận 3 sub-command của <c>PET_SERVICE</c> mang stats nhân vật, gộp state,
    /// bắn event mỗi lần đổi.
    ///
    /// <para>Wire khớp <c>GameController.messagePetService(...)</c> (server):
    /// xem <c>GameController.cs:1618-1654</c>.</para>
    ///
    /// <para>Handler thuần C# — test được ngoài Unity, cùng khuôn với
    /// <see cref="Gopet.Net.Map.MapHandler"/> và <see cref="Gopet.Net.Chat.ChatHandler"/>.</para>
    /// </summary>
    public sealed class PlayerStatsHandler
    {
        private readonly PlayerStats _state = new PlayerStats();

        /// <summary>Bắn mỗi lần state thay đổi; đối số là snapshot bất biến (copy).</summary>
        public event Action<PlayerStats> StatsUpdated;

        /// <summary>Snapshot mới nhất, đọc để hiển thị lần đầu trước khi có event.</summary>
        public PlayerStats Snapshot => _state.Copy();

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));

            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.MONEY_INFO, OnMoney);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.STAR_INFO, OnStar);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.ENERGY_INFO, OnEnergy);
        }

        /// <summary>Wire: int star, long gold, long coin, long lua, int count, [UTF iconPath, long count]*.</summary>
        private void OnMoney(Message message)
        {
            var r = message.Reader;
            _state.Star = r.ReadInt();
            _state.Gold = r.ReadLong();
            _state.Coin = r.ReadLong();
            _state.Lua = r.ReadLong();
            var extraCount = r.ReadInt();
            if (extraCount < 0 || extraCount > 128)
                throw new ProtocolException($"MONEY_INFO Extras count không hợp lệ: {extraCount}.");
            var extras = new MoneyDisplay[extraCount];
            for (var i = 0; i < extraCount; i++)
            {
                var iconPath = r.ReadUtf();
                var count = r.ReadLong();
                extras[i] = new MoneyDisplay(iconPath, count);
            }
            r.ExpectFullyConsumed("MONEY_INFO");
            _state.Extras = extras;
            Emit();
        }

        /// <summary>Wire: int star.</summary>
        private void OnStar(Message message)
        {
            var r = message.Reader;
            _state.Star = r.ReadInt();
            r.ExpectFullyConsumed("STAR_INFO");
            Emit();
        }

        /// <summary>Wire: int star, int, int, int (3 int sau server đẩy 0 — placeholder).</summary>
        private void OnEnergy(Message message)
        {
            var r = message.Reader;
            _state.Star = r.ReadInt();
            r.ReadInt(); // reserved 1
            r.ReadInt(); // reserved 2
            r.ReadInt(); // reserved 3
            r.ExpectFullyConsumed("ENERGY_INFO");
            Emit();
        }

        private void Emit()
        {
            _state.Version++;
            StatsUpdated?.Invoke(_state.Copy());
        }
    }
}
