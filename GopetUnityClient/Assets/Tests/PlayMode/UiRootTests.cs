using System;
using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nghiệm thu trọn đường: byte của server → router → view trên màn hình → gói
    /// gửi ngược lại.
    ///
    /// <para>Gói được dựng đúng như server dựng rồi dispatch qua
    /// <see cref="MessageRouter"/> thật, không gọi tắt vào sự kiện — nếu không thì
    /// test sẽ bỏ qua đúng phần dễ sai nhất.</para>
    /// </summary>
    public sealed class UiRootTests
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

        // --- Test ---

        [Test]
        public void ServerGuiMenu_ThiManHinhHienRa()
        {
            ServerSendsMenu(1040, TestPackets.Item(10, "Kiếm"));

            Assert.AreEqual(1, _ui.Stack.Depth);
            Assert.IsInstanceOf<GenericMenuView>(_ui.Current);
        }

        [Test]
        public void UiRoot_LuonNamTrenCanvasBattle()
        {
            var canvas = _ui.GetComponent<Canvas>();
            Assert.IsTrue(canvas.overrideSorting);
            Assert.AreEqual(UiRoot.SortingOrder, canvas.sortingOrder);
            Assert.Greater(canvas.sortingOrder, 40,
                "Menu vật phẩm do server gửi phải nằm trên màn battle.");
        }

        [Test]
        public void DongCanXacNhan_HopThoaiDeLENmenu_KhongThayThe()
        {
            ServerSendsMenu(1040, TestPackets.Item(10, "Khiên", showDialog: true));
            var menu = (GenericMenuView)_ui.Current;

            menu.OnRowClicked(0);

            Assert.AreEqual(2, _ui.Stack.Depth, "hộp xác nhận phải nằm TRÊN menu");
            Assert.IsInstanceOf<ChoiceDialogView>(_ui.Current);
            Assert.IsEmpty(_sent, "chưa đồng ý thì chưa gửi gì");
        }

        [Test]
        public void HuyXacNhan_QuayLaiDungMenuDangXem()
        {
            ServerSendsMenu(1040, TestPackets.Item(10, "Khiên", showDialog: true));
            var menu = _ui.Current;
            ((GenericMenuView)menu).OnRowClicked(0);

            ((ChoiceDialogView)_ui.Current).Choose(1);   // nút "Thôi"

            Assert.AreEqual(1, _ui.Stack.Depth);
            Assert.AreSame(menu, _ui.Current);
            Assert.IsEmpty(_sent);
        }

        [Test]
        public void DongYXacNhan_GuiDungGoiVaDongHopThoai()
        {
            ServerSendsMenu(1040, TestPackets.Item(10, "Khiên", showDialog: true));
            ((GenericMenuView)_ui.Current).OnRowClicked(0);

            ((ChoiceDialogView)_ui.Current).Choose(0);   // nút "Đồng ý"

            Assert.AreEqual(1, _sent.Count);
            var wire = _sent[0].ToWire();
            Assert.AreEqual(GopetCmd.SELECT_MENU_ELEMENT, unchecked((sbyte)wire[1]));
            Assert.AreEqual(1040, TestPackets.ReadInt(wire, 2));
            Assert.AreEqual(1, _ui.Stack.Depth, "hộp thoại phải đóng sau khi đồng ý");
        }

        [Test]
        public void CloseScreenAfterClick_DongMenuOGIUAChongManHinh()
        {
            // Ca khó: menu tự đóng sau khi chọn, trong khi hộp xác nhận vẫn đang
            // nằm đè lên trên. Không xử lý được thì hoặc kẹt một lớp che, hoặc
            // đóng nhầm cái đang hiện.
            ServerSendsMenu(1040, TestPackets.Item(10, "Mua", showDialog: true, closeAfter: true));
            ((GenericMenuView)_ui.Current).OnRowClicked(0);

            Assert.AreEqual(2, _ui.Stack.Depth);

            ((ChoiceDialogView)_ui.Current).Choose(0);

            Assert.AreEqual(1, _sent.Count);
            Assert.AreEqual(0, _ui.Stack.Depth, "cả hộp thoại lẫn menu đều phải đóng");
            Assert.IsNull(_ui.Current);
        }

        [Test]
        public void Back_DongTheoDungThuTuNguoc()
        {
            ServerSendsMenu(1, TestPackets.Item(1, "A"));
            ServerSendsMenu(2, TestPackets.Item(2, "B"));

            Assert.AreEqual(2, _ui.Stack.Depth);

            Assert.IsTrue(_ui.Back());
            Assert.AreEqual(1, _ui.Stack.Depth);

            Assert.IsTrue(_ui.Back());
            Assert.AreEqual(0, _ui.Stack.Depth);

            Assert.IsFalse(_ui.Back(), "hết màn hình thì trả false, không được ném");
        }

        [Test]
        public void ChiManHinhTrenCungDuocHien()
        {
            ServerSendsMenu(1, TestPackets.Item(1, "A"));
            var duoi = (GenericMenuView)_ui.Current;

            ServerSendsMenu(2, TestPackets.Item(2, "B"));
            var tren = (GenericMenuView)_ui.Current;

            Assert.IsFalse(duoi.gameObject.activeSelf, "màn hình dưới phải ẩn");
            Assert.IsTrue(tren.gameObject.activeSelf);

            _ui.Back();

            Assert.IsTrue(duoi.gameObject.activeSelf, "đóng cái trên thì cái dưới hiện lại");
        }

        [Test]
        public void NhieuManHinhKhacNhau_KhongCanCodeRiengChoTungListId()
        {
            // Nguyên tắc cốt lõi của phase: shop, kho đồ, nhiệm vụ đều đi qua đúng
            // một đường. Nếu ai đó thêm switch (listId) thì test này vẫn xanh —
            // nhưng nó ghi lại rằng thiết kế hiện tại KHÔNG cần nhánh nào.
            foreach (var listId in new[] { 1, 3, 1034, 1040, 1051 })
            {
                ServerSendsMenu(listId, TestPackets.Item(1, "dòng"));
                Assert.IsInstanceOf<GenericMenuView>(_ui.Current, $"màn hình {listId}");
                _ui.Back();
            }

            Assert.AreEqual(0, _ui.Stack.Depth);
        }
    }
}
