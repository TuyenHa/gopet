using System;

namespace Gopet.Net.Chat
{
    /// <summary>Sự kiện chat khu vực: một người chơi nói gì đó, hoặc thực hiện tương tác đặc biệt.</summary>
    public sealed class PlaceChat
    {
        public int UserId;
        public string Text;

        /// <summary>Ba từ khoá đặc biệt trong bản gốc — server dịch thành animation, không phải chat.</summary>
        public bool IsPetInteraction => Text == "kiss" || Text == "play" || Text == "poke";
    }

    /// <summary>
    /// Chat khu vực (<c>ON_PLACE_CHAT</c>, opcode 9). Thuần C# — testable, cùng pattern
    /// với <c>MapHandler</c>.
    ///
    /// <para><b>Gửi</b>: client chỉ gửi UTF text; server nhận diện người gửi qua session.
    /// <b>Nhận</b>: server broadcast <c>int userId + UTF text</c> cho toàn khu vực.</para>
    ///
    /// <para><b>Kiss/play/poke</b>: server tự dịch 3 chuỗi này thành animation pet, không
    /// broadcast lại thành chat thường. Client vẫn có thể gõ đúng chuỗi để trigger — tầng UI
    /// dùng <see cref="PlaceChat.IsPetInteraction"/> để phân biệt bong bóng chat vs animation.</para>
    /// </summary>
    public sealed class ChatHandler
    {
        private readonly Action<Message> _send;

        public ChatHandler(Action<Message> send = null)
        {
            _send = send;
        }

        /// <summary>Server bơm chat khu vực xuống — client hiện bong bóng.</summary>
        public event Action<PlaceChat> ChatReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.Register(GopetCmd.ON_PLACE_CHAT, OnChat);
        }

        /// <summary>Gửi chat khu vực. Text UTF-8, không giới hạn độ dài phía server nhưng client cắt trước ở tầng UI.</summary>
        public void SendChat(string text)
        {
            if (_send == null) throw new InvalidOperationException("ChatHandler khởi tạo không có `send`.");
            if (text == null) throw new ArgumentNullException(nameof(text));

            _send(Message.Create(GopetCmd.ON_PLACE_CHAT).PutUtf(text));
        }

        /// <summary>Wire: int userId + UTF text.</summary>
        private void OnChat(Message msg)
        {
            var r = msg.Reader;
            var evt = new PlaceChat
            {
                UserId = r.ReadInt(),
                Text = r.ReadUtf()
            };
            ChatReceived?.Invoke(evt);
        }
    }
}
