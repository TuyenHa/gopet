using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Toast một dòng, hiện ~2 giây rồi tự huỷ. Dùng cho thông báo nhẹ như
    /// "sắp có", không cần chặn tương tác của player.
    ///
    /// <para><b>Không đè lên chồng dialog:</b> toast dùng để BÁO, không để CHỜ trả
    /// lời — nên không push vào <see cref="DialogStack"/>. Player vẫn di chuyển,
    /// bấm nút khác trong lúc toast hiển thị.</para>
    /// </summary>
    public sealed class ToastView : MonoBehaviour
    {
        private const float DurationSeconds = 2f;
        private const float FadeSeconds = 0.35f;

        public static ToastView Create(Transform parent, Font font, string text)
        {
            var go = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup),
                typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            // Neo giữa dưới, cao 44 — vừa đủ đọc một dòng.
            rect.anchorMin = new Vector2(0.5f, 0.15f);
            rect.anchorMax = new Vector2(0.5f, 0.15f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 44f);

            var bg = go.GetComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.1f, 0.9f);

            var label = UiBuilder.MakeText(go.transform, font, "Label", 16, stretch: true);
            label.alignment = TextAnchor.MiddleCenter;
            label.text = text;
            label.color = UiBuilder.TextMain;

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
