using System.Collections.Concurrent;

namespace Gopet.Logging;

public class Monitor
{
    private sealed record Entry(string? Message, ConsoleColor Color, TaskCompletionSource? Barrier = null);
    private static readonly BlockingCollection<Entry> queue = new(8192);
    private static long dropped;
    public static long DroppedMessages => Interlocked.Read(ref dropped);
    public string LogName { get; }

    static Monitor()
    {
        new Thread(Drain) { Name = "Server logger", IsBackground = true }.Start();
    }
    public Monitor(string logName) => LogName = logName;
    public void LogDebug(string message) => Write(message, ConsoleColor.White);
    public void LogError(string message) => Write(message, ConsoleColor.Red);
    public void LogWarning(string message) => Write(message, ConsoleColor.Yellow);
    public void LogInfo(string message) => Write(message, ConsoleColor.Green);
    private void Write(string message, ConsoleColor color)
    {
        string prefix = $"[{LogName} {DateTime.Now}] ";
        // Bound both entry count and individual retained text size.
        if (message.Length > 16384) message = message[..16384] + " [truncated]";
        var entry = new Entry(prefix + message.Replace("\n", "\n" + prefix), color);
        if (!queue.TryAdd(entry))
        {
            long count = Interlocked.Increment(ref dropped);
            if (count == 1 || count % 1024 == 0) Fallback($"Logger queue full; dropped {count} messages");
        }
    }
    private static void Drain()
    {
        foreach (var entry in queue.GetConsumingEnumerable())
        {
            if (entry.Barrier != null)
            {
                try { GopetManager.Writer.Flush(); }
                catch (Exception error) { Fallback(error.Message); }
                entry.Barrier.TrySetResult();
                continue;
            }
            try
            {
                Console.ForegroundColor = entry.Color;
                Console.WriteLine(entry.Message);
                Console.ResetColor();
            }
            catch (Exception error) { Fallback(error.Message); }
            try
            {
                var writer = GopetManager.Writer;
                writer.WriteLine(entry.Message);
                if (queue.Count == 0) writer.Flush();
            }
            catch (Exception error) { Fallback(error.Message); }
        }
    }
    private static void Fallback(string message)
    {
        try { Console.Error.WriteLine("Logger failure: " + message); }
        catch { } // Never re-enter this logger from its error path.
    }
    public static bool Flush(TimeSpan timeout)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!queue.TryAdd(new Entry(null, default, done), timeout)) return false;
        return done.Task.Wait(TimeSpan.FromMilliseconds(Math.Max(0, (timeout - timer.Elapsed).TotalMilliseconds)));
    }
}
