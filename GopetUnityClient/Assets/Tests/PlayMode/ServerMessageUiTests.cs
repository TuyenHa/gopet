using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class ServerMessageUiTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private List<Message> _sent;
        private UiRoot _ui;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("ServerMessageUiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            var guider = new GuiderHandler(_sent.Add);
            guider.RegisterOn(_router);
            _ui = UiRoot.Create(_host.transform, null);
            _ui.Initialize(guider, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            foreach (var message in _sent) message.Dispose();
        }

        [Test]
        public void PopupMessage_CreatesVisibleToast()
        {
            using var message = Message.Create(GopetCmd.SERVER_MESSAGE)
                .PutSByte(GopetCmd.POPUP_MESSAGE).PutUtf("Không đủ năng lượng");

            _router.Dispatch(Message.FromWire(message.ToWire(), false));

            var toast = _host.GetComponentInChildren<ToastView>();
            Assert.IsNotNull(toast);
        }

        [Test]
        public void ImageDialog_ConfirmSendsSelectionAndCloses()
        {
            using var message = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.GUIDER_IMGDIALOG)
                .PutInt(0).PutInt(160).PutInt(80)
                .PutUtf("img/captcha.png123456").PutInt(1).PutInt(0);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));

            var view = _ui.Current as ImageDialogView;
            Assert.IsNotNull(view);
            Assert.AreEqual("img/captcha.png123456", view.ImagePath);

            view.Confirm();

            Assert.AreEqual(0, _ui.Stack.Depth);
            Assert.AreEqual(1, _sent.Count);
            Assert.AreEqual("7a0b00000000", Hex(_sent[0].ToWire()));
        }

        [Test]
        public void ServerMessages_AreNonBlockingToasts()
        {
            _ui.ShowServerError("Thiếu tiền");
            _ui.ShowServerSuccess("Thành công");

            Assert.IsNull(_ui.Current);
            Assert.AreEqual(2, _host.GetComponentsInChildren<ToastView>().Length);
        }

        private static string Hex(byte[] bytes)
        {
            return System.BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
