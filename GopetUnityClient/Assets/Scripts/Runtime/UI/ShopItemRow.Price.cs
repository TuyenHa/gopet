using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nút giá bên phải thẻ item: viên thuốc xanh lá, icon đồng vàng + số tiền.
    ///
    /// <para>Tách khỏi <c>ShopItemRow.Build.cs</c> để mỗi file dưới 200 dòng.</para>
    /// </summary>
    public sealed partial class ShopItemRow
    {
        private void BuildPriceButton(Transform parent)
        {
            var go = new GameObject("Price", typeof(RectTransform), typeof(Image),
                typeof(Button), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(PriceWidth, PriceHeight);
            rect.anchoredPosition = new Vector2(-6f, 0f);

            _priceFill = RoundedBorder.Apply(go, PriceRadius, PopupPalette.PriceGreen,
                PopupPalette.PriceGreenEdge);
            _priceEdge = go.GetComponent<Image>();

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(4, 5, 0, 0);
            layout.spacing = 3f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            AddCoin(go.transform);

            _price = UiBuilder.MakeText(go.transform, _font, "Amount", 12, false);
            _price.color = Color.white;
            _price.fontStyle = FontStyle.Bold;
            _price.alignment = TextAnchor.MiddleCenter;

            // Đơn vị tiền lạ ("100 điểm hoa ngọc") dài gấp mấy lần "20"; cỡ chữ cố
            // định thì chuỗi tràn hẳn ra ngoài viên thuốc xanh. Cho chữ co lại trong
            // đúng chỗ còn trống thay vì nới nút cho lệch cả cột giá.
            _price.horizontalOverflow = HorizontalWrapMode.Wrap;
            _price.verticalOverflow = VerticalOverflow;
            _price.resizeTextForBestFit = true;
            _price.resizeTextMinSize = 6;
            _price.resizeTextMaxSize = 11;

            var priceText = _price.gameObject.AddComponent<LayoutElement>();
            priceText.preferredHeight = PriceHeight;
            priceText.preferredWidth = 0f;
            priceText.flexibleWidth = 1f;

            _priceButton = go.GetComponent<Button>();
            // Tự tô màu theo trạng thái mua được / không đủ tiền, nên phải TẮT
            // ColorTint: mặc định Selectable nhân màu disabled lên ảnh viền và đè
            // mất màu vừa đặt.
            _priceButton.transition = Selectable.Transition.None;
            _priceButton.onClick.AddListener(() => Buy?.Invoke());
        }

        private static void AddCoin(Transform parent)
        {
            var sprite = HudSkin.Get(HudSkin.CoinGold);
            if (sprite == null) return;

            var go = new GameObject("Coin", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            var element = go.GetComponent<LayoutElement>();
            element.preferredWidth = 11f;
            element.preferredHeight = 11f;
        }
    }
}
