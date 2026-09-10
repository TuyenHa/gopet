using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Đọc JSON phẳng của <c>tools/extract-jar-strings</c>. Không phải parser JSON tổng quát — chỉ đúng hình dạng file này.</summary>
    public sealed class JarStringTableTests
    {
        [Fact]
        public void DoiTuongRong_TraVeRong()
        {
            var table = JarStringTable.Parse("{}");

            Assert.Empty(table);
        }

        [Fact]
        public void MotMuc_DocDung()
        {
            var table = JarStringTable.Parse("{\"5\": \"T.Khoản\"}");

            Assert.Equal("T.Khoản", table[5]);
        }

        [Fact]
        public void NhieuMuc_DocDungThuTuBatKy()
        {
            var table = JarStringTable.Parse("{\n  \"331\": \"a\",\n  \"5\": \"b\",\n  \"353\": \"c\"\n}");

            Assert.Equal("a", table[331]);
            Assert.Equal("b", table[5]);
            Assert.Equal("c", table[353]);
        }

        [Theory]
        [InlineData("\"a\\nb\"", "a\nb")]
        [InlineData("\"a\\tb\"", "a\tb")]
        [InlineData("\"a\\\"b\"", "a\"b")]
        [InlineData("\"a\\\\b\"", "a\\b")]
        [InlineData("\"a\\u0041b\"", "aAb")]
        public void Escape_DocDung(string jsonValue, string expected)
        {
            var table = JarStringTable.Parse($"{{\"1\": {jsonValue}}}");

            Assert.Equal(expected, table[1]);
        }

        /// <summary>Dòng bản quyền thật (mục 353) mở đầu bằng \n — ca thật, không phải ca dựng.</summary>
        [Fact]
        public void DongBanQuyen_GiuNguyenXuongDongDau()
        {
            var table = JarStringTable.Parse("{\"353\": \"\\nBản quyền 2011 ME Corp.\"}");

            Assert.StartsWith("\n", table[353]);
        }

        [Theory]
        [InlineData("{")]
        [InlineData("{\"5\" \"T.Khoản\"}")]
        [InlineData("{\"5\": \"T.Khoản\"")]
        [InlineData("{\"5\": \"a\\qb\"}")]
        public void JsonHong_ThiNem(string json)
        {
            Assert.ThrowsAny<Exception>(() => JarStringTable.Parse(json));
        }

        /// <summary>
        /// Đọc THẬT file <c>strings-vi.json</c> mà <c>tools/extract-jar-strings</c> đã sinh
        /// ra — cross-check giữa tool (Node) và parser (C#) trên cùng một dữ liệu thật,
        /// không phải fixture tự dựng.
        /// </summary>
        [Fact]
        public void FileThatDaSinh_DocDungCacChiSoDaBietGiaTri()
        {
            var path = FindRepoFile("GopetUnityClient", "Assets", "Resources", "Jar", "Strings", "strings-vi.json");
            if (!File.Exists(path))
            {
                // Chưa chạy `node tools/extract-jar-strings/index.js` — bỏ qua thay vì đỏ giả.
                return;
            }

            var table = JarStringTable.Parse(File.ReadAllText(path));

            Assert.Equal("T.Khoản", table[5]);
            Assert.Equal("Đăng nhập", table[266]);
            Assert.Equal("M.Kh:", table[348]);
            Assert.Equal("Nếu chưa có tên, xin đăng ký", table[331]);
            Assert.StartsWith("\nBản quyền 2011 ME Corp.", table[353]);
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
