using Gopet.Net;
using Gopet.Net.Map;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Kiểm dịch gói tin map: INIT_PLAYER, ON_PLAYER_ENTER_MAP, ON_PLAYER_EXIT_PLACE.
    /// Bơm gói giả theo đúng wire format của server (<c>GopetPlace</c>), so sự kiện.
    /// </summary>
    public sealed class MapHandlerTests
    {
        [Fact]
        public void InitPlayer_DocDungUserIdTenGioiTinh()
        {
            var (handler, router) = NewHandler();
            PlayerInit received = null;
            handler.PlayerInitReceived += e => received = e;

            using var built = Message.Create(GopetCmd.INIT_PLAYER)
                .PutInt(12345)
                .PutUtf("hato")
                .PutInt(1)
                .PutSByte(0)
                .PutInt(0);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(12345, received.UserId);
            Assert.Equal("hato", received.Name);
            Assert.Equal(1, received.Gender);
        }

        [Fact]
        public void PlayerEnterMap_DocDungMoiTruongVaToaDo()
        {
            var (handler, router) = NewHandler();
            PlayerEnterMap received = null;
            handler.PlayerEntered += e => received = e;

            using var built = Message.Create(GopetCmd.ON_PLAYER_ENTER_MAP)
                .PutInt(999)
                .PutUtf("bạn")
                .PutSByte(0)   // gender
                .PutSByte(0)   // relation
                .PutSByte(3)   // speed
                .PutSByte(2)   // faceDir
                .PutSByte(5)   // waypointIndex
                .PutInt(240)
                .PutInt(120);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(999, received.UserId);
            Assert.Equal("bạn", received.Name);
            Assert.Equal(3, received.Speed);
            Assert.Equal(2, received.FaceDir);
            Assert.Equal(5, received.WaypointIndex);
            Assert.Equal(240, received.X);
            Assert.Equal(120, received.Y);
        }

        [Fact]
        public void PlayerExitPlace_ChiCanDocUserId()
        {
            var (handler, router) = NewHandler();
            PlayerExitPlace received = null;
            handler.PlayerExited += e => received = e;

            // Server gửi: int userId + sbyte faceDir + int 0. Handler chỉ đọc userId.
            using var built = Message.Create(GopetCmd.ON_PLAYER_EXIT_PLACE)
                .PutInt(42)
                .PutSByte(1)
                .PutInt(0);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(42, received.UserId);
        }

        [Fact]
        public void PlayerMoved_LayHai_PhanTuCuoiLamViTriDich()
        {
            var (handler, router) = NewHandler();
            PlayerMoved received = null;
            handler.PlayerMoved += e => received = e;

            using var built = Message.Create(GopetCmd.ON_OTHER_USER_MOVE)
                .PutInt(7)
                .PutSByte(3)     // dir
                .PutInt(11)      // mapId
                .PutInt(4)       // pointCount
                .PutInt(10).PutInt(20).PutInt(30).PutInt(40); // (10,20)→(30,40)
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(7, received.UserId);
            Assert.Equal(3, received.Direction);
            Assert.Equal(30, received.X);
            Assert.Equal(40, received.Y);
            Assert.Equal(4, received.Points.Length);
        }

        [Fact]
        public void SendMove_DungWireFormatKhopServer()
        {
            Message sent = null;
            var handler = new MapHandler(m => sent = m);

            handler.SendMove(userId: 7, direction: 1, mapId: 11, points: new[] { 24, 24, 240, 120 });

            // Đọc lại chính gói vừa build để chắc format khớp.
            var round = Message.FromWire(sent.ToWire(), false);
            var r = round.Reader;
            Assert.Equal(GopetCmd.ON_OTHER_USER_MOVE, round.Id);
            Assert.Equal(7, r.ReadInt());
            Assert.Equal(1, r.ReadSByte());
            Assert.Equal(11, r.ReadInt());
            Assert.Equal(4, r.ReadInt());
            Assert.Equal(24, r.ReadInt());
            Assert.Equal(24, r.ReadInt());
            Assert.Equal(240, r.ReadInt());
            Assert.Equal(120, r.ReadInt());
        }

        [Fact]
        public void MapUpdated_LayDungToaDoSelf_VaListNguoiKhac()
        {
            var (handler, router) = NewHandler();
            MapUpdate received = null;
            handler.MapUpdated += e => received = e;

            using var built = Message.Create(GopetCmd.ON_UPDATE_PLAYER_IN_MAP)
                .PutInt(11)         // mapId
                .PutInt(0)          // zoneId
                .PutSByte(-1)       // selfWaypoint
                .PutInt(240)        // selfX
                .PutInt(120)        // selfY
                // 2 người khác
                .PutInt(101).PutUtf("alice").PutSByte(0).PutSByte(0).PutSByte(3).PutSByte(0).PutInt(50).PutInt(50)
                .PutInt(102).PutUtf("bob").PutSByte(1).PutSByte(0).PutSByte(3).PutSByte(2).PutInt(200).PutInt(300);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.NotNull(received);
            Assert.Equal(11, received.MapId);
            Assert.Equal(240, received.SelfX);
            Assert.Equal(120, received.SelfY);
            Assert.Equal(2, received.Others.Length);
            Assert.Equal("alice", received.Others[0].Name);
            Assert.Equal(200, received.Others[1].X);
        }

        [Fact]
        public void MapUpdated_KhongCoNguoiKhac_TraMangRong()
        {
            var (handler, router) = NewHandler();
            MapUpdate received = null;
            handler.MapUpdated += e => received = e;

            using var built = Message.Create(GopetCmd.ON_UPDATE_PLAYER_IN_MAP)
                .PutInt(11).PutInt(0).PutSByte(-1).PutInt(100).PutInt(100);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.Empty(received.Others);
        }

        [Fact]
        public void SendMove_TuChoiMangDoDaiNgoaiPham()
        {
            var handler = new MapHandler(_ => { });
            Assert.Throws<System.ArgumentException>(() => handler.SendMove(1, 0, 11, new[] { 10 }));
            var tooBig = new int[128];
            Assert.Throws<System.ArgumentException>(() => handler.SendMove(1, 0, 11, tooBig));
        }

        [Fact]
        public void SendWarp_DungWireFormatCuaServer()
        {
            Message sent = null;
            var handler = new MapHandler(m => sent = m);
            handler.SendWarp(12, 3, 1);

            var round = Message.FromWire(sent.ToWire(), false);
            Assert.Equal(GopetCmd.ON_PLAYER_WARPING, round.Id);
            Assert.Equal(12, round.Reader.ReadInt());
            Assert.Equal(3, round.Reader.ReadInt());
            Assert.Equal(1, round.Reader.ReadInt());
            Assert.Equal(0, round.Reader.Remaining);
        }

        private static (MapHandler, MessageRouter) NewHandler()
        {
            var router = new MessageRouter();
            var handler = new MapHandler();
            handler.RegisterOn(router);
            return (handler, router);
        }
    }
}
