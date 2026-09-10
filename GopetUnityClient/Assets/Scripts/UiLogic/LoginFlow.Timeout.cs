using System;
using System.Diagnostics;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Đồng hồ chờ hồi âm của <see cref="LoginFlow"/>.
    ///
    /// <para>Server nhận gói rồi im lặng là chuyện có thật: sai OTP xong gửi lại
    /// <c>LOGIN</c> thì <c>Player.cs:587</c> return không một lời hồi âm. Không có
    /// đồng hồ thì màn "đang đăng nhập" đứng mãi — mà nó lại là màn KHÔNG CÓ NÚT NÀO.</para>
    /// </summary>
    public sealed partial class LoginFlow
    {
        /// <summary>Chờ server trả lời quá lâu thì thôi. Bằng mili-giây.</summary>
        public const long ReplyTimeoutMs = 20000;

        private const long NotWaiting = long.MinValue;

        private readonly Func<long> _nowMs;
        private long _waitingSinceMs = NotWaiting;

        private static readonly Stopwatch Clock = Stopwatch.StartNew();

        /// <summary>
        /// Phải gọi mỗi frame. Server nhận gói rồi im lặng là chuyện có thật — sai OTP
        /// xong gửi lại <c>LOGIN</c> thì <c>Player.cs:587</c> return không một lời hồi
        /// âm. Không có đồng hồ ở đây thì màn "đang đăng nhập" đứng mãi, mà nó lại là
        /// màn KHÔNG CÓ NÚT NÀO.
        /// </summary>
        public void Tick()
        {
            if (_waitingSinceMs == NotWaiting || _nowMs() - _waitingSinceMs < ReplyTimeoutMs) return;

            _waitingSinceMs = NotWaiting;
            Enter(LoginStage.Disconnected, "Máy chủ không trả lời. Thử lại xem sao.");
        }

        /// <summary>Bật/tắt hạn chờ hồi âm của server.</summary>
        internal void ArmTimeout(bool waiting)
        {
            _waitingSinceMs = waiting ? _nowMs() : NotWaiting;
        }
    }
}
