using System;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Quái đi lảng vảng quanh chỗ nó sinh ra: chọn một điểm gần đó, đi tới, đứng nghỉ
    /// một lát rồi chọn điểm khác.
    ///
    /// <para><b>Chỉ là chuyển động bề ngoài của client.</b> Server chốt vị trí quái đúng
    /// một lần lúc spawn (<c>SEND_LIST_MOB_ZONE</c>) và không có gói nào báo quái đi
    /// đâu, nên mỗi máy tự thấy quái ở một chỗ hơi khác nhau. Không ảnh hưởng gì đến
    /// đánh nhau: <c>ATTACK_MOB</c> gửi mobId, server không kiểm tra khoảng cách.</para>
    ///
    /// <para>Thuần C#, không UnityEngine — test được ngoài Unity.</para>
    /// </summary>
    public sealed class MobWander
    {
        /// <summary>
        /// Bán kính lảng vảng quanh chỗ sinh, pixel jar — 4 ô tile (24px/ô). Rộng nữa
        /// thì quái lạc hẳn khỏi khu của nó và người chơi phải dò tìm con định đánh.
        /// </summary>
        public const float Radius = 96f;

        /// <summary>
        /// Tốc độ đi, pixel/giây — hơn nửa tốc độ người chơi một chút
        /// (<c>MovementController.DefaultWalkSpeedPxPerSec</c> = 96). Vẫn phải chậm hơn
        /// người chơi, không thì quái trông như đang đuổi theo.
        /// </summary>
        public const float Speed = 50f;

        private const float MinPause = 1.2f;
        private const float MaxPause = 3.6f;

        /// <summary>Thử tối đa bấy nhiêu điểm; bí chỗ đi thì đứng yên thêm một nhịp.</summary>
        private const int TargetAttempts = 6;

        private readonly int _homeX;
        private readonly int _homeY;
        private readonly Random _random;

        private float _targetX;
        private float _targetY;
        private float _pauseLeft;

        public MobWander(int homeX, int homeY, int seed)
        {
            _homeX = homeX;
            _homeY = homeY;
            _random = new Random(seed);
            X = homeX;
            Y = homeY;
            _targetX = homeX;
            _targetY = homeY;
            // Lệch pha sẵn: cả đàn quái khởi động cùng lúc mà cùng nhịp thì nhìn như duyệt binh.
            _pauseLeft = (float)_random.NextDouble() * MaxPause;
        }

        public float X { get; private set; }
        public float Y { get; private set; }

        /// <summary>Đang đứng nghỉ (không di chuyển) hay không.</summary>
        public bool Resting => _pauseLeft > 0f;

        /// <param name="canStand">Điểm (x, y) có đứng được không — dùng mặt nạ va chạm của map.</param>
        public void Tick(float deltaSeconds, Func<int, int, bool> canStand)
        {
            if (canStand == null) throw new ArgumentNullException(nameof(canStand));
            if (deltaSeconds <= 0f) return;

            if (_pauseLeft > 0f)
            {
                _pauseLeft -= deltaSeconds;
                if (_pauseLeft <= 0f) PickTarget(canStand);
                return;
            }

            var dx = _targetX - X;
            var dy = _targetY - Y;
            var distance = (float)Math.Sqrt(dx * dx + dy * dy);
            var step = Speed * deltaSeconds;
            if (distance <= step)
            {
                MoveTo(_targetX, _targetY, canStand);
                _pauseLeft = MinPause + (float)_random.NextDouble() * (MaxPause - MinPause);
                return;
            }

            MoveTo(X + dx / distance * step, Y + dy / distance * step, canStand);
        }

        /// <summary>Đi vào chỗ không đứng được thì bỏ đích, nghỉ rồi chọn đích khác.</summary>
        private void MoveTo(float x, float y, Func<int, int, bool> canStand)
        {
            if (!canStand((int)Math.Round(x), (int)Math.Round(y)))
            {
                _pauseLeft = MinPause;
                return;
            }
            X = x;
            Y = y;
        }

        private void PickTarget(Func<int, int, bool> canStand)
        {
            for (var attempt = 0; attempt < TargetAttempts; attempt++)
            {
                var angle = _random.NextDouble() * Math.PI * 2.0;
                var reach = _random.NextDouble() * Radius;
                var x = _homeX + (float)(Math.Cos(angle) * reach);
                var y = _homeY + (float)(Math.Sin(angle) * reach);
                if (!canStand((int)Math.Round(x), (int)Math.Round(y))) continue;
                _targetX = x;
                _targetY = y;
                return;
            }

            _targetX = X;
            _targetY = Y;
            _pauseLeft = MinPause;
        }
    }
}
