using System;
using System.Collections.Generic;
using Gopet.Net.Auth;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Hành vi của <see cref="AuthHandler"/> — nhất là nhịp trả lời CHECK_SPEED,
    /// thứ mà mọi test byte-exact đều mù.
    /// </summary>
    public sealed class AuthHandlerTests
    {
        private long _now;
        private readonly List<Message> _sent = new List<Message>();

        private AuthHandler NewHandler() => new AuthHandler(_sent.Add, () => _now);

        /// <summary>Dựng gói CHECK_SPEED y như server: PET_SERVICE bao ngoài + sub + int.</summary>
        private static Message CheckSpeed(int waitMs)
        {
            using var built = Message.Create(GopetCmd.PET_SERVICE)
                .PutSByte(GopetCmd.CHECK_SPEED)
                .PutInt(waitMs);

            return Message.FromWire(built.ToWire(), false);
        }

        private MessageRouter RouterFor(AuthHandler auth)
        {
            var router = new MessageRouter();
            auth.RegisterOn(router);
            return router;
        }

        [Fact]
        public void CheckSpeed_KhongTraLoiNgayLapTuc()
        {
            // Server coi trả lời sớm hơn (waitMs - 2s) là speed-hack, rồi bắn lại
            // mỗi nhịp. Đây là lỗi đã từng lọt qua cả 138 test lẫn live smoke.
            var auth = NewHandler();
            var router = RouterFor(auth);

            router.Dispatch(CheckSpeed(20000));
            auth.Tick();

            Assert.Empty(_sent);
        }

        [Fact]
        public void CheckSpeed_TraLoiSauKhiDuHan()
        {
            var auth = NewHandler();
            var router = RouterFor(auth);

            router.Dispatch(CheckSpeed(20000));

            _now = 19999;
            auth.Tick();
            Assert.Empty(_sent);

            _now = 20000;
            auth.Tick();

            var reply = Assert.Single(_sent);
            Assert.Equal(GopetCmd.PET_SERVICE, reply.Id);
            Assert.Equal(new byte[] { unchecked((byte)GopetCmd.PET_SERVICE), (byte)GopetCmd.CHECK_SPEED },
                         reply.ToWire());
        }

        [Fact]
        public void CheckSpeed_ChiTraLoiMotLanChoMoiNhip()
        {
            // Tick được gọi mỗi frame; một nhịp mà trả lời nhiều lần thì cũng
            // thành bão gói y như trả lời sớm.
            var auth = NewHandler();
            var router = RouterFor(auth);

            router.Dispatch(CheckSpeed(1000));
            _now = 5000;

            auth.Tick();
            auth.Tick();
            auth.Tick();

            Assert.Single(_sent);
        }

        [Fact]
        public void CheckSpeed_NamTrongCuaSoHopLeCuaServer()
        {
            // Server: hack nếu elapsed + 2s < waitMs; đóng kết nối nếu elapsed > 3*waitMs.
            const int waitMs = 20000;
            var auth = NewHandler();
            var router = RouterFor(auth);

            router.Dispatch(CheckSpeed(waitMs));

            _now = waitMs;
            auth.Tick();

            Assert.Single(_sent);
            Assert.True(_now + 2000 >= waitMs, "trả lời quá sớm — server coi là speed-hack");
            Assert.True(_now < waitMs * 3, "trả lời quá muộn — server đóng kết nối");
        }

        [Fact]
        public void Tick_KhongCoNhipDangCho_KhongGuiGi()
        {
            var auth = NewHandler();
            RouterFor(auth);

            _now = 999999;
            auth.Tick();

            Assert.Empty(_sent);
        }

        [Fact]
        public void OkDialog_DocDuocVanBan()
        {
            // opcode 71 + sub 0 + UTF (Player.okDialog). Thiếu handler này thì bật
            // isShowMessageWhenLogin là client đứng im không rõ lý do.
            var auth = NewHandler();
            var router = RouterFor(auth);

            string shown = null;
            auth.DialogShown += t => shown = t;

            using var built = Message.Create(AuthHandler.OkDialog).PutSByte(0).PutUtf("Đang bảo trì");
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.Equal("Đang bảo trì", shown);
        }

        [Fact]
        public void RedDialog_DocDuocVanBan()
        {
            var auth = NewHandler();
            var router = RouterFor(auth);

            string shown = null;
            auth.DialogShown += t => shown = t;

            using var built = Message.Create(AuthHandler.RedDialog).PutUtf("Phiên bản cũ rồi");
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.Equal("Phiên bản cũ rồi", shown);
        }
    }
}
