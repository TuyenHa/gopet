using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class CompactNumberFormatTests
    {
        [Theory]
        [InlineData(0, "0")]
        [InlineData(999, "999")]
        [InlineData(1000, "1.0k")]
        [InlineData(1500, "1.5k")]
        [InlineData(9999, "9.9k")]
        [InlineData(10_000, "10k")]
        [InlineData(99_400, "99.4k")]
        [InlineData(100_000, "100k")]
        [InlineData(999_999, "1000k")]
        [InlineData(1_000_000, "1M")]
        [InlineData(1_200_000, "1.2M")]
        [InlineData(50_200_000, "50.2M")]
        public void Format_PositiveValues(long input, string expected) =>
            Assert.Equal(expected, CompactNumberFormat.Format(input));

        [Fact]
        public void Format_NegativeValue() =>
            Assert.Equal("-1.5k", CompactNumberFormat.Format(-1500));
    }
}
