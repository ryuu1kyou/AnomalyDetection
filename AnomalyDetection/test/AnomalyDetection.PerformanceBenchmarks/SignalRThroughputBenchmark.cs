using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace AnomalyDetection.PerformanceBenchmarks;

/// <summary>
/// メッセージ処理スループットのパフォーマンステスト
/// 目標: 1000 messages/sec の処理能力
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 2, iterationCount: 3)]
public class MessageThroughputBenchmark
{
    private List<TestMessage> _testMessages = null!;

    [Params(100, 500, 1000)]
    public int MessageCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _testMessages = new List<TestMessage>();

        for (int i = 0; i < MessageCount; i++)
        {
            _testMessages.Add(new TestMessage
            {
                TenantId = Guid.NewGuid(),
                DeviceId = $"Device{i % 10}",
                MetricType = "Temperature",
                Value = 75.0 + (i % 20),
                Threshold = 70.0,
                Timestamp = DateTime.UtcNow.AddSeconds(-i),
                Severity = "High"
            });
        }
    }

    /// <summary>
    /// メッセージシリアライゼーションスループット
    /// JSON形式への変換パフォーマンス
    /// </summary>
    [Benchmark]
    public void MessageSerializationThroughput()
    {
        var serializedMessages = new ConcurrentBag<string>();

        foreach (var message in _testMessages)
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(message);
            serializedMessages.Add(serialized);
        }
    }

    /// <summary>
    /// 並列メッセージシリアライゼーション
    /// </summary>
    [Benchmark]
    public void ParallelMessageSerialization()
    {
        var serializedMessages = new ConcurrentBag<string>();

        Parallel.ForEach(_testMessages, message =>
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(message);
            serializedMessages.Add(serialized);
        });
    }

    /// <summary>
    /// シーケンシャルメッセージ処理
    /// 1000 messages/sec = 1ms/message のターゲット
    /// </summary>
    [Benchmark]
    public async Task SequentialMessageProcessing()
    {
        var processedCount = 0;

        foreach (var message in _testMessages)
        {
            // Simulate minimal processing time
            await Task.Yield();

            // Simulate serialization
            var serialized = System.Text.Json.JsonSerializer.Serialize(message);
            processedCount++;
        }
    }

    /// <summary>
    /// 並列メッセージ処理
    /// 複数メッセージの同時処理能力
    /// </summary>
    [Benchmark]
    public async Task ParallelMessageProcessing()
    {
        var tasks = _testMessages.Select(async message =>
        {
            await Task.Yield();
            var serialized = System.Text.Json.JsonSerializer.Serialize(message);
            return serialized;
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// バッチメッセージ処理
    /// バッチサイズ: 10メッセージ
    /// </summary>
    [Benchmark]
    public async Task BatchMessageProcessing()
    {
        const int batchSize = 10;
        var batches = _testMessages
            .Select((message, index) => new { message, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.message).ToList());

        foreach (var batch in batches)
        {
            await Task.WhenAll(batch.Select(async message =>
            {
                await Task.Yield();
                var serialized = System.Text.Json.JsonSerializer.Serialize(message);
                return serialized;
            }));
        }
    }
}

/// <summary>
/// テスト用メッセージクラス
/// </summary>
public class TestMessage
{
    public Guid TenantId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Threshold { get; set; }
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = string.Empty;
}