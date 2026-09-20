using System;
using System.Linq;
using Gopet.Net;
using Gopet.Net.Map;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// TELE_MENU phải liệt kê TOÀN BỘ map dịch chuyển kèm cờ khoá, thay vì giấu map
    /// thượng giới của người chưa mở (<c>GameController.mapTeleMenu</c>).
    ///
    /// <para>Đây là check bắt lệch wire-format: server ghi thêm một sbyte cờ khoá sau
    /// waypoint, client đọc thêm đúng một sbyte. Lệch một byte thì
    /// <c>ExpectFullyConsumed</c> ném ngay tại đây chứ không đợi tới lúc chơi thật.</para>
    /// </summary>
    internal static class TeleportMenuChecks
    {
        /// <summary>Map thượng giới — khoá với tài khoản chưa có pet trùng sinh (CheckSky).</summary>
        private const int FirstSkyMapId = 26;

        public static void Run(GopetSocket socket, MessageRouter router)
        {
            MapTeleportOption[] options = null;
            var handler = new MapTeleportHandler(socket.Send);
            handler.OptionsReceived += value => options = value;
            handler.RegisterOn(router);

            try
            {
                handler.RequestOptions();
                MessagePump.Until(socket, router, () => options != null, TimeSpan.FromSeconds(5));
                Verify(options);
            }
            finally
            {
                // Trả router về nguyên trạng: handler này ném với MỌI sub MGO_COMMAND
                // khác 12, để treo lại thì check sau vô tình nhận gói MGO sẽ đỏ vì lý
                // do không liên quan.
                router.Unregister(GopetCmd.MGO_COMMAND);
            }
        }

        private static void Verify(MapTeleportOption[] options)
        {
            Report.Check("NN. TELE_MENU — nhận được danh sách map",
                options != null && options.Length > 0,
                options == null ? "không nhận được MGO_COMMAND/TELE_MENU trong 5s" : "danh sách rỗng");
            if (options == null || options.Length == 0) return;

            Console.WriteLine("       -> " + string.Join(", ",
                options.Select(o => $"{o.MapId}:{o.Name}{(o.Locked ? $" [khoá: {o.LockReason}]" : "")}")));

            Report.Check("OO. TELE_MENU — map thượng giới vẫn có mặt trong danh sách",
                options.Any(o => o.MapId >= FirstSkyMapId),
                "server vẫn lọc bỏ map >= 26, client không thể vẽ icon ổ khoá");

            // Map 13/15 nằm ngoài danh sách tuyển TeleMapId cũ — có mặt tức là server
            // đã liệt kê MỌI map đang bật chứ không phải 8 map chọn sẵn.
            var beyondLegacyList = new[] { 13, 15 };
            Report.Check("OO2. TELE_MENU — liệt kê mọi map, không chỉ danh sách tuyển cũ",
                beyondLegacyList.All(id => options.Any(o => o.MapId == id)),
                $"thiếu map ngoài TeleMapId: {string.Join(", ", beyondLegacyList.Where(id => options.All(o => o.MapId != id)))}");

            // Tài khoản smoke chưa mở thượng giới nên map >= 26 phải khoá hết. Nếu tài
            // khoản này về sau được mở thì kỳ vọng đảo chiều: không map nào bị khoá —
            // suy ra từ chính dữ liệu thay vì bắt cứng, để check không đỏ oan.
            var skyUnlocked = options.Any(o => o.MapId >= FirstSkyMapId && !o.Locked);
            Func<MapTeleportOption, bool> expected =
                o => !skyUnlocked && o.MapId >= FirstSkyMapId;
            Report.Check("PP. TELE_MENU — cờ khoá bám đúng luật CheckSky",
                options.All(o => o.Locked == expected(o)),
                "cờ khoá lệch: " + string.Join(", ",
                    options.Where(o => o.Locked != expected(o))
                           .Select(o => $"{o.MapId} locked={o.Locked}")));

            // Cờ khoá và lý do phải đi cặp: client dùng chính chuỗi này làm thông báo khi
            // người chơi bấm map khoá, khoá mà lý do rỗng thì toast hiện trống trơn.
            Report.Check("RR. TELE_MENU — lý do khoá đi kèm cờ khoá",
                options.All(o => o.Locked == !string.IsNullOrEmpty(o.LockReason)),
                "lệch cặp cờ/lý do: " + string.Join(", ",
                    options.Where(o => o.Locked == string.IsNullOrEmpty(o.LockReason))
                           .Select(o => $"{o.MapId} locked={o.Locked} reason='{o.LockReason}'")));

            Report.Check("QQ. TELE_MENU — không có map trùng lặp",
                options.Select(o => o.MapId).Distinct().Count() == options.Length,
                "TeleMapId chứa map lặp mà server chưa lọc");
        }
    }
}
