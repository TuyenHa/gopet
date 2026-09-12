using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class ChatTranscriptTests
    {
        [Fact]
        public void VuotGioiHan_ChiGiuTinMoiNhat()
        {
            var history = new ChatTranscript(2);
            history.Add("A", "one");
            history.Add("B", "two");
            history.Add("C", "three");

            Assert.Equal(2, history.Entries.Count);
            Assert.Equal("B", history.Entries[0].Sender);
            Assert.Equal("three", history.Entries[1].Text);
        }
    }
}
