using System;

namespace Gopet.Net
{
    /// <summary>
    /// TEA 128-bit, port nguyên hành vi từ <c>GServer/Server/IO/TEA.cs</c>
    /// (và khớp với <c>er.java</c> của client J2ME cũ).
    ///
    /// KHÔNG "sửa cho đẹp" thuật toán này. Mọi chi tiết — kể cả những chỗ
    /// trông như lỗi (padding lạ, guard im lặng) — là hợp đồng trên dây.
    /// Sửa một phép dịch bit là client không nói chuyện được với server nữa.
    ///
    /// Ghi chú bảo mật: TEA yếu và khoá do chính client sinh rồi gửi lên
    /// (xem <see cref="GopetSocket"/>). Đây là thiết kế gốc, giữ nguyên để
    /// client cũ và client Unity chạy song song được. Đừng nhầm nó với
    /// một lớp bảo mật thật.
    /// </summary>
    public sealed class Tea
    {
        private const int Delta = 1640531527;      // 0x9E3779B9 dưới dạng int có dấu
        private const int SumInitial = -957401312; // = -Delta * 32, đã tràn int32

        private readonly int[] _s = new int[4];

        /// <summary>
        /// Dựng khoá từ một <see cref="long"/> — chính là timestamp client gửi
        /// trong 9 byte handshake. 8 byte của long được lặp lại 2 lần thành khoá 16 byte.
        /// </summary>
        public Tea(long longKey)
        {
            var key = new byte[16];
            GenerateKey(longKey, key);

            var off = 0;
            for (var i = 0; i < 4; i++)
            {
                // Thứ tự byte trong mỗi word là little-endian, dù dây là big-endian.
                // Đúng như server làm — không phải nhầm.
                _s[i] = key[off]
                        | (key[off + 1] << 8)
                        | (key[off + 2] << 16)
                        | (key[off + 3] << 24);
                off += 4;
            }
        }

        /// <summary>Trải 8 byte của <paramref name="value"/> (big-endian) hai lần vào mảng 16 byte.</summary>
        public static void GenerateKey(long value, byte[] array)
        {
            if (array == null || array.Length < 16)
            {
                throw new ArgumentException("Mảng khoá phải có ít nhất 16 byte.", nameof(array));
            }

            for (var i = 0; i < 8; i++)
            {
                var b = (byte)(value >> (56 - i * 8));
                array[i] = b;
                array[i + 8] = b;
            }
        }

        /// <summary>
        /// Mã hoá. Đầu vào phải khác rỗng.
        ///
        /// <para>Với mảng rỗng, <c>TEA.cs</c> của server ném
        /// <see cref="IndexOutOfRangeException"/> — <c>pack()</c> ghi vào
        /// <c>dest[1]</c> trong khi mảng chỉ dài 1. Trường hợp này không xảy ra
        /// thực tế vì mọi <see cref="Message"/> luôn có ít nhất byte opcode.
        /// Chặn tường minh ở đây để lỗi nói đúng nguyên nhân, thay vì nổ ra
        /// một exception khó truy trong lòng thuật toán.</para>
        /// </summary>
        public byte[] Encrypt(byte[] clear)
        {
            if (clear == null) throw new ArgumentNullException(nameof(clear));

            if (clear.Length == 0)
            {
                throw new ArgumentException(
                    "Không mã hoá được mảng rỗng — TEA của server cũng ném lỗi với đầu vào này. " +
                    "Mọi gói tin phải có ít nhất byte opcode.",
                    nameof(clear));
            }

            var paddedSize = ((clear.Length >> 3) + (clear.Length % 8 == 0 ? 0 : 1)) << 1;
            var buffer = new int[paddedSize + 1];
            buffer[0] = clear.Length;
            Pack(clear, buffer, 1);
            Brew(buffer);
            return Unpack(buffer, 0, buffer.Length << 2);
        }

        /// <summary>
        /// Giải mã. Trả <c>null</c> nếu độ dài không hợp lệ — giữ đúng hành vi server.
        /// </summary>
        public byte[] Decrypt(byte[] crypt)
        {
            if (crypt == null) return null;
            if (crypt.Length % 4 != 0 || (crypt.Length >> 2) % 2 != 1) return null;

            var buffer = new int[crypt.Length >> 2];
            Pack(crypt, buffer, 0);
            Unbrew(buffer);

            // buffer[0] là độ dài do bên kia ghi vào — với gói hỏng/độc hại nó
            // có thể âm hoặc rất lớn. Server không chặn âm; ở đây thì có.
            var declaredLength = buffer[0];
            if (declaredLength < 0) return null;

            return Unpack(buffer, 1, declaredLength);
        }

        private void Brew(int[] buf)
        {
            if (buf.Length % 2 != 1) return;

            unchecked
            {
                for (var i = 1; i < buf.Length; i += 2)
                {
                    var v0 = buf[i];
                    var v1 = buf[i + 1];
                    var sum = 0;

                    for (var n = 0; n < 32; n++)
                    {
                        sum -= Delta;
                        v0 += (((v1 << 4) + _s[0]) ^ v1) + (sum ^ (int)((uint)v1 >> 5)) + _s[1];
                        v1 += (((v0 << 4) + _s[2]) ^ v0) + (sum ^ (int)((uint)v0 >> 5)) + _s[3];
                    }

                    buf[i] = v0;
                    buf[i + 1] = v1;
                }
            }
        }

        private void Unbrew(int[] buf)
        {
            if (buf.Length % 2 != 1) return;

            unchecked
            {
                for (var i = 1; i < buf.Length; i += 2)
                {
                    var v0 = buf[i];
                    var v1 = buf[i + 1];
                    var sum = SumInitial;

                    for (var n = 0; n < 32; n++)
                    {
                        v1 -= (((v0 << 4) + _s[2]) ^ v0) + (sum ^ (int)((uint)v0 >> 5)) + _s[3];
                        v0 -= (((v1 << 4) + _s[0]) ^ v1) + (sum ^ (int)((uint)v1 >> 5)) + _s[1];
                        sum += Delta;
                    }

                    buf[i] = v0;
                    buf[i + 1] = v1;
                }
            }
        }

        /// <summary>Gộp byte thành int, big-endian trong từng word. Guard im lặng — giữ đúng server.</summary>
        private static void Pack(byte[] src, int[] dest, int destOffset)
        {
            if (destOffset + (src.Length >> 2) > dest.Length) return;

            var shift = 24;
            var j = destOffset;
            dest[destOffset] = 0;

            for (var i = 0; i < src.Length; i++)
            {
                dest[j] |= src[i] << shift;
                if (shift == 0)
                {
                    shift = 24;
                    j++;
                    if (j < dest.Length) dest[j] = 0;
                }
                else
                {
                    shift -= 8;
                }
            }
        }

        /// <summary>Tách int thành byte. Trả <c>null</c> nếu không đủ dữ liệu — giữ đúng server.</summary>
        private static byte[] Unpack(int[] src, int srcOffset, int destLength)
        {
            if (destLength < 0) return null;
            if (destLength > ((src.Length - srcOffset) << 2)) return null;

            var dest = new byte[destLength];
            var i = srcOffset;
            var count = 0;

            for (var j = 0; j < destLength; j++)
            {
                dest[j] = (byte)((src[i] >> (24 - (count << 3))) & 255);
                count++;
                if (count == 4)
                {
                    count = 0;
                    i++;
                }
            }

            return dest;
        }
    }
}
