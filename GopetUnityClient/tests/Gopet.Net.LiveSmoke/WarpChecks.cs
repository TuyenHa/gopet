using System;
using Gopet.Net;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Đóng tiêu chí "Warp qua ≥2 map và quay lại OK" của
    /// <c>plans/260907-2210-map-portal-warp-shop-interaction/phase-04-test-parity.md</c>.
    ///
    /// Dùng đúng packet <c>MapHandler.SendWarp</c> mà <c>MapPortalView</c> gửi khi bấm cổng
    /// map 11 (mapId đích lấy từ <c>reports/decode-map11-entities.md</c>: cổng "Đại Linh Cảnh"
    /// → mapId 15). Xác nhận server trả <c>MAP_UPDATE</c> đúng mapId đích rồi warp NGƯỢC lại
    /// map 11 — chứng minh cả hai chiều, không chỉ một lần.
    /// </summary>
    internal static class WarpChecks
    {
        private const int OriginMapId = 11;
        private const int DestMapId = 15; // cổng "Đại Linh Cảnh" — eg entity 0, ExtraA=15.

        public static void Run(GopetSocket socket, MessageRouter router, MapMovementChecks.Recorder rec)
        {
            if (rec.Update == null || rec.Update.MapId != OriginMapId)
            {
                Report.Check("X. Warp — điều kiện tiên quyết (đang ở map 11)",
                    false, $"map hiện tại = {rec.Update?.MapId.ToString() ?? "?"}, cần map {OriginMapId} để test warp cổng đã biết");
                return;
            }

            rec.Update = null;
            rec.Handler.SendWarp(DestMapId, 0);
            MessagePump.Until(socket, router, () => rec.Update != null && rec.Update.MapId == DestMapId,
                              TimeSpan.FromSeconds(5));

            var wentThrough = rec.Update != null && rec.Update.MapId == DestMapId;
            Report.Check("X. ON_PLAYER_WARPING — sang map đích (11 → 15 Đại Linh Cảnh)",
                wentThrough,
                rec.Update == null
                    ? "không nhận được MAP_UPDATE trong 5s sau khi gửi warp"
                    : $"MAP_UPDATE trả mapId={rec.Update.MapId}, kỳ vọng {DestMapId}");

            if (!wentThrough) return;

            Console.WriteLine($"       -> vào map {rec.Update.MapId}, self=({rec.Update.SelfX},{rec.Update.SelfY})");

            rec.Update = null;
            rec.Handler.SendWarp(OriginMapId, 0);
            MessagePump.Until(socket, router, () => rec.Update != null && rec.Update.MapId == OriginMapId,
                              TimeSpan.FromSeconds(5));

            var cameBack = rec.Update != null && rec.Update.MapId == OriginMapId;
            Report.Check("Y. ON_PLAYER_WARPING — quay lại map gốc (15 → 11)",
                cameBack,
                rec.Update == null
                    ? "không nhận được MAP_UPDATE trong 5s sau khi warp về"
                    : $"MAP_UPDATE trả mapId={rec.Update.MapId}, kỳ vọng {OriginMapId}");

            if (cameBack)
            {
                Console.WriteLine($"       -> về map {rec.Update.MapId}, self=({rec.Update.SelfX},{rec.Update.SelfY})");
            }
        }
    }
}
