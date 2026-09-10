using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Gopet.Net.Images
{
    /// <summary>
    /// Cache PNG xuống đĩa, xoá theo LRU khi vượt hạn mức.
    ///
    /// <para>Ghi PNG THÔ chứ không ghi texture đã giải mã: nhỏ hơn nhiều lần và
    /// nạp lại nhanh vì Unity giải mã PNG sẵn.</para>
    ///
    /// <para>Thuần .NET, nhận thư mục gốc qua tham số — nhờ vậy test được ngoài
    /// Unity, không cần <c>Application.persistentDataPath</c>.</para>
    ///
    /// <para>Client J2ME cũ cũng cache qua RMS (<c>ef.java</c>), nên đây đúng thiết
    /// kế gốc chứ không phải sáng kiến thêm.</para>
    /// </summary>
    public sealed class AssetDiskStore
    {
        private const string FileExtension = ".png";

        private readonly string _root;
        private readonly long _maxBytes;
        private bool _usable = true;

        /// <summary>Tổng byte ước lượng, để khỏi quét cả thư mục sau MỖI lần ghi.</summary>
        private long _approxBytes = -1;

        /// <param name="root">Thư mục chứa cache. Tự tạo nếu chưa có.</param>
        /// <param name="maxBytes">Hạn mức; vượt thì xoá dần file cũ nhất.</param>
        public AssetDiskStore(string root, long maxBytes = 200L * 1024 * 1024)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _maxBytes = maxBytes;

            // Không tạo được thư mục thì chạy ở chế độ không cache, KHÔNG ném:
            // cache là tối ưu, để nó làm sập cả đường ống ảnh là sai tỷ lệ.
            try { Directory.CreateDirectory(_root); }
            catch (IOException) { _usable = false; }
            catch (UnauthorizedAccessException) { _usable = false; }
        }

        /// <summary><c>false</c> khi không dùng được đĩa; mọi thao tác thành không-làm-gì.</summary>
        public bool IsUsable => _usable;

        /// <summary>
        /// Tên file = băm của đường dẫn.
        ///
        /// Đường dẫn gốc có khoảng trắng, dấu <c>\</c>, và có khi là số id vật phẩm —
        /// dùng thẳng làm tên file thì vỡ trên Windows hoặc đụng giới hạn độ dài.
        /// </summary>
        public static string KeyOf(string path)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(path));

            var sb = new StringBuilder(32);
            for (var i = 0; i < 16; i++) sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }

        private string PathFor(string assetPath) => Path.Combine(_root, KeyOf(assetPath) + FileExtension);

        public bool TryRead(string assetPath, out byte[] png)
        {
            png = null;
            if (!_usable) return false;

            var file = PathFor(assetPath);

            try
            {
                if (!File.Exists(file)) return false;
                png = File.ReadAllBytes(file);
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            // "Chạm" để đánh dấu vừa dùng — LastWriteTime chứ không phải
            // LastAccessTime: NTFS mặc định TẮT cập nhật last-access nên xếp hạng
            // LRU theo nó sẽ sai hoàn toàn.
            //
            // Nằm NGOÀI khối try ở trên: chạm hỏng chỉ làm LRU kém chính xác, không
            // có lý do gì để vứt đi tấm PNG vừa đọc thành công.
            try { File.SetLastWriteTimeUtc(file, DateTime.UtcNow); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return true;
        }

        public void Write(string assetPath, byte[] png)
        {
            if (!_usable || png == null || png.Length == 0) return;

            try
            {
                File.WriteAllBytes(PathFor(assetPath), png);

                // Quét cả thư mục sau mỗi lần ghi là 10k FileInfo cho mỗi tấm ảnh.
                // Cộng dồn ước lượng, chỉ quét thật khi có khả năng đã vượt.
                if (_approxBytes < 0) _approxBytes = TotalBytes();
                else _approxBytes += png.Length;

                if (_approxBytes > _maxBytes)
                {
                    EvictIfNeeded();
                    _approxBytes = TotalBytes();
                }
            }
            catch (IOException)
            {
                // Hết đĩa hoặc file đang bị khoá: cache là tối ưu, không phải
                // chức năng — hỏng thì lần sau tải lại, không được làm sập game.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public long TotalBytes() => EnumerateFiles().Sum(f => f.Length);

        /// <summary>Xoá dần file cũ nhất cho tới khi về dưới hạn mức.</summary>
        public void EvictIfNeeded()
        {
            var files = EnumerateFiles().OrderBy(f => f.LastWriteTimeUtc).ToList();
            var total = files.Sum(f => f.Length);

            foreach (var file in files)
            {
                if (total <= _maxBytes) return;

                try
                {
                    var size = file.Length;
                    file.Delete();
                    total -= size;
                }
                catch (IOException)
                {
                }
            }
        }

        private FileInfo[] EnumerateFiles()
        {
            try
            {
                return new DirectoryInfo(_root).GetFiles("*" + FileExtension);
            }
            catch (DirectoryNotFoundException)
            {
                return Array.Empty<FileInfo>();
            }
        }
    }
}
