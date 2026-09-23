using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Màn chuyển map — port <c>fx.java</c> chế độ 1 của jar.
    ///
    /// <para>Jar vẽ: phủ ĐEN toàn màn (<c>fillRect(0,0,w,h)</c>) rồi vẽ 10 ký tự chữ "đang tải"
    /// ở GÓC PHẢI DƯỚI (<c>w - atlasW - 10 + off</c>, <c>h - bob - 5</c>, anchor 36 = trái-đáy).
    /// Mỗi ký tự nhún dọc theo bảng <c>{0, -2, 0, 3}</c>; <c>c_()</c> cứ 16 nhịp lại châm một
    /// ký tự mới nên cái nhún chạy thành LÀN SÓNG từ trái sang phải.</para>
    ///
    /// <para>Dùng chữ thay atlas <c>common.dat</c> của jar: atlas đó là ảnh bitmap chữ hoa
    /// không dấu, mà client này đã bỏ font bitmap.</para></summary>
    public sealed class MapLoadingOverlay : MonoBehaviour
    {
        private const string Word = "Đang tải";
        private const int FontSize = 20;

        /// <summary>Lề phải và lề dưới, theo jar (10 và 5 pixel).</summary>
        private const float MarginRight = 10f, MarginBottom = 5f;

        /// <summary>Bảng nhún dọc của jar: <c>b = {0, -2, 0, 3}</c>. Trong jar trục Y hướng
        /// XUỐNG và nó vẽ ở <c>h - bob</c>, nên dấu ở đây đảo lại cho Unity (Y hướng lên).</summary>
        private static readonly float[] Bob = { 0f, 2f, 0f, -3f };

        /// <summary>Jar đổi trạng thái mỗi nhịp game (~15 nhịp/giây) và châm ký tự mới mỗi 16
        /// nhịp. Quy về giây để tốc độ không đổi theo khung hình như bản J2ME.</summary>
        private const float StepSeconds = 1f / 15f;
        private const int StepsPerLetter = 16;

        /// <summary>Hiện tối thiểu ngần này giây, kể cả khi map đã nạp xong.
        ///
        /// <para>Server chạy localhost nên round-trip đổi map chỉ vài mili-giây: không có mốc
        /// này thì màn đen nhấp nháy 1–2 frame rồi biến, người chơi không kịp thấy. Bản jar
        /// chạy trên điện thoại qua mạng chậm nên lúc nào cũng hiện đủ lâu.</para></summary>
        private const float MinVisibleSeconds = 0.45f;

        /// <summary>Chốt chặn: map không bao giờ nạp xong (mất gói, lỗi asset) thì vẫn phải
        /// gỡ màn đen ra. Để treo màn đen vĩnh viễn là hỏng nặng hơn hẳn việc thiếu hiệu ứng.</summary>
        private const float MaxVisibleSeconds = 8f;

        private Text[] _letters;
        private float[] _offsets;
        private int[] _state;
        private float _nextStep;
        private int _tick;
        private int _next;
        private float _born;
        private bool _closing;

        public static MapLoadingOverlay Create(Transform parent)
        {
            var go = new GameObject("Màn chuyển map", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Trên mọi HUD: đang tải map thì không được thấy widget của map cũ.
            canvas.sortingOrder = 200;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 1f;

            var view = go.AddComponent<MapLoadingOverlay>();
            view._born = Time.unscaledTime;
            view.Build(UiBuilder.DefaultFont());
            return view;
        }

        private void Build(Font font)
        {
            var black = new GameObject("Nền đen", typeof(RectTransform), typeof(Image));
            black.transform.SetParent(transform, false);
            UiBuilder.Stretch((RectTransform)black.transform);
            // Chặn chạm: bấm xuyên qua màn chờ sẽ trúng widget của map cũ.
            black.GetComponent<Image>().color = Color.black;

            _letters = new Text[Word.Length];
            _offsets = new float[Word.Length];
            _state = new int[Word.Length];

            // Xếp từ PHẢI sang trái để ký tự cuối cách mép phải đúng MarginRight, y như jar
            // neo cả cụm vào góc phải dưới.
            var x = -MarginRight;
            for (var i = Word.Length - 1; i >= 0; i--)
            {
                var label = UiBuilder.MakeText(transform, font, $"Ký tự {i}", FontSize, false);
                label.text = Word[i].ToString();
                label.alignment = TextAnchor.LowerRight;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
                label.raycastTarget = false;
                var rect = label.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.sizeDelta = new Vector2(FontSize, FontSize * 1.4f);
                _letters[i] = label;
                _offsets[i] = x;
                x -= label.preferredWidth > 1f ? label.preferredWidth : FontSize * 0.6f;
            }
            Place();
        }

        /// <summary>Map đã nạp xong — gỡ màn, nhưng không sớm hơn <see cref="MinVisibleSeconds"/>.</summary>
        public void RequestClose() => _closing = true;

        private void Update()
        {
            var age = Time.unscaledTime - _born;
            if ((_closing && age >= MinVisibleSeconds) || age >= MaxVisibleSeconds)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.unscaledTime < _nextStep) return;
            _nextStep = Time.unscaledTime + StepSeconds;

            // Mỗi nhịp hạ dần trạng thái từng ký tự; cứ StepsPerLetter nhịp thì châm ký tự
            // kế tiếp về 3 — đó là cái làm nhún chạy thành làn sóng.
            for (var i = 0; i < _state.Length; i++) if (_state[i] > 0) _state[i]--;
            if (_next < _state.Length) _state[_next] = Bob.Length - 1;
            _next++;
            if (++_tick > StepsPerLetter) { _tick = 0; _next = 0; }
            Place();
        }

        private void Place()
        {
            for (var i = 0; i < _letters.Length; i++)
            {
                _letters[i].rectTransform.anchoredPosition =
                    new Vector2(_offsets[i], MarginBottom + Bob[_state[i]]);
            }
        }
    }
}
