using System;
using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// <c>SHOW_MENU_ITEM</c> — gói phức tạp nhất của vertical slice, và là định dạng
    /// dùng chung cho toàn bộ 162 màn hình. Đọc lệch một field là hỏng mọi menu.
    /// </summary>
    public sealed class MenuScreenTests
    {
        /// <summary>
        /// Dựng gói y HỆT <c>GameController.showMenuItem()</c>. Viết tay theo code
        /// server chứ không gọi lại parser — nếu dùng chung một hàm thì test chỉ
        /// chứng minh parser nhất quán với chính nó.
        /// </summary>
        private sealed class ServerMenuBuilder
        {
            private readonly Message _m;

            public ServerMenuBuilder(int listId, sbyte type, string title, int count)
            {
                _m = Message.Create(GopetCmd.COMMAND_GUIDER)
                    .PutSByte(GopetCmd.SHOW_MENU_ITEM)
                    .PutInt(listId)
                    .PutSByte(type)
                    .PutUtf(title)
                    .PutInt(count);
            }

            public ServerMenuBuilder Item(int itemId, string img, string title, string desc,
                                          bool canSelect, bool showDialog,
                                          string dialogText = null, string left = null, string right = null,
                                          sbyte saleStatus = 0, bool closeAfterClick = false,
                                          (int id, string money, sbyte enabled)[] payments = null)
            {
                _m.PutInt(itemId).PutUtf(img).PutUtf(title).PutUtf(desc)
                  .PutSByte(canSelect ? 1 : 0)
                  .PutBool(showDialog);

                if (showDialog)
                {
                    _m.PutUtf(dialogText).PutUtf(left).PutUtf(right);
                }

                _m.PutSByte(saleStatus).PutBool(closeAfterClick);

                payments ??= Array.Empty<(int, string, sbyte)>();
                _m.PutInt(payments.Length);
                foreach (var p in payments)
                {
                    _m.PutInt(p.id).PutUtf(p.money).PutSByte(p.enabled);
                }

                return this;
            }

            /// <summary>Bọc lại thành gói nhận được, đã bóc bao ngoài COMMAND_GUIDER + sub.</summary>
            public Message Build()
            {
                using (_m)
                {
                    var wire = _m.ToWire();

                    // Bỏ opcode và sub-command: đó là việc của MessageRouter.
                    var body = new byte[wire.Length - 2];
                    Array.Copy(wire, 2, body, 0, body.Length);

                    var envelope = new byte[body.Length + 1];
                    envelope[0] = unchecked((byte)GopetCmd.SHOW_MENU_ITEM);
                    Array.Copy(body, 0, envelope, 1, body.Length);

                    return Message.FromWire(envelope, false);
                }
            }
        }

        [Fact]
        public void MenuTron_CoVaKhongCoShowDialog_KhongLechStream()
        {
            // Đây là ca dễ hỏng nhất: showDialog quyết định có đọc 3 chuỗi tiếp theo
            // hay không. Đọc sai thì dòng SAU mới sai, nên triệu chứng hiện ra ở chỗ
            // hoàn toàn khác — trộn lẫn trong một gói là cách bắt nó rẻ nhất.
            var message = new ServerMenuBuilder(1234, 7, "Cửa hàng", 3)
                .Item(10, "items/1.png", "Kiếm gỗ", "Sát thương 5", true, false)
                .Item(11, "items/2.png", "Khiên", "Giáp 3", true, true,
                      "Mua khiên?", "Đồng ý", "Thôi")
                .Item(12, "items/3.png", "Giáp", "Giáp 9", false, false)
                .Build();

            var screen = MenuScreen.Parse(message);

            Assert.Equal(1234, screen.ListId);
            Assert.Equal(7, screen.Type);
            Assert.Equal("Cửa hàng", screen.Title);
            Assert.Equal(3, screen.Items.Length);

            Assert.Equal("Kiếm gỗ", screen.Items[0].Title);
            Assert.False(screen.Items[0].ShowDialog);
            Assert.Null(screen.Items[0].DialogText);

            Assert.Equal("Khiên", screen.Items[1].Title);
            Assert.True(screen.Items[1].ShowDialog);
            Assert.Equal("Mua khiên?", screen.Items[1].DialogText);
            Assert.Equal("Đồng ý", screen.Items[1].LeftCommandText);
            Assert.Equal("Thôi", screen.Items[1].RightCommandText);

            // Dòng thứ ba là bằng chứng stream không lệch sau dòng có dialog.
            Assert.Equal("Giáp", screen.Items[2].Title);
            Assert.False(screen.Items[2].CanSelect);
        }

        [Fact]
        public void DocDuocLuaChonThanhToan()
        {
            var message = new ServerMenuBuilder(5, 0, "Nạp", 1)
                .Item(1, "img.png", "Gói 1", "mô tả", true, false,
                      payments: new[] { (100, "10.000đ", (sbyte)1), (200, "20.000đ", (sbyte)0) })
                .Build();

            var screen = MenuScreen.Parse(message);
            var options = screen.Items[0].PaymentOptions;

            Assert.Equal(2, options.Length);
            Assert.Equal(100, options[0].Id);
            Assert.Equal("10.000đ", options[0].MoneyText);
            Assert.Equal(1, options[0].IsEnabled);
            Assert.Equal(0, options[1].IsEnabled);
        }

        [Fact]
        public void MenuRong_VanDocDuoc()
        {
            var screen = MenuScreen.Parse(new ServerMenuBuilder(9, 1, "Trống", 0).Build());

            Assert.Empty(screen.Items);
            Assert.Equal("Trống", screen.Title);
        }

        [Fact]
        public void SoDongVoLy_NemProtocolException()
        {
            using var built = Message.Create(GopetCmd.SHOW_MENU_ITEM)
                .PutInt(1).PutSByte(0).PutUtf("x").PutInt(int.MaxValue);
            using var incoming = Message.FromWire(built.ToWire(), false);

            Assert.Throws<ProtocolException>(() => MenuScreen.Parse(incoming));
        }

        [Fact]
        public void ThieuByte_NemChuKhongDocBua()
        {
            // Khai 2 dòng nhưng chỉ gửi 1: phải nổ, không được trả về danh sách nửa vời.
            var message = new ServerMenuBuilder(1, 0, "Hụt", 2)
                .Item(1, "a.png", "A", "d", true, false)
                .Build();

            Assert.Throws<ProtocolException>(() => MenuScreen.Parse(message));
        }

        [Fact]
        public void ThuaByte_NemProtocolException()
        {
            // ExpectFullyConsumed: đọc thiếu cũng phải bị bắt, không chỉ đọc thừa.
            using var built = Message.Create(GopetCmd.SHOW_MENU_ITEM)
                .PutInt(1).PutSByte(0).PutUtf("x").PutInt(0).PutSByte(99);
            using var incoming = Message.FromWire(built.ToWire(), false);

            Assert.Throws<ProtocolException>(() => MenuScreen.Parse(incoming));
        }
    }
}
