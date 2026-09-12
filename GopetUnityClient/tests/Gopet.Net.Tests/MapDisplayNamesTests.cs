using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class MapDisplayNamesTests
    {
        [Theory]
        [InlineData(11, "Thành Phố Linh Thú")]
        [InlineData(13, "Linh Lâm")]
        [InlineData(34, "Vùng chiến sự")]
        [InlineData(99, "Map 99")]
        public void TraTenTheoMapId(int mapId, string expected) =>
            Assert.Equal(expected, MapDisplayNames.Get(mapId));
    }
}
