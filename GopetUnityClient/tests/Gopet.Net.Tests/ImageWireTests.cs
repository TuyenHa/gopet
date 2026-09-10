using Gopet.Net.Images;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Gói ảnh, đối chiếu byte thật client J2ME gửi (dump 2026-09-05).
    /// </summary>
    public sealed class ImageWireTests
    {
        /// <summary>Request thật: gameType 0, type 2, "npcs/Su gia bang hoi.png".</summary>
        private const string J2meRequestHex =
            "60000200186e7063732f5375206769612062616e6720686f692e706e67";

        /// <summary>8 byte đầu của mọi file PNG.</summary>
        private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

        [Fact]
        public void Request_KhopTungByteVoiClientJ2me()
        {
            using var m = ImagePackets.Request("npcs/Su gia bang hoi.png", ImagePackets.TypeNpc);

            Assert.Equal(J2meRequestHex, TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void Response_DocDungCacField()
        {
            using var built = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(ImagePackets.GameType)
                .PutSByte(ImagePackets.TypeNpc)
                .PutUtf("npcs/Su gia bang hoi.png")
                .PutInt(PngMagic.Length)
                .PutBytes(PngMagic);

            using var incoming = Message.FromWire(built.ToWire(), false);
            var response = ImageResponse.Parse(incoming);

            Assert.Equal(0, response.GameType);
            Assert.Equal(2, response.Type);
            Assert.Equal("npcs/Su gia bang hoi.png", response.Path);
            Assert.Equal(PngMagic, response.Png);
        }

        [Fact]
        public void Response_DoDaiVoLy_NemProtocolException()
        {
            using var built = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(0).PutSByte(2).PutUtf("x").PutInt(int.MaxValue);

            using var incoming = Message.FromWire(built.ToWire(), false);

            Assert.Throws<ProtocolException>(() => ImageResponse.Parse(incoming));
        }

        [Fact]
        public void Response_ThuaByte_NemProtocolException()
        {
            // ExpectFullyConsumed bắt parser đọc thiếu — desync rẻ nhất là bắt tại đây.
            using var built = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(0).PutSByte(2).PutUtf("x").PutInt(1).PutSByte(9).PutSByte(9);

            using var incoming = Message.FromWire(built.ToWire(), false);

            Assert.Throws<ProtocolException>(() => ImageResponse.Parse(incoming));
        }

        [Theory]
        [InlineData("img/captcha.png", true)]
        [InlineData("img/captcha.png1234567890", true)]   // server nối số ngẫu nhiên
        [InlineData("npcs/arena.png", false)]
        [InlineData(null, false)]
        public void NhanDienCaptcha(string path, bool expected)
        {
            Assert.Equal(expected, ImagePackets.IsCaptcha(path));
        }
    }
}
