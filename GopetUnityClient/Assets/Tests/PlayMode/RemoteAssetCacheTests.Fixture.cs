using System;
using System.Collections;
using System.IO;
using Gopet.Net;
using Gopet.Runtime;
using Gopet.Runtime.Assets;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gopet.PlayModeTests
{
    public sealed partial class RemoteAssetCacheTests
    {
        private GameObject _root;
        private GopetClient _client;
        private RemoteAssetCache _cache;
        private string _directory;
        private byte[] _png;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Cache test");
            _client = _root.AddComponent<GopetClient>();
            _client.enabled = false;
            _directory = Path.Combine(Application.temporaryCachePath, Guid.NewGuid().ToString("N"));
            _cache = new RemoteAssetCache(_client, _client.Router, _directory);
            var texture = new Texture2D(4, 2);
            _png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _cache.Dispose();
            Object.DestroyImmediate(_root);
            yield return WaitForDisk();
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        private void Respond(string path)
        {
            using var packet = Message.Create(GopetCmd.COMMAND_IMAGE)
                .PutSByte(0).PutSByte(0).PutUtf(path).PutInt(_png.Length).PutBytes(_png);
            _client.Router.Dispatch(Message.FromWire(packet.ToWire(), false));
        }
        private IEnumerator WaitForDisk()
        {
            var deadline = Time.realtimeSinceStartup + 10f;
            while (!_cache.DiskCompletion.IsCompleted && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue(_cache.DiskCompletion.IsCompleted, "Disk worker did not finish");
        }
    }
}
