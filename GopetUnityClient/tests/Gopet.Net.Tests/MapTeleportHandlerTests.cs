using System.Collections.Generic;
using Gopet.Net.Map;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class MapTeleportHandlerTests
    {
        [Fact]
        public void Request_UsesMgoTeleEnvelope()
        {
            var sent = new List<Message>();
            var handler = new MapTeleportHandler(sent.Add);
            handler.RequestOptions();

            var wire = sent[0].ToWire();
            Assert.Equal(GopetCmd.MGO_COMMAND, unchecked((sbyte)wire[0]));
            Assert.Equal(GopetCmd.TELE_MENU, unchecked((sbyte)wire[1]));
        }

        [Fact]
        public void Response_ParsesAllMapFields()
        {
            var router = new MessageRouter();
            var handler = new MapTeleportHandler(_ => { });
            handler.RegisterOn(router);
            MapTeleportOption[] received = null;
            handler.OptionsReceived += value => received = value;
            using var message = Message.Create(GopetCmd.MGO_COMMAND)
                .PutSByte(GopetCmd.TELE_MENU).PutSByte(2)
                .PutSByte(11).PutUtf("Làng").PutUtf("Làng").PutSByte(0)
                .PutSByte(26).PutUtf("Thiên đình").PutUtf("Cần cánh").PutSByte(1);

            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Equal(2, received.Length);
            Assert.Equal(26, received[1].MapId);
            Assert.Equal("Cần cánh", received[1].Description);
            Assert.Equal(1, received[1].WaypointIndex);
        }

        [Fact]
        public void Response_RejectsNegativeCount()
        {
            var router = new MessageRouter();
            var handler = new MapTeleportHandler(_ => { });
            handler.RegisterOn(router);
            using var message = Message.Create(GopetCmd.MGO_COMMAND)
                .PutSByte(GopetCmd.TELE_MENU).PutSByte(-1);

            Assert.Throws<ProtocolException>(() =>
                router.Dispatch(Message.FromWire(message.ToWire(), false)));
        }
    }
}
