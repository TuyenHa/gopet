using System.Text.RegularExpressions;

namespace Gopet.Net.Auth
{
    /// <summary>
    /// Ràng buộc tài khoản, chép đúng từ server để chặn ngay tại client.
    ///
    /// Không phải để thay kiểm tra phía server — chỉ để khỏi tốn một vòng
    /// round-trip cho lỗi biết trước, và để báo lỗi rõ hơn cái dialog đỏ
    /// chung chung mà server trả về.
    /// </summary>
    public static class AuthRules
    {
        /// <summary><c>Player.cs:606</c> — chỉ chữ thường và số.</summary>
        private static readonly Regex UsernamePattern = new Regex("^[a-z0-9]+$", RegexOptions.Compiled);

        public const int MinLength = 6;
        public const int MaxUsernameLength = 25;
        public const int MaxPasswordLength = 60;

        /// <summary>
        /// Kiểm tài khoản dùng để ĐĂNG NHẬP. Server chỉ áp regex ở đường này
        /// (<c>Player.cs:606</c>), không áp giới hạn độ dài như lúc đăng ký.
        /// </summary>
        public static bool IsValidUsername(string username, out string error)
        {
            if (string.IsNullOrEmpty(username))
            {
                error = "Chưa nhập tên tài khoản.";
                return false;
            }

            if (!UsernamePattern.IsMatch(username))
            {
                error = "Tên tài khoản chỉ được có chữ thường và số.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary><c>GameController.cs:719</c> — tên nhân vật, ràng buộc riêng và chặt hơn tên tài khoản.</summary>
        public const int MinCharacterNameLength = 5;

        public const int MaxCharacterNameLength = 20;

        /// <summary>
        /// Kiểm tên NHÂN VẬT (<c>onClienSendCharInfo</c>, <c>GameController.cs:718-721</c>):
        /// cùng regex với tài khoản nhưng dài 5-20, hai đầu đều tính.
        ///
        /// <para>Server không đóng kết nối khi tên sai — nó gửi dialog đỏ rồi gọi
        /// <c>loginOK()</c>, tức là màn tạo nhân vật hiện lại. Chặn tại client để
        /// người chơi không phải đoán mình sai chỗ nào.</para>
        ///
        /// <para>Tên trùng thì chỉ server biết, và nó ĐÓNG kết nối sau dialog đỏ —
        /// client phải vào lại từ đầu, không có cách nào kiểm trước.</para>
        /// </summary>
        public static bool IsValidCharacterName(string name, out string error)
        {
            if (!IsValidUsername(name, out error)) return false;

            if (name.Length < MinCharacterNameLength || name.Length > MaxCharacterNameLength)
            {
                error = $"Tên nhân vật phải từ {MinCharacterNameLength} đến {MaxCharacterNameLength} ký tự.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Kiểm cho ĐĂNG KÝ — chặt hơn đăng nhập (<c>Player.cs:196</c>).
        /// </summary>
        public static bool IsValidRegistration(string username, string password, out string error)
        {
            if (!IsValidUsername(username, out error)) return false;

            if (username.Length < MinLength || username.Length >= MaxUsernameLength)
            {
                error = $"Tên tài khoản phải từ {MinLength} đến {MaxUsernameLength - 1} ký tự.";
                return false;
            }

            if (password == null || password.Length < MinLength || password.Length >= MaxPasswordLength)
            {
                error = $"Mật khẩu phải từ {MinLength} đến {MaxPasswordLength - 1} ký tự.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
