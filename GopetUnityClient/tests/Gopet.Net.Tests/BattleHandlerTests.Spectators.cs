using Gopet.Net.Battle;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed partial class BattleHandlerTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ObserverSnapshotTurnAndRemoveUseActiveBattleId(bool liveStart)
        {
            var (handler, router) = NewHandler(999);
            var state = new SpectatorBattles();
            handler.BattleStarted += start => Assert.True(state.Start(start, 0));
            handler.TurnReceived += turn => state.Apply(turn, 1);
            handler.BattleRemoved += state.Remove;
            using var start = Message.Create(GopetCmd.PLAYER_BATTLE)
                .PutInt(1000).PutInt(15000).PutInt(8).PutSByte(1);
            WriteOwned(start, 10, "a", "Active", 100, 20, 100, 20);
            start.PutInt(9);
            WritePassive(start, 20, "b", "Passive", 80, 10, 80, 10, false);
            if (liveStart) start.PutBool(false);
            Dispatch(router, start);
            using var turn = Message.Create(GopetCmd.PET_BATTLE)
                .PutInt(8).PutInt(9).PutInt(12000).PutInt(15000).PutSByte(BattleTurn.Wait)
                .PutInt(0).PutUtf("").PutInt(-3).PutInt(1)
                .PutInt(8).PutInt(105).PutUtf("")
                .PutInt(0).PutInt(0).PutInt(0).PutInt(-25).PutInt(0).PutInt(0).PutInt(0);
            Dispatch(router, turn);
            Assert.Equal(75, state.Find(8).Left.Hp);
            Assert.Equal(7, state.Find(8).Right.Mp);
            using var remove = Message.Create(GopetCmd.FAST_REMOVE_MOB).PutInt(9);
            Dispatch(router, remove);
            Assert.Equal(0, state.Count);
        }
    }
}
