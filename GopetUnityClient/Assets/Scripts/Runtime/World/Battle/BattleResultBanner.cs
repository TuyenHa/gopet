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
    /// <para><b>Vẽ bằng CHỮ</b>, cùng font với HUD (<see cref="UiBuilder.DefaultFont"/>), chứ
    /// không phải ảnh dựng sẵn nữa. Ảnh sinh bằng AI hay sai dấu tiếng Việt — bản cũ từng ra
    /// "CHIẾN THẤNG" vì model vẽ dấu mũ thay cho dấu breve.</para></summary>
    public static class BattleResultBanner
    {
        /// <summary>0 = nằm ngang. Muốn nghiêng lại thì đặt góc: âm là chúi sang phải, dương
        /// là hếch lên phải (Unity quay ngược chiều kim đồng hồ với Z dương).</summary>
        private const float TiltDegrees = 0f;

        private const float WidthRatio = 0.72f;   // bề ngang khung chữ so với canvas

        /// <summary>Cỡ chữ so với chiều cao canvas. Theo canvas chứ không số pixel cứng:
        /// canvas co giãn theo màn hình nên số cứng sẽ bé tí ở máy lớn.</summary>
        private const float FontRatio = 0.115f;
        private const float PopSeconds = 0.35f;   // bật ra lúc xuất hiện
        private const float HoldSeconds = 1.5f;   // đứng yên, đủ đọc
        private const float FadeSeconds = 1f;     // mờ dần rồi biến mất

        /// <summary>Tổng thời gian băng chữ sống. <see cref="BattleView"/> dùng đúng hằng này
        /// làm mốc tự đóng về map — để hai chỗ không lệch nhau khiến chữ bị cắt giữa chừng
        /// hoặc màn hình treo sau khi chữ đã tan.</summary>
        public const float TotalSeconds = HoldSeconds + FadeSeconds;

        public static GameObject Create(Transform parent, BattleResult result, int localActorId,
            bool isParticipant)
        {
            var won = result.WinnerId == localActorId;
            var go = new GameObject("Băng kết quả", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var canvasHeight = parent is RectTransform pr && pr.rect.height > 1f ? pr.rect.height : 540f;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = SizeFor(parent, canvasHeight);
            rect.localRotation = Quaternion.Euler(0f, 0f, TiltDegrees);

            var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Nhãn",
                Mathf.RoundToInt(canvasHeight * FontRatio), false);
            UiBuilder.Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.raycastTarget = false;
            label.text = won ? "CHIẾN THẮNG" : isParticipant ? "THUA CUỘC" : "KẾT THÚC";
            label.color = won ? new Color(1f, 0.82f, 0.2f) : new Color(0.75f, 0.82f, 0.9f);

            // Viền TRẮNG quanh chữ để đọc được trên mọi nền map.
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            var group = go.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            go.AddComponent<BannerPop>().Begin(rect, group);
            return go;
        }

        private static Vector2 SizeFor(Transform parent, float canvasHeight)
        {
            var canvasWidth = parent is RectTransform pr && pr.rect.width > 1f ? pr.rect.width : 960f;
            return new Vector2(canvasWidth * WidthRatio, canvasHeight * FontRatio * 1.6f);
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
