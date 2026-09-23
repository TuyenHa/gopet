using System;
using System.Collections;
using System.IO;
using Gopet.Net;
using Gopet.Net.Images;
using Gopet.Runtime;
using Gopet.Runtime.Assets;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gopet.PlayModeTests
{
    public sealed partial class RemoteAssetCacheTests
    {
        [UnityTest]
        public IEnumerator ConcurrentRequestsShareOneLiveTexture()
        {
            Texture2D first = null, second = null;
            _cache.Get("shared", 0, t => first = t);
            _cache.Get("shared", 0, t => second = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Respond("shared");
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreSame(first, second);
            yield return null;
            Assert.IsTrue(first != null);
        }

        [UnityTest]
        public IEnumerator DiskImagesRespectFrameBudget()
        {
            var disk = new AssetDiskStore(_directory);
            disk.Write("a", _png);
            disk.Write("b", _png);
            _cache.MaxDecodesPerFrame = 1;
            Texture2D first = null, second = null;
            _cache.Get("a", 0, t => first = t);
            _cache.Get("b", 0, t => second = t);
            Assert.AreSame(_cache.Placeholder, first);
            Assert.AreSame(_cache.Placeholder, second);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreNotSame(_cache.Placeholder, first);
            Assert.AreSame(_cache.Placeholder, second);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreNotSame(_cache.Placeholder, second);
        }

        [UnityTest]
        public IEnumerator DisposeReleasesTexturesAndSlicedSprites()
        {
            Texture2D loaded = null;
            _cache.Get("shared", 0, t => loaded = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Respond("shared");
            yield return WaitForDisk();
            _client.SendMessage("Update");
            var frames = SpriteFrameCache.Slice("shared", loaded, 2);
            _cache.Dispose();
            yield return null;
            Assert.IsTrue(loaded == null);
            Assert.IsTrue(frames[0] == null);
        }

        [UnityTest]
        public IEnumerator MemoryPressureKeepsLiveOwnersAndEvictsDestroyedOwners()
        {
            var owner = new GameObject("Image owner");
            _cache.MaxMemoryBytes = 0;
            Texture2D loaded = null;
            _cache.Get("owned", 0, owner, t => loaded = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Respond("owned");
            yield return WaitForDisk();
            _client.SendMessage("Update");
            yield return new WaitForSecondsRealtime(1.1f);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreEqual(1, _cache.MemoryCount);
            Assert.IsTrue(loaded != null);
            Object.Destroy(owner);
            yield return new WaitForSecondsRealtime(1.1f);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            yield return null;
            Assert.AreEqual(0, _cache.MemoryCount);
            Assert.IsTrue(loaded == null);
        }

        [UnityTest]
        public IEnumerator RebindingOwnerIgnoresOldResponse()
        {
            Texture2D current = null;
            _cache.Get("old", 0, _root, t => current = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            _cache.Get("new", 0, _root, t => current = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Respond("old");
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreSame(_cache.Placeholder, current);
            Respond("new");
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreNotSame(_cache.Placeholder, current);
        }

        [UnityTest]
        public IEnumerator DestroyingBootstrapDisposesItsCache()
        {
            var bootstrap = _root.AddComponent<GopetBootstrap>();
            bootstrap.enabled = false;
            typeof(GopetBootstrap).GetField("_assets",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(bootstrap, _cache);
            Texture2D loaded = null;
            _cache.Get("owned", 0, t => loaded = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Respond("owned");
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Object.Destroy(bootstrap);
            yield return null;
            yield return null;
            Assert.IsTrue(loaded == null);
        }

        [UnityTest]
        public IEnumerator CallbackFailureDoesNotPreventOtherConsumersReceivingImage()
        {
            Texture2D received = null;
            _cache.Get("shared", 0, t =>
            {
                if (t != _cache.Placeholder) throw new InvalidOperationException("consumer failed");
            });
            _cache.Get("shared", 0, t => received = t);
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Respond("shared");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("consumer failed"));
            yield return WaitForDisk();
            _client.SendMessage("Update");
            Assert.AreNotSame(_cache.Placeholder, received);
            Assert.IsNotNull(received);
        }

    }
}
