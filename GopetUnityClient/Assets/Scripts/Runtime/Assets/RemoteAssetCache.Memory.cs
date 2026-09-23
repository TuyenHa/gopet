using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gopet.Runtime.Assets
{
    public sealed partial class RemoteAssetCache
    {
        // Soft budget: live consumers may exceed this; their textures cannot be evicted.
        public long MaxMemoryBytes = 64L * 1024 * 1024;
        private readonly Dictionary<Object, string> _owners = new Dictionary<Object, string>();
        private readonly Dictionary<Object, long> _ownerVersions = new Dictionary<Object, long>();
        private readonly HashSet<string> _unowned = new HashSet<string>();
        private readonly Dictionary<string, long> _lastAccess = new Dictionary<string, long>();
        private readonly List<Object> _deadOwners = new List<Object>();
        private readonly HashSet<string> _pinned = new HashSet<string>();
        private readonly List<string> _eviction = new List<string>();
        private long _accessSequence;
        private long _bindingSequence;
        private float _nextTrim;

        /// <summary>One image slot per owner. Rebinding a slot cancels its old callbacks.
        /// Destroying the owner allows its image to be evicted under memory pressure.</summary>
        public void Get(string path, sbyte type, Object owner, Action<Texture2D> onReady)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RemoteAssetCache));
            if (onReady == null) throw new ArgumentNullException(nameof(onReady));
            if (owner == null) return;
            _owners[owner] = path;
            var version = ++_bindingSequence;
            _ownerVersions[owner] = version;
            GetCore(path, type, texture =>
            {
                if (owner != null && _ownerVersions.TryGetValue(owner, out var current) && current == version)
                    onReady(texture);
            });
        }

        /// <summary>Release an image slot before clearing/hiding a pooled view.</summary>
        public void ReleaseOwner(Object owner)
        {
            if (ReferenceEquals(owner, null)) return;
            _owners.Remove(owner);
            _ownerVersions.Remove(owner);
        }

        public long MemoryBytes
        {
            get
            {
                long total = 0;
                foreach (var texture in _memory.Values)
                    if (texture != null) total += (long)texture.width * texture.height * 4;
                return total;
            }
        }

        private void TrimMemory()
        {
            if (_disposed || Time.unscaledTime < _nextTrim) return;
            _nextTrim = Time.unscaledTime + 1f;
            _deadOwners.Clear();
            _pinned.Clear();
            foreach (var pair in _owners)
            {
                if (pair.Key == null) _deadOwners.Add(pair.Key);
                else if (pair.Value != null) _pinned.Add(pair.Value);
            }
            foreach (var owner in _deadOwners) ReleaseOwner(owner);
            _deadOwners.Clear();
            var bytes = MemoryBytes;
            if (bytes <= MaxMemoryBytes) return;
            _eviction.Clear();
            foreach (var path in _memory.Keys)
                if (!_pinned.Contains(path) && !_unowned.Contains(path)) _eviction.Add(path);
            _eviction.Sort((a, b) => _lastAccess[a].CompareTo(_lastAccess[b]));
            foreach (var path in _eviction)
            {
                if (bytes <= MaxMemoryBytes) break;
                var texture = _memory[path];
                if (texture != null) bytes -= (long)texture.width * texture.height * 4;
                _memory.Remove(path);
                _lastAccess.Remove(path);
                Release(texture);
            }
            _eviction.Clear();
        }
    }
}
