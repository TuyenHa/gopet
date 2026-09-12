using Gopet.Net.Chat;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class GameHudStatusTests
    {
        private GameObject _host;
        private GameHud _hud;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("GameHudStatusHost");
            _hud = GameHud.Create(_host.transform, new ChatHandler(_ => { }));
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        [Test]
        public void PlaceTime_FormatsMinutesAndSeconds()
        {
            _hud.ShowPlaceTime(125);

            Assert.AreEqual("02:05", _hud.PlaceTimeText);
        }

        [Test]
        public void BigText_IsVisibleImmediately()
        {
            _hud.ShowBigText("Combo x3");

            Assert.AreEqual("Combo x3", _hud.BigText);
            Assert.IsTrue(_hud.transform.Find("Big Text Effect").gameObject.activeSelf);
        }

        [Test]
        public void NotificationTicker_IsHiddenUntilServerBannerArrives()
        {
            var router = new MessageRouter();
            var guider = new GuiderHandler(_ => { });
            guider.RegisterOn(router);
            guider.BossBannerShown += _hud.Ticker.Show;

            Assert.IsFalse(_hud.Ticker.gameObject.activeSelf);

            using var banner = Message.Create(GopetCmd.SERVER_MESSAGE)
                .PutSByte(GopetCmd.BOSS_BANNER_MESSAGE)
                .PutUtf("Sự kiện mới từ backend");
            router.Dispatch(Message.FromWire(banner.ToWire(), false));

            Assert.IsTrue(_hud.Ticker.gameObject.activeSelf);
        }

        [Test]
        public void NotificationTicker_IgnoresEmptyServerBanner()
        {
            _hud.Ticker.Show("   ");

            Assert.IsFalse(_hud.Ticker.gameObject.activeSelf);
        }
    }
}
