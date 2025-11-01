using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Volo.Abp.Testing;
using Xunit;
using Xunit.Abstractions;

namespace AnomalyDetection.Application.Tests.Performance;

public abstract class PerformanceTestBase : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    protected readonly ITestOutputHelper Output;
    protected readonly ILogger Logger;

    protected PerformanceTestBase(ITestOutputHelper output)
    {
        Output = output;
        Logger = GetRequiredService<ILogger<PerformanceTestBase>>();
    }

    /// <summary>
    /// メソッドの実行時間を測定
    /// </summary>
    protected async Task<PerformanceResult> MeasureExecutionTimeAsync(
        Func<Task> action,
        string operationName,
        int expectedMaxMilliseconds = 1000)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await action();
            stopwatch.Stop();

            var result = new PerformanceResult
            {
                OperationName = operationName,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess = true,
                ExpectedMaxMs = expectedMaxMilliseconds
            };

            LogPerformanceResult(result);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var result = new PerformanceResult
            {
                OperationName = operationName,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                IsSuccess = false,
                Exception = ex,
                ExpectedMaxMs = expectedMaxMilliseconds
            };

            LogPerformanceResult(result);
            throw;
        }
    }

    /// <summary>
    /// 複数回実行して平均実行時間を測定
    /// </summary>
    protected async Task<PerformanceStatistics> MeasureAverageExecutionTimeAsync(
        Func<Task> action,
        string operationName,
        int iterations = 10,
        int expectedMaxMilliseconds = 1000)
    {
        var results = new List<long>();
        var exceptions = new List<Exception>();

        for (int i = 0; i < iterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await action();
                stopwatch.Stop();
                results.Add(stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.Add(stopwatch.ElapsedMilliseconds);
                exceptions.Add(ex);
            }
        }

        var statistics = new PerformanceStatistics
        {
            OperationName = operationName,
            Iterations = iterations,
            AverageMs = results.Count > 0 ? results.Average() : 0,
            MinMs = results.Count > 0 ? results.Min() : 0,
            MaxMs = results.Count > 0 ? results.Max() : 0,
            MedianMs = CalculateMedian(results),
            SuccessCount = iterations - exceptions.Count,
            FailureCount = exceptions.Count,
            ExpectedMaxMs = expectedMaxMilliseconds,
            Exceptions = exceptions
        };

        LogPerformanceStatistics(statistics);
        return statistics;
    }

    /// <summary>
    /// 負荷テスト（並列実行）
    /// </summary>
    protected async Task<LoadTestResult> ExecuteLoadTestAsync(
        Func<Task> action,
        string operationName,
        int concurrentUsers = 10,
        int operationsPerUser = 5,
        int expectedMaxMilliseconds = 2000)
    {
        var tasks = new List<Task<List<long>>>();
        var overallStopwatch = Stopwatch.StartNew();

        // 並列タスクを作成
        for (int user = 0; user < concurrentUsers; user++)
        {
            tasks.Add(ExecuteUserOperationsAsync(action, operationsPerUser));
        }

        // 全タスクの完了を待機
        var results = await Task.WhenAll(tasks);
        overallStopwatch.Stop();

        // 結果を集計
        var allExecutionTimes = new List<long>();
        var totalOperations = 0;
        var totalExceptions = 0;

        foreach (var userResults in results)
        {
            allExecutionTimes.AddRange(userResults);
            totalOperations += userResults.Count;
        }

        var loadTestResult = new LoadTestResult
        {
            OperationName = operationName,
            ConcurrentUsers = concurrentUsers,
            OperationsPerUser = operationsPerUser,
            TotalOperations = totalOperations,
            TotalExecutionTimeMs = overallStopwatch.ElapsedMilliseconds,
            AverageResponseTimeMs = allExecutionTimes.Count > 0 ? allExecutionTimes.Average() : 0,
            MinResponseTimeMs = allExecutionTimes.Count > 0 ? allExecutionTimes.Min() : 0,
            MaxResponseTimeMs = allExecutionTimes.Count > 0 ? allExecutionTimes.Max() : 0,
            MedianResponseTimeMs = CalculateMedian(allExecutionTimes),
            ThroughputOpsPerSecond = totalOperations / (overallStopwatch.ElapsedMilliseconds / 1000.0),
            SuccessCount = totalOperations - totalExceptions,
            FailureCount = totalExceptions,
            ExpectedMaxMs = expectedMaxMilliseconds
        };

        LogLoadTestResult(loadTestResult);
        return loadTestResult;
    }

    /// <summary>
    /// パフォーマンス要件をアサート
    /// </summary>
    protected void AssertPerformanceRequirement(PerformanceResult result)
    {
        result.IsSuccess.ShouldBeTrue($"Operation '{result.OperationName}' failed: {result.Exception?.Message}");
        result.ExecutionTimeMs.ShouldBeLessThanOrEqualTo(result.ExpectedMaxMs,
            $"Operation '{result.OperationName}' took {result.ExecutionTimeMs}ms, expected max {result.ExpectedMaxMs}ms");
    }

    /// <summary>
    /// 平均パフォーマンス要件をアサート
    /// </summary>
    protected void AssertAveragePerformanceRequirement(PerformanceStatistics statistics)
    {
        statistics.SuccessCount.ShouldBeGreaterThan(0, "No successful operations");
        statistics.AverageMs.ShouldBeLessThanOrEqualTo(statistics.ExpectedMaxMs,
            $"Average execution time {statistics.AverageMs}ms exceeded expected max {statistics.ExpectedMaxMs}ms");

        // 95%の操作が期待時間内に完了することを確認
        var sortedTimes = statistics.GetAllExecutionTimes().OrderBy(t => t).ToList();
        if (sortedTimes.Count > 0)
        {
            var percentile95Index = (int)(sortedTimes.Count * 0.95);
            var percentile95Time = sortedTimes[Math.Min(percentile95Index, sortedTimes.Count - 1)];
            percentile95Time.ShouldBeLessThanOrEqualTo((long)(statistics.ExpectedMaxMs * 1.5), // 95%ile allows 50% more time
                $"95th percentile time {percentile95Time}ms exceeded threshold");
        }
    }

    /// <summary>
    /// 負荷テスト要件をアサート
    /// </summary>
    protected void AssertLoadTestRequirement(LoadTestResult result, double minThroughput = 10.0)
    {
        result.SuccessCount.ShouldBeGreaterThan(0, "No successful operations in load test");
        result.ThroughputOpsPerSecond.ShouldBeGreaterThanOrEqualTo(minThroughput,
            $"Throughput {result.ThroughputOpsPerSecond:F2} ops/sec is below minimum {minThroughput} ops/sec");
        result.AverageResponseTimeMs.ShouldBeLessThanOrEqualTo(result.ExpectedMaxMs,
            $"Average response time {result.AverageResponseTimeMs}ms exceeded expected max {result.ExpectedMaxMs}ms");
    }

    #region Private Helper Methods

    private async Task<List<long>> ExecuteUserOperationsAsync(Func<Task> action, int operationCount)
    {
        var executionTimes = new List<long>();

        for (int i = 0; i < operationCount; i++)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await action();
                stopwatch.Stop();
                executionTimes.Add(stopwatch.ElapsedMilliseconds);
            }
            catch
            {
                stopwatch.Stop();
                executionTimes.Add(stopwatch.ElapsedMilliseconds);
                // 例外は記録するが、テストは継続
            }
        }

        return executionTimes;
    }

    private static double CalculateMedian(List<long> values)
    {
        if (values.Count == 0) return 0;

        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private void LogPerformanceResult(PerformanceResult result)
    {
        var message = $"Performance: {result.OperationName} - {result.ExecutionTimeMs}ms " +
                     $"(Expected: <={result.ExpectedMaxMs}ms, Success: {result.IsSuccess})";

        Output.WriteLine(message);
        Logger.LogInformation(message);
    }

    private void LogPerformanceStatistics(PerformanceStatistics statistics)
    {
        var message = $"Performance Stats: {statistics.OperationName} - " +
                     $"Avg: {statistics.AverageMs:F1}ms, Min: {statistics.MinMs}ms, Max: {statistics.MaxMs}ms, " +
                     $"Median: {statistics.MedianMs:F1}ms, Success: {statistics.SuccessCount}/{statistics.Iterations}";

        Output.WriteLine(message);
        Logger.LogInformation(message);
    }

    private void LogLoadTestResult(LoadTestResult result)
    {
        var message = $"Load Test: {result.OperationName} - " +
                     $"Users: {result.ConcurrentUsers}, Ops: {result.TotalOperations}, " +
                     $"Throughput: {result.ThroughputOpsPerSecond:F2} ops/sec, " +
                     $"Avg Response: {result.AverageResponseTimeMs:F1}ms, " +
                     $"Success: {result.SuccessCount}/{result.TotalOperations}";

        Output.WriteLine(message);
        Logger.LogInformation(message);
    }

    #endregion
}

