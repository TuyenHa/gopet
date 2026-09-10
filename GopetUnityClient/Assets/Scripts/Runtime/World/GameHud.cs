using System;
using Gopet.Net;
using Gopet.Net.Chat;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>HUD gameplay: joystick mobile và ô chat khu vực.</summary>
    public sealed partial class GameHud : MonoBehaviour
    {
        private const float ChatPanelWidthRatio = 0.55f;
        private const float ChatPanelMinWidth = 600f;
        private const float ChatPanelMaxWidth = 760f;

        private InputField _chatInput;
        private ChatHandler _chat;
        private ChatChannelSelector _channel;
        private RectTransform _chatPanel;

        /// <summary>Đường gửi các gói chat kênh cộng đồng/bang. Gán từ GameSession.</summary>
        public Action<Message> Send;

        /// <summary>Trả clanId của self (0 nếu chưa vào bang). GameSession bơm từ GuildInfoHandler.</summary>
        public Func<int> ClanIdProvider;

        public VirtualJoystick Joystick { get; private set; }
        public CharacterHud Character { get; private set; }
        public NotificationTicker Ticker { get; private set; }
        public bool IsTyping => _chatInput != null && _chatInput.isFocused;

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
            return hud;
        }

        private void BuildChat(Transform parent, Font font)
        {
            var panel = new GameObject("Place Chat", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            _chatPanel = (RectTransform)panel.transform;
            ApplyChatLayout();
            panel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.1f, 0.86f);

            _channel = ChatChannelSelector.Create(panel.transform);
            _chatInput = MakeInput(panel.transform, font);
            // Cần chừa chỗ cho selector 53px bên trái.
            _chatInput.transform.parent.GetComponent<RectTransform>();
            var inputRect = (RectTransform)_chatInput.transform.parent;
            inputRect.offsetMin = new Vector2(66f, 5f);
            var button = MakeButton(panel.transform, font);
            button.onClick.AddListener(SubmitChat);
        }

        private void OnEnable()
        {
            // Khi Unity hot-reload trong lúc Play, field runtime có thể mất nhưng object UI vẫn còn.
            if (_chatPanel == null)
                _chatPanel = transform.Find("Place Chat") as RectTransform;
            ApplyChatLayout();
        }

        private void OnRectTransformDimensionsChange() => ApplyChatLayout();

        private void ApplyChatLayout()
        {
            if (_chatPanel == null) return;

            var canvasRect = (RectTransform)transform;
            var width = Mathf.Clamp(canvasRect.rect.width * ChatPanelWidthRatio,
                ChatPanelMinWidth, ChatPanelMaxWidth);

            // Neo theo chính giữa của toàn Canvas, không phụ thuộc vị trí nhân vật hay tỉ lệ màn hình.
            _chatPanel.anchorMin = _chatPanel.anchorMax = new Vector2(0.5f, 0f);
            _chatPanel.pivot = new Vector2(0.5f, 0f);
            _chatPanel.anchoredPosition = new Vector2(0f, 20f);
            _chatPanel.sizeDelta = new Vector2(width, 44f);
        }

        private static InputField MakeInput(Transform parent, Font font)
        {
            var go = new GameObject("Chat Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 5f);
            rect.offsetMax = new Vector2(-86f, -5f);
            go.GetComponent<Image>().color = UiBuilder.Field;
            var input = go.GetComponent<InputField>();
            input.characterLimit = 120;
            var text = UiBuilder.MakeText(go.transform, font, "Text", 18, true);
            text.rectTransform.offsetMin = new Vector2(8f, 0f);
            text.rectTransform.offsetMax = new Vector2(-8f, 0f);
            input.textComponent = text;
            var placeholder = UiBuilder.MakeText(go.transform, font, "Placeholder", 18, true);
            placeholder.text = "Chat khu vực…";
            placeholder.color = UiBuilder.TextMuted;
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.rectTransform.offsetMin = new Vector2(8f, 0f);
            input.placeholder = placeholder;
            return input;
        }

        private static Button MakeButton(Transform parent, Font font)
        {
            var go = new GameObject("Send", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(76f, 0f);
            rect.offsetMin = new Vector2(-80f, 5f);
            rect.offsetMax = new Vector2(-5f, -5f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var label = UiBuilder.MakeText(go.transform, font, "Label", 17, true);
            label.text = "Gửi";
            label.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }

        private void SubmitChat()
        {
            var text = _chatInput.text.Trim();
            if (text.Length == 0) return;

            switch (_channel != null ? _channel.Current : ChatChannelSelector.Channel.Place)
            {
                case ChatChannelSelector.Channel.Place:
                    _chat.SendChat(text);
                    break;
                case ChatChannelSelector.Channel.Community:
                    Send?.Invoke(ChatChannelPackets.SendGlobal(text));
                    break;
                case ChatChannelSelector.Channel.Guild:
                    var clanId = ClanIdProvider != null ? ClanIdProvider() : 0;
                    if (clanId <= 0)
                    {
                        // Chưa có bang hoặc CLAN_INFO chưa tới; đẩy request để prime,
                        // đồng thời báo cho user biết vì sao send không đi.
                        Send?.Invoke(Gopet.Net.Guild.GuildPackets.RequestClanInfo());
                        Debug.Log("[Gopet] Guild chat: chưa xác định clanId — gửi CLAN_INFO trước, hãy gửi lại.");
                        break;
                    }
                    Send?.Invoke(ChatChannelPackets.SendGuildChat(clanId, text));
                    break;
            }
            _chatInput.text = string.Empty;
            _chatInput.ActivateInputField();
        }
    }
}
