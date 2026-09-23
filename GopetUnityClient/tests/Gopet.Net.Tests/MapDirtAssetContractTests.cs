using System;
using System.IO;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class MapDirtAssetContractTests
    {
        [Theory]
        [InlineData(151, 12151)][InlineData(161, 12161)][InlineData(162, 12162)]
        [InlineData(179, 12179)][InlineData(180, 12180)]
        public void AnhDat_CoDuOTheoAnhGoc(int source, int target)
        {
            var original = PngSize(source);
            var dirt = PngSize(target);
            Assert.Equal(original, dirt);
            Assert.Equal(24, dirt.height);
            Assert.Equal(0, dirt.width % 24);
        }

        [Fact]
        public void MoiOTileTrong24Map_CoAnhSauMapping()
        {
            for (var mapId = 11; mapId <= 34; mapId++)
            {
                var map = JarMapLayout.Parse(File.ReadAllBytes(Resource($"Maps/{mapId}.bytes")));
                foreach (var layer in map.Layers)
                foreach (var row in layer)
                foreach (var packed in row)
                {
                    var strip = JarMapLayout.StripOf(packed);
                    if (strip < 0 || strip >= map.ImageCount) continue;
                    var originalId = map.ResourceIds[strip];
                    var imageId = MapSkinOverrides.ResolveImageId(mapId, originalId);
                    var size = PngSize(imageId);
                    var requiredWidth = (JarMapLayout.CellOf(packed) + 1) * 24;
                    Assert.True(size.width >= requiredWidth && size.height >= 24,
                        $"Map {mapId}: {originalId}->{imageId}, cell {JarMapLayout.CellOf(packed)}");
                    if (mapId >= 23) Assert.Equal(originalId, imageId);
                }
            }
        }

        [Fact]
        public void AiThuongGioi_KhongNhanDienTuyetTuStripKhongDung()
        {
            var map = JarMapLayout.Parse(File.ReadAllBytes(Resource("Maps/31.bytes")));
            Assert.Contains(151, map.ResourceIds);
            foreach (var layer in map.Layers)
            foreach (var row in layer)
            foreach (var packed in row)
            {
                var strip = JarMapLayout.StripOf(packed);
                if (strip < 0 || strip >= map.ImageCount) continue;
                Assert.NotEqual(151, map.ResourceIds[strip]);
            }
        }

        private static (int width, int height) PngSize(int id)
        {
            var data = File.ReadAllBytes(Resource($"Art/Raw/newMapData/{id}.png"));
            Assert.True(data.Length >= 24, $"Invalid PNG {id}");
            Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 },
                new ArraySegment<byte>(data, 0, 8).ToArray());
            return (BigEndianInt(data, 16), BigEndianInt(data, 20));
        }

        private static int BigEndianInt(byte[] data, int offset) =>
            (data[offset] << 24) | (data[offset + 1] << 16) |
            (data[offset + 2] << 8) | data[offset + 3];

        private static string Resource(string relative)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var path = Path.Combine(dir.FullName, "GopetUnityClient/Assets/Resources/Jar", relative);
                if (File.Exists(path)) return path;
            }
            throw new FileNotFoundException($"Required map test resource not found: {relative}");
        }
    }
}
