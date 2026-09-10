using System;
using Gopet.Net.Guider;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Quyết định khi chọn một dòng menu. Đây là logic quyết định 162 màn hình chạy
    /// đúng hay sai, và nó không cần Unity nên phải được test ở đây.
    /// </summary>
    public sealed class MenuSelectionTests
    {
        private static MenuScreen ScreenWith(params MenuItemInfo[] items)
        {
            return new MenuScreen { ListId = 1039, Type = 0, Title = "Thử", Items = items };
        }

        private static MenuItemInfo Row(bool canSelect = true, bool showDialog = false,
                                        bool closeAfter = false, string title = "dòng")
        {
            return new MenuItemInfo
            {
                Title = title,
                CanSelect = canSelect,
                ShowDialog = showDialog,
                DialogText = showDialog ? "Chắc chưa?" : null,
                LeftCommandText = showDialog ? "Đồng ý" : null,
                RightCommandText = showDialog ? "Thôi" : null,
                CloseScreenAfterClick = closeAfter,
                PaymentOptions = Array.Empty<MenuItemInfo.PaymentOption>()
            };
        }

        [Fact]
        public void DongKhongChoChon_BiChan()
        {
            Assert.Equal(MenuAction.Blocked, MenuSelection.Decide(ScreenWith(Row(canSelect: false)), 0));
        }

        [Fact]
        public void DongThuong_GuiNgay()
        {
            Assert.Equal(MenuAction.Send, MenuSelection.Decide(ScreenWith(Row()), 0));
        }

        [Fact]
        public void DongCoShowDialog_PhaiXacNhanTruoc()
        {
            Assert.Equal(MenuAction.Confirm, MenuSelection.Decide(ScreenWith(Row(showDialog: true)), 0));
        }

        [Fact]
        public void KhongChoChon_ThangHonCaShowDialog()
        {
            // Dòng vừa không cho chọn vừa có dialog: chặn thắng, không được mở hộp
            // xác nhận rồi mới phát hiện không gửi được.
            var screen = ScreenWith(Row(canSelect: false, showDialog: true));

            Assert.Equal(MenuAction.Blocked, MenuSelection.Decide(screen, 0));
        }

        [Fact]
        public void NoiDungHopXacNhan_LayNguyenTuServer()
        {
            var prompt = MenuSelection.PromptFor(ScreenWith(Row(showDialog: true)), 0);

            Assert.Equal("Chắc chưa?", prompt.Text);
            Assert.Equal("Đồng ý", prompt.ConfirmLabel);
            Assert.Equal("Thôi", prompt.CancelLabel);
        }

        [Fact]
        public void XinHopXacNhanChoDongKhongCan_Nem()
        {
            Assert.Throws<InvalidOperationException>(
                () => MenuSelection.PromptFor(ScreenWith(Row()), 0));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(99)]
        public void ChiSoNgoaiKhoang_Nem(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => MenuSelection.Decide(ScreenWith(Row()), index));
        }

        [Fact]
        public void CoCloseScreenAfterClick_ThiBaoDong()
        {
            Assert.True(MenuSelection.ShouldCloseAfter(ScreenWith(Row(closeAfter: true)), 0));
            Assert.False(MenuSelection.ShouldCloseAfter(ScreenWith(Row(closeAfter: false)), 0));
        }
    }
}
