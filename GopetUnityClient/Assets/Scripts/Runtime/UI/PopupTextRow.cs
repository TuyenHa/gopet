using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Dòng hai hàng chữ trong danh sách popup: tiêu đề đậm + dòng phụ mờ, cách dòng dưới
    /// bằng một vạch ngang mảnh. Cả dòng là một nút.
    ///
    /// <para><b>Cố ý KHÔNG có nền thẻ.</b> Khung danh sách bo góc nhưng
    /// <see cref="RectMask2D"/> chỉ cắt theo HÌNH CHỮ NHẬT, nên một dòng có nền đặc chạm
    /// mép trên/dưới sẽ tô đè lên bốn góc bo — nhìn ra thành viền đứt và góc trắng. Dòng
    /// trong suốt chỉ có chữ thì không có gì để tô đè, viền ngoài liền mạch.</para>
    ///
    /// <para>Khác <see cref="ShopItemRow"/>: kia là thẻ item có icon, chip chỉ số và nút
    /// giá; đây chỉ là chữ, dùng cho hộp thư và các danh sách chữ khác.</para>
    /// </summary>
    public sealed class PopupTextRow : MonoBehaviour
    {
        public const float Height = 44f;

        /// <summary>Chừa lề trong so với mép khung để chữ không dính viền.</summary>
        private const float SidePadding = 8f;

        /// <summary>
        /// Khe giữa đáy tiêu đề và đỉnh dòng phụ, chia đều hai bên đường giữa dòng.
        /// Trước đây hai dải chữ CHỒNG nhau 5px (đáy tiêu đề y=16, đỉnh dòng phụ y=21)
        /// nên chữ đè lên nhau trong hộp thư.
        /// </summary>
        private const float TextGap = 5f;
        private const float UnreadDotSize = 7f;

        /// <summary>Nhãn loại nằm sát mép phải; chữ phải chừa đúng chừng này cộng khe thở.</summary>
        private const float BadgeWidth = 30f;
        private const float BadgeHeight = 14f;

        /// <summary>Bo góc nhãn loại. Xem ghi chú ở chỗ dựng nhãn trước khi đổi số này.</summary>
        private const float BadgeRadius = 3f;
        private const float BadgeGap = 6f;

        private Text _title;
        private Text _subtitle;
        private GameObject _separator;
        private GameObject _unreadDot;
        private GameObject _badge;
        private Image _badgeFill;
        private Text _badgeLabel;

        public event Action Clicked;

        public static PopupTextRow Create(Transform parent, Font font)
        {
            var go = new GameObject("TextRow", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(0f, -Height);
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, Height);

            // Nền TRONG SUỐT: vẫn cần Image để bắt được cú chạm trên cả dòng, nhưng
            // không tô gì nên không đè lên góc bo của khung — xem mô tả class.
            go.GetComponent<Image>().color = Color.clear;

            var row = go.AddComponent<PopupTextRow>();
            row.Build(font);
            go.GetComponent<Button>().onClick.AddListener(() => row.Clicked?.Invoke());
            return row;
        }

        public void Bind(string title, string subtitle, bool unread)
        {
            _title.text = title ?? string.Empty;
            _subtitle.text = subtitle ?? string.Empty;
            // Thư chưa đọc: tiêu đề navy đậm + chấm đỏ. Đã đọc thì chìm xuống màu mờ.
            _title.color = unread ? PopupPalette.TextDark : PopupPalette.TextMuted;
            UiBuilder.SetFontStyle(_title, unread ? FontStyle.Bold : FontStyle.Normal);
            _unreadDot.SetActive(unread);
        }

        /// <summary>Dòng chữ không phải thư (nhiệm vụ…): tiêu đề đậm, không có chấm chưa đọc.</summary>
        public void Bind(string title, string subtitle)
        {
            Bind(title, subtitle, true);
            _unreadDot.SetActive(false);
        }

        /// <summary>Dải tiêu đề cố định khi dòng phụ nhiều hàng.</summary>
        private const float TitleBand = 22f;
        /// <summary>Chiều cao một hàng chữ cỡ 11 của dòng phụ.</summary>
        private const float SubtitleLineHeight = 14f;

        /// <summary>Chiều cao dòng khi dòng phụ có <paramref name="lines"/> hàng (tối thiểu
        /// bằng <see cref="Height"/>).</summary>
        public static float HeightFor(int lines) =>
            Mathf.Max(Height, TitleBand + Mathf.Max(1, lines) * SubtitleLineHeight + TextGap + 3f);

        /// <summary>
        /// Cho dòng phụ nhiều hàng (tiến độ nhiệm vụ: mỗi yêu cầu một hàng) và nới dòng
        /// cho vừa. Chuyển tiêu đề sang dải cố định ở đỉnh thay vì nửa trên dòng — nửa
        /// trên của một dòng cao sẽ đẩy tiêu đề trôi xuống giữa. Gọi SAU <c>Bind</c>.
        /// Trả về chiều cao mới để danh sách xếp dòng kế tiếp.
        /// </summary>
        public float FitMultilineSubtitle()
        {
            var left = _title.rectTransform.offsetMin.x;
            var right = _title.rectTransform.offsetMax.x;
            Place(_title.rectTransform, left, 1f, 1f, -TitleBand, 0f);
            _title.rectTransform.offsetMax = new Vector2(right, 0f);
            var subtitleTop = -TitleBand - TextGap * 0.5f;
            Place(_subtitle.rectTransform, left, 0f, 1f, 3f, subtitleTop);
            _subtitle.rectTransform.offsetMax = new Vector2(right, subtitleTop);

            var height = HeightFor(CountSubtitleLines());
            var rect = (RectTransform)transform;
            rect.offsetMin = new Vector2(rect.offsetMin.x, -height);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            return height;
        }

        /// <summary>Số hàng thật của dòng phụ: đếm '\n', rồi đo theo bề ngang (một yêu cầu
        /// có nhiều map có thể tự xuống hàng). Chưa có bề ngang thì chỉ đếm '\n'.</summary>
        private int CountSubtitleLines()
        {
            var text = _subtitle.text;
            if (string.IsNullOrEmpty(text)) return 1;
            var lines = text.Split('\n').Length;
            var width = _subtitle.rectTransform.rect.width;
            if (width <= 0f) return lines;
            var settings = _subtitle.GetGenerationSettings(new Vector2(width, 0f));
            var measured = _subtitle.cachedTextGeneratorForLayout.GetPreferredHeight(text, settings)
                / _subtitle.pixelsPerUnit;
            return Mathf.Max(lines, Mathf.CeilToInt(measured / SubtitleLineHeight - 0.2f));
        }

        /// <summary>Vạch ngăn dưới chân dòng. Dòng CUỐI phải tắt, không thì thừa một nét sát viền.</summary>
        public void SetSeparatorVisible(bool visible) => _separator.SetActive(visible);

        /// <summary>
        /// Nhãn loại ở mép phải ("BQT", "SK"…). <c>null</c>/rỗng là ẩn.
        ///
        /// <para>Chỉ chừa chỗ cho nhãn KHI có nhãn: thư bạn bè không có nhãn thì tiêu đề
        /// được dùng trọn bề ngang, không để lại một khoảng trống vô cớ.</para>
        /// </summary>
        public void SetBadge(string text, Color background, Color textColor)
        {
            var show = !string.IsNullOrEmpty(text);
            _badge.SetActive(show);
            if (show)
            {
                _badgeLabel.text = text;
                _badgeLabel.color = textColor;
                _badgeFill.color = background;
            }

            var right = show ? -(SidePadding + BadgeWidth + BadgeGap) : -SidePadding;
            _title.rectTransform.offsetMax = new Vector2(right, _title.rectTransform.offsetMax.y);
            _subtitle.rectTransform.offsetMax =
                new Vector2(right, _subtitle.rectTransform.offsetMax.y);
        }

        private void Build(Font font)
        {
            var textLeft = SidePadding + UnreadDotSize + 5f;

            _title = UiBuilder.MakeText(transform, font, "Title", 13, false);
            _title.alignment = TextAnchor.LowerLeft;
            // Chữ ở đây đến từ người khác (tiêu đề/tóm tắt thư) — tắt rich text, xem
            // LetterDetailView.
            _title.supportRichText = false;
            _title.horizontalOverflow = HorizontalWrapMode.Wrap;
            _title.verticalOverflow = VerticalWrapMode.Truncate;
            Place(_title.rectTransform, textLeft, 0.5f, 1f, TextGap * 0.5f, 0f);

            _subtitle = UiBuilder.MakeText(transform, font, "Subtitle", 11, false);
            _subtitle.alignment = TextAnchor.UpperLeft;
            _subtitle.supportRichText = false;
            _subtitle.color = PopupPalette.TextMuted;
            _subtitle.horizontalOverflow = HorizontalWrapMode.Wrap;
            _subtitle.verticalOverflow = VerticalWrapMode.Truncate;
            Place(_subtitle.rectTransform, textLeft, 0f, 0.5f, 3f, -TextGap * 0.5f);

            _unreadDot = new GameObject("Unread", typeof(RectTransform), typeof(Image));
            _unreadDot.transform.SetParent(transform, false);
            var dot = (RectTransform)_unreadDot.transform;
            dot.anchorMin = dot.anchorMax = new Vector2(0f, 0.5f);
            dot.pivot = new Vector2(0.5f, 0.5f);
            dot.sizeDelta = new Vector2(UnreadDotSize, UnreadDotSize);
            dot.anchoredPosition = new Vector2(SidePadding + UnreadDotSize * 0.5f, 0f);
            var dotImage = _unreadDot.GetComponent<Image>();
            dotImage.sprite = CircleUiSprite.Get();
            dotImage.color = new Color(0.92f, 0.28f, 0.28f, 1f);
            dotImage.raycastTarget = false;

            _badge = new GameObject("TypeBadge", typeof(RectTransform), typeof(Image));
            _badge.transform.SetParent(transform, false);
            var badgeRect = (RectTransform)_badge.transform;
            badgeRect.anchorMin = badgeRect.anchorMax = new Vector2(1f, 0.5f);
            badgeRect.pivot = new Vector2(1f, 0.5f);
            badgeRect.sizeDelta = new Vector2(BadgeWidth, BadgeHeight);
            badgeRect.anchoredPosition = new Vector2(-SidePadding, 0f);
            _badgeFill = _badge.GetComponent<Image>();
            // Bo 3: biên 9-slice khi đó là 3+2 = 5 đơn vị, hai biên dọc cộng lại 10 < 14
            // (chiều cao nhãn) nên Unity KHÔNG phải co biên. Bo 10 mặc định hay bo 7 đều
            // vượt mức đó: Unity co biên dọc mà giữ biên ngang, góc kéo thành bầu dục và
            // đường viền gãy khúc — cùng cái bẫy đã gặp ở ShopChip.
            RoundedUiSprite.Apply(_badgeFill, BadgeRadius);
            _badgeFill.raycastTarget = false;

            _badgeLabel = UiBuilder.MakeText(_badge.transform, font, "Label", 9, true);
            _badgeLabel.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(_badgeLabel, FontStyle.Bold);
            _badgeLabel.raycastTarget = false;
            _badge.SetActive(false);

            _separator = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            _separator.transform.SetParent(transform, false);
            var line = (RectTransform)_separator.transform;
            line.anchorMin = new Vector2(0f, 0f);
            line.anchorMax = new Vector2(1f, 0f);
            line.pivot = new Vector2(0.5f, 0f);
            line.offsetMin = new Vector2(SidePadding, 0f);
            line.offsetMax = new Vector2(-SidePadding, 1f);
            var lineImage = _separator.GetComponent<Image>();
            lineImage.color = PopupPalette.Hairline;
            lineImage.raycastTarget = false;
        }

        /// <summary>Neo một dòng chữ theo dải dọc [<paramref name="bottom"/>..<paramref name="top"/>] của dòng.</summary>
        private void Place(RectTransform rect, float left, float bottom, float top,
            float offsetBottom, float offsetTop)
        {
            rect.anchorMin = new Vector2(0f, bottom);
            rect.anchorMax = new Vector2(1f, top);
            rect.offsetMin = new Vector2(left, offsetBottom);
            rect.offsetMax = new Vector2(-SidePadding, offsetTop);
        }
    }
}
