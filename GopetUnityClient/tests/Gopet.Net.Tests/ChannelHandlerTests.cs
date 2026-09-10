using Gopet.Net.Map;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class ChannelHandlerTests
    {
        [Fact]
        public void Response_HasNoCountAndParsesUntilEnd()
        {
            var router = new MessageRouter();
            var handler = new ChannelHandler(_ => { });
            handler.RegisterOn(router);
            ChannelEntry[] received = null;
            handler.ChannelsReceived += value => received = value;
            using var packet = Message.Create(GopetCmd.ON_PLAYER_GET_CHANNEL_INFO)
                .PutInt(0).PutInt(12).PutBool(false).PutInt(0)
                .PutInt(1).PutInt(30).PutBool(true).PutInt(2);

            router.Dispatch(Message.FromWire(packet.ToWire(), false));

            Assert.Equal(2, received.Length);
            Assert.Equal(12, received[0].PlayerCount);
            Assert.True(received[1].Locked);
            Assert.Equal(2, received[1].Status);
        }

        [Fact]
        public void Response_RejectsPartialEntry()
        {
            var router = new MessageRouter();
            var handler = new ChannelHandler(_ => { });
            handler.RegisterOn(router);
            using var packet = Message.Create(GopetCmd.ON_PLAYER_GET_CHANNEL_INFO).PutInt(1);

            Assert.Throws<ProtocolException>(() =>
                router.Dispatch(Message.FromWire(packet.ToWire(), false)));
        }

        [Fact]
        public void ChangeChannel_FillsFourLegacyInts()
        {
            Message sent = null;
            var handler = new ChannelHandler(value => sent = value);
            handler.ChangeChannel(11, 3);
            var round = Message.FromWire(sent.ToWire(), true);

            Assert.Equal(GopetCmd.ON_PLAYER_CHANGE_CHANNEL, round.Id);
            Assert.Equal(11, round.Reader.ReadInt());
            Assert.Equal(3, round.Reader.ReadInt());
            Assert.Equal(3, round.Reader.ReadInt());
            Assert.Equal(0, round.Reader.ReadInt());
        }
    }
}
