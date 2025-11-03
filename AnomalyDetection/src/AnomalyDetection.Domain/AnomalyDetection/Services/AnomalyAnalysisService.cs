using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 異常検出分析ドメインサービス
/// </summary>
public class AnomalyAnalysisService : DomainService, IAnomalyAnalysisService
{
    private readonly IRepository<AnomalyDetectionResult, Guid> _anomalyResultRepository;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;
    private readonly IStatisticalThresholdOptimizer _thresholdOptimizer;
    private readonly ILogger<AnomalyAnalysisService> _logger;

    public AnomalyAnalysisService(
        IRepository<AnomalyDetectionResult, Guid> anomalyResultRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IStatisticalThresholdOptimizer thresholdOptimizer,
        ILogger<AnomalyAnalysisService> logger)
    {
        _anomalyResultRepository = anomalyResultRepository;
        _detectionLogicRepository = detectionLogicRepository;
        _thresholdOptimizer = thresholdOptimizer;
        _logger = logger;
    }

    /// <summary>
    /// 異常パターンを分析する
    /// </summary>
    public async Task<AnomalyPatternAnalysisResult> AnalyzePatternAsync(
        Guid canSignalId,
        DateTime analysisStartDate,
        DateTime analysisEndDate)
    {
        _logger.LogInformation("Starting anomaly pattern analysis for CAN signal {CanSignalId} from {StartDate} to {EndDate}",
            canSignalId, analysisStartDate, analysisEndDate);

        // 指定期間の異常検出結果を取得
        var anomalyResults = await _anomalyResultRepository.GetListAsync(
            r => r.CanSignalId == canSignalId &&
                 r.DetectedAt >= analysisStartDate &&
                 r.DetectedAt <= analysisEndDate);

        if (!anomalyResults.Any())
        {
            _logger.LogWarning("No anomaly results found for CAN signal {CanSignalId} in the specified period", canSignalId);
            return CreateEmptyPatternAnalysisResult(canSignalId, analysisStartDate, analysisEndDate);
        }

        // 異常タイプ別分布を計算
        var anomalyTypeDistribution = anomalyResults
            .GroupBy(r => r.AnomalyType)
            .ToDictionary(g => g.Key, g => g.Count());

        // 異常レベル別分布を計算
        var anomalyLevelDistribution = anomalyResults
            .GroupBy(r => r.AnomalyLevel)
            .ToDictionary(g => g.Key, g => g.Count());

        // 頻度パターンを分析
        var frequencyPatterns = AnalyzeFrequencyPatterns(anomalyResults);

        // 相関を分析
        var correlations = await AnalyzeCorrelationsAsync(canSignalId, anomalyResults);

        // 平均検出時間を計算
        var averageDetectionDurationMs = anomalyResults
            .Where(r => r.DetectionDuration.TotalMilliseconds > 0)
            .Average(r => r.DetectionDuration.TotalMilliseconds);

        // 誤検出率を計算
        var falsePositiveRate = (double)anomalyResults.Count(r => r.IsFalsePositive()) / anomalyResults.Count;

        // 分析サマリーを生成
        var analysisSummary = GenerateAnalysisSummary(anomalyResults, anomalyTypeDistribution, falsePositiveRate);

        var result = new AnomalyPatternAnalysisResult(
            canSignalId,
            analysisStartDate,
            analysisEndDate,
            anomalyResults.Count,
            anomalyTypeDistribution,
            anomalyLevelDistribution,
            frequencyPatterns,
            correlations,
            averageDetectionDurationMs,
            falsePositiveRate,
            analysisSummary);

        _logger.LogInformation("Completed anomaly pattern analysis for CAN signal {CanSignalId}. Found {TotalAnomalies} anomalies",
            canSignalId, anomalyResults.Count);

        return result;
    }

