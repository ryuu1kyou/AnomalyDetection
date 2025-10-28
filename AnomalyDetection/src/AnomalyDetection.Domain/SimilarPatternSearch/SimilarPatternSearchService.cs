using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Logging;
using DifferenceType = AnomalyDetection.SimilarPatternSearch.DifferenceType;
using ImpactLevel = AnomalyDetection.SimilarPatternSearch.ImpactLevel;
using RecommendationType = AnomalyDetection.SimilarPatternSearch.RecommendationType;
using RecommendationPriority = AnomalyDetection.SimilarPatternSearch.RecommendationPriority;

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
    /// 推奨レベルを決定する（最適化版）
    /// </summary>
    public RecommendationLevel DetermineRecommendationLevel(
        double similarityScore,
        SimilarityBreakdown breakdown,
        IEnumerable<AttributeDifference> differences)
    {
        var differencesList = differences?.ToList() ?? [];
        var significantDifferences = differencesList.Count(d => d.IsSignificant);
        
        // 重要な属性の類似度を重視した判定
        var criticalSimilarities = new[]
        {
            breakdown.CanIdSimilarity,
            breakdown.SignalNameSimilarity,
            breakdown.SystemTypeSimilarity
        };
        
        var averageCriticalSimilarity = criticalSimilarities.Average();
        var minCriticalSimilarity = criticalSimilarities.Min();
        
        // 詳細属性の類似度
        var detailedSimilarities = new[]
        {
            breakdown.ValueRangeSimilarity,
            breakdown.DataLengthSimilarity,
            breakdown.CycleSimilarity
        };
        
        var averageDetailedSimilarity = detailedSimilarities.Average();
        
        // 最適化された判定ロジック
        
        // 非常に高い推奨: 全体的に高い類似度で重要な差異がない
        if (similarityScore >= 0.95 && 
            averageCriticalSimilarity >= 0.9 && 
            minCriticalSimilarity >= 0.8 && 
            significantDifferences == 0)
            return RecommendationLevel.Highly;

        // 高推奨: 重要属性が高い類似度で、差異が最小限
        if (similarityScore >= 0.85 && 
            averageCriticalSimilarity >= 0.8 && 
            minCriticalSimilarity >= 0.6 && 
            significantDifferences <= 1)
            return RecommendationLevel.High;

        // 中推奨: 重要属性が中程度以上で、差異が許容範囲
        if (similarityScore >= 0.7 && 
            averageCriticalSimilarity >= 0.6 && 
            minCriticalSimilarity >= 0.4 && 
            significantDifferences <= 2)
            return RecommendationLevel.Medium;

        // 低推奨: 最低限の類似度があり、差異が多くない
        if (similarityScore >= 0.5 && 
            averageCriticalSimilarity >= 0.4 && 
            significantDifferences <= 3)
            return RecommendationLevel.Low;

        // 特別ケース: 詳細属性が非常に類似している場合は推奨度を上げる
        if (similarityScore >= 0.6 && 
            averageDetailedSimilarity >= 0.8 && 
            significantDifferences <= 2)
            return RecommendationLevel.Medium;

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

    private static SimilarityWeights DetermineWeights(SimilaritySearchCriteria criteria)
    {
        // 動的重み付け: 比較対象に応じて重みを調整
        if (criteria.HasDetailedComparisons())
        {
            // 詳細比較時は技術的属性により重みを置く
            return new SimilarityWeights(
                canIdWeight: 0.20,        // CAN IDの重要度を下げる
                signalNameWeight: 0.25,   // 信号名は重要
                systemTypeWeight: 0.15,   // システム種別
                valueRangeWeight: 0.20,   // 値範囲を重視
                dataLengthWeight: 0.10,   // データ長
                cycleWeight: 0.07,        // 周期
                oemCodeWeight: 0.03);     // OEMコードは参考程度
        }
        else if (criteria.CompareOemCode)
        {
            // OEM比較時はOEMコードの重みを上げる
            return new SimilarityWeights(
                canIdWeight: 0.30,
                signalNameWeight: 0.30,
                systemTypeWeight: 0.20,
                valueRangeWeight: 0.05,
                dataLengthWeight: 0.05,
                cycleWeight: 0.02,
                oemCodeWeight: 0.08);     // OEMコードの重みを上げる
        }
        else
        {
            // 基本比較時は主要属性に集中
            return new SimilarityWeights(
                canIdWeight: 0.35,        // CAN IDを最重視
                signalNameWeight: 0.35,   // 信号名を最重視
                systemTypeWeight: 0.30,   // システム種別も重要
                valueRangeWeight: 0.0,
                dataLengthWeight: 0.0,
                cycleWeight: 0.0,
                oemCodeWeight: 0.0);
        }
    }

    private double CalculateCanIdSimilarity(string canId1, string canId2)
    {
        if (string.IsNullOrEmpty(canId1) || string.IsNullOrEmpty(canId2))
            return 0.0;

        return canId1.Equals(canId2, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.0;
    }

    private static double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        var s1 = str1.ToLower();
        var s2 = str2.ToLower();

        // 複数のアルゴリズムを組み合わせて最適な類似度を計算
        
        // 1. レーベンシュタイン距離ベースの類似度
        var levenshteinSimilarity = CalculateLevenshteinSimilarity(s1, s2);
        
        // 2. Jaro-Winkler類似度（名前の類似性に適している）
        var jaroWinklerSimilarity = CalculateJaroWinklerSimilarity(s1, s2);
        
        // 3. 共通部分文字列の類似度
        var substringSimilarity = CalculateSubstringSimilarity(s1, s2);
        
        // 4. トークンベースの類似度（アンダースコアやハイフンで分割）
        var tokenSimilarity = CalculateTokenSimilarity(s1, s2);
        
        // 重み付き平均で最終的な類似度を計算
        return (levenshteinSimilarity * 0.3 + 
                jaroWinklerSimilarity * 0.3 + 
                substringSimilarity * 0.2 + 
                tokenSimilarity * 0.2);
    }

    private static double CalculateLevenshteinSimilarity(string s1, string s2)
    {
        var distance = CalculateLevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
    }

    private static double CalculateJaroWinklerSimilarity(string s1, string s2)
    {
        if (s1 == s2) return 1.0;
        
        var len1 = s1.Length;
        var len2 = s2.Length;
        
        if (len1 == 0 || len2 == 0) return 0.0;
        
        var matchWindow = Math.Max(len1, len2) / 2 - 1;
        if (matchWindow < 0) matchWindow = 0;
        
        var s1Matches = new bool[len1];
        var s2Matches = new bool[len2];
        
        var matches = 0;
        var transpositions = 0;
        
        // 一致する文字を見つける
        for (int i = 0; i < len1; i++)
        {
            var start = Math.Max(0, i - matchWindow);
            var end = Math.Min(i + matchWindow + 1, len2);
            
            for (int j = start; j < end; j++)
            {
                if (s2Matches[j] || s1[i] != s2[j]) continue;
                s1Matches[i] = true;
                s2Matches[j] = true;
                matches++;
                break;
            }
        }
        
        if (matches == 0) return 0.0;
        
        // 転置を計算
        var k = 0;
        for (int i = 0; i < len1; i++)
        {
            if (!s1Matches[i]) continue;
            while (!s2Matches[k]) k++;
            if (s1[i] != s2[k]) transpositions++;
            k++;
        }
        
        var jaro = ((double)matches / len1 + (double)matches / len2 + 
                   (matches - transpositions / 2.0) / matches) / 3.0;
        
        // Winkler拡張: 共通プレフィックスにボーナスを与える
        var prefix = 0;
        for (int i = 0; i < Math.Min(len1, len2) && i < 4; i++)
        {
            if (s1[i] == s2[i]) prefix++;
            else break;
        }
        
        return jaro + (0.1 * prefix * (1.0 - jaro));
    }

    private static double CalculateSubstringSimilarity(string s1, string s2)
    {
        var longestCommonSubstring = FindLongestCommonSubstring(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return maxLength == 0 ? 1.0 : (double)longestCommonSubstring / maxLength;
    }

    private static int FindLongestCommonSubstring(string s1, string s2)
    {
        var len1 = s1.Length;
        var len2 = s2.Length;
        var dp = new int[len1 + 1, len2 + 1];
        var maxLength = 0;
        
        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                if (s1[i - 1] == s2[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                    maxLength = Math.Max(maxLength, dp[i, j]);
                }
            }
        }
        
        return maxLength;
    }

    private static double CalculateTokenSimilarity(string s1, string s2)
    {
        var tokens1 = s1.Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries);
        var tokens2 = s2.Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries);
        
        if (tokens1.Length == 0 && tokens2.Length == 0) return 1.0;
        if (tokens1.Length == 0 || tokens2.Length == 0) return 0.0;
        
        var commonTokens = tokens1.Intersect(tokens2, StringComparer.OrdinalIgnoreCase).Count();
        var totalTokens = Math.Max(tokens1.Length, tokens2.Length);
        
        return (double)commonTokens / totalTokens;
    }

    private static int CalculateLevenshteinDistance(string str1, string str2)
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

    private static double CalculateValueRangeSimilarity(SignalValueRange range1, SignalValueRange range2)
    {
        if (range1 == null || range2 == null)
            return 0.0;

        // 完全一致の場合
        if (Math.Abs(range1.MinValue - range2.MinValue) < double.Epsilon && 
            Math.Abs(range1.MaxValue - range2.MaxValue) < double.Epsilon)
            return 1.0;

        // 重複範囲の計算
        var minOverlap = Math.Max(range1.MinValue, range2.MinValue);
        var maxOverlap = Math.Min(range1.MaxValue, range2.MaxValue);
        
        if (minOverlap > maxOverlap)
        {
            // 重複がない場合、距離に基づいて類似度を計算
            var gap = minOverlap - maxOverlap;
            var range1Size = range1.MaxValue - range1.MinValue;
            var range2Size = range2.MaxValue - range2.MinValue;
            var avgRangeSize = (range1Size + range2Size) / 2.0;
            
            // 距離が平均範囲サイズの10%以内なら部分的な類似度を与える
            if (avgRangeSize > 0 && gap / avgRangeSize <= 0.1)
                return Math.Max(0.0, 0.3 - (gap / avgRangeSize) * 3.0);
            
            return 0.0;
        }

        // 重複がある場合の類似度計算
        var overlapRange = maxOverlap - minOverlap;
        var union = Math.Max(range1.MaxValue, range2.MaxValue) - Math.Min(range1.MinValue, range2.MinValue);
        
        if (union == 0) return 1.0;
        
        // Jaccard係数ベースの類似度
        var jaccardSimilarity = overlapRange / union;
        
        // 範囲サイズの類似度も考慮
        var size1 = range1.MaxValue - range1.MinValue;
        var size2 = range2.MaxValue - range2.MinValue;
        var sizeSimilarity = size1 == 0 && size2 == 0 ? 1.0 :
            1.0 - Math.Abs(size1 - size2) / Math.Max(size1, size2);
        
        // 重み付き平均
        return jaccardSimilarity * 0.7 + sizeSimilarity * 0.3;
    }

    private double CalculateDataLengthSimilarity(int length1, int length2)
    {
        if (length1 == length2)
            return 1.0;

        var difference = Math.Abs(length1 - length2);
        var maxLength = Math.Max(length1, length2);
        
        return maxLength == 0 ? 1.0 : 1.0 - (double)difference / maxLength;
    }

    private static double CalculateCycleSimilarity(int cycle1, int cycle2)
    {
        if (cycle1 == cycle2)
            return 1.0;

        if (cycle1 <= 0 || cycle2 <= 0)
            return 0.0;

        var difference = Math.Abs(cycle1 - cycle2);
        var maxCycle = Math.Max(cycle1, cycle2);
        var minCycle = Math.Min(cycle1, cycle2);
        
        // 標準的なCAN周期（10ms, 20ms, 50ms, 100ms, 200ms, 500ms, 1000ms）を考慮
        var standardCycles = new[] { 10, 20, 50, 100, 200, 500, 1000 };
        
        // 両方が標準周期の場合、より高い類似度を与える
        var isStandard1 = standardCycles.Contains(cycle1);
        var isStandard2 = standardCycles.Contains(cycle2);
        
        if (isStandard1 && isStandard2)
        {
            // 標準周期間の類似度は特別に計算
            var ratio = (double)maxCycle / minCycle;
            if (ratio <= 2.0) return 0.8; // 2倍以内なら高い類似度
            if (ratio <= 5.0) return 0.6; // 5倍以内なら中程度
            if (ratio <= 10.0) return 0.4; // 10倍以内なら低い類似度
        }
        
        // 相対的な差異による類似度計算
        var relativeDifference = (double)difference / maxCycle;
        
        // 10%以内の差異なら非常に高い類似度
        if (relativeDifference <= 0.1) return 0.95;
        
        // 25%以内の差異なら高い類似度
        if (relativeDifference <= 0.25) return 0.8;
        
        // 50%以内の差異なら中程度の類似度
        if (relativeDifference <= 0.5) return 0.6;
        
        // それ以外は線形減少
        return Math.Max(0.0, 1.0 - relativeDifference);
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
                DifferenceType.ConditionDifference,
                true,
                "Condition exists in source but not in target"));
        }

        foreach (var condition in onlyInTarget)
        {
            differences.Add(new DetectionConditionDifference(
                "Additional Condition",
                "",
                condition,
                DifferenceType.ConditionDifference,
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
        if (significantThresholdDiffs.Count > 0)
        {
            recommendations.Add(new ComparisonRecommendation(
                RecommendationType.ThresholdAdjustment,
                RecommendationPriority.High,
                "Review and adjust threshold parameters based on significant differences",
                $"Found {significantThresholdDiffs.Count} significant threshold differences"));
        }

        var significantConditionDiffs = conditionDifferences.Where(d => d.IsSignificant).ToList();
        if (significantConditionDiffs.Count > 0)
        {
            recommendations.Add(new ComparisonRecommendation(
                RecommendationType.DetectionConditionChange,
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