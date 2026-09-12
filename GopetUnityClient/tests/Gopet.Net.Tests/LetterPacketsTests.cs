using Gopet.Net.Social;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class LetterPacketsTests
    {
        [Fact]
        public void RequestMailbox_GuiSubSbyte13()
        {
            using var message = LetterPackets.RequestMailbox();
            var read = Message.FromWire(message.ToWire(), false);
            Assert.Equal(GopetCmd.LETTER_COMMAND, read.Id);
            Assert.Equal(GopetCmd.LETTER_BOX, read.Reader.ReadSByte());
        }

        [Theory]
        [InlineData(true, GopetCmd.LETTER_COMMAND_SET_MARK)]
        [InlineData(false, GopetCmd.LETTER_COMMAND_REMOVE_LETTER)]
        public void UpdateLetter_GuiSubVaId(bool mark, int sub)
        {
            using var message = mark ? LetterPackets.Mark(42) : LetterPackets.Remove(42);
            var read = Message.FromWire(message.ToWire(), false);
            Assert.Equal((sbyte)sub, read.Reader.ReadSByte());
            Assert.Equal(42, read.Reader.ReadInt());
        }

        [Fact]
        public void Send_GuiNguoiNhanVaNoiDung()
        {
            using var message = LetterPackets.Send(" Alice ", " Hello ");
            var read = Message.FromWire(message.ToWire(), false);
            Assert.Equal(GopetCmd.LETTER_COMMAND_SEND_LETTER, read.Reader.ReadSByte());
            Assert.Equal("Alice", read.Reader.ReadUtf());
            Assert.Equal("Hello", read.Reader.ReadUtf());
        }
    }
}
