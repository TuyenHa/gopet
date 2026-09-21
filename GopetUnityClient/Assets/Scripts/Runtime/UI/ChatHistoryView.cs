using System;
using System.Text;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class ChatHistoryView : MonoBehaviour
    {
        private Text _history;
        private InputField _input;
        public event Action CloseRequested;
        public event Action<string> SendRequested;

        public static ChatHistoryView Create(Transform parent, string title, ChatTranscript transcript)
        {
            var root = LegacyOverlayUi.Overlay(parent, "Chat History");
            var view = root.AddComponent<ChatHistoryView>();
            view.Build(title);
            view.Refresh(transcript);
            return view;
        }

        public void Refresh(ChatTranscript transcript)
        {
            if (_history == null) return;
            var value = new StringBuilder();
            foreach (var entry in transcript.Entries)
                value.Append(entry.Sender).Append(": ").AppendLine(entry.Text);
            _history.text = value.ToString();
        }

        private void Build(string heading)
        {
            var panel = LegacyOverlayUi.Panel(transform, new Vector2(500f, 390f));
            var title = LegacyOverlayUi.Text(panel, "Title", heading, 18, 12f, 34f);
            title.alignment = TextAnchor.MiddleCenter;
            _history = LegacyOverlayUi.Text(panel, "History", string.Empty, 13, 54f, 245f);
            _history.alignment = TextAnchor.LowerLeft;
            _input = MakeInput(panel, 305f);
            LegacyOverlayUi.Button(panel, "Gửi", 220f, TrySend);
            LegacyOverlayUi.Button(panel, "Đóng", 360f, () => CloseRequested?.Invoke());
        }

        private void TrySend()
        {
            var value = _input.text?.Trim();
            if (string.IsNullOrEmpty(value)) return;
            SendRequested?.Invoke(value);
            _input.text = string.Empty;
        }

        private static InputField MakeInput(Transform panel, float top)
        {
            var go = new GameObject("Chat Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(18f, -(top + 40f));
            rect.offsetMax = new Vector2(-294f, -top);
            go.GetComponent<Image>().color = UiBuilder.Field;
            var input = go.GetComponent<InputField>();
            input.characterLimit = 120;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Text", 14, true);
            text.rectTransform.offsetMin = new Vector2(8f, 0f);
            text.rectTransform.offsetMax = new Vector2(-8f, 0f);
            input.textComponent = text;
            return input;
        }
    }
}
