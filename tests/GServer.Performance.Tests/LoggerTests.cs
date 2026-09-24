using System.Reflection;
using Log = Gopet.Logging.Monitor;

static class LoggerTests
{
    public static void Run()
    {
        var field = typeof(GopetManager).GetField("__writer", BindingFlags.Static | BindingFlags.NonPublic);
        var old = field.GetValue(null);
        using var broken = new FailingWriter();
        try
        {
            field.SetValue(null, broken);
            new Log("test").LogError("injected logging failure");
            if (!Log.Flush(TimeSpan.FromSeconds(3))) throw new Exception("logger worker stopped");
            if (broken.Attempts != 1) throw new Exception("logger recursively retried its own failure");
            using var memory = new MemoryStream();
            using var writer = new StreamWriter(memory, leaveOpen: true);
            field.SetValue(null, writer);
            new Log("test").LogInfo("worker survived");
            if (!Log.Flush(TimeSpan.FromSeconds(3)) || memory.Length == 0)
                throw new Exception("logger failed to recover");
        }
        finally { field.SetValue(null, old); }
    }
    sealed class FailingWriter() : StreamWriter(new MemoryStream())
    {
        public int Attempts;
        public override void WriteLine(string value) { Attempts++; throw new IOException("injected file failure"); }
    }
}
