using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Toast hiện ~2 giây rồi tự huỷ, cao theo nội dung (câu dài tự xuống dòng thay
    /// vì tràn ra ngoài nền). Dùng cho thông báo nhẹ như
    /// "sắp có", không cần chặn tương tác của player.
    ///
    /// <para><b>Không đè lên chồng dialog:</b> toast dùng để BÁO, không để CHỜ trả
    /// lời — nên không push vào <see cref="DialogStack"/>. Player vẫn di chuyển,
    /// bấm nút khác trong lúc toast hiển thị.</para>
    /// </summary>
    public sealed class ToastView : MonoBehaviour
    {
        private const float Width = 360f;
        private const float MinHeight = 44f;
        /// <summary>Chừa hai bên cho chữ không dính mép nền.</summary>
        private const float TextPadding = 16f;
        private const float DurationSeconds = 2f;
        /// <summary>Nền xanh đặc, chữ trắng — mã màu đo trên ảnh mẫu: rgb(75,131,228).</summary>
        private static readonly Color BackgroundBlue = new Color(0.294f, 0.514f, 0.894f, 1f);
        private const float CornerRadius = 6f;
        private const float FadeSeconds = 0.35f;

        public static ToastView Create(Transform parent, Font font, string text)
        {
            var go = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup),
                typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            // Neo giữa dưới. Cao 44 cho câu ngắn một dòng, nới thêm ở dưới theo
            // preferredHeight khi câu dài phải xuống dòng.
            rect.anchorMin = new Vector2(0.5f, 0.15f);
            rect.anchorMax = new Vector2(0.5f, 0.15f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Width, MinHeight);

            var bg = go.GetComponent<Image>();
            RoundedUiSprite.Apply(bg, CornerRadius);
            bg.color = BackgroundBlue;

            // stretch: false — label phải có BỀ NGANG XÁC ĐỌNH ngay lúc này thì
            // preferredHeight mới tính đúng số dòng; anchor stretch phải đợi layout pass.
            var label = UiBuilder.MakeText(go.transform, font, "Label", 16, stretch: false);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(Width - TextPadding, MinHeight);
            label.alignment = TextAnchor.MiddleCenter;
            label.text = text;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var height = Mathf.Max(MinHeight, label.preferredHeight + TextPadding);
            rect.sizeDelta = new Vector2(Width, height);
            labelRect.sizeDelta = new Vector2(Width - TextPadding, height - TextPadding);

            var view = go.AddComponent<ToastView>();
            view.StartCoroutine(view.FadeAndDestroy(go.GetComponent<CanvasGroup>()));
            return view;
        }

        private IEnumerator FadeAndDestroy(CanvasGroup group)
        {
            yield return new WaitForSeconds(DurationSeconds);

            var elapsed = 0f;
            while (elapsed < FadeSeconds)
            {
                elapsed += Time.deltaTime;
                if (group != null) group.alpha = Mathf.Clamp01(1f - elapsed / FadeSeconds);
                yield return null;
            }

            if (gameObject != null) Destroy(gameObject);
        }
    }
}
