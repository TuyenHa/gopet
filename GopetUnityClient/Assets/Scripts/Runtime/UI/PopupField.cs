using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Ô nhập có nhãn bên trái, theo style popup: hộp trắng bo góc viền mảnh, nhãn navy.
    ///
    /// <para>Tách ra khỏi <see cref="PopupInputForm"/> để form soạn thư dùng lại y hệt —
    /// hai bên tự dựng thì chỉ cần lệch một con số bo góc hay màu viền là nhìn ra ngay
    /// hai popup khác tông.</para>
    /// </summary>
    public static class PopupField
    {
        /// <summary>Bề ngang cột nhãn. Popup dùng để canh các dòng thẳng hàng nhau.</summary>
        public const float LabelWidth = 76f;

        /// <summary>Khe giữa cột nhãn và hộp nhập.</summary>
        public const float LabelGap = 8f;

        /// <summary>Bo góc hộp nhập, theo yêu cầu thiết kế.</summary>
        private const float Radius = 5f;

        /// <summary>
        /// Dựng một dòng nhập ghim mép trên <paramref name="parent"/>, cao cố định.
        /// Người gọi tự đặt <c>lineType</c>/<c>contentType</c> sau khi nhận về.
        /// </summary>
        public static InputField Create(Transform parent, Font font, string label,
            float top, float height, int characterLimit)
        {
            var row = new GameObject($"Field_{label}", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            UiBuilder.PlaceRow((RectTransform)row.transform, top, height, 16f);
            return Fill(row.transform, font, label, characterLimit);
        }

        /// <summary>
        /// Ô nhập KÉO GIÃN theo chiều dọc: mép trên cách đỉnh <paramref name="top"/>, mép
        /// dưới cách đáy <paramref name="bottom"/>. Dùng cho ô nội dung nhiều dòng — chiều
        /// cao bám theo popup thay vì đóng cứng một con số rồi thừa một mảng trống ở đáy.
        /// </summary>
        public static InputField CreateStretched(Transform parent, Font font, string label,
            float top, float bottom, int characterLimit)
        {
            var row = new GameObject($"Field_{label}", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16f, bottom);
            rect.offsetMax = new Vector2(-16f, -top);
            return Fill(row.transform, font, label, characterLimit);
        }

        private static InputField Fill(Transform row, Font font, string label,
            int characterLimit)
        {
            var caption = UiBuilder.MakeText(row, font, "Label", 13, false);
            caption.text = label;
            caption.alignment = TextAnchor.MiddleLeft;
            caption.color = PopupPalette.TextDark;
            var captionRect = caption.rectTransform;
            captionRect.anchorMin = new Vector2(0f, 0f);
            captionRect.anchorMax = new Vector2(0f, 1f);
            captionRect.pivot = new Vector2(0f, 0.5f);
            captionRect.sizeDelta = new Vector2(LabelWidth, 0f);
            captionRect.anchoredPosition = Vector2.zero;

            // RectMask2D: InputField tự đẩy chữ theo con trỏ khi gõ quá chỗ, nhưng KHÔNG
            // tự cắt — thiếu mặt nạ thì phần thừa vẽ tràn ra ngoài viền ô, đè lên nhãn và
            // nút bên cạnh. Có mặt nạ mới thành "cuộn trong ô".
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image),
                typeof(RectMask2D), typeof(InputField));
            box.transform.SetParent(row, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = new Vector2(0f, 0f);
            boxRect.anchorMax = new Vector2(1f, 1f);
            boxRect.offsetMin = new Vector2(LabelWidth + LabelGap, 0f);
            boxRect.offsetMax = Vector2.zero;
            RoundedBorder.Apply(box, Radius, Color.white, PopupPalette.Hairline);

            var text = UiBuilder.MakeText(box.transform, font, "Text", 13, true);
            text.color = PopupPalette.TextDark;
            text.supportRichText = false;
            text.rectTransform.offsetMin = new Vector2(8f, 0f);
            text.rectTransform.offsetMax = new Vector2(-8f, 0f);

            var input = box.GetComponent<InputField>();
            input.textComponent = text;
            input.characterLimit = characterLimit;
            return input;
        }
    }
}
