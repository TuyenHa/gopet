using Gopet.Runtime;
using Gopet.Data.Map;
using System.Diagnostics;

static class RuntimeTests
{
    public static void BackupPublication()
    {
        string root = Path.Combine(Path.GetTempPath(), "gopet-backup-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string target = Path.Combine(root, "backup.sql");
        try
        {
            File.WriteAllText(target, "previous complete backup");
            try
            {
                DBBackup.Publish(target, temporary => { File.WriteAllText(temporary, "partial"); throw new IOException("interrupted export"); });
                throw new Exception("export failure hidden");
            }
            catch (IOException) { }
            if (File.ReadAllText(target) != "previous complete backup") throw new Exception("partial export replaced complete backup");
            DBBackup.Publish(target, temporary => File.WriteAllText(temporary, "new complete backup"));
            if (File.ReadAllText(target) != "new complete backup") throw new Exception("completed export not published");
        }
        finally { Directory.Delete(root, true); }
    }
    public static void BackupShutdown()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        int calls = 0;
        var backup = new DBBackup(_ => { Interlocked.Increment(ref calls); entered.Set(); release.Wait(3000); });
        backup.Update();
        if (!entered.Wait(1000)) throw new Exception("backup never entered");
        var stop = Task.Run(backup.Stop);
        try
        {
            if (stop.Wait(150)) throw new Exception("shutdown returned while backup was still exporting");
        }
        finally { release.Set(); }
        if (!stop.Wait(3000)) throw new Exception("backup shutdown did not complete");
        backup.Update();
        if (calls != 1) throw new Exception("stopped backup accepted another job");
    }
    public static void Backup()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        int calls = 0;
        var backup = new DBBackup(_ => { Interlocked.Increment(ref calls); entered.Set(); release.Wait(3000); });
        var timer = Stopwatch.StartNew(); backup.Update();
        try
        {
            if (timer.ElapsedMilliseconds > 200) throw new Exception("backup blocked runtime update");
            if (!entered.Wait(1000)) throw new Exception("backup never started");
            Parallel.For(0,20,_ => backup.Update());
            if (calls != 1) throw new Exception("overlapping backup jobs");
        }
        finally { release.Set(); backup.Completion.Wait(3000); }
        backup.Update();
        if (calls != 1) throw new Exception("backup interval not respected");
    }
    public static void Tick()
    {
        var map = new SlowMap(); map.run();
        if (map.IntervalMs >= 750) throw new Exception($"tick added a full 500ms after work: {map.IntervalMs}ms");
    }
    sealed class SlowMap() : GopetMap(-12345, false, new MapTemplate())
    {
        readonly Stopwatch clock = new();
        int calls;
        public long IntervalMs;
        public override void createZoneDefault() { }
        public override void update()
        {
            if (++calls == 1) { clock.Start(); Thread.Sleep(350); }
            else { IntervalMs = clock.ElapsedMilliseconds; isRunning = false; }
        }
    }
}
