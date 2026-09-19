using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Vòng phép vàng dưới chân pet bị đánh khi trúng kỹ năng: hiện ra, nở nhẹ,
    /// giữ một nhịp rồi mờ đi.
    ///
    /// <para>Neo vào <c>EffectAnchor</c> của card — pivot sprite pet là (0.5, 0) tức gốc chân,
    /// nên vòng nằm đúng mặt đất chứ không lơ lửng giữa thân.</para></summary>
    public sealed class BattleGroundSigil : MonoBehaviour
    {
        private const float GrowSeconds = 0.18f;
        private const float HoldSeconds = 0.85f;
        private const float FadeSeconds = 0.35f;
        private const float WidthRatio = 2.6f;   // so với bề ngang sprite pet
        private const float PeakAlpha = 0.9f;

        private CanvasGroup _group;
        private RectTransform _rect;
        private float _born;

        public static void Play(Transform parent, RectTransform target)
        {
            if (target == null) return;

            var go = new GameObject("Vòng phép", typeof(RectTransform), typeof(Image),
                typeof(CanvasGroup));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.sprite = BattleSkin.Load("Battle/fx-ground-sigil");
            img.preserveAspect = true;
            img.raycastTarget = false;
            if (img.sprite == null) { Destroy(go); return; }

            var rect = (RectTransform)go.transform;
            rect.position = target.position;
            var width = Mathf.Max(60f, target.rect.width * WidthRatio);
            rect.sizeDelta = new Vector2(width, width * img.sprite.rect.height / img.sprite.rect.width);

            var sigil = go.AddComponent<BattleGroundSigil>();
            sigil._rect = rect;
            sigil._group = go.GetComponent<CanvasGroup>();
            sigil._group.blocksRaycasts = false;
            sigil._group.alpha = 0f;
            sigil._born = Time.unscaledTime;

            // Đẩy xuống DƯỚI mọi thứ đã dựng: vòng nằm trên mặt đất nên pet phải đứng đè lên nó,
            // không phải bị nó che mất. SetAsFirstSibling đặt sau nền nhưng trước card pet.
            go.transform.SetSiblingIndex(1);
        }

        private void Update()
        {
            var age = Time.unscaledTime - _born;

            if (age < GrowSeconds)
            {
                var t = age / GrowSeconds;
                _group.alpha = t * PeakAlpha;
                _rect.localScale = Vector3.one * Mathf.Lerp(0.55f, 1f, Mathf.SmoothStep(0f, 1f, t));
                return;
            }

            _rect.localScale = Vector3.one;
            var fade = age - GrowSeconds - HoldSeconds;
            if (fade <= 0f) { _group.alpha = PeakAlpha; return; }
            if (fade >= FadeSeconds) { Destroy(gameObject); return; }
            _group.alpha = PeakAlpha * (1f - fade / FadeSeconds);
        }
    }
}
