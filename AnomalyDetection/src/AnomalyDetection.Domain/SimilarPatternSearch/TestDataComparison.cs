using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 検査データ比較結果を表す値オブジェクト
/// </summary>
public class TestDataComparison : ValueObject
{
    /// <summary>
    /// 閾値の差異リスト
    /// </summary>
    public IReadOnlyList<ThresholdDifference> ThresholdDifferences { get; private set; }
    
    /// <summary>
    /// 検出条件の差異リスト
    /// </summary>
    public IReadOnlyList<DetectionConditionDifference> DetectionConditionDifferences { get; private set; }
    
    /// <summary>
    /// 検出結果の差異リスト
    /// </summary>
    public IReadOnlyList<ResultDifference> ResultDifferences { get; private set; }
    
    /// <summary>
    /// 推奨事項リスト
    /// </summary>
    public IReadOnlyList<ComparisonRecommendation> Recommendations { get; private set; }
    
    /// <summary>
    /// 比較対象の信号ID（比較元）
    /// </summary>
    public Guid SourceSignalId { get; private set; }
    
    /// <summary>
    /// 比較対象の信号ID（比較先）
    /// </summary>
    public Guid TargetSignalId { get; private set; }
    
    /// <summary>
    /// 比較実行日時
    /// </summary>
    public DateTime ComparedAt { get; private set; }
    
    /// <summary>
    /// 全体的な類似度スコア（0.0-1.0）
    /// </summary>
    public double OverallSimilarityScore { get; private set; }
    
    /// <summary>
    /// 比較サマリー
    /// </summary>
    public string Summary { get; private set; }

    protected TestDataComparison() 
    {
        ThresholdDifferences = new List<ThresholdDifference>();
        DetectionConditionDifferences = new List<DetectionConditionDifference>();
        ResultDifferences = new List<ResultDifference>();
        Recommendations = new List<ComparisonRecommendation>();
        Summary = string.Empty;
    }

