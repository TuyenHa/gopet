using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Form soạn thư, dựng THẲNG trong vùng nội dung của popup hộp thư chứ không phải
    /// một hộp thoại riêng đè lên.
    ///
    /// <para>Trước đây nó là một overlay tối phủ kín màn, mở đè lên hộp thư — người chơi
    /// mất hẳn ngữ cảnh và phải đóng hai lớp mới về được map. Giờ nó là nội dung của tab
    /// "Soạn thư", đổi tab là đổi nội dung, khung popup đứng yên.</para>
    /// </summary>
    public sealed class ComposeLetterView : MonoBehaviour
    {
        private const int RecipientLimit = 32;
        private const int ContentLimit = 500;

        // Đo từ ĐÁY lên: hàng nút cách mép dưới 10, rồi tới dòng báo lỗi, rồi mới tới ô
        // nội dung. Neo theo đáy nên đổi chiều cao popup là cả cụm tự đi theo.
        private const float BottomMargin = 10f;
        private const float ErrorGap = 6f;
        private const float ErrorHeight = 16f;
        private const float ContentTop = 42f;
        private const float ContentBottomGap = 4f;

        private const float ErrorBottom = BottomMargin + PopupButtonRow.Height + ErrorGap;
        private const float ContentBottom = ErrorBottom + ErrorHeight + ContentBottomGap;

        private InputField _recipient;
        private InputField _content;
        private Text _error;

        /// <summary>Người chơi bấm Gửi với cả hai ô đã điền.</summary>
        public event Action<string, string> SendRequested;

        /// <summary>Người chơi bấm Huỷ.</summary>
        public event Action Cancelled;

        public static ComposeLetterView Create(RectTransform parent, Font font)
        {
            var go = new GameObject("ComposeLetter", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);

            var view = go.AddComponent<ComposeLetterView>();
            view.Build(font);
            return view;
        }

        /// <summary>Xoá nội dung đã gõ và câu báo lỗi — dùng khi quay lại tab này.</summary>
        public void Reset()
        {
            if (_recipient != null) _recipient.text = string.Empty;
            if (_content != null) _content.text = string.Empty;
            if (_error != null) _error.text = string.Empty;
        }

        private void Build(Font font)
        {
            _recipient = PopupField.Create(transform, font, "Người nhận", 6f, 28f, RecipientLimit);
            _recipient.lineType = InputField.LineType.SingleLine;

            _content = PopupField.CreateStretched(transform, font, "Nội dung",
                ContentTop, ContentBottom, ContentLimit);
            _content.lineType = InputField.LineType.MultiLineNewline;
            // Chữ phải bám MÉP TRÊN ô: canh giữa thì đoạn dài chạy tràn ra ngoài khung.
            _content.textComponent.alignment = TextAnchor.UpperLeft;
            // Cho chữ tràn khỏi rect: InputField cuộn bằng cách DỜI ô chữ theo con trỏ,
            // nên ô chữ phải vẽ đủ mọi dòng rồi để RectMask2D của hộp cắt phần ngoài.
            // Đặt Truncate là mất hẳn phần dưới, cuộn xuống chỉ thấy khoảng trắng.
            _content.textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            _content.textComponent.rectTransform.offsetMin = new Vector2(8f, 6f);
            _content.textComponent.rectTransform.offsetMax = new Vector2(-8f, -6f);

            _error = UiBuilder.MakeText(transform, font, "Error", 11, false);
            _error.alignment = TextAnchor.MiddleCenter;
            _error.color = new Color(0.78f, 0.2f, 0.2f, 1f);
            PlaceAtBottom(_error.rectTransform, ErrorBottom, ErrorHeight);

            PopupButtonRow.CreateAtBottom(transform, font, BottomMargin, "Gửi", TrySend,
                "Huỷ", () => Cancelled?.Invoke());
        }

        /// <summary>Ghim một dòng theo mép ĐÁY thay vì mép trên như <c>UiBuilder.PlaceRow</c>.</summary>
        private static void PlaceAtBottom(RectTransform rect, float bottom, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(16f, bottom);
            rect.offsetMax = new Vector2(-16f, bottom + height);
        }

        private void TrySend()
        {
            var recipient = _recipient.text == null ? string.Empty : _recipient.text.Trim();
            var content = _content.text == null ? string.Empty : _content.text.Trim();
            if (recipient.Length == 0)
            {
                _error.text = "Chưa nhập người nhận.";
                return;
            }
            if (content.Length == 0)
            {
                _error.text = "Chưa nhập nội dung.";
                return;
            }

            _error.text = string.Empty;
            SendRequested?.Invoke(recipient, content);
        }
    }
}
