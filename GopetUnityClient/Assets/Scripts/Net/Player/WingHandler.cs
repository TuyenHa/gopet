using System;

namespace Gopet.Net.Player
{
    public sealed class WingUpdate
    {
        public int UserId;
        public string FrameImagePath;
        public int FrameCount;
    }

    /// <summary>Wing inventory actions and WING/3 place synchronization.</summary>
    public sealed class WingHandler
    {
        public const sbyte Sync = 3;
        private readonly Action<Message> _send;
        public event Action<WingUpdate[]> WingsReceived;

        public WingHandler(Action<Message> send) =>
            _send = send ?? throw new ArgumentNullException(nameof(send));

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.WING, OnWing);
        }

        public void RequestInventory() => _send(Header(GopetCmd.WING_TYPE_INVENTORY));
        public void Use(int inventoryIndex) =>
            _send(Header(GopetCmd.WING_TYPE_USE).PutInt(inventoryIndex));
        public void Unequip() => _send(Header(GopetCmd.WING_TYPE_UNEQUIP));
        public void Enchant(int inventoryIndex) =>
            _send(Header(GopetCmd.WING_TYPE_ENCHANT).PutInt(inventoryIndex));

        private void OnWing(Message message)
        {
            var type = message.Reader.ReadSByte();
            if (type != Sync) throw new ProtocolException($"WING server type chưa hỗ trợ: {type}.");
            var count = message.Reader.ReadInt();
            if (count < 0 || count > 256)
                throw new ProtocolException($"WING/3 có {count} bản ghi — ngoài khoảng hợp lệ.");
            var updates = new WingUpdate[count];
            for (var i = 0; i < count; i++)
            {
                var userId = message.Reader.ReadInt();
                var path = message.Reader.ReadUtf();
                var frames = message.Reader.ReadSByte();
                if (!string.IsNullOrEmpty(path) && (frames <= 0 || frames > 64))
                    throw new ProtocolException($"WING/3 có frameCount không hợp lệ: {frames}.");
                updates[i] = new WingUpdate
                {
                    UserId = userId,
                    FrameImagePath = path,
                    FrameCount = frames
                };
            }
            message.Reader.ExpectFullyConsumed("WING/3");
            WingsReceived?.Invoke(updates);
        }

        private static Message Header(sbyte type) =>
            Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.WING).PutSByte(type);
    }
}
