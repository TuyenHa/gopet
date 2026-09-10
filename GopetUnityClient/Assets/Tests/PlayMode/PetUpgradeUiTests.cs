using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class PetUpgradeUiTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private List<Message> _sent;
        private UiRoot _ui;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("PetUpgradeUiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            var guider = new GuiderHandler(_sent.Add);
            guider.RegisterOn(_router);
            var upgrade = new PetUpgradeHandler(_sent.Add);
            upgrade.RegisterOn(_router);
            _ui = UiRoot.Create(_host.transform, null);
            _ui.Initialize(guider, null);
            _ui.InitializePetUpgrade(upgrade);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            foreach (var message in _sent) message.Dispose();
        }

        [Test]
        public void ServerFlow_OpensUpdatesAndClosesUpgradeView()
        {
            Dispatch(GopetCmd.SHOW_UPGRADE_PET, _ => { });
            var view = _ui.Current as PetUpgradeView;
            Assert.IsNotNull(view);
            Assert.AreEqual(GopetCmd.PRICE_UPGRADE_PET,
                Message.FromWire(_sent[0].ToWire(), true).Reader.ReadSByte());

            Dispatch(GopetCmd.PET_UPGRADE_PET_INFO, m => m
                .PutSByte(GopetCmd.PET_UPGRADE_ACTIVE).PutInt(11).PutUtf("pet/a.png").PutSByte(0));
            Dispatch(GopetCmd.PET_UPGRADE_PET_INFO, m => m
                .PutSByte(GopetCmd.PET_UPGRADE_PASSIVE).PutInt(12).PutUtf("pet/b.png").PutSByte(0));
            Dispatch(GopetCmd.INFO_UP_TIER_PET, m => m
                .PutUtf("Thần Long").PutSByte(1).PutUtf("Chỉ số mới"));

            Assert.AreEqual(11, view.ActivePetId);
            Assert.AreEqual(12, view.MaterialPetId);
            StringAssert.Contains("Thần Long", view.PreviewText);

            Dispatch(GopetCmd.PET_UP_TIER, _ => { });
            Assert.IsFalse(_ui.Current is PetUpgradeView);
        }

        private void Dispatch(sbyte sub, System.Action<Message> write)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub);
            write(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
