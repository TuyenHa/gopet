using System;
using System.IO;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Chạy tầng Net thật với GServer thật. Mỗi check ứng với một tiêu chí trong
    /// phase-02-transport-layer.md.
    ///
    /// Thoát 0 = tất cả pass. Tắt server rồi chạy lại phải ra FAIL — nếu không,
    /// bài test này vô nghĩa.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Neo mọi đường dẫn mặc định vào vị trí file exe, KHÔNG vào thư mục hiện hành:
        /// "dotnet run" và chạy thẳng exe cho ra cwd khác nhau, đường dẫn tương đối
        /// theo cwd sẽ lúc trúng lúc trượt.
        ///
        /// Từ bin/Debug/net8.0 lên 6 cấp là gốc repo.
        /// </summary>
        private static string RepoRoot =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));

        private static int Main()
        {
            var host = Env("GOPET_HOST", "127.0.0.1");
            var port = int.TryParse(Environment.GetEnvironmentVariable("GOPET_PORT"), out var p) ? p : 19180;
            var clientDump = Path.GetFullPath(Env("GOPET_DUMP",
                Path.Combine(AppContext.BaseDirectory, "packet-dump-unity.log")));
            var serverDump = Path.GetFullPath(Env("GOPET_SERVER_DUMP",
                Path.Combine(RepoRoot, "SRCGOPETGOC", "GServer", "bin", "Debug", "net8.0", "log", "packet-dump-server.log")));

            Console.WriteLine($"Server      : {host}:{port}");
            Console.WriteLine($"Dump client : {clientDump}");
            Console.WriteLine($"Dump server : {serverDump}");
            Console.WriteLine();

            // Mỗi nhóm một try riêng: nhóm giao thức ném thì nhóm rò vẫn phải chạy.
            Report.Group("A-C. Giao thức", () => ProtocolChecks.Run(host, port, clientDump));
            Report.Group("D,F. Packet dump", () => DumpChecks.Run(clientDump, serverDump));
            Report.Group("E. Rò luồng/socket", () => LeakChecks.Run(host, port));

            // Luồng đăng nhập đầy đủ (P3). Cần tài khoản test đã kích hoạt —
            // xem tools/README.md phần "Tài khoản test".
            // Tài khoản RIÊNG cho harness. Dùng chung với tài khoản thao tác tay
            // (gopettest) thì server báo "Người chơi khác đăng nhập vào tài khoản"
            // mỗi khi emulator đang mở — test đỏ vì lý do không liên quan.
            var user = Env("GOPET_USER", "gopetsmoke");
            var pass = Env("GOPET_PASS", "abc12345");
            var hold = int.TryParse(Environment.GetEnvironmentVariable("GOPET_HOLD_SECONDS"), out var h) ? h : 40;
            Report.Group("G-L. Đăng nhập", () => new LoginChecks(host, port, user, pass, hold).Run());
            Report.Group("M. Đăng ký", () => RegisterCheck.Run(host, port));

            // PvP thật (P7 "Nghiệm thu còn lại") — 2 tài khoản RIÊNG, xem PvpBattleChecks
            // để biết vì sao không tái dùng account PvE ở trên.
            Report.Group("EE-II. PvP", () => PvpBattleChecks.Run(host, port));

            Console.WriteLine();
            if (Report.AnyFailure)
            {
                Console.WriteLine($"LIVE SMOKE THAT BAI ({Report.FailureCount} check fail)");
                return 1;
            }

            Console.WriteLine("LIVE SMOKE OK");
            return 0;
        }

        private static string Env(string name, string fallback)
        {
            var v = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(v) ? fallback : v;
        }
    }
}
