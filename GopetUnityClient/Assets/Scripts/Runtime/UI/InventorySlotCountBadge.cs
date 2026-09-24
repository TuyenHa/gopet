using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Số lượng vật phẩm xếp chồng ở góc dưới-phải ô lưới: chữ vàng viền đen, nổi trên mọi
    /// icon. Số lấy từ tên server gửi — <c>Item.getName</c> ghi "Tên xN" cho món xếp chồng
    /// (có thể kèm chữ khoá giao dịch phía sau), món không xếp chồng thì không có.
    /// </summary>
    public static class InventorySlotCountBadge
    {
        private const string ChildName = "Count";
        private static readonly Regex CountPattern = new Regex(@"\sx(\d+)\b", RegexOptions.RightToLeft);
        private static readonly Color CountColor = new Color(1f, 0.86f, 0.16f, 1f);

        /// <summary>Hiện/ẩn số lượng cho <paramref name="slot"/> theo <paramref name="title"/>.</summary>
        public static void Bind(GameObject slot, string title)
        {
            var label = slot.transform.Find(ChildName)?.GetComponent<Text>();
            // Chỉ 1 cái thì không hiện số.
            if (!TryParseCount(title, out var count) || count <= 1)
            {
                if (label != null) label.gameObject.SetActive(false);
                return;
            }

            if (label == null) label = Create(slot.transform);
            label.gameObject.SetActive(true);
            label.text = count.ToString();
        }

        public static bool TryParseCount(string title, out int count)
        {
            count = 0;
            if (string.IsNullOrEmpty(title)) return false;
            var match = CountPattern.Match(title);
            return match.Success && int.TryParse(match.Groups[1].Value, out count);
        }

        private static Text Create(Transform slot)
        {
            var label = UiBuilder.MakeText(slot, UiBuilder.DefaultFont(), ChildName, 11, false);
            label.alignment = TextAnchor.LowerRight;
            label.color = CountColor;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            // Viền dày hơn một chút và đều: Outline chỉ nhân 4 góc chéo, thêm lớp thứ hai theo
            // hướng ngang/dọc để cạnh chữ không mỏng. Không dùng Shadow (đổ bóng).
            AddOutline(label, new Vector2(1.2f, -1.2f));
            AddOutline(label, new Vector2(1.2f, 0f));
            AddOutline(label, new Vector2(0f, 1.2f));

            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 1f);
            rect.offsetMax = new Vector2(-3f, -2f);
            // Đứng sau Icon trong thứ tự con → vẽ đè lên icon.
            label.transform.SetAsLastSibling();
            return label;
        }

        private static void AddOutline(Text label, Vector2 distance)
        {
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = distance;
        }
    }
}
