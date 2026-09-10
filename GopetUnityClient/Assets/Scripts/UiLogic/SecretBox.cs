using System;
using System.Security.Cryptography;
using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Bọc một chuỗi bằng AES-256-CBC rồi ký HMAC-SHA256 (encrypt-then-MAC).
    /// Thuần C#, không UnityEngine — test được ngoài Editor.
    ///
    /// <para><b>Nói thẳng mức bảo vệ:</b> khoá nằm cùng máy với dữ liệu, nên đây là
    /// chống ĐỌC LƯỚT — người nhặt được file, hoặc app khác đọc chung thư mục, không
    /// thấy mật khẩu. Nó KHÔNG chống được người đã chiếm quyền trên chính thiết bị
    /// đó: có khoá là mở được. Muốn hơn thì phải là Keychain (iOS) /
    /// EncryptedSharedPreferences (Android), tức là plugin native.</para>
    ///
    /// <para>Ký HMAC chứ không chỉ mã hoá: thiếu MAC thì file sửa một byte vẫn giải
    /// mã ra rác mà không ai biết, và padding oracle là chuyện có thật. Kiểm MAC
    /// trước rồi mới giải mã.</para>
    /// </summary>
    public static class SecretBox
    {
        public const int KeySize = 32;

        private const int IvSize = 16;
        private const int MacSize = 32;

        private static readonly byte[] CipherLabel = Encoding.ASCII.GetBytes("gopet-cipher-v1");
        private static readonly byte[] MacLabel = Encoding.ASCII.GetBytes("gopet-mac-v1");

        public static byte[] NewKey()
        {
            var key = new byte[KeySize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            return key;
        }

        /// <summary>Trả về base64 của <c>iv || ciphertext || mac</c>.</summary>
        public static string Seal(string plaintext, byte[] key)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            RequireKey(key);

            using (var aes = Aes.Create())
            {
                aes.Key = Derive(key, CipherLabel);
                aes.GenerateIV();

                byte[] cipher;
                using (var encryptor = aes.CreateEncryptor())
                {
                    var bytes = Encoding.UTF8.GetBytes(plaintext);
                    cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                }

                var body = new byte[IvSize + cipher.Length];
                Buffer.BlockCopy(aes.IV, 0, body, 0, IvSize);
                Buffer.BlockCopy(cipher, 0, body, IvSize, cipher.Length);

                var sealedBytes = new byte[body.Length + MacSize];
                Buffer.BlockCopy(body, 0, sealedBytes, 0, body.Length);
                Buffer.BlockCopy(Mac(body, key), 0, sealedBytes, body.Length, MacSize);

                return Convert.ToBase64String(sealedBytes);
            }
        }

        /// <summary>
        /// Mở gói. Trả <c>false</c> cho mọi thứ không mở được — sai khoá, file hỏng,
        /// base64 rác. Người gọi coi đó là "chưa lưu gì" và hỏi lại người dùng.
        /// </summary>
        public static bool TryOpen(string sealedText, byte[] key, out string plaintext)
        {
            plaintext = null;
            if (string.IsNullOrEmpty(sealedText) || key == null || key.Length != KeySize) return false;

            byte[] sealedBytes;
            try
            {
                sealedBytes = Convert.FromBase64String(sealedText);
            }
            catch (FormatException)
            {
                return false;
            }

            if (sealedBytes.Length < IvSize + MacSize) return false;

            var bodyLength = sealedBytes.Length - MacSize;
            var body = new byte[bodyLength];
            Buffer.BlockCopy(sealedBytes, 0, body, 0, bodyLength);

            // Cộng dồn chênh lệch rồi mới so MỘT lần: `return false` ngay khi thấy
            // byte lệch làm thời gian chạy tỉ lệ với số byte đầu đã khớp, và đó là
            // rò thời gian thật. (Bản đầu ghi comment "so hết mảng" nhưng code lại
            // thoát sớm — comment nói một đằng, code làm một nẻo.)
            var expected = Mac(body, key);
            var diff = 0;
            for (var i = 0; i < MacSize; i++)
            {
                diff |= sealedBytes[bodyLength + i] ^ expected[i];
            }

            if (diff != 0) return false;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Derive(key, CipherLabel);

                    var iv = new byte[IvSize];
                    Buffer.BlockCopy(body, 0, iv, 0, IvSize);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        var clear = decryptor.TransformFinalBlock(body, IvSize, bodyLength - IvSize);
                        plaintext = Encoding.UTF8.GetString(clear);
                        return true;
                    }
                }
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private static byte[] Mac(byte[] data, byte[] key)
        {
            using (var hmac = new HMACSHA256(Derive(key, MacLabel)))
            {
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>
        /// Tách một khoá gốc thành hai khoá con: một để mã hoá, một để ký.
        ///
        /// <para>Dùng CHUNG một khoá cho AES và HMAC không có lỗ hổng nào đã biết,
        /// nhưng nó là thứ mọi tài liệu về encrypt-then-MAC đều dặn đừng làm, và cái
        /// giá để làm đúng ở đây là bốn dòng.</para>
        /// </summary>
        private static byte[] Derive(byte[] key, byte[] label)
        {
            using (var hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(label);
            }
        }

        private static void RequireKey(byte[] key)
        {
            if (key == null || key.Length != KeySize)
            {
                throw new ArgumentException($"Khoá phải đúng {KeySize} byte.", nameof(key));
            }
        }
    }
}
