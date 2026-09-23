using System;
using System.IO;
using System.Threading.Tasks;

namespace Gopet.Net.Images
{
    /// <summary>Serial disk lane. No directory creation, reads, writes or eviction
    /// run on the caller thread. Disposal skips queued work without joining I/O.</summary>
    public sealed class AssetDiskWorker : IDisposable
    {
        private readonly object _gate = new object();
        private readonly Func<AssetDiskStore> _createStore;
        private AssetDiskStore _store;
        private Task _tail = Task.CompletedTask;
        private volatile bool _stopped;

        public AssetDiskWorker(string root, long maxBytes)
            : this(() => new AssetDiskStore(root, maxBytes)) { }

        public AssetDiskWorker(Func<AssetDiskStore> createStore)
        {
            _createStore = createStore ?? throw new ArgumentNullException(nameof(createStore));
        }

        public Task Completion { get { lock (_gate) return _tail; } }

        public Task<byte[]> ReadAsync(string path) => Enqueue(store =>
            store.TryRead(path, out var png) ? png : null);

        public Task WriteAsync(string path, byte[] png) => Enqueue(store =>
        {
            store.Write(path, png);
            return true;
        });

        private Task<T> Enqueue<T>(Func<AssetDiskStore, T> action)
        {
            lock (_gate)
            {
                if (_stopped) return Task.FromResult(default(T));
                var previous = _tail;
                var task = Task.Run(async () =>
                {
                    // A failed optional cache operation must not poison the lane.
                    try { await previous.ConfigureAwait(false); } catch { }
                    if (_stopped) return default;
                    try
                    {
                        var store = _store ?? (_store = _createStore());
                        if (_stopped) return default;
                        return action(store);
                    }
                    catch (IOException) { return default; }
                    catch (UnauthorizedAccessException) { return default; }
                    catch (ArgumentException) { return default; }
                    catch (NotSupportedException) { return default; }
                });
                _tail = task;
                return task;
            }
        }

        public void Dispose()
        {
            lock (_gate) _stopped = true;
        }
    }
}
