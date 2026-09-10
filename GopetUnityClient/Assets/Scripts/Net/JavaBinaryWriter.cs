using System;
using System.IO;
using System.Text;

namespace Gopet.Net
{
    /// <summary>
    /// Ghi kiểu dữ liệu theo quy ước <c>java.io.DataOutputStream</c> (big-endian).
    ///
    /// KHÔNG dùng <see cref="BinaryWriter"/> của .NET — nó ghi little-endian,
    /// ngược với Java/J2ME mà GServer đang nói.
    ///
    /// Mirror của <c>GServer/Server/IO/DataOutputStream.cs</c>.
    /// </summary>
    public sealed class JavaBinaryWriter : IDisposable
    {
        private readonly MemoryStream _stream;

        public JavaBinaryWriter()
        {
            _stream = new MemoryStream();
        }

        public int Length => (int)_stream.Length;

        public void WriteSByte(int value)
        {
            _stream.WriteByte((byte)(value & 0xFF));
        }

        public void WriteBool(bool value)
        {
            _stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteShort(int value)
        {
            // Server: writeShort(int) ép về short rồi ghi 2 byte big-endian.
            var v = (short)value;
            _stream.WriteByte((byte)(v >> 8));
            _stream.WriteByte((byte)v);
        }

        public void WriteInt(int value)
        {
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        public void WriteLong(long value)
        {
            for (var i = 7; i >= 0; i--)
            {
                _stream.WriteByte((byte)(value >> (i * 8)));
            }
        }

        /// <summary>
        /// Ghi chuỗi: 2 byte độ dài (big-endian, không dấu) + UTF-8.
        ///
        /// Lưu ý: server ghi <b>UTF-8 thường</b>, không phải Java modified UTF-8
        /// (xem <c>DataOutputStream.cs:137-149</c> — nó dùng <c>Encoding.Convert</c>
        /// sang codepage 65001). Với ký tự BMP thì hai thứ này trùng nhau,
        /// nên <see cref="Encoding.UTF8"/> là đúng.
        ///
        /// Ngoại lệ đã biết: ký tự ngoài BMP (emoji) sẽ lệch — Java ghi CESU-8
        /// 6 byte, .NET ghi UTF-8 4 byte. Chưa xử lý vì game gốc không dùng.
        /// </summary>
        public void WriteUtf(string value)
        {
            value ??= string.Empty;
            var bytes = Encoding.UTF8.GetBytes(value);

            if (bytes.Length > ushort.MaxValue)
            {
                throw new ArgumentException(
                    $"Chuỗi dài {bytes.Length} byte, vượt giới hạn {ushort.MaxValue} của khung UTF.",
                    nameof(value));
            }

            _stream.WriteByte((byte)(bytes.Length >> 8));
            _stream.WriteByte((byte)bytes.Length);
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null || value.Length == 0) return;
            _stream.Write(value, 0, value.Length);
        }

        public byte[] ToArray() => _stream.ToArray();

        public void Dispose() => _stream.Dispose();
    }
}
