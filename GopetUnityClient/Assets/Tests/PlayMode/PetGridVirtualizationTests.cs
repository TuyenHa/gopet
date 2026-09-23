using System;
using System.Linq;
using System.Collections;
using UnityEngine.TestTools;
using System.IO;
using Gopet.Runtime;
using Gopet.Runtime.Assets;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class PetGridVirtualizationTests
    {
        private GameObject _root;
        private PetGridView _view;
        private MenuScreen _screen;
        private RemoteAssetCache _cache;
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _cache = null;
            _directory = null;
            _root = new GameObject("Root", typeof(RectTransform));
            ((RectTransform)_root.transform).sizeDelta = new Vector2(600, 230);
            _view = PetGridView.Create(_root.transform, null);
            _screen = new MenuScreen { ListId = 1040, Items = Enumerable.Range(0, 200)
                .Select(i => new MenuItemInfo { Title = "Pet " + i, CanSelect = true,
                    Description = "2(str) 3(agi)", PaymentOptions = Array.Empty<MenuItemInfo.PaymentOption>() }).ToArray() };
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_view != null) _view.Bind(null, null, null);
            _cache?.Dispose();
            UnityEngine.Object.DestroyImmediate(_root);
            if (_cache == null) yield break;
            var completion = _cache.DiskCompletion;
            var deadline = Time.realtimeSinceStartup + 10f;
            while (!completion.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            var directory = _directory;
            if (!completion.IsCompleted)
            {
                // Timeout still leaves cleanup ordered after the worker, without Unity calls.
                completion.ContinueWith(_ => DeleteDirectory(directory));
                Assert.Fail("Asset disk worker did not stop within ten seconds.");
            }
            DeleteDirectory(directory);
            _cache = null;
            _directory = null;
        }

        private static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        private static IEnumerator WaitForDisk(RemoteAssetCache cache)
        {
            var completion = cache.DiskCompletion;
            var deadline = Time.realtimeSinceStartup + 10f;
            while (!completion.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(completion.IsCompleted, "Asset disk worker timed out.");
        }

        private Transform Content => _view.GetComponent<ScrollRect>().content;
        private Transform Row(int index) => Content.Find("PetCard_" + index);
        private void Scroll(int index)
        {
            var scroll = _view.GetComponent<ScrollRect>();
            scroll.content.anchoredPosition = new Vector2(0, index * 46);
            scroll.onValueChanged.Invoke(Vector2.zero);
        }

        [TestCase(PetGridMode.Receive)]
        [TestCase(PetGridMode.Shop)]
        [TestCase(PetGridMode.Top)]
        public void LargeListsKeepBoundedReusableRowTrees(PetGridMode mode)
        {
            _view.Bind(_screen, null, null, "Buy", mode);
            Assert.That(Content.childCount, Is.InRange(1, 10));
            Scroll(30); // Warm the symmetric overscan capacity.
            var transforms = Content.GetComponentsInChildren<Transform>(true);
            for (var i = 50; i < 180; i += 10) Scroll(i);
            CollectionAssert.AreEquivalent(transforms,
                Content.GetComponentsInChildren<Transform>(true));
            Assert.AreEqual("Pet 170", Row(170).Find("Title").GetComponent<Text>().text);
        }

        [Test]
        public void RecycledActionSendsAbsoluteIndexOnce()
        {
            var count = 0;
            var selected = -1;
            var guider = new GuiderHandler(message =>
            {
                var wire = message.ToWire();
                selected = (wire[6] << 24) | (wire[7] << 16) | (wire[8] << 8) | wire[9];
                count++;
                message.Dispose();
            });
            _view.Bind(_screen, null, guider);
            Scroll(150);
            Row(150).Find("Action").GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(150, selected);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void RebindModesAndNullClearExistingRowsAndScroll()
        {
            _view.Bind(_screen, null, null);
            Scroll(100);
            _view.Bind(_screen, null, null, "Buy", PetGridMode.Top);
            Assert.IsFalse(Row(0).Find("Action").gameObject.activeSelf);
            Assert.IsFalse(Row(0).Find("strength-v2Text").gameObject.activeSelf);
            Assert.IsTrue(Row(0).Find("hp-v2Text").gameObject.activeSelf);
            _view.Bind(_screen, null, null, "Buy", PetGridMode.Shop);
            Assert.IsTrue(Row(0).Find("Action").gameObject.activeSelf);
            Assert.AreEqual("Buy", Row(0).Find("Action/Label").GetComponent<Text>().text);
            _view.Bind(null, null, null);
            Assert.AreEqual(0, Content.GetComponentsInChildren<Button>().Length);
            Assert.AreEqual(Vector2.zero, ((RectTransform)Content).anchoredPosition);
        }

        [UnityTest]
        public IEnumerator LateImageFromFormerBindingCannotReplaceCurrentIcon()
        {
            var client = _root.AddComponent<GopetClient>();
            client.enabled = false;
            _directory = Path.Combine(Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            _cache = new RemoteAssetCache(client, client.Router, _directory);
            var cache = _cache;
            var texture = new Texture2D(2, 2);
            var png = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);
            foreach (var item in _screen.Items) item.ImagePath = "old";
            _view.Bind(_screen, cache, null);
            yield return WaitForDisk(cache);
            client.SendMessage("Update");
            foreach (var item in _screen.Items) item.ImagePath = "new";
            Scroll(100);
            yield return WaitForDisk(cache);
            client.SendMessage("Update");
            Respond(client, "old", png);
            client.SendMessage("Update");
            foreach (var icon in Content.GetComponentsInChildren<RawImage>())
                Assert.AreSame(cache.Placeholder, icon.texture);
            Respond(client, "new", png);
            client.SendMessage("Update");
            foreach (var icon in Content.GetComponentsInChildren<RawImage>())
                Assert.AreNotSame(cache.Placeholder, icon.texture);
            _view.Bind(null, null, null);
        }

        private static void Respond(GopetClient client, string path, byte[] png)
        {
            using var packet = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(0).PutSByte(Gopet.Net.Images.ImagePackets.TypeIcon).PutUtf(path).PutInt(png.Length).PutBytes(png);
            client.Router.Dispatch(Message.FromWire(packet.ToWire(), false));
        }

        [Test]
        public void ResizeRefreshesVisibleRangeWithoutRebinding()
        {
            _view.Bind(_screen, null, null);
            ((RectTransform)_root.transform).sizeDelta = new Vector2(600, 460);
            _view.SendMessage("OnRectTransformDimensionsChange");
            Assert.IsNotNull(Row(9));
            Assert.That(Content.childCount, Is.LessThanOrEqualTo(14));
        }
    }
}
