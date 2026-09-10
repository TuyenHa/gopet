using Gopet.Net.Map;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class WorldStatusHandlerTests
    {
        private readonly MessageRouter _router = new MessageRouter();
        private readonly WorldStatusHandler _handler = new WorldStatusHandler();

        public WorldStatusHandlerTests()
        {
            _handler.RegisterOn(_router);
        }

        [Fact]
        public void BossHp_ReadsMobAndCurrentHp()
        {
            BossHpUpdate received = null;
            _handler.BossHpUpdated += value => received = value;
            Dispatch(GopetCmd.UPDATE_HP_BOSS, m => m.PutInt(73).PutInt(12050));

            Assert.NotNull(received);
            Assert.Equal(73, received.MobId);
            Assert.Equal(12050, received.Hp);
        }

        [Fact]
        public void PlaceTimeAndBigText_AreParsed()
        {
            var seconds = -1;
            string text = null;
            _handler.PlaceTimeUpdated += value => seconds = value;
            _handler.BigTextShown += value => text = value;

            Dispatch(GopetCmd.TIME_PLACE, m => m.PutInt(125));
            Dispatch(GopetCmd.SHOW_BIG_TEXT_EFF, m => m.PutUtf("Combo x3"));

            Assert.Equal(125, seconds);
            Assert.Equal("Combo x3", text);
        }

        [Fact]
        public void ExpBuff_ReadsIconExpiryAndLegacyFields()
        {
            ExpBuffStatus received = null;
            _handler.ExpBuffUpdated += value => received = value;
            Dispatch(GopetCmd.SHOW_EXP, m => m.PutUtf("buffs/exp.png")
                .PutInt(1900000000).PutInt(7).PutLong(99));

            Assert.Equal("buffs/exp.png", received.IconPath);
            Assert.Equal(1900000000, received.ExpiresAtUnixSeconds);
            Assert.Equal(7, received.ReservedInt);
            Assert.Equal(99, received.ReservedLong);
        }

        private void Dispatch(sbyte sub, System.Func<Message, Message> body)
        {
            using var message = body(Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub));
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