    /// <summary>
    /// 閾値最適化推奨を生成する
    /// </summary>
    public async Task<ThresholdRecommendationResult> GenerateThresholdRecommendationsAsync(
        Guid detectionLogicId,
        DateTime analysisStartDate,
        DateTime analysisEndDate)
    {
        _logger.LogInformation("Generating threshold recommendations for detection logic {DetectionLogicId} from {StartDate} to {EndDate}",
            detectionLogicId, analysisStartDate, analysisEndDate);

        // 検出ロジックを取得
        var detectionLogic = await _detectionLogicRepository.GetAsync(detectionLogicId);

        // 指定期間の検出結果を取得
        var detectionResults = await _anomalyResultRepository.GetListAsync(
            r => r.DetectionLogicId == detectionLogicId &&
                 r.DetectedAt >= analysisStartDate &&
                 r.DetectedAt <= analysisEndDate);

        if (!detectionResults.Any())
        {
            _logger.LogWarning("No detection results found for logic {DetectionLogicId} in the specified period", detectionLogicId);
            return CreateEmptyThresholdRecommendationResult(detectionLogicId, analysisStartDate, analysisEndDate);
        }

        // 現在のメトリクスを計算
        var currentMetrics = CalculateOptimizationMetrics(detectionResults);

        // 閾値推奨を生成
        var recommendations = GenerateThresholdRecommendations(detectionLogic, detectionResults);

        // 予測メトリクスを計算（シミュレーション）
        var predictedMetrics = SimulatePredictedMetrics(currentMetrics, recommendations);

        // 期待される改善を計算
        var expectedImprovement = CalculateExpectedImprovement(currentMetrics, predictedMetrics);

        // 推奨サマリーを生成
        var recommendationSummary = GenerateRecommendationSummary(recommendations, expectedImprovement);

        var result = new ThresholdRecommendationResult(
            detectionLogicId,
            analysisStartDate,
            analysisEndDate,
            recommendations,
            currentMetrics,
            predictedMetrics,
            expectedImprovement,
            recommendationSummary);

        _logger.LogInformation("Generated {RecommendationCount} threshold recommendations for detection logic {DetectionLogicId}",
            recommendations.Count, detectionLogicId);

        return result;
    }

    /// <summary>
    /// 検出精度を評価する
    /// </summary>
    public async Task<DetectionAccuracyMetrics> CalculateDetectionAccuracyAsync(
        Guid detectionLogicId,
        DateTime analysisStartDate,
        DateTime analysisEndDate)
    {
        _logger.LogInformation("Calculating detection accuracy for logic {DetectionLogicId} from {StartDate} to {EndDate}",
            detectionLogicId, analysisStartDate, analysisEndDate);

        // 指定期間の検出結果を取得
        var detectionResults = await _anomalyResultRepository.GetListAsync(
            r => r.DetectionLogicId == detectionLogicId &&
                 r.DetectedAt >= analysisStartDate &&
                 r.DetectedAt <= analysisEndDate);

        if (!detectionResults.Any())
        {
            _logger.LogWarning("No detection results found for logic {DetectionLogicId} in the specified period", detectionLogicId);
            return CreateEmptyAccuracyMetrics(detectionLogicId, analysisStartDate, analysisEndDate);
        }

        // 混同行列の要素を計算
        var truePositives = detectionResults.Count(r => !r.IsFalsePositive() && r.IsResolved());
        var falsePositives = detectionResults.Count(r => r.IsFalsePositive());
        var falseNegatives = EstimateFalseNegatives(detectionResults); // 実際の実装では外部データが必要
        var trueNegatives = EstimateTrueNegatives(detectionResults); // 実際の実装では外部データが必要

        // 検出時間の統計を計算
        var detectionTimes = detectionResults
            .Where(r => r.DetectionDuration.TotalMilliseconds > 0)
            .Select(r => r.DetectionDuration.TotalMilliseconds)
            .ToList();

        var averageDetectionTimeMs = detectionTimes.Any() ? detectionTimes.Average() : 0.0;
        var medianDetectionTimeMs = detectionTimes.Any() ? CalculateMedian(detectionTimes) : 0.0;

        // 異常タイプ別精度を計算
        var accuracyByType = CalculateAccuracyByAnomalyType(detectionResults);

        // 時間範囲別精度を計算
        var accuracyByTime = CalculateAccuracyByTimeRange(detectionResults, analysisStartDate, analysisEndDate);

        // パフォーマンスサマリーを生成
        var performanceSummary = GeneratePerformanceSummary(truePositives, falsePositives, falseNegatives, trueNegatives);

        var result = new DetectionAccuracyMetrics(
            detectionLogicId,
            analysisStartDate,
            analysisEndDate,
            detectionResults.Count,
            truePositives,
            falsePositives,
            trueNegatives,
            falseNegatives,
            averageDetectionTimeMs,
            medianDetectionTimeMs,
            accuracyByType,
            accuracyByTime,
            performanceSummary);

        _logger.LogInformation("Calculated detection accuracy for logic {DetectionLogicId}. Precision: {Precision:F3}, Recall: {Recall:F3}, F1: {F1:F3}",
            detectionLogicId, result.Precision, result.Recall, result.F1Score);

        return result;
    }

    #region Private Helper Methods

