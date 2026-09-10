using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class JarActorAnimationTests
    {
        [Theory]
        [InlineData(125)][InlineData(126)][InlineData(127)]
        [InlineData(128)][InlineData(129)][InlineData(130)]
        public void AnuGoc_ParseHetVaThamChieuHopLe(int skillId)
        {
            var path = FindFile("GopetUnityClient", "Assets", "Resources", "Jar",
                "BattleAnimations", "skills", $"{skillId}.anu.bytes");
            var animation = JarActorAnimation.Parse(File.ReadAllBytes(path));
            Assert.NotEmpty(animation.Clips);
            Assert.NotEmpty(animation.Steps);
            Assert.NotEmpty(animation.Frames);
            Assert.NotEmpty(animation.Regions);
        }

        private static string FindFile(params string[] segments)
        {
            var dir = AppContext.BaseDirectory;
            for (var depth = 0; depth < 8; depth++)
            {
                var candidate = Path.Combine(new[] { dir }.Concat(segments).ToArray());
                if (File.Exists(candidate)) return candidate;
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }
            throw new FileNotFoundException(string.Join("/", segments));
        }
    }
}
