using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 異常検出分析ドメインサービスのインターフェース
/// </summary>
public interface IAnomalyAnalysisService : IDomainService
{
    /// <summary>
    /// 異常パターンを分析する
    /// </summary>
    /// <param name="canSignalId">CAN信号ID</param>
    /// <param name="analysisStartDate">分析開始日</param>
    /// <param name="analysisEndDate">分析終了日</param>
    /// <returns>異常パターン分析結果</returns>
    Task<AnomalyPatternAnalysisResult> AnalyzePatternAsync(
        Guid canSignalId, 
        DateTime analysisStartDate, 
        DateTime analysisEndDate);

    /// <summary>
    /// 閾値最適化推奨を生成する
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID</param>
    /// <param name="analysisStartDate">分析開始日</param>
    /// <param name="analysisEndDate">分析終了日</param>
    /// <returns>閾値最適化推奨結果</returns>
    Task<ThresholdRecommendationResult> GenerateThresholdRecommendationsAsync(
        Guid detectionLogicId, 
        DateTime analysisStartDate, 
        DateTime analysisEndDate);

    /// <summary>
    /// 検出精度を評価する
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID</param>
    /// <param name="analysisStartDate">分析開始日</param>
    /// <param name="analysisEndDate">分析終了日</param>
    /// <returns>検出精度評価結果</returns>
    Task<DetectionAccuracyMetrics> CalculateDetectionAccuracyAsync(
        Guid detectionLogicId, 
        DateTime analysisStartDate, 
        DateTime analysisEndDate);
}