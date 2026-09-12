using Gopet.Net.Chat;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private readonly ChatTranscript _placeChat = new ChatTranscript(50);
        private readonly ChatTranscript _globalChat = new ChatTranscript(200);
        private ChatHistoryView _chatHistoryView;
        private bool _showingGlobalChat;

        private void InitializeChatHistory()
        {
            _chatHandler.ChatReceived += value =>
            {
                _placeChat.Add($"#{value.UserId}", value.Text);
                if (!_showingGlobalChat) _chatHistoryView?.Refresh(_placeChat);
            };
            _chatHandler.GlobalChatReceived += OnGlobalChat;
        }

        private void OnGlobalChat(GlobalChat value)
        {
            _globalChat.Add(value.Sender, value.Text);
            if (_showingGlobalChat) _chatHistoryView?.Refresh(_globalChat);
        }

        private void OpenChatHistory(bool global)
        {
            CloseChatHistory();
            _showingGlobalChat = global;
            var transcript = global ? _globalChat : _placeChat;
            _chatHistoryView = ChatHistoryView.Create(_hudParent,
                global ? "Chat cộng đồng" : "Chat khu vực", transcript);
            _chatHistoryView.CloseRequested += CloseChatHistory;
            _chatHistoryView.SendRequested += text =>
            {
                if (global)
                {
                    if (!TryChatCooldown("global-chat")) return;
                    _client.Send(ChatChannelPackets.SendGlobal(text));
                }
                else _chatHandler.SendChat(text);
            };
        }

        private bool TryChatCooldown(string channel)
        {
            if (_actionThrottle.TryAcquire(channel, 2000, out var remaining)) return true;
            ShowToast($"Vui lòng chờ {(remaining + 999) / 1000} giây trước khi gửi tiếp");
            return false;
        }

        private void CloseChatHistory()
        {
            if (_chatHistoryView != null) Object.Destroy(_chatHistoryView.gameObject);
            _chatHistoryView = null;
        }
    }
}
