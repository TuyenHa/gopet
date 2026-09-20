using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Vệt lửa của ĐÒN THƯỜNG trong màn đánh quái — dùng chung đúng một ảnh
    /// <c>Ui/fx-slash</c> với nút đánh ngoài map (<see cref="WorldSlashEffect"/>), nên bấm
    /// nút ở map hay ở màn đấu đều ra cùng một nhát chém.
    ///
    /// <para>Thay cho dải <c>SlashEffect</c> của jar: dải đó là 4 khung vệt vàng mảnh vẽ cho
    /// màn hình 240px, phóng lên màn hình bây giờ thành hai que chéo to đùng.</para>
    ///
    /// <para>Ảnh vẽ vệt bụng hướng sang PHẢI. Bên ra đòn suy từ vị trí mục tiêu so với tâm
    /// canvas — đòn thường không mang theo vị trí kẻ tấn công (<c>BattleTurnAnimator</c> gọi
    /// với <c>caster = null</c> cho đòn cận chiến) — nên mục tiêu ở nửa phải thì đòn tới từ
    /// trái và ngược lại.</para></summary>
    public sealed class BattleSlashFx : MonoBehaviour
    {
        /// <summary>Marker của <c>TurnEffect.skillId</c> cho đòn thường và chí mạng.</summary>
        private const int NormalAttackId = 0;
        private const int CritAttackId = 2;

        /// <summary>Giây tới lúc coi như chạm — người gọi chờ ngần này rồi mới trừ máu.
        /// Ngắn: vệt quét qua thân gần như tức thì, để lâu thì số sát thương hiện trễ nhịp.</summary>
        private const float ImpactSeconds = 0.10f;

        private const float SwingSeconds = 0.26f;
        private const float FadeSeconds = 0.14f;

        /// <summary>Bề cao vệt so với chiều cao pet. Vệt phải trùm qua cả thân mới ra nhát
        /// chém; bằng đúng thân thì trông như một mảnh lửa dính trên người.</summary>
        private const float HeightRatio = 1.7f;

        /// <summary>Chí mạng to hơn — khác biệt duy nhất giữa hai marker.</summary>
        private const float CritScale = 1.3f;

        /// <summary>Vệt đặt ở đâu trên thân pet, tính theo chiều cao pet (0 = gốc chân).</summary>
        private const float BodyCenterRatio = 0.45f;

        /// <summary>Góc vung: ngửa về sau rồi quét tới. Cùng ý với WorldSlashEffect.</summary>
        private const float StartDegrees = -46f;
        private const float EndDegrees = 30f;

        private RectTransform _rect;
        private Image _image;
        private float _born;
        private float _baseSize;
        private float _flip;

        /// <param name="impactSeconds">Giây tới lúc chạm đích.</param>
        /// <returns>false nếu marker này không phải đòn thường/chí mạng, khi đó người gọi
        /// rơi về atlas jar như cũ.</returns>
        public static bool TryPlay(Transform parent, RectTransform target, int skillId,
            out float impactSeconds)
        {
            impactSeconds = 0f;
            if (target == null || parent == null) return false;
            if (skillId != NormalAttackId && skillId != CritAttackId) return false;

            var sprite = BattleSkin.Load(WorldSlashEffect.SpriteResource);
            if (sprite == null) return false;

            var petHeight = Mathf.Max(32f, target.rect.height);
            var size = petHeight * HeightRatio * (skillId == CritAttackId ? CritScale : 1f);
            var canvasCenterX = parent is RectTransform root ? root.position.x : 0f;

            var go = new GameObject($"Vệt chém {skillId}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var fx = go.AddComponent<BattleSlashFx>();
            fx._image = go.GetComponent<Image>();
            fx._image.sprite = sprite;
            fx._image.raycastTarget = false;
            fx._rect = (RectTransform)go.transform;
            fx._rect.sizeDelta = new Vector2(size, size);
            fx._rect.position = target.position + new Vector3(0f, petHeight * BodyCenterRatio, 0f);
            // Mục tiêu ở nửa phải ⇒ đòn tới từ trái ⇒ giữ nguyên hướng vệ; nửa trái thì lật.
            fx._flip = target.position.x >= canvasCenterX ? 1f : -1f;
            fx._baseSize = size;
            fx._born = Time.unscaledTime;
            fx.Apply(0f);

            impactSeconds = ImpactSeconds;
            return true;
        }

        private void Update()
        {
            var age = Time.unscaledTime - _born;
            if (age >= SwingSeconds + FadeSeconds)
            {
                Destroy(gameObject);
                return;
            }
            Apply(age);
        }

        private void Apply(float age)
        {
            var swing = Mathf.Clamp01(age / SwingSeconds);
            var angle = Mathf.Lerp(StartDegrees, EndDegrees, Mathf.SmoothStep(0f, 1f, swing));
            _rect.localRotation = Quaternion.Euler(0f, 0f, angle * _flip);
            // Lật theo bên ra đòn bằng scale.x — xoay 180° sẽ lộn luôn cả chiều vung.
            var grow = Mathf.Lerp(0.82f, 1.15f, swing);
            _rect.localScale = new Vector3(grow * _flip, grow, 1f);
            _rect.sizeDelta = new Vector2(_baseSize, _baseSize);

            var fade = Mathf.Clamp01((age - SwingSeconds) / FadeSeconds);
            var color = _image.color;
            color.a = 1f - fade;
            _image.color = color;
        }
    }
}
