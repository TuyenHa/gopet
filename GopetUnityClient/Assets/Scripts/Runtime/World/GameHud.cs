using System;
using System.Text;
using Gopet.Net;
using Gopet.Net.Chat;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>HUD gameplay: joystick và thanh chat thu gọn ở giữa cạnh dưới.</summary>
    public sealed partial class GameHud : MonoBehaviour
    {
        private const float ChatWidthRatio = 0.46f;
        private const float ChatMinWidth = 420f;
        private const float ChatMaxWidth = 520f;
        private const float CollapsedHeight = 42f;
        private const float ExpandedHeight = 194f;

        private readonly ChatTranscript _placeMessages = new ChatTranscript(50);
        private readonly ChatTranscript _worldMessages = new ChatTranscript(100);
        private InputField _chatInput;
        private ChatHandler _chat;
        private ChatChannelSelector _channel;
        private RectTransform _chatPanel;
        private GameObject _chatBody;
        private Text _chatLog;
        private Text _toggleLabel;
        private bool _chatExpanded;

        /// <summary>Đường gửi gói chat Thế giới.</summary>
        public Action<Message> Send;

        /// <summary>Đổi userId trong chat khu vực thành tên nhân vật đang có trên map.</summary>
        public Func<int, string> PlaceChatNameProvider;

        /// <summary>Giữ API cho luồng bang hội hiện tại; thanh chat dưới chỉ hiện hai tab.</summary>
        public Func<int> ClanIdProvider;

        public VirtualJoystick Joystick { get; private set; }
        public CharacterHud Character { get; private set; }
        public NotificationTicker Ticker { get; private set; }
        public bool IsTyping => _chatInput != null && _chatInput.isFocused;
        public bool IsChatExpanded => _chatExpanded;

        public void SetBattleMode(bool active) => gameObject.SetActive(!active);

        public static GameHud Create(Transform parent, ChatHandler chat, string playerName = null)
        {
            var go = new GameObject("Game HUD", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 1f;

            var hud = go.AddComponent<GameHud>();
            hud._chat = chat;
            hud.Character = CharacterHud.Create(go.transform, playerName);
            hud.Joystick = VirtualJoystick.Create(go.transform);
            hud.Ticker = NotificationTicker.Create(go.transform);
            hud.BuildChat(go.transform, UiBuilder.BuiltinFont());
            hud.BuildStatusOverlays(go.transform, UiBuilder.BuiltinFont());
            hud.SubscribeChat();
            return hud;
        }

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

        private void OnEnable()
        {
            if (_chatPanel == null)
                _chatPanel = transform.Find("Place Chat") as RectTransform;
            ApplyChatLayout();
        }

        private void OnRectTransformDimensionsChange() => ApplyChatLayout();

        private void ApplyChatLayout()
        {
            if (_chatPanel == null) return;
            var canvasRect = (RectTransform)transform;
            var width = Mathf.Clamp(canvasRect.rect.width * ChatWidthRatio, ChatMinWidth, ChatMaxWidth);
            _chatPanel.anchorMin = _chatPanel.anchorMax = new Vector2(0.5f, 0f);
            _chatPanel.pivot = new Vector2(0.5f, 0f);
            // Giữ đúng 8 pixel màn hình kể cả khi CanvasScaler đang phóng HUD.
            var canvas = GetComponent<Canvas>();
            var scale = canvas != null ? Mathf.Max(0.01f, canvas.scaleFactor) : 1f;
            _chatPanel.anchoredPosition = new Vector2(0f, 8f / scale);
            _chatPanel.sizeDelta = new Vector2(width, _chatExpanded ? ExpandedHeight : CollapsedHeight);
        }

        public void SetChatExpanded(bool expanded)
        {
            _chatExpanded = expanded;
            if (_chatBody != null) _chatBody.SetActive(expanded);
            if (_toggleLabel != null) _toggleLabel.text = expanded ? "▼" : "▲";
            ApplyChatLayout();
            if (expanded) RefreshChatLog();
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
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;
            return go.GetComponent<Button>();
        }

        private void SubscribeChat()
        {
            if (_chat == null) return;
            _chat.ChatReceived += OnPlaceChat;
            _chat.GlobalChatReceived += OnWorldChat;
        }

        private void OnDestroy()
        {
            if (_chat == null) return;
            _chat.ChatReceived -= OnPlaceChat;
            _chat.GlobalChatReceived -= OnWorldChat;
        }

        private void OnPlaceChat(PlaceChat value)
        {
            if (value == null || value.IsPetInteraction) return;
            var name = PlaceChatNameProvider?.Invoke(value.UserId);
            if (string.IsNullOrWhiteSpace(name)) name = $"#{value.UserId}";
            _placeMessages.Add(name, value.Text);
            if (_channel.Current == ChatChannelSelector.Channel.Place) RefreshChatLog();
        }

        private void OnWorldChat(GlobalChat value)
        {
            if (value == null) return;
            _worldMessages.Add(value.Sender, value.Text);
            if (_channel.Current == ChatChannelSelector.Channel.Community) RefreshChatLog();
        }

        private void RefreshChatLog()
        {
            if (_chatLog == null || _channel == null) return;
            var entries = _channel.Current == ChatChannelSelector.Channel.Place
                ? _placeMessages.Entries : _worldMessages.Entries;
            var first = Mathf.Max(0, entries.Count - 6);
            var builder = new StringBuilder();
            for (var i = first; i < entries.Count; i++)
            {
                if (builder.Length > 0) builder.Append('\n');
                builder.Append(entries[i].Sender).Append(": ").Append(entries[i].Text);
            }
            _chatLog.text = builder.ToString();
        }

        private void UpdatePlaceholder()
        {
            if (_chatInput?.placeholder is Text placeholder)
                placeholder.text = _channel != null && _channel.Current == ChatChannelSelector.Channel.Community
                    ? "Nhập tin nhắn thế giới…" : "Nhập tin nhắn…";
        }

        private void SubmitChat()
        {
            var text = _chatInput.text.Trim();
            if (text.Length == 0) return;

            if (_channel == null || _channel.Current == ChatChannelSelector.Channel.Place)
                _chat.SendChat(text);
            else
                Send?.Invoke(ChatChannelPackets.SendGlobal(text));

            _chatInput.text = string.Empty;
            _chatInput.ActivateInputField();
        }
    }
}
