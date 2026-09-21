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
    /// Popup Dịch vụ: thân popup giữ được nền trắng qua các lần đổi màn, và menu quy
    /// đổi dựng thẻ sáng cùng kiểu cửa hàng.
    ///
    /// <para><b>Vì sao có file này:</b> <c>ClearBody</c> xoá sạch con của thân popup,
    /// mà nền trắng do <see cref="RoundedBorder"/> dựng LẠI LÀ MỘT CON — lần đổi màn
    /// đầu tiên đã ăn mất nền, chỉ còn trơ lớp viền, và vạch ngăn giữa các dòng (cùng
    /// màu viền) biến mất theo. Lỗi chỉ lộ ra bằng mắt.</para>
    /// </summary>
    public sealed class ServicePopupTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private GuiderHandler _guider;
        private List<Message> _sent;
        private AtmPopupView _view;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("UiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            _guider = new GuiderHandler(_sent.Add);
            _guider.RegisterOn(_router);

            _view = AtmPopupView.Create(_host.transform, UiBuilder.BuiltinFont(), _guider,
                null, () => { });
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        private Transform Body => _view.transform.Find("Content/Body");

        private static bool HasFill(Transform panel)
        {
            var fill = panel.Find("Fill");
            return fill != null && fill.GetComponent<Image>() != null;
        }

        [Test]
        public void DoiMan_ThanPopupVanConNenTrang()
        {
            Assert.IsTrue(HasFill(Body), "vừa dựng đã phải có nền");

            _view.RequestAtm();   // đi qua ClearBody
            _view.RequestAtm();

            Assert.IsTrue(HasFill(Body),
                "đổi màn không được xoá lớp nền của RoundedBorder — mất nền là vạch ngăn tàng hình");
        }

        [Test]
        public void MenuQuyDoi_DungTheSangCungKieuCuaHang()
        {
            var screen = TestPackets.Screen(AtmPopupView.ExchangeGoldMenuId,
                TestPackets.Item(1, "Đổi 2.400 (vang)"),
                TestPackets.Item(2, "Đổi 5.400 (vang)"));

            Assert.IsTrue(_view.TryConsumeMenu(screen));

            var list = _view.GetComponentInChildren<PopupItemList>();
            Assert.IsNotNull(list, "phải dùng PopupItemList, không phải GenericMenuView nền tối");
            Assert.AreEqual(2, list.RowCount);
            Assert.IsNull(_view.GetComponentInChildren<GenericMenuView>(),
                "không được còn menu generic nào trong popup");
        }

        [Test]
        public void ManCuoiKhongKeVachNgan()
        {
            var screen = TestPackets.Screen(AtmPopupView.ExchangeGoldMenuId,
                TestPackets.Item(1, "Đổi 2.400 (vang)"),
                TestPackets.Item(2, "Đổi 5.400 (vang)"));
            _view.TryConsumeMenu(screen);

            var rows = _view.GetComponentsInChildren<ShopItemRow>();
            Assert.AreEqual(2, rows.Length);
            Assert.IsTrue(rows[0].transform.Find("Separator").gameObject.activeSelf,
                "dòng giữa phải có vạch ngăn");
            Assert.IsFalse(rows[1].transform.Find("Separator").gameObject.activeSelf,
                "dòng cuối kẻ vạch là thừa một nét lửng");
        }
    }
}
