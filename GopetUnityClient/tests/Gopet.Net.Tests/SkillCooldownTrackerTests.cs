using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Kiểm chứng SkillCooldownTracker — nguồn cooldown client-side vì server không
    /// gửi trạng thái cooldown qua packet. Xem plan 260917 phase-05.</summary>
    public sealed class SkillCooldownTrackerTests
    {
        private const int LocalActor = 10;
        private const int OpponentActor = 20;

        [Fact]
        public void MarkUsed_KhoaDungBaLuotXenKe()
        {
            // Server gửi BattleTurn xen kẽ giữa 2 bên — mỗi khi actorId trở lại local
            // (sau 1 lượt của opponent) là mình mất 1 lượt cooldown.
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            Assert.False(t.IsReady(101));
            Assert.Equal(3, t.TurnsLeft(101));

            t.OnTurnAdvanced(LocalActor, LocalActor); Assert.Equal(2, t.TurnsLeft(101));
            t.OnTurnAdvanced(OpponentActor, LocalActor); // opponent lượt, không giảm
            t.OnTurnAdvanced(LocalActor, LocalActor); Assert.Equal(1, t.TurnsLeft(101));
            t.OnTurnAdvanced(OpponentActor, LocalActor);
            t.OnTurnAdvanced(LocalActor, LocalActor); Assert.True(t.IsReady(101));
        }

        [Fact]
        public void OnTurnAdvanced_ChiTruKhiActorLaLocal()
        {
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            t.OnTurnAdvanced(OpponentActor, LocalActor);
            Assert.Equal(3, t.TurnsLeft(101));
        }

        [Fact]
        public void OnTurnAdvanced_TrungActorLienTiepChiTruMotLan()
        {
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            t.OnTurnAdvanced(LocalActor, LocalActor);
            t.OnTurnAdvanced(LocalActor, LocalActor); // trùng actorId → skip
            Assert.Equal(2, t.TurnsLeft(101));
        }

        [Fact]
        public void ResyncDenied_DuaVeDungBaLuot()
        {
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            t.OnTurnAdvanced(LocalActor, LocalActor); Assert.Equal(2, t.TurnsLeft(101));
            t.ResyncDenied(101);
            Assert.Equal(3, t.TurnsLeft(101));
        }

        [Fact]
        public void NhieuSkillDocLap()
        {
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            t.OnTurnAdvanced(LocalActor, LocalActor);
            t.MarkUsed(202);
            Assert.Equal(2, t.TurnsLeft(101));
            Assert.Equal(3, t.TurnsLeft(202));
        }

        [Fact]
        public void IsReady_MacDinhChoSkillChuaBamKhongKhoa()
        {
            var t = new SkillCooldownTracker();
            Assert.True(t.IsReady(999));
            Assert.Equal(0, t.TurnsLeft(999));
        }
    }
}
