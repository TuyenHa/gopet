using System;
using System.IO;
using System.Linq;
using Gopet.Net.Images;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Cache đĩa: ghi/đọc, băm tên file, và xoá theo LRU.</summary>
    public sealed class AssetDiskStoreTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "gopet-cache-test-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            try { Directory.Delete(_root, true); } catch (IOException) { }
        }

        private static byte[] Blob(int size, byte fill) => Enumerable.Repeat(fill, size).ToArray();

        [Fact]
        public void GhiRoiDocLai_DungNoiDung()
        {
            var store = new AssetDiskStore(_root);
            var png = Blob(64, 7);

            store.Write("npcs/Su gia bang hoi.png", png);

            Assert.True(store.TryRead("npcs/Su gia bang hoi.png", out var read));
            Assert.Equal(png, read);
        }

        [Fact]
        public void ChuaCo_TraVeFalse()
        {
            var store = new AssetDiskStore(_root);

            Assert.False(store.TryRead("chua-tung-tai.png", out var read));
            Assert.Null(read);
        }

        [Theory]
        [InlineData("npcs/Su gia bang hoi.png")]     // khoảng trắng
        [InlineData(@"npcs\su_gia_thien_than.png")]  // dấu gạch chéo ngược
        [InlineData("123")]                          // id vật phẩm, không phải đường dẫn
        [InlineData("a/very/deep/path/that/keeps/going/and/going/and/going/icon.png")]
        public void DuongDanKhoChiu_VanGhiDocDuoc(string path)
        {
            // Dùng thẳng đường dẫn làm tên file sẽ vỡ trên Windows; tên file là băm.
            var store = new AssetDiskStore(_root);
            var png = Blob(16, 3);

            store.Write(path, png);

            Assert.True(store.TryRead(path, out var read));
            Assert.Equal(png, read);
        }

        [Fact]
        public void HaiDuongDanKhacNhau_KhongDungFileNhau()
        {
            var store = new AssetDiskStore(_root);

            store.Write("a.png", Blob(8, 1));
            store.Write("b.png", Blob(8, 2));

            Assert.True(store.TryRead("a.png", out var a));
            Assert.True(store.TryRead("b.png", out var b));
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void VuotHanMuc_XoaFileCuNhatTruoc()
        {
            // Hạn mức 300 byte, ghi 4 file 100 byte -> phải xoá bớt cho về <= 300.
            var store = new AssetDiskStore(_root, maxBytes: 300);

            store.Write("cu-nhat.png", Blob(100, 1));
            Touch("cu-nhat.png", DateTime.UtcNow.AddHours(-3));

            store.Write("cu.png", Blob(100, 2));
            Touch("cu.png", DateTime.UtcNow.AddHours(-2));

            store.Write("moi.png", Blob(100, 3));
            Touch("moi.png", DateTime.UtcNow.AddHours(-1));

            store.Write("moi-nhat.png", Blob(100, 4));

            Assert.True(store.TotalBytes() <= 300);
            Assert.False(store.TryRead("cu-nhat.png", out _));
            Assert.True(store.TryRead("moi-nhat.png", out _));
        }

        [Fact]
        public void DocMotFile_LamNoTreLaiTranhBiXoa()
        {
            // TryRead "chạm" vào file. Không có bước đó thì LRU thành FIFO và
            // ảnh đang dùng liên tục vẫn bị xoá.
            var store = new AssetDiskStore(_root, maxBytes: 250);

            store.Write("hay-dung.png", Blob(100, 1));
            Touch("hay-dung.png", DateTime.UtcNow.AddHours(-5));

            store.Write("it-dung.png", Blob(100, 2));
            Touch("it-dung.png", DateTime.UtcNow.AddHours(-4));

            store.TryRead("hay-dung.png", out _);   // chạm -> trẻ lại

            store.Write("moi.png", Blob(100, 3));

            Assert.True(store.TryRead("hay-dung.png", out _));
            Assert.False(store.TryRead("it-dung.png", out _));
        }

        [Fact]
        public void GhiRong_BoQua()
        {
            // Ghi một tấm thật trước, để test đỏ được nếu Write bắt đầu tạo file
            // rỗng — bản cũ chỉ so 0 với 0 trên thư mục trống.
            var store = new AssetDiskStore(_root);
            store.Write("that.png", Blob(32, 5));

            store.Write("rong.png", Array.Empty<byte>());
            store.Write("null.png", null);

            Assert.Equal(32, store.TotalBytes());
            Assert.False(store.TryRead("rong.png", out _));
        }

        private void Touch(string assetPath, DateTime whenUtc)
        {
            File.SetLastWriteTimeUtc(Path.Combine(_root, AssetDiskStore.KeyOf(assetPath) + ".png"), whenUtc);
        }
    }
}