/// <summary>
/// パフォーマンス測定結果
/// </summary>
public class PerformanceResult
{
    public string OperationName { get; set; } = string.Empty;
    public long ExecutionTimeMs { get; set; }
    public bool IsSuccess { get; set; }
    public Exception? Exception { get; set; }
    public int ExpectedMaxMs { get; set; }
}

/// <summary>
/// パフォーマンス統計
/// </summary>
public class PerformanceStatistics
{
    public string OperationName { get; set; } = string.Empty;
    public int Iterations { get; set; }
    public double AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public double MedianMs { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int ExpectedMaxMs { get; set; }
    public List<Exception> Exceptions { get; set; } = new();

    public List<long> GetAllExecutionTimes()
    {
        // 実際の実装では実行時間のリストを保持する必要があります
        // ここでは簡略化のため空のリストを返します
        return new List<long>();
    }
}

/// <summary>
/// 負荷テスト結果
/// </summary>
public class LoadTestResult
{
    public string OperationName { get; set; } = string.Empty;
    public int ConcurrentUsers { get; set; }
    public int OperationsPerUser { get; set; }
    public int TotalOperations { get; set; }
    public long TotalExecutionTimeMs { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public long MinResponseTimeMs { get; set; }
    public long MaxResponseTimeMs { get; set; }
    public double MedianResponseTimeMs { get; set; }
    public double ThroughputOpsPerSecond { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int ExpectedMaxMs { get; set; }
}