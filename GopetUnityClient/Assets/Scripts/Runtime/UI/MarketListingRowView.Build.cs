using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Phần dựng hình của <see cref="MarketListingRowView"/>: icon trong khung sắc nét,
    /// 3 dòng chữ, khoảng trống nút hành động và vạch ngăn. Tách khỏi file chính để mỗi
    /// file dưới 200 dòng — cùng khuôn <c>ShopItemRow.Build.cs</c>.
    /// </summary>
    public sealed partial class MarketListingRowView
    {
        private void Build(Transform parent)
        {
            BuildIcon(parent);
            BuildTexts(parent);
            BuildActionSlot(parent);
            BuildSeparator(parent);
        }

        /// <summary>
        /// Icon 36×36 trong khung <see cref="RoundedBorder"/> (viền 2 lớp, KHÔNG
        /// <see cref="Outline"/>) — yêu cầu viền sắc nét của popup này, khác
        /// <see cref="ShopItemRow"/> chỉ có RawImage trần.
        /// </summary>
        private void BuildIcon(Transform parent)
        {
            var frame = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(parent, false);

            var rect = (RectTransform)frame.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            rect.anchoredPosition = new Vector2(IconLeft, 0f);

            RoundedBorder.Apply(frame, 4f, Color.white, PopupPalette.Hairline);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(frame.transform, false);
            UiBuilder.Stretch((RectTransform)iconGo.transform);
            _icon = iconGo.GetComponent<RawImage>();
        }

        private void BuildTexts(Transform parent)
        {
            _titleName = StarNameLabel.Create(parent, _font, 11, 10f);
            var title = _titleName.Label;
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.color = PopupPalette.TextDark;
            title.horizontalOverflow = HorizontalWrapMode.Wrap; // Overflow sẽ đè lên icon sao
            title.verticalOverflow = VerticalWrapMode.Truncate;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 8;
            title.resizeTextMaxSize = 11;
            Place(_titleName.Rect, 2f, 15f);

            BuildPriceRow(parent);

            _sellerOrTime = UiBuilder.MakeText(parent, _font, "SellerOrTime", 9, false);
            _sellerOrTime.color = PopupPalette.TextMuted;
            _sellerOrTime.horizontalOverflow = HorizontalWrapMode.Overflow;
            _sellerOrTime.verticalOverflow = VerticalWrapMode.Truncate;
            Place(_sellerOrTime.rectTransform, 31f, 13f);
        }

        private void BuildPriceRow(Transform parent)
        {
            var go = new GameObject("Price", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            Place((RectTransform)go.transform, 17f, 13f);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 3f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var coin = HudSkin.Get(HudSkin.CoinGold);
            if (coin != null)
            {
                var coinGo = new GameObject("Coin", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                coinGo.transform.SetParent(go.transform, false);
                var image = coinGo.GetComponent<Image>();
                image.sprite = coin;
                image.preserveAspect = true;
                var element = coinGo.GetComponent<LayoutElement>();
                element.preferredWidth = 10f;
                element.preferredHeight = 10f;
            }

            _price = UiBuilder.MakeText(go.transform, _font, "Amount", 10, false);
            UiBuilder.SetFontStyle(_price, FontStyle.Bold);
            _price.color = PopupPalette.TextDark;
            var priceLe = _price.gameObject.AddComponent<LayoutElement>();
            priceLe.preferredHeight = 13f;
        }

        /// <summary>Một dòng chữ neo mép trên, chừa chỗ icon trái và <see cref="ActionSlot"/> phải.</summary>
        private static void Place(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(TextLeft, -(top + height));
            rect.offsetMax = new Vector2(-(ActionWidth + ActionRight), -top);
        }

        /// <summary>
        /// Nới/thu <see cref="ActionSlot"/> (mặc định <see cref="ActionWidth"/>) — vd 2 nút
        /// đặt cạnh nhau cần rộng hơn 1 nút. 3 dòng chữ co mép phải theo để không đè nút.
        /// </summary>
        public void SetActionWidth(float width)
        {
            _actionSlot.sizeDelta = new Vector2(width, Height);
            var right = -(width + ActionRight);
            foreach (var rect in new[] { _titleName.Rect, (RectTransform)_price.transform.parent,
                         _sellerOrTime.rectTransform })
                rect.offsetMax = new Vector2(right, rect.offsetMax.y);
        }

        private void BuildActionSlot(Transform parent)
        {
            var go = new GameObject("ActionSlot", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(ActionWidth, Height);
            rect.anchoredPosition = new Vector2(-ActionRight, 0f);

            _actionSlot = rect;
        }

        private void BuildSeparator(Transform parent)
        {
            _separator = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            _separator.transform.SetParent(parent, false);

            var rect = (RectTransform)_separator.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = new Vector2(4f, 0f);
            rect.offsetMax = new Vector2(-4f, 2f);

            var image = _separator.GetComponent<Image>();
            image.color = PopupPalette.Border;
            image.raycastTarget = false;
        }
    }
}
