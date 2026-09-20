using System;
using System.Collections.Generic;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Chọn NPC gần nhất để hiện nút "Nói chuyện". Loại lỗi dễ mắc: nhầm sentinel với id
    /// NPC âm thật, quên hysteresis gây nhấp nháy nút ở biên tầm.
    /// </summary>
    public sealed class NpcProximityTests
    {
        [Fact]
        public void Pick_DanhSachRong_TraVeNone()
        {
            var result = NpcProximity.Pick(0, 0, Array.Empty<NpcPoint>(), NpcProximity.None);
            Assert.Equal(NpcProximity.None, result);
        }

        [Fact]
        public void Pick_DanhSachNull_NemArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                NpcProximity.Pick(0, 0, null, NpcProximity.None));
        }

        [Fact]
        public void Pick_KhongNpcNaoTrongTam_TraVeNone()
        {
            var npcs = new List<NpcPoint> { new NpcPoint(1, 500, 500) };
            var result = NpcProximity.Pick(0, 0, npcs, NpcProximity.None);
            Assert.Equal(NpcProximity.None, result);
        }

        [Fact]
        public void Pick_NhieuNpcTrongTam_ChonNpcGanNhat()
        {
            var npcs = new List<NpcPoint>
            {
                new NpcPoint(1, 55, 0), // xa hơn (vẫn trong Show 60)
                new NpcPoint(2, 20, 0), // gần nhất
                new NpcPoint(3, 40, 0),
            };
            var result = NpcProximity.Pick(0, 0, npcs, NpcProximity.None);
            Assert.Equal(2, result);
        }

        [Fact]
        public void Pick_IdNpcAm_KhongBiNhamVoiSentinel()
        {
            // TRAN CHAN id = -1 (LinhThuCityNpcOptions) — id âm phải hoạt động bình thường.
            var npcs = new List<NpcPoint> { new NpcPoint(-1, 5, 0) };
            var result = NpcProximity.Pick(0, 0, npcs, NpcProximity.None);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Pick_DangChonVaVanTrongHideRadius_GiuNguyen()
        {
            // NPC đang chọn ở 65px (> ShowRadius 60 nhưng <= HideRadius 70) → giữ nguyên,
            // dù có NPC khác gần hơn ở 30px, tránh nhảy nút liên tục ở biên tầm.
            var npcs = new List<NpcPoint>
            {
                new NpcPoint(1, 65, 0),
                new NpcPoint(2, 30, 0),
            };
            var result = NpcProximity.Pick(0, 0, npcs, currentId: 1);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Pick_DangChonRaKhoiHideRadius_DoiSangNpcKhac()
        {
            // NPC đang chọn ra khỏi HideRadius (80 > 70) → đổi sang NPC gần nhất trong ShowRadius.
            var npcs = new List<NpcPoint>
            {
                new NpcPoint(1, 80, 0),
                new NpcPoint(2, 30, 0),
            };
            var result = NpcProximity.Pick(0, 0, npcs, currentId: 1);
            Assert.Equal(2, result);
        }

        [Fact]
        public void Pick_DangChonRaKhoiHideRadius_KhongNpcKhacTrongTam_TraVeNone()
        {
            var npcs = new List<NpcPoint> { new NpcPoint(1, 80, 0) };
            var result = NpcProximity.Pick(0, 0, npcs, currentId: 1);
            Assert.Equal(NpcProximity.None, result);
        }

        [Fact]
        public void Pick_DungRanhGioiShowRadius_VanDuocChon()
        {
            var npcs = new List<NpcPoint> { new NpcPoint(1, NpcProximity.ShowRadius, 0) };
            var result = NpcProximity.Pick(0, 0, npcs, NpcProximity.None);
            Assert.Equal(1, result);
        }

        [Fact]
        public void Pick_VuaQuaRanhGioiShowRadius_KhongDuocChon()
        {
            var npcs = new List<NpcPoint> { new NpcPoint(1, NpcProximity.ShowRadius + 1f, 0) };
            var result = NpcProximity.Pick(0, 0, npcs, NpcProximity.None);
            Assert.Equal(NpcProximity.None, result);
        }

        /// <summary>Nút đánh quái dùng tầm rộng hơn tầm nói chuyện với NPC.</summary>
        [Fact]
        public void Pick_BanKinhRieng_BatDuocMucTieuNgoaiTamMacDinh()
        {
            var mobs = new List<NpcPoint> { new NpcPoint(7, 90, 0) };

            Assert.Equal(NpcProximity.None, NpcProximity.Pick(0, 0, mobs, NpcProximity.None));
            Assert.Equal(7, NpcProximity.Pick(0, 0, mobs, NpcProximity.None,
                showRadius: 96f, hideRadius: 120f));
        }

        /// <summary>Hysteresis phải chạy theo bán kính truyền vào, không phải hằng số NPC.</summary>
        [Fact]
        public void Pick_BanKinhRieng_GiuMucTieuDangNhamTrongTamNha()
        {
            var mobs = new List<NpcPoint> { new NpcPoint(7, 110, 0) };

            Assert.Equal(7, NpcProximity.Pick(0, 0, mobs, currentId: 7,
                showRadius: 96f, hideRadius: 120f));
            Assert.Equal(NpcProximity.None, NpcProximity.Pick(0, 0, mobs, currentId: 7,
                showRadius: 96f, hideRadius: 100f));
        }
    }
}
