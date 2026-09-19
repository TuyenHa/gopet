using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Chữ "CHIẾN THẮNG" / "THUA CUỘC" nghiêng 45° giữa màn hình khi hết trận,
    /// thay cho popup có nút "Tiếp tục" trước đây. Phần thưởng không nằm ở đây nữa —
    /// nó bay thành số trên đầu pet, giống jar gốc (<c>e.java:57-63</c>).
    ///
    /// <para>Ảnh sinh bằng <c>tools/image-gen</c>, đã cắt sát nội dung nên
    /// <c>preserveAspect</c> là đủ để không méo chữ.</para></summary>
    public static class BattleResultBanner
    {
        /// <summary>0 = nằm ngang. Muốn nghiêng lại thì đặt góc: âm là chúi sang phải, dương
        /// là hếch lên phải (Unity quay ngược chiều kim đồng hồ với Z dương).</summary>
        private const float TiltDegrees = 0f;

        private const float WidthRatio = 0.48f;   // bề ngang so với canvas
        private const float PopSeconds = 0.35f;   // bật ra lúc xuất hiện
        private const float HoldSeconds = 2.5f;   // đứng yên, đọc được thoải mái
        private const float FadeSeconds = 2f;     // mờ dần rồi biến mất

        /// <summary>Tổng thời gian băng chữ sống. <see cref="BattleView"/> dùng đúng hằng này
        /// làm mốc tự đóng về map — để hai chỗ không lệch nhau khiến chữ bị cắt giữa chừng
        /// hoặc màn hình treo sau khi chữ đã tan.</summary>
        public const float TotalSeconds = HoldSeconds + FadeSeconds;

        public static GameObject Create(Transform parent, BattleResult result, int localActorId,
            bool isParticipant)
        {
            var won = result.WinnerId == localActorId;
            var go = new GameObject("Băng kết quả", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.sprite = BattleSkin.Load(won ? "Battle/banner-victory" : "Battle/banner-defeat");
            image.preserveAspect = true;
            image.raycastTarget = false;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = SizeFor(image.sprite, parent);
            rect.localRotation = Quaternion.Euler(0f, 0f, TiltDegrees);

            // Không có ảnh (thiếu asset) thì rơi về chữ thường, đừng để màn hình trống trơn.
            if (image.sprite == null)
            {
                image.color = new Color(0f, 0f, 0f, 0f);
                var label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Nhãn", 40, false);
                UiBuilder.Stretch(label.rectTransform);
                label.alignment = TextAnchor.MiddleCenter;
                label.fontStyle = FontStyle.Bold;
                label.text = won ? "CHIẾN THẮNG" : isParticipant ? "THUA CUỘC" : "KẾT THÚC";
                label.color = won ? new Color(1f, 0.82f, 0.2f) : new Color(0.75f, 0.82f, 0.9f);
            }

            var group = go.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            go.AddComponent<BannerPop>().Begin(rect, group);
            return go;
        }

        private static Vector2 SizeFor(Sprite sprite, Transform parent)
        {
            var canvasWidth = parent is RectTransform pr && pr.rect.width > 1f ? pr.rect.width : 960f;
            var width = canvasWidth * WidthRatio;
            if (sprite == null || sprite.rect.width <= 0f) return new Vector2(width, width * 0.22f);
            return new Vector2(width, width * sprite.rect.height / sprite.rect.width);
        }

        /// <summary>Vòng đời băng chữ: bật ra → đứng yên → mờ dần. Dùng thời gian unscaled
        /// cho khớp phần còn lại của battle UI.</summary>
        private sealed class BannerPop : MonoBehaviour
        {
            private RectTransform _rect;
            private CanvasGroup _group;
            private float _born;

            public void Begin(RectTransform rect, CanvasGroup group)
            {
                _rect = rect;
                _group = group;
                _born = Time.unscaledTime;
                rect.localScale = Vector3.one * 0.6f;
            }

            private void Update()
            {
                if (_rect == null) return;
                var age = Time.unscaledTime - _born;

                // Bật ra: vọt quá 1 rồi lắng về 1 cho có lực.
                if (age < PopSeconds)
                {
                    var t = Mathf.Clamp01(age / PopSeconds);
                    _rect.localScale = Vector3.one *
                        Mathf.LerpUnclamped(0.6f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f) * 1.08f);
                }
                else
                {
                    _rect.localScale = Vector3.one;
                }

                if (_group == null) return;
                // Mờ dần trong FadeSeconds cuối, hơi nhích lên cho cảm giác tan đi.
                var fade = Mathf.Clamp01((age - HoldSeconds) / FadeSeconds);
                _group.alpha = 1f - fade;
                _rect.anchoredPosition = new Vector2(0f, fade * 24f);
            }
        }
    }
}
