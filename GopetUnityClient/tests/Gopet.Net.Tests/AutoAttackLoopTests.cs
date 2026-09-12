using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class AutoAttackLoopTests
    {
        [Fact]
        public void BatLen_GuiNgayVaLapLaiSauBonGiay()
        {
            long now = 1000;
            var loop = new AutoAttackLoop(() => now);

            loop.SetEnabled(true);

            Assert.True(loop.ShouldRequest());
            Assert.False(loop.ShouldRequest());
            now = 4999;
            Assert.False(loop.ShouldRequest());
            now = 5000;
            Assert.True(loop.ShouldRequest());
        }

        [Fact]
        public void Tat_DungGuiVaBatLaiGuiNgay()
        {
            long now = 10;
            var loop = new AutoAttackLoop(() => now);
            loop.SetEnabled(true);
            Assert.True(loop.ShouldRequest());

            loop.SetEnabled(false);
            now = 9000;
            Assert.False(loop.ShouldRequest());

            loop.SetEnabled(true);
            Assert.True(loop.ShouldRequest());
        }
    }
}
