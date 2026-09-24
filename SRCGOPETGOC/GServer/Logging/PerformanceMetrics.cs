using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Gopet.Logging;

public static class PerformanceMetrics
{
    private static readonly Meter meter = new("Gopet.Server");
    private static readonly Histogram<double> duration = meter.CreateHistogram<double>("gopet.operation.duration", "ms");
    public static Measurement Measure(string operation) => new(operation);
    public readonly struct Measurement(string operation) : IDisposable
    {
        private readonly long start = Stopwatch.GetTimestamp();
        public void Dispose() => duration.Record(Stopwatch.GetElapsedTime(start).TotalMilliseconds,
            new KeyValuePair<string, object?>("operation", operation));
    }
}
