using Gopet.Net;
using Gopet.Net.Player;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Kiểm PlayerStatsHandler với 3 sub-command PET_SERVICE:
    /// MONEY_INFO (25), STAR_INFO (94), ENERGY_INFO (102).
    /// Wire theo GameController.cs:1618-1654.
    /// </summary>
    public sealed class PlayerStatsHandlerTests
    {
        private static (MessageRouter router, PlayerStatsHandler handler) BuildRouter()
        {
            var router = new MessageRouter();
            var handler = new PlayerStatsHandler();
            handler.RegisterOn(router);
            return (router, handler);
        }

        private static void Dispatch(MessageRouter router, sbyte sub, System.Action<Message> body)
        {
            using var built = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);
            body(built);
            router.Dispatch(Message.FromWire(built.ToWire(), false));
        }

        [Fact]
        public void MoneyInfo_DocDungTien4LoaiVaExtras()
        {
            var (router, handler) = BuildRouter();
            PlayerStats got = null;
            handler.StatsUpdated += s => got = s;

            Dispatch(router, GopetCmd.MONEY_INFO, m => m
                .PutInt(150)             // star
                .PutLong(1_000_000L)     // gold
                .PutLong(250L)           // coin
                .PutLong(9_999_999_999L) // lua
                .PutInt(2)               // extras count
                .PutUtf("icons/ruby.png").PutLong(7)
                .PutUtf("icons/star_shard.png").PutLong(42));

            Assert.NotNull(got);
            Assert.Equal(150, got.Star);
            Assert.Equal(1_000_000L, got.Gold);
            Assert.Equal(250L, got.Coin);
            Assert.Equal(9_999_999_999L, got.Lua);
            Assert.Equal(2, got.Extras.Count);
            Assert.Equal("icons/ruby.png", got.Extras[0].IconPath);
            Assert.Equal(7, got.Extras[0].Count);
            Assert.Equal("icons/star_shard.png", got.Extras[1].IconPath);
            Assert.Equal(42, got.Extras[1].Count);
        }

        [Fact]
        public void MoneyInfo_KhongCoExtras_VanChayDuoc()
        {
            var (router, handler) = BuildRouter();
            PlayerStats got = null;
            handler.StatsUpdated += s => got = s;

            Dispatch(router, GopetCmd.MONEY_INFO, m => m
                .PutInt(0).PutLong(0).PutLong(0).PutLong(0).PutInt(0));

            Assert.NotNull(got);
            Assert.Empty(got.Extras);
        }

        [Fact]
        public void MoneyInfo_ExtrasCountAm_Nem()
        {
            var (router, handler) = BuildRouter();

            Assert.Throws<ProtocolException>(() =>
                Dispatch(router, GopetCmd.MONEY_INFO, m => m
                    .PutInt(0).PutLong(0).PutLong(0).PutLong(0).PutInt(-1)));
        }

        [Fact]
        public void StarInfo_ChiCapNhatStar_GiuNguyenTien()
        {
            var (router, handler) = BuildRouter();

            // Nạp trạng thái ban đầu qua MONEY_INFO
            Dispatch(router, GopetCmd.MONEY_INFO, m => m
                .PutInt(10).PutLong(500).PutLong(20).PutLong(30).PutInt(0));

            PlayerStats got = null;
            handler.StatsUpdated += s => got = s;

            Dispatch(router, GopetCmd.STAR_INFO, m => m.PutInt(99));

            Assert.Equal(99, got.Star);
            Assert.Equal(500L, got.Gold);
            Assert.Equal(20L, got.Coin);
            Assert.Equal(30L, got.Lua);
        }

        [Fact]
        public void EnergyInfo_BoQua3IntSau_ChiLayStar()
        {
            var (router, handler) = BuildRouter();
            PlayerStats got = null;
            handler.StatsUpdated += s => got = s;

            Dispatch(router, GopetCmd.ENERGY_INFO, m => m
                .PutInt(77).PutInt(1).PutInt(2).PutInt(3));

            Assert.Equal(77, got.Star);
        }

        [Fact]
        public void VersionTangMoiLan()
        {
            var (router, handler) = BuildRouter();
            int lastVersion = -1;
            handler.StatsUpdated += s => lastVersion = s.Version;

            Dispatch(router, GopetCmd.STAR_INFO, m => m.PutInt(1));
            Assert.Equal(1, lastVersion);
            Dispatch(router, GopetCmd.STAR_INFO, m => m.PutInt(2));
            Assert.Equal(2, lastVersion);
        }

        [Fact]
        public void SnapshotLaCopyDocLap()
        {
            var (router, handler) = BuildRouter();
            Dispatch(router, GopetCmd.STAR_INFO, m => m.PutInt(50));

            var a = handler.Snapshot;
            Dispatch(router, GopetCmd.STAR_INFO, m => m.PutInt(60));
            var b = handler.Snapshot;

            Assert.Equal(50, a.Star);
            Assert.Equal(60, b.Star);
        }
    }
}
