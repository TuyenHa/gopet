using Gopet.Net.Battle;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class SpectatorBattlesTests
    {
        private static BattleStart Start(int id = 1) => new BattleStart
        {
            BattleId = id, Kind = BattleKind.Player, TurnDurationMs = 1000,
            LocalPet = new BattlePet { ActorId = id, Hp = 100, MaxHp = 100, Mp = 20, MaxMp = 20 },
            Opponent = new BattlePet { ActorId = id + 100, Hp = 80, MaxHp = 80, Mp = 10, MaxMp = 10 }
        };

        [Fact] public void ObserverUpdatesBothPetsWithoutMutatingPacket()
        {
            var state = new SpectatorBattles(); var packet = Start();
            Assert.True(state.Start(packet, 0));
            state.Apply(new BattleTurn { BattleId = 1, ActorId = 101, MainMpDelta = -3,
                Effects = new[] { new BattleEffect { ActorId = 1, HpDelta = -25 } } }, 1);
            Assert.Equal(75, state.Find(1).Left.Hp);
            Assert.Equal(7, state.Find(1).Right.Mp);
            Assert.Equal(100, packet.LocalPet.Hp);
        }

        [Fact] public void CapAndRepeatedSnapshotsDoNotLeakSlots()
        {
            var state = new SpectatorBattles();
            for (var i = 1; i <= 5; i++) Assert.True(state.Start(Start(i), 0));
            Assert.False(state.Start(Start(6), 0));
            Assert.True(state.Start(Start(1), 1));
            Assert.Equal(5, state.Count);
            state.Remove(101);
            Assert.Equal(4, state.Count);
            Assert.True(state.Start(Start(6), 1));
            state.Clear(); Assert.Equal(0, state.Count);
        }

        [Fact] public void OwnBattleIsNotSpectatedAndResultExpires()
        {
            var state = new SpectatorBattles(); var own = Start(); own.IsParticipant = true;
            Assert.False(state.Start(own, 0));
            state.Start(Start(), 0);
            state.End(new BattleResult { BattleId = 1, WinnerId = 101 }, 1);
            state.Expire(2.9); Assert.Equal(1, state.Count);
            state.Expire(3); Assert.Equal(0, state.Count);
        }

        [Fact] public void MissingEndPacketTimesOutAndLateTurnsAreIgnored()
        {
            var state = new SpectatorBattles(); state.Start(Start(), 0);
            state.Expire(3); Assert.Equal(0, state.Count);
            Assert.Null(state.Apply(new BattleTurn { BattleId = 1 }, 4));
        }

        [Fact] public void FinishedBattleDoesNotTakeMoreDamageOrExtendResult()
        {
            var state = new SpectatorBattles(); state.Start(Start(), 0);
            state.End(new BattleResult { BattleId = 1 }, 1);
            state.End(new BattleResult { BattleId = 1 }, 2);
            Assert.Null(state.Apply(new BattleTurn { BattleId = 1, ActorId = 1, MainMpDelta = -10 }, 2));
            state.Expire(3); Assert.Equal(0, state.Count);
        }
    }
}
