using System;
using System.IO;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Lưu tài khoản giữa hai lần mở app, và <see cref="SecretBox"/> bọc mật khẩu.
    /// </summary>
    public sealed class CredentialStoreTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "gopet-cred-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Fact]
        public void LuuRoiDoc_RaDungTaiKhoan()
        {
            new CredentialStore(_root).Save("gopettest", "abc12345");

            var loaded = new CredentialStore(_root).Load();

            Assert.Equal("gopettest", loaded.Username);
            Assert.Equal("abc12345", loaded.Password);
        }

        /// <summary>
        /// Mật khẩu KHÔNG được nằm nguyên văn trên đĩa. Đây là ca duy nhất phân biệt
        /// được "đã bọc" với "ghi thẳng" — mọi test round-trip đều xanh với cả hai.
        /// </summary>
        [Fact]
        public void MatKhau_KhongNamNguyenVanTrenDia()
        {
            new CredentialStore(_root).Save("gopettest", "abc12345");

            foreach (var file in Directory.GetFiles(_root))
            {
                var bytes = File.ReadAllBytes(file);
                Assert.DoesNotContain("abc12345", System.Text.Encoding.UTF8.GetString(bytes));
            }
        }

        [Fact]
        public void KhongCoMatKhau_VanNhoTenTaiKhoan()
        {
            new CredentialStore(_root).Save("gopettest", null);

            var loaded = new CredentialStore(_root).Load();

            Assert.Equal("gopettest", loaded.Username);
            Assert.False(loaded.HasPassword);
        }

        [Fact]
        public void ChuaLuoGi_TraVeBanGhiRong()
        {
            var loaded = new CredentialStore(_root).Load();

            Assert.Equal(string.Empty, loaded.Username);
            Assert.False(loaded.HasPassword);
        }

        [Fact]
        public void Clear_XoaCaKhoaLanBanGhi()
        {
            var store = new CredentialStore(_root);
            store.Save("gopettest", "abc12345");

            store.Clear();

            Assert.Empty(Directory.GetFiles(_root));
            Assert.Equal(string.Empty, store.Load().Username);
        }

        /// <summary>Mất khoá thì tên vẫn đọc được, mật khẩu thì không — và không được ném.</summary>
        [Fact]
        public void MatKhoa_ConTenNhungKhongConMatKhau()
        {
            new CredentialStore(_root).Save("gopettest", "abc12345");
            File.Delete(Path.Combine(_root, "device.key"));

            var loaded = new CredentialStore(_root).Load();

            Assert.Equal("gopettest", loaded.Username);
            Assert.False(loaded.HasPassword);
        }

        [Fact]
        public void FileRac_KhongNem()
        {
            Directory.CreateDirectory(_root);
            File.WriteAllText(Path.Combine(_root, "credentials.txt"), "gopettest\nkhong-phai-base64!!");

            var loaded = new CredentialStore(_root).Load();

            Assert.Equal("gopettest", loaded.Username);
            Assert.False(loaded.HasPassword);
        }
    }
}
