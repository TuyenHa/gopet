using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Luồng mua trong popup cửa hàng: hỏi xác nhận rồi mới gửi, và thiếu tiền thì
    /// báo tại chỗ chứ không gửi lên.
    /// </summary>
    public sealed class ShopBuyTests
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
        public void BamNutGia_HoiXacNhanRoiMoiGuiKemLuaChonThanhToan()
        {
            _ui.OpenShopPopup();
            ServerSendsMenu(ShopPopupView.ShopWeapon,
                TestPackets.ShopItem(10, "búa gỗ", "mô tả", "20 (vang)"));
            _sent.Clear();

            _ui.ShopPopup.TryBuy(0);

            Assert.AreEqual(0, _sent.Count, "phải hỏi xác nhận trước, chưa được gửi gì");
            Assert.AreEqual(2, _ui.Stack.Depth, "hộp xác nhận nằm trên popup");

            ((ChoiceDialogView)_ui.Current).Choose(0);

            Assert.AreEqual(1, _sent.Count, "đồng ý rồi mới gửi lệnh mua");
            var wire = _sent[0].ToWire();
            Assert.AreEqual(GopetCmd.SELECT_MENU_ELEMENT, unchecked((sbyte)wire[0]));
        }

        [Test]
        public void KhongDuTien_BaoTaiCho_KhongGuiLenServer()
        {
            // Nhánh "thiếu tiền" của server (selectMenu.cs:797-823) bỏ sót các loại
            // tiền sự kiện như điểm hoa ngọc, nên gửi lên là im lặng tuyệt đối.
            _ui.OpenShopPopup();
            ServerSendsMenu(ShopPopupView.ShopWeapon,
                TestPackets.ShopItem(10, "búa gỗ", "mô tả", "100 điểm hoa ngọc", affordable: 0));
            _sent.Clear();

            string toast = null;
            _ui.ShopPopup.Message += text => toast = text;
            _ui.ShopPopup.TryBuy(0);

            Assert.AreEqual(0, _sent.Count, "thiếu tiền thì không gửi gì lên server");
            Assert.AreEqual(1, _ui.Stack.Depth, "cũng không mở hộp xác nhận");
            Assert.AreEqual("Bạn cần 100 điểm hoa ngọc để thực hiện giao dịch", toast,
                "phải nói rõ cần bao nhiêu và loại tiền nào");
        }
    }
}
