using System.Text;
using Gopet.Net.Chat;
using Gopet.Net.Guild;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed partial class GameHud
    {
        private void BuildChat(Transform parent, Font font)
        {
            var panel = new GameObject("Place Chat", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            _chatPanel = (RectTransform)panel.transform;
            panel.GetComponent<Image>().color = new Color(0.035f, 0.12f, 0.15f, 0.86f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());

            _channel = ChatChannelSelector.Create(panel.transform);
            _channel.Changed += _ =>
            {
                SetChatExpanded(true);
                if (_channel.Current == ChatChannelSelector.Channel.Guild)
                    GuildHistoryRequested?.Invoke();
                RefreshChatLog();
                UpdatePlaceholder();
            };
            MakeToggleButton(panel.transform, font);
            BuildChatBody(panel.transform, font);
            SetChatExpanded(false);
        }

        private void BuildChatBody(Transform parent, Font font)
        {
            _chatBody = new GameObject("Chat Body", typeof(RectTransform));
            _chatBody.transform.SetParent(parent, false);
            var bodyRect = (RectTransform)_chatBody.transform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = new Vector2(0f, -42f);

            _chatLog = UiBuilder.MakeText(_chatBody.transform, font, "Messages", 15, false);
            var logRect = _chatLog.rectTransform;
            logRect.anchorMin = Vector2.zero;
            logRect.anchorMax = Vector2.one;
            logRect.offsetMin = new Vector2(8f, 48f);
            logRect.offsetMax = new Vector2(-8f, -4f);
            _chatLog.alignment = TextAnchor.LowerLeft;
            _chatLog.horizontalOverflow = HorizontalWrapMode.Wrap;
            _chatLog.verticalOverflow = VerticalWrapMode.Truncate;
            _chatLog.color = Color.white;

            _chatInput = MakeInput(_chatBody.transform, font);
            var button = MakeSendButton(_chatBody.transform, font);
            button.onClick.AddListener(SubmitChat);
            UpdatePlaceholder();
        }

        private void MakeToggleButton(Transform parent, Font font)
        {
            var go = new GameObject("Toggle Chat", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-5f, -4f);
            rect.sizeDelta = new Vector2(42f, 34f);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.04f, 0.13f, 0.17f, 0.95f);
            RoundedUiSprite.Apply(image);
            _toggleLabel = UiBuilder.MakeText(go.transform, font, "Label", 20, true);
            _toggleLabel.alignment = TextAnchor.MiddleCenter;
            _toggleLabel.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => SetChatExpanded(!_chatExpanded));
        }

        private static InputField MakeInput(Transform parent, Font font)
        {
            var go = new GameObject("Chat Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.offsetMin = new Vector2(5f, 5f);
            rect.offsetMax = new Vector2(-76f, 43f);
            go.GetComponent<Image>().color = new Color(0.93f, 0.95f, 0.98f, 1f);
            var input = go.GetComponent<InputField>();
            input.characterLimit = 120;
            var text = UiBuilder.MakeText(go.transform, font, "Text", 17, true);
            text.rectTransform.offsetMin = new Vector2(8f, 0f);
            text.rectTransform.offsetMax = new Vector2(-8f, 0f);
            text.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            input.textComponent = text;
            var placeholder = UiBuilder.MakeText(go.transform, font, "Placeholder", 17, true);
            placeholder.color = new Color(0.52f, 0.55f, 0.6f, 1f);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.rectTransform.offsetMin = new Vector2(8f, 0f);
            input.placeholder = placeholder;
            return input;
        }

        private static Button MakeSendButton(Transform parent, Font font)
        {
            var go = new GameObject("Send", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-5f, 5f);
            rect.sizeDelta = new Vector2(66f, 38f);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.82f, 0.57f, 0.08f, 1f);
            RoundedUiSprite.Apply(image);
            var label = UiBuilder.MakeText(go.transform, font, "Label", 17, true);
            label.text = "GỬI";
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = Color.white;
            return go.GetComponent<Button>();
        }

    }
}
