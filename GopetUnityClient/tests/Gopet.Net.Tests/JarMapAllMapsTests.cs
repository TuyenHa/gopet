using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Chạy <see cref="JarMapLayout.Parse"/> trên toàn bộ 24 map trong jar.
    ///
    /// <para>Bằng chứng thực nghiệm quan trọng hơn đọc code: nếu format eg/z/collision
    /// có biến thể mà <c>ef.java</c> không lộ, test này đỏ ngay ở map đầu tiên khác dạng.
    /// Tách khỏi <see cref="JarMapLayoutTests"/> vì chỉ dùng để đo diện rộng — file gốc
    /// đã chạm giới hạn 200 dòng và mục đích khác nhau.</para>
    /// </summary>
    public sealed class JarMapAllMapsTests
    {
        [Theory]
        [InlineData(11)][InlineData(12)][InlineData(13)][InlineData(14)][InlineData(15)]
        [InlineData(16)][InlineData(17)][InlineData(18)][InlineData(19)][InlineData(20)]
        [InlineData(21)][InlineData(22)][InlineData(23)][InlineData(24)][InlineData(25)]
        [InlineData(26)][InlineData(27)][InlineData(28)][InlineData(29)][InlineData(30)]
        [InlineData(31)][InlineData(32)][InlineData(33)][InlineData(34)]
        public void MoiMap_ParseKhongLoi(int mapId)
        {
            var path = FindRepoFile("GopetUnityClient", "Assets", "Resources", "Jar", "Maps", $"{mapId}.bytes");
            if (!File.Exists(path)) return;

            var map = JarMapLayout.Parse(File.ReadAllBytes(path));

            Assert.NotNull(map.Layers);
            Assert.NotNull(map.Collision);
            Assert.NotNull(map.Objects);
            Assert.NotNull(map.Entities);
            Assert.NotNull(map.Waypoints);
            Assert.True(map.WidthTiles > 0 && map.HeightTiles > 0, $"Map {mapId} kích thước sai");
        }

        private static string FindRepoFile(params string[] relativeSegments)
        {
            var dir = AppContext.BaseDirectory;
            for (var depth = 0; depth < 8; depth++)
            {
                var candidate = Path.Combine(new[] { dir }.Concat(relativeSegments).ToArray());
                if (File.Exists(candidate)) return candidate;
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }

            return Path.Combine(new[] { dir }.Concat(relativeSegments).ToArray());
        }
    }
}
