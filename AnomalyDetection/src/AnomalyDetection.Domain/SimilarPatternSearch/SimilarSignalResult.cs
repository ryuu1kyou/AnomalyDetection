using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 類似CAN信号検索結果を表す値オブジェクト
/// </summary>
public class SimilarSignalResult : ValueObject
{
    /// <summary>
    /// 類似信号のID
    /// </summary>
    public Guid SignalId { get; private set; }
    
    /// <summary>
    /// 類似度スコア（0.0-1.0）
    /// </summary>
    public double SimilarityScore { get; private set; }
    
    /// <summary>
    /// 類似度の詳細内訳
    /// </summary>
    public SimilarityBreakdown Breakdown { get; private set; }
    
    /// <summary>
    /// 一致した属性リスト
    /// </summary>
    public IReadOnlyList<string> MatchedAttributes { get; private set; }
    
    /// <summary>
    /// 差異のある属性リスト
    /// </summary>
    public IReadOnlyList<AttributeDifference> Differences { get; private set; }
    
    /// <summary>
    /// 推奨度（使用推奨レベル）
    /// </summary>
    public RecommendationLevel RecommendationLevel { get; private set; }
    
    /// <summary>
    /// 推奨理由
    /// </summary>
    public string RecommendationReason { get; private set; }

    protected SimilarSignalResult() 
    {
        MatchedAttributes = new List<string>();
        Differences = new List<AttributeDifference>();
        RecommendationReason = string.Empty;
    }

    public SimilarSignalResult(
        Guid signalId,
        double similarityScore,
        SimilarityBreakdown breakdown,
        IEnumerable<string> matchedAttributes,
        IEnumerable<AttributeDifference> differences,
        RecommendationLevel recommendationLevel = RecommendationLevel.Medium,
        string recommendationReason = "")
    {
        SignalId = signalId;
        SimilarityScore = ValidateSimilarityScore(similarityScore);
        Breakdown = breakdown ?? throw new ArgumentNullException(nameof(breakdown));
        MatchedAttributes = (matchedAttributes ?? new List<string>()).ToList();
        Differences = (differences ?? new List<AttributeDifference>()).ToList();
        RecommendationLevel = recommendationLevel;
        RecommendationReason = recommendationReason ?? string.Empty;
    }

    /// <summary>
    /// 高い類似度を持つかチェック
    /// </summary>
    public bool IsHighSimilarity(double threshold = 0.8)
    {
        return SimilarityScore >= threshold;
    }

    /// <summary>
    /// 推奨される信号かチェック
    /// </summary>
    public bool IsRecommended()
    {
        return RecommendationLevel >= RecommendationLevel.High;
    }

    /// <summary>
    /// 重要な差異があるかチェック
    /// </summary>
    public bool HasSignificantDifferences()
    {
        return Differences.Any(d => d.IsSignificant);
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
        yield return SignalId;
        yield return SimilarityScore;
        yield return Breakdown;
        yield return string.Join(",", MatchedAttributes);
        yield return string.Join(",", Differences.Select(d => d.ToString()));
        yield return RecommendationLevel;
    }
}

/// <summary>
/// 類似度の詳細内訳を表す値オブジェクト
/// </summary>
public class SimilarityBreakdown : ValueObject
{
    /// <summary>
    /// CAN ID類似度
    /// </summary>
    public double CanIdSimilarity { get; private set; }
    
    /// <summary>
    /// 信号名類似度
    /// </summary>
    public double SignalNameSimilarity { get; private set; }
    
    /// <summary>
    /// システム種別類似度
    /// </summary>
    public double SystemTypeSimilarity { get; private set; }
    
    /// <summary>
    /// 値範囲類似度
    /// </summary>
    public double ValueRangeSimilarity { get; private set; }
    
    /// <summary>
    /// データ長類似度
    /// </summary>
    public double DataLengthSimilarity { get; private set; }
    
    /// <summary>
    /// 周期類似度
    /// </summary>
    public double CycleSimilarity { get; private set; }
    
    /// <summary>
    /// OEMコード類似度
    /// </summary>
    public double OemCodeSimilarity { get; private set; }
    
    /// <summary>
    /// 重み付け設定
    /// </summary>
    public SimilarityWeights Weights { get; private set; }

