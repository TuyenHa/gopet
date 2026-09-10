using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class AnimationMenuUiTests
    {
        private GameObject _host;
        private MessageRouter _router;
        private List<Message> _sent;
        private UiRoot _ui;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("AnimationMenuUiHost", typeof(RectTransform));
            _router = new MessageRouter();
            _sent = new List<Message>();
            var guider = new GuiderHandler(_sent.Add);
            guider.RegisterOn(_router);
            var animationMenus = new AnimationMenuHandler();
            animationMenus.RegisterOn(_router);
            _ui = UiRoot.Create(_host.transform, null);
            _ui.Initialize(guider, null);
            _ui.InitializeAnimationMenus(animationMenus);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            foreach (var message in _sent) message.Dispose();
        }

        [Test]
        public void AchievementDetail_RendersAndCloseCommandClosesView()
        {
            Dispatch(m => m.PutSByte(0).PutInt(12).PutUtf("Chi tiết thành tựu")
                .PutInt(1)
                .PutSByte(0).PutBool(false).PutUtf("Đạt cấp 10").PutSByte(0).PutSByte(3)
                .PutInt(1)
                .PutInt(0).PutSByte(0).PutUtf("Đóng").PutBool(true).PutBool(false));

            Assert.IsInstanceOf<AnimationMenuView>(_ui.Current);
            var labels = ((AnimationMenuView)_ui.Current).GetComponentsInChildren<Text>();
            Assert.IsTrue(System.Array.Exists(labels, value => value.text == "Đạt cấp 10"));

            var button = ((AnimationMenuView)_ui.Current).GetComponentInChildren<Button>();
            Assert.IsNotNull(button);
            button.onClick.Invoke();
            Assert.IsNull(_ui.Current);
        }

        private void Dispatch(System.Action<Message> body)
        {
            using var message = Message.Create(GopetCmd.PET_SERVICE).PutSByte(GopetCmd.ANIMATION_MENU);
            body(message);
            _router.Dispatch(Message.FromWire(message.ToWire(), false));
        }
    }
}
