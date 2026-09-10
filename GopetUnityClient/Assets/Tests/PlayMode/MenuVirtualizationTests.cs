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
    /// Virtualization ở tầng view: chỉ dựng dòng đang nhìn thấy, và TÁI DÙNG
    /// GameObject khi cuộn.
    ///
    /// <para>Phép toán khoảng dòng đã test offline ở <c>MenuVirtualizerTests</c>.
    /// Còn lại đây kiểm phần chỉ Unity mới thấy: GameObject có thật sự được thu hồi
    /// hay mỗi lần cuộn lại sinh thêm.</para>
    /// </summary>
    public sealed class MenuVirtualizationTests
    {
        private const float RowHeight = 64f;      // MenuItemRow.Height
        private const float Viewport = 640f;      // 10 dòng

        private GameObject _root;
        private GuiderHandler _guider;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot", typeof(RectTransform));
            _guider = new GuiderHandler(m => m.Dispose());
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null) UnityEngine.Object.DestroyImmediate(_root);
        }

        private static MenuScreen BigScreen(int count)
        {
            var items = new MenuItemInfo[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = new MenuItemInfo
                {
                    ItemId = i,
                    ImagePath = "icon.png",
                    Title = "Dòng " + i,
                    Description = "mô tả",
                    CanSelect = true,
                    ShowDialog = false,
                    PaymentOptions = Array.Empty<MenuItemInfo.PaymentOption>()
                };
            }

            return new MenuScreen { ListId = 1040, Type = 0, Title = "Dài", Items = items };
        }

        private GenericMenuView Bind(int itemCount, float viewport = Viewport)
        {
            var view = GenericMenuView.Create(_root.transform, null);
            view.Bind(BigScreen(itemCount), null, _guider);
            view.SetViewport(viewport);
            return view;
        }

        private int LiveRowObjects(GenericMenuView view)
        {
            // Đếm cả dòng đang tắt: đó chính là những cái nằm trong hồ tái dùng.
            return view.GetComponentsInChildren<MenuItemRow>(includeInactive: true).Length;
        }

        [Test]
        public void DanhSach500Dong_ChiDungChucCai()
        {
            // Lý do tồn tại của virtualization. Không có nó là 500 GameObject cho
            // một màn hình mắt chỉ thấy 10 dòng.
            var view = Bind(500);

            Assert.LessOrEqual(view.Rows.Count, 15, "dựng quá nhiều dòng cho vùng nhìn 10 dòng");
            Assert.Greater(view.Rows.Count, 0);
        }

        [Test]
        public void KhongDatVungNhin_DungHetNhuCu()
        {
            // Tương thích ngược: menu ngắn và test không dựng layout vẫn chạy như trước.
            var view = GenericMenuView.Create(_root.transform, null);
            view.Bind(BigScreen(7), null, _guider);

            Assert.AreEqual(7, view.Rows.Count);
        }

        [Test]
        public void CuonXuong_TaiDungGameObjectChuKhongSinhThem()
        {
            var view = Bind(500);
            var before = LiveRowObjects(view);

            for (var step = 1; step <= 20; step++)
            {
                view.SetScroll(step * 10 * RowHeight);
            }

            var after = LiveRowObjects(view);

            Assert.AreEqual(before, after,
                $"cuộn 20 lần sinh thêm {after - before} GameObject — hồ tái dùng không hoạt động");
        }

        [Test]
        public void CuonToiDau_ThiDungDongODo()
        {
            var view = Bind(500);

            view.SetScroll(100 * RowHeight);

            Assert.IsNotNull(view.RowAt(100), "dòng 100 phải được dựng khi cuộn tới đó");
            Assert.IsNull(view.RowAt(0), "dòng 0 đã ra khỏi tầm nhìn, phải được thu hồi");
            Assert.AreEqual("Dòng 100", view.RowAt(100).Item.Title);
        }

        [Test]
        public void DongTaiDung_MangDuLieuMoiChuKhongGiuDuLieuCu()
        {
            // Bug kinh điển của việc tái dùng: GameObject đổi chỗ nhưng nội dung
            // vẫn của dòng cũ.
            var view = Bind(500);
            view.SetScroll(200 * RowHeight);

            var row = view.RowAt(200);

            Assert.IsNotNull(row);
            Assert.AreEqual("Dòng 200", row.Item.Title);
            Assert.AreEqual(200, row.Index);
        }

        [Test]
        public void CuonVeDinh_LaiDungDongDau()
        {
            var view = Bind(500);
            view.SetScroll(300 * RowHeight);
            view.SetScroll(0f);

            Assert.IsNotNull(view.RowAt(0));
            Assert.AreEqual("Dòng 0", view.RowAt(0).Item.Title);
        }

        [Test]
        public void BamDongOXaVanGuiDungChiSo()
        {
            // Chỉ số phải là vị trí TRONG DANH SÁCH, không phải thứ tự GameObject.
            var sent = new List<Message>();
            var guider = new GuiderHandler(sent.Add);

            var view = GenericMenuView.Create(_root.transform, null);
            view.Bind(BigScreen(500), null, guider);
            view.SetViewport(Viewport);
            view.SetScroll(150 * RowHeight);

            view.OnRowClicked(150);

            Assert.AreEqual(1, sent.Count);
            var wire = sent[0].ToWire();
            Assert.AreEqual(150, (wire[6] << 24) | (wire[7] << 16) | (wire[8] << 8) | wire[9]);
        }

        [Test]
        public void InteractiveScroll_CuonThatVaChiDungDongTrongTamNhin()
        {
            var view = GenericMenuView.Create(_root.transform, null);
            view.Bind(BigScreen(500), null, _guider);
            view.EnableInteractiveScroll(Viewport);
            var scroll = view.GetComponentInChildren<ScrollRect>();

            Assert.IsNotNull(scroll);
            Assert.AreEqual(500 * RowHeight, scroll.content.sizeDelta.y);
            Assert.LessOrEqual(view.Rows.Count, 15);

            scroll.content.anchoredPosition = new Vector2(0f, 100 * RowHeight);
            scroll.onValueChanged.Invoke(Vector2.zero);

            Assert.IsNotNull(view.RowAt(100));
            Assert.IsNull(view.RowAt(0));
        }
    }
}
