using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gopet.Net.Images;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class AssetDiskWorkerTests
    {
        [Fact]
        public async Task WritesAndReadsRemainOrderedOnBackgroundWorker()
        {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var caller = Thread.CurrentThread.ManagedThreadId;
            var workerThread = caller;
            using var started = new ManualResetEventSlim();
            using var worker = new AssetDiskWorker(() =>
            {
                workerThread = Thread.CurrentThread.ManagedThreadId;
                started.Set();
                return new AssetDiskStore(root);
            });
            try
            {
                var write = worker.WriteAsync("icon", new byte[] { 1, 2, 3 });
                var read = worker.ReadAsync("icon");
                Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
                await write;
                Assert.Equal(new byte[] { 1, 2, 3 }, await read);
                Assert.NotEqual(caller, workerThread);
            }
            finally { await worker.Completion; if (Directory.Exists(root)) Directory.Delete(root, true); }
        }

        [Fact]
        public async Task DisposeDoesNotWaitForDiskAndSkipsQueuedWork()
        {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            var worker = new AssetDiskWorker(() =>
            {
                entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
                return new AssetDiskStore(root);
            });
            try
            {
                var read = worker.ReadAsync("missing");
                Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
                var write = worker.WriteAsync("cancelled", new byte[] { 9 });
                worker.Dispose();
                Assert.False(read.IsCompleted); // Dispose returned while disk is still blocked.
                release.Set();
                await write;
                Assert.Null(await read);
                Assert.False(new AssetDiskStore(root).TryRead("cancelled", out _));
            }
            finally
            {
                release.Set();
                worker.Dispose();
                await worker.Completion;
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Fact]
        public async Task UnavailableDiskFallsBackToCacheMiss()
        {
            using var worker = new AssetDiskWorker(() => throw new UnauthorizedAccessException());
            Assert.Null(await worker.ReadAsync("icon"));
            await worker.WriteAsync("icon", new byte[] { 1 });
            Assert.Null(await worker.ReadAsync("icon"));
        }
    }
}
