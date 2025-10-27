using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.AnomalyDetection.Dtos;

/// <summary>
/// 異常パターン分析結果DTO
/// </summary>
public class AnomalyPatternAnalysisDto : EntityDto
{
    public Guid CanSignalId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
    public int TotalAnomalies { get; set; }
    public Dictionary<AnomalyType, int> AnomalyTypeDistribution { get; set; } = new();
    public Dictionary<AnomalyLevel, int> AnomalyLevelDistribution { get; set; } = new();
    public List<AnomalyFrequencyPatternDto> FrequencyPatterns { get; set; } = new();
    public List<AnomalyCorrelationDto> Correlations { get; set; } = new();
    public double AverageDetectionDurationMs { get; set; }
    public double FalsePositiveRate { get; set; }
    public string AnalysisSummary { get; set; } = string.Empty;
}

/// <summary>
/// 異常頻度パターンDTO
/// </summary>
public class AnomalyFrequencyPatternDto
{
    public string PatternName { get; set; } = string.Empty;
    public TimeSpan TimeInterval { get; set; }
    public int Frequency { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// 異常相関DTO
/// </summary>
public class AnomalyCorrelationDto
{
    public Guid RelatedCanSignalId { get; set; }
    public string RelatedSignalName { get; set; } = string.Empty;
    public double CorrelationCoefficient { get; set; }
    public string CorrelationType { get; set; } = string.Empty;
}

/// <summary>
/// 閾値最適化推奨結果DTO
/// </summary>
public class ThresholdRecommendationResultDto : EntityDto
{
    public Guid DetectionLogicId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
    public List<ThresholdRecommendationDto> Recommendations { get; set; } = new();
    public OptimizationMetricsDto CurrentMetrics { get; set; } = new();
    public OptimizationMetricsDto PredictedMetrics { get; set; } = new();
    public double ExpectedImprovement { get; set; }
    public string RecommendationSummary { get; set; } = string.Empty;
}

/// <summary>
/// 閾値推奨DTO
/// </summary>
public class ThresholdRecommendationDto
{
    public string ParameterName { get; set; } = string.Empty;
    public string CurrentValue { get; set; } = string.Empty;
    public string RecommendedValue { get; set; } = string.Empty;
    public string RecommendationReason { get; set; } = string.Empty;
    public double Priority { get; set; }
    public double ConfidenceLevel { get; set; }
}

/// <summary>
/// 最適化メトリクスDTO
/// </summary>
public class OptimizationMetricsDto
{
    public double DetectionRate { get; set; }
    public double FalsePositiveRate { get; set; }
    public double FalseNegativeRate { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AverageDetectionTimeMs { get; set; }
}

/// <summary>
/// 検出精度評価メトリクスDTO
/// </summary>
public class DetectionAccuracyMetricsDto : EntityDto
{
    public Guid DetectionLogicId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
    public int TotalDetections { get; set; }
    public int TruePositives { get; set; }
    public int FalsePositives { get; set; }
    public int TrueNegatives { get; set; }
    public int FalseNegatives { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double Accuracy { get; set; }
    public double Specificity { get; set; }
    public double AverageDetectionTimeMs { get; set; }
    public double MedianDetectionTimeMs { get; set; }
    public List<AccuracyByAnomalyTypeDto> AccuracyByType { get; set; } = new();
    public List<AccuracyByTimeRangeDto> AccuracyByTime { get; set; } = new();
    public string PerformanceSummary { get; set; } = string.Empty;
}

/// <summary>
/// 異常タイプ別精度DTO
/// </summary>
public class AccuracyByAnomalyTypeDto
{
    public AnomalyType AnomalyType { get; set; }
    public int TruePositives { get; set; }
    public int FalsePositives { get; set; }
    public int FalseNegatives { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
}

/// <summary>
/// 時間範囲別精度DTO
/// </summary>
public class AccuracyByTimeRangeDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TruePositives { get; set; }
    public int FalsePositives { get; set; }
    public int FalseNegatives { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
}

/// <summary>
/// 異常分析リクエストDTO
/// </summary>
public class AnomalyAnalysisRequestDto
{
    public Guid CanSignalId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
}

/// <summary>
/// 閾値推奨リクエストDTO
/// </summary>
public class ThresholdRecommendationRequestDto
{
    public Guid DetectionLogicId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
}

/// <summary>
/// 検出精度評価リクエストDTO
/// </summary>
public class DetectionAccuracyRequestDto
{
    public Guid DetectionLogicId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
}