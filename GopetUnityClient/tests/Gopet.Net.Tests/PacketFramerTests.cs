using System;
using System.IO;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Format khung gói, test qua MemoryStream — không cần server.
    /// </summary>
    public sealed class PacketFramerTests
    {
        [Fact]
        public void WriteFrame_DungCauTruc()
        {
            using var ms = new MemoryStream();
            PacketFramer.WriteFrame(ms, new byte[] { 0xDC, 0x01 }, encrypted: true);

            // [int32 BE = 2+1][byte 1][0xDC][0x01]
            Assert.Equal("00000003" + "01" + "dc01", TestVectorData.ToHex(ms.ToArray()));
        }

        [Fact]
        public void WriteFrame_CoPhanBietCoMaHoa()
        {
            using var plain = new MemoryStream();
            using var enc = new MemoryStream();

            PacketFramer.WriteFrame(plain, new byte[] { 1 }, encrypted: false);
            PacketFramer.WriteFrame(enc, new byte[] { 1 }, encrypted: true);

            Assert.Equal(0, plain.ToArray()[4]);
            Assert.Equal(1, enc.ToArray()[4]);
        }

        [Fact]
        public void RoundTrip_QuaMemoryStream()
        {
            var payload = new byte[] { 0xDC, 0x00, 0x2A, 0xFF };

            using var ms = new MemoryStream();
            PacketFramer.WriteFrame(ms, payload, encrypted: true);
            ms.Position = 0;

            Assert.True(PacketFramer.TryReadFrame(ms, out var read, out var encrypted));
            Assert.Equal(payload, read);
            Assert.True(encrypted);
        }

        [Fact]
        public void NhieuKhungNoiTiep_DocDungTungCai()
        {
            using var ms = new MemoryStream();
            PacketFramer.WriteFrame(ms, new byte[] { 1 }, false);
            PacketFramer.WriteFrame(ms, new byte[] { 2, 3 }, true);
            PacketFramer.WriteFrame(ms, new byte[] { 4, 5, 6 }, false);
            ms.Position = 0;

            Assert.True(PacketFramer.TryReadFrame(ms, out var a, out var encA));
            Assert.True(PacketFramer.TryReadFrame(ms, out var b, out var encB));
            Assert.True(PacketFramer.TryReadFrame(ms, out var c, out var encC));

            Assert.Equal(new byte[] { 1 }, a);
            Assert.False(encA);
            Assert.Equal(new byte[] { 2, 3 }, b);
            Assert.True(encB);
            Assert.Equal(new byte[] { 4, 5, 6 }, c);
            Assert.False(encC);

            Assert.False(PacketFramer.TryReadFrame(ms, out _, out _));
        }

        [Fact]
        public void StreamRong_TraVeFalseChuKhongNem()
        {
            using var ms = new MemoryStream();

            Assert.False(PacketFramer.TryReadFrame(ms, out _, out _));
        }

        [Fact]
        public void DoDaiVuotNguong_BiTuChoi()
        {
            // Server ném IOException("Dữ liệu quá lớn") khi > 10000.
            // Client phải chặn trước khi gửi.
            using var ms = new MemoryStream();

            Assert.Throws<ProtocolException>(
                () => PacketFramer.WriteFrame(ms, new byte[PacketFramer.MaxPacketSize], false));
        }

        [Fact]
        public void DoDaiKhaiBaoQuaLon_BiTuChoiKhiDoc()
        {
            using var ms = new MemoryStream();
            ms.Write(new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, 0, 4); // int.MaxValue
            ms.WriteByte(0);
            ms.Position = 0;

            Assert.Throws<ProtocolException>(() => PacketFramer.TryReadFrame(ms, out _, out _));
        }

        [Fact]
        public void AnhLonTuServer_VanDocDuoc()
        {
            // Icon Top Pet thực tế khoảng 37 KB. Server chỉ áp trần 10 KB cho
            // chiều client gửi lên; dùng cùng trần đó cho chiều nhận làm client
            // tự ngắt kết nối ngay khi COMMAND_IMAGE tới.
            const int payloadLength = 37079;
            var frameLength = payloadLength + 1;
            using var ms = new MemoryStream();
            ms.WriteByte((byte)(frameLength >> 24));
            ms.WriteByte((byte)(frameLength >> 16));
            ms.WriteByte((byte)(frameLength >> 8));
            ms.WriteByte((byte)frameLength);
            ms.WriteByte(0); // server gửi plaintext
            var payload = new byte[payloadLength];
            payload[0] = unchecked((byte)GopetCmd.COMMAND_IMAGE);
            ms.Write(payload, 0, payload.Length);
            ms.Position = 0;

            Assert.True(PacketFramer.TryReadFrame(ms, out var read, out var encrypted));
            Assert.Equal(payloadLength, read.Length);
            Assert.Equal(unchecked((byte)GopetCmd.COMMAND_IMAGE), read[0]);
            Assert.False(encrypted);
        }

        [Fact]
        public void ThanGoiCutGiuaChung_NemEndOfStream()
        {
            using var ms = new MemoryStream();
            ms.Write(new byte[] { 0, 0, 0, 11 }, 0, 4); // hứa 10 byte thân
            ms.WriteByte(0);
            ms.Write(new byte[] { 1, 2, 3 }, 0, 3);     // chỉ đưa 3
            ms.Position = 0;

            Assert.Throws<EndOfStreamException>(() => PacketFramer.TryReadFrame(ms, out _, out _));
        }

        [Fact]
        public void Handshake_DungCauTruc9Byte()
        {
            var bytes = Handshake.Build(1740000000000L);

            Assert.Equal(9, bytes.Length);
            Assert.Equal(9, bytes[0]);

            // Vector sinh từ tools/gen-test-vectors.
            Assert.Equal("09000001952014f800", TestVectorData.ToHex(bytes));
        }

        [Fact]
        public void Handshake_ServerSuyNguocRaDungKhoa()
        {
            const long key = 1740000000000L;
            var bytes = Handshake.Build(key);

            // Đúng logic Session.readKey() phía server.
            long derived = 0;
            for (var i = 1; i <= 8; i++)
            {
                derived = (derived << 8) | bytes[i];
            }

            Assert.Equal(key, derived);
        }
    }
}
