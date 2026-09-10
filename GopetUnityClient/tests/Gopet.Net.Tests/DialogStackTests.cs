using System;
using Gopet.Net.Guider;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Chồng màn hình — chỗ dễ sinh ra trạng thái kẹt không thoát được.</summary>
    public sealed class DialogStackTests
    {
        [Fact]
        public void BanDau_Rong()
        {
            var stack = new DialogStack();

            Assert.True(stack.IsEmpty);
            Assert.Null(stack.Top);
            Assert.False(stack.Pop());
        }

        [Fact]
        public void DongTheoDungThuTuNguoc()
        {
            var stack = new DialogStack();
            stack.Push("a");
            stack.Push("b");
            stack.Push("c");

            Assert.Equal("c", stack.Top);
            stack.Pop();
            Assert.Equal("b", stack.Top);
            stack.Pop();
            Assert.Equal("a", stack.Top);
            stack.Pop();
            Assert.True(stack.IsEmpty);
        }

        [Fact]
        public void PopHetRoi_TraVeFalseChuKhongNem()
        {
            // Nút back bấm thêm một lần sau khi đóng hết: tầng trên tự quyết làm gì,
            // không được ném lỗi giữa lúc người chơi đang bấm.
            var stack = new DialogStack();
            stack.Push("a");

            Assert.True(stack.Pop());
            Assert.False(stack.Pop());
        }

        [Fact]
        public void XoaManHinhOGiua_KhongDoiCaiDangHien()
        {
            // closeScreenAfterClick đóng chính màn hình chứa dòng, trong khi hộp xác
            // nhận vừa mở vẫn đang nằm đè lên.
            var stack = new DialogStack();
            stack.Push("menu");
            stack.Push("confirm");

            Assert.True(stack.Remove("menu"));

            Assert.Equal("confirm", stack.Top);
            Assert.Equal(1, stack.Depth);
        }

        [Fact]
        public void XoaCaiDangHien_ThiLoRaCaiDuoi()
        {
            var stack = new DialogStack();
            stack.Push("menu");
            stack.Push("confirm");

            Assert.True(stack.Remove("confirm"));

            Assert.Equal("menu", stack.Top);
        }

        [Fact]
        public void XoaCaiKhongCo_TraVeFalse()
        {
            var stack = new DialogStack();
            stack.Push("a");

            Assert.False(stack.Remove("khong-co"));
            Assert.Equal(1, stack.Depth);
        }

        [Fact]
        public void SuKienTopChanged_BanDungLuc()
        {
            var stack = new DialogStack();
            var seen = new System.Collections.Generic.List<object>();
            stack.TopChanged += t => seen.Add(t);

            stack.Push("a");
            stack.Push("b");
            stack.Pop();
            stack.Clear();

            Assert.Equal(new object[] { "a", "b", "a", null }, seen);
        }

        [Fact]
        public void ChongQuaSau_Nem()
        {
            var stack = new DialogStack();
            for (var i = 0; i < DialogStack.MaxDepth; i++) stack.Push($"m{i}");

            Assert.Throws<InvalidOperationException>(() => stack.Push("mot-lop-nua"));
        }

        [Fact]
        public void ClearKhiDangRong_KhongBanSuKien()
        {
            var stack = new DialogStack();
            var fired = 0;
            stack.TopChanged += _ => fired++;

            stack.Clear();

            Assert.Equal(0, fired);
        }
    }
}
