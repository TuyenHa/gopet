using Gopet.Net;
using Gopet.Net.Social;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// LETTER_COMMAND (121) đối xứng: client gửi sub=sbyte, server gửi sub=INT.
    /// Test ở đây bơm gói theo format SERVER (sub INT + payload).
    /// Nguồn: GameController.cs:592-608 (showLetterBox) + 461 (letterMessage).
    /// </summary>
    public sealed class LetterHandlerTests
    {
        private static Message BuildServerLetterMessage(int sub, System.Action<Message> body)
        {
            var m = Message.Create((sbyte)121).PutInt(sub);
            body(m);
            return m;
        }

        [Fact]
        public void MailboxReceived_DocDungDanhSachThu()
        {
            var router = new MessageRouter();
            var handler = new LetterHandler();
            handler.RegisterOn(router);

            Mailbox got = null;
            handler.MailboxReceived += m => got = m;

            using var built = BuildServerLetterMessage(LetterHandler.LetterBox, m => m
                .PutInt(2)
                .PutInt(100).PutSByte(0).PutUtf("Xin chào").PutUtf("Ngắn 1").PutUtf("Dài 1").PutBool(false)
                .PutInt(101).PutSByte(1).PutUtf("Reward").PutUtf("Ngắn 2").PutUtf("Dài 2").PutBool(true));
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(got);
            Assert.Equal(2, got.Letters.Length);
            Assert.Equal(100, got.Letters[0].LetterId);
            Assert.Equal("Xin chào", got.Letters[0].Title);
            Assert.False(got.Letters[0].IsMark);
            Assert.Equal(101, got.Letters[1].LetterId);
            Assert.True(got.Letters[1].IsMark);
        }

        [Fact]
        public void MailboxTrong_KhongThrow()
        {
            var router = new MessageRouter();
            var handler = new LetterHandler();
            handler.RegisterOn(router);

            Mailbox got = null;
            handler.MailboxReceived += m => got = m;

            using var built = BuildServerLetterMessage(LetterHandler.LetterBox, m => m.PutInt(0));
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(got);
            Assert.Empty(got.Letters);
        }

        [Fact]
        public void MailboxCountAmOrHuge_Nem()
        {
            var router = new MessageRouter();
            var handler = new LetterHandler();
            handler.RegisterOn(router);

            using var built = BuildServerLetterMessage(LetterHandler.LetterBox, m => m.PutInt(-1));
            Assert.Throws<ProtocolException>(() => router.Dispatch(Message.FromWire(built.ToWire(), false)));
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]     // bất kỳ khác 0
        public void HasLetter_DocBoolFlag(int flag, bool expected)
        {
            var router = new MessageRouter();
            var handler = new LetterHandler();
            handler.RegisterOn(router);

            HasLetterNotice got = null;
            handler.HasLetterReceived += h => got = h;

            using var built = BuildServerLetterMessage(LetterHandler.HasLetter, m => m.PutSByte(flag));
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(got);
            Assert.Equal(expected, got.HasUnread);
        }
    }
}
