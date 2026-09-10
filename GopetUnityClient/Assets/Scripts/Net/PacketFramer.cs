using System;
using System.IO;

namespace Gopet.Net
{
    /// <summary>
    /// Đóng/mở khung gói tin trên một <see cref="Stream"/> bất kỳ.
    ///
    /// Tách riêng khỏi <see cref="GopetSocket"/> để test được format khung
    /// bằng <see cref="MemoryStream"/>, không cần server thật.
    ///
    /// Khung:
    /// <code>
    /// [int32 big-endian: payloadLength + 1][byte: isEncrypted][payload...]
    /// </code>
    ///
    /// Mirror của <c>MsgSender.doSendMessage()</c> và <c>MsgReader.readMessage()</c>.
    /// </summary>
    public static class PacketFramer
    {
        /// <summary>
        /// Server từ chối gói lớn hơn ngưỡng này (<c>MsgReader.cs</c>:
        /// <c>if (Length > 10000) throw new IOException("Dữ liệu quá lớn")</c>).
        /// Client dùng cùng ngưỡng cho cả hai chiều để bắt lỗi sớm.
        /// </summary>
        public const int MaxPacketSize = 10000;

        public static void WriteFrame(Stream stream, byte[] payload, bool encrypted)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            if (payload.Length + 1 > MaxPacketSize)
            {
                throw new ProtocolException(
                    $"Gói {payload.Length + 1} byte vượt giới hạn {MaxPacketSize} của server.");
            }

            var length = payload.Length + 1;
            stream.WriteByte((byte)(length >> 24));
            stream.WriteByte((byte)(length >> 16));
            stream.WriteByte((byte)(length >> 8));
            stream.WriteByte((byte)length);
            stream.WriteByte(encrypted ? (byte)1 : (byte)0);
            stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }

        /// <summary>
        /// Đọc một khung. Trả <c>false</c> khi server đóng kết nối bình thường.
        /// </summary>
        public static bool TryReadFrame(Stream stream, out byte[] payload, out bool encrypted)
        {
            payload = null;
            encrypted = false;

            var header = ReadExactly(stream, 4);
            if (header == null) return false;

            var frameLength = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];

            // Server dùng -1 làm tín hiệu kết thúc (MsgReader.cs).
            if (frameLength == -1) return false;

            var payloadLength = frameLength - 1;
            if (payloadLength < 0 || payloadLength > MaxPacketSize)
            {
                throw new ProtocolException($"Độ dài gói không hợp lệ: {payloadLength}.");
            }

            var flag = stream.ReadByte();
            if (flag == -1) return false;

            encrypted = flag == 1;

            // Khung độ dài 0 là hợp lệ (server gửi khi message rỗng) — bỏ qua.
            if (payloadLength == 0)
            {
                payload = Array.Empty<byte>();
                return true;
            }

            payload = ReadExactly(stream, payloadLength)
                      ?? throw new EndOfStreamException("Kết nối đứt giữa lúc đọc thân gói tin.");

            return true;
        }

        /// <summary>
        /// Đọc đủ <paramref name="count"/> byte, trả <c>null</c> nếu stream kết thúc
        /// đúng lúc chưa đọc byte nào.
        ///
        /// <see cref="Stream.Read(byte[], int, int)"/> được phép trả về ít hơn yêu cầu.
        /// Quên vòng lặp này là lỗi gói tin ngắt quãng — chỉ hiện ra khi mạng chậm,
        /// và cực khó truy.
        /// </summary>
        private static byte[] ReadExactly(Stream stream, int count)
        {
            var buf = new byte[count];
            var read = 0;

            while (read < count)
            {
                var n = stream.Read(buf, read, count - read);
                if (n <= 0)
                {
                    if (read == 0) return null;
                    throw new EndOfStreamException(
                        $"Kết nối đứt giữa chừng: cần {count} byte, mới đọc được {read}.");
                }

                read += n;
            }

            return buf;
        }
    }
}
