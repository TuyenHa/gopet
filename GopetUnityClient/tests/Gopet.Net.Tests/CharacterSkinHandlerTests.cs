using Gopet.Net.Player;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class CharacterSkinHandlerTests
    {
        [Fact]
        public void SendSkin_ParsesInitialListAndUnequipPath()
        {
            var router = new MessageRouter();
            var handler = new CharacterSkinHandler();
            handler.RegisterOn(router);
            CharacterSkinUpdate[] received = null;
            handler.SkinsReceived += value => received = value;
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.SEND_SKIN)
                .PutInt(2).PutInt(10).PutUtf("anim_characters/5.png").PutInt(11).PutUtf("");

            router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Equal(2, received.Length);
            Assert.Equal(10, received[0].UserId);
            Assert.Equal("anim_characters/5.png", received[0].FrameImagePath);
            Assert.Equal(string.Empty, received[1].FrameImagePath);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(257)]
        public void SendSkin_RejectsInvalidCount(int count)
        {
            var router = new MessageRouter();
            var handler = new CharacterSkinHandler();
            handler.RegisterOn(router);
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.SEND_SKIN).PutInt(count);
            Assert.Throws<ProtocolException>(() => router.Dispatch(Message.FromWire(message.ToWire(), false)));
        }
    }
}
