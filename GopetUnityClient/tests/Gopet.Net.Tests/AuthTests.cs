using System;
using Gopet.Net.Auth;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Kiểm tầng đăng nhập bằng ĐÚNG byte bắt được từ client J2ME gốc nói chuyện
    /// với server test (phiên 2026-09-05, xem phase-03).
    ///
    /// Vector tự bịa chỉ chứng minh code nhất quán với chính nó. Byte thật chứng
    /// minh nó nhất quán với client mà ta phải đạt parity.
    /// </summary>
    public sealed class AuthWireParityTests
    {
        /// <summary>Gói CLIENT_INFO client J2ME gửi lên, 109 byte.</summary>
        private const string J2meClientInfoHex =
            "dc00000000040005312e342e33004b467265654a324d452d506c75732c20612043" +
            "726f73732d506c6174666f726d204a324d4520456d756c61746f722e3b434c4443" +
            "2d312e313b4d4944502d322e303b6e756c6c3b676f70657400000140000000f000" +
            "0276690005322e342e39";

        /// <summary>Gói LOGIN client J2ME gửi lên, 77 byte: 4 UTF + 8 byte 0.</summary>
        private const string J2meLoginHex =
            "010009676f706574746573740008616263313233343500267265662d6d63623232" +
            "717265313462722d3134343730363334353931323037313133363036340005312e" +
            "342e330000000000000000";

        [Fact]
        public void ClientInfo_KhopTungByteVoiClientJ2me()
        {
            // Đặt đúng giá trị client cũ gửi. Khớp được nghĩa là thứ tự field,
            // kiểu số và writeUTF đều đúng — cả 8 field một lúc.
            var info = new ClientInfo
            {
                ClientType = 0,
                Provider = 4,
                Version = "1.4.3",
                Info = "FreeJ2ME-Plus, a Cross-Platform J2ME Emulator.;CLDC-1.1;MIDP-2.0;null;gopet",
                DisplayWidth = 320,
                DisplayHeight = 240,
                LanguageCode = "vi",
                TrailingField = "2.4.9"
            };

            using var message = info.ToMessage();

            Assert.Equal(J2meClientInfoHex, TestVectorData.ToHex(message.ToWire()));
        }

        [Fact]
        public void Login_KhopTungByteVoiClientJ2me()
        {
            using var message = AuthPackets.Login(
                "gopettest", "abc12345", "ref-mcb22qre14br-144706345912071136064", "1.4.3");

            Assert.Equal(J2meLoginHex, TestVectorData.ToHex(message.ToWire()));
        }

        [Fact]
        public void Login_CoDuBonUtfVaTamByteDem()
        {
            // Bảng opcode trong plan từng ghi LOGIN là 3 UTF. Gửi thiếu thì server
            // VẪN cho đăng nhập, nên chỉ diff dump mới phát hiện được — test này
            // giữ cho sai sót đó không quay lại.
            //
            // So độ dài với bản 3-UTF là chưa đủ: bỏ 8 byte đệm vẫn "dài hơn".
            // Nên kiểm thẳng: 4 chuỗi đọc lại đúng, rồi đúng 8 byte 0 ở cuối.
            using var m = AuthPackets.Login("gopettest", "abc12345", "ref", "1.4.3");

            var body = m.ToWire();
            var reader = new JavaBinaryReader(body[1..]);

            Assert.Equal("gopettest", reader.ReadUtf());
            Assert.Equal("abc12345", reader.ReadUtf());
            Assert.Equal("ref", reader.ReadUtf());
            Assert.Equal("1.4.3", reader.ReadUtf());

            Assert.Equal(8, reader.Remaining);
            for (var i = 0; i < 8; i++)
            {
                Assert.Equal(0, reader.ReadSByte());
            }
        }
    }

    public sealed class ServerListTests
    {
        /// <summary>Thân gói SERVER_LIST thật: 1 máy chủ "Localhost".</summary>
        private const string RealServerListHex =
            "400000000100094c6f63616c686f737400093132372e302e302e31" +
            "00004aec00004aec00004aec0101";

        [Fact]
        public void Parse_GoiThat()
        {
            using var m = global::Gopet.Net.Message.FromWire(TestVectorData.FromHex(RealServerListHex), false);
            var servers = ServerList.Parse(m);

            var only = Assert.Single(servers);
            Assert.Equal("Localhost", only.Name);
            Assert.Equal("127.0.0.1", only.Address);
            Assert.Equal(19180, only.Port);
            Assert.True(only.FlagA);
            Assert.True(only.FlagB);
        }

        [Fact]
        public void Parse_SoLuongVoLy_NemProtocolException()
        {
            using var m = global::Gopet.Net.Message.Create(GopetCmd.SERVER_LIST).PutInt(int.MaxValue);
            using var incoming = global::Gopet.Net.Message.FromWire(m.ToWire(), false);

            Assert.Throws<global::Gopet.Net.ProtocolException>(() => ServerList.Parse(incoming));
        }
    }

    public sealed class LoginSuccessTests
    {
        /// <summary>LOGIN_SUCCES thật của tài khoản gopettest (user_id 1458).</summary>
        private const string RealLoginSuccessHex =
            "03000005b200066b7a6864397800066b7a68643978001b666538303a3a62366664" +
            "3a393465663a393136343a63313535253400004aec";

        [Fact]
        public void Parse_GoiThat()
        {
            using var m = global::Gopet.Net.Message.FromWire(TestVectorData.FromHex(RealLoginSuccessHex), false);
            var result = LoginSuccess.Parse(m);

            Assert.Equal(1458, result.UserId);
            Assert.Equal("kzhd9x", result.Name);
            Assert.Equal("fe80::b6fd:94ef:9164:c155%4", result.ServerAddress);
            Assert.Equal(19180, result.ServerPort);
        }

        [Fact]
        public void Parse_HaiTenKhacNhau_NemProtocolException()
        {
            // Đọc lệch một field là hai tên khác nhau. Nổ ngay còn hơn để nó
            // biểu hiện thành thứ khó hiểu ở màn hình sau đó.
            using var built = global::Gopet.Net.Message.Create(GopetCmd.LOGIN_SUCCES)
                .PutInt(1).PutUtf("a").PutUtf("b").PutUtf("127.0.0.1").PutInt(19180);
            using var incoming = global::Gopet.Net.Message.FromWire(built.ToWire(), false);

            Assert.Throws<global::Gopet.Net.ProtocolException>(() => LoginSuccess.Parse(incoming));
        }
    }
}