    private AnomalyPatternAnalysisResult CreateEmptyPatternAnalysisResult(Guid canSignalId, DateTime startDate, DateTime endDate)
    {
        return new AnomalyPatternAnalysisResult(
            canSignalId,
            startDate,
            endDate,
            0,
            new Dictionary<AnomalyType, int>(),
            new Dictionary<AnomalyLevel, int>(),
            new List<AnomalyFrequencyPattern>(),
            new List<AnomalyCorrelation>(),
            0.0,
            0.0,
            "No anomalies found in the specified period.");
    }

    private List<AnomalyFrequencyPattern> AnalyzeFrequencyPatterns(List<AnomalyDetectionResult> anomalyResults)
    {
        var patterns = new List<AnomalyFrequencyPattern>();

        if (!anomalyResults.Any())
            return patterns;

        // 時間別パターン分析（時間単位）
        var hourlyDistribution = anomalyResults
            .GroupBy(r => r.DetectedAt.Hour)
            .Where(g => g.Count() > 1)
            .Select(g => new AnomalyFrequencyPattern(
                $"Hourly Pattern {g.Key:D2}:00",
                TimeSpan.FromHours(1),
                g.Count(),
                CalculatePatternConfidence(g.Count(), anomalyResults.Count, 24))) // 24時間での信頼度
            .OrderByDescending(p => p.Frequency)
            .ToList();

        patterns.AddRange(hourlyDistribution);

        // 曜日別パターン分析
        var dailyDistribution = anomalyResults
            .GroupBy(r => r.DetectedAt.DayOfWeek)
            .Where(g => g.Count() > 1)
            .Select(g => new AnomalyFrequencyPattern(
                $"Daily Pattern {g.Key}",
                TimeSpan.FromDays(1),
                g.Count(),
                CalculatePatternConfidence(g.Count(), anomalyResults.Count, 7))) // 7日間での信頼度
            .OrderByDescending(p => p.Frequency)
            .ToList();

        patterns.AddRange(dailyDistribution);

        // 異常レベル別パターン分析
        var levelPatterns = anomalyResults
            .GroupBy(r => r.AnomalyLevel)
            .Where(g => g.Count() > 1)
            .Select(g => new AnomalyFrequencyPattern(
                $"Level Pattern {g.Key}",
                TimeSpan.Zero, // レベルパターンは時間間隔なし
                g.Count(),
                (double)g.Count() / anomalyResults.Count)) // 全体に対する割合
            .OrderByDescending(p => p.Frequency)
            .ToList();

        patterns.AddRange(levelPatterns);

        // 異常タイプ別パターン分析
        var typePatterns = anomalyResults
            .GroupBy(r => r.AnomalyType)
            .Where(g => g.Count() > 1)
            .Select(g => new AnomalyFrequencyPattern(
                $"Type Pattern {g.Key}",
                TimeSpan.Zero, // タイプパターンは時間間隔なし
                g.Count(),
                (double)g.Count() / anomalyResults.Count)) // 全体に対する割合
            .OrderByDescending(p => p.Frequency)
            .ToList();

        patterns.AddRange(typePatterns);

        return patterns;
    }

    private double CalculatePatternConfidence(int patternCount, int totalCount, int expectedSlots)
    {
        // 期待値に対する実際の出現頻度の比率を計算
        var expectedFrequency = (double)totalCount / expectedSlots;
        var actualFrequency = patternCount;

        // 統計的有意性を考慮した信頼度計算
        var ratio = actualFrequency / Math.Max(expectedFrequency, 1.0);

        // 0.0から1.0の範囲に正規化（2倍以上の頻度で最大信頼度）
        return Math.Min(1.0, Math.Max(0.0, (ratio - 1.0) / 1.0));
    }

    private async Task<List<AnomalyCorrelation>> AnalyzeCorrelationsAsync(Guid canSignalId, List<AnomalyDetectionResult> anomalyResults)
    {
        var correlations = new List<AnomalyCorrelation>();

        if (!anomalyResults.Any())
            return correlations;

        try
        {
            // 同じ時間帯に発生した他の信号の異常を検索
            var timeWindows = anomalyResults.Select(r => new
            {
                StartTime = r.DetectedAt.AddMinutes(-5), // 5分前後の時間窓
                EndTime = r.DetectedAt.AddMinutes(5),
                Result = r
            }).ToList();

            // 各時間窓で他の信号の異常を検索
            foreach (var window in timeWindows)
            {
                var correlatedResults = await _anomalyResultRepository.GetListAsync(
                    r => r.CanSignalId != canSignalId &&
                         r.DetectedAt >= window.StartTime &&
                         r.DetectedAt <= window.EndTime);

                // 相関のある信号をグループ化
                var signalGroups = correlatedResults
                    .GroupBy(r => r.CanSignalId)
                    .Where(g => g.Count() >= 2) // 最低2回以上の相関
                    .ToList();

                foreach (var group in signalGroups)
                {
                    var correlationCoefficient = CalculateTemporalCorrelation(
                        anomalyResults,
                        group.ToList());

                    if (Math.Abs(correlationCoefficient) > 0.3) // 相関閾値
                    {
                        var existingCorrelation = correlations
                            .FirstOrDefault(c => c.RelatedCanSignalId == group.Key);

                        if (existingCorrelation == null)
                        {
                            correlations.Add(new AnomalyCorrelation(
                                group.Key,
                                $"Signal_{group.Key:N}",
                                correlationCoefficient,
                                correlationCoefficient > 0 ? "Positive Temporal" : "Negative Temporal"));
                        }
                    }
                }
            }

            // 異常レベル相関の分析
            await AnalyzeLevelCorrelationsAsync(canSignalId, anomalyResults, correlations);

            // 異常タイプ相関の分析
            await AnalyzeTypeCorrelationsAsync(canSignalId, anomalyResults, correlations);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing correlations for signal {CanSignalId}", canSignalId);
        }

        return correlations.Take(10).ToList(); // 上位10件の相関のみ返す
    }

