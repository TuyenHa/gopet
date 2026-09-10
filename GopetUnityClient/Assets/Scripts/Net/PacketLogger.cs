using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Gopet.Net
{
    public enum PacketDirection
    {
        /// <summary>Client -> Server</summary>
        Out,

        /// <summary>Server -> Client</summary>
        In
    }

    /// <summary>
    /// Ghi hex dump gói tin ra file, format khớp với logger phía GServer
    /// để công cụ <c>packet-diff</c> so được hai bên.
    ///
    /// Đây không phải tiện ích phụ — nó là công cụ chính để bắt desync,
    /// tức rủi ro số 1 của cả dự án. Bật từ ngày đầu.
    ///
    /// Format mỗi dòng:
    /// <code>
    /// {timestamp}\t{IN|OUT}\t{opcode}\t{enc:0|1}\t{length}\t{hex}
    /// </code>
    /// </summary>
    public sealed class PacketLogger : IDisposable
    {
        /// <summary>Số byte tối đa dump ra mỗi gói. Gói ảnh có thể vài trăm KB.</summary>
        private const int MaxHexBytes = 256;

        private readonly StreamWriter _writer;
        private readonly object _lock = new object();

        public PacketLogger(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            _writer = new StreamWriter(filePath, append: false, Encoding.UTF8) { AutoFlush = true };
            _writer.WriteLine("# timestamp\tdir\topcode\tenc\tlen\thex");
        }

        public void Log(PacketDirection direction, sbyte opcode, bool encrypted, byte[] payload)
        {
            var length = payload?.Length ?? 0;
            var dumpLen = Math.Min(length, MaxHexBytes);

            var hex = new StringBuilder(dumpLen * 2);
            for (var i = 0; i < dumpLen; i++)
            {
                hex.Append(payload[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            if (length > dumpLen) hex.Append("...");

            lock (_lock)
            {
                _writer.Write(DateTime.UtcNow.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture));
                _writer.Write('\t');
                _writer.Write(direction == PacketDirection.Out ? "OUT" : "IN");
                _writer.Write('\t');
                _writer.Write(opcode.ToString(CultureInfo.InvariantCulture));
                _writer.Write('\t');
                _writer.Write(encrypted ? '1' : '0');
                _writer.Write('\t');
                _writer.Write(length.ToString(CultureInfo.InvariantCulture));
                _writer.Write('\t');
                _writer.WriteLine(hex.ToString());
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _writer?.Dispose();
            }
        }
    }
}
