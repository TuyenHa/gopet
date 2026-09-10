using System;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Đọc big-endian trên byte[], khớp <c>java.io.DataInputStream</c>. Dùng chung cho
    /// mọi format nhị phân trong jar (map, string table, ảnh dải…).
    /// </summary>
    /// <remarks>
    /// Tách khỏi <see cref="JarMapLayout"/> vì các format khác cũng cần cùng bộ đọc.
    /// Public vì test và các parser khác cần dùng lại.
    /// </remarks>
    public sealed class JarBigEndianReader
    {
        private readonly byte[] _data;
        private int _p;

        public JarBigEndianReader(byte[] data) => _data = data ?? throw new ArgumentNullException(nameof(data));

        public int Position => _p;

        public bool Eof => _p >= _data.Length;

        public int Byte() => Next();

        public int SignedByte() => (sbyte)Next();

        public int Short() => (short)((Next() << 8) | Next());

        public int Int() => (Next() << 24) | (Next() << 16) | (Next() << 8) | Next();

        public byte[] Bytes(int count)
        {
            var slice = new byte[count];
            for (var i = 0; i < count; i++) slice[i] = (byte)Next();
            return slice;
        }

        /// <summary>
        /// Đọc chuỗi kiểu <c>DataInputStream.readUTF()</c>: 2 byte length + bytes UTF-8 (modified).
        /// </summary>
        public string Utf()
        {
            var len = (Next() << 8) | Next();
            if (len == 0) return string.Empty;
            var raw = Bytes(len);
            return System.Text.Encoding.UTF8.GetString(raw);
        }

        private int Next()
        {
            if (_p >= _data.Length) throw new InvalidOperationException("Đọc quá cuối file — bố cục không khớp nguồn.");
            return _data[_p++];
        }
    }
}
