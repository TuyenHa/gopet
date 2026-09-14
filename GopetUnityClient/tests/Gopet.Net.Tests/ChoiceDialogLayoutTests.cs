using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Bố cục nút của ChoiceDialogView. Loại lỗi dễ mắc: hộp thoại 2-3 nút quen thuộc
    /// (YesNo/Confirm) đổi bố cục ngoài ý muốn, hoặc hộp nhiều nút (option NPC) tràn màn hình.
    /// </summary>
    public sealed class ChoiceDialogLayoutTests
    {
        private const float ScreenHeight = 720f;

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Compute_ToiDa3Nut_XepNgangVaGiuNguyenCongThucCu(int count)
        {
            var result = ChoiceDialogLayout.Compute(count, ScreenHeight);

            Assert.False(result.Vertical);
            Assert.Equal(ChoiceDialogLayout.DefaultPanelHeight, result.PanelHeight);
            Assert.Equal(ChoiceDialogLayout.ButtonHeight, result.RowHeight);
            Assert.Equal(ChoiceDialogLayout.HorizontalGap, result.Gap);
            Assert.Equal(ChoiceDialogLayout.DefaultFontSize, result.FontSize);

            // Công thức cũ trong ChoiceDialogView.MakeButton trước khi tách layout.
            var expectedWidth = System.Math.Min(170f,
                (ChoiceDialogLayout.PanelWidth - 48f - ChoiceDialogLayout.HorizontalGap * (count - 1)) / count);
            Assert.Equal(expectedWidth, result.RowWidth);
        }

        [Fact]
        public void Compute_4Nut_XepDoc()
        {
            var result = ChoiceDialogLayout.Compute(4, ScreenHeight);
            Assert.True(result.Vertical);
        }

        [Fact]
        public void Compute_7Nut_TranChan_XepDocVaDocDuoc()
        {
            // TRAN CHAN (LinhThuCityNpcOptions) có 7 option — case thật gây ra bug hàng ngang.
            var result = ChoiceDialogLayout.Compute(7, ScreenHeight);

            Assert.True(result.Vertical);
            Assert.True(result.RowWidth >= 300f, $"rowWidth={result.RowWidth} phải >= 300px để đọc được nhãn dài");
            Assert.True(result.PanelHeight <= ScreenHeight * 0.88f,
                $"panelHeight={result.PanelHeight} phải <= 88% màn hình ({ScreenHeight * 0.88f})");
        }

        [Fact]
        public void Compute_20Nut_RowHeightChamSanToiThieu()
        {
            var result = ChoiceDialogLayout.Compute(20, ScreenHeight);

            Assert.True(result.Vertical);
            Assert.True(result.RowHeight >= ChoiceDialogLayout.MinRowHeight);
            Assert.Equal(ChoiceDialogLayout.CompactFontSize, result.FontSize);
        }

        [Theory]
        [InlineData(4)]
        [InlineData(7)]
        [InlineData(12)]
        [InlineData(20)]
        [InlineData(30)]
        public void Compute_KhoiNutLuonNamTrongPanelHeight_KhongBaoGioTran(int count)
        {
            // Bug đã sửa: khi rowHeight chạm sàn MinRowHeight, code cũ vẫn ép panelHeight
            // xuống trần 88% màn hình dù khối nút thật cần nhiều chỗ hơn → nút tràn ra
            // ngoài panel. Bất kể count bao nhiêu, khối nút PHẢI vừa trong panelHeight trả về.
            var result = ChoiceDialogLayout.Compute(count, ScreenHeight);

            var contentHeight = ChoiceDialogLayout.TopPadding
                + count * result.RowHeight + (count - 1) * result.Gap + ChoiceDialogLayout.BottomPadding;

            Assert.True(contentHeight <= result.PanelHeight + 0.01f,
                $"count={count}: contentHeight={contentHeight} phải nằm trong panelHeight={result.PanelHeight}");
        }

        [Fact]
        public void Compute_SoNutVuaPhaiVanTuan88PhanTramManHinh()
        {
            // Case thật (TRAN CHAN 7 option) không cần chạm sàn compact → vẫn theo đúng
            // ràng buộc 88% màn hình như thiết kế ban đầu.
            var result = ChoiceDialogLayout.Compute(7, ScreenHeight);
            Assert.True(result.PanelHeight <= ScreenHeight * 0.88f);
        }

        [Fact]
        public void Compute_KhiThuNhoRowHeight_FontCungThuNho()
        {
            var normal = ChoiceDialogLayout.Compute(7, ScreenHeight);
            var crowded = ChoiceDialogLayout.Compute(20, ScreenHeight);

            Assert.Equal(ChoiceDialogLayout.DefaultFontSize, normal.FontSize);
            Assert.Equal(ChoiceDialogLayout.CompactFontSize, crowded.FontSize);
        }

        [Fact]
        public void Compute_SoNutNhoHon1_ClampVe1()
        {
            var result = ChoiceDialogLayout.Compute(0, ScreenHeight);
            Assert.False(result.Vertical);
        }
    }
}
