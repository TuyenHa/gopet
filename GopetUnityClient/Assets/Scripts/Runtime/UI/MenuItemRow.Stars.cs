using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Sao của tên pet trong dòng menu ("Chọn pet"…). <c>MenuItemInfo</c> đổi tag
    /// <c>(sao)</c>/<c>(saoden)</c> thành ★/☆ lúc parse; font vẽ ☆ thành sao rỗng xám. Ở đây
    /// bỏ ký tự sao khỏi chữ và vẽ icon sao vàng ngay sau tên — cùng ảnh với
    /// <see cref="StarNameLabel"/> (MỌI sao, đạt lẫn chưa đạt, đều vàng theo thiết kế).
    /// </summary>
    public sealed partial class MenuItemRow
    {
        private const char FilledStar = '★';
        private const char EmptyStar = '☆';
        private const int MaxStars = 5;

        private Image[] _stars;
        private int _starCount;

        /// <summary>Tách sao khỏi tiêu đề; gọi sau khi gán chữ.</summary>
        private void ApplyTitleStars()
        {
            var text = _title.text ?? string.Empty;
            var count = 0;
            if (text.IndexOf(FilledStar) >= 0 || text.IndexOf(EmptyStar) >= 0)
            {
                var sb = new System.Text.StringBuilder(text.Length);
                foreach (var c in text)
                {
                    if (c == FilledStar || c == EmptyStar) count++;
                    else sb.Append(c);
                }
                text = sb.ToString().TrimEnd();
            }
            _title.text = text;
            _starCount = Mathf.Min(count, MaxStars);
            LayoutTitleStars();
        }

        /// <summary>Đặt sao ngay sau chữ. Gọi lại khi cỡ chữ đổi (thẻ gọn/thẻ thường).</summary>
        private void LayoutTitleStars()
        {
            if (_title == null) return;
            var sprite = _starCount > 0 ? StarNameLabel.Star() : null;
            if (sprite == null && _stars == null) return;
            EnsureStars();

            var size = Mathf.Round(_title.fontSize * 0.8f);
            var width = _title.rectTransform.rect.width;
            var textWidth = width > 0f ? Mathf.Min(_title.preferredWidth, width) : _title.preferredWidth;
            for (var i = 0; i < _stars.Length; i++)
            {
                var show = sprite != null && i < _starCount;
                _stars[i].gameObject.SetActive(show);
                if (!show) continue;
                _stars[i].sprite = sprite;
                var rect = _stars[i].rectTransform;
                rect.sizeDelta = new Vector2(size, size);
                rect.anchoredPosition = new Vector2(textWidth + 4f + i * (size + 1f), 0f);
            }
        }

        private void EnsureStars()
        {
            if (_stars != null) return;
            _stars = new Image[MaxStars];
            for (var i = 0; i < MaxStars; i++)
            {
                var go = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_title.transform, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                var image = go.GetComponent<Image>();
                image.preserveAspect = true;
                image.raycastTarget = false;
                go.SetActive(false);
                _stars[i] = image;
            }
        }
    }
}