    private double CalculateTemporalCorrelation(List<AnomalyDetectionResult> baseResults, List<AnomalyDetectionResult> correlatedResults)
    {
        if (!baseResults.Any() || !correlatedResults.Any())
            return 0.0;

        // 時間的近接性に基づく相関係数の計算
        var correlationCount = 0;
        var totalComparisons = 0;

        foreach (var baseResult in baseResults)
        {
            totalComparisons++;
            var hasCorrelation = correlatedResults.Any(cr =>
                Math.Abs((cr.DetectedAt - baseResult.DetectedAt).TotalMinutes) <= 5);

            if (hasCorrelation)
                correlationCount++;
        }

        return totalComparisons > 0 ? (double)correlationCount / totalComparisons : 0.0;
    }

    private async Task AnalyzeLevelCorrelationsAsync(Guid canSignalId, List<AnomalyDetectionResult> anomalyResults, List<AnomalyCorrelation> correlations)
    {
        // 同じ異常レベルの他の信号との相関を分析
        var levelGroups = anomalyResults.GroupBy(r => r.AnomalyLevel).ToList();

        foreach (var levelGroup in levelGroups.Where(g => g.Count() > 1))
        {
            var sameLevel = await _anomalyResultRepository.GetListAsync(
                r => r.CanSignalId != canSignalId &&
                     r.AnomalyLevel == levelGroup.Key);

            if (sameLevel.Count > 2)
            {
                var coefficient = (double)sameLevel.Count / (sameLevel.Count + levelGroup.Count());

                if (coefficient > 0.2)
                {
                    correlations.Add(new AnomalyCorrelation(
                        Guid.Empty, // レベル相関は特定の信号ではない
                        $"Level_{levelGroup.Key}",
                        coefficient,
                        "Level Correlation"));
                }
            }
        }
    }

    private async Task AnalyzeTypeCorrelationsAsync(Guid canSignalId, List<AnomalyDetectionResult> anomalyResults, List<AnomalyCorrelation> correlations)
    {
        // 同じ異常タイプの他の信号との相関を分析
        var typeGroups = anomalyResults.GroupBy(r => r.AnomalyType).ToList();

        foreach (var typeGroup in typeGroups.Where(g => g.Count() > 1))
        {
            var sameType = await _anomalyResultRepository.GetListAsync(
                r => r.CanSignalId != canSignalId &&
                     r.AnomalyType == typeGroup.Key);

            if (sameType.Count > 2)
            {
                var coefficient = (double)sameType.Count / (sameType.Count + typeGroup.Count());

                if (coefficient > 0.2)
                {
                    correlations.Add(new AnomalyCorrelation(
                        Guid.Empty, // タイプ相関は特定の信号ではない
                        $"Type_{typeGroup.Key}",
                        coefficient,
                        "Type Correlation"));
                }
            }
        }
    }

    private string GenerateAnalysisSummary(
        List<AnomalyDetectionResult> anomalyResults,
        Dictionary<AnomalyType, int> typeDistribution,
        double falsePositiveRate)
    {
        var mostCommonType = typeDistribution.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        var criticalCount = anomalyResults.Count(r => r.AnomalyLevel >= AnomalyLevel.Critical);

        return $"Analysis of {anomalyResults.Count} anomalies. " +
               $"Most common type: {mostCommonType.Key} ({mostCommonType.Value} occurrences). " +
               $"Critical anomalies: {criticalCount}. " +
               $"False positive rate: {falsePositiveRate:P2}.";
    }

