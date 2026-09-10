using System;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Tính khoảng dòng cần dựng khi cuộn. Chỗ dễ sai nhất của virtualization là
    /// lệch một dòng ở mép — bug đó biểu hiện thành "thỉnh thoảng thấy khoảng
    /// trắng lúc cuộn nhanh", rất khó bắt bằng mắt.
    /// </summary>
    public sealed class MenuVirtualizerTests
    {
        private const float Row = 64f;
        private const float Viewport = 640f;   // vừa đúng 10 dòng

        [Fact]
        public void ChuaCuon_DungTuDongDauVaCoDemODuoi()
        {
            var range = MenuVirtualizer.Compute(200, Row, Viewport, scrollY: 0f);

            Assert.Equal(0, range.First);

            // 10 dòng nhìn thấy + 2 dòng đệm dưới. Không có đệm trên vì đang ở đỉnh.
            Assert.Equal(12, range.Count);
        }

        [Fact]
        public void CuonGiua_CoDemCaHaiPhia()
        {
            // Cuộn xuống đúng 20 dòng.
            var range = MenuVirtualizer.Compute(200, Row, Viewport, scrollY: 20 * Row);

            Assert.Equal(18, range.First);          // 20 - 2 đệm
            Assert.Equal(20 + 10 + 2, range.LastExclusive);
        }

        [Fact]
        public void CuonLechNuaDong_VanDungDongBiCatODuoi()
        {
            // Thiếu ceil ở đây là để lại khoảng trắng đúng bằng nửa dòng ở mép dưới.
            var range = MenuVirtualizer.Compute(200, Row, Viewport, scrollY: 0.5f * Row);

            Assert.True(range.Contains(10), "dòng thứ 11 đang bị cắt một nửa, phải dựng");
        }

        [Fact]
        public void CuonToiCuoi_KhongVuotQuaSoDong()
        {
            var range = MenuVirtualizer.Compute(50, Row, Viewport, scrollY: 40 * Row);

            Assert.Equal(50, range.LastExclusive);
            Assert.True(range.First >= 0);
        }

        [Fact]
        public void DanhSachNganHonVungNhin_DungHetKhongDu()
        {
            var range = MenuVirtualizer.Compute(3, Row, Viewport, scrollY: 0f);

            Assert.Equal(0, range.First);
            Assert.Equal(3, range.Count);
        }

        [Fact]
        public void DanhSachRong_KhongDungGi()
        {
            Assert.Equal(0, MenuVirtualizer.Compute(0, Row, Viewport, 0f).Count);
        }

        [Fact]
        public void VungNhinBangKhong_KhongDungGi()
        {
            // Xảy ra thật ở frame đầu, trước khi layout tính xong kích thước.
            Assert.Equal(0, MenuVirtualizer.Compute(100, Row, 0f, 0f).Count);
        }

        [Fact]
        public void CuonAm_CoiNhuODinh()
        {
            // Kéo quá đỉnh (hiệu ứng nảy trên mobile) không được cho ra chỉ số âm.
            var range = MenuVirtualizer.Compute(100, Row, Viewport, scrollY: -500f);

            Assert.Equal(0, range.First);
            Assert.True(range.Count > 0);
        }

        [Fact]
        public void KhongDem_ChiDungDungPhanNhinThay()
        {
            var range = MenuVirtualizer.Compute(200, Row, Viewport, scrollY: 20 * Row, overscan: 0);

            Assert.Equal(20, range.First);
            Assert.Equal(30, range.LastExclusive);
        }

        [Fact]
        public void ChieuCaoDongKhongHopLe_Nem()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => MenuVirtualizer.Compute(10, 0f, Viewport, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => MenuVirtualizer.Compute(10, -5f, Viewport, 0f));
        }

        [Fact]
        public void DemAm_Nem()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => MenuVirtualizer.Compute(10, Row, Viewport, 0f, overscan: -1));
        }

        [Fact]
        public void SoDongDungLuonNhoHonNhieuSoVoiDanhSachDai()
        {
            // Đây là lý do tồn tại của cả class: 500 dòng nhưng chỉ dựng chục cái.
            var range = MenuVirtualizer.Compute(500, Row, Viewport, scrollY: 100 * Row);

            Assert.True(range.Count <= 15, $"dựng {range.Count} dòng cho một vùng nhìn 10 dòng");
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(37f)]
        [InlineData(640f)]
        [InlineData(12800f)]
        [InlineData(999999f)]
        public void MoiViTriCuon_LuonTraKhoangHopLe(float scrollY)
        {
            var range = MenuVirtualizer.Compute(200, Row, Viewport, scrollY);

            Assert.InRange(range.First, 0, 199);
            Assert.InRange(range.LastExclusive, range.First, 200);
        }

        [Fact]
        public void ChieuCaoNoiDungVaViTriDong()
        {
            Assert.Equal(200 * Row, MenuVirtualizer.ContentHeight(200, Row));
            Assert.Equal(0f, MenuVirtualizer.ContentHeight(0, Row));
            Assert.Equal(5 * Row, MenuVirtualizer.OffsetOf(5, Row));
        }
    }
}