    public TestDataComparison(
        Guid sourceSignalId,
        Guid targetSignalId,
        IEnumerable<ThresholdDifference> thresholdDifferences,
        IEnumerable<DetectionConditionDifference> detectionConditionDifferences,
        IEnumerable<ResultDifference> resultDifferences,
        IEnumerable<ComparisonRecommendation> recommendations,
        double overallSimilarityScore,
        string summary = "")
    {
        SourceSignalId = sourceSignalId;
        TargetSignalId = targetSignalId;
        ThresholdDifferences = (thresholdDifferences ?? Enumerable.Empty<ThresholdDifference>()).ToList();
        DetectionConditionDifferences = (detectionConditionDifferences ?? Enumerable.Empty<DetectionConditionDifference>()).ToList();
        ResultDifferences = (resultDifferences ?? Enumerable.Empty<ResultDifference>()).ToList();
        Recommendations = (recommendations ?? Enumerable.Empty<ComparisonRecommendation>()).ToList();
        OverallSimilarityScore = ValidateSimilarityScore(overallSimilarityScore);
        Summary = summary ?? string.Empty;
        ComparedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 重要な差異があるかチェック
    /// </summary>
    public bool HasSignificantDifferences()
    {
        return ThresholdDifferences.Any(d => d.IsSignificant) ||
               DetectionConditionDifferences.Any(d => d.IsSignificant) ||
               ResultDifferences.Any(d => d.IsSignificant);
    }

    /// <summary>
    /// 高い類似度を持つかチェック
    /// </summary>
    public bool HasHighSimilarity(double threshold = 0.8)
    {
        return OverallSimilarityScore >= threshold;
    }

    /// <summary>
    /// 推奨事項があるかチェック
    /// </summary>
    public bool HasRecommendations()
    {
        return Recommendations.Any();
    }

    /// <summary>
    /// 高優先度の推奨事項があるかチェック
    /// </summary>
    public bool HasHighPriorityRecommendations()
    {
        return Recommendations.Any(r => r.Priority == RecommendationPriority.High);
    }

    /// <summary>
    /// 閾値差異の数を取得
    /// </summary>
    public int GetThresholdDifferenceCount()
    {
        return ThresholdDifferences.Count;
    }

    /// <summary>
    /// 重要な閾値差異の数を取得
    /// </summary>
    public int GetSignificantThresholdDifferenceCount()
    {
        return ThresholdDifferences.Count(d => d.IsSignificant);
    }

    /// <summary>
    /// 検出条件差異の数を取得
    /// </summary>
    public int GetDetectionConditionDifferenceCount()
    {
        return DetectionConditionDifferences.Count;
    }

    /// <summary>
    /// 結果差異の数を取得
    /// </summary>
    public int GetResultDifferenceCount()
    {
        return ResultDifferences.Count;
    }

    /// <summary>
    /// 特定タイプの推奨事項を取得
    /// </summary>
    public IEnumerable<ComparisonRecommendation> GetRecommendationsByType(RecommendationType type)
    {
        return Recommendations.Where(r => r.Type == type);
    }

    /// <summary>
    /// 優先度別の推奨事項を取得
    /// </summary>
    public IEnumerable<ComparisonRecommendation> GetRecommendationsByPriority(RecommendationPriority priority)
    {
        return Recommendations.Where(r => r.Priority == priority);
    }

    private static double ValidateSimilarityScore(double score)
    {
        if (score < 0.0 || score > 1.0)
            throw new ArgumentOutOfRangeException(nameof(score), 
                "Similarity score must be between 0.0 and 1.0");
        return score;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return SourceSignalId;
        yield return TargetSignalId;
        yield return OverallSimilarityScore;
        yield return string.Join(",", ThresholdDifferences.Select(d => d.ToString()));
        yield return string.Join(",", DetectionConditionDifferences.Select(d => d.ToString()));
        yield return string.Join(",", ResultDifferences.Select(d => d.ToString()));
        yield return string.Join(",", Recommendations.Select(r => r.ToString()));
    }
}

/// <summary>
/// 閾値の差異を表す値オブジェクト
/// </summary>
public class ThresholdDifference : ValueObject
{
    /// <summary>
    /// パラメータ名
    /// </summary>
    public string ParameterName { get; private set; }
    
    /// <summary>
    /// 比較元の値
    /// </summary>
    public double SourceValue { get; private set; }
    
    /// <summary>
    /// 比較先の値
    /// </summary>
    public double TargetValue { get; private set; }
    
    /// <summary>
    /// 差異の絶対値
    /// </summary>
    public double AbsoluteDifference { get; private set; }
    
    /// <summary>
    /// 差異の相対値（パーセンテージ）
    /// </summary>
    public double RelativeDifference { get; private set; }
    
    /// <summary>
    /// 重要な差異かどうか
    /// </summary>
    public bool IsSignificant { get; private set; }
    
    /// <summary>
    /// 差異の説明
    /// </summary>
    public string Description { get; private set; }

    protected ThresholdDifference() 
    {
        ParameterName = string.Empty;
        Description = string.Empty;
    }

    public ThresholdDifference(
        string parameterName,
        double sourceValue,
        double targetValue,
        string description = "")
    {
        ParameterName = ValidateParameterName(parameterName);
        SourceValue = sourceValue;
        TargetValue = targetValue;
        AbsoluteDifference = Math.Abs(targetValue - sourceValue);
        RelativeDifference = CalculateRelativeDifference(sourceValue, targetValue);
        IsSignificant = DetermineSignificance(RelativeDifference);
        Description = description ?? string.Empty;
    }

    private static double CalculateRelativeDifference(double sourceValue, double targetValue)
    {
        if (Math.Abs(sourceValue) < double.Epsilon)
        {
            return Math.Abs(targetValue) < double.Epsilon ? 0.0 : 100.0;
        }
        
        return Math.Abs((targetValue - sourceValue) / sourceValue) * 100.0;
    }

