using System;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Nội suy vị trí 2D bằng exponential smoothing — mượt hơn linear, không bị "khục"
    /// khi target đổi giữa chừng. Thuần C# để test khép kín.
    ///
    /// <para><b>Vì sao smoothing chứ không lerp cứng</b>: server cập nhật rời rạc mỗi
    /// ~2 giây (cooldown TIME_MOVE_SEND), giữa các mốc client vẫn tick 60fps. Linear
    /// lerp tới target trong khoảng thời gian cố định thì cần biết deadline; exponential
    /// smoothing chỉ cần <see cref="RatePerSecond"/> và luôn hội tụ về target.</para>
    /// </summary>
    public sealed class PositionInterpolator
    {
        /// <summary>Tốc độ hội tụ. 10 = trong 100 ms đã đi ~63% quãng đường tới target.</summary>
        public float RatePerSecond { get; set; } = 10f;

        public float X { get; private set; }
        public float Y { get; private set; }

        private float _targetX;
        private float _targetY;

        /// <summary>Đặt cả vị trí hiện tại và target = (x, y). Dùng khi spawn / warp.</summary>
        public void Snap(float x, float y)
        {
            X = _targetX = x;
            Y = _targetY = y;
        }

        /// <summary>Đổi target — vị trí hiện tại tự nội suy tới đó qua các lần <see cref="Tick"/>.</summary>
        public void SetTarget(float x, float y)
        {
            _targetX = x;
            _targetY = y;
        }

        /// <summary>Bước một khoảng <paramref name="deltaSeconds"/>. Cập nhật <see cref="X"/>/<see cref="Y"/>.</summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f) return;

            var t = 1f - (float)Math.Exp(-deltaSeconds * RatePerSecond);
            X += (_targetX - X) * t;
            Y += (_targetY - Y) * t;
        }

        public float TargetX => _targetX;
        public float TargetY => _targetY;
    }
}
