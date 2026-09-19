using System;
using System.Collections.Generic;
using Gopet.Net.Images;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Vòng đời và kế toán trạng thái của <see cref="ImageHandler"/> — những lỗi
    /// chỉ nổ sau một chuỗi sự kiện, không bài test byte nào chạm tới.
    /// </summary>
    public sealed class ImageHandlerLifecycleTests
    {
        private long _now;
        private readonly List<Message> _sent = new List<Message>();

        private static Message Response(string path)
        {
            using var built = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(0).PutSByte(2).PutUtf(path)
                .PutInt(2).PutBytes(new byte[] { 9, 9 });

            return Message.FromWire(built.ToWire(), false);
        }

        [Fact]
        public void GoiMuonSauKhiHetHan_KhongLamLechSoGoiDangBay()
        {
            // Kịch bản làm _inFlight tụt xuống âm:
            //   xin -> hết hạn (trừ 1) -> xin lại lúc đã đủ MaxInFlight nên nằm chờ
            //   -> gói muộn của lần trước tới, khớp entry mới -> trừ lần nữa.
            // Sau đó MaxInFlight mất tác dụng vĩnh viễn.
            var handler = new ImageHandler(_sent.Add, () => _now) { MaxInFlight = 2, TimeoutMs = 1000 };
            var router = new MessageRouter();
            handler.RegisterOn(router);

            handler.Request("a.png", 2, _ => { });

            _now = 2000;
            handler.Tick();                       // "a.png" hết hạn
            Assert.Equal(0, handler.InFlightCount);

            handler.Request("b.png", 2, _ => { });
            handler.Request("c.png", 2, _ => { }); // đủ 2 gói đang bay
            handler.Request("a.png", 2, _ => { }); // nằm chờ trong hàng đợi

            Assert.Equal(2, handler.InFlightCount);
            Assert.Equal(1, handler.QueuedCount);

            router.Dispatch(Response("a.png"));    // gói muộn của lần xin ĐẦU

            Assert.Equal(2, handler.InFlightCount);
            Assert.Equal(0, handler.QueuedCount);
        }

        [Fact]
        public void Retry_KhiTimeout_GuiLaiVaChiFireTimedOutSauKhiHetSoLan()
        {
            // MaxRetries=2 → tổng 3 lần gửi. Sau 3 lần đều im, TimedOut mới fire 1 lần.
            var handler = new ImageHandler(_sent.Add, () => _now)
            {
                MaxInFlight = 4, TimeoutMs = 1000, MaxRetries = 2,
            };
            var timedOut = 0;
            handler.TimedOut += _ => timedOut++;

            handler.Request("a.png", 2, _ => { });
            Assert.Equal(1, _sent.Count);           // gửi lần 1

            _now = 1500;
            handler.Tick();                          // hết hạn lần 1 → retry
            Assert.Equal(2, _sent.Count);
            Assert.Equal(0, timedOut);

            _now = 3000;
            handler.Tick();                          // hết hạn lần 2 → retry
            Assert.Equal(3, _sent.Count);
            Assert.Equal(0, timedOut);

            _now = 4500;
            handler.Tick();                          // hết hạn lần 3 → BỎ CUỘC
            Assert.Equal(3, _sent.Count);
            Assert.Equal(1, timedOut);
            Assert.Equal(0, handler.InFlightCount);
        }

        [Fact]
        public void HetRetry_WaiterVanNhanResponseVoiPngNull()
        {
            // Bug lịch sử: hết retry → _pending.Remove() xoá Waiters → callback từ
            // RemoteAssetCache.Get không bao giờ chạy → NPC đọng ở placeholder 1×1
            // trong suốt → chỉ thấy tên. Giờ ImageHandler bắn synthetic response
            // (Png=null) trước khi TimedOut để downstream biết fail vĩnh viễn.
            var handler = new ImageHandler(_sent.Add, () => _now)
            {
                TimeoutMs = 1000, MaxRetries = 1,
            };
            ImageResponse received = null;
            var timedOut = 0;
            handler.TimedOut += _ => timedOut++;
            handler.Request("mgo.png", 2, r => received = r);

            _now = 1500; handler.Tick();             // retry lần 1
            _now = 3000; handler.Tick();             // hết retry → notify + TimedOut

            Assert.NotNull(received);
            Assert.Equal("mgo.png", received.Path);
            Assert.Null(received.Png);
            Assert.Equal(1, timedOut);
        }

        [Fact]
        public void Retry_ResponseVeGiuaChung_HuyRetryVaNotifyWaiter()
        {
            var handler = new ImageHandler(_sent.Add, () => _now)
            {
                TimeoutMs = 1000, MaxRetries = 2,
            };
            var router = new MessageRouter();
            handler.RegisterOn(router);

            var delivered = 0;
            handler.Request("a.png", 2, _ => delivered++);

            _now = 1500;
            handler.Tick();                          // retry lần 1
            Assert.Equal(2, _sent.Count);

            router.Dispatch(Response("a.png"));      // gói lần retry về
            Assert.Equal(1, delivered);
            Assert.Equal(0, handler.InFlightCount);
        }

        [Fact]
        public void SoGoiDangBay_KhongBaoGioAm()
        {
            var handler = new ImageHandler(_sent.Add, () => _now) { TimeoutMs = 1000 };
            var router = new MessageRouter();
            handler.RegisterOn(router);

            handler.Request("a.png", 2, _ => { });
            _now = 2000;
            handler.Tick();

            router.Dispatch(Response("a.png"));
            router.Dispatch(Response("a.png"));

            Assert.True(handler.InFlightCount >= 0, $"_inFlight = {handler.InFlightCount}");
        }

        [Fact]
        public void MotNoiChoNemLoi_KhongKeoTheoNhungNoiConLai()
        {
            var handler = new ImageHandler(_sent.Add, () => _now);
            var router = new MessageRouter();
            handler.RegisterOn(router);

            var failures = 0;
            handler.WaiterFailed += (_, __) => failures++;

            var delivered = 0;
            handler.Request("a.png", 2, _ => throw new InvalidOperationException("waiter hỏng"));
            handler.Request("a.png", 2, _ => delivered++);
            handler.Request("a.png", 2, _ => delivered++);

            router.Dispatch(Response("a.png"));

            Assert.Equal(1, failures);
            Assert.Equal(2, delivered);
        }

        [Fact]
        public void HangDoiKhongPhinhMai()
        {
            // Xin nhiều hơn MaxInFlight rồi để hết hạn hết: cả _pending lẫn _queue
            // phải sạch, nếu không mỗi lần mất mạng lại rò thêm một ít.
            var handler = new ImageHandler(_sent.Add, () => _now) { MaxInFlight = 2, TimeoutMs = 1000 };
            handler.RegisterOn(new MessageRouter());

            for (var i = 0; i < 10; i++)
            {
                handler.Request($"{i}.png", 2, _ => { });
            }

            for (var step = 0; step < 10; step++)
            {
                _now += 2000;
                handler.Tick();
            }

            Assert.Equal(0, handler.InFlightCount);
            Assert.Equal(0, handler.QueuedCount);
        }
    }
}
