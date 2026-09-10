using System;
using Gopet.Net;
using Gopet.Net.Map;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Đóng ba tiêu chí end-to-end của P6:
    /// T. INIT_PLAYER (31) đến sau LOGIN_SUCCES, đúng user_id.
    /// U. ON_UPDATE_PLAYER_IN_MAP (29) mang toạ độ spawn của self hợp lệ.
    /// V. ON_OTHER_USER_MOVE (27) — client gửi đích, server broadcast lại đúng cặp cuối.
    ///
    /// Server đẩy INIT_PLAYER + MAP_UPDATE NGAY sau loginOK, TRƯỚC khi client kịp
    /// gửi request nào. Nên MapHandler phải được đăng ký ngay khi mở router — nếu
    /// đợi đến sau ImageChecks/GuiderChecks thì gói đã bị router bỏ (không handler).
    ///
    /// <see cref="Recorder"/> giữ dữ liệu, <see cref="Register"/> lắp handler sớm,
    /// <see cref="Verify"/> chấm điểm ở cuối.
    /// </summary>
    internal static class MapMovementChecks
    {
        internal sealed class Recorder
        {
            public PlayerInit Init;
            public MapUpdate Update;
            public PlayerMoved Echo;
            public MapHandler Handler;
            public int ExpectedUserId;
        }

        public static Recorder Register(GopetSocket socket, MessageRouter router)
        {
            var rec = new Recorder { Handler = new MapHandler(socket.Send) };
            rec.Handler.PlayerInitReceived += e => rec.Init = e;
            rec.Handler.MapUpdated += e => rec.Update = e;
            rec.Handler.PlayerMoved += e =>
            {
                // Server broadcast mọi move trong khu vực, kể cả self. Chỉ giữ echo cho user mình.
                if (e.UserId == rec.ExpectedUserId) rec.Echo = e;
            };
            rec.Handler.RegisterOn(router);
            return rec;
        }

        public static void Verify(GopetSocket socket, MessageRouter router, Recorder rec, int expectedUserId)
        {
            rec.ExpectedUserId = expectedUserId;

            // Server gửi INIT_PLAYER + ENTER_MAP + MAP_UPDATE ngay sau loginOK — 5s là dư,
            // nhưng phần lớn đã tới trước khi Verify chạy (đã bắt lúc pump ở các check trước).
            MessagePump.Until(socket, router, () => rec.Init != null && rec.Update != null,
                              TimeSpan.FromSeconds(5));

            Report.Check("T. INIT_PLAYER đến sau LOGIN_SUCCES, đúng user",
                rec.Init != null && rec.Init.UserId == expectedUserId,
                rec.Init == null ? "không nhận được INIT_PLAYER trong 5s"
                                 : $"user_id nhận được {rec.Init.UserId}, kỳ vọng {expectedUserId}");

            if (rec.Init != null)
            {
                Console.WriteLine($"       -> INIT_PLAYER #{rec.Init.UserId} \"{rec.Init.Name}\" gender={rec.Init.Gender}");
            }

            Report.Check("U. ON_UPDATE_PLAYER_IN_MAP mang toạ độ spawn của self",
                rec.Update != null && rec.Update.MapId > 0 && rec.Update.SelfX >= 0 && rec.Update.SelfY >= 0,
                rec.Update == null
                    ? "không nhận được MAP_UPDATE trong 5s"
                    : $"mapId={rec.Update.MapId} selfX={rec.Update.SelfX} selfY={rec.Update.SelfY} — trông không hợp lệ");

            if (rec.Update == null || rec.Init == null) return;

            Console.WriteLine($"       -> MAP_UPDATE map={rec.Update.MapId} zone={rec.Update.ZoneId} " +
                              $"self=({rec.Update.SelfX},{rec.Update.SelfY}) waypoint={rec.Update.SelfWaypointIndex} " +
                              $"others={rec.Update.Others.Length}");

            // Gửi move: đi 24px về phải (1 tile). direction 3 = phải trong bảng jar.
            const int direction = 3;
            var destX = rec.Update.SelfX + 24;
            var destY = rec.Update.SelfY;
            var points = new[] { destX, destY };
            rec.Echo = null;

            rec.Handler.SendMove(expectedUserId, direction, rec.Update.MapId, points);

            // Server phát lại trong dưới 2s (không có rate limit khi mảng ngắn).
            MessagePump.Until(socket, router, () => rec.Echo != null, TimeSpan.FromSeconds(3));

            var echoOk = rec.Echo != null && rec.Echo.X == destX && rec.Echo.Y == destY && rec.Echo.Direction == direction;
            Report.Check("V. ON_OTHER_USER_MOVE — server broadcast lại đúng cặp (x,y) cuối",
                echoOk,
                rec.Echo == null
                    ? $"không nhận được echo trong 3s (gửi đích ({destX},{destY}))"
                    : $"echo ({rec.Echo.X},{rec.Echo.Y}) dir={rec.Echo.Direction} != gửi ({destX},{destY}) dir={direction}");

            if (rec.Echo != null)
            {
                Console.WriteLine($"       -> echo user=#{rec.Echo.UserId} dir={rec.Echo.Direction} " +
                                  $"đích=({rec.Echo.X},{rec.Echo.Y}) count={rec.Echo.Points.Length}");
            }
        }
    }
}
