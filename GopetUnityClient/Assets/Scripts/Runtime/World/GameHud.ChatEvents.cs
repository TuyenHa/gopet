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
            var entries = _placeMessages.Entries;
            if (_channel.Current == ChatChannelSelector.Channel.Community)
                entries = _worldMessages.Entries;
            else if (_channel.Current == ChatChannelSelector.Channel.Guild)
                entries = _guildMessages.Entries;
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
            {
                if (_channel != null && _channel.Current == ChatChannelSelector.Channel.Community)
                    placeholder.text = "Nhập tin nhắn thế giới…";
                else if (_channel != null && _channel.Current == ChatChannelSelector.Channel.Guild)
                    placeholder.text = ClanIdProvider?.Invoke() > 0
                        ? "Nhập tin nhắn bang hội…" : "Bạn chưa tham gia bang hội";
                else placeholder.text = "Nhập tin nhắn…";
            }
        }

        private void SubmitChat()
        {
            var text = _chatInput.text.Trim();
            if (text.Length == 0) return;
            if (_channel == null || _channel.Current == ChatChannelSelector.Channel.Place)
                _chat.SendChat(text);
            else if (_channel.Current == ChatChannelSelector.Channel.Community)
                Send?.Invoke(ChatChannelPackets.SendGlobal(text));
            else
            {
                if (ClanIdProvider?.Invoke() <= 0) return;
                GuildChatRequested?.Invoke(text);
            }
            _chatInput.text = string.Empty;
            _chatInput.ActivateInputField();
        }

        public void ShowGuildChatHistory(GuildChatHistory history)
        {
            _guildMessages.Clear();
            if (history?.Messages != null)
                foreach (var message in history.Messages)
                    _guildMessages.Add(message.Who, message.Text);
            if (_channel?.Current == ChatChannelSelector.Channel.Guild) RefreshChatLog();
        }

        public void AppendGuildChat(GuildChatIncoming message)
        {
            if (message == null) return;
            _guildMessages.Add(message.Who, message.Text);
            if (_channel?.Current == ChatChannelSelector.Channel.Guild) RefreshChatLog();
        }
    }
}
