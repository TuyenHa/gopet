using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class ComposeLetterView : MonoBehaviour
    {
        private InputField _recipient, _content;
        public event Action CloseRequested;
        public event Action<string, string> SendRequested;

        public static ComposeLetterView Create(Transform parent)
        {
            var root = LetterDetailView.Overlay(parent, "Compose Letter");
            var view = root.AddComponent<ComposeLetterView>();
            var panel = LetterDetailView.Panel(root.transform, new Vector2(440f, 330f));
            var title = LetterDetailView.Text(panel, "Title", "Soạn thư", 18, 16f, 36f);
            title.alignment = TextAnchor.MiddleCenter;
            view._recipient = Field(panel, "Người nhận", 64f, 40f, 32);
            view._content = Field(panel, "Nội dung", 116f, 130f, 500);
            view._content.lineType = InputField.LineType.MultiLineNewline;
            LetterDetailView.Button(panel, "Gửi", 88f, view.TrySend);
            LetterDetailView.Button(panel, "Huỷ", 228f, () => view.CloseRequested?.Invoke());
            return view;
        }

        private void TrySend()
        {
            if (string.IsNullOrWhiteSpace(_recipient.text) || string.IsNullOrWhiteSpace(_content.text)) return;
            SendRequested?.Invoke(_recipient.text, _content.text);
        }

        private static InputField Field(Transform parent, string placeholder, float top, float height, int limit)
        {
            var go = new GameObject(placeholder, typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, top, height, 18f);
            go.GetComponent<Image>().color = UiBuilder.Field;
            var input = go.GetComponent<InputField>();
            input.characterLimit = limit;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Text", 14, true);
            text.rectTransform.offsetMin = new Vector2(10f, 4f);
            text.rectTransform.offsetMax = new Vector2(-10f, -4f);
            input.textComponent = text;
            var hint = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Placeholder", 13, true);
            hint.text = placeholder;
            hint.color = UiBuilder.TextMuted;
            hint.rectTransform.offsetMin = new Vector2(10f, 4f);
            hint.rectTransform.offsetMax = new Vector2(-10f, -4f);
            input.placeholder = hint;
            return input;
        }
    }
}
