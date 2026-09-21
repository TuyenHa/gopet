using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Form nhập một dòng trong popup: tiêu đề, ô nhập có nhãn, nút Đồng ý / Huỷ.
    ///
    /// <para>Dựng thẳng trong tab thay vì chờ server mở hộp thoại. Các handler nhập
    /// liệu của server (mã quà tặng, tên bang hội…) đọc dữ liệu từ chính gói gửi lên
    /// và không phụ thuộc việc hộp thoại đã mở trước đó, nên client submit thẳng
    /// được — mà lại không bị một hộp thoại riêng đè lên popup.</para>
    /// </summary>
    public sealed class PopupInputForm : MonoBehaviour
    {
        /// <summary>Kiểu hộp nhập của server — <c>MenuController.INPUT_TYPE_GIFT_CODE</c>. GIỮ khớp server.</summary>
        public const int GiftCodeDialogId = 17;

        private const float LabelWidth = 76f;
        private const float FieldHeight = 30f;
        private const float ButtonHeight = 30f;
        /// <summary>Bo góc ô nhập, theo yêu cầu thiết kế.</summary>
        private const float FieldRadius = 5f;

        private InputField _input;
        private Text _error;

        /// <summary>Người chơi gửi mã. Tham số là mã đã cắt khoảng trắng, chắc chắn không rỗng.</summary>
        public event Action<string> Submitted;

        /// <summary>Người chơi bấm Huỷ.</summary>
        public event Action Cancelled;

        private string _emptyWarning = "Chưa nhập gì cả.";

        public static PopupInputForm Create(RectTransform parent, Font font, string title,
            string fieldLabel, string hint = null)
        {
            var go = new GameObject("InputForm", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);

            var form = go.AddComponent<PopupInputForm>();
            form._emptyWarning = "Chưa nhập " + fieldLabel.TrimEnd(':').ToLowerInvariant() + ".";
            form.Build(font, title, fieldLabel, hint);
            return form;
        }

        /// <summary>Xoá mã đã gõ và câu báo lỗi — dùng khi quay lại tab này.</summary>
        public void Reset()
        {
            if (_input != null) _input.text = string.Empty;
            if (_error != null) _error.text = string.Empty;
        }

        private void Build(Font font, string titleText, string fieldLabel, string hint)
        {
            var title = UiBuilder.MakeText(transform, font, "Title", 14, false);
            title.text = titleText;
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.color = PopupPalette.TextDark;
            UiBuilder.PlaceRow(title.rectTransform, 18f, 24f, 16f);

            BuildField(font, 58f, fieldLabel);
            if (!string.IsNullOrEmpty(hint))
            {
                var note = UiBuilder.MakeText(transform, font, "Hint", 10, false);
                note.text = hint;
                note.alignment = TextAnchor.MiddleCenter;
                note.color = PopupPalette.TextMuted;
                UiBuilder.PlaceRow(note.rectTransform, 38f, 16f, 16f);
            }

            _error = UiBuilder.MakeText(transform, font, "Error", 11, false);
            _error.alignment = TextAnchor.MiddleCenter;
            _error.color = new Color(0.78f, 0.2f, 0.2f, 1f);
            UiBuilder.PlaceRow(_error.rectTransform, 94f, 18f, 16f);

            BuildButtons(font, 120f);
        }

        private void BuildField(Font font, float top, string fieldLabel)
        {
            var row = new GameObject("Field", typeof(RectTransform));
            row.transform.SetParent(transform, false);
            UiBuilder.PlaceRow((RectTransform)row.transform, top, FieldHeight, 16f);

            var label = UiBuilder.MakeText(row.transform, font, "Label", 13, false);
            label.text = fieldLabel;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = PopupPalette.TextDark;
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0f, 1f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(LabelWidth, 0f);
            labelRect.anchoredPosition = Vector2.zero;

            var box = new GameObject("Box", typeof(RectTransform), typeof(Image),
                typeof(InputField));
            box.transform.SetParent(row.transform, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = new Vector2(0f, 0f);
            boxRect.anchorMax = new Vector2(1f, 1f);
            boxRect.offsetMin = new Vector2(LabelWidth + 8f, 0f);
            boxRect.offsetMax = Vector2.zero;
            RoundedBorder.Apply(box, FieldRadius, Color.white, PopupPalette.Hairline);

            var text = UiBuilder.MakeText(box.transform, font, "Text", 13, true);
            text.color = PopupPalette.TextDark;
            text.supportRichText = false;
            text.rectTransform.offsetMin = new Vector2(8f, 0f);
            text.rectTransform.offsetMax = new Vector2(-8f, 0f);

            _input = box.GetComponent<InputField>();
            _input.textComponent = text;
            _input.contentType = InputField.ContentType.Alphanumeric;
            _input.lineType = InputField.LineType.SingleLine;
            _input.characterLimit = 32;
        }

        private void BuildButtons(Font font, float top)
        {
            var row = new GameObject("Buttons", typeof(RectTransform));
            row.transform.SetParent(transform, false);
            UiBuilder.PlaceRow((RectTransform)row.transform, top, ButtonHeight, 16f);

            MakeButton(row.transform, font, "Đồng ý", 0, PopupPalette.ButtonBlue,
                Color.white, Submit);
            MakeButton(row.transform, font, "Huỷ", 1, new Color(0.88f, 0.91f, 0.95f, 1f),
                PopupPalette.TextDark, () => Cancelled?.Invoke());
        }

        private void Submit()
        {
            var code = _input.text == null ? string.Empty : _input.text.Trim();
            if (code.Length == 0)
            {
                _error.text = _emptyWarning;
                return;
            }

            _error.text = string.Empty;
            Submitted?.Invoke(code);
        }

        private static void MakeButton(Transform parent, Font font, string label, int index,
            Color face, Color textColor, Action onClick)
        {
            var go = new GameObject($"Button_{index}", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(index * 0.5f, 0f);
            rect.anchorMax = new Vector2(index * 0.5f + 0.5f, 1f);
            rect.offsetMin = new Vector2(index == 0 ? 0f : 5f, 0f);
            rect.offsetMax = new Vector2(index == 0 ? -5f : 0f, 0f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 6f);
            image.color = face;

            var text = UiBuilder.MakeText(go.transform, font, "Label", 13, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.color = textColor;

            go.GetComponent<Button>().onClick.AddListener(() => onClick());
        }
    }
}
