using System;
using Gopet.Net.Battle;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class BattleSummaryTrackerTests
    {
        [Theory]
        [InlineData(0, "0 phút 00 giây")]
        [InlineData(8.9, "0 phút 08 giây")]
        [InlineData(83, "1 phút 23 giây")]
        [InlineData(3662, "61 phút 02 giây")]
        [InlineData(-5, "0 phút 00 giây")]
        public void Duration_UsesElapsedWholeSeconds(double seconds, string expected)
        {
            var tracker = new BattleSummaryTracker(Start(), 100d);
            Assert.Equal(expected, tracker.Complete(Result(), 100d + seconds).DurationText);
        }

        [Fact]
        public void Skills_OnlyConfirmedLocalSkills_UniqueInFirstUseOrder()
        {
            var tracker = new BattleSummaryTracker(Start(), 0);
            tracker.RecordTurn(Turn(-99, 105)); // quái đánh vào pet mình
            tracker.RecordTurn(Turn(7, 0, 1, 2)); // thường, trượt, chí mạng
            tracker.RecordTurn(Turn(7, 103, 103)); // buff lên chính mình
            tracker.RecordTurn(Turn(7, 101, 1)); // đã thi triển nhưng sát thương trượt
            tracker.RecordTurn(Turn(7, 101, 999));
            var wrongBattle = Turn(7, 105);
            wrongBattle.BattleId = 8;
            tracker.RecordTurn(wrongBattle);
            Assert.Equal(new[] { "Cuồng nộ", "Cào", "Kỹ năng #999" },
                tracker.Complete(Result(), 1).SkillNames);
        }

        [Fact]
        public void Complete_FreezesTimeRewardsAndSkills_AndRejectsOtherBattle()
        {
            var tracker = new BattleSummaryTracker(Start(), 100);
            tracker.RecordTurn(Turn(7, 101));
            var result = Result();
            result.Messages = new[] { "Bùa x1", "  ", null, "<b>Hồng ngọc</b> x2" };
            var snapshot = tracker.Complete(result, 183);
            result.Messages[0] = "Thay đổi";
            result.Coin = 999;
            tracker.RecordTurn(Turn(7, 105));
            Assert.Same(snapshot, tracker.Complete(result, 400));
            Assert.Equal("1 phút 23 giây", snapshot.DurationText);
            Assert.Equal(new[] { "Bùa x1", "<b>Hồng ngọc</b> x2" }, snapshot.RewardLines);
            Assert.Equal(new[] { "Cào" }, snapshot.SkillNames);
            Assert.Equal(120, snapshot.Coin);
            Assert.Equal(350, snapshot.Experience);
            Assert.Equal("Sói", snapshot.OpponentName);
            Assert.Throws<ArgumentException>(() => tracker.Complete(
                new BattleResult { BattleId = 8 }, 500));
        }

        [Fact]
        public void NewBattle_HasNoRewardsOrSkillsFromPreviousBattle()
        {
            var first = new BattleSummaryTracker(Start(), 0);
            first.RecordTurn(Turn(7, 101));
            first.Complete(Result(), 100);
            var second = new BattleSummaryTracker(Start(), 200);
            var result = Result();
            result.Messages = null;
            var snapshot = second.Complete(result, 201);
            Assert.Empty(snapshot.RewardLines);
            Assert.Empty(snapshot.SkillNames);
            Assert.Equal("0 phút 01 giây", snapshot.DurationText);
        }

        private static BattleStart Start() => new BattleStart {
            BattleId = 7, Kind = BattleKind.Mob, IsParticipant = true,
            LocalPet = new BattlePet { ActorId = 7, Skills = new[] {
                new BattleSkill { Id = 101, Name = "Cào" },
                new BattleSkill { Id = 103, Name = "Cuồng nộ" },
                new BattleSkill { Id = 105, Name = "Sấm sét" }
            } },
            Opponent = new BattlePet { ActorId = -99, Name = "Sói" }
        };

        private static BattleResult Result() => new BattleResult {
            BattleId = 7, WinnerId = 7, Coin = 120, Experience = 350
        };

        private static BattleTurn Turn(int actorId, params int[] skills) => new BattleTurn {
            BattleId = 7, ActorId = actorId, Type = BattleTurn.Wait,
            Effects = Array.ConvertAll(skills, id => new BattleEffect {
                ActorId = id == 103 || actorId == -99 ? 7 : -99, SkillId = id
            })
        };
    }
}
