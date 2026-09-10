using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Net.Player;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class WingInventoryUiTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private List<Message> _sent;
        private UiRoot _ui;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("WingUiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            var guider = new GuiderHandler(_sent.Add);
            guider.RegisterOn(_router);
            _ui = UiRoot.Create(_host.transform, null);
            _ui.Initialize(guider, null);
            _ui.InitializeWings(new WingHandler(_sent.Add));
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            foreach (var message in _sent) message.Dispose();
        }

        [Test]
        public void InventoryItem_EnchantActionUsesWingSubcommand()
        {
            TestPackets.Dispatch(_router, TestPackets.MenuWire(81040,
                TestPackets.Item(3, "Cánh thiên thần")));
            ((GenericMenuView)_ui.Current).OnRowClicked(0);
            ((ChoiceDialogView)_ui.Current).Choose(1);

            Assert.AreEqual(1, _sent.Count);
            var round = Message.FromWire(_sent[0].ToWire(), false);
            Assert.AreEqual(GopetCmd.WING, round.Reader.ReadSByte());
            Assert.AreEqual(GopetCmd.WING_TYPE_ENCHANT, round.Reader.ReadSByte());
            Assert.AreEqual(3, round.Reader.ReadInt());
        }

        [Test]
        public void EquippedItem_FirstActionUnequips()
        {
            TestPackets.Dispatch(_router, TestPackets.MenuWire(81040,
                TestPackets.Item(-1, "Cánh đang dùng")));
            ((GenericMenuView)_ui.Current).OnRowClicked(0);
            ((ChoiceDialogView)_ui.Current).Choose(0);

            var round = Message.FromWire(_sent[0].ToWire(), false);
            Assert.AreEqual(GopetCmd.WING, round.Reader.ReadSByte());
            Assert.AreEqual(GopetCmd.WING_TYPE_UNEQUIP, round.Reader.ReadSByte());
        }
    }
}
