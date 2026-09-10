using System;

namespace Gopet.Net.Map
{
    public sealed class ChannelEntry
    {
        public int ZoneId;
        public int PlayerCount;
        public bool Locked;
        public int Status;
    }

    public sealed class ChannelHandler
    {
        private const int EntryBytes = 13;
        private readonly Action<Message> _send;

        public ChannelHandler(Action<Message> send)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        public event Action<ChannelEntry[]> ChannelsReceived;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.Register(GopetCmd.ON_PLAYER_GET_CHANNEL_INFO, OnChannels);
        }

        public void RequestChannels() => _send(ChannelPackets.GetChannelInfo());

        public void ChangeChannel(int mapId, int placeId) =>
            _send(ChannelPackets.ChangeChannel(mapId, placeId, placeId, 0));

        private void OnChannels(Message message)
        {
            var remaining = message.Reader.Remaining;
            if (remaining % EntryBytes != 0)
                throw new ProtocolException($"CHANNEL_INFO dài {remaining} byte — không chia hết cho {EntryBytes}.");
            var count = remaining / EntryBytes;
            if (count > 256)
                throw new ProtocolException($"CHANNEL_INFO có {count} khu — vượt giới hạn.");
            var result = new ChannelEntry[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = new ChannelEntry
                {
                    ZoneId = message.Reader.ReadInt(),
                    PlayerCount = message.Reader.ReadInt(),
                    Locked = message.Reader.ReadBool(),
                    Status = message.Reader.ReadInt()
                };
            }
            message.Reader.ExpectFullyConsumed("ON_PLAYER_GET_CHANNEL_INFO");
            ChannelsReceived?.Invoke(result);
        }
    }
}
