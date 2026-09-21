using Gopet.Data.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gopet.CommandLine
{
    internal class AdminCommand : BaseCommand
    {
        /// <summary>Tóm tắt nội dung thư trong danh sách hộp thư dài bấy nhiêu ký tự.</summary>
        private const int ShortContentLength = 60;

        public override string Description
        {
            get
            {
                return "Công cụ quản lý cho người quản trị máy chủ"
                        + "\n<admin> <banner> <text> để chat thế giới" +
                        "\n<admin> <location> <playerName> để lấy vị trí cụ thể" +
                        "\n<admin> <letter> <playerName|all> <admin|event> <tiêu đề>|<nội dung> để gửi thư hệ thống";
            }
        }

        public override string CommandName => "admin";

        public override void Execute(params string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "banner":
                        //PlayerManager.showBanner(args[1]);
                        GopetManager.ServerMonitor.LogWarning("Thao tác thành công");
                        break;
                    case "location":
                        Player p = PlayerManager.get(args[1]);
                        if (p != null)
                        {
                            GopetManager.ServerMonitor.LogWarning($"Map: {p.getPlace().map.mapTemplate.name} Zone {p.getPlace().zoneID} [{p.playerData.x},{p.playerData.y}]");
#if DEBUG
                            GopetManager.ServerMonitor.LogWarning($"INSERT INTO `gopet_mob_location` (`mapID`, `x`, `y`) VALUES ('{p.getPlace().map.mapID}', '{p.playerData.x}', '{p.playerData.y}');");
#endif
                        }
                        else
                        {
                            GopetManager.ServerMonitor.LogError("Người chơi đã offline");
                        }
                        break;
                    case "letter":
                        SendLetter(args);
                        break;
                }
            }
        }

        /// <summary>
        /// <c>admin letter &lt;tên|all&gt; &lt;admin|event&gt; &lt;tiêu đề&gt;|&lt;nội dung&gt;</c>
        ///
        /// <para>Tiêu đề và nội dung đều có dấu cách nên KHÔNG tách được bằng dấu cách —
        /// ghép lại phần đuôi rồi cắt theo <c>|</c> đúng một lần, để nội dung vẫn được phép
        /// chứa dấu <c>|</c>.</para>
        ///
        /// <para>Bắt hết ngoại lệ: vòng đọc lệnh của <c>CommandManager</c> mà ăn một exception
        /// là mất luôn khả năng nhận lệnh cho tới khi khởi động lại máy chủ.</para>
        /// </summary>
        private static void SendLetter(string[] args)
        {
            try
            {
                if (args.Length < 4)
                {
                    LogUsage("Thiếu tham số.");
                    return;
                }

                if (!TryParseType(args[2], out var type))
                {
                    LogUsage($"Loại thư '{args[2]}' không hợp lệ.");
                    return;
                }

                var rest = string.Join(" ", args.Skip(3));
                var separator = rest.IndexOf('|');
                if (separator <= 0 || separator == rest.Length - 1)
                {
                    LogUsage("Phải có cả tiêu đề và nội dung, ngăn nhau bằng dấu |.");
                    return;
                }

                var title = rest.Substring(0, separator).Trim();
                var content = rest.Substring(separator + 1).Trim();
                if (title.Length == 0 || content.Length == 0)
                {
                    LogUsage("Tiêu đề và nội dung không được để trống.");
                    return;
                }

                var shortContent = Summarize(content);

                if (string.Equals(args[1], "all", StringComparison.OrdinalIgnoreCase))
                {
                    var count = SystemLetterService.SendToAll(type, title, shortContent, content);
                    GopetManager.ServerMonitor.LogWarning($"Đã gửi thư cho {count} người chơi.");
                    return;
                }

                if (SystemLetterService.SendTo(args[1], type, title, shortContent, content))
                    GopetManager.ServerMonitor.LogWarning($"Đã gửi thư cho '{args[1]}'.");
                else
                    GopetManager.ServerMonitor.LogError($"Không tìm thấy người chơi '{args[1]}'.");
            }
            catch (Exception e)
            {
                GopetManager.ServerMonitor.LogError($"Lệnh gửi thư lỗi: {e.Message}");
            }
        }

        private static bool TryParseType(string name, out sbyte type)
        {
            switch (name.ToLowerInvariant())
            {
                case "admin": type = Letter.ADMIN; return true;
                case "event": type = Letter.EVENT; return true;
                default: type = 0; return false;
            }
        }

        /// <summary>Không bắt quản trị gõ riêng dòng tóm tắt — cắt từ nội dung là đủ.</summary>
        private static string Summarize(string content)
        {
            return content.Length <= ShortContentLength
                ? content
                : content.Substring(0, ShortContentLength) + "…";
        }

        private static void LogUsage(string reason)
        {
            GopetManager.ServerMonitor.LogError(
                $"{reason} Cú pháp: admin letter <tên người chơi|all> <admin|event> <tiêu đề>|<nội dung>");
        }
    }
}
