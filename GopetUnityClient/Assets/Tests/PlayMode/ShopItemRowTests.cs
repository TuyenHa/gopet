using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Hàng chip chỉ số của thẻ item phải xếp cạnh nhau và nằm trong khung.
    ///
    /// <para><b>Vì sao có file này:</b> hai lần liên tiếp hàng chip vỡ theo hai kiểu
    /// ngược nhau mà không test nào bắt được — lần đầu chip bị bóp nhỏ hơn chữ nên chữ
    /// tràn đè lên chip bên cạnh, lần sau ba chip chồng lên nhau ở mép trái vì
    /// <c>ContentSizeFitter</c> nong chip ra sau khi hàng chip đã xếp chỗ. Cả hai đều
    /// chỉ lộ ra khi nhìn bằng mắt. Test này đo toạ độ thật sau một lượt dựng layout.
    /// </para>
    /// </summary>
    public sealed class ShopItemRowTests
    {
        private const string WeaponDesc =
            "búa dành cho chiến binh( [80 (atk) -85 (atk) ] ,  [0 (def) -0 (def) ],  " +
            "[0 (hp) -0 (hp) ] ,  [0 (mp) -0 (mp) ] )";

        private GameObject _host;
        private ShopItemRow _row;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var hostRect = (RectTransform)_host.transform;
            hostRect.sizeDelta = new Vector2(370f, ShopItemRow.Height);

            _row = ShopItemRow.Create(_host.transform, UiBuilder.DefaultFont());
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        private RectTransform Chips => (RectTransform)_row.transform.Find("Stats");

        private void BindAndLayout(string title, string description)
        {
            _row.Bind(new MenuItemInfo
            {
                ItemId = 1,
                Title = title,
                Description = description,
                CanSelect = true,
                PaymentOptions = new[]
                {
                    new MenuItemInfo.PaymentOption { Id = 0, MoneyText = "20 (vang)", IsEnabled = 1 }
                }
            }, null);

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_row.transform);
        }

        [Test]
        public void TrangBi_HaiChipXepCanhNhau_KhongChongLen()
        {
            BindAndLayout("búa gỗ", WeaponDesc);

            var chips = Chips;
            Assert.AreEqual(2, chips.childCount, "vũ khí: Tấn công + Phòng thủ");

            var first = (RectTransform)chips.GetChild(0);
            var second = (RectTransform)chips.GetChild(1);

            Assert.Greater(first.rect.width, 20f,
                "chip bị bóp gần bằng 0 — chữ sẽ tràn ra ngoài nền chip");
            Assert.GreaterOrEqual(second.anchoredPosition.x,
                first.anchoredPosition.x + first.rect.width,
                "chip thứ hai phải bắt đầu sau chip thứ nhất, không chồng lên");
        }

        [Test]
        public void ChipNamTrongKhungHangChip_KhongTranSangNutGia()
        {
            BindAndLayout("búa gỗ", WeaponDesc);

            var chips = Chips;
            var last = (RectTransform)chips.GetChild(chips.childCount - 1);
            var right = last.anchoredPosition.x + last.rect.width;

            Assert.LessOrEqual(right, chips.rect.width + 0.5f,
                "chip cuối vượt khỏi vùng chip — sẽ đè lên nút giá và tràn khỏi viền");
            Assert.GreaterOrEqual(((RectTransform)chips.GetChild(0)).anchoredPosition.x, -0.5f,
                "chip đầu bị đẩy ra ngoài mép trái");
        }

        [Test]
        public void ChipCoBeRongRiengTheoDoDaiChu()
        {
            BindAndLayout("búa gỗ", WeaponDesc);

            var chips = Chips;
            var tanCong = ((RectTransform)chips.GetChild(0)).rect.width;   // "Tấn công 80-85"
            var phongThu = ((RectTransform)chips.GetChild(1)).rect.width;  // "Phòng thủ 0"

            Assert.Greater(tanCong, phongThu,
                "chuỗi dài hơn phải cho chip rộng hơn; bằng nhau nghĩa là bề rộng bị ép cứng");
        }

        [Test]
        public void ThucAn_KhongCoChipNao()
        {
            BindAndLayout("thịt nướng", "hồi 200 hp cho pet");

            Assert.AreEqual(0, Chips.childCount,
                "item không phải trang bị thì không có khối chỉ số nào để hiện");
        }
    }
}
