using System.Text.Json;

namespace Gopet.Manager;

public sealed record HistoryRecord(string EventId, int TargetId, string Log, string ObjJson, string? CharName);
public sealed class HistoryBatchTooLargeException(string message, Exception? inner = null) : IOException(message, inner);

/// <summary>Durable snapshots; memory use is bounded by one batch. Sink must be idempotent by EventId.</summary>
public sealed class HistoryDeliveryQueue
{
    private readonly string directory;
    private readonly Action<IReadOnlyList<HistoryRecord>> deliver;
    private readonly long maxBytes;
    private readonly long maxBatchBytes;
    private readonly object gate = new();
    private readonly object deliveryGate = new();
    private long pendingBytes;
    private int pendingCount;
    public int PendingCount { get { lock (gate) return pendingCount; } }
    public long PendingBytes { get { lock (gate) return pendingBytes; } }
    public Exception? LastError { get; private set; }
    public DateTime HeartbeatUtc { get; private set; }

    public HistoryDeliveryQueue(string directory, Action<IReadOnlyList<HistoryRecord>> deliver,
        long maxBytes = 256L * 1024 * 1024, long maxBatchBytes = 256 * 1024)
    {
        this.directory = directory;
        this.deliver = deliver;
        this.maxBytes = maxBytes;
        this.maxBatchBytes = maxBatchBytes > 0 ? maxBatchBytes : throw new ArgumentOutOfRangeException(nameof(maxBatchBytes));
        Directory.CreateDirectory(directory);
        foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
        {
            pendingBytes += new FileInfo(file).Length;
            pendingCount++;
        }
    }

    public void Add(HistoryRecord record)
    {
        if (!Guid.TryParseExact(record.EventId, "N", out _)) throw new ArgumentException("Invalid history event ID");
        var bytes = JsonSerializer.SerializeToUtf8Bytes(record);
        lock (gate)
        {
            if (bytes.LongLength > maxBytes - pendingBytes)
                throw new IOException("History spool capacity exceeded; restore log DB or increase GOPET_HISTORY_MAX_BYTES");
            string target = Path.Combine(directory, record.EventId + ".json");
            string staging = target + ".tmp";
            using (var file = new FileStream(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                file.Write(bytes);
                file.Flush(flushToDisk: true);
            }
            File.Move(staging, target);
            pendingBytes += bytes.Length;
            pendingCount++;
        }
    }

    public bool DeliverBatch()
    {
        lock (deliveryGate)
        {
            HeartbeatUtc = DateTime.UtcNow;
            try
            {
                var paths = new List<string>();
                long batchBytes = 0;
                foreach (var path in Directory.EnumerateFiles(directory, "*.json"))
                {
                    long size = new FileInfo(path).Length;
                    if (paths.Count > 0 && (paths.Count == 64 || size > maxBatchBytes - batchBytes)) break;
                    paths.Add(path);
                    batchBytes += size;
                }
                if (paths.Count == 0) return false;
                DeliverFiles(paths.ToArray());
                LastError = null;
                return true;
            }
            catch (Exception error)
            {
                // Keep every unacknowledged snapshot and its ID, including ambiguous commits.
                LastError = error;
                return false;
            }
        }
    }

    private void DeliverFiles(string[] paths)
    {
        var records = paths.Select(path => JsonSerializer.Deserialize<HistoryRecord>(File.ReadAllBytes(path))
            ?? throw new InvalidDataException("Invalid history snapshot: " + path)).ToArray();
        try { deliver(records); }
        catch (HistoryBatchTooLargeException) when (paths.Length > 1)
        {
            int middle = paths.Length / 2;
            DeliverFiles(paths[..middle]);
            DeliverFiles(paths[middle..]);
            return;
        }
        // A single oversized record is retained and reported through LastError.
        lock (gate)
        {
            foreach (var path in paths)
            {
                long size = new FileInfo(path).Length;
                File.Delete(path);
                pendingBytes -= size;
                pendingCount--;
            }
        }
    }
}
