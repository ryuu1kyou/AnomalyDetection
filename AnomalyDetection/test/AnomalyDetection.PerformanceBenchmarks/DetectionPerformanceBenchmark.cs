using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.Logging.Abstractions;
using AnomalyDetection.AnomalyDetection.Services;

namespace AnomalyDetection.PerformanceBenchmarks;

/// <summary>
/// 異常検知のパフォーマンステスト
/// 目標: 閾値計算が100ms未満で完了すること
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class DetectionPerformanceBenchmark
{
    private StatisticalThresholdOptimizer _optimizer = null!;
    private List<double> _sampleData = null!;
    private ThresholdOptimizationConfig _config = null!;

    [Params(100, 500, 1000)]
    public int SampleSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _optimizer = new StatisticalThresholdOptimizer(NullLogger<StatisticalThresholdOptimizer>.Instance);

        // Generate realistic sample data with some anomalies
        var random = new Random(42);
        _sampleData = new List<double>();

        for (int i = 0; i < SampleSize; i++)
        {
            // Normal data: mean=50, stddev=10
            var value = random.NextDouble() * 10 + 50;

            // Add some anomalies (5% of data)
            if (i % 20 == 0)
            {
                value = random.NextDouble() * 20 + 90; // Outlier
            }

            _sampleData.Add(value);
        }

        // Setup config
        _config = new ThresholdOptimizationConfig
        {
            TargetFalsePositiveRate = 0.05,
            TargetTruePositiveRate = 0.95,
            ConfidenceLevel = 0.95,
            UpperPercentile = 0.95,
            LowerPercentile = 0.05,
            MinimumSampleSize = 100,
            ConsiderSeasonality = false
        };
    }

    /// <summary>
    /// 最適閾値計算のパフォーマンステスト
    /// 目標: 100ms未満
    /// </summary>
    [Benchmark]
    public async Task<OptimalThresholdResult> CalculateOptimalThreshold()
    {
        return await _optimizer.CalculateOptimalThresholdAsync(_sampleData, _config);
    }

    /// <summary>
    /// 外れ値検出のパフォーマンステスト（IQR法）
    /// </summary>
    [Benchmark]
    public async Task<OutlierDetectionResult> DetectOutliers_IQR()
    {
        return await _optimizer.DetectOutliersAsync(_sampleData, OutlierDetectionMethod.IQR);
    }

    /// <summary>
    /// 外れ値検出のパフォーマンステスト（Z-Score法）
    /// </summary>
    [Benchmark]
    public async Task<OutlierDetectionResult> DetectOutliers_ZScore()
    {
        return await _optimizer.DetectOutliersAsync(_sampleData, OutlierDetectionMethod.ZScore);
    }

    /// <summary>
    /// 統計情報の計算（平均、標準偏差、中央値など）
    /// </summary>
    [Benchmark]
    public void CalculateStatistics()
    {
        var mean = _sampleData.Average();
        var variance = _sampleData.Select(x => Math.Pow(x - mean, 2)).Average();
        var stdDev = Math.Sqrt(variance);
        var sorted = _sampleData.OrderBy(x => x).ToList();
        var median = sorted[sorted.Count / 2];
    }
}