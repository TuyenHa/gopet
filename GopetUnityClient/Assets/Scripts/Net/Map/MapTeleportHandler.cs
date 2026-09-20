using System;

namespace Gopet.Net.Map
{
    public sealed class MapTeleportOption
    {
        public int MapId;
        public string Name;
        public string Description;
        public int WaypointIndex;
        /// <summary>Map tồn tại nhưng người chơi chưa đủ điều kiện vào (thượng giới, hoặc chưa xong nhiệm vụ).</summary>
        public bool Locked;
        /// <summary>
        /// Lý do khoá do SERVER gửi (rỗng khi map mở). Client hiển thị nguyên văn chứ
        /// không tự chế câu chữ: thêm luật khoá mới bên server thì client không phải sửa.
        /// </summary>
        public string LockReason;
    }

    /// <summary>
    /// MGO_COMMAND/TELE_MENU: danh sách MỌI map dịch chuyển kèm cờ khoá và lý do khoá.
    /// Wire-format 6 trường mỗi map — server cũ (5 trường) sẽ làm
    /// <c>ExpectFullyConsumed</c> ném, nên phải deploy server trước client.
    /// </summary>
    public sealed class MapTeleportHandler
    {
        private readonly Action<Message> _send;
        public event Action<MapTeleportOption[]> OptionsReceived;

        public MapTeleportHandler(Action<Message> send) =>
            _send = send ?? throw new ArgumentNullException(nameof(send));

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.Register(GopetCmd.MGO_COMMAND, OnMessage);
        }

        public void RequestOptions() =>
            _send(Message.Create(GopetCmd.MGO_COMMAND).PutSByte(GopetCmd.TELE_MENU));

        private void OnMessage(Message message)
        {
            var sub = message.Reader.ReadSByte();
            if (sub != GopetCmd.TELE_MENU)
                throw new ProtocolException($"MGO_COMMAND sub chưa hỗ trợ: {sub}.");
            var count = message.Reader.ReadSByte();
            if (count < 0 || count > 100)
                throw new ProtocolException($"TELE_MENU có số map không hợp lệ: {count}.");
            var options = new MapTeleportOption[count];
            for (var i = 0; i < count; i++)
            {
                options[i] = new MapTeleportOption
                {
                    MapId = message.Reader.ReadSByte(),
                    Name = message.Reader.ReadUtf(),
                    Description = message.Reader.ReadUtf(),
                    WaypointIndex = message.Reader.ReadSByte(),
                    Locked = message.Reader.ReadSByte() != 0,
                    LockReason = message.Reader.ReadUtf()
                };
            }
            message.Reader.ExpectFullyConsumed("MGO_COMMAND/TELE_MENU");
            OptionsReceived?.Invoke(options);
        }
    }
}
