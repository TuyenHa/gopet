using Gopet.Net.Map;
using Gopet.Net.Pet;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Dàn cảnh hôn / chơi với / xoa đầu pet khi server phát <c>ON_PET_INTERACT</c> — bám theo
    /// <c>bi.java</c> của jar.
    ///
    /// <para>Ở lại <see cref="GameSession"/> chứ không nằm trong <c>MapScene</c> vì cảnh này
    /// cần cả ba thứ: pet (<c>PetLayer</c>), người chơi (<c>MapScene</c>) và quyền khoá điều
    /// khiển (<c>MovementController</c>) — chỉ phiên chơi mới giữ đủ.</para>
    /// </summary>
    public sealed partial class GameSession
    {
        /// <summary>Hôn và xoa đầu giữ 2 giây (<c>bi.java</c> case 0 và 2).</summary>
        private const float PetInteractShortSeconds = 2f;

        /// <summary>Chơi với pet giữ 8 giây (<c>bi.java</c> case 1) — đủ lâu cho màn pet chạy
        /// tới đứng cạnh rồi về chỗ.</summary>
        private const float PetInteractPlaySeconds = 8f;

        /// <summary>Jar đặt điểm neo hiệu ứng lệch khỏi pet đúng ngần này (pet.i + 5, pet.j + 1).</summary>
        private const float PetInteractAnchorX = 5f;
        private const float PetInteractAnchorY = 1f;

        /// <summary>Hướng "tây" của <see cref="MovementController.FaceDirection"/> — pet vào
        /// thế đứng ở BÊN TRÁI nên người chơi phải quay sang trái mới nhìn vào nó.</summary>
        private const int FaceWest = 1;

        /// <summary>Icon phải nằm TRÊN cả pet lẫn nhãn tên của nó mới đọc được.</summary>
        private const int EffectLayersAbovePet = 25;

        private void OnPetInteraction(PetInteraction evt)
        {
            // Không có pet thì không diễn gì cả — jar cũng bỏ qua gói (fr.java:732 đòi
            // player.pet != null). Hiện icon lơ lửng cạnh người không pet là sai cảnh.
            var pet = _petLayer != null ? _petLayer.PetOf(evt.UserId) : null;
            if (pet == null) return;
            var owner = _scene != null ? _scene.TryGetAvatarTransform(evt.UserId) : null;

            var isPlay = evt.Type == PetActionPackets.Play;
            var seconds = isPlay ? PetInteractPlaySeconds : PetInteractShortSeconds;
            var isSelf = evt.UserId == _login.UserId;

            if (isPlay) pet.PlayInteractPose(seconds);

            if (isSelf && _movement != null)
            {
                // Jar cấm đi lại suốt lúc diễn (bi.java bật/tắt cờ di chuyển của chính mình).
                _movement.LockInput(seconds);
                if (isPlay) _movement.FaceDirection(FaceWest);
            }

            PetInteractionEffect.Attach(_scene.transform, AnchorFor(evt.Type, owner, pet),
                evt.Type, pet.SortingOrder + EffectLayersAbovePet, seconds);
        }

        /// <summary>
        /// Điểm neo hiệu ứng, đã cộng sẵn phần nâng của jar. Trục Y của jar hướng XUỐNG còn
        /// world hướng LÊN, nên mọi độ lệch dọc đều đổi dấu khi chuyển sang world.
        /// </summary>
        private static Vector3 AnchorFor(int type, Transform owner, PetAvatar pet)
        {
            var lift = PetInteractionEffect.LiftOf(type);

            // Chơi với pet: jar đặt icon vào GIỮA người và pet, sau khi đã dời pet sang cạnh.
            if (type == PetActionPackets.Play && owner != null)
            {
                var spot = PetAvatar.InteractPoseSpot(owner.position);
                return new Vector3((spot.x + owner.position.x) * 0.5f,
                                   spot.y - PetInteractAnchorY + lift, owner.position.z);
            }

            var basePos = pet.transform.position;
            return new Vector3(basePos.x + PetInteractAnchorX,
                               basePos.y - PetInteractAnchorY + lift, basePos.z);
        }
    }
}
