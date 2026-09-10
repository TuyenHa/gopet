using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Giá trị client gửi ngược lên khi chọn một dòng.
    ///
    /// <para><b>Vì sao cần cả một file riêng:</b> mọi vector test trước đó đều có
    /// <c>itemId == vị trí dòng</c> (menu tự dựng đặt id = 0 hoặc = i; màn ATM thật
    /// thì id trùng vị trí). Với dữ liệu như vậy, gửi vị trí và gửi id cho ra kết
    /// quả giống hệt nhau — nên bug gửi nhầm vị trí lọt qua toàn bộ test lẫn kiểm
    /// chứng live.</para>
    ///
    /// <para>Mọi test ở đây cố ý đặt <b>id khác vị trí</b>.</para>
    /// </summary>
    public sealed class GuiderEchoTests
    {
        private readonly List<Message> _sent = new List<Message>();

        private GuiderHandler NewHandler() => new GuiderHandler(_sent.Add);

        private static int SelectedValue(Message m)
        {
            // 7a 03 <listId:4> <echo:4>
            var wire = m.ToWire();
            return (wire[6] << 24) | (wire[7] << 16) | (wire[8] << 8) | wire[9];
        }

        private static MenuItemInfo Item(int itemId)
        {
            return new MenuItemInfo
            {
                ItemId = itemId,
                ImagePath = "i.png",
                Title = "t",
                Description = "d",
                CanSelect = true,
                PaymentOptions = Array.Empty<MenuItemInfo.PaymentOption>()
            };
        }

        [Fact]
        public void ChonDongMenu_GuiItemIdChuKhongPhaiViTri()
        {
            // Server dùng giá trị này như ID thật: clan.getJoinRequestByUserId(index)
            // (selectMenu.cs:1475). Gửi vị trí là tra nhầm người.
            var handler = NewHandler();
            var screen = new MenuScreen
            {
                ListId = 500,
                Items = new[] { Item(1001), Item(1002), Item(1003) }
            };

            handler.Select(screen, 2);

            Assert.Equal(1003, SelectedValue(_sent[0]));
        }

        [Fact]
        public void ItemIdAm_VanGuiDung()
        {
            // MENU_PET_INVENTORY dùng -1 cho pet đang đeo (selectMenu.cs:423).
            // Vị trí dòng không bao giờ âm được, nên nếu gửi vị trí thì nhánh này
            // không đời nào chạy.
            var handler = NewHandler();
            var screen = new MenuScreen { ListId = 1051, Items = new[] { Item(-1), Item(0), Item(1) } };

            handler.Select(screen, 0);

            Assert.Equal(-1, SelectedValue(_sent[0]));
        }

        [Fact]
        public void DongKhongCoIdRieng_ItemIdDaBangViTri()
        {
            // Server ghi putInt(i) khi dòng không có id (GameController.cs:843),
            // nên gửi ItemId vẫn đúng — không cần nhánh riêng.
            var handler = NewHandler();
            var screen = new MenuScreen { ListId = 1, Items = new[] { Item(0), Item(1), Item(2) } };

            handler.Select(screen, 1);

            Assert.Equal(1, SelectedValue(_sent[0]));
        }

        [Fact]
        public void ChonKemThanhToan_CungGuiItemId()
        {
            var handler = NewHandler();
            var screen = new MenuScreen { ListId = 7, Items = new[] { Item(900), Item(901) } };

            handler.Select(screen, 1, paymentIndex: 0);

            // 7a 09 <listId:4> <mode:1> <echo:4> <paymentIndex:4>
            var wire = _sent[0].ToWire();
            var echo = (wire[7] << 24) | (wire[8] << 16) | (wire[9] << 8) | wire[10];
            Assert.Equal(901, echo);
        }

        [Fact]
        public void ChonListOption_GuiOptionIdChuKhongPhaiViTri()
        {
            // Menu lời mời kết bạn có id theo thứ tự 0, 1, 3, 2 (sendMenu.cs:245-249)
            // và handler switch trên chính id đó (selectMenu.cs:2064). Gửi vị trí thì
            // "Từ chối tất cả" chạy nhầm thành "Từ chối và chặn".
            var handler = NewHandler();
            var screen = new ListOptionScreen
            {
                ListId = 200,
                Title = "Lời mời",
                CommandText = "OK",
                Options = new[]
                {
                    new ListOptionScreen.Option { Id = 0, Text = "Đồng ý" },
                    new ListOptionScreen.Option { Id = 1, Text = "Từ chối" },
                    new ListOptionScreen.Option { Id = 3, Text = "Từ chối tất cả" },
                    new ListOptionScreen.Option { Id = 2, Text = "Từ chối và chặn" }
                }
            };

            handler.Select(screen, 2);   // "Từ chối tất cả", nằm ở vị trí 2, id 3

            Assert.Equal(3, SelectedValue(_sent[0]));
        }

        [Fact]
        public void ChonDongCuoiListOption_KhongLanSangHanhDongKhac()
        {
            var handler = NewHandler();
            var screen = new ListOptionScreen
            {
                ListId = 200,
                Options = new[]
                {
                    new ListOptionScreen.Option { Id = 0, Text = "a" },
                    new ListOptionScreen.Option { Id = 3, Text = "b" },
                    new ListOptionScreen.Option { Id = 2, Text = "c" }
                }
            };

            handler.Select(screen, 1);

            Assert.Equal(3, SelectedValue(_sent[0]));
        }

        [Fact]
        public void DongKhongChoChon_VanBiChan()
        {
            var handler = NewHandler();
            var item = Item(1001);
            item.CanSelect = false;

            var screen = new MenuScreen { ListId = 1, Items = new[] { item } };

            Assert.Throws<InvalidOperationException>(() => handler.Select(screen, 0));
            Assert.Empty(_sent);
        }
    }
}
