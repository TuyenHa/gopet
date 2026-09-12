using System;

namespace Gopet.UiLogic
{
    /// <summary>Nhịp auto đánh của JAR (el/ej): xin server tìm quái mỗi bốn giây.</summary>
    public sealed class AutoAttackLoop
    {
        private readonly Func<long> _now;
        private long _nextAt;

        public AutoAttackLoop(Func<long> now = null)
        {
            _now = now ?? (() => Environment.TickCount);
        }

        public bool Enabled { get; private set; }

        public void SetEnabled(bool value)
        {
            Enabled = value;
            _nextAt = value ? _now() : 0;
        }

        public bool ShouldRequest()
        {
            if (!Enabled) return false;
            var now = _now();
            if (now < _nextAt) return false;
            _nextAt = now + 4000;
            return true;
        }
    }
}
