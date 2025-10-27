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
    private readonly ILogger<AnomalyAnalysisService> _logger;

    public AnomalyAnalysisService(
        IRepository<AnomalyDetectionResult, Guid> anomalyResultRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        ILogger<AnomalyAnalysisService> logger)
    {
        _anomalyResultRepository = anomalyResultRepository;
        _detectionLogicRepository = detectionLogicRepository;
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

        // 時間別パターン分析（簡単な実装例）
        var hourlyDistribution = anomalyResults
            .GroupBy(r => r.DetectedAt.Hour)
            .Where(g => g.Count() > 1)
            .Select(g => new AnomalyFrequencyPattern(
                $"Hourly Pattern {g.Key}:00",
                TimeSpan.FromHours(1),
                g.Count(),
                Math.Min(0.9, g.Count() / (double)anomalyResults.Count * 10))) // 簡単な信頼度計算
            .ToList();

        patterns.AddRange(hourlyDistribution);

        return patterns;
    }

    private async Task<List<AnomalyCorrelation>> AnalyzeCorrelationsAsync(Guid canSignalId, List<AnomalyDetectionResult> anomalyResults)
    {
        // 実際の実装では他のCAN信号との相関を分析
        // ここでは簡単な例として空のリストを返す
        await Task.CompletedTask;
        return new List<AnomalyCorrelation>();
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
        var truePositives = detectionResults.Count(r => !r.IsFalsePositive() && r.IsResolved());
        var falsePositives = detectionResults.Count(r => r.IsFalsePositive());
        var totalDetections = detectionResults.Count;

        var precision = totalDetections > 0 ? (double)truePositives / totalDetections : 0.0;
        var recall = truePositives > 0 ? (double)truePositives / (truePositives + EstimateFalseNegatives(detectionResults)) : 0.0;
        var f1Score = precision + recall > 0 ? 2 * (precision * recall) / (precision + recall) : 0.0;

        var averageDetectionTime = detectionResults
            .Where(r => r.DetectionDuration.TotalMilliseconds > 0)
            .Average(r => r.DetectionDuration.TotalMilliseconds);

        return new OptimizationMetrics(
            (double)truePositives / totalDetections,
            (double)falsePositives / totalDetections,
            0.0, // 簡略化
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

        // 誤検出率が高い場合の推奨
        var falsePositiveRate = (double)detectionResults.Count(r => r.IsFalsePositive()) / detectionResults.Count;
        if (falsePositiveRate > 0.1)
        {
            recommendations.Add(new ThresholdRecommendation(
                "Threshold",
                "Current",
                "Recommended Higher",
                "High false positive rate detected. Consider increasing threshold to reduce false positives.",
                0.8,
                0.7));
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
        // 実際の実装では外部データソースから取得
        // ここでは簡単な推定値を返す
        return (int)(detectionResults.Count * 0.05); // 5%と仮定
    }

    private int EstimateTrueNegatives(List<AnomalyDetectionResult> detectionResults)
    {
        // 実際の実装では外部データソースから取得
        // ここでは簡単な推定値を返す
        return detectionResults.Count * 10; // 検出数の10倍と仮定
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

    #endregion
}