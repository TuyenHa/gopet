using System.Collections.Generic;
using Gopet.Net.Images;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Hành vi của <see cref="ImageHandler"/>: gộp request, giới hạn gói đang bay,
    /// và hết hạn. Không cái nào kiểm được bằng test byte.
    /// </summary>
    public sealed class ImageHandlerTests
    {
        private long _now;
        private readonly List<Message> _sent = new List<Message>();

        /// <summary>Dựng gói trả lời y như server.</summary>
        private static Message Response(string path, sbyte type = 2)
        {
            using var built = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(0).PutSByte(type).PutUtf(path)
                .PutInt(4).PutBytes(new byte[] { 1, 2, 3, 4 });

            return Message.FromWire(built.ToWire(), false);
        }

        private static MessageRouter RouterFor(ImageHandler handler)
        {
            var router = new MessageRouter();
            handler.RegisterOn(router);
            return router;
        }

        [Fact]
        public void NhieuNoiXinCungMotAnh_ChiGuiMotGoi()
        {
            // Mở một menu 50 icon giống nhau không được thành 50 gói.
            var handler = new ImageHandler(_sent.Add, () => _now);
            RouterFor(handler);

            var delivered = 0;
            for (var i = 0; i < 50; i++)
            {
                handler.Request("npcs/arena.png", 2, _ => delivered++);
            }

            Assert.Single(_sent);
            Assert.Equal(0, delivered);
        }

        [Fact]
        public void KhiCoTraLoi_MoiNguoiChoDeuDuocGoi()
        {
            var handler = new ImageHandler(_sent.Add, () => _now);
            var router = RouterFor(handler);

            var delivered = 0;
            for (var i = 0; i < 50; i++)
            {
                handler.Request("npcs/arena.png", 2, _ => delivered++);
            }

            router.Dispatch(Response("npcs/arena.png"));

            Assert.Equal(50, delivered);
            Assert.Equal(0, handler.InFlightCount);
        }

        [Fact]
        public void GhepTheoOriginPath_KhongPhaiDuongDaGiai()
        {
            // Client xin bằng id vật phẩm; server tra ra đường dẫn thật nhưng DỘI
            // LẠI con số ban đầu. Ghép theo đường đã giải là ghép trượt.
            var handler = new ImageHandler(_sent.Add, () => _now);
            var router = RouterFor(handler);

            ImageResponse got = null;
            handler.Request("123", 3, r => got = r);

            router.Dispatch(Response("123", 3));

            Assert.NotNull(got);
            Assert.Equal("123", got.Path);
        }

        [Fact]
        public void TraLoiChoDuongDanKhongXin_KhongLamHongTrangThai()
        {
            // Bản cũ của test này chỉ assert InFlightCount == 0 khi chưa xin gì —
            // không thể đỏ. Giờ xin thật một cái rồi mới bắn gói lạc vào.
            var handler = new ImageHandler(_sent.Add, () => _now);
            var router = RouterFor(handler);

            var delivered = 0;
            handler.Request("dang-cho.png", 2, _ => delivered++);

            router.Dispatch(Response("khong-ai-xin.png"));

            Assert.Equal(1, handler.InFlightCount);
            Assert.Equal(0, delivered);

            router.Dispatch(Response("dang-cho.png"));
            Assert.Equal(1, delivered);
            Assert.Equal(0, handler.InFlightCount);
        }

        [Fact]
        public void GioiHanSoGoiDangBay()
        {
            var handler = new ImageHandler(_sent.Add, () => _now) { MaxInFlight = 8 };
            RouterFor(handler);

            for (var i = 0; i < 20; i++)
            {
                handler.Request($"npcs/{i}.png", 2, _ => { });
            }

            Assert.Equal(8, _sent.Count);
            Assert.Equal(8, handler.InFlightCount);
            Assert.Equal(12, handler.QueuedCount);
        }

        [Fact]
        public void CoTraLoi_ThiDayTiepHangDoi()
        {
            var handler = new ImageHandler(_sent.Add, () => _now) { MaxInFlight = 8 };
            var router = RouterFor(handler);

            for (var i = 0; i < 20; i++)
            {
                handler.Request($"npcs/{i}.png", 2, _ => { });
            }

            router.Dispatch(Response("npcs/0.png"));

            Assert.Equal(9, _sent.Count);
            Assert.Equal(11, handler.QueuedCount);
        }

        [Fact]
        public void HetHan_BanSuKienVaGiaiPhongChoDangBay()
        {
            // requestImg chỉ `return` khi đường dẫn không tồn tại — server KHÔNG
            // báo lỗi. Không tự đặt hạn thì callback treo vĩnh viễn.
            var handler = new ImageHandler(_sent.Add, () => _now) { TimeoutMs = 5000 };
            RouterFor(handler);

            string timedOut = null;
            handler.TimedOut += p => timedOut = p;

            handler.Request("khong-ton-tai.png", 2, _ => { });

            _now = 4999;
            handler.Tick();
            Assert.Null(timedOut);

            _now = 5000;
            handler.Tick();

            Assert.Equal("khong-ton-tai.png", timedOut);
            Assert.Equal(0, handler.InFlightCount);
        }

        [Fact]
        public void HetHan_GoiWaiterVoiSyntheticFail_GoiMuonBiBoQua()
        {
            // Đổi hợp đồng 2026-09-17 (plan 260917-1916 phase-01): trước đây timeout
            // silently drop waiter → RemoteAssetCache đọng ở placeholder → NPC vô hình.
            // Giờ waiter nhận response Png=null → downstream swap sang FailedTexture.
            // Gói muộn tới sau vẫn bị bỏ (path đã xoá khỏi _pending).
            var handler = new ImageHandler(_sent.Add, () => _now) { TimeoutMs = 5000 };
            var router = RouterFor(handler);

            var responses = new List<ImageResponse>();
            handler.Request("cham.png", 2, r => responses.Add(r));

            _now = 6000;
            handler.Tick();

            router.Dispatch(Response("cham.png"));

            Assert.Single(responses);
            Assert.Null(responses[0].Png);
        }
    }
}
