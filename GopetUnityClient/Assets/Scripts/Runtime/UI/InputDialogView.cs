using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hộp thoại nhập liệu (<c>TYPE_DIALOG_INPUT</c>): tiêu đề + N ô nhập có nhãn.
    ///
    /// <para>Số ô do server quyết định và câu trả lời phải gửi <b>đúng bấy nhiêu</b>
    /// chuỗi — server dựng <c>InputReader</c> theo số ô nó đã hỏi
    /// (<c>GameController.cs:788-795</c>).</para>
    ///
    /// <para><b>Bẫy đã trả giá:</b> bộ gõ tiếng Việt (Telex) nuốt phím trong ô nhập —
    /// gõ <c>test1234</c> ra <c>tét1234</c>. Đã dính khi nhập vào emulator ở P1, và
    /// ô nhập của Unity dính y hệt. Khi test bằng tay phải chuyển sang bàn phím
    /// tiếng Anh.</para>
    /// </summary>
    public sealed class InputDialogView : MonoBehaviour
    {
        private readonly List<InputField> _fields = new List<InputField>();

        private Text _title;
        private Font _font;

        public int DialogId { get; private set; }

        public IReadOnlyList<InputField> Fields => _fields;

        /// <summary>Người dùng xác nhận. Mảng trả về đúng số ô server đã hỏi.</summary>
        public event Action<int, string[]> Submitted;

        public static InputDialogView Create(Transform parent, Font font)
        {
            var go = new GameObject("InputDialogView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = UiBuilder.Panel;

            var view = go.AddComponent<InputDialogView>();
            view._font = font;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleGo.transform.SetParent(go.transform, false);
            view._title = titleGo.GetComponent<Text>();
            view._title.font = font;
            view._title.fontSize = 18;
            view._title.color = UiBuilder.TextMain;

            return view;
        }

        public void Bind(InputDialogSpec spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            DialogId = spec.DialogId;
            _title.text = spec.Title;

            foreach (var field in _fields)
            {
                if (field != null) Destroy(field.gameObject);
            }

            _fields.Clear();

            foreach (var definition in spec.Fields)
            {
                _fields.Add(MakeField(definition));
            }
        }

        /// <summary>Cho test và cho phím Enter gọi thẳng.</summary>
        public void Submit()
        {
            var texts = new string[_fields.Count];
            for (var i = 0; i < _fields.Count; i++)
            {
                // Ô trống phải gửi chuỗi rỗng chứ không phải null: server đọc đủ
                // số chuỗi nó đã hỏi, thiếu một cái là lệch cả gói.
                texts[i] = _fields[i].text ?? string.Empty;
            }

            Submitted?.Invoke(DialogId, texts);
        }

        private InputField MakeField(InputDialogSpec.Field definition)
        {
            var go = new GameObject($"Field_{definition.Label}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            go.GetComponent<Image>().color = UiBuilder.Field;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.GetComponent<Text>();
            text.font = _font;
            text.fontSize = 16;
            text.supportRichText = false;
            text.color = UiBuilder.TextMain;

            var field = go.AddComponent<InputField>();
            field.textComponent = text;

            // Server gửi kiểu ô nhập; khác 0 nghĩa là ô số, để bàn phím mobile mở
            // đúng bàn phím số thay vì bàn phím chữ.
            field.contentType = definition.InputType != 0
                ? InputField.ContentType.IntegerNumber
                : InputField.ContentType.Standard;

            return field;
        }
    }
}
