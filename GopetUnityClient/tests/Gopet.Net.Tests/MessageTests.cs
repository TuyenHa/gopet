using System;
using System.IO;
using Xunit;

namespace Gopet.Net.Tests
{

    public sealed class MessageTests
    {
        [Fact]
        public void GoiGui_MacDinhLaMaHoa()
        {
            // Bất đối xứng đã kiểm chứng: client gửi lên thì mã hoá,
            // server gửi xuống thì không. Xem ghi chú đầu class Message.
            using var m = Message.Create(GopetCmd.LOGIN);

            Assert.True(m.IsEncrypted);
        }

        [Fact]
        public void ToWire_OpcodeDungViTriDau()
        {
            using var m = Message.Create(GopetCmd.CLIENT_INFO);
            m.PutSByte(1).PutInt(4);

            var wire = m.ToWire();

            Assert.Equal(unchecked((byte)GopetCmd.CLIENT_INFO), wire[0]);
            Assert.Equal(0xDC, wire[0]); // -36 dưới dạng byte
            Assert.Equal(6, wire.Length);
        }

        [Fact]
        public void GoiKhongThan_ChiCoOpcode()
        {
            using var m = Message.Create(GopetCmd.SERVER_LIST);

            Assert.Single(m.ToWire());
        }

        [Fact]
        public void FromWire_TachDungOpcodeVaThan()
        {
            var payload = new byte[] { 0xDC, 0x00, 0x00, 0x00, 0x2A };

            var m = Message.FromWire(payload, wasEncrypted: false);

            Assert.Equal((sbyte)-36, m.Id);
            Assert.Equal(42, m.Reader.ReadInt());
            Assert.Equal(0, m.Reader.Remaining);
        }

        [Fact]
        public void FromWire_PayloadRong_NemProtocolException()
        {
            Assert.Throws<ProtocolException>(() => Message.FromWire(Array.Empty<byte>(), false));
        }

        [Fact]
        public void GoiNhan_KhongGhiDuoc()
        {
            var m = Message.FromWire(new byte[] { 1 }, false);

            Assert.Throws<InvalidOperationException>(() => m.PutInt(1));
        }

        [Fact]
        public void GoiGui_KhongDocDuoc()
        {
            using var m = Message.Create(GopetCmd.LOGIN);

            Assert.Throws<InvalidOperationException>(() => m.Reader.ReadInt());
        }

        [Fact]
        public void RoundTrip_QuaTeaVaKhungGoi()
        {
            // Mô phỏng nguyên đường đi: dựng gói -> mã hoá -> giải mã -> đọc lại.
            var tea = new Tea(1740000000000L);

            using var outgoing = Message.Create(GopetCmd.LOGIN);
            outgoing.PutUtf("admin").PutUtf("matkhau123").PutUtf("1.4.3");

            var incoming = RoundTrip(outgoing, tea);

            Assert.Equal(GopetCmd.LOGIN, incoming.Id);
            Assert.Equal("admin", incoming.Reader.ReadUtf());
            Assert.Equal("matkhau123", incoming.Reader.ReadUtf());
            Assert.Equal("1.4.3", incoming.Reader.ReadUtf());
            incoming.Reader.ExpectFullyConsumed("LOGIN round-trip");
        }

        [Fact]
        public void RoundTrip_DuMoiKieuDuLieu()
        {
            // Tiêu chí P2: mọi kiểu phải qua được cả đường mã hoá lẫn đóng khung.
            // Trộn chung một gói vì lỗi hay nằm ở CHỖ NỐI giữa hai kiểu —
            // ghi thiếu/thừa một byte chỉ lộ ra khi có field đứng ngay sau.
            var tea = new Tea(1740000000000L);

            using var outgoing = Message.Create(GopetCmd.CLIENT_INFO);
            outgoing.PutSByte(-36)
                    .PutBool(true)
                    .PutShort(-12345)
                    .PutInt(int.MinValue)
                    .PutLong(-1234567890123L)
                    .PutUtf("Bảo trì cập nhật")
                    .PutBool(false)
                    .PutShort(short.MaxValue);

            var incoming = RoundTrip(outgoing, tea);

            Assert.Equal(GopetCmd.CLIENT_INFO, incoming.Id);
            Assert.Equal((sbyte)-36, incoming.Reader.ReadSByte());
            Assert.True(incoming.Reader.ReadBool());
            Assert.Equal((short)-12345, incoming.Reader.ReadShort());
            Assert.Equal(int.MinValue, incoming.Reader.ReadInt());
            Assert.Equal(-1234567890123L, incoming.Reader.ReadLong());
            Assert.Equal("Bảo trì cập nhật", incoming.Reader.ReadUtf());
            Assert.False(incoming.Reader.ReadBool());
            Assert.Equal(short.MaxValue, incoming.Reader.ReadShort());
            incoming.Reader.ExpectFullyConsumed("round-trip đủ kiểu");
        }

        /// <summary>
        /// Đi trọn đường một gói: mã hoá -> đóng khung -> đọc khung -> giải mã.
        ///
        /// Dùng <see cref="MemoryStream"/> nên không cần server, nhưng vẫn chạy qua
        /// đúng <see cref="PacketFramer"/> mà socket dùng — round-trip chỉ có Tea
        /// thì bỏ sót toàn bộ phần đóng khung, tức là nửa đường.
        /// </summary>
        private static Message RoundTrip(Message outgoing, Tea tea)
        {
            var stream = new MemoryStream();
            PacketFramer.WriteFrame(stream, tea.Encrypt(outgoing.ToWire()), outgoing.IsEncrypted);

            stream.Position = 0;
            Assert.True(PacketFramer.TryReadFrame(stream, out var raw, out var encrypted));
            Assert.True(encrypted);
            Assert.Equal(stream.Length, stream.Position); // không thừa byte nào

            return Message.FromWire(tea.Decrypt(raw), encrypted);
        }

    }
}
