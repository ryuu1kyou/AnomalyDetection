using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 閾値最適化推奨結果
/// </summary>
public class ThresholdRecommendationResult : ValueObject
{
    public Guid DetectionLogicId { get; private set; }
    public DateTime AnalysisStartDate { get; private set; }
    public DateTime AnalysisEndDate { get; private set; }
    public List<ThresholdRecommendation> Recommendations { get; private set; }
    public OptimizationMetrics CurrentMetrics { get; private set; }
    public OptimizationMetrics PredictedMetrics { get; private set; }
    public double ExpectedImprovement { get; private set; }
    public string RecommendationSummary { get; private set; }

    protected ThresholdRecommendationResult() 
    {
        Recommendations = new List<ThresholdRecommendation>();
        CurrentMetrics = null!;
        PredictedMetrics = null!;
        RecommendationSummary = string.Empty;
    }

    public ThresholdRecommendationResult(
        Guid detectionLogicId,
        DateTime analysisStartDate,
        DateTime analysisEndDate,
        List<ThresholdRecommendation> recommendations,
        OptimizationMetrics currentMetrics,
        OptimizationMetrics predictedMetrics,
        double expectedImprovement,
        string recommendationSummary)
    {
        DetectionLogicId = detectionLogicId;
        AnalysisStartDate = analysisStartDate;
        AnalysisEndDate = analysisEndDate;
        Recommendations = recommendations ?? new List<ThresholdRecommendation>();
        CurrentMetrics = currentMetrics ?? throw new ArgumentNullException(nameof(currentMetrics));
        PredictedMetrics = predictedMetrics ?? throw new ArgumentNullException(nameof(predictedMetrics));
        ExpectedImprovement = ValidateImprovement(expectedImprovement);
        RecommendationSummary = recommendationSummary ?? string.Empty;
    }

    public bool HasRecommendations()
    {
        return Recommendations.Any();
    }

    public ThresholdRecommendation? GetHighestPriorityRecommendation()
    {
        return Recommendations.OrderByDescending(r => r.Priority).FirstOrDefault();
    }

    public bool IsSignificantImprovement()
    {
        return ExpectedImprovement > 0.05; // 5%以上の改善を有意とする
    }

    private static double ValidateImprovement(double improvement)
    {
        if (improvement < -1.0 || improvement > 1.0)
            throw new ArgumentOutOfRangeException(nameof(improvement), "Expected improvement must be between -1.0 and 1.0");
        return improvement;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DetectionLogicId;
        yield return AnalysisStartDate;
        yield return AnalysisEndDate;
        yield return string.Join(",", Recommendations.Select(r => r.ParameterName));
        yield return CurrentMetrics;
        yield return PredictedMetrics;
        yield return ExpectedImprovement;
        yield return RecommendationSummary;
    }
}

/// <summary>
/// 閾値推奨
/// </summary>
public class ThresholdRecommendation : ValueObject
{
    public string ParameterName { get; private set; }
    public object CurrentValue { get; private set; }
    public object RecommendedValue { get; private set; }
    public string RecommendationReason { get; private set; }
    public double Priority { get; private set; }
    public double ConfidenceLevel { get; private set; }

    protected ThresholdRecommendation() 
    {
        ParameterName = string.Empty;
        CurrentValue = null!;
        RecommendedValue = null!;
        RecommendationReason = string.Empty;
    }

    public ThresholdRecommendation(
        string parameterName,
        object currentValue,
        object recommendedValue,
        string recommendationReason,
        double priority,
        double confidenceLevel)
    {
        ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        CurrentValue = currentValue ?? throw new ArgumentNullException(nameof(currentValue));
        RecommendedValue = recommendedValue ?? throw new ArgumentNullException(nameof(recommendedValue));
        RecommendationReason = recommendationReason ?? throw new ArgumentNullException(nameof(recommendationReason));
        Priority = ValidatePriority(priority);
        ConfidenceLevel = ValidateConfidenceLevel(confidenceLevel);
    }

    public bool IsHighPriority()
    {
        return Priority > 0.7;
    }

    public bool IsHighConfidence()
    {
        return ConfidenceLevel > 0.8;
    }

    private static double ValidatePriority(double priority)
    {
        if (priority < 0.0 || priority > 1.0)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be between 0.0 and 1.0");
        return priority;
    }

    private static double ValidateConfidenceLevel(double confidenceLevel)
    {
        if (confidenceLevel < 0.0 || confidenceLevel > 1.0)
            throw new ArgumentOutOfRangeException(nameof(confidenceLevel), "Confidence level must be between 0.0 and 1.0");
        return confidenceLevel;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ParameterName;
        yield return CurrentValue.ToString() ?? string.Empty;
        yield return RecommendedValue.ToString() ?? string.Empty;
        yield return RecommendationReason;
        yield return Priority;
        yield return ConfidenceLevel;
    }
}

/// <summary>
/// 最適化メトリクス
/// </summary>
public class OptimizationMetrics : ValueObject
{
    public double DetectionRate { get; private set; }
    public double FalsePositiveRate { get; private set; }
    public double FalseNegativeRate { get; private set; }
    public double Precision { get; private set; }
    public double Recall { get; private set; }
    public double F1Score { get; private set; }
    public double AverageDetectionTimeMs { get; private set; }

    protected OptimizationMetrics() { }

    public OptimizationMetrics(
        double detectionRate,
        double falsePositiveRate,
        double falseNegativeRate,
        double precision,
        double recall,
        double f1Score,
        double averageDetectionTimeMs)
    {
        DetectionRate = ValidateRate(detectionRate, nameof(detectionRate));
        FalsePositiveRate = ValidateRate(falsePositiveRate, nameof(falsePositiveRate));
        FalseNegativeRate = ValidateRate(falseNegativeRate, nameof(falseNegativeRate));
        Precision = ValidateRate(precision, nameof(precision));
        Recall = ValidateRate(recall, nameof(recall));
        F1Score = ValidateRate(f1Score, nameof(f1Score));
        AverageDetectionTimeMs = averageDetectionTimeMs;
    }

    public bool IsGoodPerformance()
    {
        return F1Score > 0.8 && FalsePositiveRate < 0.1;
    }

    public double GetAccuracy()
    {
        return 1.0 - (FalsePositiveRate + FalseNegativeRate) / 2.0;
    }

    private static double ValidateRate(double rate, string parameterName)
    {
        if (rate < 0.0 || rate > 1.0)
            throw new ArgumentOutOfRangeException(parameterName, "Rate must be between 0.0 and 1.0");
        return rate;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DetectionRate;
        yield return FalsePositiveRate;
        yield return FalseNegativeRate;
        yield return Precision;
        yield return Recall;
        yield return F1Score;
        yield return AverageDetectionTimeMs;
    }
}