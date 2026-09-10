using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class ActionThrottleTests
    {
        [Fact]
        public void SameActionIsBlockedUntilCooldownEnds()
        {
            long now = 1000;
            var throttle = new ActionThrottle(() => now);

            Assert.True(throttle.TryAcquire("pk", 1500, out var firstRemaining));
            Assert.Equal(0, firstRemaining);
            Assert.False(throttle.TryAcquire("pk", 1500, out var blockedRemaining));
            Assert.Equal(1500, blockedRemaining);

            now = 2500;
            Assert.True(throttle.TryAcquire("pk", 1500, out _));
        }

        [Fact]
        public void DifferentActionsHaveIndependentWindows()
        {
            var throttle = new ActionThrottle(() => 1000);

            Assert.True(throttle.TryAcquire("pk", 1500, out _));
            Assert.True(throttle.TryAcquire("challenge", 1500, out _));
        }
    }
}
