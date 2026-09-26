using Gopet.Net.Battle;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Server không có gói "quái chết": thắng quái chỉ có FAST_REMOVE_MOB mang
    /// battleId. Client phải tự suy ra mobId từ ATTACK_MOB để gỡ xác quái khỏi map.</summary>
    public sealed partial class BattleHandlerTests
    {
        [Fact]
        public void FastRemoveSauTranQuai_BaoMobKilledDungMobId()
        {
            var (handler, router) = NewHandler(77);
            var killed = -1;
            handler.MobKilled += id => killed = id;
            StartMobBattle(router, 77, 9001);
            using var remove = Message.Create(GopetCmd.FAST_REMOVE_MOB).PutInt(77);
            Dispatch(router, remove);

            Assert.Equal(9001, killed);
        }

        [Fact]
        public void ThuaQuaiRoiDanhPvp_FastRemoveKhongGoNhamQuaiConSong()
        {
            var (handler, router) = NewHandler(77);
            var killed = -1;
            handler.MobKilled += id => killed = id;
            StartMobBattle(router, 77, 9001);
            // Quái thắng: winnerId = mobId ≠ battleId.
            using var lose = Message.Create(GopetCmd.PET_BATTLE_STATE)
                .PutInt(77).PutInt(9001).PutSByte(0).PutInt(0).PutInt(0).PutSByte(0);
            Dispatch(router, lose);
            using var remove = Message.Create(GopetCmd.FAST_REMOVE_MOB).PutInt(77);
            Dispatch(router, remove);

            Assert.Equal(-1, killed);
        }

        [Fact]
        public void FastRemoveKhongCoTranQuai_KhongBaoMobKilled()
        {
            var (handler, router) = NewHandler(7);
            var fired = false;
            handler.MobKilled += _ => fired = true;
            using var remove = Message.Create(GopetCmd.FAST_REMOVE_MOB).PutInt(77);
            Dispatch(router, remove);

            Assert.False(fired);
        }

        private static void StartMobBattle(MessageRouter router, int ownerId, int mobId)
        {
            using var packet = Message.Create(GopetCmd.ATTACK_MOB)
                .PutInt(2500).PutInt(15000).PutInt(ownerId);
            WriteOwned(packet, 11, "pets/me.png", "Mèo", 420, 90, 500, 100);
            packet.PutInt(mobId);
            WritePassive(packet, 22, "pets/mob.png", "Khủng long", 350, 1, 400, 1, false);
            Dispatch(router, packet);
        }
    }
}
