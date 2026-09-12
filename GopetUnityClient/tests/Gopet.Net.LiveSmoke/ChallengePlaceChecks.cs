using System;
using Gopet.Net;
using Gopet.Net.Map;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Chứng thực <c>ChallengePlace</c> (map 12, "Vượt ải") — mục 11 của
    /// <c>plans/260905-1355-gopet-unity-client-rebuild/phase-08-long-tail-features-outline.md</c>.
    ///
    /// <c>ChallengePlace.add()</c> (server) bắn NGAY 2 thứ khi vào phòng chờ:
    /// <c>sendPlaceTime</c> (TIME_PLACE, đếm ngược 60s) và <c>showBigTextEff</c>
    /// ("Phòng chờ"). Không đợi hết pha chờ 60s để wave quái xuất hiện — xác nhận
    /// ĐÚNG cơ chế vào phòng + 2 broadcast đã đủ chứng minh entry hoạt động, không
    /// cần giữ kết nối thêm 1 phút chỉ để thấy wave đầu.
    /// </summary>
    internal static class ChallengePlaceChecks
    {
        private const int OriginMapId = 11;
        private const int ChallengeMapId = 12;

        public static void Run(GopetSocket socket, MessageRouter router, MapMovementChecks.Recorder rec)
        {
            if (rec.Update == null || rec.Update.MapId != OriginMapId)
            {
                Report.Check("Z1. ChallengePlace — điều kiện tiên quyết (đang ở map 11)",
                    false, $"map hiện tại = {rec.Update?.MapId.ToString() ?? "?"}, cần map {OriginMapId} để test");
                return;
            }

            var world = new WorldStatusHandler();
            int? placeTimeSeconds = null;
            string bigText = null;
            world.PlaceTimeUpdated += s => placeTimeSeconds = s;
            world.BigTextShown += t => bigText = t;
            world.RegisterOn(router);

            rec.Update = null;
            rec.Handler.SendWarp(ChallengeMapId, 0);
            MessagePump.Until(socket, router,
                () => rec.Update != null && rec.Update.MapId == ChallengeMapId && placeTimeSeconds != null && bigText != null,
                TimeSpan.FromSeconds(5));

            var entered = rec.Update != null && rec.Update.MapId == ChallengeMapId;
            Report.Check("Z1. ON_PLAYER_WARPING — vào ChallengePlace (map 12, Vượt ải)",
                entered,
                rec.Update == null ? "không nhận được MAP_UPDATE trong 5s"
                                   : $"MAP_UPDATE trả mapId={rec.Update.MapId}, kỳ vọng {ChallengeMapId}");

            Report.Check("Z2. ChallengePlace.add() bắn TIME_PLACE đúng pha chờ (≤60s)",
                placeTimeSeconds.HasValue && placeTimeSeconds.Value > 0 && placeTimeSeconds.Value <= 60,
                placeTimeSeconds == null ? "không nhận được TIME_PLACE trong 5s"
                                         : $"TIME_PLACE={placeTimeSeconds.Value}s — ngoài khoảng (0,60]");

            Report.Check("Z3. ChallengePlace.add() bắn SHOW_BIG_TEXT_EFF phòng chờ",
                !string.IsNullOrEmpty(bigText),
                "không nhận được SHOW_BIG_TEXT_EFF trong 5s");

            if (bigText != null) Console.WriteLine($"       -> big text: \"{bigText}\"");

            if (!entered) return;

            rec.Update = null;
            rec.Handler.SendWarp(OriginMapId, 0);
            MessagePump.Until(socket, router, () => rec.Update != null && rec.Update.MapId == OriginMapId,
                              TimeSpan.FromSeconds(5));

            Report.Check("Z4. Thoát ChallengePlace — warp về map gốc",
                rec.Update != null && rec.Update.MapId == OriginMapId,
                rec.Update == null ? "không nhận được MAP_UPDATE trong 5s"
                                   : $"MAP_UPDATE trả mapId={rec.Update.MapId}, kỳ vọng {OriginMapId}");
        }
    }
}
