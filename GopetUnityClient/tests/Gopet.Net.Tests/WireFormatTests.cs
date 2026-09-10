using System;
using System.Collections.Generic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Đọc/ghi big-endian, khung UTF, và suy khoá từ handshake.
    /// </summary>
    public sealed class WireFormatTests
    {
        public static IEnumerable<object[]> UtfVectors()
        {
            foreach (var v in TestVectorData.Vectors.Utf)
            {
                yield return new object[] { v.Value, v.EncodedHex };
            }
        }

        [Theory]
        [MemberData(nameof(UtfVectors))]
        public void WriteUtf_KhopVoiVector(string value, string expectedHex)
        {
            using var w = new JavaBinaryWriter();
            w.WriteUtf(value);

            Assert.Equal(expectedHex, TestVectorData.ToHex(w.ToArray()));
        }

        [Theory]
        [MemberData(nameof(UtfVectors))]
        public void ReadUtf_DocLaiDuocChinhNo(string value, string encodedHex)
        {
            var r = new JavaBinaryReader(TestVectorData.FromHex(encodedHex));

            Assert.Equal(value, r.ReadUtf());
            Assert.Equal(0, r.Remaining);
        }

        [Fact]
        public void TiengVietCoDau_KhongLech()
        {
            // Chuỗi thật lấy từ config/server.json của GServer.
            const string text = "Bảo trì cập nhật chỉ số boss";

            using var w = new JavaBinaryWriter();
            w.WriteUtf(text);

            var r = new JavaBinaryReader(w.ToArray());
            Assert.Equal(text, r.ReadUtf());
        }
    }
}
