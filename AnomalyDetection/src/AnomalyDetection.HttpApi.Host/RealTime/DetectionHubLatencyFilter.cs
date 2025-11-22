using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AnomalyDetection.RealTime.Latency
{
    /// <summary>
    /// Hub filter measuring invocation latency & payload size; logs slow calls and exposes metrics (Req9 enhancement).
    /// </summary>
    public class DetectionHubLatencyFilter : IHubFilter
    {
        private static readonly ConcurrentDictionary<string, LatencyAggregate> _metrics = new();
        private readonly ILogger<DetectionHubLatencyFilter> _logger;
        private readonly int _slowWarningMs;
        private readonly int _payloadLogThresholdBytes;

        public DetectionHubLatencyFilter(ILogger<DetectionHubLatencyFilter> logger)
        {
            _logger = logger;
            _slowWarningMs = 250; // default warning threshold (can be externalized later)
            _payloadLogThresholdBytes = 32_000; // 32KB informational threshold
        }

        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var sw = Stopwatch.StartNew();
            long payloadBytes = EstimatePayloadBytes(context);
            try
            {
                var result = await next(context);
                sw.Stop();
                Record(context.HubMethodName, sw.ElapsedMilliseconds, payloadBytes, success: true);
                if (sw.ElapsedMilliseconds > _slowWarningMs)
                {
                    _logger.LogWarning("SignalR slow invocation {Method} took {Elapsed}ms (payload {PayloadBytes} bytes, args {ArgCount})", context.HubMethodName, sw.ElapsedMilliseconds, payloadBytes, context.HubMethodArguments?.Count ?? 0);
                }
                else if (payloadBytes > _payloadLogThresholdBytes)
                {
                    _logger.LogInformation("SignalR large payload {Method} size {PayloadBytes} bytes (elapsed {Elapsed}ms)", context.HubMethodName, payloadBytes, sw.ElapsedMilliseconds);
                }
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                Record(context.HubMethodName, sw.ElapsedMilliseconds, payloadBytes, success: false);
                _logger.LogError(ex, "SignalR Hub method {Method} failed after {Elapsed}ms (payload {PayloadBytes} bytes)", context.HubMethodName, sw.ElapsedMilliseconds, payloadBytes);
                throw;
            }
        }

        public async ValueTask OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, ValueTask> next)
        {
            Record("__connect__", 0, 0, success: true);
            await next(context);
        }

        public async ValueTask OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, ValueTask> next)
        {
            Record("__disconnect__", 0, 0, success: exception == null);
            await next(context, exception);
        }

        private static void Record(string method, long elapsedMs, long payloadBytes, bool success)
        {
            var agg = _metrics.GetOrAdd(method, _ => new LatencyAggregate());
            agg.Add(elapsedMs, payloadBytes, success);
        }

        private static long EstimatePayloadBytes(HubInvocationContext context)
        {
            if (context.HubMethodArguments == null || context.HubMethodArguments.Count == 0) return 0;
            long total = 0;
            foreach (var arg in context.HubMethodArguments)
            {
                if (arg == null) continue;
                try
                {
                    switch (arg)
                    {
                        case string s:
                            total += System.Text.Encoding.UTF8.GetByteCount(s);
                            break;
                        case byte[] bytes:
                            total += bytes.Length;
                            break;
                        default:
                            var json = System.Text.Json.JsonSerializer.Serialize(arg);
                            total += System.Text.Encoding.UTF8.GetByteCount(json);
                            break;
                    }
                }
                catch { total += 64; }
            }
            return total;
        }

        internal static HubLatencySnapshot CreateSnapshot()
        {
            var snap = new HubLatencySnapshot();
            foreach (var kv in _metrics)
            {
                snap.Items.Add(new HubLatencyItem
                {
                    Method = kv.Key,
                    Count = kv.Value.Count,
                    ErrorCount = kv.Value.ErrorCount,
                    AvgMs = kv.Value.Count == 0 ? 0 : Math.Round((double)kv.Value.TotalMs / kv.Value.Count, 2),
                    P95Ms = kv.Value.GetPercentile(95),
                    P99Ms = kv.Value.GetPercentile(99),
                    MaxMs = kv.Value.MaxMs,
                    AvgPayloadBytes = kv.Value.AvgPayloadBytes,
                    MaxPayloadBytes = kv.Value.MaxPayloadBytes,
                    PayloadP95Bytes = kv.Value.GetPayloadPercentile(95),
                    PayloadP99Bytes = kv.Value.GetPayloadPercentile(99),
                    SlowCount = kv.Value.SlowCount
                });
            }
            return snap;
        }

        private class LatencyAggregate
        {
            private readonly ConcurrentQueue<long> _samples = new();
            private readonly ConcurrentQueue<long> _payloadSamples = new();
            public long TotalMs { get; private set; }
            public long TotalPayloadBytes { get; private set; }
            public int Count { get; private set; }
            public int ErrorCount { get; private set; }
            public long MaxMs { get; private set; }
            public long MaxPayloadBytes { get; private set; }
            public int SlowCount { get; private set; }
            private readonly long[] _bucketBounds = new long[] { 5, 10, 25, 50, 100, 250, 500, 1000 };
            private readonly int[] _bucketCounts = new int[9];
            public void Add(long ms, long payloadBytes, bool success)
            {
                _samples.Enqueue(ms);
                TotalMs += ms;
                Count++;
                if (!success) ErrorCount++;
                if (ms > MaxMs) MaxMs = ms;
                _payloadSamples.Enqueue(payloadBytes);
                TotalPayloadBytes += payloadBytes;
                if (payloadBytes > MaxPayloadBytes) MaxPayloadBytes = payloadBytes;
                if (ms > 250) SlowCount++;
                var idx = _bucketBounds.Length;
                for (int i = 0; i < _bucketBounds.Length; i++)
                {
                    if (ms <= _bucketBounds[i]) { idx = i; break; }
                }
                _bucketCounts[idx]++;
            }
            public double GetPercentile(int percentile)
            {
                if (Count == 0) return 0;
                var arr = _samples.ToArray();
                Array.Sort(arr);
                var index = (int)Math.Ceiling(percentile / 100.0 * arr.Length) - 1;
                if (index < 0) index = 0;
                if (index >= arr.Length) index = arr.Length - 1;
                return arr[index];
            }
            public double GetPayloadPercentile(int percentile)
            {
                if (Count == 0) return 0;
                var arr = _payloadSamples.ToArray();
                Array.Sort(arr);
                var index = (int)Math.Ceiling(percentile / 100.0 * arr.Length) - 1;
                if (index < 0) index = 0;
                if (index >= arr.Length) index = arr.Length - 1;
                return arr[index];
            }
            public double AvgPayloadBytes => Count == 0 ? 0 : (double)TotalPayloadBytes / Count;
            // MaxPayloadBytes & SlowCount already exposed via fields
            public long[] BucketBounds => _bucketBounds;
            public int[] BucketCounts => _bucketCounts;
        }
    }
    public class HubLatencySnapshot
    {
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
        public System.Collections.Generic.List<HubLatencyItem> Items { get; set; } = new();
    }

    public class HubLatencyItem
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ErrorCount { get; set; }
        public double AvgMs { get; set; }
        public double P95Ms { get; set; }
        public double P99Ms { get; set; }
        public long MaxMs { get; set; }
        public System.Collections.Generic.List<string> Buckets { get; set; } = new();
        public double AvgPayloadBytes { get; set; }
        public long MaxPayloadBytes { get; set; }
        public double PayloadP95Bytes { get; set; }
        public double PayloadP99Bytes { get; set; }
        public int SlowCount { get; set; }
    }
    public class HubLatencyMetricsProvider
    {
        public HubLatencySnapshot GetSnapshot() => DetectionHubLatencyFilter.CreateSnapshot();

        public string GetPrometheusMetrics()
        {
            var snap = DetectionHubLatencyFilter.CreateSnapshot();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("# HELP signalr_method_latency_seconds SignalR hub method latency histogram (simplified)");
            sb.AppendLine("# TYPE signalr_method_latency_seconds histogram");
            foreach (var method in snap.Items)
            {
                sb.AppendLine($"signalr_method_latency_avg_ms{{method=\"{method.Method}\"}} {method.AvgMs}");
                sb.AppendLine($"signalr_method_latency_p95_ms{{method=\"{method.Method}\"}} {method.P95Ms}");
                sb.AppendLine($"signalr_method_latency_p99_ms{{method=\"{method.Method}\"}} {method.P99Ms}");
                sb.AppendLine($"signalr_method_latency_max_ms{{method=\"{method.Method}\"}} {method.MaxMs}");
                sb.AppendLine($"signalr_method_invocations_total{{method=\"{method.Method}\"}} {method.Count}");
                sb.AppendLine($"signalr_method_errors_total{{method=\"{method.Method}\"}} {method.ErrorCount}");
                sb.AppendLine($"signalr_method_slow_invocations_total{{method=\"{method.Method}\"}} {method.SlowCount}");
                sb.AppendLine($"signalr_method_payload_avg_bytes{{method=\"{method.Method}\"}} {method.AvgPayloadBytes}");
                sb.AppendLine($"signalr_method_payload_max_bytes{{method=\"{method.Method}\"}} {method.MaxPayloadBytes}");
                sb.AppendLine($"signalr_method_payload_p95_bytes{{method=\"{method.Method}\"}} {method.PayloadP95Bytes}");
                sb.AppendLine($"signalr_method_payload_p99_bytes{{method=\"{method.Method}\"}} {method.PayloadP99Bytes}");
            }
            return sb.ToString();
        }
    }
}
