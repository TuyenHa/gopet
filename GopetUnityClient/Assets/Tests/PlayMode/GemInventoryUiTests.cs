using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class GemInventoryUiTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private List<Message> _sent;
        private UiRoot _ui;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("GemInventoryUiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            var guider = new GuiderHandler(_sent.Add);
            guider.RegisterOn(_router);
            var gems = new GemHandler(_sent.Add);
            gems.RegisterOn(_router);
            _ui = UiRoot.Create(_host.transform, null);
            _ui.Initialize(guider, null);
            _ui.InitializeGems(gems);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            foreach (var message in _sent) message.Dispose();
        }

        [Test]
        public void InventoryUpdateAndRemove_StayInOneView()
        {
            Dispatch(GopetCmd.SHOW_GEM_INVENTORY, m => m.PutInt(1)
                .PutInt(10).PutInt(301).PutUtf("Hồng ngọc").PutInt(1));
            var view = _ui.Current as GemInventoryView;
            Assert.IsNotNull(view);
            Assert.AreEqual(1, view.ItemCount);

            Dispatch(GopetCmd.SEND_GEM_INFo, m => m
                .PutInt(10).PutUtf("gem/red.png").PutUtf("Hồng ngọc +2").PutSByte(2));
            Assert.AreEqual(1, view.ItemCount);

            Dispatch(GopetCmd.REMOVE_GEM_ITEM, m => m.PutInt(10));
            Assert.AreEqual(0, view.ItemCount);
        }

        private void Dispatch(sbyte sub, System.Action<Message> body)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);
            body(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
