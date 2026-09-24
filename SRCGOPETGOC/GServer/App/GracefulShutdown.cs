using System.Runtime.InteropServices;
using Gopet.Manager;
using Gopet.Util;

namespace Gopet.App
{
    /// <summary>
    /// Lưu dữ liệu khi tiến trình bị yêu cầu dừng từ bên ngoài: <c>docker stop</c>,
    /// <c>systemctl stop</c> (SIGTERM) hay Ctrl+C (SIGINT).
    ///
    /// <para>Trước đây chỉ lệnh console <c>shutdown</c> mới lưu. Tín hiệu dừng giết tiến
    /// trình ngay, mất market, clan và dữ liệu người chơi đang online kể từ lần lưu gần
    /// nhất — chạy trong container là mỗi lần cập nhật đều mất.</para>
    ///
    /// <para>Handler huỷ hành vi mặc định (thoát ngay), chạy <see cref="Main.shutdown"/>
    /// rồi mới thoát. Docker chờ <c>stop_grace_period</c> trước khi SIGKILL, nên việc lưu
    /// phải xong trong khoảng đó.</para>
    /// </summary>
    public static class GracefulShutdown
    {
        // Phải giữ tham chiếu: registration bị GC thu hồi là handler biến mất.
        private static readonly List<PosixSignalRegistration> Registrations = new();

        public static void Register()
        {
            foreach (var signal in new[] { PosixSignal.SIGTERM, PosixSignal.SIGINT })
            {
                try
                {
                    Registrations.Add(PosixSignalRegistration.Create(signal, OnSignal));
                }
                catch (PlatformNotSupportedException)
                {
                    // Nền tảng không hỗ trợ tín hiệu này: giữ hành vi cũ, không chặn khởi động.
                }
            }
        }

        private static void OnSignal(PosixSignalContext context)
        {
            // Không để runtime tự thoát trước khi lưu xong.
            context.Cancel = true;
            GopetManager.ServerMonitor.LogWarning($"Nhận {context.Signal}: đang lưu dữ liệu trước khi dừng máy chủ...");
            try
            {
                Main.shutdown();
                GopetManager.ServerMonitor.LogWarning("Đã lưu xong, thoát tiến trình.");
            }
            catch (Exception e)
            {
                e.printStackTrace();
            }
            Gopet.Logging.Monitor.Flush(TimeSpan.FromSeconds(3));
            Environment.Exit(0);
        }
    }
}