    private static bool DetermineSignificance(double relativeDifference)
    {
        // 10%以上の差異を重要とみなす
        return relativeDifference >= 10.0;
    }

    private static string ValidateParameterName(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));
        return parameterName.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ParameterName;
        yield return SourceValue;
        yield return TargetValue;
        yield return AbsoluteDifference;
        yield return RelativeDifference;
        yield return IsSignificant;
    }

    public override string ToString()
    {
        return $"{ParameterName}: {SourceValue} -> {TargetValue} ({RelativeDifference:F1}%)";
    }
}

/// <summary>
/// 検出条件の差異を表す値オブジェクト
/// </summary>
public class DetectionConditionDifference : ValueObject
{
    /// <summary>
    /// 条件名
    /// </summary>
    public string ConditionName { get; private set; }
    
    /// <summary>
    /// 比較元の条件
    /// </summary>
    public string SourceCondition { get; private set; }
    
    /// <summary>
    /// 比較先の条件
    /// </summary>
    public string TargetCondition { get; private set; }
    
    /// <summary>
    /// 差異タイプ
    /// </summary>
    public DifferenceType DifferenceType { get; private set; }
    
    /// <summary>
    /// 重要な差異かどうか
    /// </summary>
    public bool IsSignificant { get; private set; }
    
    /// <summary>
    /// 差異の説明
    /// </summary>
    public string Description { get; private set; }

    protected DetectionConditionDifference() 
    {
        ConditionName = string.Empty;
        SourceCondition = string.Empty;
        TargetCondition = string.Empty;
        Description = string.Empty;
    }

    public DetectionConditionDifference(
        string conditionName,
        string sourceCondition,
        string targetCondition,
        DifferenceType differenceType,
        bool isSignificant = false,
        string description = "")
    {
        ConditionName = ValidateConditionName(conditionName);
        SourceCondition = sourceCondition ?? string.Empty;
        TargetCondition = targetCondition ?? string.Empty;
        DifferenceType = differenceType;
        IsSignificant = isSignificant;
        Description = description ?? string.Empty;
    }

    private static string ValidateConditionName(string conditionName)
    {
        if (string.IsNullOrWhiteSpace(conditionName))
            throw new ArgumentException("Condition name cannot be null or empty", nameof(conditionName));
        return conditionName.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ConditionName;
        yield return SourceCondition;
        yield return TargetCondition;
        yield return DifferenceType;
        yield return IsSignificant;
    }

    public override string ToString()
    {
        return $"{ConditionName}: {DifferenceType} ({(IsSignificant ? "Significant" : "Minor")})";
    }
}

/// <summary>
/// 検出結果の差異を表す値オブジェクト
/// </summary>
public class ResultDifference : ValueObject
{
    /// <summary>
    /// 結果項目名
    /// </summary>
    public string ResultItem { get; private set; }
    
    /// <summary>
    /// 比較元の値
    /// </summary>
    public string SourceValue { get; private set; }
    
    /// <summary>
    /// 比較先の値
    /// </summary>
    public string TargetValue { get; private set; }
    
    /// <summary>
    /// 差異タイプ
    /// </summary>
    public DifferenceType DifferenceType { get; private set; }
    
    /// <summary>
    /// 重要な差異かどうか
    /// </summary>
    public bool IsSignificant { get; private set; }
    
    /// <summary>
    /// 影響度
    /// </summary>
    public ImpactLevel ImpactLevel { get; private set; }

    protected ResultDifference() 
    {
        ResultItem = string.Empty;
        SourceValue = string.Empty;
        TargetValue = string.Empty;
    }

    public ResultDifference(
        string resultItem,
        string sourceValue,
        string targetValue,
        DifferenceType differenceType,
        bool isSignificant = false,
        ImpactLevel impactLevel = ImpactLevel.Low)
    {
        ResultItem = ValidateResultItem(resultItem);
        SourceValue = sourceValue ?? string.Empty;
        TargetValue = targetValue ?? string.Empty;
        DifferenceType = differenceType;
        IsSignificant = isSignificant;
        ImpactLevel = impactLevel;
    }

