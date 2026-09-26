using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Pager dưới đáy tab Chợ: ‹ + "Trang x/y" + › — khoá mờ ở biên đầu/cuối. Glyph ‹/›
    /// thay vì ảnh mũi tên: cùng font Be Vietnam Pro đã vẽ đúng ký tự này ở
    /// <c>CharacterHubPopupView.Settings.cs:98</c>, khỏi phải sinh thêm asset.
    /// </summary>
    public sealed class MarketPagerBar : MonoBehaviour
    {
        public const float Height = 28f;
        private const float ButtonSize = 24f;

        private Button _prev;
        private Button _next;
        private Image _prevImage;
        private Image _nextImage;
        private Text _prevLabel;
        private Text _nextLabel;
        private Text _pageLabel;
        private int _page;
        private int _totalPages = 1;

        /// <summary>Trang MỚI người chơi muốn xem (0-based). Caller tự gửi packet 47.</summary>
        public event Action<int> PageRequested;

        public static MarketPagerBar Create(RectTransform parent, Font font)
        {
            var go = new GameObject("PagerBar", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, Height);
            rect.anchoredPosition = Vector2.zero;

            var bar = go.AddComponent<MarketPagerBar>();
            bar.Build(font);
            bar.SetPage(0, 1);
            return bar;
        }

        public void SetPage(int page, int totalPages)
        {
            _page = page;
            _totalPages = Mathf.Max(1, totalPages);
            _pageLabel.text = $"Trang {_page + 1}/{_totalPages}";

            var canPrev = _page > 0;
            var canNext = _page < _totalPages - 1;
            _prev.interactable = canPrev;
            _next.interactable = canNext;
            SetDimmed(_prevImage, _prevLabel, !canPrev);
            SetDimmed(_nextImage, _nextLabel, !canNext);
        }

        private static void SetDimmed(Image image, Text label, bool dimmed)
        {
            var alpha = dimmed ? 0.4f : 1f;
            var bg = image.color; bg.a = alpha; image.color = bg;
            var fg = label.color; fg.a = alpha; label.color = fg;
        }

        private void Build(Font font)
        {
            _prev = MakeArrowButton("Prev", "‹", new Vector2(0f, 0.5f), () => Request(_page - 1),
                out _prevImage, out _prevLabel, font);
            _next = MakeArrowButton("Next", "›", new Vector2(1f, 0.5f), () => Request(_page + 1),
                out _nextImage, out _nextLabel, font);

            _pageLabel = UiBuilder.MakeText(transform, font, "PageLabel", 11, false);
            _pageLabel.alignment = TextAnchor.MiddleCenter;
            _pageLabel.color = PopupPalette.TextDark;
            UiBuilder.SetFontStyle(_pageLabel, FontStyle.Bold);

            var labelRect = _pageLabel.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(ButtonSize + 6f, 0f);
            labelRect.offsetMax = new Vector2(-(ButtonSize + 6f), 0f);
        }

        private Button MakeArrowButton(string name, string glyph, Vector2 anchor, Action onClick,
            out Image image, out Text label, Font font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rect.anchoredPosition = Vector2.zero;

            image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 6f);
            image.color = PopupPalette.ButtonBlue;

            label = UiBuilder.MakeText(go.transform, font, "Glyph", 14, true);
            label.text = glyph;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => onClick());
            return button;
        }

        private void Request(int page)
        {
            if (page < 0 || page >= _totalPages) return;
            PageRequested?.Invoke(page);
        }
    }
}
