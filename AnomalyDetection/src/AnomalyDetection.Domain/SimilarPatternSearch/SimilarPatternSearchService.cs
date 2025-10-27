using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Logging;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 類似パターン検索サービスの実装
/// </summary>
public class SimilarPatternSearchService : DomainService, ISimilarPatternSearchService
{
    private readonly ILogger<SimilarPatternSearchService> _logger;

    public SimilarPatternSearchService(ILogger<SimilarPatternSearchService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 類似CAN信号を検索する
    /// </summary>
    public async Task<IEnumerable<SimilarSignalResult>> SearchSimilarSignalsAsync(
        CanSignal targetSignal,
        SimilaritySearchCriteria criteria,
        IEnumerable<CanSignal>? candidateSignals = null)
    {
        if (targetSignal == null)
            throw new ArgumentNullException(nameof(targetSignal));
        if (criteria == null)
            throw new ArgumentNullException(nameof(criteria));

        _logger.LogInformation("Starting similar signal search for signal {SignalId} with criteria {Criteria}", 
            targetSignal.Id, criteria);

        var candidates = candidateSignals?.ToList() ?? new List<CanSignal>();
        var results = new List<SimilarSignalResult>();

        foreach (var candidate in candidates)
        {
            // 自分自身は除外
            if (candidate.Id == targetSignal.Id)
                continue;

            // フィルター条件をチェック
            if (!PassesFilters(candidate, criteria))
                continue;

            // 類似度を計算
            var similarityScore = CalculateSimilarity(targetSignal, candidate, criteria);
            
            // 最小類似度を満たさない場合はスキップ
            if (similarityScore < criteria.MinimumSimilarity)
                continue;

            // 詳細な類似度内訳を計算
            var breakdown = CalculateSimilarityBreakdown(targetSignal, candidate, criteria);
            
            // 一致した属性と差異を分析
            var (matchedAttributes, differences) = AnalyzeAttributeMatches(targetSignal, candidate, criteria);
            
            // 推奨レベルを決定
            var recommendationLevel = DetermineRecommendationLevel(similarityScore, breakdown, differences);
            
            // 推奨理由を生成
            var recommendationReason = GenerateRecommendationReason(similarityScore, breakdown, differences);

            var result = new SimilarSignalResult(
                candidate.Id,
                similarityScore,
                breakdown,
                matchedAttributes,
                differences,
                recommendationLevel,
                recommendationReason);

            results.Add(result);
        }

        // 類似度でソートして最大結果数まで返す
        var sortedResults = results
            .OrderByDescending(r => r.SimilarityScore)
            .ThenByDescending(r => r.RecommendationLevel)
            .Take(criteria.MaxResults)
            .ToList();

        _logger.LogInformation("Found {Count} similar signals for signal {SignalId}", 
            sortedResults.Count, targetSignal.Id);

        return await Task.FromResult(sortedResults);
    }

    /// <summary>
    /// 検査データを比較する
    /// </summary>
    public async Task<TestDataComparison> CompareTestDataAsync(
        IEnumerable<AnomalyDetectionResult> sourceResults,
        IEnumerable<AnomalyDetectionResult> targetResults,
        CanSignal sourceSignal,
        CanSignal targetSignal)
    {
        if (sourceResults == null)
            throw new ArgumentNullException(nameof(sourceResults));
        if (targetResults == null)
            throw new ArgumentNullException(nameof(targetResults));
        if (sourceSignal == null)
            throw new ArgumentNullException(nameof(sourceSignal));
        if (targetSignal == null)
            throw new ArgumentNullException(nameof(targetSignal));

        _logger.LogInformation("Comparing test data between signals {SourceSignalId} and {TargetSignalId}", 
            sourceSignal.Id, targetSignal.Id);

        var sourceResultsList = sourceResults.ToList();
        var targetResultsList = targetResults.ToList();

        // 閾値差異を分析
        var thresholdDifferences = AnalyzeThresholdDifferences(sourceResultsList, targetResultsList);
        
        // 検出条件差異を分析
        var conditionDifferences = AnalyzeDetectionConditionDifferences(sourceResultsList, targetResultsList);
        
        // 結果差異を分析
        var resultDifferences = AnalyzeResultDifferences(sourceResultsList, targetResultsList);
        
        // 推奨事項を生成
        var recommendations = GenerateComparisonRecommendations(
            thresholdDifferences, conditionDifferences, resultDifferences, sourceSignal, targetSignal);
        
        // 全体的な類似度を計算
        var overallSimilarity = CalculateOverallTestDataSimilarity(
            thresholdDifferences, conditionDifferences, resultDifferences);
        
        // サマリーを生成
        var summary = $"Source: {sourceResultsList.Count} results, Target: {targetResultsList.Count} results. " +
                     $"Differences - Thresholds: {thresholdDifferences.Count()}, Conditions: {conditionDifferences.Count()}, Results: {resultDifferences.Count()}. " +
                     $"Overall Similarity: {overallSimilarity:P1}";

        var comparison = new TestDataComparison(
            sourceSignal.Id,
            targetSignal.Id,
            thresholdDifferences,
            conditionDifferences,
            resultDifferences,
            recommendations,
            overallSimilarity,
            summary);

        _logger.LogInformation("Test data comparison completed with similarity score {Similarity}", 
            overallSimilarity);

        return await Task.FromResult(comparison);
    }

    /// <summary>
    /// 類似度を計算する
    /// </summary>
    public double CalculateSimilarity(
        CanSignal signal1,
        CanSignal signal2,
        SimilaritySearchCriteria criteria)
    {
        if (signal1 == null || signal2 == null || criteria == null)
            return 0.0;

        var breakdown = CalculateSimilarityBreakdown(signal1, signal2, criteria);
        return breakdown.CalculateWeightedSimilarity();
    }

    /// <summary>
    /// 類似度の詳細内訳を計算する
    /// </summary>
    public SimilarityBreakdown CalculateSimilarityBreakdown(
        CanSignal signal1,
        CanSignal signal2,
        SimilaritySearchCriteria criteria)
    {
        if (signal1 == null || signal2 == null || criteria == null)
            return new SimilarityBreakdown();

        var weights = DetermineWeights(criteria);

        var canIdSimilarity = criteria.CompareCanId ? 
            CalculateCanIdSimilarity(signal1.Identifier.CanId, signal2.Identifier.CanId) : 0.0;

        var signalNameSimilarity = criteria.CompareSignalName ? 
            CalculateStringSimilarity(signal1.Identifier.SignalName, signal2.Identifier.SignalName) : 0.0;

        var systemTypeSimilarity = criteria.CompareSystemType ? 
            (signal1.SystemType == signal2.SystemType ? 1.0 : 0.0) : 0.0;

        var valueRangeSimilarity = criteria.CompareValueRange ? 
            CalculateValueRangeSimilarity(signal1.Specification.ValueRange, signal2.Specification.ValueRange) : 0.0;

        var dataLengthSimilarity = criteria.CompareDataLength ? 
            CalculateDataLengthSimilarity(signal1.Specification.Length, signal2.Specification.Length) : 0.0;

        var cycleSimilarity = criteria.CompareCycle ? 
            CalculateCycleSimilarity(signal1.Timing.CycleTimeMs, signal2.Timing.CycleTimeMs) : 0.0;

        var oemCodeSimilarity = criteria.CompareOemCode ? 
            (signal1.OemCode.Equals(signal2.OemCode) ? 1.0 : 0.0) : 0.0;

        return new SimilarityBreakdown(
            canIdSimilarity,
            signalNameSimilarity,
            systemTypeSimilarity,
            valueRangeSimilarity,
            dataLengthSimilarity,
            cycleSimilarity,
            oemCodeSimilarity,
            weights);
    }

    /// <summary>
    /// 推奨レベルを決定する
    /// </summary>
    public RecommendationLevel DetermineRecommendationLevel(
        double similarityScore,
        SimilarityBreakdown breakdown,
        IEnumerable<AttributeDifference> differences)
    {
        var differencesList = differences?.ToList() ?? new List<AttributeDifference>();
        var significantDifferences = differencesList.Count(d => d.IsSignificant);

        // 類似度が非常に高い場合
        if (similarityScore >= 0.95 && significantDifferences == 0)
            return RecommendationLevel.Highly;

        // 類似度が高く、重要な差異が少ない場合
        if (similarityScore >= 0.85 && significantDifferences <= 1)
            return RecommendationLevel.High;

        // 類似度が中程度で、重要な差異が適度な場合
        if (similarityScore >= 0.7 && significantDifferences <= 2)
            return RecommendationLevel.Medium;

        // 類似度が低いか、重要な差異が多い場合
        if (similarityScore >= 0.5 && significantDifferences <= 3)
            return RecommendationLevel.Low;

        // それ以外は推奨しない
        return RecommendationLevel.NotRecommended;
    }

    #region Private Helper Methods

    private bool PassesFilters(CanSignal signal, SimilaritySearchCriteria criteria)
    {
        if (criteria.StandardSignalsOnly && !signal.IsStandard)
            return false;

        if (criteria.ActiveSignalsOnly && !signal.IsActive())
            return false;

        return true;
    }

    private (IEnumerable<string> matchedAttributes, IEnumerable<AttributeDifference> differences) 
        AnalyzeAttributeMatches(CanSignal signal1, CanSignal signal2, SimilaritySearchCriteria criteria)
    {
        var matched = new List<string>();
        var differences = new List<AttributeDifference>();

        if (criteria.CompareCanId)
        {
            if (signal1.Identifier.CanId == signal2.Identifier.CanId)
                matched.Add("CAN ID");
            else
                differences.Add(new AttributeDifference("CAN ID", 
                    signal1.Identifier.CanId, signal2.Identifier.CanId, true));
        }

        if (criteria.CompareSignalName)
        {
            var similarity = CalculateStringSimilarity(signal1.Identifier.SignalName, signal2.Identifier.SignalName);
            if (similarity > 0.8)
                matched.Add("Signal Name");
            else
                differences.Add(new AttributeDifference("Signal Name", 
                    signal1.Identifier.SignalName, signal2.Identifier.SignalName, similarity < 0.5));
        }

        if (criteria.CompareSystemType)
        {
            if (signal1.SystemType == signal2.SystemType)
                matched.Add("System Type");
            else
                differences.Add(new AttributeDifference("System Type", 
                    signal1.SystemType.ToString(), signal2.SystemType.ToString(), true));
        }

        if (criteria.CompareOemCode)
        {
            if (signal1.OemCode.Equals(signal2.OemCode))
                matched.Add("OEM Code");
            else
                differences.Add(new AttributeDifference("OEM Code", 
                    signal1.OemCode.ToString(), signal2.OemCode.ToString(), false));
        }

        return (matched, differences);
    }

    private SimilarityWeights DetermineWeights(SimilaritySearchCriteria criteria)
    {
        if (criteria.HasDetailedComparisons())
            return SimilarityWeights.CreateDetailedComparison();
        else
            return SimilarityWeights.CreateBasicComparison();
    }

    private double CalculateCanIdSimilarity(string canId1, string canId2)
    {
        if (string.IsNullOrEmpty(canId1) || string.IsNullOrEmpty(canId2))
            return 0.0;

        return canId1.Equals(canId2, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;
    }

    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // レーベンシュタイン距離を使用した類似度計算
        var distance = CalculateLevenshteinDistance(str1.ToLower(), str2.ToLower());
        var maxLength = Math.Max(str1.Length, str2.Length);
        
        return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
    }

    private int CalculateLevenshteinDistance(string str1, string str2)
    {
        var matrix = new int[str1.Length + 1, str2.Length + 1];

        for (int i = 0; i <= str1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= str2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[str1.Length, str2.Length];
    }

    private double CalculateValueRangeSimilarity(SignalValueRange range1, SignalValueRange range2)
    {
        if (range1 == null || range2 == null)
            return 0.0;

        var minOverlap = Math.Max(range1.MinValue, range2.MinValue);
        var maxOverlap = Math.Min(range1.MaxValue, range2.MaxValue);
        
        if (minOverlap > maxOverlap)
            return 0.0; // 重複なし

        var overlapRange = maxOverlap - minOverlap;
        var totalRange = Math.Max(range1.MaxValue, range2.MaxValue) - Math.Min(range1.MinValue, range2.MinValue);
        
        return totalRange == 0 ? 1.0 : overlapRange / totalRange;
    }

    private double CalculateDataLengthSimilarity(int length1, int length2)
    {
        if (length1 == length2)
            return 1.0;

        var difference = Math.Abs(length1 - length2);
        var maxLength = Math.Max(length1, length2);
        
        return maxLength == 0 ? 1.0 : 1.0 - (double)difference / maxLength;
    }

    private double CalculateCycleSimilarity(int cycle1, int cycle2)
    {
        if (cycle1 == cycle2)
            return 1.0;

        var difference = Math.Abs(cycle1 - cycle2);
        var maxCycle = Math.Max(cycle1, cycle2);
        
        return maxCycle == 0 ? 1.0 : 1.0 - (double)difference / maxCycle;
    }

    private IEnumerable<ThresholdDifference> AnalyzeThresholdDifferences(
        IList<AnomalyDetectionResult> sourceResults,
        IList<AnomalyDetectionResult> targetResults)
    {
        var differences = new List<ThresholdDifference>();

        // 簡単な例：異常レベル分布の比較
        var sourceAnomalyLevels = sourceResults.GroupBy(r => r.AnomalyLevel)
            .ToDictionary(g => g.Key, g => g.Count());
        var targetAnomalyLevels = targetResults.GroupBy(r => r.AnomalyLevel)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var level in Enum.GetValues<AnomalyLevel>())
        {
            var sourceCount = sourceAnomalyLevels.GetValueOrDefault(level, 0);
            var targetCount = targetAnomalyLevels.GetValueOrDefault(level, 0);

            if (sourceCount != targetCount)
            {
                differences.Add(new ThresholdDifference(
                    $"{level} Count",
                    sourceCount,
                    targetCount,
                    $"Difference in {level} anomaly detection count"));
            }
        }

        return differences;
    }

    private IEnumerable<DetectionConditionDifference> AnalyzeDetectionConditionDifferences(
        IList<AnomalyDetectionResult> sourceResults,
        IList<AnomalyDetectionResult> targetResults)
    {
        var differences = new List<DetectionConditionDifference>();

        // 検出条件の比較（簡単な例）
        var sourceConditions = sourceResults.Select(r => r.Details.TriggerCondition).Distinct().ToList();
        var targetConditions = targetResults.Select(r => r.Details.TriggerCondition).Distinct().ToList();

        var onlyInSource = sourceConditions.Except(targetConditions).ToList();
        var onlyInTarget = targetConditions.Except(sourceConditions).ToList();

        foreach (var condition in onlyInSource)
        {
            differences.Add(new DetectionConditionDifference(
                "Missing Condition",
                condition,
                "",
                DifferenceType.MissingSetting,
                true,
                "Condition exists in source but not in target"));
        }

        foreach (var condition in onlyInTarget)
        {
            differences.Add(new DetectionConditionDifference(
                "Additional Condition",
                "",
                condition,
                DifferenceType.AdditionalSetting,
                false,
                "Condition exists in target but not in source"));
        }

        return differences;
    }

    private IEnumerable<ResultDifference> AnalyzeResultDifferences(
        IList<AnomalyDetectionResult> sourceResults,
        IList<AnomalyDetectionResult> targetResults)
    {
        var differences = new List<ResultDifference>();

        // 結果統計の比較
        var sourceAvgConfidence = sourceResults.Any() ? sourceResults.Average(r => r.ConfidenceScore) : 0.0;
        var targetAvgConfidence = targetResults.Any() ? targetResults.Average(r => r.ConfidenceScore) : 0.0;

        if (Math.Abs(sourceAvgConfidence - targetAvgConfidence) > 0.1)
        {
            differences.Add(new ResultDifference(
                "Average Confidence",
                sourceAvgConfidence.ToString("F2"),
                targetAvgConfidence.ToString("F2"),
                DifferenceType.ValueDifference,
                Math.Abs(sourceAvgConfidence - targetAvgConfidence) > 0.2,
                Math.Abs(sourceAvgConfidence - targetAvgConfidence) > 0.3 ? ImpactLevel.High : ImpactLevel.Medium));
        }

        return differences;
    }

    private IEnumerable<ComparisonRecommendation> GenerateComparisonRecommendations(
        IEnumerable<ThresholdDifference> thresholdDifferences,
        IEnumerable<DetectionConditionDifference> conditionDifferences,
        IEnumerable<ResultDifference> resultDifferences,
        CanSignal sourceSignal,
        CanSignal targetSignal)
    {
        var recommendations = new List<ComparisonRecommendation>();

        var significantThresholdDiffs = thresholdDifferences.Where(d => d.IsSignificant).ToList();
        if (significantThresholdDiffs.Any())
        {
            recommendations.Add(new ComparisonRecommendation(
                RecommendationType.ThresholdAdjustment,
                RecommendationPriority.High,
                "Review and adjust threshold parameters based on significant differences",
                $"Found {significantThresholdDiffs.Count} significant threshold differences"));
        }

        var significantConditionDiffs = conditionDifferences.Where(d => d.IsSignificant).ToList();
        if (significantConditionDiffs.Any())
        {
            recommendations.Add(new ComparisonRecommendation(
                RecommendationType.ConditionChange,
                RecommendationPriority.Medium,
                "Consider updating detection conditions to align with target signal",
                $"Found {significantConditionDiffs.Count} significant condition differences"));
        }

        return recommendations;
    }

    private double CalculateOverallTestDataSimilarity(
        IEnumerable<ThresholdDifference> thresholdDifferences,
        IEnumerable<DetectionConditionDifference> conditionDifferences,
        IEnumerable<ResultDifference> resultDifferences)
    {
        var totalDifferences = thresholdDifferences.Count() + conditionDifferences.Count() + resultDifferences.Count();
        var significantDifferences = thresholdDifferences.Count(d => d.IsSignificant) +
                                   conditionDifferences.Count(d => d.IsSignificant) +
                                   resultDifferences.Count(d => d.IsSignificant);

        if (totalDifferences == 0)
            return 1.0;

        // 重要な差異の割合に基づいて類似度を計算
        var significantRatio = (double)significantDifferences / totalDifferences;
        return Math.Max(0.0, 1.0 - significantRatio);
    }

    private string GenerateComparisonSummary(
        int sourceCount,
        int targetCount,
        int thresholdDiffCount,
        int conditionDiffCount,
        int resultDiffCount,
        double similarity)
    {
        return $"Compared {sourceCount} source results with {targetCount} target results. " +
               $"Found {thresholdDiffCount} threshold differences, {conditionDiffCount} condition differences, " +
               $"and {resultDiffCount} result differences. Overall similarity: {similarity:F2}";
    }

    private string GenerateRecommendationReason(
        double similarityScore,
        SimilarityBreakdown breakdown,
        IEnumerable<AttributeDifference> differences)
    {
        var differencesList = differences.ToList();
        var significantCount = differencesList.Count(d => d.IsSignificant);

        if (similarityScore >= 0.9)
            return "High similarity with minimal differences - strongly recommended for reference";

        if (similarityScore >= 0.7 && significantCount <= 1)
            return "Good similarity with acceptable differences - recommended for reference";

        if (similarityScore >= 0.5)
            return "Moderate similarity - may be useful for reference with careful consideration";

        return "Low similarity - use with caution and thorough review";
    }

    #endregion
}