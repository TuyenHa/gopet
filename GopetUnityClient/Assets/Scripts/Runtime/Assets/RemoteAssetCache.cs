using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Gopet.Net;
using Gopet.Net.Images;
using Gopet.Runtime.World;
using UnityEngine;

namespace Gopet.Runtime.Assets
{
    /// <summary>Shared textures owned by this cache until Dispose. Callbacks receive
    /// a placeholder immediately, then the decoded image on the main thread.</summary>
    public sealed partial class RemoteAssetCache : IDisposable
    {
        public int MaxDecodesPerFrame = 4;
        private readonly Dictionary<string, Texture2D> _memory = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, Pending> _pending = new Dictionary<string, Pending>();
        private readonly Queue<Pending> _decodeQueue = new Queue<Pending>();
        private readonly AssetDiskWorker _disk;
        private readonly Queue<Pending> _diskReads = new Queue<Pending>();
        private readonly ImageHandler _handler;
        private readonly GopetClient _client;
        private readonly MessageRouter _router;
        private Texture2D _placeholder, _failed;
        private bool _disposed;

        private sealed class Pending
        {
            public string Path;
            public sbyte Type;
            public bool FromDisk;
            public byte[] Png;
            public Task<byte[]> DiskRead;
            public readonly List<Action<Texture2D>> Waiters = new List<Action<Texture2D>>();
        }

        public RemoteAssetCache(GopetClient client, MessageRouter router, string cacheRoot = null,
                                long maxCacheBytes = 200L * 1024 * 1024)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _disk = new AssetDiskWorker(cacheRoot ?? Path.Combine(Application.persistentDataPath, "assetcache"),
                maxCacheBytes);
            _handler = new ImageHandler(client.Send, () => (long)(Time.realtimeSinceStartup * 1000f))
                { MaxRetries = 3, TimeoutMs = 15000 };
            _handler.RegisterOn(router);
            _handler.TimedOut += path => Debug.LogWarning($"[Gopet] Image timed out: {path}");
            _handler.WaiterFailed += (path, ex) => Debug.LogException(ex);
            client.Ticked += OnTick;
        }

        public Texture2D Placeholder => _placeholder != null ? _placeholder : _placeholder = TextureFactory.Placeholder();
        public Texture2D FailedTexture => _failed != null ? _failed : _failed = TextureFactory.Failed();
        public int MemoryCount => _memory.Count;
        public int PendingDecodeCount => _decodeQueue.Count;
        public Task DiskCompletion => _disk.Completion;

        public void Get(string path, sbyte type, Action<Texture2D> onReady)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteAssetCache));
            if (onReady == null) throw new ArgumentNullException(nameof(onReady));
            if (!string.IsNullOrEmpty(path)) _unowned.Add(path);
            GetCore(path, type, onReady);
        }

        private void GetCore(string path, sbyte type, Action<Texture2D> onReady)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteAssetCache));
            if (onReady == null) throw new ArgumentNullException(nameof(onReady));
            if (string.IsNullOrEmpty(path) || path == ImagePackets.EmptyImagePath)
            {
                onReady(Placeholder);
                return;
            }
            _lastAccess[path] = ++_accessSequence;
            if (_memory.TryGetValue(path, out var cached) && cached != null)
            {
                onReady(cached);
                return;
            }
            // Register before invoking user code: a reentrant Get must join this request.
            if (!_pending.TryGetValue(path, out var pending))
            {
                pending = new Pending { Path = path, Type = type, FromDisk = true };
                _pending.Add(path, pending);
                pending.DiskRead = ImagePackets.IsCaptcha(path)
                    ? Task.FromResult<byte[]>(null) : _disk.ReadAsync(path);
                _diskReads.Enqueue(pending);
            }
            pending.Waiters.Add(onReady);
            onReady(Placeholder);
        }

        private void OnTick()
        {
            if (_disposed) return;
            _handler.Tick();
            var reads = MaxDecodesPerFrame;
            while (reads-- > 0 && _diskReads.Count > 0 && _diskReads.Peek().DiskRead.IsCompleted)
            {
                var pending = _diskReads.Dequeue();
                try { pending.Png = pending.DiskRead.GetAwaiter().GetResult(); }
                catch (Exception ex) { Debug.LogException(ex); }
                pending.DiskRead = null;
                if (pending.Png == null) RequestRemote(pending);
                else _decodeQueue.Enqueue(pending);
            }
            var budget = MaxDecodesPerFrame;
            while (!_disposed && budget-- > 0 && _decodeQueue.Count > 0)
                Materialize(_decodeQueue.Dequeue());
            TrimMemory();
        }

        private void RequestRemote(Pending pending)
        {
            pending.FromDisk = false;
            _handler.Request(pending.Path, pending.Type, response =>
            {
                if (_disposed) return;
                pending.Png = response.Png;
                _decodeQueue.Enqueue(pending);
            });
        }

        private void Materialize(Pending pending)
        {
            var texture = pending.Png == null || pending.Png.Length == 0
                ? null : TextureFactory.Decode(pending.Png);
            if (texture == null && pending.FromDisk)
            {
                RequestRemote(pending);
                return;
            }
            if (texture != null)
            {
                _memory[pending.Path] = texture;
                if (!pending.FromDisk && !ImagePackets.IsCaptcha(pending.Path))
                    _ = _disk.WriteAsync(pending.Path, pending.Png);
            }
            else
            {
                _lastAccess.Remove(pending.Path);
                Debug.LogWarning($"[Gopet] Image unavailable: {pending.Path}");
                texture = FailedTexture;
            }
            _pending.Remove(pending.Path);
            foreach (var waiter in pending.Waiters)
            {
                if (_disposed) break;
                try { waiter(texture); }
                catch (Exception ex) { Debug.LogException(ex); }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _disk.Dispose();
            _diskReads.Clear();
            _client.Ticked -= OnTick;
            _router.Unregister(GopetCmd.COMMAND_IMAGE);
            foreach (var texture in _memory.Values) Release(texture);
            _memory.Clear();
            _owners.Clear();
            _ownerVersions.Clear();
            _unowned.Clear();
            _lastAccess.Clear();
            _pending.Clear();
            _decodeQueue.Clear();
            Release(_placeholder);
            Release(_failed);
            _placeholder = _failed = null;
        }

        private static void Release(Texture2D texture)
        {
            if (texture == null) return;
            SpriteFrameCache.Release(texture);
            UnityEngine.Object.Destroy(texture);
        }
    }
}
