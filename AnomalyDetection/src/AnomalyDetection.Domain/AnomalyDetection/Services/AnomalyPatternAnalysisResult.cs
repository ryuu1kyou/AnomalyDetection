using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 異常パターン分析結果
/// </summary>
public class AnomalyPatternAnalysisResult : ValueObject
{
    public Guid CanSignalId { get; private set; }
    public DateTime AnalysisStartDate { get; private set; }
    public DateTime AnalysisEndDate { get; private set; }
    public int TotalAnomalies { get; private set; }
    public Dictionary<AnomalyType, int> AnomalyTypeDistribution { get; private set; }
    public Dictionary<AnomalyLevel, int> AnomalyLevelDistribution { get; private set; }
    public List<AnomalyFrequencyPattern> FrequencyPatterns { get; private set; }
    public List<AnomalyCorrelation> Correlations { get; private set; }
    public double AverageDetectionDurationMs { get; private set; }
    public double FalsePositiveRate { get; private set; }
    public string AnalysisSummary { get; private set; }

    protected AnomalyPatternAnalysisResult() 
    {
        AnomalyTypeDistribution = new Dictionary<AnomalyType, int>();
        AnomalyLevelDistribution = new Dictionary<AnomalyLevel, int>();
        FrequencyPatterns = new List<AnomalyFrequencyPattern>();
        Correlations = new List<AnomalyCorrelation>();
        AnalysisSummary = string.Empty;
    }

    public AnomalyPatternAnalysisResult(
        Guid canSignalId,
        DateTime analysisStartDate,
        DateTime analysisEndDate,
        int totalAnomalies,
        Dictionary<AnomalyType, int> anomalyTypeDistribution,
        Dictionary<AnomalyLevel, int> anomalyLevelDistribution,
        List<AnomalyFrequencyPattern> frequencyPatterns,
        List<AnomalyCorrelation> correlations,
        double averageDetectionDurationMs,
        double falsePositiveRate,
        string analysisSummary)
    {
        CanSignalId = canSignalId;
        AnalysisStartDate = analysisStartDate;
        AnalysisEndDate = analysisEndDate;
        TotalAnomalies = totalAnomalies;
        AnomalyTypeDistribution = anomalyTypeDistribution ?? new Dictionary<AnomalyType, int>();
        AnomalyLevelDistribution = anomalyLevelDistribution ?? new Dictionary<AnomalyLevel, int>();
        FrequencyPatterns = frequencyPatterns ?? new List<AnomalyFrequencyPattern>();
        Correlations = correlations ?? new List<AnomalyCorrelation>();
        AverageDetectionDurationMs = averageDetectionDurationMs;
        FalsePositiveRate = ValidateFalsePositiveRate(falsePositiveRate);
        AnalysisSummary = analysisSummary ?? string.Empty;
    }

    public AnomalyType GetMostFrequentAnomalyType()
    {
        return AnomalyTypeDistribution.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
    }

    public AnomalyLevel GetMostFrequentAnomalyLevel()
    {
        return AnomalyLevelDistribution.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
    }

    public bool HasHighFalsePositiveRate()
    {
        return FalsePositiveRate > 0.1; // 10%以上を高い誤検出率とする
    }

    public TimeSpan GetAnalysisPeriod()
    {
        return AnalysisEndDate - AnalysisStartDate;
    }

    private static double ValidateFalsePositiveRate(double rate)
    {
        if (rate < 0.0 || rate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(rate), "False positive rate must be between 0.0 and 1.0");
        return rate;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CanSignalId;
        yield return AnalysisStartDate;
        yield return AnalysisEndDate;
        yield return TotalAnomalies;
        yield return string.Join(",", AnomalyTypeDistribution.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return string.Join(",", AnomalyLevelDistribution.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        yield return AverageDetectionDurationMs;
        yield return FalsePositiveRate;
        yield return AnalysisSummary;
    }
}

/// <summary>
/// 異常頻度パターン
/// </summary>
public class AnomalyFrequencyPattern : ValueObject
{
    public string PatternName { get; private set; }
    public TimeSpan TimeInterval { get; private set; }
    public int Frequency { get; private set; }
    public double Confidence { get; private set; }

    protected AnomalyFrequencyPattern() 
    {
        PatternName = string.Empty;
    }

    public AnomalyFrequencyPattern(string patternName, TimeSpan timeInterval, int frequency, double confidence)
    {
        PatternName = patternName ?? throw new ArgumentNullException(nameof(patternName));
        TimeInterval = timeInterval;
        Frequency = frequency;
        Confidence = ValidateConfidence(confidence);
    }

    private static double ValidateConfidence(double confidence)
    {
        if (confidence < 0.0 || confidence > 1.0)
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 0.0 and 1.0");
        return confidence;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return PatternName;
        yield return TimeInterval;
        yield return Frequency;
        yield return Confidence;
    }
}

/// <summary>
/// 異常相関
/// </summary>
public class AnomalyCorrelation : ValueObject
{
    public Guid RelatedCanSignalId { get; private set; }
    public string RelatedSignalName { get; private set; }
    public double CorrelationCoefficient { get; private set; }
    public string CorrelationType { get; private set; }

    protected AnomalyCorrelation() 
    {
        RelatedSignalName = string.Empty;
        CorrelationType = string.Empty;
    }

    public AnomalyCorrelation(Guid relatedCanSignalId, string relatedSignalName, double correlationCoefficient, string correlationType)
    {
        RelatedCanSignalId = relatedCanSignalId;
        RelatedSignalName = relatedSignalName ?? throw new ArgumentNullException(nameof(relatedSignalName));
        CorrelationCoefficient = ValidateCorrelationCoefficient(correlationCoefficient);
        CorrelationType = correlationType ?? throw new ArgumentNullException(nameof(correlationType));
    }

    public bool IsStrongCorrelation()
    {
        return Math.Abs(CorrelationCoefficient) > 0.7;
    }

    private static double ValidateCorrelationCoefficient(double coefficient)
    {
        if (coefficient < -1.0 || coefficient > 1.0)
            throw new ArgumentOutOfRangeException(nameof(coefficient), "Correlation coefficient must be between -1.0 and 1.0");
        return coefficient;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return RelatedCanSignalId;
        yield return RelatedSignalName;
        yield return CorrelationCoefficient;
        yield return CorrelationType;
    }
}