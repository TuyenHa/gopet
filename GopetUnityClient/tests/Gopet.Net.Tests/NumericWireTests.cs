using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Số nguyên và bool trên dây: big-endian, dấu, và tràn.
    ///
    /// Round-trip một mình không đủ — ghi rồi đọc bằng cùng một quy ước sai vẫn
    /// khớp. Nên mỗi kiểu có thêm một phép so byte tuyệt đối.
    /// </summary>
    public sealed class NumericWireTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(19180)]
        public void Int_RoundTrip(int value)
        {
            using var w = new JavaBinaryWriter();
            w.WriteInt(value);

            Assert.Equal(value, new JavaBinaryReader(w.ToArray()).ReadInt());
        }

        [Fact]
        public void Int_LaBigEndian()
        {
            using var w = new JavaBinaryWriter();
            w.WriteInt(0x01020304);

            // Big-endian: byte cao ra trước. BinaryWriter của .NET sẽ ra 04030201.
            Assert.Equal("01020304", TestVectorData.ToHex(w.ToArray()));
        }

        [Fact]
        public void Long_LaBigEndian()
        {
            using var w = new JavaBinaryWriter();
            w.WriteLong(0x0102030405060708L);

            Assert.Equal("0102030405060708", TestVectorData.ToHex(w.ToArray()));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(short.MaxValue)]
        [InlineData(short.MinValue)]
        public void Short_RoundTrip(short value)
        {
            using var w = new JavaBinaryWriter();
            w.WriteShort(value);

            Assert.Equal(value, new JavaBinaryReader(w.ToArray()).ReadShort());
        }

        [Fact]
        public void Short_LaBigEndian()
        {
            using var w = new JavaBinaryWriter();
            w.WriteShort(0x0102);

            Assert.Equal("0102", TestVectorData.ToHex(w.ToArray()));
        }

        [Fact]
        public void Short_CatDungPhanThapKhiTran()
        {
            // Server ép (short) rồi mới ghi, nên tràn phải cắt chứ không nổ.
            using var w = new JavaBinaryWriter();
            w.WriteShort(0x1FFFF);

            Assert.Equal("ffff", TestVectorData.ToHex(w.ToArray()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Bool_RoundTrip(bool value)
        {
            using var w = new JavaBinaryWriter();
            w.WriteBool(value);

            Assert.Equal(value, new JavaBinaryReader(w.ToArray()).ReadBool());
        }

        [Fact]
        public void Bool_GhiDungMotByte01()
        {
            using var w = new JavaBinaryWriter();
            w.WriteBool(true);
            w.WriteBool(false);

            Assert.Equal("0100", TestVectorData.ToHex(w.ToArray()));
        }

        [Fact]
        public void Bool_MoiGiaTriKhac0DeuLaTrue()
        {
            // Server ghi 0/1, nhưng đọc phải chịu được mọi byte khác 0 —
            // readBoolean của Java là "!= 0", không phải "== 1".
            Assert.True(new JavaBinaryReader(new byte[] { 0x7f }).ReadBool());
        }

        [Theory]
        [InlineData(-36)]  // CLIENT_INFO — opcode âm, chỗ dễ sai nhất
        [InlineData(-1)]   // SKILL_CLAN_LOCK
        [InlineData(0)]
        [InlineData(122)]  // COMMAND_GUIDER
        [InlineData(81)]   // PET_SERVICE
        public void SByte_GiuDuocGiaTriAm(int opcode)
        {
            using var w = new JavaBinaryWriter();
            w.WriteSByte(opcode);

            Assert.Equal((sbyte)opcode, new JavaBinaryReader(w.ToArray()).ReadSByte());
        }

        [Fact]
        public void HandshakeVector_SuyRaDungKhoa()
        {
            // Server đọc 9 byte rồi ghép byte [1..8] thành khoá (Session.readKey).
            // Client phải sinh ra 9 byte mà server suy ngược lại đúng khoá gốc.
            foreach (var v in TestVectorData.Vectors.KeyDerivation)
            {
                var bytes = TestVectorData.FromHex(v.HandshakeHex);

                Assert.Equal(9, bytes.Length);
                Assert.Equal(9, bytes[0]);

                ulong derived = 0;
                for (var i = 1; i <= 8; i++)
                {
                    derived = (derived << 8) | bytes[i];
                }

                Assert.Equal(v.DerivedKey, derived.ToString());
            }
        }

        [Fact]
        public void DocQuaCuoiGoi_NemProtocolException()
        {
            var r = new JavaBinaryReader(new byte[] { 1, 2 });

            Assert.Throws<ProtocolException>(() => r.ReadInt());
        }

        [Fact]
        public void ReadIntArray_ChanDoDaiVuotNguong()
        {
            // Đây chính là lỗ hổng GameController.cs:215 phía server.
            // Client không được lặp lại.
            using var w = new JavaBinaryWriter();
            w.WriteInt(int.MaxValue);

            var r = new JavaBinaryReader(w.ToArray());
            Assert.Throws<ProtocolException>(() => r.ReadIntArray(64));
        }

        [Fact]
        public void ReadIntArray_ChanDoDaiAm()
        {
            using var w = new JavaBinaryWriter();
            w.WriteInt(-5);

            var r = new JavaBinaryReader(w.ToArray());
            Assert.Throws<ProtocolException>(() => r.ReadIntArray(64));
        }

        [Fact]
        public void ExpectFullyConsumed_BatDuocParserLech()
        {
            using var w = new JavaBinaryWriter();
            w.WriteInt(1);
            w.WriteInt(2);

            var r = new JavaBinaryReader(w.ToArray());
            r.ReadInt(); // cố tình đọc thiếu một int

            Assert.Throws<ProtocolException>(() => r.ExpectFullyConsumed("test"));
        }
    }
}
