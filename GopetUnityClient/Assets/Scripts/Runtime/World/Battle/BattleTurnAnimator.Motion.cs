using Gopet.Net.Battle;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Phần dàn cảnh chuyển động của <see cref="BattleTurnAnimator"/>: ai lao sang,
    /// lao bao xa, tìm đối thủ, đưa pet về chỗ. Tách khỏi phần hàng đợi để giữ file dưới
    /// ngưỡng 200 dòng.</summary>
    public sealed partial class BattleTurnAnimator
    {
        /// <summary>Điểm xuất phát cho hiệu ứng kỹ năng: phía TRÊN và LỆCH SANG BÊN mục tiêu,
        /// để nó rơi chéo chứ không cắm thẳng xuống đầu.
        ///
        /// <para>Lệch về phía pet ra đòn — đòn bay từ hướng kẻ tấn công xuống thì đọc ra câu
        /// chuyện, lệch bừa một bên thì không.</para>
        ///
        /// <para>Lấy theo kích thước canvas chứ không hằng số pixel: canvas co giãn theo màn
        /// hình nên số cứng sẽ rơi hụt ở máy cao và xuất phát ngoài khung ở máy thấp.</para></summary>
        private Vector3 FallOrigin(RectTransform target, BattlePetCard caster)
        {
            var self = transform as RectTransform;
            var height = self != null && self.rect.height > 1f ? self.rect.height : 540f;
            var width = self != null && self.rect.width > 1f ? self.rect.width : 960f;
            var side = caster == null ? -1f
                : Mathf.Sign(caster.EffectAnchor.position.x - target.position.x);
            if (side == 0f) side = -1f;
            return target.position + new Vector3(side * width * FallSideRatio,
                height * FallHeightRatio, 0f);
        }

        /// <summary>Quãng lao theo trục x, dấu cho biết hướng. 0 nghĩa là không đủ chỗ để lao.</summary>
        private float LungeTravel(BattlePetCard attacker)
        {
            var target = Other(attacker);
            if (target == null || !(transform is RectTransform self)) return 0f;
            var gap = (target.AnchorX - attacker.AnchorX) * self.rect.width;
            return Mathf.Abs(gap) <= StandoffPixels ? 0f : gap - Mathf.Sign(gap) * StandoffPixels;
        }

        /// <summary>Hiệu ứng KHÔNG được làm chết hàng đợi: JarSkin ném khi thiếu sprite, mà

        /// <summary>Chỉ lao khi lượt này thật sự đánh bên kia. Buff lên mình và gói hệ thống
        /// (độc/phản đòn, ActorId == -1) thì đứng yên.</summary>
        private bool HitsOpponent(BattleTurn turn, BattlePetCard actor)
        {
            // == SystemActorId, KHÔNG phải <= 0: id quái cũng âm (GopetPlace.cs:90).
            if (turn.ActorId == BattleTurnState.SystemActorId || actor == null || actor.Fainted) return false;
            foreach (var effect in turn.Effects)
            {
                if (effect.ActorId != turn.ActorId && Card(effect.ActorId) != null) return true;
            }
            return false;
        }


        private BattlePetCard Other(BattlePetCard card)
        {
            foreach (var sibling in GetComponentsInChildren<BattlePetCard>(true))
            {
                if (sibling != card) return sibling;
            }
            return null;
        }

        private void SnapAll()
        {
            foreach (var card in GetComponentsInChildren<BattlePetCard>(true)) card.SnapHome();
        }
    }
}
