using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 検出精度評価メトリクス
/// </summary>
public class DetectionAccuracyMetrics : ValueObject
{
    public Guid DetectionLogicId { get; private set; }
    public DateTime AnalysisStartDate { get; private set; }
    public DateTime AnalysisEndDate { get; private set; }
    public int TotalDetections { get; private set; }
    public int TruePositives { get; private set; }
    public int FalsePositives { get; private set; }
    public int TrueNegatives { get; private set; }
    public int FalseNegatives { get; private set; }
    public double Precision { get; private set; }
    public double Recall { get; private set; }
    public double F1Score { get; private set; }
    public double Accuracy { get; private set; }
    public double Specificity { get; private set; }
    public double AverageDetectionTimeMs { get; private set; }
    public double MedianDetectionTimeMs { get; private set; }
    public List<AccuracyByAnomalyType> AccuracyByType { get; private set; }
    public List<AccuracyByTimeRange> AccuracyByTime { get; private set; }
    public string PerformanceSummary { get; private set; }

    protected DetectionAccuracyMetrics() 
    {
        AccuracyByType = new List<AccuracyByAnomalyType>();
        AccuracyByTime = new List<AccuracyByTimeRange>();
        PerformanceSummary = string.Empty;
    }

    public DetectionAccuracyMetrics(
        Guid detectionLogicId,
        DateTime analysisStartDate,
        DateTime analysisEndDate,
        int totalDetections,
        int truePositives,
        int falsePositives,
        int trueNegatives,
        int falseNegatives,
        double averageDetectionTimeMs,
        double medianDetectionTimeMs,
        List<AccuracyByAnomalyType>? accuracyByType = null,
        List<AccuracyByTimeRange>? accuracyByTime = null,
        string? performanceSummary = null)
    {
        DetectionLogicId = detectionLogicId;
        AnalysisStartDate = analysisStartDate;
        AnalysisEndDate = analysisEndDate;
        TotalDetections = totalDetections;
        TruePositives = truePositives;
        FalsePositives = falsePositives;
        TrueNegatives = trueNegatives;
        FalseNegatives = falseNegatives;
        AverageDetectionTimeMs = averageDetectionTimeMs;
        MedianDetectionTimeMs = medianDetectionTimeMs;
        AccuracyByType = accuracyByType ?? new List<AccuracyByAnomalyType>();
        AccuracyByTime = accuracyByTime ?? new List<AccuracyByTimeRange>();
        PerformanceSummary = performanceSummary ?? string.Empty;

        // 計算されるメトリクス
        Precision = CalculatePrecision();
        Recall = CalculateRecall();
        F1Score = CalculateF1Score();
        Accuracy = CalculateAccuracy();
        Specificity = CalculateSpecificity();
    }

    public bool IsHighPerformance()
    {
        return F1Score > 0.8 && Accuracy > 0.85;
    }

    public bool HasAcceptablePerformance()
    {
        return F1Score > 0.6 && Accuracy > 0.7;
    }

    public double GetFalsePositiveRate()
    {
        return FalsePositives + TrueNegatives > 0 ? (double)FalsePositives / (FalsePositives + TrueNegatives) : 0.0;
    }

    public double GetFalseNegativeRate()
    {
        return FalseNegatives + TruePositives > 0 ? (double)FalseNegatives / (FalseNegatives + TruePositives) : 0.0;
    }

    public AccuracyByAnomalyType? GetWorstPerformingAnomalyType()
    {
        return AccuracyByType.OrderBy(a => a.F1Score).FirstOrDefault();
    }

    public AccuracyByAnomalyType? GetBestPerformingAnomalyType()
    {
        return AccuracyByType.OrderByDescending(a => a.F1Score).FirstOrDefault();
    }

    private double CalculatePrecision()
    {
        return TruePositives + FalsePositives > 0 ? (double)TruePositives / (TruePositives + FalsePositives) : 0.0;
    }

