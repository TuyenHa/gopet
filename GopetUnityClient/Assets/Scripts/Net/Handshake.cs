using System.IO;

namespace Gopet.Net
{
    /// <summary>
    /// Chín byte đầu tiên của mọi phiên: client tự sinh khoá TEA rồi gửi lên,
    /// server dùng đúng khoá đó.
    ///
    /// <code>
    /// [0]    = 0x09          độ dài
    /// [1..8] = khoá big-endian
    /// </code>
    ///
    /// Mirror của <c>eq.java:a(long)</c> phía client cũ và
    /// <c>GServer/Server/IO/Session.readKey()</c> phía server.
    ///
    /// Tách khỏi <see cref="GopetSocket"/> để test được mà không cần socket,
    /// giống cách <see cref="PacketFramer"/> tách phần đóng khung.
    /// </summary>
    public static class Handshake
    {
        /// <summary>Số byte cố định của gói handshake.</summary>
        public const int Length = 9;

        public static byte[] Build(long key)
        {
            var buf = new byte[Length];
            buf[0] = Length;
            for (var i = 0; i < 8; i++)
            {
                buf[i + 1] = (byte)(key >> (56 - i * 8));
            }

            return buf;
        }

        public static void Write(Stream stream, long key)
        {
            var buf = Build(key);
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }
    }
}
