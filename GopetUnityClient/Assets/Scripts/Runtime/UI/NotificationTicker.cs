using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Băng thông báo chạy chữ ở đỉnh map, dưới CharacterHud. Ẩn khi rỗng; có thông
    /// báo mới thì hiện loa + text chạy phải → trái. Nền viên thuốc nâu đen mờ, chữ
    /// trắng in đậm — copy nguyên HUD "loa thông báo" của Gopet mobile.
    ///
    /// <para><b>Vị trí</b>: neo đỉnh, giãn đều hai bên với lề <see cref="SideMargin"/>,
    /// cách mép trên <see cref="TopMargin"/> — nằm HẲN dưới CharacterHud.</para>
    /// <para><b>Loa</b>: sprite lớn (<see cref="SpeakerSize"/>) tràn ra mép trái pill để
    /// mắt bám vào biểu tượng trước, giống HUD gốc.</para>
    /// <para><b>Vòng đời text</b>: mỗi thông báo lặp <see cref="RepeatsPerMessage"/> lần rồi
    /// mới pop message kế tiếp trong hàng đợi. Chuỗi rỗng bị bỏ qua.</para>
    /// </summary>
    public sealed class NotificationTicker : MonoBehaviour
    {
        // Asset generated specifically for the Linh Thu City announcement HUD.
        // The ticker itself remains data-driven: it is hidden until BossBannerShown
        // delivers non-empty text from the server.
        private const string SpeakerResource = "Ui/Hud/notify-speaker-v2";

        // Neo cao ở vùng giữa phía trên, NGAY DƯỚI thanh tài nguyên (đậu/lúa/vàng) và
        // thẳng cột với nó: hai thanh cùng một khối thông tin, lệch mép trái nhìn rất
        // chướng. Mép phải chừa rộng hơn để không chạm cụm icon shop góc phải-trên.
        private const float TopMargin = 38f;
        // Cái mắt thấy ở mép trái băng là CÁI LOA (nó thò ra ngoài viên thuốc), nên
        // thẳng cột với thanh tài nguyên nghĩa là loa thẳng cột, không phải viên thuốc.
        private const float LeftMargin = World.CurrencyBar.LeftMargin + SpeakerOverhang;
        /// <summary>
        /// Lề phải. Ở khung chuẩn 960 băng dài 261px — bằng 3/4 độ dài cũ (348px): chữ
        /// vẫn chạy thoải mái mà băng không kéo dài gần hết bề ngang màn.
        /// </summary>
        private const float RightMargin = 417f;
        private const float Height = 32f;
        private const float SpeakerSize = 48f;      // loa to hơn pill, tràn ra ngoài
        private const float SpeakerOverhang = 14f;  // px thò ra mép trái pill
        private const float TextLeftPad = 40f;      // chỗ trống bên trong pill cho loa
        private const float ScrollSpeed = 90f;      // px/s
        private const float GapBetweenLoops = 60f;
        private const int RepeatsPerMessage = 2;

        private RectTransform _viewport;
        private Text _label;
        private RectTransform _labelRect;
        private readonly Queue<string> _queue = new Queue<string>();
        private string _current;
        private int _repeatsLeft;
        private float _labelWidth;

        public static NotificationTicker Create(Transform parent)
        {
            var go = new GameObject("Notification Ticker",
                typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(LeftMargin, -(TopMargin + Height));
            rect.offsetMax = new Vector2(-RightMargin, -TopMargin);

            // Viên thuốc nâu đen mờ — copy HUD "loa thông báo" mobile: chỉ 1 lớp, không viền.
            var pill = go.GetComponent<Image>();
            pill.raycastTarget = false;
            RoundedUiSprite.Apply(pill);
            pill.color = new Color(0.10f, 0.08f, 0.06f, 0.72f);

            var ticker = go.AddComponent<NotificationTicker>();
            ticker.Build(go.transform, UiBuilder.BuiltinFont());
            ticker.gameObject.SetActive(false);
            return ticker;
        }

        /// <summary>Thêm thông báo vào hàng đợi. Chuỗi rỗng bị bỏ qua.</summary>
        public void Show(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _queue.Enqueue(text);
            if (_current == null) AdvanceToNext();
        }

        /// <summary>Xoá hàng đợi và ẩn ticker.</summary>
        public void Clear()
        {
            _queue.Clear();
            _current = null;
            _repeatsLeft = 0;
            gameObject.SetActive(false);
        }

        private void Build(Transform surface, Font font)
        {
            // Loa tràn ra mép trái pill, canh giữa trục dọc — mắt bám icon trước, chữ sau.
            var speaker = MakeImage(surface, "Speaker", Color.white);
            var sRect = speaker.rectTransform;
            sRect.anchorMin = new Vector2(0f, 0.5f);
            sRect.anchorMax = new Vector2(0f, 0.5f);
            sRect.pivot = new Vector2(0.5f, 0.5f);
            sRect.anchoredPosition = new Vector2(-SpeakerOverhang + SpeakerSize * 0.5f, 0f);
            sRect.sizeDelta = new Vector2(SpeakerSize, SpeakerSize);
            var speakerSprite = Resources.Load<Sprite>(SpeakerResource);
            if (speakerSprite != null)
            {
                speaker.sprite = speakerSprite;
                speaker.preserveAspect = true;
            }
            else FallbackDot(speaker, new Color(1f, 0.78f, 0.18f, 1f));

            // Viewport cắt text; text con rộng theo nội dung, dịch sang trái mỗi frame.
            var viewportGo = new GameObject("Text Viewport",
                typeof(RectTransform), typeof(RectMask2D));
            viewportGo.transform.SetParent(surface, false);
            _viewport = (RectTransform)viewportGo.transform;
            _viewport.anchorMin = new Vector2(0f, 0f);
            _viewport.anchorMax = new Vector2(1f, 1f);
            _viewport.offsetMin = new Vector2(TextLeftPad, 2f);
            _viewport.offsetMax = new Vector2(-10f, -2f);

            _label = UiBuilder.MakeText(_viewport, font, "Marquee", 17, false);
            _label.alignment = TextAnchor.MiddleLeft;
            _label.color = Color.white;
            _label.fontStyle = FontStyle.Bold;
            _label.horizontalOverflow = HorizontalWrapMode.Overflow;
            _labelRect = _label.rectTransform;
            _labelRect.anchorMin = new Vector2(0f, 0f);
            _labelRect.anchorMax = new Vector2(0f, 1f);
            _labelRect.pivot = new Vector2(0f, 0.5f);
        }

        private void Update()
        {
            if (_current == null) return;

            var pos = _labelRect.anchoredPosition;
            pos.x -= ScrollSpeed * Time.deltaTime;
            // Chạy hết chữ (đuôi qua khỏi mép trái viewport) → lặp lại hoặc pop.
            if (pos.x + _labelWidth < 0f)
            {
                _repeatsLeft--;
                if (_repeatsLeft <= 0)
                {
                    AdvanceToNext();
                    return;
                }
                pos.x = _viewport.rect.width + GapBetweenLoops * 0.25f;
            }
            _labelRect.anchoredPosition = pos;
        }

        private void AdvanceToNext()
        {
            if (_queue.Count == 0)
            {
                _current = null;
                gameObject.SetActive(false);
                return;
            }
            _current = _queue.Dequeue();
            _repeatsLeft = RepeatsPerMessage;
            _label.text = _current;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_labelRect);
            _labelWidth = Mathf.Max(_label.preferredWidth, 1f);
            _labelRect.sizeDelta = new Vector2(_labelWidth, 0f);
            _labelRect.anchoredPosition = new Vector2(_viewport.rect.width, 0f);
            gameObject.SetActive(true);
        }

        private static Image MakeImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void FallbackDot(Image image, Color tint)
        {
            RoundedUiSprite.Apply(image);
            image.color = tint;
        }

    }
}
