using Gopet.Net;
using Gopet.Net.Auth;
using Gopet.Net.Chat;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class ChatChannelPacketsTests
    {
        [Fact]
        public void SendGlobal_Wire_81_10_Utf()
        {
            using var m = ChatChannelPackets.SendGlobal("hello world");
            Assert.Equal(GopetCmd.PET_SERVICE, m.Id);
            var r = Message.FromWire(m.ToWire(), false).Reader;
            Assert.Equal((sbyte)10, r.ReadSByte());
            Assert.Equal("hello world", r.ReadUtf());
        }

        [Fact]
        public void RequestGuildHistory_Wire_81_91_20()
        {
            using var m = ChatChannelPackets.RequestGuildHistory();
            Assert.Equal(GopetCmd.PET_SERVICE, m.Id);
            var r = Message.FromWire(m.ToWire(), false).Reader;
            Assert.Equal((sbyte)91, r.ReadSByte());
            Assert.Equal((sbyte)20, r.ReadSByte());
        }

        [Fact]
        public void SendGlobal_TextRong_KhongThrow()
        {
            using var m = ChatChannelPackets.SendGlobal("");
            Assert.Equal(GopetCmd.PET_SERVICE, m.Id);
        }

        [Fact]
        public void ChangePassword_Wire_93_2_7_OldUtf_NewUtf()
        {
            using var m = ChangePasswordPackets.ChangePassword("old", "new");
            Assert.Equal(GopetCmd.CHANGE_NEW_PASSWORD, m.Id);
            var r = Message.FromWire(m.ToWire(), false).Reader;
            Assert.Equal((sbyte)2, r.ReadSByte());
            Assert.Equal((sbyte)7, r.ReadSByte());
            Assert.Equal("old", r.ReadUtf());
            Assert.Equal("new", r.ReadUtf());
        }
    }
}
