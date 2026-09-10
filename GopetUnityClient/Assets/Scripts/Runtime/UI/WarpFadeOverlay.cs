using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Lớp phủ đen fade-in / fade-out khi warp giữa các map. Đơn nhất — tạo 1 lần ở
    /// đầu session, gọi <see cref="FadeOut"/> khi bấm portal và <see cref="FadeIn"/>
    /// khi map mới nạp xong.
    /// </summary>
    public sealed class WarpFadeOverlay : MonoBehaviour
    {
        private const float FadeSeconds = 0.35f;

        private CanvasGroup _group;
        private Coroutine _running;

        public static WarpFadeOverlay Create(Transform parent)
        {
            var go = new GameObject("Warp Fade", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasGroup), typeof(Image));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 95;  // dưới VerticalSplit (100), trên GameHud (30) và popup (35).

            UiBuilder.Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = Color.black;

            var overlay = go.AddComponent<WarpFadeOverlay>();
            overlay._group = go.GetComponent<CanvasGroup>();
            overlay._group.alpha = 0f;
            overlay._group.blocksRaycasts = false;
            return overlay;
        }

        /// <summary>Fade đen (dùng lúc bắt đầu warp). Chặn raycast khi đang che.</summary>
        public void FadeOut() => Play(from: _group.alpha, to: 1f, blockRaycasts: true);

        /// <summary>Trở về trong suốt (dùng khi map mới đã nạp xong).</summary>
        public void FadeIn() => Play(from: _group.alpha, to: 0f, blockRaycasts: false);

        private void Play(float from, float to, bool blockRaycasts)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Tween(from, to, blockRaycasts));
        }

        private IEnumerator Tween(float from, float to, bool blockRaycasts)
        {
            _group.blocksRaycasts = true; // luôn chặn trong lúc tween để tránh double-click warp.
            var elapsed = 0f;
            while (elapsed < FadeSeconds)
            {
                elapsed += Time.deltaTime;
                _group.alpha = Mathf.Lerp(from, to, elapsed / FadeSeconds);
                yield return null;
            }
            _group.alpha = to;
            _group.blocksRaycasts = blockRaycasts;
            _running = null;
        }
    }
}
