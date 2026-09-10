using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class JarMapAnimationTests
    {
        [Theory]
        [InlineData(2)][InlineData(3)][InlineData(72)][InlineData(107)][InlineData(108)]
        [InlineData(190)][InlineData(194)][InlineData(217)][InlineData(218)]
        public void MetadataGoc_ParseHetVaThamChieuHopLe(int resourceId)
        {
            var path = FindRepoFile("GopetUnityClient", "Assets", "Resources", "Jar",
                "MapAnimations", $"{resourceId}.bytes");
            var animation = JarMapAnimation.Parse(File.ReadAllBytes(path));

            Assert.NotEmpty(animation.Regions);
            Assert.NotEmpty(animation.Frames);
            Assert.NotEmpty(animation.Clips);
            Assert.All(animation.Regions, r =>
            {
                Assert.True(r.Width > 0);
                Assert.True(r.Height > 0);
            });
            Assert.All(animation.Frames.SelectMany(f => f.Parts), p =>
                Assert.InRange(p.RegionIndex, 0, animation.Regions.Length - 1));
            Assert.All(animation.Clips.SelectMany(c => c.FrameIndices), frame =>
                Assert.InRange(frame, 0, animation.Frames.Length - 1));
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
            throw new FileNotFoundException(string.Join("/", relativeSegments));
        }
    }
}
