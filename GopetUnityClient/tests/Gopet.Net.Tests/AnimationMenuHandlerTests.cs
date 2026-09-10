using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class AnimationMenuHandlerTests
    {
        private readonly MessageRouter _router = new MessageRouter();
        private readonly AnimationMenuHandler _handler = new AnimationMenuHandler();

        public AnimationMenuHandlerTests() => _handler.RegisterOn(_router);

        [Fact]
        public void Screen_ParsesLabelsImagesAndCommands()
        {
            AnimationMenuScreen received = null;
            _handler.ScreenShown += value => received = value;

            Dispatch(m => m.PutSByte(0).PutInt(91).PutUtf("Thành tựu")
                .PutInt(2)
                .PutSByte(0).PutBool(false).PutUtf("Thợ săn boss").PutSByte(1).PutSByte(3)
                .PutSByte(1).PutBool(false).PutUtf("achievement/boss.png").PutSByte(2)
                    .PutBool(true).PutInt(4).PutInt(2)
                .PutInt(1)
                .PutInt(7).PutSByte(2).PutUtf("Đóng").PutBool(true).PutBool(false));

            Assert.Equal(91, received.MenuId);
            Assert.Equal("Thành tựu", received.Title);
            Assert.False(received.Elements[0].IsImage);
            Assert.Equal(3, received.Elements[0].FontStyle);
            Assert.True(received.Elements[1].IsImage);
            Assert.True(received.Elements[1].Animated);
            Assert.Equal(4, received.Elements[1].FrameCount);
            Assert.True(received.Commands[0].ClosesScreen);
            Assert.False(received.Commands[0].RepliesToServer);
        }

        [Fact]
        public void Screen_RejectsUnknownElementKind()
        {
            Assert.Throws<ProtocolException>(() => Dispatch(m => m
                .PutSByte(0).PutInt(1).PutUtf("Lỗi").PutInt(1).PutSByte(9).PutBool(false)));
        }

        [Fact]
        public void Screen_RejectsUnboundedElementCount()
        {
            Assert.Throws<ProtocolException>(() => Dispatch(m => m
                .PutSByte(0).PutInt(1).PutUtf("Lỗi").PutInt(257)));
        }

        private void Dispatch(System.Action<Message> body)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.ANIMATION_MENU);
            body(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
