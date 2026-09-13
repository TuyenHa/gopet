using System.Collections.Generic;
using Gopet.Net.Player;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class WingHandlerTests
    {
        [Fact]
        public void Sync_ParsesListAndUnequip()
        {
            var router = new MessageRouter();
            var handler = new WingHandler(_ => { });
            handler.RegisterOn(router);
            WingUpdate[] received = null;
            handler.WingsReceived += value => received = value;
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.WING)
                .PutSByte(WingHandler.Sync).PutInt(2)
                .PutInt(7).PutUtf("anim_wings/3.png").PutSByte(4)
                .PutInt(8).PutUtf("").PutSByte(0);

            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Equal(2, received.Length);
            Assert.Equal(7, received[0].UserId);
            Assert.Equal(4, received[0].VerticalOffset);
            Assert.Equal(string.Empty, received[1].FrameImagePath);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(257)]
        public void Sync_RejectsInvalidCount(int count)
        {
            var router = new MessageRouter();
            var handler = new WingHandler(_ => { });
            handler.RegisterOn(router);
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.WING)
                .PutSByte(WingHandler.Sync).PutInt(count);
            Assert.Throws<ProtocolException>(() => router.Dispatch(Message.FromWire(message.ToWire(), false)));
        }

        [Fact]
        public void Actions_UseNestedWingWireLayouts()
        {
            var sent = new List<Message>();
            var handler = new WingHandler(sent.Add);
            handler.RequestInventory();
            handler.Use(3);
            handler.Enchant(4);
            handler.Unequip();

            Assert.Equal(4, sent.Count);
            AssertPacket(sent[0], GopetCmd.WING_TYPE_INVENTORY, null);
            AssertPacket(sent[1], GopetCmd.WING_TYPE_USE, 3);
            AssertPacket(sent[2], GopetCmd.WING_TYPE_ENCHANT, 4);
            AssertPacket(sent[3], GopetCmd.WING_TYPE_UNEQUIP, null);
        }

        private static void AssertPacket(Message message, sbyte type, int? index)
        {
            var round = Message.FromWire(message.ToWire(), false);
            Assert.Equal(GopetCmd.PET_SERVICE, round.Id);
            Assert.Equal(GopetCmd.WING, round.Reader.ReadSByte());
            Assert.Equal(type, round.Reader.ReadSByte());
            if (index.HasValue) Assert.Equal(index.Value, round.Reader.ReadInt());
            round.Reader.ExpectFullyConsumed("wing action test");
        }
    }
}
