using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Biểu mẫu do CLIENT dựng: tiêu đề + N ô nhập có nhãn + N nút + một dòng
    /// thông báo. Dùng cho đăng nhập, tạo nhân vật và màn báo trạng thái kết nối.
    ///
    /// <para>Khác <see cref="InputDialogView"/> ở chỗ nguồn: hộp kia do SERVER mô tả
    /// (<c>TYPE_DIALOG_INPUT</c>) nên hình dạng của nó bị giao thức ràng buộc; cái
    /// này client tự quyết. Gộp hai thứ lại thì mỗi lần đổi màn đăng nhập là một
    /// lần rủi ro cho toàn bộ 162 màn hình của server.</para>
    ///
    /// <para>Mọi phần tử neo trải ngang qua <see cref="UiBuilder"/>. Để anchor mặc
    /// định thì chiều rộng bằng 0 — chữ vẫn hiện nhưng không bấm được, và test gọi
    /// thẳng phương thức sẽ không thấy gì (đã trả giá ở <see cref="MenuItemRow"/>).</para>
    /// </summary>
    public sealed class FormView : MonoBehaviour
    {
        public const float RowHeight = 44f;

        private const float Padding = 12f;

        private readonly List<InputField> _fields = new List<InputField>();
        private readonly List<Button> _buttons = new List<Button>();

        private Text _title;
        private Text _notice;
        private Font _font;
        private float _cursorY;

        /// <summary>Nút được bấm, tính từ 0 theo thứ tự truyền vào <see cref="Bind"/>.</summary>
        public event Action<int> Pressed;

        public IReadOnlyList<InputField> Fields => _fields;

        public IReadOnlyList<Button> Buttons => _buttons;

        public string Title => _title == null ? null : _title.text;

        /// <summary>Nút mà phím Enter kích hoạt. -1 nghĩa là Enter không làm gì.</summary>
        public int DefaultButton { get; set; } = -1;

        public static FormView Create(Transform parent, Font font)
        {
            var go = new GameObject("FormView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            go.GetComponent<Image>().color = UiBuilder.Panel;

            var view = go.AddComponent<FormView>();
            view._font = font;
            view._title = view.MakeText("Title", 22);
            view._notice = view.MakeText("Notice", 16);
            view._notice.color = UiBuilder.TextMuted;
            return view;
        }

        /// <param name="fieldLabels">Nhãn từng ô nhập; rỗng nếu màn hình không có ô nào.</param>
        /// <param name="buttonLabels">Nhãn từng nút, theo đúng thứ tự.</param>
        public void Bind(string title, IReadOnlyList<string> fieldLabels, IReadOnlyList<string> buttonLabels)
        {
            if (fieldLabels == null) throw new ArgumentNullException(nameof(fieldLabels));
            if (buttonLabels == null) throw new ArgumentNullException(nameof(buttonLabels));

            foreach (var field in _fields) Discard(field);
            foreach (var button in _buttons) Discard(button);

            _fields.Clear();
            _buttons.Clear();
            _cursorY = 0f;

            _title.text = title;
            Place((RectTransform)_title.transform);

            for (var i = 0; i < fieldLabels.Count; i++) _fields.Add(MakeField(fieldLabels[i]));
            for (var i = 0; i < buttonLabels.Count; i++) _buttons.Add(MakeButton(buttonLabels[i], i));

            SetNotice(null);
            Place((RectTransform)_notice.transform);
        }

        /// <summary>Câu báo cho người dùng — lỗi nhập, lý do server từ chối, hoặc trạng thái đang chờ.</summary>
        public void SetNotice(string text)
        {
            _notice.text = text ?? string.Empty;
        }

        public string NoticeText => _notice == null ? null : _notice.text;

        public string TextOf(int fieldIndex)
        {
            return _fields[fieldIndex].text ?? string.Empty;
        }

        /// <summary>
        /// Che ô nhập (mật khẩu). Đặt sau <see cref="Bind"/>.
        ///
        /// <para><b>Bẫy đã trả giá ở P1:</b> bộ gõ Telex nuốt phím trong ô nhập —
        /// gõ <c>test1234</c> ra <c>tét1234</c>. Ô che thì không nhìn thấy để mà
        /// biết. Kiểm bằng bàn phím tiếng Anh.</para>
        /// </summary>
        public void SetSecret(int fieldIndex, bool secret = true)
        {
            _fields[fieldIndex].contentType = secret
                ? InputField.ContentType.Password
                : InputField.ContentType.Standard;
        }

        /// <summary>Cho phím Enter và cho test gọi thẳng, không phải qua chuột.</summary>
        public void Press(int index)
        {
            if (index < 0 || index >= _buttons.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Nút {index} không có trên biểu mẫu.");
            }

            if (!_buttons[index].interactable) return;

            Pressed?.Invoke(index);
        }

        /// <summary>Phím Enter. Không làm gì khi biểu mẫu chưa đặt nút mặc định.</summary>
        public void SubmitDefault()
        {
            if (DefaultButton >= 0 && DefaultButton < _buttons.Count) Press(DefaultButton);
        }

        private InputField MakeField(string label)
        {
            var go = new GameObject($"Field_{label}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            go.GetComponent<Image>().color = UiBuilder.Field;
            Place((RectTransform)go.transform);

            var text = MakeText("Text", 18, go.transform);
            var placeholder = MakeText("Placeholder", 18, go.transform);
            placeholder.text = label;
            placeholder.color = new Color(UiBuilder.TextMuted.r, UiBuilder.TextMuted.g, UiBuilder.TextMuted.b, 0.55f);

            var field = go.AddComponent<InputField>();
            field.textComponent = text;
            field.placeholder = placeholder;
            return field;
        }

        private Button MakeButton(string label, int index)
        {
            var go = new GameObject($"Button_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            Place((RectTransform)go.transform);

            var text = MakeText("Label", 18, go.transform);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;

            var button = go.GetComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => Press(captured));
            return button;
        }

        private Text MakeText(string name, int size, Transform parent = null)
        {
            return UiBuilder.MakeText(parent ?? transform, _font, name, size, parent != null);
        }

        /// <summary>
        /// Tắt trước rồi mới huỷ: <c>Destroy</c> chỉ có hiệu lực cuối frame, nên hàng
        /// của biểu mẫu CŨ vẫn hiện và vẫn nhận được click trong suốt frame đó.
        /// </summary>
        private static void Discard(Component widget)
        {
            if (widget == null) return;

            widget.gameObject.SetActive(false);
            Destroy(widget.gameObject);
        }

        /// <summary>Xếp một hàng xuống dưới hàng trước.</summary>
        private void Place(RectTransform rect)
        {
            UiBuilder.PlaceRow(rect, _cursorY, RowHeight, Padding);
            _cursorY += RowHeight + Padding;
        }
    }
}