    private double CalculateRecall()
    {
        return TruePositives + FalseNegatives > 0 ? (double)TruePositives / (TruePositives + FalseNegatives) : 0.0;
    }

    private double CalculateF1Score()
    {
        return Precision + Recall > 0 ? 2 * (Precision * Recall) / (Precision + Recall) : 0.0;
    }

    private double CalculateAccuracy()
    {
        var total = TruePositives + FalsePositives + TrueNegatives + FalseNegatives;
        return total > 0 ? (double)(TruePositives + TrueNegatives) / total : 0.0;
    }

    private double CalculateSpecificity()
    {
        return TrueNegatives + FalsePositives > 0 ? (double)TrueNegatives / (TrueNegatives + FalsePositives) : 0.0;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DetectionLogicId;
        yield return AnalysisStartDate;
        yield return AnalysisEndDate;
        yield return TotalDetections;
        yield return TruePositives;
        yield return FalsePositives;
        yield return TrueNegatives;
        yield return FalseNegatives;
        yield return Precision;
        yield return Recall;
        yield return F1Score;
        yield return Accuracy;
        yield return Specificity;
        yield return AverageDetectionTimeMs;
        yield return MedianDetectionTimeMs;
        yield return PerformanceSummary;
    }
}

/// <summary>
/// 異常タイプ別精度
/// </summary>
public class AccuracyByAnomalyType : ValueObject
{
    public AnomalyType AnomalyType { get; private set; }
    public int TruePositives { get; private set; }
    public int FalsePositives { get; private set; }
    public int FalseNegatives { get; private set; }
    public double Precision { get; private set; }
    public double Recall { get; private set; }
    public double F1Score { get; private set; }

    protected AccuracyByAnomalyType() { }

    public AccuracyByAnomalyType(
        AnomalyType anomalyType,
        int truePositives,
        int falsePositives,
        int falseNegatives)
    {
        AnomalyType = anomalyType;
        TruePositives = truePositives;
        FalsePositives = falsePositives;
        FalseNegatives = falseNegatives;

        Precision = TruePositives + FalsePositives > 0 ? (double)TruePositives / (TruePositives + FalsePositives) : 0.0;
        Recall = TruePositives + FalseNegatives > 0 ? (double)TruePositives / (TruePositives + FalseNegatives) : 0.0;
        F1Score = Precision + Recall > 0 ? 2 * (Precision * Recall) / (Precision + Recall) : 0.0;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return AnomalyType;
        yield return TruePositives;
        yield return FalsePositives;
        yield return FalseNegatives;
        yield return Precision;
        yield return Recall;
        yield return F1Score;
    }
}

/// <summary>
/// 時間範囲別精度
/// </summary>
public class AccuracyByTimeRange : ValueObject
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int TruePositives { get; private set; }
    public int FalsePositives { get; private set; }
    public int FalseNegatives { get; private set; }
    public double Precision { get; private set; }
    public double Recall { get; private set; }
    public double F1Score { get; private set; }

    protected AccuracyByTimeRange() { }

    public AccuracyByTimeRange(
        DateTime startTime,
        DateTime endTime,
        int truePositives,
        int falsePositives,
        int falseNegatives)
    {
        StartTime = startTime;
        EndTime = endTime;
        TruePositives = truePositives;
        FalsePositives = falsePositives;
        FalseNegatives = falseNegatives;

        Precision = TruePositives + FalsePositives > 0 ? (double)TruePositives / (TruePositives + FalsePositives) : 0.0;
        Recall = TruePositives + FalseNegatives > 0 ? (double)TruePositives / (TruePositives + FalseNegatives) : 0.0;
        F1Score = Precision + Recall > 0 ? 2 * (Precision * Recall) / (Precision + Recall) : 0.0;
    }

    public TimeSpan GetTimeRange()
    {
        return EndTime - StartTime;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartTime;
        yield return EndTime;
        yield return TruePositives;
        yield return FalsePositives;
        yield return FalseNegatives;
        yield return Precision;
        yield return Recall;
        yield return F1Score;
    }
}