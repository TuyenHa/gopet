using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Thế ĐỨNG CẠNH CHỦ lúc chơi với pet: jar dời hẳn pet sang sát người chơi rồi trả về chỗ
    /// cũ khi hết (<c>bi.java</c> case 1 — <c>pet.i = player.i - 40</c>, <c>pet.j = player.j</c>,
    /// hai bên quay mặt vào nhau, 8 giây sau khôi phục toạ độ đã cất).
    ///
    /// <para>Bên này không cất toạ độ cũ: hết thế thì <see cref="PetAvatar"/> tự bám đuôi chủ
    /// trở lại, mà chủ bị khoá di chuyển suốt lúc diễn nên điểm bám vẫn y nguyên chỗ cũ.</para>
    ///
    /// <para>Tách file cùng lý do với <c>PetAvatar.Lunge.cs</c>: file chính đã sát 200 dòng.</para>
    /// </summary>
    public sealed partial class PetAvatar
    {
        /// <summary>Pet đứng lệch sang TRÁI chủ bấy nhiêu pixel jar — trục X của world trùng
        /// trục X của jar nên dùng thẳng số âm.</summary>
        private const float InteractBesideX = -40f;

        private float _interactPoseUntil;

        private bool InteractPosing => _interactPoseUntil > 0f && Time.time < _interactPoseUntil;

        /// <summary>Ghim pet cạnh chủ trong <paramref name="seconds"/> giây.</summary>
        public void PlayInteractPose(float seconds)
        {
            if (seconds <= 0f) return;
            _interactPoseUntil = Time.time + seconds;
        }

        /// <summary>Vị trí pet SẼ đứng khi vào thế, tính trước lúc thế kịp áp — người gọi cần
        /// nó để đặt hiệu ứng vào giữa người và pet ngay trong khung hình đầu.</summary>
        public static Vector3 InteractPoseSpot(Vector3 ownerWorldPosition) =>
            new Vector3(ownerWorldPosition.x + InteractBesideX, ownerWorldPosition.y,
                        ownerWorldPosition.z);

        /// <summary>Đè lên cả phần bám đuôi lẫn phần lao tới, vì thế đứng phải giữ nguyên
        /// một chỗ cho tới lúc hết giờ.</summary>
        private void ApplyInteractPose()
        {
            if (!InteractPosing) return;
            if (_owner == null) return;

            var ownerPos = _owner.position;
            transform.position = new Vector3(ownerPos.x + InteractBesideX,
                                             ownerPos.y + _verticalOffset, ownerPos.z);
            // Chủ luôn ở BÊN PHẢI trong thế này; sprite pet vẽ sẵn quay trái nên phải lật.
            if (_renderer != null) _renderer.flipX = true;
        }
    }
}
