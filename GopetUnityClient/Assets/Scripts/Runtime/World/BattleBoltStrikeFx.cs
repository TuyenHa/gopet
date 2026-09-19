using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Hiệu ứng "sét giáng": một tia lớn đánh thẳng từ trên xuống mục tiêu, kèm vài
    /// nhánh điện toé ra quanh điểm chạm, nhấp nháy vài nhịp rồi tắt.
    ///
    /// <para>Khác hẳn <see cref="BattleFlameFallFx"/> ở NHỊP: lửa có quãng rơi rồi mới bùng,
    /// còn sét thì <b>đánh tức thì</b> — hiện ngay ở cường độ tối đa rồi giật. Cho sét rơi từ
    /// từ sẽ thành cây gậy xanh trôi xuống, mất hết chất.</para>
    ///
    /// <para>Nhánh toé dùng LẠI chính ảnh tia, thu nhỏ và xoay quanh điểm chạm — khỏi phải
    /// thêm asset thứ hai.</para></summary>
    public sealed class BattleBoltStrikeFx : MonoBehaviour
    {
        /// <summary>Tới lúc coi như đã chạm. Sét gần như tức thì, để lâu thì số sát thương
        /// hiện ra trễ hơn cú đánh và trông như trượt nhịp.</summary>
        private const float StrikeSeconds = 0.10f;

        private const float FlickerSeconds = 0.30f;
        private const float FadeSeconds = 0.18f;
        private const float FlickerHz = 26f;

        /// <summary>Chiều cao tia chính so với chiều cao canvas. Trên 1 để tia chạy hẳn ra
        /// ngoài mép trên — sét phải như từ trời giáng xuống, không phải mọc trong khung.</summary>
        private const float BoltHeightRatio = 1.05f;

        /// <summary>Đầu tia dừng ở đâu trên thân pet, tính theo chiều cao pet.</summary>
        private const float TipAboveFeet = 0.45f;

        private const float ArcHeightRatio = 1.00f;   // nhánh toé, so với chiều cao pet

        /// <summary>Góc các nhánh toé, độ, 0 là chỉ thẳng lên. Trải từ gần NGANG (100) tới gần
        /// thẳng đứng (170) cho điện bò rộng quanh mục tiêu. Dồn hết vào góc chúc xuống thì
        /// các nhánh chụm lại trông như bộ rễ chứ không phải điện toé.</summary>
        private static readonly float[] ArcAngles = { -100f, -135f, -170f, 100f, 135f, 170f };

        private CanvasGroup _bolt;
        private CanvasGroup[] _arcs;
        private float _born;

        /// <param name="impactSeconds">Giây tới lúc chạm đích.</param>
        /// <returns>false nếu kỹ năng này không có ảnh ghi đè.</returns>
        public static bool TryPlay(Transform parent, RectTransform target, int skillId,
            out float impactSeconds)
        {
            impactSeconds = 0f;
            var sprite = BattleSkin.Load($"Battle/fx/{skillId}");
            if (sprite == null || target == null) return false;

            var canvasHeight = parent is RectTransform root && root.rect.height > 1f
                ? root.rect.height : 540f;
            var petH = Mathf.Max(32f, target.rect.height);
            var aspect = sprite.rect.width / sprite.rect.height;
            var tip = new Vector3(target.position.x, target.position.y + petH * TipAboveFeet,
                target.position.z);

            var go = new GameObject($"Sét giáng {skillId}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var fx = go.AddComponent<BattleBoltStrikeFx>();
            fx._born = Time.unscaledTime;
            fx._bolt = Spawn(go.transform, sprite, "Tia chính", canvasHeight * BoltHeightRatio,
                aspect, tip, 0f);

            fx._arcs = new CanvasGroup[ArcAngles.Length];
            for (var i = 0; i < ArcAngles.Length; i++)
            {
                fx._arcs[i] = Spawn(go.transform, sprite, $"Nhánh {i}", petH * ArcHeightRatio,
                    aspect, tip, ArcAngles[i]);
                fx._arcs[i].alpha = 0f;
            }

            impactSeconds = StrikeSeconds;
            return true;
        }

        private static CanvasGroup Spawn(Transform parent, Sprite sprite, string label,
            float height, float aspect, Vector3 tip, float angle)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image),
                typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;

            var rect = (RectTransform)go.transform;
            // Pivot ĐÁY: xoay quanh đúng điểm chạm, nên nhánh nào cũng toé ra từ một gốc.
            // Pivot giữa sẽ làm nhánh văng lệch khỏi mục tiêu khi xoay.
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(height * aspect, height);
            rect.position = tip;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            return go.GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            var age = Time.unscaledTime - _born;
            var alpha = AlphaAt(age);
            if (alpha < 0f) { Destroy(gameObject); return; }

            _bolt.alpha = alpha;
            // Nhánh chỉ toé RA khi tia đã chạm; hiện sớm thì thành điện bò trước cả sét.
            var arcAlpha = age < StrikeSeconds ? 0f : alpha * 0.85f;
            for (var i = 0; i < _arcs.Length; i++) _arcs[i].alpha = arcAlpha;
        }

        /// <returns>Độ mờ hiện tại, hoặc số âm khi đã diễn xong.</returns>
        private static float AlphaAt(float age)
        {
            if (age < 0f) return 0f;

            var live = StrikeSeconds + FlickerSeconds;
            if (age < live)
            {
                // Trị TUYỆT ĐỐI của sin: sét giật chứ không sáng đều, nhưng nửa chu kỳ âm
                // của sin thường sẽ tắt tia quá lâu, nhìn thành chớp nháy rời rạc.
                return 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(age * FlickerHz));
            }

            var fade = (age - live) / FadeSeconds;
            return fade >= 1f ? -1f : (1f - fade) * 0.8f;
        }
    }
}
