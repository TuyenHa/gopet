using System.Collections.Generic;
using Gopet.Net.Auth;
using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class ServerMessageHandlerTests
    {
        private readonly List<Message> _sent = new List<Message>();

        private (MessageRouter router, GuiderHandler handler) CreateGuider()
        {
            var router = new MessageRouter();
            var handler = new GuiderHandler(_sent.Add);
            handler.RegisterOn(router);
            return (router, handler);
        }

        [Fact]
        public void PopupAndBanner_AreParsedAsSeparateEvents()
        {
            var pair = CreateGuider();
            string popup = null;
            string banner = null;
            pair.handler.PopupShown += text => popup = text;
            pair.handler.BannerShown += text => banner = text;

            using (var m = Message.Create(GopetCmd.SERVER_MESSAGE)
                       .PutSByte(GopetCmd.POPUP_MESSAGE).PutUtf("Không đủ thể lực"))
                pair.router.Dispatch(Message.FromWire(m.ToWire(), false));
            using (var m = Message.Create(GopetCmd.SERVER_MESSAGE)
                       .PutSByte(GopetCmd.BANNER_MESSAGE).PutUtf("Chào mừng"))
                pair.router.Dispatch(Message.FromWire(m.ToWire(), false));

            Assert.Equal("Không đủ thể lực", popup);
            Assert.Equal("Chào mừng", banner);
        }

        [Fact]
        public void BossBanner_UsesDedicatedEvent()
        {
            var pair = CreateGuider();
            string regular = null;
            string boss = null;
            pair.handler.BannerShown += text => regular = text;
            pair.handler.BossBannerShown += text => boss = text;

            using var message = Message.Create(GopetCmd.SERVER_MESSAGE)
                .PutSByte(GopetCmd.BOSS_BANNER_MESSAGE)
                .PutUtf("Boss thế giới đã xuất hiện");
            pair.router.Dispatch(Message.FromWire(message.ToWire(), false));

            Assert.Null(regular);
            Assert.Equal("Boss thế giới đã xuất hiện", boss);
        }

        [Fact]
        public void ImageDialog_ParsesCaptchaShape()
        {
            var pair = CreateGuider();
            ImageDialogSpec shown = null;
            pair.handler.ImageDialogShown += spec => shown = spec;

            using var m = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.GUIDER_IMGDIALOG)
                .PutInt(0).PutInt(160).PutInt(80)
                .PutUtf("img/captcha.png123456").PutInt(1).PutInt(0);
            pair.router.Dispatch(Message.FromWire(m.ToWire(), false));

            Assert.NotNull(shown);
            Assert.Equal(0, shown.DialogId);
            Assert.Equal(160, shown.Width);
            Assert.Equal(80, shown.Height);
            Assert.Equal("img/captcha.png123456", shown.ImagePath);
        }

        [Fact]
        public void SelectImageDialog_MatchesServerWire()
        {
            var pair = CreateGuider();

            pair.handler.SelectImageDialog(0);

            Assert.Single(_sent);
            Assert.Equal("7a0b00000000", TestVectorData.ToHex(_sent[0].ToWire()));
        }

        [Fact]
        public void AuthDialogs_KeepLegacyEventAndEmitTypedEvent()
        {
            var sent = new List<Message>();
            var auth = new AuthHandler(sent.Add);
            var router = new MessageRouter();
            auth.RegisterOn(router);
            string legacy = null;
            string error = null;
            string success = null;
            auth.DialogShown += text => legacy = text;
            auth.ErrorDialogShown += text => error = text;
            auth.SuccessDialogShown += text => success = text;

            using (var red = Message.Create(AuthHandler.RedDialog).PutUtf("Thiếu tiền"))
                router.Dispatch(Message.FromWire(red.ToWire(), false));
            Assert.Equal("Thiếu tiền", legacy);
            Assert.Equal("Thiếu tiền", error);
            Assert.Null(success);

            using (var green = Message.Create(AuthHandler.OkDialog).PutSByte(0).PutUtf("Thành công"))
                router.Dispatch(Message.FromWire(green.ToWire(), false));
            Assert.Equal("Thành công", legacy);
            Assert.Equal("Thành công", success);
        }
    }
}
