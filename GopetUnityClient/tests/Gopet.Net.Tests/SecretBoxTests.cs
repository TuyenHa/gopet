using System;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Bọc chuỗi bằng AES + HMAC. Ba thứ phải đúng: mở lại được, khoá khác thì
    /// không, và sửa một byte thì bị phát hiện.
    /// </summary>
    public sealed class SecretBoxTests
    {
        [Theory]
        [InlineData("abc12345")]
        [InlineData("")]
        [InlineData("mật khẩu có dấu tiếng Việt")]
        public void BocRoiMo_RaDungChuoiCu(string plaintext)
        {
            var key = SecretBox.NewKey();

            Assert.True(SecretBox.TryOpen(SecretBox.Seal(plaintext, key), key, out var opened));
            Assert.Equal(plaintext, opened);
        }

        [Fact]
        public void KhoaKhac_KhongMoDuoc()
        {
            var boxed = SecretBox.Seal("abc12345", SecretBox.NewKey());

            Assert.False(SecretBox.TryOpen(boxed, SecretBox.NewKey(), out _));
        }

        /// <summary>
        /// Sửa một byte của CHỮ KÝ. Đây là ca duy nhất chứng minh chữ ký được kiểm:
        /// bỏ HMAC đi thì byte đó chẳng ai đọc, gói vẫn mở ra đúng nguyên văn.
        ///
        /// <para>Sửa byte trong phần mã hoá thì KHÔNG chứng minh gì: đệm CBC tự nó
        /// bắt được phần lớn trường hợp, nên test kiểu đó xanh cả khi không có chữ ký.</para>
        /// </summary>
        [Fact]
        public void SuaChuKy_BiPhatHien()
        {
            var key = SecretBox.NewKey();
            var bytes = Convert.FromBase64String(SecretBox.Seal("abc12345", key));

            bytes[bytes.Length - 1] ^= 0xFF;

            Assert.False(SecretBox.TryOpen(Convert.ToBase64String(bytes), key, out _));
        }

        /// <summary>
        /// Sửa một byte của khối mã hoá ĐẦU trên chuỗi dài: đệm nằm ở khối cuối nên
        /// vẫn hợp lệ, giải mã ra rác mà không có gì báo — trừ chữ ký.
        /// </summary>
        [Fact]
        public void SuaKhoiDau_TrenChuoiDai_BiPhatHien()
        {
            var key = SecretBox.NewKey();
            var bytes = Convert.FromBase64String(SecretBox.Seal(new string('a', 64), key));

            bytes[16] ^= 0xFF;

            Assert.False(SecretBox.TryOpen(Convert.ToBase64String(bytes), key, out _));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("khong-phai-base64!!")]
        [InlineData("AAAA")]
        public void DauVaoHong_TraFalseChuKhongNem(string boxed)
        {
            Assert.False(SecretBox.TryOpen(boxed, SecretBox.NewKey(), out _));
        }

        /// <summary>Hai lần bọc cùng một chuỗi phải ra khác nhau — IV ngẫu nhiên mỗi lần.</summary>
        [Fact]
        public void MoiLanBoc_RaKhacNhau()
        {
            var key = SecretBox.NewKey();

            Assert.NotEqual(SecretBox.Seal("abc12345", key), SecretBox.Seal("abc12345", key));
        }
    }
}
