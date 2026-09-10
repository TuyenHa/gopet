using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Camera trôi ngang của nền map. Map 11 rộng 576, khung nhìn 320 → quãng cuộn 256.
    /// </summary>
    public sealed class JarMapScrollTests
    {
        private const float Speed = 30f;

        [Fact]
        public void BatDau_ODauMapVaDiSangPhai()
        {
            var scroll = new JarMapScroll(256f);

            scroll.Advance(0.5f, Speed);

            Assert.Equal(15f, scroll.X, 3);
            Assert.Equal(1f, scroll.Direction);
        }

        [Fact]
        public void ChamMepPhai_DaoChieuVaKhongVuotQua()
        {
            var scroll = new JarMapScroll(256f);

            scroll.Advance(100f, Speed); // thừa sức chạm mép

            Assert.Equal(256f, scroll.X, 3);
            Assert.Equal(-1f, scroll.Direction);
        }

        [Fact]
        public void SauKhiDaoChieu_QuayVeMepTrai_RoiLaiDaoChieu()
        {
            var scroll = new JarMapScroll(256f);

            scroll.Advance(100f, Speed); // tới mép phải, quay đầu
            scroll.Advance(1f, Speed);
            Assert.Equal(226f, scroll.X, 3); // 256 - 30

            scroll.Advance(100f, Speed); // về mép trái, quay đầu lần nữa
            Assert.Equal(0f, scroll.X, 3);
            Assert.Equal(1f, scroll.Direction);
        }

        /// <summary>
        /// Kẹp PHẢI xảy ra trước khi đảo chiều. Nếu chỉ đảo dấu mà không kẹp, camera
        /// lấn ra ngoài map và rìa màn hình lộ khoảng trống.
        /// </summary>
        [Fact]
        public void BuocDaiQuaMep_KhongBaoGioRaNgoaiKhoang()
        {
            var scroll = new JarMapScroll(256f);

            for (var i = 0; i < 50; i++)
            {
                scroll.Advance(3f, Speed); // 90 pixel mỗi bước, cố tình nhảy quá mép
                Assert.InRange(scroll.X, 0f, 256f);
            }
        }

        [Fact]
        public void MapHepHonKhungNhin_DungYen()
        {
            var scroll = new JarMapScroll(-100f);

            scroll.Advance(10f, Speed);

            Assert.Equal(0f, scroll.Max);
            Assert.Equal(0f, scroll.X);
        }
    }
}
