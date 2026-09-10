using Gopet.Net.Chat;
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
    }
}