    private static string ValidateResultItem(string resultItem)
    {
        if (string.IsNullOrWhiteSpace(resultItem))
            throw new ArgumentException("Result item cannot be null or empty", nameof(resultItem));
        return resultItem.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ResultItem;
        yield return SourceValue;
        yield return TargetValue;
        yield return DifferenceType;
        yield return IsSignificant;
        yield return ImpactLevel;
    }

    public override string ToString()
    {
        return $"{ResultItem}: {SourceValue} -> {TargetValue} ({ImpactLevel} impact)";
    }
}

/// <summary>
/// 比較推奨事項を表す値オブジェクト
/// </summary>
public class ComparisonRecommendation : ValueObject
{
    /// <summary>
    /// 推奨事項のタイプ
    /// </summary>
    public RecommendationType Type { get; private set; }
    
    /// <summary>
    /// 優先度
    /// </summary>
    public RecommendationPriority Priority { get; private set; }
    
    /// <summary>
    /// 推奨事項の内容
    /// </summary>
    public string Content { get; private set; }
    
    /// <summary>
    /// 根拠・理由
    /// </summary>
    public string Rationale { get; private set; }
    
    /// <summary>
    /// 推奨される値・設定
    /// </summary>
    public string RecommendedValue { get; private set; }

    protected ComparisonRecommendation() 
    {
        Content = string.Empty;
        Rationale = string.Empty;
        RecommendedValue = string.Empty;
    }

    public ComparisonRecommendation(
        RecommendationType type,
        RecommendationPriority priority,
        string content,
        string rationale = "",
        string recommendedValue = "")
    {
        Type = type;
        Priority = priority;
        Content = ValidateContent(content);
        Rationale = rationale ?? string.Empty;
        RecommendedValue = recommendedValue ?? string.Empty;
    }

    private static string ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));
        return content.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return Priority;
        yield return Content;
        yield return Rationale;
        yield return RecommendedValue;
    }

    public override string ToString()
    {
        return $"{Type} ({Priority}): {Content}";
    }
}

/// <summary>
/// 差異タイプ
/// </summary>
public enum DifferenceType
{
    /// <summary>
    /// 値が異なる
    /// </summary>
    ValueDifference = 1,
    
    /// <summary>
    /// 条件が異なる
    /// </summary>
    ConditionDifference = 2,
    
    /// <summary>
    /// 設定が欠落
    /// </summary>
    MissingSetting = 3,
    
    /// <summary>
    /// 追加設定
    /// </summary>
    AdditionalSetting = 4,
    
    /// <summary>
    /// 形式が異なる
    /// </summary>
    FormatDifference = 5
}

/// <summary>
/// 影響度レベル
/// </summary>
public enum ImpactLevel
{
    /// <summary>
    /// 低影響
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// 中影響
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// 高影響
    /// </summary>
    High = 3,
    
    /// <summary>
    /// 重大影響
    /// </summary>
    Critical = 4
}

/// <summary>
/// 推奨事項タイプ
/// </summary>
public enum RecommendationType
{
    /// <summary>
    /// 閾値調整
    /// </summary>
    ThresholdAdjustment = 1,
    
    /// <summary>
    /// 条件変更
    /// </summary>
    ConditionChange = 2,
    
    /// <summary>
    /// パラメータ追加
    /// </summary>
    ParameterAddition = 3,
    
    /// <summary>
    /// 設定削除
    /// </summary>
    SettingRemoval = 4,
    
    /// <summary>
    /// 検証推奨
    /// </summary>
    ValidationRecommended = 5,
    
    /// <summary>
    /// 注意事項
    /// </summary>
    Caution = 6
}

/// <summary>
/// 推奨事項優先度
/// </summary>
public enum RecommendationPriority
{
    /// <summary>
    /// 低優先度
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// 中優先度
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// 高優先度
    /// </summary>
    High = 3,
    
    /// <summary>
    /// 緊急
    /// </summary>
    Urgent = 4
}