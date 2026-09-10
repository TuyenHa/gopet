using System;
using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nghiệm thu <see cref="GenericMenuView"/> bằng máy, không phải bằng mắt.
    ///
    /// <para>Chạy được từ dòng lệnh:
    /// <c>Unity.exe -batchmode -runTests -testPlatform PlayMode</c>, nên UI không
    /// còn là phần "viết xong rồi hy vọng".</para>
    ///
    /// <para>Không dùng prefab và không dùng font: mọi thứ dựng bằng code để test
    /// chạy được trong <c>-nographics</c> và không phụ thuộc asset nào.</para>
    /// </summary>
    public sealed class GenericMenuViewTests
    {
        private GameObject _root;
        private List<Message> _sent;
        private GuiderHandler _guider;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot", typeof(RectTransform));
            _sent = new List<Message>();
            _guider = new GuiderHandler(_sent.Add);
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
        }



        private GenericMenuView Bind(MenuScreen screen)
        {
            var view = GenericMenuView.Create(_root.transform, null);
            view.Bind(screen, null, _guider);
            return view;
        }

        [Test]
        public void DungDungSoDongTheoDanhSachServerGui()
        {
            var view = Bind(TestPackets.Screen(1040, TestPackets.Item(0, "Kiếm"), TestPackets.Item(0, "Khiên"), TestPackets.Item(0, "Giáp")));

            Assert.AreEqual(3, view.Rows.Count);
            Assert.AreEqual("Kiếm", view.RowAt(0).Item.Title);
            Assert.AreEqual("Giáp", view.RowAt(2).Item.Title);
        }

        [Test]
        public void MenuRong_KhongDungDongNao_VaKhongNem()
        {
            // Cửa hàng chưa có hàng là chuyện thật: bốn gói SHOW_MENU_ITEM bắt được
            // từ client J2ME đều có 0 dòng.
            var view = Bind(TestPackets.Screen(1));

            Assert.IsEmpty(view.Rows);
        }

        [Test]
        public void DongKhongChoChon_KhongBamDuoc()
        {
            var view = Bind(TestPackets.Screen(1040, TestPackets.Item(0, "Được"), TestPackets.Item(0, "Không", canSelect: false)));

            Assert.IsTrue(view.RowAt(0).Interactable);
            Assert.IsFalse(view.RowAt(1).Interactable, "dòng canSelect = 0 phải tắt nút, không chỉ làm mờ");
        }

        [Test]
        public void BamDongKhongChoChon_KhongGuiGoiNao()
        {
            var view = Bind(TestPackets.Screen(1040, TestPackets.Item(0, "Không", canSelect: false)));

            var blocked = -1;
            view.BlockedRowTapped += i => blocked = i;

            view.OnRowClicked(0);

            Assert.AreEqual(0, blocked);
            Assert.IsEmpty(_sent, "không được gửi gì cho dòng bị chặn");
        }

        [Test]
        public void BamDongThuong_GuiListIdVaItemId()
        {
            // Giá trị gửi lên là ItemId server đã gửi xuống, KHÔNG phải vị trí dòng.
            // Đặt id lệch hẳn vị trí để test phân biệt được hai cách — bản trước
            // của test này dùng id = 0 cho cả ba dòng nên không phân biệt nổi.
            var view = Bind(TestPackets.Screen(1040,
                TestPackets.Item(1001, "A"), TestPackets.Item(1002, "B"), TestPackets.Item(1003, "C")));

            view.OnRowClicked(2);

            Assert.AreEqual(1, _sent.Count);
            Assert.AreEqual(GopetCmd.COMMAND_GUIDER, _sent[0].Id);

            // 7a 03 <listId> <itemId>
            var wire = _sent[0].ToWire();
            Assert.AreEqual(GopetCmd.SELECT_MENU_ELEMENT, unchecked((sbyte)wire[1]));
            Assert.AreEqual(1040, TestPackets.ReadInt(wire, 2));
            Assert.AreEqual(1003, TestPackets.ReadInt(wire, 6));
        }

        [Test]
        public void DongCoShowDialog_HoiTruoc_ChuaGuiGiCa()
        {
            var view = Bind(TestPackets.Screen(1040, TestPackets.Item(0, "Khiên", showDialog: true)));

            MenuSelection.ConfirmPrompt prompt = null;
            view.ConfirmRequested += (p, _) => prompt = p;

            view.OnRowClicked(0);

            Assert.IsNotNull(prompt);
            Assert.AreEqual("Mua Khiên?", prompt.Text);
            Assert.AreEqual("Đồng ý", prompt.ConfirmLabel, "nhãn nút phải lấy từ server, không hardcode");
            Assert.AreEqual("Thôi", prompt.CancelLabel);
            Assert.IsEmpty(_sent, "chưa đồng ý thì chưa được gửi");
        }

        [Test]
        public void DongY_ThiMoiGui()
        {
            var view = Bind(TestPackets.Screen(1040, TestPackets.Item(0, "Khiên", showDialog: true)));

            Action confirm = null;
            view.ConfirmRequested += (_, onYes) => confirm = onYes;

            view.OnRowClicked(0);
            Assert.IsEmpty(_sent);

            confirm();

            Assert.AreEqual(1, _sent.Count);
        }

        [Test]
        public void CloseScreenAfterClick_BaoDongManHinh()
        {
            var view = Bind(TestPackets.Screen(1040, TestPackets.Item(0, "Thoát", closeAfter: true), TestPackets.Item(0, "Ở lại")));

            GenericMenuView closed = null;
            view.CloseRequested += v => closed = v;

            view.OnRowClicked(1);
            Assert.IsNull(closed, "dòng không có cờ thì không được đóng màn hình");

            view.OnRowClicked(0);
            Assert.AreSame(view, closed);
        }

        [Test]
        public void HaiManHinhKhacNhau_CungMotDuongCode()
        {
            // Bằng chứng cho nguyên tắc cốt lõi của phase: không có nhánh riêng theo
            // listId. Shop và kho đồ chạy qua đúng cùng một view.
            var shop = Bind(TestPackets.Screen(1, TestPackets.Item(0, "Kiếm")));
            var inventory = Bind(TestPackets.Screen(1051, TestPackets.Item(0, "Pet 1"), TestPackets.Item(0, "Pet 2")));

            shop.OnRowClicked(0);
            inventory.OnRowClicked(1);

            Assert.AreEqual(2, _sent.Count);
            Assert.AreEqual(1, TestPackets.ReadInt(_sent[0].ToWire(), 2));
            Assert.AreEqual(1051, TestPackets.ReadInt(_sent[1].ToWire(), 2));
        }

    }
}
