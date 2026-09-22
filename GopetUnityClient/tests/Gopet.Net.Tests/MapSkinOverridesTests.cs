using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Bản jar xài chung bộ tile mùa đông cho nhiều map. Test chốt từng map được đổi
    /// những dải nào — đổi nhầm là cả map lệch tông (cỏ xanh viền tuyết, hoặc ngược lại).
    /// </summary>
    public sealed class MapSkinOverridesTests
    {
        private const int SnowGrassA = 161, SnowGrassB = 162, SnowBorder = 3;
        private const int GrassA = 11161, GrassB = 11162, StoneBorder = 11003;

        [Fact]
        public void ThanhPhoLinhThu_DoiCaCoLanVien()
        {
            var map = MapSkinOverrides.BeastCityMapId;

            Assert.Equal(GrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(GrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
            Assert.Equal(StoneBorder, MapSkinOverrides.ResolveImageId(map, SnowBorder));
        }

        /// <summary>Đấu trường mặc cùng bộ áo với thành phố: cỏ xanh + viền đá.</summary>
        [Fact]
        public void DauTruong_DoiCaCoLanVien()
        {
            var map = MapSkinOverrides.ArenaMapId;

            Assert.Equal(StoneBorder, MapSkinOverrides.ResolveImageId(map, SnowBorder));
            Assert.Equal(GrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(GrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
        }

        /// <summary>
        /// Đường lên đỉnh núi: mặt tuyết thành thảm cỏ, vách tuyết thành vách đá lộ cỏ,
        /// cỏ-lẫn-tuyết thành cỏ, viền tuyết thành viền đá — cả map hết tuyết chứ không
        /// nửa nọ nửa kia.
        /// </summary>
        [Fact]
        public void DuongLenDinhNui_HetTuyet()
        {
            const int snowGround = 180, snowCliff = 179;
            const int greenGround = 11180, greenCliff = 11179;
            var map = MapSkinOverrides.MountainPathMapId;

            Assert.Equal(greenGround, MapSkinOverrides.ResolveImageId(map, snowGround));
            Assert.Equal(greenCliff, MapSkinOverrides.ResolveImageId(map, snowCliff));
            Assert.Equal(GrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(GrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
            Assert.Equal(StoneBorder, MapSkinOverrides.ResolveImageId(map, SnowBorder));
        }

        /// <summary>
        /// Linh Lâm: lối tuyết thành đường đất. PHẢI là bộ 12xxx chứ không phải bộ cỏ 11xxx —
        /// đổi sang cỏ thì lối đi tan vào bãi cỏ, không còn thấy đường.
        /// </summary>
        [Fact]
        public void LinhLam_LoiTuyetThanhDuongDat()
        {
            const int snowPath = 151;
            const int dirtPath = 12151, dirtGrassA = 12161, dirtGrassB = 12162;
            var map = MapSkinOverrides.SpiritForestMapId;

            Assert.Equal(dirtPath, MapSkinOverrides.ResolveImageId(map, snowPath));
            Assert.Equal(dirtGrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(dirtGrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
        }

        /// <summary>Đại Linh Cảnh: cùng ba dải đó và cũng thành đường ĐẤT — dùng chung
        /// bộ 12xxx với Linh Lâm để hai map rừng cùng một tông nền.</summary>
        [Fact]
        public void DaiLinhCanh_LoiTuyetThanhDuongDat()
        {
            const int snowPath = 151;
            const int dirtPath = 12151, dirtGrassA = 12161, dirtGrassB = 12162;
            var map = MapSkinOverrides.GreatSpiritViewMapId;

            Assert.Equal(dirtPath, MapSkinOverrides.ResolveImageId(map, snowPath));
            Assert.Equal(dirtGrassA, MapSkinOverrides.ResolveImageId(map, SnowGrassA));
            Assert.Equal(dirtGrassB, MapSkinOverrides.ResolveImageId(map, SnowGrassB));
        }

        /// <summary>Đại Linh Cảnh: vật thể mùa đông đổi sang bản nhiệt đới.</summary>
        [Theory]
        [InlineData(158, 14158)]    // cây thông có tuyết -> cây dừa
        [InlineData(177, 14177)]    // nhà gỗ phủ tuyết   -> nhà lá
        public void DaiLinhCanh_VatTheMuaDongThanhNhietDoi(int winter, int tropical)
        {
            Assert.Equal(tropical, MapSkinOverrides.ResolveObjectImageId(
                MapSkinOverrides.GreatSpiritViewMapId, winter));
        }

        /// <summary>
        /// Bộ nhiệt đới CHỈ ở Đại Linh Cảnh. Linh Lâm cũng dùng ảnh 158 và cũng đã đổi
        /// nền đất, nhưng cây thì giữ nguyên — không được đổi lây theo bảng nền.
        /// </summary>
        [Theory]
        [InlineData(158)]
        [InlineData(177)]
        public void BoNhietDoi_ChiODaiLinhCanh(int winterObject)
        {
            Assert.Equal(winterObject, MapSkinOverrides.ResolveObjectImageId(
                MapSkinOverrides.SpiritForestMapId, winterObject));
            Assert.Equal(winterObject, MapSkinOverrides.ResolveObjectImageId(
                MapSkinOverrides.BeastCityMapId, winterObject));
        }

        /// <summary>
        /// Bảng VẬT THỂ và bảng Ô NỀN là hai bảng riêng: dải nền 151/161/162 của Đại Linh
        /// Cảnh không được chui sang bảng vật thể, và ngược lại cây 158 không được đổi ở
        /// bảng nền.
        /// </summary>
        [Fact]
        public void BangVatTheVaBangONen_KhongDinhVaoNhau()
        {
            const int snowPath = 151, pineTree = 158;
            var map = MapSkinOverrides.GreatSpiritViewMapId;

            Assert.Equal(snowPath, MapSkinOverrides.ResolveObjectImageId(map, snowPath));
            Assert.Equal(pineTree, MapSkinOverrides.ResolveImageId(map, pineTree));
        }

        /// <summary>Linh Mộc dùng cùng bộ đường đất với Linh Lâm.</summary>
        [Fact]
        public void LinhMoc_DungChungDuongDatLinhLam()
        {
            const int snowPath = 151;
            const int linhMocMapId = 14;

            Assert.Equal(12151, MapSkinOverrides.ResolveImageId(linhMocMapId, snowPath));
            Assert.Equal(12161, MapSkinOverrides.ResolveImageId(linhMocMapId, SnowGrassA));
        }

        /// <summary>Dải ngoài ba dải lối đi thì map 13/15 cũng không đụng tới.</summary>
        [Fact]
        public void DaiKhongThuocLoiDi_GiuNguyenOMapDoiAo()
        {
            const int treeStrip = 158;

            Assert.Equal(treeStrip,
                MapSkinOverrides.ResolveImageId(MapSkinOverrides.SpiritForestMapId, treeStrip));
            Assert.Equal(treeStrip,
                MapSkinOverrides.ResolveImageId(MapSkinOverrides.GreatSpiritViewMapId, treeStrip));
        }

        /// <summary>Dải 180/179 chỉ đổi ở map núi — map khác không có dải này.</summary>
        [Fact]
        public void MatNui_ChiApDungChoMapNui()
        {
            const int snowGround = 180;

            Assert.Equal(snowGround,
                MapSkinOverrides.ResolveImageId(MapSkinOverrides.BeastCityMapId, snowGround));
        }

        /// <summary>Map không khai báo thì giữ nguyên mọi dải — không tự ý đổi tông.</summary>
        [Fact]
        public void MapKhac_GiuNguyenBoTileGoc()
        {
            const int unknownMapId = 999;

            Assert.Equal(SnowBorder, MapSkinOverrides.ResolveImageId(unknownMapId, SnowBorder));
            Assert.Equal(SnowGrassA, MapSkinOverrides.ResolveImageId(unknownMapId, SnowGrassA));
        }

        /// <summary>Dải không nằm trong bảng đổi thì trả về chính nó.</summary>
        [Fact]
        public void DaiKhongKhaiBao_TraVeChinhNo()
        {
            const int someStrip = 158;

            Assert.Equal(someStrip,
                MapSkinOverrides.ResolveImageId(MapSkinOverrides.ArenaMapId, someStrip));
        }
    }
}
