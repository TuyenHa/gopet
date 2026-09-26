using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class TaskGuidePacketTests
    {
        [Fact]
        public void RequestNextTaskGuide_ThemByteTab1SauShowListTask()
        {
            // 51 (PET_SERVICE=81) + 36 (SHOW_LIST_TASK=54) + 01 (tab "Nhiệm vụ tiếp theo").
            // Jar gửi gói không có byte tab ⇒ server vẫn trả menu 1034 như cũ.
            using var m = GuiderPackets.RequestNextTaskGuide();
            Assert.Equal("513601", TestVectorData.ToHex(m.ToWire()));
        }
    }
}
