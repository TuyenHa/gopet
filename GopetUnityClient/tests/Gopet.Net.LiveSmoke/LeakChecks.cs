using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Gopet.Net;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Ngắt kết nối có rò luồng hay socket không.
    ///
    /// Không đếm <c>Process.Threads.Count</c>: threadpool và JIT tự co giãn theo cả
    /// hai chiều, và một luồng quá hạn join vẫn thoát ngay sau đó nên biến mất trước
    /// khi kịp đếm — phép đo đó có thể xanh đúng lúc nó sinh ra để bắt lỗi.
    /// Thay bằng <see cref="GopetSocket.DisposedCleanly"/>: tất định.
    /// </summary>
    internal static class LeakChecks
    {
        private const int Cycles = 5;

        public static void Run(string host, int port)
        {
            var ports = new int[Cycles];
            var allClean = true;

            for (var i = 0; i < Cycles; i++)
            {
                // Server chặn kết nối lại trong 2s/IP: không chờ thì 4 trong 5 chu kỳ
                // bị nó đóng ngay, và ta hoá ra đang đo dispose của socket đã chết.
                if (i > 0) { Thread.Sleep(GopetSocket.ReconnectCooldownMs + 500); }

                using var socket = new GopetSocket();
                socket.Connect(host, port);
                Thread.Sleep(100);

                // Lấy cổng từ chính socket, không dò bảng TCP của hệ điều hành:
                // trên loopback rất dễ nhặt nhầm kết nối cũ đang ở TIME_WAIT.
                ports[i] = socket.LocalEndPoint?.Port ?? 0;

                socket.Dispose();
                allClean &= socket.DisposedCleanly;
            }

            Report.Check($"E1. {Cycles} chu kỳ connect/dispose, cả hai luồng đều thoát",
                allClean, "có luồng nền không thoát trong 1s sau khi Dispose");

            Report.Check("E2a. Ghi nhận được cổng cục bộ của cả 5 chu kỳ",
                ports.All(p => p > 0), $"cổng đọc được: {string.Join(", ", ports)}");

            Thread.Sleep(500);

            // ESTABLISHED = chưa đóng. CLOSE_WAIT = phía kia đã FIN mà ta không đóng
            // — đây mới là dạng rò socket kinh điển, bỏ sót nó thì check này vô nghĩa.
            var leaked = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .Where(c => c.State == TcpState.Established || c.State == TcpState.CloseWait)
                .Select(c => c.LocalEndPoint.Port)
                .Intersect(ports.Where(p => p > 0))
                .ToArray();

            Report.Check("E2b. Không còn socket ESTABLISHED/CLOSE_WAIT sau dispose",
                leaked.Length == 0, $"còn mở ở cổng cục bộ: {string.Join(", ", leaked)}");
        }
    }
}
