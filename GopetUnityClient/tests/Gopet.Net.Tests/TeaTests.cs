using System;
using System.Collections.Generic;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class TeaTests
    {
        /// <summary>
        /// Test quan trọng nhất của cả basecode.
        ///
        /// Mã hoá bằng Tea.cs rồi so byte-với-byte với vector sinh từ bản port
        /// JS độc lập của TEA.cs. Khớp = client nói đúng thứ mà server nghe.
        /// Lệch = sai một phép dịch bit ở đâu đó, và không có test này thì
        /// triệu chứng sẽ là "server đóng kết nối, không rõ lý do".
        /// </summary>
        public static IEnumerable<object[]> TeaVectors()
        {
            foreach (var v in TestVectorData.Vectors.Tea)
            {
                yield return new object[] { v.Key, v.PayloadName, v.PlainHex, v.EncryptedHex };
            }
        }

        [Theory]
        [MemberData(nameof(TeaVectors))]
        public void Encrypt_KhopVoiBanPortDocLap(string key, string payloadName, string plainHex, string expectedHex)
        {
            // Khoá trong vector là chuỗi thập phân không dấu 64-bit
            // (generator ghi ra dạng uint64 để tránh dấu trừ trong JSON).
            var tea = new Tea(unchecked((long)ulong.Parse(key)));

            var actual = tea.Encrypt(TestVectorData.FromHex(plainHex));

            Assert.True(
                expectedHex == TestVectorData.ToHex(actual),
                $"Lệch với bản port độc lập — payload '{payloadName}', khoá {key}.\n" +
                $"  mong đợi: {expectedHex}\n" +
                $"  thực tế : {TestVectorData.ToHex(actual)}");
        }

        [Theory]
        [MemberData(nameof(TeaVectors))]
        public void Decrypt_HoanNguyenDuocVector(string key, string payloadName, string plainHex, string encryptedHex)
        {
            var tea = new Tea(unchecked((long)ulong.Parse(key)));

            var actual = tea.Decrypt(TestVectorData.FromHex(encryptedHex));

            Assert.True(actual != null, $"Decrypt trả null — payload '{payloadName}', khoá {key}.");
            Assert.True(
                plainHex == TestVectorData.ToHex(actual),
                $"Giải mã sai — payload '{payloadName}', khoá {key}.\n" +
                $"  mong đợi: {plainHex}\n" +
                $"  thực tế : {TestVectorData.ToHex(actual)}");
        }

        [Fact]
        public void MangRong_BiTuChoiVoiLoiRoRang()
        {
            var tea = new Tea(1740000000000L);

            // TEA của server ném IndexOutOfRangeException với đầu vào rỗng.
            // Client phải chặn sớm và nói rõ lý do.
            var ex = Assert.Throws<ArgumentException>(() => tea.Encrypt(Array.Empty<byte>()));
            Assert.Contains("rỗng", ex.Message);
        }

        [Fact]
        public void Decrypt_DoDaiKhongHopLe_TraVeNull()
        {
            var tea = new Tea(1740000000000L);

            // Server yêu cầu length % 4 == 0 và (length >> 2) chẵn-lẻ == 1.
            Assert.Null(tea.Decrypt(new byte[3]));   // không chia hết cho 4
            Assert.Null(tea.Decrypt(new byte[8]));   // (8>>2)%2 == 0
        }

        [Fact]
        public void Decrypt_KhongNemVoiRacNgauNhien()
        {
            var tea = new Tea(1740000000000L);
            var rng = new Random(12345);

            // Gói hỏng/độc hại không được làm crash luồng đọc. Trả null là đủ.
            for (var i = 0; i < 500; i++)
            {
                var len = (rng.Next(1, 20) * 2 - 1) * 4; // đảm bảo (len>>2) lẻ
                var junk = new byte[len];
                rng.NextBytes(junk);

                var ex = Record.Exception(() => tea.Decrypt(junk));
                Assert.Null(ex);
            }
        }

        [Fact]
        public void GenerateKey_LapTamByteHaiLan()
        {
            var key = new byte[16];
            Tea.GenerateKey(0x0102030405060708L, key);

            var expected = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8,
                1, 2, 3, 4, 5, 6, 7, 8
            };

            Assert.Equal(expected, key);
        }
    }
}
