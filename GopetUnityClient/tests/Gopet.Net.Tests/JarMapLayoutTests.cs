using System;
using System.IO;
using System.Linq;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Bố cục map của jar. Chạy trên FILE THẬT (<c>maps/11.dat</c> — nền màn đăng nhập)
    /// chứ không phải fixture tự dựng: format này chỉ có một nguồn sự thật là
    /// <c>ef.java</c>, tự bịa dữ liệu thì test xanh mà đọc sai map thật vẫn không ai biết.
    /// </summary>
    public sealed class JarMapLayoutTests
    {
        /// <summary>Map nền màn đăng nhập — <c>fb.java</c> dựng <c>new ef(11, …)</c>.</summary>
        private const int LoginMapId = 11;

        [Fact]
        public void Map11_DocDungKichThuocVaSoTaiNguyen()
        {
            var map = LoadRealMap();
            if (map == null) return;

            Assert.Equal(24, map.WidthTiles);
            Assert.Equal(20, map.HeightTiles);
            Assert.Equal(576, map.WidthPixels);
            Assert.Equal(480, map.HeightPixels);

            // 3 ảnh dải nền + 19 tài nguyên vật thể.
            Assert.Equal(3, map.ImageCount);
            Assert.Equal(22, map.ResourceIds.Length);
            Assert.Equal(new[] { 161, 162, 3 }, map.ResourceIds.Take(3));
        }

        [Fact]
        public void Map11_MoiAnhDaiDeuLaBoiSoCuaMotO()
        {
            var map = LoadRealMap();
            if (map == null) return;

            // 3 tài nguyên đầu phải mang kiểu "ảnh dải" — nếu lệch thì chỉ số ô nền
            // (4 bit cao) đang trỏ nhầm sang tài nguyên vật thể.
            foreach (var type in map.ResourceTypes.Take(map.ImageCount))
            {
                Assert.Equal(JarMapLayout.TypeTileStrip, type);
            }
        }

        [Fact]
        public void Map11_MoiOTroDungVaoMotAnhDaiCoThat()
        {
            var map = LoadRealMap();
            if (map == null) return;

            foreach (var layer in map.Layers)
            {
                foreach (var row in layer)
                {
                    foreach (var tile in row)
                    {
                        var strip = JarMapLayout.StripOf(tile);
                        if (strip < 0) continue; // ô trống, hợp lệ

                        Assert.InRange(strip, 0, map.ImageCount - 1);
                        Assert.InRange(JarMapLayout.CellOf(tile), 0, 15);
                    }
                }
            }
        }

        [Fact]
        public void Map11_VatTheNamTrongMapVaTroDungTaiNguyen()
        {
            var map = LoadRealMap();
            if (map == null) return;

            Assert.Equal(84, map.Objects.Length);

            // Toạ độ là CHÂN vật thể (neo giữa-dưới), nên được phép nhô quá mép một ô:
            // map 11 thật có vật thể ở y = 481 trong khi map cao 480. Ràng buộc đúng
            // bằng mép sẽ đỏ trên chính dữ liệu gốc.
            foreach (var item in map.Objects)
            {
                Assert.InRange(item.ResourceIndex, 0, map.ResourceIds.Length - 1);
                Assert.InRange(item.X, 0, map.WidthPixels + JarMapLayout.TileSize);
                Assert.InRange(item.Y, 0, map.HeightPixels + JarMapLayout.TileSize);
            }
        }

        /// <summary>
        /// Bẫy đã trả giá: <c>yOffset</c> là byte CÓ DẤU. Đọc không dấu thì vật thể
        /// mang giá trị âm tụt xuống hàng trăm pixel, ra ngoài map — mà mọi test đếm
        /// số lượng vẫn xanh.
        /// </summary>
        [Fact]
        public void Map11_CoVatTheYOffsetAm()
        {
            var map = LoadRealMap();
            if (map == null) return;

            Assert.Contains(map.Objects, o => o.YOffset < 0);
        }

        [Fact]
        public void DuLieuCutNgang_NemLoiRoRang()
        {
            var full = ReadRealMapBytes();
            if (full == null) return;

            var truncated = full.Take(full.Length / 2).ToArray();

            Assert.ThrowsAny<Exception>(() => JarMapLayout.Parse(truncated));
        }

        /// <summary>Lớp va chạm: P5.1 đọc rồi vứt; P6 cần để chặn nhân vật nên phải phơi ra.</summary>
        [Fact]
        public void Map11_LopVaChamCoKichThuocDungBangMap()
        {
            var map = LoadRealMap();
            if (map == null) return;

            Assert.Equal(map.HeightTiles, map.Collision.Length);
            foreach (var row in map.Collision) Assert.Equal(map.WidthTiles, row.Length);
        }

        /// <summary>Map 11: số nhà/NPC + waypoint kiểm chứng bằng scan độc lập (scratchpad/scan-maps).</summary>
        [Fact]
        public void Map11_DocDungEntityVaWaypoint()
        {
            var map = LoadRealMap();
            if (map == null) return;

            Assert.Equal(11, map.Entities.Length);
            Assert.Equal(3, map.Waypoints.Length);
            Assert.Contains(map.Entities, e => e.Kind == 0);
            Assert.Contains(map.Entities, e => e.Kind != 0 && !string.IsNullOrEmpty(e.Name));

            foreach (var e in map.Entities)
            {
                Assert.InRange(e.X, 0, map.WidthPixels + JarMapLayout.TileSize);
                Assert.InRange(e.Y, 0, map.HeightPixels + JarMapLayout.TileSize);
                Assert.Equal(5, e.Raw5.Length);
            }
            foreach (var w in map.Waypoints)
            {
                Assert.InRange(w.X, 0, map.WidthPixels + JarMapLayout.TileSize);
                Assert.InRange(w.Y, 0, map.HeightPixels + JarMapLayout.TileSize);
            }
        }

        [Fact]
        public void ONen_GiaiNenDungHaiNuaByte()
        {
            // 0x25 = ảnh dải 2, ô thứ 5-1 = 4.
            Assert.Equal(2, JarMapLayout.StripOf(0x25));
            Assert.Equal(4, JarMapLayout.CellOf(0x25));

            // 4 bit thấp bằng 0 nghĩa là ô trống, bất kể 4 bit cao là gì.
            Assert.Equal(-1, JarMapLayout.StripOf(0x30));
            Assert.Equal(-1, JarMapLayout.CellOf(0x30));
        }

        private static JarMapLayout LoadRealMap()
        {
            var bytes = ReadRealMapBytes();
            return bytes == null ? null : JarMapLayout.Parse(bytes);
        }

        private static byte[] ReadRealMapBytes()
        {
            var path = FindRepoFile("GopetUnityClient", "Assets", "Resources", "Jar", "Maps", $"{LoginMapId}.bytes");

            // Chưa chạy `node tools/unpack-jar-dat/index.js` — bỏ qua thay vì đỏ giả.
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
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
