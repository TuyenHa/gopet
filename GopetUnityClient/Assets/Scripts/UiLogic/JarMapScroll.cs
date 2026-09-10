using System;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Camera ngang của nền map: trôi tới mép rồi quay đầu, mãi mãi. Thuần C# nên
    /// test được cả hai lần đảo chiều trong một phần nghìn giây, thay vì phải chờ
    /// gần chín giây thật trong PlayMode.
    ///
    /// <para>Bản jar (<c>fb.c_()</c>) cộng <c>this.h = 2</c> pixel MỖI NHỊP GAME và
    /// đảo dấu <c>h</c> khi chạm mép. Ở đây tính theo GIÂY: Unity chạy 60 fps còn máy
    /// J2ME khoảng 15, cộng theo frame thì cảnh trôi nhanh gấp bốn và còn đổi tốc độ
    /// theo máy khoẻ/yếu.</para>
    /// </summary>
    public sealed class JarMapScroll
    {
        private readonly float _max;
        private float _direction = 1f;

        /// <param name="max">Quãng cuộn tối đa = bề ngang map trừ bề ngang khung nhìn. Âm bị kẹp về 0 (map hẹp hơn khung thì đứng yên).</param>
        public JarMapScroll(float max)
        {
            _max = Math.Max(0f, max);
        }

        public float X { get; private set; }

        public float Max => _max;

        /// <summary>+1 đang đi sang phải, -1 đang quay về trái.</summary>
        public float Direction => _direction;

        /// <summary>
        /// Đi tiếp <paramref name="deltaSeconds"/> giây. KẸP vào mép trước khi đảo
        /// chiều: cộng dồn quá mép rồi mới đổi dấu sẽ để camera lấn ra ngoài map một
        /// khoảng bằng tốc độ × delta, và khoảng đó lộ ra viền trống ở rìa màn hình.
        /// </summary>
        public void Advance(float deltaSeconds, float pixelsPerSecond)
        {
            if (_max <= 0f) return;

            X += _direction * pixelsPerSecond * deltaSeconds;

            if (X < 0f)
            {
                X = 0f;
                _direction = 1f;
            }
            else if (X > _max)
            {
                X = _max;
                _direction = -1f;
            }
        }
    }
}
