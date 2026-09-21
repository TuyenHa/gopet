using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Popup Cửa hàng nuốt <see cref="MenuScreen"/> của shop, các listId khác vẫn
    /// dựng <see cref="GenericMenuView"/> như thường.
    ///
    /// <para><b>Đường thật:</b> byte của server dispatch qua <see cref="MessageRouter"/>
    /// (không gọi tắt sự kiện). Không thì test bỏ qua đúng chỗ dễ sai — cờ swallow
    /// trong <see cref="UiRoot.ShowMenu"/>.</para>
    /// </summary>
    public sealed class ShopPopupTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private GuiderHandler _guider;
        private List<Message> _sent;
        private UiRoot _ui;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("UiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            _guider = new GuiderHandler(_sent.Add);
            _guider.RegisterOn(_router);

            _ui = UiRoot.Create(_host.transform, null);
            _ui.Initialize(_guider, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) UnityEngine.Object.DestroyImmediate(_host);
        }

        private void ServerSendsMenu(int listId, params MenuItemInfo[] items)
        {
            TestPackets.Dispatch(_router, TestPackets.MenuWire(listId, items));
        }

        [Test]
        public void OpenShopPopup_GuiRequestShopWeapon()
        {
            _ui.OpenShopPopup();

            Assert.AreEqual(1, _sent.Count, "phải gửi 1 gói REQUEST_SHOP khi mở popup");
            var wire = _sent[0].ToWire();
            Assert.AreEqual(GopetCmd.REQUEST_SHOP, unchecked((sbyte)wire[0]),
                "opcode phải là REQUEST_SHOP");
            Assert.AreEqual(ShopPopupView.ShopWeapon, unchecked((sbyte)wire[1]),
                "shopId mặc định là Vũ khí = SHOP_WEAPON");
            Assert.IsNotNull(_ui.ShopPopup);
            Assert.AreEqual(ShopPopupView.ShopWeapon, _ui.ShopPopup.ActiveShopId);
        }

        [Test]
        public void ServerShopMenu_PopupNuot_KhongDeGenericMenuViewNgoai()
        {
            _ui.OpenShopPopup();
            ServerSendsMenu(ShopPopupView.ShopWeapon, TestPackets.Item(10, "Kiếm sắt"));

            Assert.AreEqual(1, _ui.Stack.Depth,
                "popup shop đã ở trong stack; MenuScreen khớp KHÔNG được đẻ thêm view ngoài");
            Assert.IsInstanceOf<ShopPopupView>(_ui.Current);
        }

        [Test]
        public void MenuKhacListId_VanDeGenericMenuViewNhuThuong()
        {
            _ui.OpenShopPopup();
            // Kho đồ, task hay bất kỳ menu nào khác vẫn phải dùng đường cũ — không
            // được nuốt nhầm chỉ vì popup shop đang mở.
            ServerSendsMenu(1040, TestPackets.Item(11, "Sách hồi sinh"));

            Assert.AreEqual(2, _ui.Stack.Depth,
                "menu ngoài shop phải nằm trên popup — 2 tầng");
            Assert.IsInstanceOf<GenericMenuView>(_ui.Current);
        }

        [Test]
        public void OpenShopHaiLan_ChiCoMotPopup()
        {
            _ui.OpenShopPopup();
            _ui.OpenShopPopup();

            Assert.AreEqual(1, _ui.Stack.Depth);
            Assert.AreEqual(1, _sent.Count, "chỉ 1 lần gửi RequestShop, không double");
        }

        [Test]
        public void BackDongPopup_ThamChieuTrongUiRootDuocXoa()
        {
            _ui.OpenShopPopup();
            Assert.IsNotNull(_ui.ShopPopup);

            _ui.Back();

            Assert.IsNull(_ui.ShopPopup,
                "sau khi đóng, UiRoot không được giữ tham chiếu popup đã Destroy");
            Assert.AreEqual(0, _ui.Stack.Depth);
        }

        [Test]
        public void GoiShopKhacTabActive_VanSwallow_KhongDeGenericMenuViewNgoai()
        {
            // Race đổi tab: gói shop KHÁC tab active vẫn phải swallow. Nếu chỉ khớp
            // _activeShopId mới nuốt (như bản trước), gói chậm sẽ đẻ GenericMenuView
            // đè lên popup — chính điều popup được thiết kế để chặn.
            _ui.OpenShopPopup();  // _activeShopId = ShopWeapon (1)
            ServerSendsMenu(ShopPopupView.ShopArmour, TestPackets.Item(20, "Giáp cũ"));

            Assert.AreEqual(1, _ui.Stack.Depth,
                "gói shop-family khác listId active phải bị swallow, không đẻ view ngoài");
            Assert.IsInstanceOf<ShopPopupView>(_ui.Current);
        }

        [Test]
        public void ServerShopMenu_DungTheItemTrongPopup()
        {
            _ui.OpenShopPopup();
            ServerSendsMenu(ShopPopupView.ShopWeapon,
                TestPackets.ShopItem(10, "búa gỗ(Yêu cầu   25 (str) ,  0 (agi) ,  0 (int))",
                    "búa cho chiến binh( [80 (atk) -85 (atk) ] ,  [0 (def) -0 (def) ],  " +
                    "[0 (hp) -0 (hp) ] ,  [0 (mp) -0 (mp) ] )", "20 (vang)"),
                TestPackets.ShopItem(11, "kiếm cùi", "cho sát thủ", "25 (vang)"));

            Assert.AreEqual(2, _ui.ShopPopup.RowCount, "mỗi dòng server gửi phải có một thẻ item");
        }

        [Test]
        public void DoiTab_XoaDanhSachCu_KhongDeMuaNhamMonTabTruoc()
        {
            _ui.OpenShopPopup();
            ServerSendsMenu(ShopPopupView.ShopWeapon,
                TestPackets.ShopItem(10, "búa gỗ", "mô tả", "20 (vang)"));
            Assert.AreEqual(1, _ui.ShopPopup.RowCount);

            _ui.ShopPopup.SelectTab(ShopPopupView.ShopArmour);

            Assert.AreEqual(0, _ui.ShopPopup.RowCount,
                "danh sách vũ khí phải biến mất ngay khi chuyển sang tab Giáp");
            Assert.AreEqual(ShopPopupView.ShopArmour, _ui.ShopPopup.ActiveShopId);
        }

        [Test]
        public void GoiShopSkinFromMap19Quirk_VanDuocSwallow()
        {
            // Server tự đổi SHOP_FOOD sang SHOP_SKIN=7 khi player ở map 19
            // (GameController.cs:2367). Không có tab Skin nhưng gói vẫn thuộc họ shop.
            _ui.OpenShopPopup();
            ServerSendsMenu(ShopPopupView.ShopSkin, TestPackets.Item(30, "Áo skin"));

            Assert.AreEqual(1, _ui.Stack.Depth,
                "SHOP_SKIN từ map-19 quirk phải bị popup swallow, không đè GenericMenuView");
        }
    }
}
