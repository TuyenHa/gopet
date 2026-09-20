using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Nhịp LAO TỚI của pet khi người chơi bấm nút đánh: pet chồm về phía con quái rồi tự
    /// lùi về chỗ bám đuôi chủ. Cùng ý với <c>BattleTurnAnimator</c> trong màn đánh — đòn
    /// đánh phải có người ra đòn, chỉ một vệt lửa bay ngang thì không ai ra tay cả.
    ///
    /// <para>Tách file vì <see cref="PetAvatar"/> đã sát ngưỡng 200 dòng.</para>
    /// </summary>
    public sealed partial class PetAvatar
    {
        /// <summary>Phần thời gian dành cho chiều ĐI; phần còn lại là chiều về. Đi nhanh hơn
        /// về cho ra nhịp "bật tới rồi thu người", không phải con lắc đều đều.</summary>
        private const float LungeOutRatio = 0.45f;

        /// <summary>Chỉ lao tới ngần này quãng đường tới quái rồi dừng. Lao sát quá thì pet
        /// chồng lên sprite quái, mà vệt chém lại nổ ngay chỗ đó nên rối hình.</summary>
        private const float LungeReach = 0.55f;

        private Vector3 _lungeTarget;
        private float _lungeSeconds;
        private float _lungeStart;

        private bool Lunging => _lungeSeconds > 0f;

        /// <summary>Lao một nhịp về phía <paramref name="worldTarget"/> rồi tự về chỗ.
        /// Gọi lại lúc đang lao thì nhịp mới ghi đè nhịp cũ.</summary>
        public void PlayLunge(Vector3 worldTarget, float seconds)
        {
            if (seconds <= 0f) return;
            _lungeTarget = worldTarget;
            _lungeSeconds = seconds;
            _lungeStart = Time.time;
        }

        /// <summary>Đè vị trí sau khi phần bám đuôi chủ đã chạy xong, nên lao xong là pet
        /// trở lại đúng điểm bám hiện tại — không giật về chỗ cũ dù chủ có đang đi.</summary>
        private void ApplyLunge()
        {
            if (!Lunging) return;

            var elapsed = (Time.time - _lungeStart) / _lungeSeconds;
            if (elapsed >= 1f)
            {
                _lungeSeconds = 0f;
                return;
            }

            var wave = elapsed < LungeOutRatio
                ? elapsed / LungeOutRatio
                : 1f - (elapsed - LungeOutRatio) / (1f - LungeOutRatio);
            var follow = transform.position;
            transform.position = Vector3.Lerp(follow, _lungeTarget,
                Mathf.SmoothStep(0f, 1f, wave) * LungeReach);

            // Quay mặt về phía quái trong lúc lao — FaceOwner chạy trước đó đã quay về
            // phía chủ, giữ nguyên thì pet lao đi bằng lưng.
            if (_renderer != null)
            {
                var dx = _lungeTarget.x - follow.x;
                if (Mathf.Abs(dx) >= FaceDeadZone) _renderer.flipX = dx < 0f;
            }
        }
    }
}
