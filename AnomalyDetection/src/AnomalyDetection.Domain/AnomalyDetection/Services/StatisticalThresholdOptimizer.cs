using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 統計的閾値最適化サービス実装
/// 機械学習ベースの閾値最適化アルゴリズムを提供
/// </summary>
public class StatisticalThresholdOptimizer : DomainService, IStatisticalThresholdOptimizer, ITransientDependency
{
    private readonly ILogger<StatisticalThresholdOptimizer> _logger;

    public StatisticalThresholdOptimizer(ILogger<StatisticalThresholdOptimizer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 履歴データから最適閾値を計算（分位数ベース）
    /// </summary>
    public async Task<OptimalThresholdResult> CalculateOptimalThresholdAsync(
        List<double> historicalValues,
        ThresholdOptimizationConfig config)
    {
        _logger.LogInformation("Calculating optimal threshold for {Count} samples", historicalValues.Count);

        if (historicalValues.Count < config.MinimumSampleSize)
        {
            throw new InvalidOperationException(
                $"Insufficient samples: {historicalValues.Count} < {config.MinimumSampleSize}");
        }

        // 基本統計量計算
        var sortedValues = historicalValues.OrderBy(v => v).ToList();
        var mean = CalculateMean(sortedValues);
        var stdDev = CalculateStandardDeviation(sortedValues, mean);
        var median = CalculateMedian(sortedValues);

        // 分位数ベースの閾値計算
        var upperThreshold = CalculatePercentile(sortedValues, config.UpperPercentile);
        var lowerThreshold = CalculatePercentile(sortedValues, config.LowerPercentile);

        // 外れ値を除外した場合の統計量で検証
        var cleanedValues = RemoveOutliers(sortedValues, mean, stdDev);
        var cleanedMean = CalculateMean(cleanedValues);
        var cleanedStdDev = CalculateStandardDeviation(cleanedValues, cleanedMean);

        // シグマルール調整（3σルール）
        var adjustedUpper = Math.Min(upperThreshold, cleanedMean + 3 * cleanedStdDev);
        var adjustedLower = Math.Max(lowerThreshold, cleanedMean - 3 * cleanedStdDev);

        // 期待される性能メトリクス計算
        var expectedFPR = CalculateExpectedFalsePositiveRate(
            sortedValues, adjustedUpper, adjustedLower);
        var expectedTPR = config.TargetTruePositiveRate; // 簡易推定

        await Task.CompletedTask;

        return new OptimalThresholdResult
        {
            RecommendedUpperThreshold = adjustedUpper,
            RecommendedLowerThreshold = adjustedLower,
            Mean = mean,
            StandardDeviation = stdDev,
            Median = median,
            SampleSize = historicalValues.Count,
            ConfidenceLevel = config.ConfidenceLevel,
            ExpectedFalsePositiveRate = expectedFPR,
            ExpectedTruePositiveRate = expectedTPR,
            OptimizationMethod = "Percentile-based with 3-sigma adjustment",
            Metadata = new Dictionary<string, object>
            {
                { "CleanedSampleSize", cleanedValues.Count },
                { "OutliersRemoved", sortedValues.Count - cleanedValues.Count },
                { "CleanedMean", cleanedMean },
                { "CleanedStdDev", cleanedStdDev }
            }
        };
    }

    /// <summary>
    /// 外れ値検出による異常閾値推定（IQRメソッド）
    /// </summary>
    public async Task<OutlierDetectionResult> DetectOutliersAsync(
        List<double> values,
        OutlierDetectionMethod method = OutlierDetectionMethod.IQR)
    {
        _logger.LogInformation("Detecting outliers using {Method} method for {Count} samples",
            method, values.Count);

        var outliers = new List<OutlierDataPoint>();
        double upperBound = 0;
        double lowerBound = 0;

        switch (method)
        {
            case OutlierDetectionMethod.IQR:
                (outliers, upperBound, lowerBound) = DetectOutliersIQR(values);
                break;

            case OutlierDetectionMethod.ZScore:
                (outliers, upperBound, lowerBound) = DetectOutliersZScore(values);
                break;

            case OutlierDetectionMethod.ModifiedZScore:
                (outliers, upperBound, lowerBound) = DetectOutliersModifiedZScore(values);
                break;

            case OutlierDetectionMethod.MovingAverage:
                (outliers, upperBound, lowerBound) = DetectOutliersMovingAverage(values);
                break;
        }

        await Task.CompletedTask;

        return new OutlierDetectionResult
        {
            Outliers = outliers,
            UpperBound = upperBound,
            LowerBound = lowerBound,
            Method = method,
            TotalSamples = values.Count,
            OutlierCount = outliers.Count,
            OutlierPercentage = (double)outliers.Count / values.Count * 100
        };
    }

    /// <summary>
    /// トレンド分析による動的閾値調整
    /// </summary>
    public async Task<DynamicThresholdResult> CalculateDynamicThresholdAsync(
        List<TimeSeriesDataPoint> timeSeriesData,
        int windowSize = 100)
    {
        _logger.LogInformation("Calculating dynamic threshold with window size {WindowSize}",
            windowSize);

        var thresholds = new List<TimeSeriesThreshold>();
        var values = timeSeriesData.Select(d => d.Value).ToList();

        // トレンド分析
        var trendInfo = AnalyzeTrend(values);

        // 移動窓による動的閾値計算
        for (int i = windowSize; i < timeSeriesData.Count; i++)
        {
            var window = values.Skip(i - windowSize).Take(windowSize).ToList();
            var mean = CalculateMean(window);
            var stdDev = CalculateStandardDeviation(window, mean);

            // トレンドベースの予測値
            var predictedValue = mean + (trendInfo.Slope * (i - windowSize / 2.0));

            // 動的閾値（予測値 ± 2σ）
            var confidenceInterval = 2 * stdDev;

            thresholds.Add(new TimeSeriesThreshold
            {
                Timestamp = timeSeriesData[i].Timestamp,
                UpperThreshold = predictedValue + confidenceInterval,
                LowerThreshold = predictedValue - confidenceInterval,
                PredictedValue = predictedValue,
                ConfidenceInterval = confidenceInterval
            });
        }

        await Task.CompletedTask;

        return new DynamicThresholdResult
        {
            Thresholds = thresholds,
            TrendInfo = trendInfo,
            BaselineValue = CalculateMean(values),
            WindowSize = windowSize
        };
    }

    /// <summary>
    /// 多変量分析による閾値最適化（複数信号の相関考慮）
    /// </summary>
    public async Task<MultivariateThresholdResult> OptimizeMultivariateThresholdAsync(
        Dictionary<string, List<double>> signalValues,
        double correlationThreshold = 0.7)
    {
        _logger.LogInformation("Optimizing multivariate thresholds for {Count} signals",
            signalValues.Count);

        var result = new MultivariateThresholdResult();
        var config = new ThresholdOptimizationConfig();

        // 各信号の閾値を個別に計算
        foreach (var (signalName, values) in signalValues)
        {
            if (values.Count >= config.MinimumSampleSize)
            {
                var threshold = await CalculateOptimalThresholdAsync(values, config);
                result.SignalThresholds[signalName] = threshold;
            }
        }

        // 信号間の相関分析
        var correlations = CalculateSignalCorrelations(signalValues);
        result.Correlations = correlations
            .Where(c => Math.Abs(c.CorrelationCoefficient) >= correlationThreshold)
            .ToList();

        // 高相関の信号グループを特定
        result.RecommendedSignalGroups = IdentifyCorrelatedSignalGroups(
            correlations, correlationThreshold);

        return result;
    }

    #region Private Helper Methods

    private double CalculateMean(List<double> values)
    {
        return values.Average();
    }

    private double CalculateMedian(List<double> sortedValues)
    {
        int n = sortedValues.Count;
        if (n % 2 == 1)
            return sortedValues[n / 2];
        return (sortedValues[n / 2 - 1] + sortedValues[n / 2]) / 2.0;
    }

    private double CalculateStandardDeviation(List<double> values, double mean)
    {
        var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
        return Math.Sqrt(variance);
    }

    private double CalculatePercentile(List<double> sortedValues, double percentile)
    {
        int n = sortedValues.Count;
        double index = percentile * (n - 1);
        int lowerIndex = (int)Math.Floor(index);
        int upperIndex = (int)Math.Ceiling(index);

        if (lowerIndex == upperIndex)
            return sortedValues[lowerIndex];

        double fraction = index - lowerIndex;
        return sortedValues[lowerIndex] * (1 - fraction) + sortedValues[upperIndex] * fraction;
    }

    private List<double> RemoveOutliers(List<double> values, double mean, double stdDev)
    {
        // 3σルールで外れ値除外
        var threshold = 3 * stdDev;
        return values.Where(v => Math.Abs(v - mean) <= threshold).ToList();
    }

    private double CalculateExpectedFalsePositiveRate(
        List<double> values, double upperThreshold, double lowerThreshold)
    {
        var outsideCount = values.Count(v => v > upperThreshold || v < lowerThreshold);
        return (double)outsideCount / values.Count;
    }

    private (List<OutlierDataPoint>, double, double) DetectOutliersIQR(List<double> values)
    {
        var sortedValues = values.OrderBy(v => v).ToList();
        var q1 = CalculatePercentile(sortedValues, 0.25);
        var q3 = CalculatePercentile(sortedValues, 0.75);
        var iqr = q3 - q1;

        var lowerBound = q1 - 1.5 * iqr;
        var upperBound = q3 + 1.5 * iqr;

        var outliers = new List<OutlierDataPoint>();
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] < lowerBound || values[i] > upperBound)
            {
                outliers.Add(new OutlierDataPoint
                {
                    Index = i,
                    Value = values[i],
                    DeviationScore = Math.Min(
                        Math.Abs(values[i] - lowerBound) / iqr,
                        Math.Abs(values[i] - upperBound) / iqr),
                    Reason = values[i] < lowerBound ? "Below Q1-1.5*IQR" : "Above Q3+1.5*IQR"
                });
            }
        }

        return (outliers, upperBound, lowerBound);
    }

    private (List<OutlierDataPoint>, double, double) DetectOutliersZScore(List<double> values)
    {
        var mean = CalculateMean(values);
        var stdDev = CalculateStandardDeviation(values, mean);

        var threshold = 3.0; // 3σ
        var lowerBound = mean - threshold * stdDev;
        var upperBound = mean + threshold * stdDev;

        var outliers = new List<OutlierDataPoint>();
        for (int i = 0; i < values.Count; i++)
        {
            var zScore = Math.Abs((values[i] - mean) / stdDev);
            if (zScore > threshold)
            {
                outliers.Add(new OutlierDataPoint
                {
                    Index = i,
                    Value = values[i],
                    DeviationScore = zScore,
                    Reason = $"Z-score {zScore:F2} exceeds threshold {threshold}"
                });
            }
        }

        return (outliers, upperBound, lowerBound);
    }

    private (List<OutlierDataPoint>, double, double) DetectOutliersModifiedZScore(
        List<double> values)
    {
        var sortedValues = values.OrderBy(v => v).ToList();
        var median = CalculateMedian(sortedValues);

        // MAD (Median Absolute Deviation)
        var deviations = values.Select(v => Math.Abs(v - median)).ToList();
        var mad = CalculateMedian(deviations.OrderBy(d => d).ToList());

        var threshold = 3.5; // Modified Z-score threshold
        var outliers = new List<OutlierDataPoint>();

        for (int i = 0; i < values.Count; i++)
        {
            var modifiedZScore = 0.6745 * (values[i] - median) / mad;
            if (Math.Abs(modifiedZScore) > threshold)
            {
                outliers.Add(new OutlierDataPoint
                {
                    Index = i,
                    Value = values[i],
                    DeviationScore = Math.Abs(modifiedZScore),
                    Reason = $"Modified Z-score {Math.Abs(modifiedZScore):F2} exceeds {threshold}"
                });
            }
        }

        var lowerBound = median - threshold * mad / 0.6745;
        var upperBound = median + threshold * mad / 0.6745;

        return (outliers, upperBound, lowerBound);
    }

    private (List<OutlierDataPoint>, double, double) DetectOutliersMovingAverage(
        List<double> values, int windowSize = 20)
    {
        var outliers = new List<OutlierDataPoint>();
        var movingAvg = new List<double>();
        var movingStdDev = new List<double>();

        // 移動平均と移動標準偏差計算
        for (int i = 0; i < values.Count; i++)
        {
            var start = Math.Max(0, i - windowSize / 2);
            var end = Math.Min(values.Count, i + windowSize / 2);
            var window = values.Skip(start).Take(end - start).ToList();

            var avg = CalculateMean(window);
            var stdDev = CalculateStandardDeviation(window, avg);

            movingAvg.Add(avg);
            movingStdDev.Add(stdDev);

            // 2σを超える点を外れ値とする
            if (Math.Abs(values[i] - avg) > 2 * stdDev)
            {
                outliers.Add(new OutlierDataPoint
                {
                    Index = i,
                    Value = values[i],
                    DeviationScore = Math.Abs(values[i] - avg) / stdDev,
                    Reason = $"Deviates from moving average by {Math.Abs(values[i] - avg):F2}"
                });
            }
        }

        var globalMean = CalculateMean(values);
        var globalStdDev = CalculateStandardDeviation(values, globalMean);
        var upperBound = globalMean + 2 * globalStdDev;
        var lowerBound = globalMean - 2 * globalStdDev;

        return (outliers, upperBound, lowerBound);
    }

    private TrendAnalysis AnalyzeTrend(List<double> values)
    {
        int n = values.Count;
        if (n < 10)
        {
            return new TrendAnalysis
            {
                Direction = TrendDirection.Stable,
                Slope = 0,
                Volatility = 0,
                IsStationary = true,
                AutocorrelationLag1 = 0
            };
        }

        // 線形回帰で傾き計算
        var xValues = Enumerable.Range(0, n).Select(i => (double)i).ToList();
        var slope = CalculateLinearRegressionSlope(xValues, values);

        // ボラティリティ（標準偏差）
        var mean = CalculateMean(values);
        var volatility = CalculateStandardDeviation(values, mean);

        // 定常性チェック（簡易版：分散が安定しているか）
        var firstHalfVariance = CalculateVariance(values.Take(n / 2).ToList());
        var secondHalfVariance = CalculateVariance(values.Skip(n / 2).ToList());
        var isStationary = Math.Abs(firstHalfVariance - secondHalfVariance) /
            Math.Max(firstHalfVariance, secondHalfVariance) < 0.3;

        // 自己相関（lag=1）
        var autocorr = CalculateAutocorrelation(values, 1);

        // トレンド方向判定
        var direction = TrendDirection.Stable;
        if (Math.Abs(slope) > volatility * 0.1)
        {
            direction = slope > 0 ? TrendDirection.Increasing : TrendDirection.Decreasing;
        }
        else if (autocorr < -0.3)
        {
            direction = TrendDirection.Oscillating;
        }

        return new TrendAnalysis
        {
            Direction = direction,
            Slope = slope,
            Volatility = volatility,
            IsStationary = isStationary,
            AutocorrelationLag1 = autocorr
        };
    }

    private double CalculateLinearRegressionSlope(List<double> x, List<double> y)
    {
        int n = x.Count;
        var xMean = CalculateMean(x);
        var yMean = CalculateMean(y);

        var numerator = 0.0;
        var denominator = 0.0;

        for (int i = 0; i < n; i++)
        {
            numerator += (x[i] - xMean) * (y[i] - yMean);
            denominator += Math.Pow(x[i] - xMean, 2);
        }

        return denominator != 0 ? numerator / denominator : 0;
    }

    private double CalculateVariance(List<double> values)
    {
        var mean = CalculateMean(values);
        return values.Select(v => Math.Pow(v - mean, 2)).Average();
    }

    private double CalculateAutocorrelation(List<double> values, int lag)
    {
        int n = values.Count;
        if (lag >= n)
            return 0;

        var mean = CalculateMean(values);
        var variance = CalculateVariance(values);

        if (variance == 0)
            return 0;

        var numerator = 0.0;
        for (int i = 0; i < n - lag; i++)
        {
            numerator += (values[i] - mean) * (values[i + lag] - mean);
        }

        return numerator / ((n - lag) * variance);
    }

    private List<SignalCorrelation> CalculateSignalCorrelations(
        Dictionary<string, List<double>> signalValues)
    {
        var correlations = new List<SignalCorrelation>();
        var signalNames = signalValues.Keys.ToList();

        for (int i = 0; i < signalNames.Count; i++)
        {
            for (int j = i + 1; j < signalNames.Count; j++)
            {
                var signal1 = signalNames[i];
                var signal2 = signalNames[j];

                var minLength = Math.Min(
                    signalValues[signal1].Count,
                    signalValues[signal2].Count);

                var values1 = signalValues[signal1].Take(minLength).ToList();
                var values2 = signalValues[signal2].Take(minLength).ToList();

                var correlation = CalculatePearsonCorrelation(values1, values2);

                correlations.Add(new SignalCorrelation
                {
                    Signal1 = signal1,
                    Signal2 = signal2,
                    CorrelationCoefficient = correlation,
                    IsSignificant = Math.Abs(correlation) >= 0.5
                });
            }
        }

        return correlations;
    }

    private double CalculatePearsonCorrelation(List<double> x, List<double> y)
    {
        int n = x.Count;
        if (n != y.Count || n == 0)
            return 0;

        var xMean = CalculateMean(x);
        var yMean = CalculateMean(y);

        var numerator = 0.0;
        var xSumSquares = 0.0;
        var ySumSquares = 0.0;

        for (int i = 0; i < n; i++)
        {
            var xDiff = x[i] - xMean;
            var yDiff = y[i] - yMean;

            numerator += xDiff * yDiff;
            xSumSquares += xDiff * xDiff;
            ySumSquares += yDiff * yDiff;
        }

        var denominator = Math.Sqrt(xSumSquares * ySumSquares);
        return denominator != 0 ? numerator / denominator : 0;
    }

    private List<string> IdentifyCorrelatedSignalGroups(
        List<SignalCorrelation> correlations,
        double threshold)
    {
        var groups = new List<string>();
        var processedSignals = new HashSet<string>();

        var strongCorrelations = correlations
            .Where(c => Math.Abs(c.CorrelationCoefficient) >= threshold)
            .OrderByDescending(c => Math.Abs(c.CorrelationCoefficient))
            .ToList();

        foreach (var corr in strongCorrelations)
        {
            if (!processedSignals.Contains(corr.Signal1) &&
                !processedSignals.Contains(corr.Signal2))
            {
                groups.Add($"Group: {corr.Signal1} <-> {corr.Signal2} (r={corr.CorrelationCoefficient:F2})");
                processedSignals.Add(corr.Signal1);
                processedSignals.Add(corr.Signal2);
            }
        }

        return groups;
    }

    #endregion
}
