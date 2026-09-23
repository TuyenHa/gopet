using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Chạy parser trên đúng ba file jar đã chép vào <c>Resources</c>. Mốc đối chiếu là kích
    /// thước ảnh <c>.png</c> đi kèm: các ô cắt phải nằm gọn trong ảnh và phủ đúng phần ảnh —
    /// đọc lệch một byte là số ra lung tung ngay, không cần mở game mới biết.
    /// </summary>
    public sealed class JarEffectAnimationTests
    {
        [Theory]
        [InlineData("kiss", 46, 72)]
        [InlineData("play", 28, 15)]
        [InlineData("poke", 26, 40)]
        public void DocHetFile_VaMoiOCatNamGonTrongAnh(string name, int sheetWidth, int sheetHeight)
        {
            var anim = Parse(name);

            Assert.NotEmpty(anim.Regions);
            Assert.NotEmpty(anim.Frames);
            Assert.NotEmpty(anim.Steps);

            foreach (var region in anim.Regions)
            {
                Assert.InRange(region.X, 0, sheetWidth - 1);
                Assert.InRange(region.Y, 0, sheetHeight - 1);
                Assert.InRange(region.X + region.Width, 1, sheetWidth);
                Assert.InRange(region.Y + region.Height, 1, sheetHeight);
            }

            foreach (var frame in anim.Frames)
                foreach (var piece in frame)
                    Assert.InRange(piece.RegionIndex, 0, anim.Regions.Length - 1);

            foreach (var step in anim.Steps)
            {
                Assert.InRange(step.FrameIndex, 0, anim.Frames.Length - 1);
                Assert.True(step.DurationMs > 0, $"Bước có thời lượng {step.DurationMs}ms.");
            }
        }

        /// <summary>Hôn và xoa đầu chỉ lật qua lại hai khung; jar để mặc định 150ms mỗi khung.</summary>
        [Theory]
        [InlineData("kiss")]
        [InlineData("poke")]
        public void HonVaXoaDau_HaiKhungLuanPhien_150msMoiKhung(string name)
        {
            var anim = Parse(name);
            Assert.Equal(2, anim.Regions.Length);
            Assert.Equal(2, anim.Frames.Length);
            Assert.Equal(new[] { 0, 1 }, anim.Steps.Select(s => s.FrameIndex));
            Assert.All(anim.Steps, s => Assert.Equal(150, s.DurationMs));
        }

        /// <summary>Chơi với pet là vòng 8 bước đi-rồi-về trên 12 khung, tổng 1,2 giây mỗi vòng.</summary>
        [Fact]
        public void ChoiVoiPet_VongTamBuocDiRoiVe()
        {
            var anim = Parse("play");
            Assert.Equal(12, anim.Frames.Length);
            Assert.Equal(new[] { 0, 3, 4, 6, 8, 6, 4, 3 }, anim.Steps.Select(s => s.FrameIndex));
            Assert.Equal(1200, anim.Steps.Sum(s => s.DurationMs));
        }

        /// <summary>Cả ba file đều một mảnh mỗi khung — người vẽ chỉ cần một renderer.</summary>
        [Theory]
        [InlineData("kiss")]
        [InlineData("play")]
        [InlineData("poke")]
        public void MoiKhungChiMotManh(string name) => Assert.Equal(1, Parse(name).MaxPiecesPerFrame);

        private static JarEffectAnimation Parse(string name)
        {
            var path = FindFile("GopetUnityClient", "Assets", "Resources", "Jar", "Art", "Raw",
                "pet", "petInteract", $"{name}.bytes");
            return JarEffectAnimation.Parse(File.ReadAllBytes(path));
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
