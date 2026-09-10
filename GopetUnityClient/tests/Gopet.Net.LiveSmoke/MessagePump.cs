using System;
using System.Diagnostics;
using System.Threading;
using Gopet.Net;

namespace Gopet.Net.LiveSmoke
{
    /// <summary>
    /// Rút hàng đợi và dispatch trên luồng gọi — mô phỏng đúng
    /// <c>GopetClient.Update()</c> của Unity.
    /// </summary>
    internal static class MessagePump
    {
        /// <summary>
        /// Bơm cho tới khi <paramref name="until"/> đúng, hết hạn, hoặc mất kết nối.
        /// Trả <c>true</c> nếu dừng vì điều kiện đã đạt.
        /// </summary>
        /// <param name="onTick">
        /// Gọi mỗi vòng, sau khi dispatch. Đây là chỗ <c>AuthHandler.Tick()</c> chạy —
        /// quên nó thì nhịp CHECK_SPEED không bao giờ được trả lời.
        /// </param>
        public static bool Until(GopetSocket socket, MessageRouter router, Func<bool> until,
                                 TimeSpan timeout, Action onTick = null)
        {
            var clock = Stopwatch.StartNew();
            while (clock.Elapsed < timeout)
            {
                // Rút hết trước rồi mới xét điều kiện: gói cuối server gửi ngay
                // trước khi đóng vẫn nằm trong hàng đợi, xét ngược lại là mất nó.
                while (socket.Incoming.TryDequeue(out var message))
                {
                    router.Dispatch(message);
                }

                onTick?.Invoke();

                if (until()) return true;
                if (!socket.IsConnected) return false;

                Thread.Sleep(20);
            }

            return until();
        }
    }
}
