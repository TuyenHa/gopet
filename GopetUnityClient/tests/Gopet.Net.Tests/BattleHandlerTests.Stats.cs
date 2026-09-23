using Gopet.Net.Battle;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed partial class BattleHandlerTests
    {
        [Fact]
        public void BuffState_ParseHaiActorMoiActorHaiBuff()
        {
            var (handler, router) = NewHandler(50);
            BattleBuffState state = null;
            handler.BuffStateReceived += s => state = s;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_BUFF)
                .PutInt(50).PutSByte(2)
                .PutInt(50).PutSByte(2)
                    .PutInt(30).PutInt(1).PutSByte(2)   // STUN, val 1, 2 lượt còn lại
                    .PutInt(24).PutInt(100).PutSByte(3) // RECOVERY_HP
                .PutInt(99).PutSByte(1)
                    .PutInt(28).PutInt(50).PutSByte(5); // DAMGE_TOXIC_IN_5_TURN

            Dispatch(router, packet);

            Assert.NotNull(state);
            Assert.Equal(50, state.BattleId);
            Assert.Equal(2, state.Actors.Length);
            Assert.Equal(50, state.Actors[0].ActorId);
            Assert.Equal(2, state.Actors[0].Entries.Length);
            Assert.Equal(30, state.Actors[0].Entries[0].TypeId);
            Assert.Equal(2, state.Actors[0].Entries[0].TurnsLeft);
            Assert.Equal(24, state.Actors[0].Entries[1].TypeId);
            Assert.Equal(100, state.Actors[0].Entries[1].Value);
            Assert.Equal(99, state.Actors[1].ActorId);
            Assert.Single(state.Actors[1].Entries);
        }

        [Fact]
        public void BuffState_TooManyActorsThrowsProtocolException()
        {
            var (handler, router) = NewHandler(1);
            using var packet = Message.Create(GopetCmd.PET_BATTLE_BUFF)
                .PutInt(1).PutSByte(9); // > MaxBuffActors=2
            Assert.Throws<ProtocolException>(() => Dispatch(router, packet));
        }

        [Fact]
        public void Stats_ParseFullPacket()
        {
            var (handler, router) = NewHandler(1);
            BattleStatsState received = null;
            handler.StatsReceived += s => received = s;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_STATS)
                .PutInt(42)     // battleId
                .PutSByte(2)    // 2 actors
                .PutInt(10)     // actor 1 id
                .PutInt(5)      // level
                .PutInt(120)    // atk
                .PutInt(80)     // def
                .PutShort(230)  // critPermille
                .PutSByte(1)    // 1 skill
                .PutInt(301)    // skillId
                .PutInt(15)     // mpCost
                .PutInt(20)     // actor 2 id
                .PutInt(3)      // level
                .PutInt(65)     // atk
                .PutInt(45)     // def
                .PutShort(0)    // critPermille
                .PutSByte(0);   // 0 skills
            Dispatch(router, packet);
            Assert.NotNull(received);
            Assert.Equal(42, received.BattleId);
            Assert.Equal(2, received.Actors.Length);
            Assert.Equal(120, received.Actors[0].Atk);
            Assert.Equal(80, received.Actors[0].Def);
            Assert.Equal(230, received.Actors[0].CritPermille);
            Assert.Single(received.Actors[0].Skills);
            Assert.Equal(301, received.Actors[0].Skills[0].SkillId);
            Assert.Equal(15, received.Actors[0].Skills[0].MpCost);
            Assert.Empty(received.Actors[1].Skills);
        }

        [Fact]
        public void Stats_ZeroActorsIsValid()
        {
            var (handler, router) = NewHandler(1);
            BattleStatsState received = null;
            handler.StatsReceived += s => received = s;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_STATS)
                .PutInt(1).PutSByte(0);
            Dispatch(router, packet);
            Assert.NotNull(received);
            Assert.Empty(received.Actors);
        }

        [Fact]
        public void Stats_TooManyActorsThrows()
        {
            var (handler, router) = NewHandler(1);
            using var packet = Message.Create(GopetCmd.PET_BATTLE_STATS)
                .PutInt(1).PutSByte(5);
            Assert.Throws<ProtocolException>(() => Dispatch(router, packet));
        }

        [Fact]
        public void SendSurrender_CorrectBytes()
        {
            Message sent = null;
            var handler = new BattleHandler(m => sent = m, 1);
            handler.SendSurrender();
            Assert.NotNull(sent);
            var r = Round(sent);
            Assert.Equal(GopetCmd.PET_SERVICE, sent.Id);
            Assert.Equal(GopetCmd.PET_BATTLE, r.ReadSByte());
            Assert.Equal(GopetCmd.PET_BATTLE_SURRENDER, r.ReadSByte());
        }

    }
}
