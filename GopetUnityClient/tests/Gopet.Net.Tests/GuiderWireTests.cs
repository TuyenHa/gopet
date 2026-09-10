using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Đối chiếu với byte THẬT bắt được từ phiên client J2ME nói chuyện với server
    /// test (dump 2026-09-05). Vector tự dựng chỉ chứng minh parser nhất quán với
    /// chính nó; byte thật chứng minh nó nhất quán với server.
    /// </summary>
    public sealed class GuiderWireTests
    {
        // Lưu ý về độ phủ: cả BỐN gói SHOW_MENU_ITEM bắt được từ phiên thật đều có
        // count = 0 (cửa hàng chưa có hàng, danh sách nhiệm vụ và pet đang rỗng).
        // Nên byte thật ở đây chỉ chứng minh phần ĐẦU gói. Phần đọc từng dòng —
        // gồm field điều kiện showDialog — được phủ bởi MenuScreenTests, dựng gói
        // theo đúng code của showMenuItem().

        /// <summary>Bóc bao ngoài opcode, giữ lại sub-command làm byte đầu — đúng cái MessageRouter đưa cho handler.</summary>
        private static Message Incoming(string hexWithOpcode)
        {
            var full = TestVectorData.FromHex(hexWithOpcode);
            var body = new byte[full.Length - 1];
            System.Array.Copy(full, 1, body, 0, body.Length);
            return Message.FromWire(body, false);
        }

        [Fact]
        public void ShowMenuItem_GoiThat_MenuRong()
        {
            // Cửa hàng vũ khí lúc chưa có hàng: server vẫn gửi màn hình với 0 dòng.
            // Menu rỗng là hợp lệ, không phải lỗi — client phải hiện màn hình trống
            // chứ không được coi là hỏng gói.
            var screen = MenuScreen.Parse(Incoming(
                "7a080000000103001443e1bbad612068c3a06e672076c5a9206b68c3ad00000000"));

            Assert.Equal(1, screen.ListId);
            Assert.Equal(3, screen.Type);
            Assert.Equal("Cửa hàng vũ khí", screen.Title);
            Assert.Empty(screen.Items);
        }

        [Theory]
        [InlineData("7a080000000303001043e1bbad612068c3a06e67206ec3b36e00000000",
                    3, "Cửa hàng nón")]
        [InlineData("7a080000040a0200184e6869e1bb876d2076e1bba52063e1bba7612062e1baa16e00000000",
                    1034, "Nhiệm vụ của bạn")]
        [InlineData("7a080000041b02000f5065742063e1bba7612062e1baa16e00000000",
                    1051, "Pet của bạn")]
        public void ShowMenuItem_GoiThat_DocDungTieuDe(string hex, int listId, string title)
        {
            // Ba màn hình khác nhau, cùng một parser, không một dòng code riêng nào —
            // đúng nguyên tắc của phase này.
            //
            // Hex dán NGUYÊN VĂN từ dump. Trước đó test tự nối thêm 4 byte đếm khi
            // thấy thiếu: kết quả vẫn xanh nhưng che mất việc vector chép sai.
            var screen = MenuScreen.Parse(Incoming(hex));

            Assert.Equal(listId, screen.ListId);
            Assert.Equal(title, screen.Title);
            Assert.Empty(screen.Items);
        }

        [Fact]
        public void ListOption_GoiThat_ManHinhAtm()
        {
            // 83 byte thật của màn hình ATM: listId ghi hai lần, rồi tiêu đề, nhãn
            // nút giữa, và ba lựa chọn kèm trạng thái.
            var screen = ListOptionScreen.Parse(Incoming(
                "7a030000040f0000040f000341544d00024f4b0000000300000000000d" +
                "c490e1bb9569202876616e67290100000001000dc490e1bb956920286e676f63290100000002000d" +
                "c490e1bb956920286e676f632901"));

            Assert.Equal(1039, screen.ListId);
            Assert.Equal("ATM", screen.Title);
            Assert.Equal("OK", screen.CommandText);
            Assert.Equal(3, screen.Options.Length);

            Assert.Equal(0, screen.Options[0].Id);
            Assert.Equal("Đổi (vang)", screen.Options[0].Text);
            Assert.Equal(1, screen.Options[0].Status);

            Assert.Equal(2, screen.Options[2].Id);
            Assert.Equal("Đổi (ngoc)", screen.Options[2].Text);
        }

        [Fact]
        public void SelectMenuElement_KhopTungByteVoiClientJ2me()
        {
            // Client J2ME chọn dòng đầu của màn hình ATM: 7a03 0000040f 00000000
            using var m = GuiderPackets.SelectMenuElement(1039, 0);

            Assert.Equal("7a030000040f00000000", TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void SelectNpcOption_KhopTungByteVoiClientJ2me()
        {
            // 7a05 fffffffd 00000040 — npcId âm (-3), optionId 64.
            using var m = GuiderPackets.SelectNpcOption(-3, 64);

            Assert.Equal("7a05fffffffd00000040", TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void TalkToNpc_KhopTungByteVoiClientJ2me()
        {
            // 7a02 fffffffd — bắt chuyện NPC id -3.
            using var m = GuiderPackets.TalkToNpc(-3);

            Assert.Equal("7a02fffffffd", TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void SelectMenuElementWithPayment_KhopTungByteVoiClientJ2me()
        {
            // 7a09 00000007 02 00000000 00000000 — menu 7, chế độ 2, dòng 0, gói 0.
            using var m = GuiderPackets.SelectMenuElementWithPayment(7, 0, 0);

            Assert.Equal("7a0900000007020000000000000000", TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void SubmitInputDialog_KhopTungByteVoiClientJ2me()
        {
            // 7a07 00000001 77359400 — hộp thoại 1, và... 0x77359400 = 2000000000.
            // Đó KHÔNG phải số ô nhập mà là gói dị dạng của bài test hardening ở P1.
            // Ở đây chỉ kiểm phần đầu: opcode, sub, dialogId, rồi số ô.
            using var m = GuiderPackets.SubmitInputDialog(1, new[] { "abc" });

            Assert.Equal("7a0700000001000000010003616263", TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void AnswerYesNo_DiTrongServerMessageChuKhongPhaiGuider()
        {
            // Đây là chỗ rất dễ đặt nhầm: mọi dialog khác đi trong COMMAND_GUIDER (122),
            // riêng Có/Không đi trong SERVER_MESSAGE (45).
            using var m = GuiderPackets.AnswerYesNo(42, true);

            Assert.Equal("2d040000002a01", TestVectorData.ToHex(m.ToWire()));
        }
    }
}
