using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Ba thẻ chỉ số STR/AGI/INT của <see cref="PetGymView"/>: chip tên màu riêng, mô tả,
    /// giá trị lớn và nút cộng xanh lá kiểu nút giá của Cửa hàng.
    /// </summary>
    public sealed partial class PetGymView
    {
        private const float CardGap = 10f;
        private const float CardBottom = PointsHeight + 10f;
        private const float StatButtonHeight = 34f;

        private static readonly string[] StatNames = { "STR", "AGI", "INT" };
        private static readonly string[] StatHints = { "Sức mạnh", "Nhanh nhẹn", "Trí tuệ" };
        private static readonly Color[] StatColors =
        {
            new Color(0.93f, 0.40f, 0.33f, 1f),
            new Color(0.25f, 0.70f, 0.45f, 1f),
            new Color(0.50f, 0.42f, 0.90f, 1f)
        };

        private readonly Text[] _values = new Text[3];
        private readonly Button[] _buttons = new Button[3];
        private readonly Image[] _buttonEdges = new Image[3];
        private readonly Image[] _buttonFills = new Image[3];

        private void BuildCards(Transform parent, Font font, float contentWidth, float top)
        {
            var width = (contentWidth - CardGap * 2f) / 3f;
            for (var i = 0; i < StatNames.Length; i++)
            {
                var go = new GameObject("Card_" + StatNames[i], typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.offsetMin = new Vector2(i * (width + CardGap), CardBottom);
                rect.offsetMax = new Vector2(i * (width + CardGap) + width, -top);
                RoundedBorder.Apply(go, 12f, PopupPalette.ListBg, PopupPalette.Hairline, 1f);

                BuildStatChip(go.transform, font, i);

                var hint = UiBuilder.MakeText(go.transform, font, "Hint", 12, false);
                hint.text = StatHints[i];
                hint.alignment = TextAnchor.MiddleCenter;
                hint.color = PopupPalette.TextMuted;
                PlaceTop(hint.rectTransform, 36f, 16f);

                var value = UiBuilder.MakeText(go.transform, font, "Value", 28, false);
                value.alignment = TextAnchor.MiddleCenter;
                value.color = PopupPalette.TextDark;
                UiBuilder.SetFontStyle(value, FontStyle.Bold);
                PlaceTop(value.rectTransform, 54f, 36f);
                _values[i] = value;

                BuildStatButton(go.transform, font, i);
            }
        }

        private static void BuildStatChip(Transform card, Font font, int index)
        {
            var go = new GameObject("Chip", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(card, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(58f, 22f);
            rect.anchoredPosition = new Vector2(0f, -9f);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 11f);
            image.color = StatColors[index];
            image.raycastTarget = false;

            var label = UiBuilder.MakeText(go.transform, font, "Label", 13, true);
            label.text = StatNames[index];
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
        }

        private void BuildStatButton(Transform card, Font font, int index)
        {
            var go = new GameObject("Add_" + StatNames[index], typeof(RectTransform),
                typeof(Image), typeof(Button));
            go.transform.SetParent(card, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(10f, 10f);
            rect.offsetMax = new Vector2(-10f, 10f + StatButtonHeight);
            // Viền dày 2 màu sẫm làm "gờ" nút, giống nút giá của Cửa hàng.
            _buttonFills[index] = RoundedBorder.Apply(go, StatButtonHeight * 0.5f,
                PopupPalette.PriceGreen, PopupPalette.PriceGreenEdge, 2f);
            _buttonEdges[index] = go.GetComponent<Image>();

            var label = UiBuilder.MakeText(go.transform, font, "Label", 15, true);
            label.text = "+ " + StatNames[index];
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);

            var button = go.GetComponent<Button>();
            button.targetGraphic = _buttonEdges[index];
            // Màu nút tự đổi ở SetButtonsEnabled; tint mặc định chỉ nhuộm lớp viền
            // nên nút xám sẽ loang lổ.
            button.transition = Selectable.Transition.None;
            var stat = (sbyte)index;
            button.onClick.AddListener(() =>
            {
                if (_points > 0) StatSelected?.Invoke(stat);
            });
            _buttons[index] = button;
        }

        private void SetValue(int index, int value)
        {
            if (_values[index] != null) _values[index].text = value.ToString();
        }

        /// <summary>Hết điểm thì nút chuyển xám và khoá, không để bấm mà không có phản hồi.</summary>
        private void SetButtonsEnabled(bool enabled)
        {
            for (var i = 0; i < _buttons.Length; i++)
            {
                if (_buttons[i] == null) continue;
                _buttons[i].interactable = enabled;
                _buttonFills[i].color = enabled ? PopupPalette.PriceGreen : PopupPalette.PriceLocked;
                _buttonEdges[i].color = enabled ? PopupPalette.PriceGreenEdge : PopupPalette.PriceLockedEdge;
            }
        }
    }
}
