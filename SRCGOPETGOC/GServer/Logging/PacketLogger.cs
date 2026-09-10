using System.Globalization;
using System.Text;

namespace Gopet.Logging
{
    public enum PacketDirection
    {
        /// <summary>Server -> Client (theo góc nhìn máy chủ là đi ra).</summary>
        Out,

        /// <summary>Client -> Server.</summary>
        In
    }

    /// <summary>
    /// Ghi hex dump mọi gói tin ra file, format khớp <c>ClientPacketLogger</c>
    /// của client Unity để công cụ <c>packet-diff</c> so được hai bên.
    ///
    /// <para>Đây không phải tiện ích phụ. Rủi ro lớn nhất của việc viết lại
    /// client là desync giao thức — đọc lệch một trường thì cả luồng hỏng và
    /// lỗi hiện ra ở chỗ hoàn toàn khác. Cách bắt duy nhất hiệu quả là làm
    /// cùng một thao tác trên client cũ và client mới rồi so hai dump.</para>
    ///
    /// <para><b>Chú ý hướng:</b> ở đây <c>Out</c> nghĩa là máy chủ gửi đi. Bên
    /// client thì <c>Out</c> là client gửi đi. Nên khi so, ghép
    /// <c>IN</c> của máy chủ với <c>OUT</c> của client và ngược lại —
    /// <c>packet-diff</c> có cờ <c>--direction</c> cho việc đó.</para>
    ///
    /// <para>Ghi theo lô ở luồng nền: mọi gói tin đều đi qua đây, nên chặn
    /// luồng đọc/gửi để chờ đĩa sẽ làm méo chính thứ đang đo.</para>
    /// </summary>
    public sealed class PacketLogger : IDisposable
    {
        /// <summary>Số byte tối đa dump mỗi gói. Gói ảnh có thể vài trăm KB.</summary>
        private const int MaxHexBytes = 256;

        private static readonly Lazy<PacketLogger?> LazyInstance = new(Create);

        /// <summary>Trả về <c>null</c> khi tắt trong config — chỗ gọi phải kiểm null.</summary>
        public static PacketLogger? Instance => LazyInstance.Value;

        private readonly System.Collections.Concurrent.BlockingCollection<string> _queue = new(8192);
        private readonly StreamWriter _writer;
        private readonly Thread _worker;

        private static PacketLogger? Create()
        {
            if (!ServerSetting.instance.enablePacketLog)
            {
                return null;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "log", "packet-dump-server.log");
            var logger = new PacketLogger(path);
            GopetManager.ServerMonitor.LogWarning($"Ghi log gói tin: {path}");
            return logger;
        }

        private PacketLogger(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            _writer = new StreamWriter(filePath, append: false, Encoding.UTF8);
            _writer.WriteLine("# timestamp\tdir\topcode\tenc\tlen\thex");
            _writer.Flush();

            _worker = new Thread(DrainQueue)
            {
                IsBackground = true,
                Name = "Packet Logger"
            };
            _worker.Start();
        }

        public void Log(PacketDirection direction, sbyte opcode, bool encrypted, sbyte[] payload)
        {
            var length = payload?.Length ?? 0;
            var dumpLength = Math.Min(length, MaxHexBytes);

            var hex = new StringBuilder(dumpLength * 2);
            for (var i = 0; i < dumpLength; i++)
            {
                hex.Append(((byte)payload![i]).ToString("x2", CultureInfo.InvariantCulture));
            }

            if (length > dumpLength)
            {
                hex.Append("...");
            }

            var line = string.Join('\t',
                DateTime.UtcNow.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                direction == PacketDirection.Out ? "OUT" : "IN",
                opcode.ToString(CultureInfo.InvariantCulture),
                encrypted ? "1" : "0",
                length.ToString(CultureInfo.InvariantCulture),
                hex.ToString());

            // Bỏ gói log khi hàng đợi đầy, chứ không chặn. Log gói tin là công cụ
            // chẩn đoán — nó không được phép làm chậm chính thứ nó đang đo.
            _queue.TryAdd(line);
        }

        private void DrainQueue()
        {
            try
            {
                foreach (var line in _queue.GetConsumingEnumerable())
                {
                    _writer.WriteLine(line);

                    // Chỉ flush khi đã rút hết hàng đợi: giữ được dữ liệu lúc
                    // crash mà không phải chạm đĩa cho từng gói.
                    if (_queue.Count == 0)
                    {
                        _writer.Flush();
                    }
                }
            }
            catch (Exception)
            {
                // Đang tắt máy chủ, hoặc lỗi đĩa. Không kéo theo luồng mạng.
            }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _worker.Join(TimeSpan.FromSeconds(2));
            _writer.Flush();
            _writer.Dispose();
            _queue.Dispose();
        }
    }
}