    protected SimilarityBreakdown() { }

    public SimilarityBreakdown(
        double canIdSimilarity = 0.0,
        double signalNameSimilarity = 0.0,
        double systemTypeSimilarity = 0.0,
        double valueRangeSimilarity = 0.0,
        double dataLengthSimilarity = 0.0,
        double cycleSimilarity = 0.0,
        double oemCodeSimilarity = 0.0,
        SimilarityWeights? weights = null)
    {
        CanIdSimilarity = ValidateSimilarity(canIdSimilarity, nameof(canIdSimilarity));
        SignalNameSimilarity = ValidateSimilarity(signalNameSimilarity, nameof(signalNameSimilarity));
        SystemTypeSimilarity = ValidateSimilarity(systemTypeSimilarity, nameof(systemTypeSimilarity));
        ValueRangeSimilarity = ValidateSimilarity(valueRangeSimilarity, nameof(valueRangeSimilarity));
        DataLengthSimilarity = ValidateSimilarity(dataLengthSimilarity, nameof(dataLengthSimilarity));
        CycleSimilarity = ValidateSimilarity(cycleSimilarity, nameof(cycleSimilarity));
        OemCodeSimilarity = ValidateSimilarity(oemCodeSimilarity, nameof(oemCodeSimilarity));
        Weights = weights ?? SimilarityWeights.CreateDefault();
    }

    /// <summary>
    /// 重み付き総合類似度を計算
    /// </summary>
    public double CalculateWeightedSimilarity()
    {
        var totalWeight = 0.0;
        var weightedSum = 0.0;

        if (Weights.CanIdWeight > 0)
        {
            weightedSum += CanIdSimilarity * Weights.CanIdWeight;
            totalWeight += Weights.CanIdWeight;
        }

        if (Weights.SignalNameWeight > 0)
        {
            weightedSum += SignalNameSimilarity * Weights.SignalNameWeight;
            totalWeight += Weights.SignalNameWeight;
        }

        if (Weights.SystemTypeWeight > 0)
        {
            weightedSum += SystemTypeSimilarity * Weights.SystemTypeWeight;
            totalWeight += Weights.SystemTypeWeight;
        }

        if (Weights.ValueRangeWeight > 0)
        {
            weightedSum += ValueRangeSimilarity * Weights.ValueRangeWeight;
            totalWeight += Weights.ValueRangeWeight;
        }

        if (Weights.DataLengthWeight > 0)
        {
            weightedSum += DataLengthSimilarity * Weights.DataLengthWeight;
            totalWeight += Weights.DataLengthWeight;
        }

        if (Weights.CycleWeight > 0)
        {
            weightedSum += CycleSimilarity * Weights.CycleWeight;
            totalWeight += Weights.CycleWeight;
        }

        if (Weights.OemCodeWeight > 0)
        {
            weightedSum += OemCodeSimilarity * Weights.OemCodeWeight;
            totalWeight += Weights.OemCodeWeight;
        }

        return totalWeight > 0 ? weightedSum / totalWeight : 0.0;
    }

    private static double ValidateSimilarity(double similarity, string paramName)
    {
        if (similarity < 0.0 || similarity > 1.0)
            throw new ArgumentOutOfRangeException(paramName, 
                "Similarity must be between 0.0 and 1.0");
        return similarity;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CanIdSimilarity;
        yield return SignalNameSimilarity;
        yield return SystemTypeSimilarity;
        yield return ValueRangeSimilarity;
        yield return DataLengthSimilarity;
        yield return CycleSimilarity;
        yield return OemCodeSimilarity;
        yield return Weights;
    }
}

/// <summary>
/// 類似度計算の重み設定を表す値オブジェクト
/// </summary>
public class SimilarityWeights : ValueObject
{
    public double CanIdWeight { get; private set; }
    public double SignalNameWeight { get; private set; }
    public double SystemTypeWeight { get; private set; }
    public double ValueRangeWeight { get; private set; }
    public double DataLengthWeight { get; private set; }
    public double CycleWeight { get; private set; }
    public double OemCodeWeight { get; private set; }

    protected SimilarityWeights() { }

