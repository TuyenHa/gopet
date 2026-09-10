using Gopet.Net.Player;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class CharacterAnimationHandlerTests
    {
        private readonly MessageRouter _router = new MessageRouter();
        private readonly CharacterAnimationHandler _handler = new CharacterAnimationHandler();

        public CharacterAnimationHandlerTests() => _handler.RegisterOn(_router);

        [Fact]
        public void PlayerUpdate_ParsesAllFields()
        {
            CharacterAnimationUpdate received = null;
            _handler.PlayerUpdated += value => received = value;
            Dispatch(GopetCmd.SEND_ANIMATION_CHARACTER, m => PutPlayer(m, 42, "achieve.png"));

            Assert.Equal(42, received.UserId);
            Assert.Single(received.Animations);
            var animation = received.Animations[0];
            Assert.Equal(4, animation.FrameCount);
            Assert.Equal(-8, animation.OffsetX);
            Assert.Equal(12, animation.OffsetY);
            Assert.True(animation.DrawAtEnd);
            Assert.True(animation.MirrorWithCharacter);
            Assert.Equal(3, animation.Type);
        }

        [Fact]
        public void PlayerList_ParsesCountlessRecordsUntilEof()
        {
            CharacterAnimationUpdate[] received = null;
            _handler.PlayersListed += value => received = value;
            Dispatch(GopetCmd.SEND_LIST_ANIMATION_CHARACTER, m =>
            {
                PutPlayer(m, 1, "one.png");
                m.PutInt(2).PutInt(0);
            });

            Assert.Equal(2, received.Length);
            Assert.Equal(1, received[0].UserId);
            Assert.Empty(received[1].Animations);
        }

        [Fact]
        public void PlayerUpdate_RejectsInvalidFrameCount()
        {
            Assert.Throws<ProtocolException>(() => Dispatch(GopetCmd.SEND_ANIMATION_CHARACTER, m => m
                .PutInt(1).PutInt(1).PutSByte(0)));
        }

        private static Message PutPlayer(Message message, int userId, string path) => message
            .PutInt(userId).PutInt(1)
            .PutSByte(4).PutUtf(path).PutShort(-8).PutShort(12)
            .PutBool(true).PutBool(true).PutSByte(3);

        private void Dispatch(sbyte sub, System.Action<Message> body)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);
            body(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
