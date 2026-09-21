using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Phần dựng hình của <see cref="ShopItemRow"/>: icon trái, hai dòng chữ, hàng
    /// chip chỉ số ở đáy và nút giá bên phải.
    ///
    /// <para>Tách khỏi file chính để mỗi file dưới 200 dòng — cùng khuôn với
    /// <c>CharacterHubPopupView.Chrome.cs</c>.</para>
    /// </summary>
    public sealed partial class ShopItemRow
    {
        private const float IconSize = 46f;
        private const float TextLeft = 58f;
        // 57×21: đo trên ảnh mẫu (128×48 px, hệ số 0.4415), góc bo 4.5.
        private const float PriceWidth = 57f;
        private const float PriceHeight = 21f;
        private const float PriceRadius = 4.5f;
        private const float DescTop = 22f;

        public static ShopItemRow Create(Transform parent, Font font)
        {
            var go = new GameObject("ShopItemRow", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // Neo trải ngang, ghim mép trên: bề rộng bám vùng chứa, chiều cao cố định.
            // Để anchor mặc định thì dòng rộng 0 — chữ vẫn vẽ (Text tự tràn) nhưng
            // không nút nào bên trong nhận được chạm.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, Height);

            var row = go.AddComponent<ShopItemRow>();
            row._font = font;
            row.Build(go.transform);
            return row;
        }

        private void Build(Transform parent)
        {
            BuildIcon(parent);
            BuildTexts(parent);
            BuildChipRow(parent);
            BuildPriceButton(parent);
            BuildSeparator(parent);
        }

        private void BuildIcon(Transform parent)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            rect.anchoredPosition = new Vector2(6f, 0f);

            _icon = go.GetComponent<RawImage>();
        }

        private void BuildTexts(Transform parent)
        {
            // MakeText bật Overflow cả hai chiều. Giữ nguyên là chữ dài chạy đè lên
            // nút giá và tràn khỏi thẻ (thấy rõ ở tab Thức ăn, mô tả dài cả câu).
            // Wrap để xuống dòng, Truncate để phần không còn chỗ bị cắt chứ không tràn.
            _title = UiBuilder.MakeText(parent, _font, "Title", 14, false);
            _title.fontStyle = FontStyle.Bold;
            _title.color = PopupPalette.TextDark;
            _title.horizontalOverflow = HorizontalWrapMode.Wrap;
            _title.verticalOverflow = VerticalOverflow;
            // Tên kèm đủ ba yêu cầu chỉ số ("Mũ Sắt (Yêu cầu 25 str, 20 agi, 20 int)")
            // dài gấp rưỡi chỗ có; co chữ lại còn đọc được hết, cắt ngang thì mất chữ.
            _title.resizeTextForBestFit = true;
            _title.resizeTextMinSize = 10;
            _title.resizeTextMaxSize = 14;
            Place(_title.rectTransform, 4f, 18f);

            _description = UiBuilder.MakeText(parent, _font, "Description", 11, false);
            _description.color = PopupPalette.TextMuted;
            _description.alignment = TextAnchor.UpperLeft;
            _description.horizontalOverflow = HorizontalWrapMode.Wrap;
            _description.verticalOverflow = VerticalOverflow;
            PlaceDescription(false);
        }

        private void BuildChipRow(Transform parent)
        {
            // RectMask2D là lưới an toàn: chip nào không còn chỗ thì bị cắt gọn ở mép
            // vùng chip, thay vì đè lên nút giá và tràn khỏi viền khung danh sách.
            var go = new GameObject("Stats", typeof(RectTransform),
                typeof(HorizontalLayoutGroup), typeof(RectMask2D));
            go.transform.SetParent(parent, false);

            _chips = (RectTransform)go.transform;
            _chips.anchorMin = new Vector2(0f, 0f);
            _chips.anchorMax = new Vector2(1f, 0f);
            _chips.pivot = new Vector2(0f, 0f);
            _chips.offsetMin = new Vector2(TextLeft, 6f);
            _chips.offsetMax = new Vector2(-(PriceWidth + 12f), 23f);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            // BẬT: hàng chip đặt bề rộng cho từng chip, lấy từ LayoutElement mà
            // ShopChip tự tính. Tắt nó đi thì hàng chip xếp chỗ theo bề rộng HIỆN TẠI
            // của chip — lúc đó vẫn là 0 — nên cả ba chip chồng lên nhau ở mép trái.
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void BuildSeparator(Transform parent)
        {
            _separator = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            _separator.transform.SetParent(parent, false);

            var rect = (RectTransform)_separator.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = new Vector2(8f, 0f);
            rect.offsetMax = new Vector2(-8f, 1f);

            var image = _separator.GetComponent<Image>();
            image.color = PopupPalette.Hairline;
            image.raycastTarget = false;
        }

        private const VerticalWrapMode VerticalOverflow = VerticalWrapMode.Truncate;

        /// <summary>
        /// Chiều cao dành cho mô tả. Item có chip chỉ số thì chỉ còn một dòng vì hàng
        /// chip chiếm đáy thẻ; item không chip (thức ăn, item xài liền) được lấy hết
        /// chỗ trống xuống sát đáy — mô tả của chúng thường dài cả câu.
        /// </summary>
        private void PlaceDescription(bool hasChips)
        {
            Place(_description.rectTransform, DescTop,
                hasChips ? 16f : Height - DescTop - 8f);
        }

        /// <summary>Một dòng chữ neo mép trên, chừa chỗ icon bên trái và nút giá bên phải.</summary>
        private static void Place(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(TextLeft, -(top + height));
            rect.offsetMax = new Vector2(-(PriceWidth + 12f), -top);
        }
    }
}
