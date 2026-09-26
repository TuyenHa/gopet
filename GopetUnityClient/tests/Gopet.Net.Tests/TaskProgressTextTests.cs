using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class TaskProgressTextTests
    {
        [Theory]
        [InlineData("Tiêu diệt khủng long 10/10 tại Đấu trường", true)]
        [InlineData("Tiêu diệt khủng long 12 / 10", true)]
        [InlineData("Tiêu diệt tề thiên 6/10 tại Đấu trường", false)]
        [InlineData("Học 0/0 kỹ năng", false)]
        [InlineData("Không có tiến độ", false)]
        public void IsLineDone_TheoSoDaLamVaCan(string line, bool done) =>
            Assert.Equal(done, TaskProgressText.IsLineDone(line));

        [Fact]
        public void Colorize_ChiToHangDaDat()
        {
            var result = TaskProgressText.Colorize("A 10/10\nB 6/10");
            Assert.Equal("<color=#3FCB57>A 10/10</color>\nB 6/10", result);
        }

        [Fact]
        public void Colorize_ThayNgoacNhonCuaDuLieuServer()
        {
            var result = TaskProgressText.Colorize("<b>X</b> 1/10");
            Assert.DoesNotContain("<b>", result);
            Assert.Contains("‹b›X", result);
        }
    }
}
