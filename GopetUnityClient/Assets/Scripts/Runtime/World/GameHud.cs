using System;
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
        private readonly ChatTranscript _guildMessages = new ChatTranscript(100);
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

        public Action GuildHistoryRequested;
        public Action<string> GuildChatRequested;

        public VirtualJoystick Joystick { get; private set; }
        public CharacterHud Character { get; private set; }
        public NotificationTicker Ticker { get; private set; }
        public TaskTrackerWidget TaskTracker { get; private set; }
        public bool IsTyping => _chatInput != null && _chatInput.isFocused;
        public bool IsChatExpanded => _chatExpanded;
        public bool IsGuildChannel => _channel != null &&
            _channel.Current == ChatChannelSelector.Channel.Guild;

        public void SetGuildAvailable(bool available) => _channel?.SetGuildAvailable(available);

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
            hud.TaskTracker = TaskTrackerWidget.Create(hud.Character.transform);
            hud.Joystick = VirtualJoystick.Create(go.transform);
            hud.Ticker = NotificationTicker.Create(go.transform);
            hud.BuildChat(go.transform, UiBuilder.DefaultFont());
            hud.BuildStatusOverlays(go.transform, UiBuilder.DefaultFont());
            hud.SubscribeChat();
            return hud;
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

    }
}
