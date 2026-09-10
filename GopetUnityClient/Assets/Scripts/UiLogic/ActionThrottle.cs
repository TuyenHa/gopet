using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gopet.UiLogic
{
    /// <summary>Chặn gửi lặp thao tác đắt tiền trong một cửa sổ thời gian ngắn.</summary>
    public sealed class ActionThrottle
    {
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        private readonly Func<long> _nowMs;
        private readonly Dictionary<string, long> _nextAllowed = new Dictionary<string, long>();

        public ActionThrottle(Func<long> nowMs = null)
        {
            _nowMs = nowMs ?? (() => Clock.ElapsedMilliseconds);
        }

        public bool TryAcquire(string key, int cooldownMs, out int remainingMs)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Cần khóa throttle.", nameof(key));
            if (cooldownMs < 0) throw new ArgumentOutOfRangeException(nameof(cooldownMs));

            var now = _nowMs();
            if (_nextAllowed.TryGetValue(key, out var due) && now < due)
            {
                remainingMs = (int)Math.Min(int.MaxValue, due - now);
                return false;
            }

            _nextAllowed[key] = now + cooldownMs;
            remainingMs = 0;
            return true;
        }
    }
}
