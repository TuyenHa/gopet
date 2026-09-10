using System;
using System.IO;
using System.Text;

namespace Gopet.UiLogic
{
    /// <summary>Tài khoản đã lưu. <see cref="Password"/> rỗng khi người dùng không cho nhớ mật khẩu.</summary>
    public sealed class SavedCredentials
    {
        public string Username = string.Empty;
        public string Password = string.Empty;

        public bool HasPassword => !string.IsNullOrEmpty(Password);
    }

    /// <summary>
    /// Lưu tài khoản giữa hai lần mở app. Thuần .NET, nhận thư mục qua tham số —
    /// test được ngoài Unity, giống <c>AssetDiskStore</c>.
    ///
    /// <para><b>Tên tài khoản lưu thẳng, mật khẩu thì không.</b> Tên không phải bí
    /// mật và người chơi muốn thấy nó điền sẵn; mật khẩu đi qua
    /// <see cref="SecretBox"/> với khoá sinh riêng cho từng lần cài
    /// (<c>device.key</c>). Đọc kỹ giới hạn ghi trong <see cref="SecretBox"/>: đây
    /// là chống đọc lướt, không phải Keychain.</para>
    ///
    /// <para>Hỏng ở bất kỳ khâu nào (mất khoá, file rác, không có quyền ghi) đều
    /// quy về "chưa lưu gì" và hỏi lại người dùng. Ném lỗi ở màn đăng nhập vì cái
    /// tiện ích này là sai tỷ lệ.</para>
    /// </summary>
    public sealed class CredentialStore
    {
        private const string CredentialsFile = "credentials.txt";
        private const string KeyFile = "device.key";

        private readonly string _root;

        public CredentialStore(string root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>Trả về bản ghi rỗng khi chưa lưu gì hoặc không đọc được.</summary>
        public SavedCredentials Load()
        {
            var empty = new SavedCredentials();

            string[] lines;
            try
            {
                var path = Path.Combine(_root, CredentialsFile);
                if (!File.Exists(path)) return empty;

                lines = File.ReadAllLines(path, Encoding.UTF8);
            }
            catch (Exception e) when (IsRecoverable(e))
            {
                return empty;
            }

            if (lines.Length == 0) return empty;

            var result = new SavedCredentials { Username = lines[0] };

            // Dòng hai là mật khẩu đã bọc; không có nghĩa là người dùng chọn không nhớ.
            if (lines.Length > 1 && lines[1].Length > 0)
            {
                var key = LoadKey();
                try
                {
                    if (key != null && SecretBox.TryOpen(lines[1], key, out var password))
                    {
                        result.Password = password;
                    }
                }
                catch (Exception e) when (IsRecoverable(e))
                {
                    // Mật khẩu coi như chưa lưu.
                }
            }

            return result;
        }

        /// <param name="password">Để trống hoặc <c>null</c> nếu người dùng không cho nhớ mật khẩu.</param>
        public void Save(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                Clear();
                return;
            }

            try
            {
                var wrapped = string.Empty;
                if (!string.IsNullOrEmpty(password))
                {
                    var key = LoadOrCreateKey();
                    if (key != null) wrapped = SecretBox.Seal(password, key);
                }

                Directory.CreateDirectory(_root);
                File.WriteAllLines(Path.Combine(_root, CredentialsFile), new[] { username, wrapped }, Encoding.UTF8);
            }
            catch (Exception e) when (IsRecoverable(e))
            {
                // Không lưu được thì thôi; lần sau người dùng gõ lại.
            }
        }

        /// <summary>Xoá cả bản ghi lẫn khoá — "đăng xuất" phải xoá được thật.</summary>
        public void Clear()
        {
            Delete(CredentialsFile);
            Delete(KeyFile);
        }

        private byte[] LoadKey()
        {
            try
            {
                var path = Path.Combine(_root, KeyFile);
                if (!File.Exists(path)) return null;

                var key = File.ReadAllBytes(path);
                return key.Length == SecretBox.KeySize ? key : null;
            }
            catch (Exception e) when (IsRecoverable(e))
            {
                return null;
            }
        }

        private byte[] LoadOrCreateKey()
        {
            var existing = LoadKey();
            if (existing != null) return existing;

            var key = SecretBox.NewKey();
            try
            {
                Directory.CreateDirectory(_root);
                File.WriteAllBytes(Path.Combine(_root, KeyFile), key);
                return key;
            }
            catch (Exception e) when (IsRecoverable(e))
            {
                // Ghi khoá không được thì đừng bọc mật khẩu bằng khoá chỉ sống
                // trong RAM — lần mở sau không có gì để giải mã, chỉ tổ ghi rác.
                return null;
            }
        }

        private void Delete(string name)
        {
            try
            {
                var path = Path.Combine(_root, name);
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e) when (IsRecoverable(e))
            {
            }
        }

        /// <summary>
        /// Mọi thứ hỏng được ở đây đều quy về "chưa lưu gì".
        ///
        /// <para>Không chỉ lỗi đĩa: <c>Aes.Create()</c> có thể trả <c>null</c> hoặc ném
        /// <see cref="System.Security.Cryptography.CryptographicException"/> trên nền
        /// tảng bị cắt bớt thư viện (IL2CPP + managed stripping). Để nó thoát ra ngoài
        /// thì một tiện ích ghi nhớ tài khoản chặn mất cả lần đăng nhập thành công —
        /// sai tỷ lệ hoàn toàn.</para>
        /// </summary>
        private static bool IsRecoverable(Exception e)
        {
            return e is IOException
                   || e is UnauthorizedAccessException
                   || e is NotSupportedException
                   || e is System.Security.Cryptography.CryptographicException
                   || e is ArgumentException
                   || e is PlatformNotSupportedException
                   || e is NullReferenceException;
        }
    }
}
