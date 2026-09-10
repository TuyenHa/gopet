using System;
using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Bấm qua ĐÚNG đường con trỏ của uGUI, không gọi tắt vào <c>OnRowClicked</c>.
    ///
    /// <para><b>Vì sao cần:</b> mọi PlayMode test khác gọi thẳng phương thức xử lý,
    /// nên nhánh "người dùng thật sự chạm vào" chưa bao giờ chạy. Với cách đó, một
    /// dòng rộng 0 pixel hay một nút bị vô hiệu vẫn cho test xanh — trong khi trên
    /// máy thật thì không bấm được gì. Đã suýt để lọt đúng lỗi đó: dòng đặt
    /// <c>sizeDelta.x = 0</c> với anchor mặc định là rộng đúng 0.</para>
    ///
    /// <para>Dùng <see cref="ExecuteEvents"/> thay vì mô phỏng chuột: nó đi qua
    /// <c>Button</c> thật và <b>tôn trọng cờ <c>interactable</c></b>, mà lại không
    /// cần raycaster hay con trỏ vật lý — chạy được trong <c>-nographics</c>.</para>
    /// </summary>
    public sealed class PointerPathTests
    {
        private GameObject _canvas;
        private GameObject _eventSystem;
        private List<Message> _sent;
        private GuiderHandler _guider;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var rect = (RectTransform)_canvas.transform;
            rect.sizeDelta = new Vector2(800f, 600f);

            _eventSystem = new GameObject("EventSystem", typeof(EventSystem));

            _sent = new List<Message>();
            _guider = new GuiderHandler(_sent.Add);
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) UnityEngine.Object.DestroyImmediate(_canvas);
            if (_eventSystem != null) UnityEngine.Object.DestroyImmediate(_eventSystem);
        }

        private static MenuItemInfo Item(int itemId, string title, bool canSelect)
        {
            return new MenuItemInfo
            {
                ItemId = itemId,
                ImagePath = "icon.png",
                Title = title,
                Description = "mô tả",
                CanSelect = canSelect,
                ShowDialog = false,
                PaymentOptions = Array.Empty<MenuItemInfo.PaymentOption>()
            };
        }

        private GenericMenuView BuildMenu(params MenuItemInfo[] items)
        {
            var view = GenericMenuView.Create(_canvas.transform, null);
            view.Bind(new MenuScreen { ListId = 1040, Type = 0, Title = "Thử", Items = items }, null, _guider);
            return view;
        }

        /// <summary>Bấm như người dùng: qua Button thật, tôn trọng cờ interactable.</summary>
        private static void ClickThroughPointer(GameObject target)
        {
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
        }

        [Test]
        public void DongCoChieuRongThat_KhongPhaiZero()
        {
            // Dòng rộng 0 thì trên máy thật không bấm vào đâu được, dù test gọi
            // thẳng OnRowClicked vẫn xanh.
            var view = BuildMenu(Item(10, "Kiếm", true));
            Canvas.ForceUpdateCanvases();

            var rect = (RectTransform)view.RowAt(0).transform;

            Assert.Greater(rect.rect.width, 1f, "dòng rộng 0 — không thể bấm trên máy thật");
            Assert.AreEqual(64f, rect.rect.height, 0.01f);
        }

        [Test]
        public void BamQuaConTro_GuiDungGoi()
        {
            var view = BuildMenu(Item(1001, "Kiếm", true), Item(1002, "Khiên", true));

            ClickThroughPointer(view.RowAt(1).gameObject);

            Assert.AreEqual(1, _sent.Count, "bấm qua con trỏ phải gửi gói");

            var wire = _sent[0].ToWire();
            var echo = (wire[6] << 24) | (wire[7] << 16) | (wire[8] << 8) | wire[9];
            Assert.AreEqual(1002, echo, "phải gửi ItemId của dòng đã bấm");
        }

        [Test]
        public void BamQuaConTroVaoDongBiChan_KhongGuiGi()
        {
            // Đây là điều `interactable = false` phải bảo đảm, và chỉ đường con trỏ
            // mới kiểm được: gọi thẳng OnRowClicked thì bỏ qua hoàn toàn cờ đó.
            var view = BuildMenu(Item(1001, "Không chọn được", canSelect: false));

            ClickThroughPointer(view.RowAt(0).gameObject);

            Assert.IsEmpty(_sent, "nút bị vô hiệu mà vẫn gửi gói");
        }

        [Test]
        public void BamQuaConTroVaoNutHopThoai()
        {
            var dialog = ChoiceDialogView.Create(_canvas.transform, null);
            dialog.Bind("Chắc chưa?", new[] { "Đồng ý", "Thôi" });

            var chosen = -1;
            dialog.Chosen += i => chosen = i;

            ClickThroughPointer(dialog.Buttons[1].gameObject);

            Assert.AreEqual(1, chosen, "bấm nút thứ hai phải ra lựa chọn thứ hai");
        }

        [Test]
        public void MoiDongOMotViTriKhacNhau_KhongChongLenNhau()
        {
            // Chồng lên nhau thì chỉ dòng trên cùng nhận được click, phần còn lại
            // im lặng — triệu chứng giống hệt "game treo".
            var view = BuildMenu(Item(1, "A", true), Item(2, "B", true), Item(3, "C", true));
            Canvas.ForceUpdateCanvases();

            var y0 = ((RectTransform)view.RowAt(0).transform).anchoredPosition.y;
            var y1 = ((RectTransform)view.RowAt(1).transform).anchoredPosition.y;
            var y2 = ((RectTransform)view.RowAt(2).transform).anchoredPosition.y;

            Assert.AreEqual(0f, y0, 0.01f);
            Assert.AreEqual(-64f, y1, 0.01f);
            Assert.AreEqual(-128f, y2, 0.01f);
        }

        [Test]
        public void BindManHinhKhac_DongCuKhongConGiuNoiDungCu()
        {
            // Refresh() giữ nguyên dòng đã dựng nếu chỉ số vẫn trong tầm nhìn —
            // đúng khi cuộn, nhưng sai khi đổi màn hình.
            var view = BuildMenu(Item(1, "Kiếm", true), Item(2, "Khiên", true));

            view.Bind(new MenuScreen
            {
                ListId = 1051,
                Items = new[] { Item(90, "Pet 1", true) }
            }, null, _guider);

            Assert.AreEqual(1, view.Rows.Count, "màn hình mới chỉ có 1 dòng");
            Assert.AreEqual("Pet 1", view.RowAt(0).Item.Title);
            Assert.IsNull(view.RowAt(1), "dòng của màn hình cũ phải được thu hồi");
        }
    }
}
