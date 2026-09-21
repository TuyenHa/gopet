using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Tách chuỗi thô của cửa hàng thành tên / yêu cầu / mô tả / chip chỉ số.
    ///
    /// <para>Chuỗi mẫu lấy đúng khuôn <c>ShopTemplateItem.getName/getDesc</c> của
    /// server (kể cả các khoảng trắng thừa nó sinh ra), không phải bản gõ lại cho
    /// gọn — khuôn thật mới là thứ parser phải chịu được.</para>
    /// </summary>
    public sealed class ShopItemTextTests
    {
        private const string BuaGoTitle = "búa gỗ(Yêu cầu   25 (str) ,  20 (agi) ,  20 (int))";
        private const string BuaGoDesc =
            "búa dành cho chiến binh( [80 (atk) -85 (atk) ] ,  [0 (def) -0 (def) ],  " +
            "[0 (hp) -0 (hp) ] ,  [0 (mp) -0 (mp) ] )";

        [Fact]
        public void Parse_TachTenKhoiKhoiYeuCau()
        {
            var text = ShopItemText.Parse(BuaGoTitle, BuaGoDesc);

            Assert.Equal("Búa gỗ", text.Name);
            Assert.Equal("Yêu cầu 25 str, 20 agi, 20 int", text.Requirement);
        }

        [Fact]
        public void Parse_MoTaBoKhoiChiSo()
        {
            var text = ShopItemText.Parse(BuaGoTitle, BuaGoDesc);

            Assert.Equal("Búa dành cho chiến binh", text.Description);
        }

        [Fact]
        public void Parse_DaiTanCongGopThanhMotChip()
        {
            var text = ShopItemText.Parse(BuaGoTitle, BuaGoDesc);

            Assert.Equal(2, text.Stats.Length);
            Assert.Equal("Tấn công", text.Stats[0].Label);
            // Dấu '-' giữa hai đầu dải là dấu nối, không phải dấu âm: "80--85" là sai.
            Assert.Equal("80-85", text.Stats[0].Value);
            Assert.Equal("Phòng thủ", text.Stats[1].Label);
            Assert.Equal("0", text.Stats[1].Value);
        }

        [Fact]
        public void Parse_HaiDauDaiBangNhau_ChiHienMotSo()
        {
            var text = ShopItemText.Parse("giáp",
                "áo( [0 (atk) -0 (atk) ] ,  [12 (def) -12 (def) ],  [0 (hp) -0 (hp) ] ,  [0 (mp) -0 (mp) ] )");

            Assert.Equal("0", text.Stats[0].Value);
            Assert.Equal("12", text.Stats[1].Value);
        }

        [Fact]
        public void Parse_UuTienChiSoKhac0_BoTanCongBang0()
        {
            // Mũ cộng def + hp: nhét thêm "Tấn công 0" là thừa một chip, mà thẻ chỉ
            // chứa nổi 3 chip theo bề ngang.
            var text = ShopItemText.Parse("mũ",
                "nón( [0 (atk) -0 (atk) ] ,  [3 (def) -3 (def) ],  [50 (hp) -50 (hp) ] ,  [0 (mp) -0 (mp) ] )");

            Assert.Equal(2, text.Stats.Length);
            Assert.Equal("Phòng thủ", text.Stats[0].Label);
            Assert.Equal("HP", text.Stats[1].Label);
            Assert.Equal("50", text.Stats[1].Value);
        }

        [Fact]
        public void Parse_BonChiSoDeuKhac0_CatConBaChip()
        {
            var text = ShopItemText.Parse("mũ",
                "nón( [5 (atk) -6 (atk) ] ,  [3 (def) -3 (def) ],  [50 (hp) -60 (hp) ] ,  [7 (mp) -7 (mp) ] )");

            Assert.Equal(3, text.Stats.Length);
            Assert.Equal("Tấn công", text.Stats[0].Label);
            Assert.Equal("Phòng thủ", text.Stats[1].Label);
            Assert.Equal("HP", text.Stats[2].Label);
        }

        [Fact]
        public void Parse_ChiCoTanCong_VanBuThemPhongThu0()
        {
            // Đúng như ảnh mẫu: vũ khí hiện "Tấn công 80-85" + "Phòng thủ 0".
            var text = ShopItemText.Parse(BuaGoTitle, BuaGoDesc);

            Assert.Equal(2, text.Stats.Length);
            Assert.Equal("Phòng thủ", text.Stats[1].Label);
            Assert.Equal("0", text.Stats[1].Value);
        }

        [Fact]
        public void Parse_YeuCauToanSo0_KhongHienDongYeuCau()
        {
            var text = ShopItemText.Parse("áo vải(Yêu cầu   0 (str) ,  0 (agi) ,  0 (int))", "áo");

            Assert.Equal("Áo vải", text.Name);
            Assert.Equal(string.Empty, text.Requirement);
        }

        [Fact]
        public void Parse_ItemNgoaiHinh_MoTaLaNguyenKhoiChiSo_KhongDoRaDongMoTa()
        {
            // SKIN_ITEM: getDesc trả "+%s +%s +%s +%s", KHÔNG có ngoặc bao ngoài như
            // trang bị. Không bắt riêng thì cả đoạn đổ thẳng ra dòng mô tả.
            var text = ShopItemText.Parse("Áo choàng lửa",
                "+[0 (atk) -0 (atk) ]  +[12 (def) -12 (def) ] +[0 (hp) -0 (hp) ]  +[0 (mp) -0 (mp) ] ");

            Assert.Equal("Áo choàng lửa", text.Name);
            Assert.Equal(string.Empty, text.Description);
            Assert.Equal(2, text.Stats.Length);
            Assert.Equal("0", text.Stats[0].Value);
            Assert.Equal("12", text.Stats[1].Value);
        }

        [Fact]
        public void Parse_ItemThuong_GiuNguyenChuoi_KhongCoChip()
        {
            // Thức ăn, pet, item xài liền: server không nhét khối chỉ số nào.
            var text = ShopItemText.Parse("thịt nướng  x5", "hồi 200 hp cho pet");

            Assert.Equal("Thịt nướng  x5", text.Name);
            Assert.Equal("Hồi 200 hp cho pet", text.Description);
            Assert.Empty(text.Stats);
            Assert.Equal(string.Empty, text.Requirement);
        }

        [Fact]
        public void Parse_ChuoiRong_KhongNem()
        {
            var text = ShopItemText.Parse(null, null);

            Assert.Equal(string.Empty, text.Name);
            Assert.Equal(string.Empty, text.Description);
            Assert.Empty(text.Stats);
        }

        [Theory]
        [InlineData("20 (vang)", "20")]
        [InlineData("5 (ngoc)", "5")]
        [InlineData("3 thỏi bạc", "3 thỏi bạc")]
        [InlineData("", "")]
        public void ShortMoney_BoTagJ2ME_GiuDonViBangChu(string input, string expected)
        {
            Assert.Equal(expected, ShopItemText.ShortMoney(input));
        }
    }
}
