using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hàng hai nút chia đôi bề ngang ở chân form popup: nút chính xanh chữ trắng, nút
    /// phụ xám nhạt chữ navy.
    ///
    /// <para>Dùng chung cho mọi form trong popup (nhập mã quà tặng, soạn thư…) — mỗi form
    /// tự dựng thì chỉ lệch một con số bo góc hay bề rộng khe là hai popup khác nhau.</para>
    /// </summary>
    public static class PopupButtonRow
    {
        public const float Height = 30f;

        /// <summary>Khe giữa hai nút, chia đều mỗi bên.</summary>
        private const float Gap = 5f;
        private const float Radius = 6f;

        private static readonly Color SecondaryFace = new Color(0.88f, 0.91f, 0.95f, 1f);

        /// <summary>Hàng nút ghim mép TRÊN, cách đỉnh <paramref name="top"/>.</summary>
        public static void Create(Transform parent, Font font, float top,
            string primaryLabel, Action onPrimary, string secondaryLabel, Action onSecondary)
        {
            var row = NewRow(parent);
            UiBuilder.PlaceRow(row, top, Height, 16f);
            Fill(row, font, primaryLabel, onPrimary, secondaryLabel, onSecondary);
        }

        /// <summary>
        /// Hàng nút ghim mép ĐÁY, cách đáy <paramref name="bottom"/>. Neo theo đáy thì đổi
        /// chiều cao popup là hàng nút tự đi theo, khỏi tính lại khoảng cách từ đỉnh.
        /// </summary>
        public static void CreateAtBottom(Transform parent, Font font, float bottom,
            string primaryLabel, Action onPrimary, string secondaryLabel, Action onSecondary)
        {
            var row = NewRow(parent);
            row.anchorMin = new Vector2(0f, 0f);
            row.anchorMax = new Vector2(1f, 0f);
            row.pivot = new Vector2(0.5f, 0f);
            row.offsetMin = new Vector2(16f, bottom);
            row.offsetMax = new Vector2(-16f, bottom + Height);
            Fill(row, font, primaryLabel, onPrimary, secondaryLabel, onSecondary);
        }

        private static RectTransform NewRow(Transform parent)
        {
            var go = new GameObject("Buttons", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Fill(RectTransform row, Font font, string primaryLabel,
            Action onPrimary, string secondaryLabel, Action onSecondary)
        {
            MakeButton(row, font, primaryLabel, 0, PopupPalette.ButtonBlue,
                Color.white, onPrimary);
            MakeButton(row, font, secondaryLabel, 1, SecondaryFace,
                PopupPalette.TextDark, onSecondary);
        }

        /// <summary>
        /// Một nút của hàng hai nút, chia đôi bề ngang <paramref name="parent"/>.
        /// Công khai để chỗ nào cần cặp nút màu khác (đọc thư: xanh + ĐỎ) dùng lại đúng
        /// kích thước và bo góc này thay vì dựng nút riêng.
        /// </summary>
        /// <param name="index">0 = nửa trái, 1 = nửa phải.</param>
        public static Button MakeButton(Transform parent, Font font, string label, int index,
            Color face, Color textColor, Action onClick)
        {
            var go = new GameObject($"Button_{index}", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(index * 0.5f, 0f);
            rect.anchorMax = new Vector2(index * 0.5f + 0.5f, 1f);
            rect.offsetMin = new Vector2(index == 0 ? 0f : Gap, 0f);
            rect.offsetMax = new Vector2(index == 0 ? -Gap : 0f, 0f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, Radius);
            image.color = face;

            var text = UiBuilder.MakeText(go.transform, font, "Label", 13, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.color = textColor;

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());
            return button;
        }
    }
}
