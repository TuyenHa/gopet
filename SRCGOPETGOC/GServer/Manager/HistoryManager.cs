using Dapper;
using Gopet.Manager;
using MySqlConnector;
using Newtonsoft.Json;

public class HistoryManager
{
    public static HistoryManager Instance = new();
    private readonly Lazy<HistoryDeliveryQueue> queue;
    private readonly AutoResetEvent wake = new(false);
    private int started;
    private volatile bool stopping;
    public Thread HistoryThread;
    public int Backlog => queue.IsValueCreated ? queue.Value.PendingCount : 0;
    public long BacklogBytes => queue.IsValueCreated ? queue.Value.PendingBytes : 0;
    public DateTime HeartbeatUtc => queue.IsValueCreated ? queue.Value.HeartbeatUtc : default;

    public HistoryManager()
    {
        queue = new(() => new HistoryDeliveryQueue(
            Environment.GetEnvironmentVariable("GOPET_HISTORY_SPOOL") ?? Path.Combine(AppContext.BaseDirectory, "history-spool"),
            WriteBatch, long.TryParse(Environment.GetEnvironmentVariable("GOPET_HISTORY_MAX_BYTES"), out var budget)
                && budget > 0 ? budget : 256L * 1024 * 1024));
        HistoryThread = new Thread(run) { Name = "History thread", IsBackground = true };
    }
    public void start()
    {
        _ = queue.Value; // Fail startup clearly if durable storage is unavailable.
        if (Interlocked.Exchange(ref started, 1) == 0) HistoryThread.Start();
    }
    public void add(History history)
    {
        // Capture now so the backlog retains neither Player nor mutable playerData.
        var record = new HistoryRecord(Guid.NewGuid().ToString("N"), history.user_id, history.log,
            JsonConvert.SerializeObject(history.obj), history.player == null ? null : history.player.playerData?.name ?? "Chưa tạo nhân vật");
        queue.Value.Add(record);
        wake.Set();
    }
    public static void addHistory(History history) => Instance.add(history);
    public void run()
    {
        while (!stopping)
        {
            if (queue.Value.DeliverBatch()) continue;
            if (queue.Value.LastError is Exception error)
            {
                // Independent fallback: a broken normal logger must not kill this worker.
                try { Console.Error.WriteLine($"History delivery failed; {Backlog} records retained: {error.Message}"); }
                catch { }
                Thread.Sleep(2000); // Producer signals must not turn a DB outage into a busy loop.
            }
            else wake.WaitOne(2000);
        }
    }
    public bool Stop(TimeSpan timeout)
    {
        var deadline = System.Diagnostics.Stopwatch.StartNew();
        while (Backlog > 0 && deadline.Elapsed < timeout && HistoryThread.IsAlive)
        {
            wake.Set();
            Thread.Sleep(20);
        }
        stopping = true;
        wake.Set();
        if (HistoryThread.IsAlive) HistoryThread.Join(TimeSpan.FromMilliseconds(Math.Max(0, (timeout - deadline.Elapsed).TotalMilliseconds)));
        return Backlog == 0; // Remaining files are replayed on next startup.
    }
    private static void WriteBatch(IReadOnlyList<HistoryRecord> records)
    {
        if (records.Count == 0) return;
        MySqlConnection? game = null;
        try
        {
            var parameters = new DynamicParameters();
            var rows = new List<string>();
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                string? name = record.CharName;
                if (name == null)
                {
                    game ??= MYSQLManager.create();
                    name = game.QuerySingleOrDefault<string>("SELECT name FROM player WHERE user_id = @id", new { id = record.TargetId }) ?? "Chưa tạo nhân vật";
                }
                rows.Add($"(@event{i},@target{i},@log{i},@obj{i},@name{i})");
                parameters.Add($"event{i}", record.EventId);
                parameters.Add($"target{i}", record.TargetId);
                parameters.Add($"log{i}", record.Log);
                parameters.Add($"obj{i}", record.ObjJson);
                parameters.Add($"name{i}", name);
            }
            using var connection = MYSQLManager.createLogConnection();
            using var transaction = connection.BeginTransaction();
            connection.Execute("INSERT INTO history (eventId,targetId,log,obj,charname) VALUES " + string.Join(",", rows)
                + " ON DUPLICATE KEY UPDATE eventId=VALUES(eventId)", parameters, transaction);
            transaction.Commit();
        }
        catch (MySqlException error) when (error.ErrorCode == MySqlErrorCode.PacketTooLarge)
        {
            throw new HistoryBatchTooLargeException("Log DB rejected the history packet size", error);
        }
        finally { game?.Dispose(); }
    }
}