    private ThresholdRecommendationResult CreateEmptyThresholdRecommendationResult(Guid detectionLogicId, DateTime startDate, DateTime endDate)
    {
        var emptyMetrics = new OptimizationMetrics(0, 0, 0, 0, 0, 0, 0);
        return new ThresholdRecommendationResult(
            detectionLogicId,
            startDate,
            endDate,
            new List<ThresholdRecommendation>(),
            emptyMetrics,
            emptyMetrics,
            0.0,
            "No detection results found for analysis.");
    }

    private OptimizationMetrics CalculateOptimizationMetrics(List<AnomalyDetectionResult> detectionResults)
    {
        if (!detectionResults.Any())
        {
            return new OptimizationMetrics(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
        }

        // より正確な分類
        var truePositives = detectionResults.Count(r => !r.IsFalsePositive() && r.IsValidated);
        var falsePositives = detectionResults.Count(r => r.IsFalsePositive());
        var unvalidated = detectionResults.Count(r => !r.IsValidated);
        var totalDetections = detectionResults.Count;

        // 未検証の結果を真陽性として仮定（保守的な推定）
        var estimatedTruePositives = truePositives + (int)(unvalidated * 0.7); // 70%が真陽性と仮定
        var estimatedFalseNegatives = EstimateFalseNegatives(detectionResults);
        var estimatedTrueNegatives = EstimateTrueNegatives(detectionResults);

        // メトリクス計算
        var detectionRate = totalDetections > 0 ? (double)estimatedTruePositives / totalDetections : 0.0;
        var falsePositiveRate = totalDetections > 0 ? (double)falsePositives / totalDetections : 0.0;
        var falseNegativeRate = estimatedTruePositives + estimatedFalseNegatives > 0 ?
            (double)estimatedFalseNegatives / (estimatedTruePositives + estimatedFalseNegatives) : 0.0;

        var precision = estimatedTruePositives + falsePositives > 0 ?
            (double)estimatedTruePositives / (estimatedTruePositives + falsePositives) : 0.0;
        var recall = estimatedTruePositives + estimatedFalseNegatives > 0 ?
            (double)estimatedTruePositives / (estimatedTruePositives + estimatedFalseNegatives) : 0.0;
        var f1Score = precision + recall > 0 ? 2 * (precision * recall) / (precision + recall) : 0.0;

        // 検出時間の統計
        var detectionTimes = detectionResults
            .Where(r => r.DetectionDuration.TotalMilliseconds > 0)
            .Select(r => r.DetectionDuration.TotalMilliseconds)
            .ToList();

        var averageDetectionTime = detectionTimes.Any() ? detectionTimes.Average() : 0.0;

        return new OptimizationMetrics(
            detectionRate,
            falsePositiveRate,
            falseNegativeRate,
            precision,
            recall,
            f1Score,
            averageDetectionTime);
    }

    private List<ThresholdRecommendation> GenerateThresholdRecommendations(
        CanAnomalyDetectionLogic detectionLogic,
        List<AnomalyDetectionResult> detectionResults)
    {
        var recommendations = new List<ThresholdRecommendation>();

        if (!detectionResults.Any())
            return recommendations;

        // 誤検出率分析
        var falsePositiveRate = (double)detectionResults.Count(r => r.IsFalsePositive()) / detectionResults.Count;
        var truePositiveRate = (double)detectionResults.Count(r => !r.IsFalsePositive() && r.IsValidated) / detectionResults.Count;

        // 1. 誤検出率が高い場合の閾値調整推奨
        if (falsePositiveRate > 0.15) // 15%以上の誤検出率
        {
            var recommendedIncrease = CalculateThresholdAdjustment(falsePositiveRate, "increase");
            recommendations.Add(new ThresholdRecommendation(
                "Detection Threshold",
                "Current Value",
                $"Increase by {recommendedIncrease:P0}",
                $"High false positive rate ({falsePositiveRate:P1}) detected. Increasing threshold will reduce false positives.",
                CalculatePriority(falsePositiveRate, 0.15, 0.30),
                CalculateConfidence(falsePositiveRate, detectionResults.Count)));
        }

        // 2. 検出漏れが多い場合の閾値調整推奨
        if (truePositiveRate < 0.70 && falsePositiveRate < 0.05) // 真陽性率が低く、誤検出率が低い場合
        {
            var recommendedDecrease = CalculateThresholdAdjustment(truePositiveRate, "decrease");
            recommendations.Add(new ThresholdRecommendation(
                "Detection Threshold",
                "Current Value",
                $"Decrease by {recommendedDecrease:P0}",
                $"Low detection rate ({truePositiveRate:P1}) with acceptable false positive rate. Decreasing threshold may improve detection.",
                CalculatePriority(1.0 - truePositiveRate, 0.30, 0.50),
                CalculateConfidence(truePositiveRate, detectionResults.Count)));
        }

        // 3. 信頼度スコア分析に基づく推奨
        var lowConfidenceResults = detectionResults.Where(r => r.ConfidenceScore < 0.6).ToList();
        if (lowConfidenceResults.Count > detectionResults.Count * 0.3) // 30%以上が低信頼度
        {
            recommendations.Add(new ThresholdRecommendation(
                "Confidence Threshold",
                "0.5",
                "0.7",
                $"Many detections ({lowConfidenceResults.Count}) have low confidence scores. Consider raising confidence threshold.",
                0.6,
                0.8));
        }

        // 4. 異常レベル分布に基づく推奨
        var criticalCount = detectionResults.Count(r => r.AnomalyLevel >= AnomalyLevel.Critical);
        var warningCount = detectionResults.Count(r => r.AnomalyLevel == AnomalyLevel.Warning);

        if (warningCount > criticalCount * 5) // 警告が重要な異常の5倍以上
        {
            recommendations.Add(new ThresholdRecommendation(
                "Warning Level Threshold",
                "Current",
                "Stricter",
                $"High ratio of warnings to critical alerts ({warningCount}:{criticalCount}). Consider stricter warning criteria.",
                0.5,
                0.7));
        }

        // 5. 検出時間に基づく推奨
        var slowDetections = detectionResults.Where(r => r.DetectionDuration.TotalMilliseconds > 1000).ToList();
        if (slowDetections.Count > detectionResults.Count * 0.2) // 20%以上が1秒超
        {
            recommendations.Add(new ThresholdRecommendation(
                "Performance Threshold",
                "Current Algorithm",
                "Optimized Algorithm",
                $"Many detections ({slowDetections.Count}) are slow (>1s). Consider algorithm optimization or simpler thresholds.",
                0.7,
                0.6));
        }

        // 6. 時間パターンに基づく推奨
        var timeBasedRecommendations = GenerateTimeBasedRecommendations(detectionResults);
        recommendations.AddRange(timeBasedRecommendations);

        return recommendations.OrderByDescending(r => r.Priority).Take(5).ToList(); // 上位5件の推奨
    }

    private double CalculateThresholdAdjustment(double currentRate, string direction)
    {
        // 現在の率に基づいて調整幅を計算
        return direction == "increase" ?
            Math.Min(0.5, currentRate * 2) :  // 最大50%増加
            Math.Min(0.3, (1.0 - currentRate) * 1.5); // 最大30%減少
    }

    private double CalculatePriority(double currentValue, double minThreshold, double maxThreshold)
    {
        // 閾値範囲に基づいて優先度を計算（0.0-1.0）
        if (currentValue <= minThreshold) return 0.3;
        if (currentValue >= maxThreshold) return 1.0;

        return 0.3 + (currentValue - minThreshold) / (maxThreshold - minThreshold) * 0.7;
    }

    private double CalculateConfidence(double rate, int sampleSize)
    {
        // サンプルサイズと率に基づいて信頼度を計算
        var baseLine = Math.Min(0.9, 0.5 + Math.Log10(sampleSize) * 0.1);
        var rateConfidence = 1.0 - Math.Abs(rate - 0.5) * 0.5; // 極端な値ほど信頼度が高い

        return Math.Min(1.0, baseLine * rateConfidence);
    }

    private List<ThresholdRecommendation> GenerateTimeBasedRecommendations(List<AnomalyDetectionResult> detectionResults)
    {
        var recommendations = new List<ThresholdRecommendation>();

        // 時間帯別の異常発生パターンを分析
        var hourlyGroups = detectionResults.GroupBy(r => r.DetectedAt.Hour).ToList();
        var peakHours = hourlyGroups
            .Where(g => g.Count() > detectionResults.Count * 0.1) // 全体の10%以上
            .OrderByDescending(g => g.Count())
            .Take(3)
            .ToList();

        if (peakHours.Any())
        {
            var peakHoursList = string.Join(", ", peakHours.Select(h => $"{h.Key}:00"));
            recommendations.Add(new ThresholdRecommendation(
                "Time-based Threshold",
                "Static",
                "Dynamic (Peak Hours)",
                $"Peak anomaly hours detected: {peakHoursList}. Consider time-based threshold adjustment.",
                0.6,
                0.8));
        }

        return recommendations;
    }

    private OptimizationMetrics SimulatePredictedMetrics(OptimizationMetrics currentMetrics, List<ThresholdRecommendation> recommendations)
    {
        // 簡単なシミュレーション - 実際の実装ではより複雑な予測モデルを使用
        var improvementFactor = recommendations.Any() ? 1.1 : 1.0;

        return new OptimizationMetrics(
            currentMetrics.DetectionRate * improvementFactor,
            Math.Max(0, currentMetrics.FalsePositiveRate * 0.9),
            Math.Max(0, currentMetrics.FalseNegativeRate * 0.95),
            currentMetrics.Precision * improvementFactor,
            currentMetrics.Recall * improvementFactor,
            currentMetrics.F1Score * improvementFactor,
            currentMetrics.AverageDetectionTimeMs);
    }

    private double CalculateExpectedImprovement(OptimizationMetrics current, OptimizationMetrics predicted)
    {
        return predicted.F1Score - current.F1Score;
    }

    private string GenerateRecommendationSummary(List<ThresholdRecommendation> recommendations, double expectedImprovement)
    {
        if (!recommendations.Any())
            return "No recommendations generated. Current performance appears optimal.";

        var highPriorityCount = recommendations.Count(r => r.IsHighPriority());
        return $"Generated {recommendations.Count} recommendations ({highPriorityCount} high priority). " +
               $"Expected F1 score improvement: {expectedImprovement:+0.000;-0.000;0}";
    }

    private DetectionAccuracyMetrics CreateEmptyAccuracyMetrics(Guid detectionLogicId, DateTime startDate, DateTime endDate)
    {
        return new DetectionAccuracyMetrics(
            detectionLogicId,
            startDate,
            endDate,
            0, 0, 0, 0, 0, 0, 0,
            new List<AccuracyByAnomalyType>(),
            new List<AccuracyByTimeRange>(),
            "No detection results found for accuracy calculation.");
    }

    private int EstimateFalseNegatives(List<AnomalyDetectionResult> detectionResults)
    {
        // より洗練された偽陰性推定
        // 異常レベル別の推定
        var criticalCount = detectionResults.Count(r => r.AnomalyLevel >= AnomalyLevel.Critical);
        var errorCount = detectionResults.Count(r => r.AnomalyLevel == AnomalyLevel.Error);
        var warningCount = detectionResults.Count(r => r.AnomalyLevel == AnomalyLevel.Warning);

        // レベル別の偽陰性率を適用
        var estimatedFalseNegatives =
            (int)(criticalCount * 0.02) +  // Critical: 2%の偽陰性率
            (int)(errorCount * 0.05) +     // Error: 5%の偽陰性率
            (int)(warningCount * 0.10);    // Warning: 10%の偽陰性率

        return Math.Max(1, estimatedFalseNegatives);
    }

    private int EstimateTrueNegatives(List<AnomalyDetectionResult> detectionResults)
    {
        // データ量に基づく真陰性の推定
        var analysisTimeSpan = detectionResults.Any() ?
            detectionResults.Max(r => r.DetectedAt) - detectionResults.Min(r => r.DetectedAt) :
            TimeSpan.Zero;

        if (analysisTimeSpan.TotalHours < 1)
            return detectionResults.Count * 5; // 短期間の場合

        // 時間あたりの正常データポイント数を推定
        var hoursAnalyzed = Math.Max(1, analysisTimeSpan.TotalHours);
        var normalDataPointsPerHour = 100; // 仮定値
        var totalNormalDataPoints = (int)(hoursAnalyzed * normalDataPointsPerHour);

        return Math.Max(detectionResults.Count, totalNormalDataPoints - detectionResults.Count);
    }

    private double CalculateMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private List<AccuracyByAnomalyType> CalculateAccuracyByAnomalyType(List<AnomalyDetectionResult> detectionResults)
    {
        return detectionResults
            .GroupBy(r => r.AnomalyType)
            .Select(g => new AccuracyByAnomalyType(
                g.Key,
                g.Count(r => !r.IsFalsePositive() && r.IsResolved()),
                g.Count(r => r.IsFalsePositive()),
                0)) // 簡略化
            .ToList();
    }

    private List<AccuracyByTimeRange> CalculateAccuracyByTimeRange(
        List<AnomalyDetectionResult> detectionResults,
        DateTime startDate,
        DateTime endDate)
    {
        var ranges = new List<AccuracyByTimeRange>();
        var timeSpan = endDate - startDate;
        var intervalHours = Math.Max(1, timeSpan.TotalHours / 10); // 10区間に分割

        for (var i = 0; i < 10; i++)
        {
            var rangeStart = startDate.AddHours(i * intervalHours);
            var rangeEnd = startDate.AddHours((i + 1) * intervalHours);

            var rangeResults = detectionResults
                .Where(r => r.DetectedAt >= rangeStart && r.DetectedAt < rangeEnd)
                .ToList();

            if (rangeResults.Any())
            {
                ranges.Add(new AccuracyByTimeRange(
                    rangeStart,
                    rangeEnd,
                    rangeResults.Count(r => !r.IsFalsePositive() && r.IsResolved()),
                    rangeResults.Count(r => r.IsFalsePositive()),
                    0)); // 簡略化
            }
        }

        return ranges;
    }

    private string GeneratePerformanceSummary(int truePositives, int falsePositives, int falseNegatives, int trueNegatives)
    {
        var total = truePositives + falsePositives + falseNegatives + trueNegatives;
        var accuracy = total > 0 ? (double)(truePositives + trueNegatives) / total : 0.0;
        var precision = truePositives + falsePositives > 0 ? (double)truePositives / (truePositives + falsePositives) : 0.0;
        var recall = truePositives + falseNegatives > 0 ? (double)truePositives / (truePositives + falseNegatives) : 0.0;

        return $"Performance Summary: Accuracy {accuracy:P2}, Precision {precision:P2}, Recall {recall:P2}. " +
               $"True Positives: {truePositives}, False Positives: {falsePositives}, " +
               $"False Negatives: {falseNegatives}, True Negatives: {trueNegatives}.";
    }


    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨
    /// </summary>
    public async Task<ThresholdRecommendationResult> GenerateAdvancedThresholdRecommendationsAsync(
        Guid detectionLogicId, DateTime analysisStartDate, DateTime analysisEndDate)
    {
        _logger.LogInformation("Generating ML-based threshold recommendations for {DetectionLogicId}", detectionLogicId);

        var detectionLogic = await _detectionLogicRepository.GetAsync(detectionLogicId);
        var detectionResults = await _anomalyResultRepository.GetListAsync(
            r => r.DetectionLogicId == detectionLogicId && r.DetectedAt >= analysisStartDate && r.DetectedAt <= analysisEndDate);

        if (!detectionResults.Any())
        {
            var empty = new OptimizationMetrics(0, 0, 0, 0, 0, 0, 0);
            return new ThresholdRecommendationResult(detectionLogicId, analysisStartDate, analysisEndDate,
                new List<ThresholdRecommendation>(), empty, empty, 0, "No data.");
        }

        var recommendations = new List<ThresholdRecommendation>();
        var actualValues = detectionResults.Select(r => r.InputData.SignalValue).ToList();

        if (actualValues.Count >= 100)
        {
            var config = new ThresholdOptimizationConfig { TargetFalsePositiveRate = 0.05, TargetTruePositiveRate = 0.95 };
            var optimalResult = await _thresholdOptimizer.CalculateOptimalThresholdAsync(actualValues, config);
            var currentMax = detectionLogic.Parameters.FirstOrDefault(p => p.Name == "MaxThreshold")?.Value ?? "N/A";
            recommendations.Add(new ThresholdRecommendation("Upper Threshold (Statistical)",
                currentMax,
                optimalResult.RecommendedUpperThreshold.ToString("F2"),
                $"Statistical: {optimalResult.RecommendedUpperThreshold:F2}, FPR: {optimalResult.ExpectedFalsePositiveRate:P2}", 0.9, optimalResult.ConfidenceLevel));
        }

        if (actualValues.Count >= 50)
        {
            var outlierResult = await _thresholdOptimizer.DetectOutliersAsync(actualValues, OutlierDetectionMethod.IQR);
            if (outlierResult.OutlierPercentage > 10)
            {
                recommendations.Add(new ThresholdRecommendation("Outlier Threshold", "Current",
                    $"IQR: [{outlierResult.LowerBound:F2}, {outlierResult.UpperBound:F2}]",
                    $"Outliers: {outlierResult.OutlierCount} ({outlierResult.OutlierPercentage:F1}%)", 0.8, 0.85));
            }
        }

        recommendations.AddRange(GenerateThresholdRecommendations(detectionLogic, detectionResults));
        var currentMetrics = CalculateOptimizationMetrics(detectionResults);
        var predictedMetrics = SimulatePredictedMetrics(currentMetrics, recommendations);
        var expectedImprovement = CalculateExpectedImprovement(currentMetrics, predictedMetrics);

        return new ThresholdRecommendationResult(detectionLogicId, analysisStartDate, analysisEndDate,
            recommendations.OrderByDescending(r => r.Priority).Take(10).ToList(),
            currentMetrics, predictedMetrics, expectedImprovement,
            $"ML analysis: {recommendations.Count} recommendations. Improvement: {expectedImprovement:P1}");
    }

    #endregion
}