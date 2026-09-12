using System;
using Gopet.Net;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Chứng thực <c>MarketPlace</c> (map 22, "Chợ trời") — mục 11 của
    /// <c>plans/260905-1355-gopet-unity-client-rebuild/phase-08-long-tail-features-outline.md</c>.
    ///
    /// Entry là <c>ON_PLAYER_WARPING</c> thuần (server ném <c>UnsupportedOperationException</c>
    /// nếu ai đó tạo <c>MarketPlace</c> với map khác 22, nên tới được map này = đúng chỗ).
    /// Tương tác kiosk (mua/bán/xem sạp) đã có check riêng (<c>ShopChecks</c> cho
    /// <c>REQUEST_SHOP</c>) — check này chỉ chứng minh ĐƯỜNG VÀO, không lặp lại phần đó.
    /// </summary>
    internal static class MarketPlaceChecks
    {
        private const int OriginMapId = 11;
        private const int MarketMapId = 22;

        public static void Run(GopetSocket socket, MessageRouter router, MapMovementChecks.Recorder rec)
        {
            if (rec.Update == null || rec.Update.MapId != OriginMapId)
            {
                Report.Check("Z5. MarketPlace — điều kiện tiên quyết (đang ở map 11)",
                    false, $"map hiện tại = {rec.Update?.MapId.ToString() ?? "?"}, cần map {OriginMapId} để test");
                return;
            }

            rec.Update = null;
            rec.Handler.SendWarp(MarketMapId, 0);
            MessagePump.Until(socket, router, () => rec.Update != null && rec.Update.MapId == MarketMapId,
                              TimeSpan.FromSeconds(5));

            var entered = rec.Update != null && rec.Update.MapId == MarketMapId;
            Report.Check("Z5. ON_PLAYER_WARPING — vào MarketPlace (map 22, Chợ trời)",
                entered,
                rec.Update == null ? "không nhận được MAP_UPDATE trong 5s"
                                   : $"MAP_UPDATE trả mapId={rec.Update.MapId}, kỳ vọng {MarketMapId}");

            if (!entered) return;

            Console.WriteLine($"       -> vào map {rec.Update.MapId}, self=({rec.Update.SelfX},{rec.Update.SelfY})");

            rec.Update = null;
            rec.Handler.SendWarp(OriginMapId, 0);
            MessagePump.Until(socket, router, () => rec.Update != null && rec.Update.MapId == OriginMapId,
                              TimeSpan.FromSeconds(5));

            Report.Check("Z6. Thoát MarketPlace — warp về map gốc",
                rec.Update != null && rec.Update.MapId == OriginMapId,
                rec.Update == null ? "không nhận được MAP_UPDATE trong 5s"
                                   : $"MAP_UPDATE trả mapId={rec.Update.MapId}, kỳ vọng {OriginMapId}");
        }
    }
}
