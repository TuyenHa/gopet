using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Hai hộp thoại đi ngoài luồng menu: Có/Không (bao ngoài SERVER_MESSAGE, không
    /// phải COMMAND_GUIDER) và hộp nhập liệu.
    /// </summary>
    public sealed class UiRootDialogTests
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

        [Test]
        public void HopCoKhong_TraLoiDungTrongServerMessage()
        {
            TestPackets.Dispatch(_router, TestPackets.YesNoWire(77, "Xác nhận?"), GopetCmd.SERVER_MESSAGE);

            Assert.IsInstanceOf<ChoiceDialogView>(_ui.Current);
            Assert.AreEqual("Xác nhận?", ((ChoiceDialogView)_ui.Current).Message);

            ((ChoiceDialogView)_ui.Current).Choose(0);

            Assert.AreEqual(1, _sent.Count);
            Assert.AreEqual(GopetCmd.SERVER_MESSAGE, _sent[0].Id);

            var wire = _sent[0].ToWire();
            Assert.AreEqual(GopetCmd.SEND_YES_NO, unchecked((sbyte)wire[1]));
            Assert.AreEqual(77, TestPackets.ReadInt(wire, 2));
            Assert.AreEqual(1, wire[6], "đồng ý phải là bool true");
        }

        [Test]
        public void HopNhapLieu_GuiDungSoOServerDaHoi()
        {
            TestPackets.Dispatch(_router, TestPackets.InputWire(5, "Tên: ", "Số lượng: "));

            var view = (InputDialogView)_ui.Current;
            Assert.AreEqual(2, view.Fields.Count);

            view.Submit();

            Assert.AreEqual(1, _sent.Count);
            var wire = _sent[0].ToWire();
            Assert.AreEqual(GopetCmd.TYPE_DIALOG_INPUT, unchecked((sbyte)wire[1]));
            Assert.AreEqual(5, TestPackets.ReadInt(wire, 2));
            Assert.AreEqual(2, TestPackets.ReadInt(wire, 6), "phải gửi đúng số ô server đã hỏi");
        }

        [Test]
        public void ChamHaiNutCungLuc_ChiGuiMotGoi()
        {
            // Multi-touch chạm hai nút trong CÙNG một frame. Chặn ở tầng đóng màn
            // hình là quá muộn — cả hai đã kịp gửi.
            TestPackets.Dispatch(_router, TestPackets.YesNoWire(77, "Xác nhận?"), GopetCmd.SERVER_MESSAGE);
            var dialog = (ChoiceDialogView)_ui.Current;

            dialog.Choose(0);
            dialog.Choose(1);

            Assert.AreEqual(1, _sent.Count, "chạm hai nút cùng frame mà gửi hai gói");
        }

        [Test]
        public void KhoiTaoLanHai_Nem()
        {
            // Đăng ký trùng thì mỗi gói tin mở hai màn hình chồng lên nhau.
            Assert.Throws<System.InvalidOperationException>(() => _ui.Initialize(_guider, null));
        }
    }
}
