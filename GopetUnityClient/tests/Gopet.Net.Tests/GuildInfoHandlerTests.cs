using Gopet.Net;
using Gopet.Net.Guild;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class GuildInfoHandlerTests
    {
        [Fact]
        public void ClanInfo_CapNhatClanIdVaPhatHaiEvent()
        {
            var (router, handler) = Create();
            GuildClanInfo received = null;
            var changed = 0;
            handler.ClanInfoReceived += value => received = value;
            handler.ClanIdChanged += value => changed = value;
            using var packet = ClanPacket(GopetCmd.CLAN_INFO).PutInt(17).PutSByte(2)
                .PutUtf("Bang Rồng").PutUtf("Cấp 5");

            router.Dispatch(Message.FromWire(packet.ToWire(), false));

            Assert.Equal(17, handler.ClanId);
            Assert.Equal(17, changed);
            Assert.Equal(new[] { "Bang Rồng", "Cấp 5" }, received.DescriptionLines);
        }

        [Fact]
        public void GuildList_DocDungFixtureServerWriter()
        {
            var (router, handler) = Create();
            GuildListResponse received = null;
            handler.GuildListReceived += value => received = value;
            using var packet = ClanPacket(GopetCmd.GUILD_LIST).PutUtf("Danh sách bang")
                .PutSByte(0).PutSByte(0).PutInt(1).PutInt(9).PutInt(123)
                .PutUtf("Hiệp sĩ").PutUtf("Cùng phiêu lưu");

            router.Dispatch(Message.FromWire(packet.ToWire(), false));

            Assert.Equal("Danh sách bang", received.Title);
            Assert.Single(received.Entries);
            Assert.Equal(9, received.Entries[0].ClanId);
            Assert.Equal("Hiệp sĩ", received.Entries[0].Name);
        }

        [Fact]
        public void GuildChat_DocLichSuVaTinDen()
        {
            var (router, handler) = Create();
            GuildChatHistory history = null;
            GuildChatIncoming incoming = null;
            handler.ChatHistoryReceived += value => history = value;
            handler.ChatMessageReceived += value => incoming = value;
            using var first = ClanPacket(GopetCmd.GUILD_CHAT).PutInt(7).PutUtf("")
                .PutSByte(1).PutUtf("An").PutUtf("Xin chào");
            using var second = ClanPacket(GopetCmd.GUILD_ON_PLAYER_CHAT)
                .PutUtf("Bình").PutUtf("Chào An");

            router.Dispatch(Message.FromWire(first.ToWire(), false));
            router.Dispatch(Message.FromWire(second.ToWire(), false));

            Assert.Equal(7, history.ClanId);
            Assert.Equal("Xin chào", history.Messages[0].Text);
            Assert.Equal("Bình", incoming.Who);
        }

        [Fact]
        public void GuildList_BiCatNgan_NemProtocolException()
        {
            var (router, handler) = Create();
            handler.GuildListReceived += _ => { };
            using var packet = ClanPacket(GopetCmd.GUILD_LIST).PutUtf("thiếu phần còn lại");

            Assert.Throws<ProtocolException>(() =>
                router.Dispatch(Message.FromWire(packet.ToWire(), false)));
        }

        private static (MessageRouter Router, GuildInfoHandler Handler) Create()
        {
            var router = new MessageRouter();
            var handler = new GuildInfoHandler();
            handler.RegisterOn(router);
            return (router, handler);
        }

        private static Message ClanPacket(sbyte sub)
        {
            return Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.CLAN).PutSByte(sub);
        }
    }
}
