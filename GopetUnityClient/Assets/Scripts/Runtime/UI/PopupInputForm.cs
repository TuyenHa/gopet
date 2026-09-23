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

        private const float FieldHeight = 30f;

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
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
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

            PopupButtonRow.Create(transform, font, 120f, "Đồng ý", Submit,
                "Huỷ", () => Cancelled?.Invoke());
        }

        private void BuildField(Font font, float top, string fieldLabel)
        {
            _input = PopupField.Create(transform, font, fieldLabel, top, FieldHeight, 32);
            // Mã quà tặng chỉ có chữ và số — chặn ngay ở bàn phím cho đỡ gõ nhầm.
            _input.contentType = InputField.ContentType.Alphanumeric;
            _input.lineType = InputField.LineType.SingleLine;
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
    }
}
