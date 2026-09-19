using Gopet.Net.Battle;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Gói <c>PET_BATTLE_EXP</c> (81/33) — EXP nhỏ giọt mỗi đòn trúng PvE, và việc
    /// huỷ cooldown lạc quan khi kỹ năng trượt. Xem plan 260919-1102-exp-moi-don-va-danh-truot.</summary>
    public sealed class BattleExpPacketTests
    {
        [Fact]
        public void HitExp_ParseDungBaTruong()
        {
            var (handler, router) = NewHandler(77);
            BattleExpGain gain = null;
            handler.HitExpReceived += g => gain = g;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_EXP)
                .PutInt(77).PutInt(77).PutInt(12);

            Dispatch(router, packet);

            Assert.NotNull(gain);
            Assert.Equal(77, gain.BattleId);
            Assert.Equal(77, gain.ActorId);
            Assert.Equal(12, gain.Amount);
        }

        [Fact]
        public void HitExp_ThuaByteThiNem()
        {
            var (handler, router) = NewHandler(1);
            using var packet = Message.Create(GopetCmd.PET_BATTLE_EXP)
                .PutInt(1).PutInt(1).PutInt(5).PutInt(999); // thừa 1 int
            Assert.Throws<ProtocolException>(() => Dispatch(router, packet));
        }

        [Fact]
        public void KyNangTruot_HuyCooldownLacQuan()
        {
            // Client MarkUsed ngay khi bấm; server trượt thì KHÔNG đặt cooldown nên phải gỡ,
            // nếu không nút xám oan 3 lượt.
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            Assert.False(t.IsReady(101));

            t.CancelLastUsed();
            Assert.True(t.IsReady(101));
        }

        [Fact]
        public void HuyCooldown_KhongDungToiKyNangKhac()
        {
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            t.MarkUsed(102);
            t.CancelLastUsed(); // chỉ gỡ 102

            Assert.False(t.IsReady(101));
            Assert.True(t.IsReady(102));
        }

        [Fact]
        public void HuyCooldown_GoiHaiLanKhongGoNhamThemCaiNua()
        {
            var t = new SkillCooldownTracker();
            t.MarkUsed(101);
            t.MarkUsed(102);
            t.CancelLastUsed();
            t.CancelLastUsed(); // lần hai là no-op

            Assert.False(t.IsReady(101));
        }

        private static (BattleHandler, MessageRouter) NewHandler(int userId)
        {
            var router = new MessageRouter();
            var handler = new BattleHandler(null, userId);
            handler.RegisterOn(router);
            return (handler, router);
        }

        private static void Dispatch(MessageRouter router, Message packet)
        {
            var body = packet.ToWire();
            var wire = new byte[body.Length + 1];
            wire[0] = unchecked((byte)GopetCmd.PET_SERVICE);
            System.Array.Copy(body, 0, wire, 1, body.Length);
            router.Dispatch(Message.FromWire(wire, false));
        }
    }
}
