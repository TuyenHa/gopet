using Gopet.Net;
using Gopet.Net.Chat;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Chat khu vực opcode 9. Server format: int userId + UTF text. Client gửi chỉ UTF text.</summary>
    public sealed class ChatHandlerTests
    {
        [Fact]
        public void ChatReceived_DocDungUserIdVaText()
        {
            var router = new MessageRouter();
            var handler = new ChatHandler();
            handler.RegisterOn(router);

            PlaceChat received = null;
            handler.ChatReceived += e => received = e;

            using var built = Message.Create(GopetCmd.ON_PLACE_CHAT)
                .PutInt(42)
                .PutUtf("chào cả nhà");
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(42, received.UserId);
            Assert.Equal("chào cả nhà", received.Text);
            Assert.False(received.IsPetInteraction);
        }

        [Theory]
        [InlineData("kiss")]
        [InlineData("play")]
        [InlineData("poke")]
        public void ChatReceived_NhanDienDuocTuKhoaTuongTacPet(string keyword)
        {
            var router = new MessageRouter();
            var handler = new ChatHandler();
            handler.RegisterOn(router);

            PlaceChat received = null;
            handler.ChatReceived += e => received = e;

            using var built = Message.Create(GopetCmd.ON_PLACE_CHAT).PutInt(1).PutUtf(keyword);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.True(received.IsPetInteraction, $"Từ khoá '{keyword}' phải nhận diện thành pet interaction");
        }

        [Fact]
        public void SendChat_GuiChiUTFTextKhongUserId()
        {
            Message sent = null;
            var handler = new ChatHandler(m => sent = m);

            handler.SendChat("test");

            var round = Message.FromWire(sent.ToWire(), false);
            Assert.Equal(GopetCmd.ON_PLACE_CHAT, round.Id);
            Assert.Equal("test", round.Reader.ReadUtf());
            // Không có bytes thừa: server xác định user qua session.
        }

        [Fact]
        public void SendChat_KhongCoSend_Nem()
        {
            var handler = new ChatHandler();
            Assert.Throws<System.InvalidOperationException>(() => handler.SendChat("x"));
        }

        [Fact]
        public void GlobalChat_DocDungNguoiGuiVaNoiDung()
        {
            var router = new MessageRouter();
            var handler = new ChatHandler();
            handler.RegisterOn(router);
            GlobalChat received = null;
            handler.GlobalChatReceived += value => received = value;

            using var built = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.CHAT_GLOBAL).PutUtf("Admin").PutUtf("Thông báo");
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal("Admin", received.Sender);
            Assert.Equal("Thông báo", received.Text);
        }

        [Fact]
        public void PublicChat_DocTypeNguoiGuiVaNoiDung()
        {
            var router = new MessageRouter();
            var handler = new ChatHandler();
            handler.RegisterOn(router);
            GlobalChat received = null;
            handler.GlobalChatReceived += value => received = value;

            using var built = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.CHAT_PUBLIC).PutSByte(1).PutUtf("Linh thú").PutUtf("Xin chào");
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal("Linh thú", received.Sender);
            Assert.Equal("Xin chào", received.Text);
        }

        [Fact]
        public void PublicChat_TypeLa_NemProtocolException()
        {
            var router = new MessageRouter();
            new ChatHandler().RegisterOn(router);
            using var built = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.CHAT_PUBLIC).PutSByte(2).PutUtf("Tên").PutUtf("Tin");
            Assert.Throws<ProtocolException>(() => router.Dispatch(Message.FromWire(built.ToWire(), false)));
        }
    }
}
