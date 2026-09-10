using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class BattleAnimationTests
    {
        [Fact]
        public void TatCaMetadataDyBattle_ParseHetVaThamChieuHopLe()
        {
            var root = FindDirectory("GopetUnityClient", "Assets", "Resources", "Jar", "BattleAnimations");
            var files = Directory.GetFiles(root, "*.bytes", SearchOption.AllDirectories)
                .Where(path => !path.EndsWith(".anu.bytes", StringComparison.OrdinalIgnoreCase)).ToArray();
            Assert.Equal(27, files.Length);
            foreach (var file in files)
            {
                var animation = JarMapAnimation.Parse(File.ReadAllBytes(file));
                Assert.NotEmpty(animation.Regions);
                Assert.NotEmpty(animation.Frames);
                Assert.NotEmpty(animation.Clips);
                Assert.All(animation.Frames.SelectMany(frame => frame.Parts), part =>
                    Assert.InRange(part.RegionIndex, 0, animation.Regions.Length - 1));
                Assert.All(animation.Clips.SelectMany(clip => clip.FrameIndices), frame =>
                    Assert.InRange(frame, 0, animation.Frames.Length - 1));
            }
        }

        private static string FindDirectory(params string[] segments)
        {
            var dir = AppContext.BaseDirectory;
            for (var depth = 0; depth < 8; depth++)
            {
                var candidate = Path.Combine(new[] { dir }.Concat(segments).ToArray());
                if (Directory.Exists(candidate)) return candidate;
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }
            throw new DirectoryNotFoundException(string.Join("/", segments));
        }
    }
}