    public SimilarityWeights(
        double canIdWeight = 0.3,
        double signalNameWeight = 0.3,
        double systemTypeWeight = 0.2,
        double valueRangeWeight = 0.1,
        double dataLengthWeight = 0.05,
        double cycleWeight = 0.03,
        double oemCodeWeight = 0.02)
    {
        CanIdWeight = ValidateWeight(canIdWeight, nameof(canIdWeight));
        SignalNameWeight = ValidateWeight(signalNameWeight, nameof(signalNameWeight));
        SystemTypeWeight = ValidateWeight(systemTypeWeight, nameof(systemTypeWeight));
        ValueRangeWeight = ValidateWeight(valueRangeWeight, nameof(valueRangeWeight));
        DataLengthWeight = ValidateWeight(dataLengthWeight, nameof(dataLengthWeight));
        CycleWeight = ValidateWeight(cycleWeight, nameof(cycleWeight));
        OemCodeWeight = ValidateWeight(oemCodeWeight, nameof(oemCodeWeight));
    }

    public static SimilarityWeights CreateDefault()
    {
        return new SimilarityWeights();
    }

    public static SimilarityWeights CreateBasicComparison()
    {
        return new SimilarityWeights(
            canIdWeight: 0.4,
            signalNameWeight: 0.4,
            systemTypeWeight: 0.2,
            valueRangeWeight: 0.0,
            dataLengthWeight: 0.0,
            cycleWeight: 0.0,
            oemCodeWeight: 0.0);
    }

    public static SimilarityWeights CreateDetailedComparison()
    {
        return new SimilarityWeights(
            canIdWeight: 0.25,
            signalNameWeight: 0.25,
            systemTypeWeight: 0.15,
            valueRangeWeight: 0.15,
            dataLengthWeight: 0.1,
            cycleWeight: 0.05,
            oemCodeWeight: 0.05);
    }

    private static double ValidateWeight(double weight, string paramName)
    {
        if (weight < 0.0 || weight > 1.0)
            throw new ArgumentOutOfRangeException(paramName, 
                "Weight must be between 0.0 and 1.0");
        return weight;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CanIdWeight;
        yield return SignalNameWeight;
        yield return SystemTypeWeight;
        yield return ValueRangeWeight;
        yield return DataLengthWeight;
        yield return CycleWeight;
        yield return OemCodeWeight;
    }
}

/// <summary>
/// 属性差異を表す値オブジェクト
/// </summary>
public class AttributeDifference : ValueObject
{
    /// <summary>
    /// 属性名
    /// </summary>
    public string AttributeName { get; private set; }
    
    /// <summary>
    /// 比較元の値
    /// </summary>
    public string SourceValue { get; private set; }
    
    /// <summary>
    /// 比較先の値
    /// </summary>
    public string TargetValue { get; private set; }
    
    /// <summary>
    /// 差異の重要度
    /// </summary>
    public bool IsSignificant { get; private set; }
    
    /// <summary>
    /// 差異の説明
    /// </summary>
    public string Description { get; private set; }

    protected AttributeDifference() 
    {
        AttributeName = string.Empty;
        SourceValue = string.Empty;
        TargetValue = string.Empty;
        Description = string.Empty;
    }

    public AttributeDifference(
        string attributeName,
        string sourceValue,
        string targetValue,
        bool isSignificant = false,
        string description = "")
    {
        AttributeName = ValidateAttributeName(attributeName);
        SourceValue = sourceValue ?? string.Empty;
        TargetValue = targetValue ?? string.Empty;
        IsSignificant = isSignificant;
        Description = description ?? string.Empty;
    }

    private static string ValidateAttributeName(string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
            throw new ArgumentException("Attribute name cannot be null or empty", nameof(attributeName));
        return attributeName.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return AttributeName;
        yield return SourceValue;
        yield return TargetValue;
        yield return IsSignificant;
    }

    public override string ToString()
    {
        return $"{AttributeName}: {SourceValue} -> {TargetValue}";
    }
}

/// <summary>
/// 推奨レベル
/// </summary>
public enum RecommendationLevel
{
    /// <summary>
    /// 推奨しない
    /// </summary>
    NotRecommended = 0,
    
    /// <summary>
    /// 低推奨
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// 中推奨
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// 高推奨
    /// </summary>
    High = 3,
    
    /// <summary>
    /// 強く推奨
    /// </summary>
    Highly = 4
}