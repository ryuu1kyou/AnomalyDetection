using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 検出結果統計情報
/// </summary>
public class DetectionResultStatistics : ValueObject
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int TotalDetections { get; private set; }
    public int ResolvedDetections { get; private set; }
    public int UnresolvedDetections { get; private set; }
    public int FalsePositives { get; private set; }
    public int ValidatedDetections { get; private set; }
    public Dictionary<AnomalyType, int> DetectionsByType { get; private set; }
    public Dictionary<AnomalyLevel, int> DetectionsByLevel { get; private set; }
    public Dictionary<ResolutionStatus, int> DetectionsByStatus { get; private set; }
    public double AverageDetectionDurationMs { get; private set; }
    public double MedianDetectionDurationMs { get; private set; }
    public double FalsePositiveRate { get; private set; }
    public double ResolutionRate { get; private set; }
    public double ValidationRate { get; private set; }

    protected DetectionResultStatistics() 
    {
        DetectionsByType = new Dictionary<AnomalyType, int>();
        DetectionsByLevel = new Dictionary<AnomalyLevel, int>();
        DetectionsByStatus = new Dictionary<ResolutionStatus, int>();
    }

    public DetectionResultStatistics(
        DateTime startDate,
        DateTime endDate,
        int totalDetections,
        int resolvedDetections,
        int unresolvedDetections,
        int falsePositives,
        int validatedDetections,
        Dictionary<AnomalyType, int> detectionsByType,
        Dictionary<AnomalyLevel, int> detectionsByLevel,
        Dictionary<ResolutionStatus, int> detectionsByStatus,
        double averageDetectionDurationMs,
        double medianDetectionDurationMs)
    {
        StartDate = startDate;
        EndDate = endDate;
        TotalDetections = ValidateNonNegative(totalDetections, nameof(totalDetections));
        ResolvedDetections = ValidateNonNegative(resolvedDetections, nameof(resolvedDetections));
        UnresolvedDetections = ValidateNonNegative(unresolvedDetections, nameof(unresolvedDetections));
        FalsePositives = ValidateNonNegative(falsePositives, nameof(falsePositives));
        ValidatedDetections = ValidateNonNegative(validatedDetections, nameof(validatedDetections));
        DetectionsByType = detectionsByType ?? new Dictionary<AnomalyType, int>();
        DetectionsByLevel = detectionsByLevel ?? new Dictionary<AnomalyLevel, int>();
        DetectionsByStatus = detectionsByStatus ?? new Dictionary<ResolutionStatus, int>();
        AverageDetectionDurationMs = averageDetectionDurationMs;
        MedianDetectionDurationMs = medianDetectionDurationMs;

        // 計算されるメトリクス
        FalsePositiveRate = TotalDetections > 0 ? (double)FalsePositives / TotalDetections : 0.0;
        ResolutionRate = TotalDetections > 0 ? (double)ResolvedDetections / TotalDetections : 0.0;
        ValidationRate = TotalDetections > 0 ? (double)ValidatedDetections / TotalDetections : 0.0;
    }

    public TimeSpan GetAnalysisPeriod()
    {
        return EndDate - StartDate;
    }

    public AnomalyType GetMostFrequentAnomalyType()
    {
        return DetectionsByType.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
    }

    public AnomalyLevel GetMostFrequentAnomalyLevel()
    {
        return DetectionsByLevel.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
    }

    public bool HasHighFalsePositiveRate()
    {
        return FalsePositiveRate > 0.1; // 10%以上を高い誤検出率とする
    }

    public bool HasGoodResolutionRate()
    {
        return ResolutionRate > 0.8; // 80%以上を良い解決率とする
    }

    public bool HasGoodValidationRate()
    {
        return ValidationRate > 0.7; // 70%以上を良い検証率とする
    }

    public int GetCriticalDetections()
    {
        return DetectionsByLevel.Where(kvp => kvp.Key >= AnomalyLevel.Critical).Sum(kvp => kvp.Value);
    }

    public double GetCriticalDetectionRate()
    {
        return TotalDetections > 0 ? (double)GetCriticalDetections() / TotalDetections : 0.0;
    }

    private static int ValidateNonNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative");
        return value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartDate;
        yield return EndDate;
        yield return TotalDetections;
        yield return ResolvedDetections;
        yield return UnresolvedDetections;
        yield return FalsePositives;
        yield return ValidatedDetections;
        yield return string.Join(",", DetectionsByType.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return string.Join(",", DetectionsByLevel.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return string.Join(",", DetectionsByStatus.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return AverageDetectionDurationMs;
        yield return MedianDetectionDurationMs;
        yield return FalsePositiveRate;
        yield return ResolutionRate;
        yield return ValidationRate;
    }
}

/// <summary>
/// 検出時間統計情報
/// </summary>
public class DetectionTimeStatistics : ValueObject
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int TotalMeasurements { get; private set; }
    public double AverageDetectionTimeMs { get; private set; }
    public double MedianDetectionTimeMs { get; private set; }
    public double MinDetectionTimeMs { get; private set; }
    public double MaxDetectionTimeMs { get; private set; }
    public double StandardDeviationMs { get; private set; }
    public double Percentile95Ms { get; private set; }
    public double Percentile99Ms { get; private set; }
    public Dictionary<AnomalyType, double> AverageTimeByType { get; private set; }

    protected DetectionTimeStatistics() 
    {
        AverageTimeByType = new Dictionary<AnomalyType, double>();
    }

    public DetectionTimeStatistics(
        DateTime startDate,
        DateTime endDate,
        int totalMeasurements,
        double averageDetectionTimeMs,
        double medianDetectionTimeMs,
        double minDetectionTimeMs,
        double maxDetectionTimeMs,
        double standardDeviationMs,
        double percentile95Ms,
        double percentile99Ms,
        Dictionary<AnomalyType, double>? averageTimeByType = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        TotalMeasurements = ValidateNonNegative(totalMeasurements, nameof(totalMeasurements));
        AverageDetectionTimeMs = ValidateNonNegative(averageDetectionTimeMs, nameof(averageDetectionTimeMs));
        MedianDetectionTimeMs = ValidateNonNegative(medianDetectionTimeMs, nameof(medianDetectionTimeMs));
        MinDetectionTimeMs = ValidateNonNegative(minDetectionTimeMs, nameof(minDetectionTimeMs));
        MaxDetectionTimeMs = ValidateNonNegative(maxDetectionTimeMs, nameof(maxDetectionTimeMs));
        StandardDeviationMs = ValidateNonNegative(standardDeviationMs, nameof(standardDeviationMs));
        Percentile95Ms = ValidateNonNegative(percentile95Ms, nameof(percentile95Ms));
        Percentile99Ms = ValidateNonNegative(percentile99Ms, nameof(percentile99Ms));
        AverageTimeByType = averageTimeByType ?? new Dictionary<AnomalyType, double>();
    }

    public bool HasGoodPerformance()
    {
        return AverageDetectionTimeMs < 100.0; // 100ms以下を良いパフォーマンスとする
    }

    public bool HasConsistentPerformance()
    {
        return StandardDeviationMs < AverageDetectionTimeMs * 0.5; // 標準偏差が平均の50%以下
    }

    public AnomalyType GetSlowestAnomalyType()
    {
        return AverageTimeByType.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
    }

    public AnomalyType GetFastestAnomalyType()
    {
        return AverageTimeByType.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;
    }

    public TimeSpan GetAverageDetectionTime()
    {
        return TimeSpan.FromMilliseconds(AverageDetectionTimeMs);
    }

    public TimeSpan GetMedianDetectionTime()
    {
        return TimeSpan.FromMilliseconds(MedianDetectionTimeMs);
    }

    private static double ValidateNonNegative(double value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative");
        return value;
    }

    private static int ValidateNonNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative");
        return value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartDate;
        yield return EndDate;
        yield return TotalMeasurements;
        yield return AverageDetectionTimeMs;
        yield return MedianDetectionTimeMs;
        yield return MinDetectionTimeMs;
        yield return MaxDetectionTimeMs;
        yield return StandardDeviationMs;
        yield return Percentile95Ms;
        yield return Percentile99Ms;
        yield return string.Join(",", AverageTimeByType.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }
}