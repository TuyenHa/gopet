using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Lớp phủ đen fade-in / fade-out khi warp giữa các map. Đơn nhất — tạo 1 lần ở
    /// đầu session, gọi <see cref="FadeOut"/> khi bấm portal và <see cref="FadeIn"/>
    /// khi map mới nạp xong.
    ///
    /// <para><b>Watchdog:</b> server có vài nhánh im lặng không trả MAP_UPDATE (pet
    /// đang đánh nhau, pet chết vì PK, hoặc ngoại lệ bị nuốt ở
    /// <c>Player.cs</c>). Không có hạn giờ thì màn đen ở lại vĩnh viễn VÀ
    /// <c>blocksRaycasts</c> khoá luôn mọi thao tác — người chơi phải thoát game.
    /// Nên <see cref="FadeOut"/> tự hẹn giờ mở lại; <see cref="FadeIn"/> huỷ hẹn.</para>
    /// </summary>
    public sealed class WarpFadeOverlay : MonoBehaviour
    {
        private const float FadeSeconds = 0.35f;

        /// <summary>
        /// Hạn chờ MAP_UPDATE. Rộng tay hơn nhiều so với warp bình thường (đo được
        /// dưới 1s trên máy local) để không cắt ngang một cú warp chậm vì mạng.
        /// </summary>
        private const float DefaultWatchdogSeconds = 8f;

        /// <summary>Hạn chờ thực tế. Test rút ngắn để khỏi phải chờ thật 8 giây.</summary>
        public float WatchdogSeconds { get; set; } = DefaultWatchdogSeconds;

        private CanvasGroup _group;
        private Text _caption;
        private Coroutine _running;
        private Coroutine _watchdog;

        /// <summary>Hết hạn chờ mà map mới không nạp — session dùng để báo người chơi.</summary>
        public event Action TimedOut;

        public static WarpFadeOverlay Create(Transform parent)
        {
            // Canvas riêng thì phải có raycaster riêng: GraphicRaycaster chỉ bắn vào
            // graphic của CHÍNH canvas nó, thiếu nó thì blocksRaycasts chặn hụt và cú
            // chạm rơi xuống HUD trong lúc đang warp.
            var go = new GameObject("Warp Fade", typeof(RectTransform), typeof(Canvas),
                typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(Image));
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
            overlay._caption = MakeCaption(go.transform);
            return overlay;
        }

        /// <summary>
        /// Tên map đích hiện giữa màn đen. Màn đen trơn không phân biệt được với
        /// "game treo" — có tên map thì người chơi biết cú bấm đã ăn và đang đi đâu.
        /// </summary>
        private static Text MakeCaption(Transform parent)
        {
            var text = UiBuilder.MakeText(parent, UiBuilder.BuiltinFont(), "Caption", 20, true);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.93f, 0.72f, 1f);
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            text.text = string.Empty;
            return text;
        }

        /// <summary>Fade đen (dùng lúc bắt đầu warp). Chặn raycast khi đang che.</summary>
        /// <param name="destination">Tên map đích; rỗng thì màn đen không có chữ.</param>
        public void FadeOut(string destination = null)
        {
            if (_caption != null)
                _caption.text = string.IsNullOrEmpty(destination) ? string.Empty : $"Đang đến {destination}...";
            Play(from: _group.alpha, to: 1f, blockRaycasts: true);
            if (_watchdog != null) StopCoroutine(_watchdog);
            _watchdog = StartCoroutine(Watchdog());
        }

        /// <summary>Trở về trong suốt (dùng khi map mới đã nạp xong).</summary>
        public void FadeIn()
        {
            if (_watchdog != null) { StopCoroutine(_watchdog); _watchdog = null; }
            if (_caption != null) _caption.text = string.Empty;
            Play(from: _group.alpha, to: 0f, blockRaycasts: false);
        }

        /// <summary>
        /// Mở khoá màn hình rồi mới bắn sự kiện: người chơi phải thao tác được ngay,
        /// kể cả khi phía nghe sự kiện ném.
        /// </summary>
        private IEnumerator Watchdog()
        {
            yield return new WaitForSeconds(WatchdogSeconds);
            _watchdog = null;
            Debug.LogWarning($"[Gopet] Warp quá {WatchdogSeconds}s không thấy map mới — mở lại màn hình.");
            Play(from: _group.alpha, to: 0f, blockRaycasts: false);
            TimedOut?.Invoke();
        }

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
