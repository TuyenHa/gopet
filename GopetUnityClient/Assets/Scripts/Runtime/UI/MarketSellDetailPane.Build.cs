using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Phần dựng hình của <see cref="MarketSellDetailPane"/>. Tách khỏi file chính để mỗi
    /// file dưới 200 dòng — cùng khuôn <c>MarketListingRowView.Build.cs</c>.
    ///
    /// <para>Toạ độ tính từ TRÊN pane (283 ref-unit cao, xem <c>GamePopupFrame.Content</c>
    /// không băng chân: 310 − 18 − 9). Vùng dưới icon/tên chừa cố định cho 2 ô nhập + dòng
    /// thực nhận + dòng lỗi + nút — kể cả khi ô số lượng ẩn, để đổi món không phải tính
    /// lại vị trí từng phần tử.</para>
    /// </summary>
    public sealed partial class MarketSellDetailPane
    {
        private const float Padding = 8f;
        private const float IconSize = 48f;
        private const float TextLeft = IconSize + 14f;
        private const float DescTop = 64f;
        private const float DescBottom = 154f;
        private const float PriceTop = 137f;
        private const float QtyTop = 169f;
        private const float NetTop = 201f;
        private const float ErrorTop = 223f;
        private const float ButtonTop = 243f;
        private const float FieldHeight = 24f;
        private const float ButtonHeight = 32f;

        private void Build(Font font)
        {
            RoundedBorder.Apply(gameObject, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            _placeholder = UiBuilder.MakeText(transform, font, "Placeholder", 11, true);
            _placeholder.alignment = TextAnchor.MiddleCenter;
            _placeholder.color = PopupPalette.TextMuted;
            _placeholder.text = "Chọn một món bên trái để đăng bán.";
            _placeholder.raycastTarget = false;

            _content = new GameObject("Content", typeof(RectTransform));
            _content.transform.SetParent(transform, false);
            UiBuilder.Stretch((RectTransform)_content.transform);

            BuildIcon(font);
            BuildDesc(font);
            BuildFields(font);
            BuildNetAndError(font);
            BuildButton(font);
        }

        private void BuildIcon(Font font)
        {
            var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(_content.transform, false);
            var rect = (RectTransform)frame.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            rect.anchoredPosition = new Vector2(Padding, -Padding);
            RoundedBorder.Apply(frame, 6f, Color.white, PopupPalette.Hairline);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(frame.transform, false);
            UiBuilder.Stretch((RectTransform)iconGo.transform);
            _icon = iconGo.GetComponent<RawImage>();
            _icon.raycastTarget = false;

            _name = StarNameLabel.Create(_content.transform, font, 12, 11f);
            var nameText = _name.Label;
            nameText.color = PopupPalette.TextDark;
            UiBuilder.SetFontStyle(nameText, FontStyle.Bold);
            nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
            nameText.verticalOverflow = VerticalWrapMode.Truncate;
            var nameRect = _name.Rect;
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.pivot = new Vector2(0f, 1f);
            nameRect.offsetMin = new Vector2(TextLeft, -Padding - 40f);
            nameRect.offsetMax = new Vector2(-Padding, -Padding);
        }

        private void BuildDesc(Font font)
        {
            var viewport = new GameObject("Desc", typeof(RectTransform), typeof(Image),
                typeof(RectMask2D), typeof(ScrollRect));
            viewport.transform.SetParent(_content.transform, false);
            var rect = (RectTransform)viewport.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(Padding, DescBottom);
            rect.offsetMax = new Vector2(-Padding, -DescTop);
            viewport.GetComponent<Image>().color = Color.clear;

            _descText = UiBuilder.MakeText(viewport.transform, font, "Text", 10, false);
            _descText.alignment = TextAnchor.UpperLeft;
            _descText.color = PopupPalette.TextMuted;
            _descText.supportRichText = false;
            _descText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _descText.verticalOverflow = VerticalWrapMode.Overflow;
            var bodyRect = _descText.rectTransform;
            bodyRect.anchorMin = new Vector2(0f, 1f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.pivot = new Vector2(0.5f, 1f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            var fitter = _descText.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _descScroll = viewport.GetComponent<ScrollRect>();
            _descScroll.viewport = rect;
            _descScroll.content = bodyRect;
            _descScroll.horizontal = false;
            _descScroll.vertical = true;
            _descScroll.movementType = ScrollRect.MovementType.Clamped;
            _descScroll.scrollSensitivity = 20f;
        }

        private void BuildFields(Font font)
        {
            _priceInput = PopupField.Create(_content.transform, font, "Giá bán:", PriceTop, FieldHeight, 10);
            _priceInput.contentType = InputField.ContentType.IntegerNumber;
            _priceInput.lineType = InputField.LineType.SingleLine;
            _priceInput.onValueChanged.AddListener(_ => UpdateComputed());
            _priceInput.onEndEdit.AddListener(_ => ClampPriceOnEndEdit());

            _qtyInput = PopupField.Create(_content.transform, font, "Số lượng:", QtyTop, FieldHeight, 9);
            _qtyInput.contentType = InputField.ContentType.IntegerNumber;
            _qtyInput.lineType = InputField.LineType.SingleLine;
            _qtyInput.onValueChanged.AddListener(_ => UpdateComputed());
            _qtyInput.onEndEdit.AddListener(_ => ClampCountOnEndEdit());
            // Ẩn/hiện cả DÒNG (nhãn + ô), không chỉ InputField — nhãn "Số lượng:" đứng
            // riêng, ẩn mỗi ô nhập vẫn để lại nhãn trơ trọi.
            _qtyRow = _qtyInput.transform.parent.gameObject;

            ShrinkField(_priceInput);
            ShrinkField(_qtyInput);
        }

        /// <summary>Chữ nhãn + chữ nhập nhỏ hơn cỡ 13 mặc định của <see cref="PopupField"/>
        /// cho vừa popup nhỏ — chỉnh tại chỗ để không ảnh hưởng popup khác.</summary>
        private static void ShrinkField(InputField input)
        {
            const int size = 11;
            input.textComponent.fontSize = size;
            var caption = input.transform.parent.Find("Label")?.GetComponent<Text>();
            if (caption != null) caption.fontSize = size;
        }

        private void BuildNetAndError(Font font)
        {
            _netText = UiBuilder.MakeText(_content.transform, font, "Net", 11, false);
            _netText.color = PopupPalette.TextDark;
            _netText.raycastTarget = false;
            UiBuilder.PlaceRow(_netText.rectTransform, NetTop, 18f, 16f);

            _errorText = UiBuilder.MakeText(_content.transform, font, "Error", 10, false);
            _errorText.color = new Color(0.82f, 0.2f, 0.2f, 1f);
            _errorText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _errorText.verticalOverflow = VerticalWrapMode.Truncate;
            _errorText.raycastTarget = false;
            UiBuilder.PlaceRow(_errorText.rectTransform, ErrorTop, 16f, 16f);
        }

        private void BuildButton(Font font)
        {
            var go = new GameObject("SellButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_content.transform, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, ButtonTop, ButtonHeight, 16f);

            var image = go.GetComponent<Image>();
            var label = UiBuilder.MakeText(go.transform, font, "Label", 12, true);
            RoundedUiSprite.Apply(image);
            image.color = PopupPalette.ButtonBlue;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = "Đăng bán";

            _sellButton = go.GetComponent<Button>();
            _sellButton.interactable = false;
            _sellButton.onClick.AddListener(Submit);
        }
    }
}
