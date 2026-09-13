using Gopet.Net.Guild;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class GuildView
    {
        private Transform _chatContainer;
        private InputField _chatInput;
        private int _chatMessageCount;

        private void BuildChatPage(Transform page)
        {
            var listGo = new GameObject("ChatMessages", typeof(RectTransform));
            listGo.transform.SetParent(page, false);
            var lr = (RectTransform)listGo.transform;
            lr.anchorMin = new Vector2(0f, 0.1f);
            lr.anchorMax = Vector2.one;
            lr.offsetMin = lr.offsetMax = Vector2.zero;
            _chatContainer = listGo.transform;

            var bar = new GameObject("ChatBar", typeof(RectTransform));
            bar.transform.SetParent(page, false);
            var br = (RectTransform)bar.transform;
            br.anchorMin = Vector2.zero;
            br.anchorMax = new Vector2(1f, 0.1f);
            br.offsetMin = br.offsetMax = Vector2.zero;

            var fieldGo = new GameObject("ChatInput", typeof(RectTransform), typeof(Image));
            fieldGo.transform.SetParent(bar.transform, false);
            var fr = (RectTransform)fieldGo.transform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = new Vector2(0.78f, 1f);
            fr.offsetMin = new Vector2(0f, 2f);
            fr.offsetMax = new Vector2(-4f, -2f);
            fieldGo.GetComponent<Image>().color = Color.white;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(fieldGo.transform, false);
            UiBuilder.Stretch((RectTransform)textGo.transform);
            textGo.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 0f);
            var txt = textGo.GetComponent<Text>();
            txt.font = _font;
            txt.fontSize = 12;
            txt.color = GuildText;
            txt.supportRichText = false;

            _chatInput = fieldGo.AddComponent<InputField>();
            _chatInput.textComponent = txt;

            var sendGo = new GameObject("SendBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            sendGo.transform.SetParent(bar.transform, false);
            var sr = (RectTransform)sendGo.transform;
            sr.anchorMin = new Vector2(0.8f, 0f);
            sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(0f, 2f);
            sr.offsetMax = new Vector2(0f, -2f);
            RoundedUiSprite.Apply(sendGo.GetComponent<Image>());
            sendGo.GetComponent<Image>().color = TabActive;
            var sLabel = UiBuilder.MakeText(sendGo.transform, _font, "Label", 12, true);
            sLabel.text = "Gửi";
            sLabel.alignment = TextAnchor.MiddleCenter;
            sLabel.fontStyle = FontStyle.Bold;
            sLabel.color = new Color(0.1f, 0.08f, 0.02f, 1f);
            sendGo.GetComponent<Button>().onClick.AddListener(SendChat);
        }

        private void SendChat()
        {
            if (_clanId == 0) return;
            var text = _chatInput.text;
            if (string.IsNullOrEmpty(text)) return;
            ChatSent?.Invoke(_clanId, text);
            _chatInput.text = string.Empty;
        }

        public void ShowChatHistory(GuildChatHistory history)
        {
            _clanId = history.ClanId;
            foreach (Transform child in _chatContainer) Destroy(child.gameObject);
            _chatMessageCount = 0;

            if (history.Messages == null) return;
            foreach (var msg in history.Messages)
                AppendChatRow(msg.Who, msg.Text);
        }

        public void AppendChat(string who, string text)
        {
            AppendChatRow(who, text);
        }

        private void AppendChatRow(string who, string text)
        {
            var go = new GameObject($"Chat:{_chatMessageCount}", typeof(RectTransform));
            go.transform.SetParent(_chatContainer, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0f, 1f);
            float top = _chatMessageCount * 22f;
            r.offsetMin = new Vector2(0f, -(top + 20f));
            r.offsetMax = new Vector2(0f, -top);

            var label = UiBuilder.MakeText(go.transform, _font, "Msg", 11, true);
            label.text = $"<b>{Gopet.UiLogic.JarIconTokens.Strip(who)}</b>: {text}";
            label.color = GuildText;
            label.supportRichText = true;
            _chatMessageCount++;
        }
    }
}
