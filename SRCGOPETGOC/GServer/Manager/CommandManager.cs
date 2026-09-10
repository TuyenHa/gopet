using Gopet.CommandLine;
using Gopet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gopet.Manager
{
    public class CommandManager
    {
        public static readonly BaseCommand[] baseCommands = new BaseCommand[]
        {
            new HelpCommand(),
            new ShutdownCommand(),
            new ZoneCommand(),
            new ExecuteCommand(),
            new AdminCommand()
        };

        public static void StartReadingKeys()
        {
            while (true)
            {
                try
                {
                    string text = Console.ReadLine();

                    // ReadLine() trả null khi stdin đóng hoặc bị chuyển hướng —
                    // chạy như dịch vụ nền, trong container, hay khi output bị pipe.
                    // Trước đây chỗ này ném NullReferenceException rồi lặp lại ngay,
                    // biến thành vòng lặp bận ghi log không ngừng.
                    if (text == null)
                    {
                        GopetManager.ServerMonitor.LogWarning(
                            "Không có đầu vào bàn phím (stdin đã đóng). Ngừng đọc lệnh; máy chủ vẫn chạy bình thường.");

                        // KHÔNG return: mọi thread của máy chủ (listener, sender,
                        // reader) đều là background thread, nên Main() kết thúc là
                        // tiến trình thoát ngay. Chặn ở đây để giữ tiến trình sống.
                        Thread.Sleep(Timeout.Infinite);
                    }

                    var options = text.Split(" ");
                    if (options.Length > 0)
                    {
                        var cmd = options[0];

                        var op = new string[options.Length - 1];

                        for (int i = 1; i < options.Length; i++)
                        {
                            op[i - 1] = options[i];
                        }

                        var listCmd = baseCommands.Where(p => p.CommandName == cmd);

                        if (listCmd.Count() > 0)
                        {
                            listCmd.FirstOrDefault().Execute(op);
                        }
                        else
                        {
                            GopetManager.ServerMonitor.LogError($"Không có lệnh {cmd} \r\nDùng lệnh help <cmd> để tra cứu nhiều thông tin");
                        }
                    }
                    else
                    {
                        GopetManager.ServerMonitor.LogError("Dùng lệnh help <cmd> để tra cứu nhiều thông tin");
                    }
                }
                catch (Exception ex)
                {
                    ex.printStackTrace();
                    Thread.Sleep(3000);
                }
            }
        }
    }
}
