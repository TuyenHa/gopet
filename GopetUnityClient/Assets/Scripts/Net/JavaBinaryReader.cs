using System;
using System.IO;
using System.Text;

namespace Gopet.Net
{
    /// <summary>
    /// Đọc kiểu dữ liệu theo quy ước <c>java.io.DataInputStream</c> (big-endian).
    ///
    /// Mọi lỗi đọc (hết dữ liệu, độ dài âm) ném <see cref="ProtocolException"/>
    /// chứ không trả giá trị rác — desync phải nổ ngay tại chỗ, không được
    /// lặng lẽ lan sang gói sau.
    /// </summary>
    public sealed class JavaBinaryReader
    {
        private readonly byte[] _data;
        private int _pos;

        public JavaBinaryReader(byte[] data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public int Position => _pos;
        public int Remaining => _data.Length - _pos;

        private void Require(int count)
        {
            // Viết dạng trừ, KHÔNG phải `_pos + count > _data.Length`: với count sát
            // int.MaxValue thì phép cộng tràn thành số âm và lọt qua kiểm tra, rồi
            // ReadBytes cấp phát một mảng khổng lồ. Ngưỡng của tầng trên chỉ là lớp
            // thứ hai — chỗ chặn thật phải ở đây.
            if (count < 0 || count > _data.Length - _pos)
            {
                throw new ProtocolException(
                    $"Đọc quá cuối gói tin: cần {count} byte tại offset {_pos}, chỉ còn {Remaining}.");
            }
        }

        public sbyte ReadSByte()
        {
            Require(1);
            return unchecked((sbyte)_data[_pos++]);
        }

        public byte ReadByte()
        {
            Require(1);
            return _data[_pos++];
        }

        public bool ReadBool() => ReadByte() != 0;

        public short ReadShort()
        {
            Require(2);
            var v = (short)((_data[_pos] << 8) | _data[_pos + 1]);
            _pos += 2;
            return v;
        }

        public int ReadUnsignedShort()
        {
            Require(2);
            var v = (_data[_pos] << 8) | _data[_pos + 1];
            _pos += 2;
            return v;
        }

        public int ReadInt()
        {
            Require(4);
            var v = (_data[_pos] << 24) | (_data[_pos + 1] << 16) | (_data[_pos + 2] << 8) | _data[_pos + 3];
            _pos += 4;
            return v;
        }

        public long ReadLong()
        {
            Require(8);
            long v = 0;
            for (var i = 0; i < 8; i++)
            {
                v = (v << 8) | _data[_pos + i];
            }
            _pos += 8;
            return v;
        }

        /// <summary>
        /// Đọc chuỗi: 2 byte độ dài không dấu + UTF-8. Đối xứng với
        /// <see cref="JavaBinaryWriter.WriteUtf"/>.
        /// </summary>
        public string ReadUtf()
        {
            var len = ReadUnsignedShort();
            Require(len);
            var s = Encoding.UTF8.GetString(_data, _pos, len);
            _pos += len;
            return s;
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ProtocolException($"Độ dài âm khi đọc mảng byte: {count}.");
            }

            Require(count);
            var buf = new byte[count];
            Buffer.BlockCopy(_data, _pos, buf, 0, count);
            _pos += count;
            return buf;
        }

        /// <summary>
        /// Đọc mảng int có độ dài do server quyết định, kèm chặn trên.
        ///
        /// Luôn dùng hàm này thay vì <c>new int[ReadInt()]</c>. Lỗ hổng
        /// <c>GameController.cs:215</c> phía server chính là lỗi đó — client
        /// không được lặp lại.
        /// </summary>
        public int[] ReadIntArray(int maxCount)
        {
            var count = ReadInt();
            if (count < 0 || count > maxCount)
            {
                throw new ProtocolException(
                    $"Độ dài mảng int không hợp lệ: {count} (cho phép 0..{maxCount}).");
            }

            var arr = new int[count];
            for (var i = 0; i < count; i++) arr[i] = ReadInt();
            return arr;
        }

        /// <summary>
        /// Kiểm tra đã đọc hết gói tin. Gọi ở cuối mỗi handler — thừa byte
        /// nghĩa là parser sai, và bắt được ngay lúc đó rẻ hơn nhiều so với
        /// truy ngược từ triệu chứng ở màn hình.
        /// </summary>
        public void ExpectFullyConsumed(string context)
        {
            if (Remaining != 0)
            {
                throw new ProtocolException(
                    $"{context}: còn thừa {Remaining} byte chưa đọc. Parser lệch so với server.");
            }
        }
    }

    /// <summary>Lỗi giao thức: gói tin không khớp với format mong đợi.</summary>
    public sealed class ProtocolException : Exception
    {
        public ProtocolException(string message) : base(message) { }
    }
}
