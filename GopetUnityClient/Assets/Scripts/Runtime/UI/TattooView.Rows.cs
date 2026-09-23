using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một dòng ô xăm của <see cref="TattooView"/>: số ô trong vòng tròn, tên + trạng
    /// thái, hai nút Nâng/Xoá khi ô đã có hình xăm, vạch kẻ ngang dưới chân.
    /// </summary>
    public sealed partial class TattooView
    {
        private const float RowHeight = 46f;
        private const float BadgeSize = 28f;
        private const float TextLeft = 48f;
        private const float RowButtonWidth = 54f;
        private const float RowButtonHeight = 26f;
        private static readonly Color DangerRed = new Color(0.90f, 0.36f, 0.33f, 1f);
        private static readonly Color DangerRedEdge = new Color(0.76f, 0.26f, 0.24f, 1f);

        private void BuildRow(Transform parent, TattooSlot slot, int index, bool separator)
        {
            var go = new GameObject($"Tattoo:{slot.Position}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, RowHeight);
            rect.anchoredPosition = new Vector2(0f, -index * RowHeight);

            var hasTattoo = slot.TattooId != 0;
            BuildSlotBadge(go.transform, slot.Position, hasTattoo);

            // Chừa chỗ cho hai nút bên phải khi ô đã xăm.
            var right = hasTattoo ? RowButtonWidth * 2f + 16f : 8f;
            var name = UiBuilder.MakeText(go.transform, _font, "Name", 14, false);
            name.text = slot.Name;
            name.color = PopupPalette.TextDark;
            UiBuilder.SetFontStyle(name, FontStyle.Bold);
            PlaceText(name.rectTransform, 5f, 20f, right);

            var status = UiBuilder.MakeText(go.transform, _font, "Status", 11, false);
            status.text = hasTattoo ? $"Ô {slot.Position} · Đã có hình xăm" : $"Ô {slot.Position} · Chưa có hình xăm";
            status.color = PopupPalette.TextMuted;
            PlaceText(status.rectTransform, 25f, 16f, right);

            if (hasTattoo)
            {
                var id = slot.TattooId;
                PlaceRowButton(MakePillButton(go.transform, "Nâng", 12, PopupPalette.PriceGreen,
                    PopupPalette.PriceGreenEdge, () => EnchantRequested?.Invoke(id)), RowButtonWidth * 2f + 12f);
                PlaceRowButton(MakePillButton(go.transform, "Xoá", 12, DangerRed,
                    DangerRedEdge, () => RemoveRequested?.Invoke(id)), RowButtonWidth + 6f);
            }

            if (separator) BuildSeparator(go.transform);
        }

        /// <summary>Số ô trong vòng tròn: xanh đậm khi đã xăm, nhạt khi còn trống.</summary>
        private void BuildSlotBadge(Transform row, int position, bool filled)
        {
            var go = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(BadgeSize, BadgeSize);
            rect.anchoredPosition = new Vector2(10f, 0f);
            var image = go.GetComponent<Image>();
            image.sprite = CircleUiSprite.Get();
            image.color = filled ? PopupPalette.ButtonBlue : PopupPalette.TabInactive;
            image.raycastTarget = false;

            var label = UiBuilder.MakeText(go.transform, _font, "Number", 13, true);
            label.text = position.ToString();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = filled ? Color.white : PopupPalette.TextDark;
            label.raycastTarget = false;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
        }

        private static void BuildSeparator(Transform row)
        {
            var go = new GameObject("Separator", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = new Vector2(8f, 0f);
            rect.offsetMax = new Vector2(-8f, 1f);
            var image = go.GetComponent<Image>();
            image.color = PopupPalette.Hairline;
            image.raycastTarget = false;
        }

        private static void PlaceText(RectTransform rect, float top, float height, float right)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(TextLeft, -(top + height));
            rect.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Neo nút theo mép phải dòng; <paramref name="right"/> là khoảng từ mép phải tới mép trái nút.</summary>
        private static void PlaceRowButton(Button button, float right)
        {
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(RowButtonWidth, RowButtonHeight);
            rect.anchoredPosition = new Vector2(-right, 0f);
        }
    }
}
