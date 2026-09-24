using Gopet.IO;
using Gopet.Util;

namespace Gopet.Data.BattleBackground
{
    /// <summary>
    /// Nối luật khung cảnh với người chơi thật: gói STATE, trừ vàng, thông báo.
    /// Gói đi qua COMMAND_GUIDER (sub 43..46), xem docs/battle-system.md mục khung cảnh.
    /// </summary>
    public static class BattleBackgroundService
    {
        /// <summary>
        /// Layout: selectedId(sbyte), n(sbyte), lặp n: id(sbyte) + name(utf) + priceGold(long) + owned(bool).
        /// </summary>
        public static void SendState(Player player)
        {
            var pd = player.playerData;
            Message m = new Message(GopetCMD.COMMAND_GUIDER);
            m.putsbyte(GopetCMD.TYPE_BATTLE_BG_STATE);
            m.putsbyte(BattleBackgroundRules.EffectiveSelected(pd));
            m.putsbyte((sbyte)BattleBackgroundCatalog.All.Count);
            foreach (var def in BattleBackgroundCatalog.All)
            {
                m.putsbyte(def.Id);
                m.putUTF(def.Name);
                m.putlong(def.PriceGold);
                m.putbool(BattleBackgroundRules.Owns(pd, def.Id));
            }
            m.cleanup();
            player.session.sendMessage(m);
        }

        public static void Buy(Player player, int id)
        {
            var result = BattleBackgroundRules.Buy(player.playerData, id, price =>
            {
                if (!player.checkGold(price)) return false;
                player.mineGold(price);
                return true;
            });
            switch (result)
            {
                case BattleBackgroundResult.Ok:
                    player.okDialog(Utilities.Format("Đã mua khung cảnh %s", BattleBackgroundCatalog.Find(id).Name));
                    break;
                case BattleBackgroundResult.NotEnoughGold:
                    player.controller.notEnoughGold();
                    break;
            }
            // Luôn gửi lại STATE để client mở khoá nút, kể cả khi bị từ chối.
            SendState(player);
        }

        public static void Select(Player player, int id)
        {
            BattleBackgroundRules.Select(player.playerData, id);
            SendState(player);
        }
    }
}
