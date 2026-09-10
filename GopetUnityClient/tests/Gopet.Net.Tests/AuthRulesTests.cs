using Gopet.Net.Auth;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Ràng buộc tài khoản, đối chiếu <c>Player.cs:606</c> (đăng nhập) và
    /// <c>Player.cs:196</c> (đăng ký).
    /// </summary>
    public sealed class AuthRulesTests
    {
        [Theory]
        [InlineData("gopettest")]
        [InlineData("abc123")]
        [InlineData("a")]           // đăng nhập không áp giới hạn độ dài
        public void Username_HopLe(string username)
        {
            Assert.True(AuthRules.IsValidUsername(username, out _));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("GoPetTest")]   // chữ hoa
        [InlineData("go pet")]      // khoảng trắng
        [InlineData("go_pet")]      // gạch dưới
        [InlineData("tênviệt")]
        public void Username_KhongHopLe(string username)
        {
            Assert.False(AuthRules.IsValidUsername(username, out var error));
            Assert.False(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void DangKy_HopLe()
        {
            Assert.True(AuthRules.IsValidRegistration("gopettest", "abc12345", out _));
        }

        [Theory]
        [InlineData("abc12", "abc12345")]    // tên 5 ký tự, cần >= 6
        [InlineData("gopettest", "abc12")]   // mật khẩu 5 ký tự
        [InlineData("Gopettest", "abc12345")] // chữ hoa
        public void DangKy_KhongHopLe(string username, string password)
        {
            Assert.False(AuthRules.IsValidRegistration(username, password, out var error));
            Assert.False(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void DangKy_TenDungNguong25_BiTuChoi()
        {
            // Server: username.Length < 25, nên đúng 25 là hỏng.
            Assert.False(AuthRules.IsValidRegistration(new string('a', 25), "abc12345", out _));
            Assert.True(AuthRules.IsValidRegistration(new string('a', 24), "abc12345", out _));
        }

        [Fact]
        public void DangKy_MatKhauDungNguong60_BiTuChoi()
        {
            Assert.False(AuthRules.IsValidRegistration("gopettest", new string('a', 60), out _));
            Assert.True(AuthRules.IsValidRegistration("gopettest", new string('a', 59), out _));
        }

        /// <summary>
        /// Tên nhân vật (<c>GameController.cs:718-721</c>) chặt hơn tên đăng nhập:
        /// đúng 5-20 ký tự, HAI ĐẦU đều tính. Ngưỡng một phía là ngưỡng mù một nửa.
        /// </summary>
        [Theory]
        [InlineData("gopet")]                   // đúng 5
        [InlineData("abcdefghijklmnopqrst")]    // đúng 20
        public void TenNhanVat_HopLe(string name)
        {
            Assert.True(AuthRules.IsValidCharacterName(name, out _));
        }

        [Theory]
        [InlineData("abcd")]                     // 4
        [InlineData("abcdefghijklmnopqrstu")]    // 21
        [InlineData("GoPet1")]                   // chữ hoa
        [InlineData("go pet")]                   // khoảng trắng
        [InlineData("")]
        public void TenNhanVat_KhongHopLe(string name)
        {
            Assert.False(AuthRules.IsValidCharacterName(name, out var error));
            Assert.False(string.IsNullOrEmpty(error));
        }
    }
}
