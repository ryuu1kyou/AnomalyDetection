using System;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 異常検出分析アプリケーションサービスのインターフェース
/// </summary>
public interface IAnomalyAnalysisAppService : IApplicationService
{
    /// <summary>
    /// 異常パターンを分析する
    /// </summary>
    /// <param name="request">分析リクエスト</param>
    /// <returns>異常パターン分析結果</returns>
    Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(AnomalyAnalysisRequestDto request);

    /// <summary>
    /// 閾値最適化推奨を取得する
    /// </summary>
    /// <param name="request">推奨リクエスト</param>
    /// <returns>閾値最適化推奨結果</returns>
    Task<ThresholdRecommendationResultDto> GetThresholdRecommendationsAsync(ThresholdRecommendationRequestDto request);

    /// <summary>
    /// 検出精度評価メトリクスを取得する
    /// </summary>
    /// <param name="request">精度評価リクエスト</param>
    /// <returns>検出精度評価メトリクス</returns>
    Task<DetectionAccuracyMetricsDto> GetDetectionAccuracyMetricsAsync(DetectionAccuracyRequestDto request);

    /// <summary>
    /// CAN信号の異常パターン分析を実行する（簡易版）
    /// </summary>
    /// <param name="canSignalId">CAN信号ID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <returns>異常パターン分析結果</returns>
    Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(Guid canSignalId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// 検出ロジックの閾値最適化推奨を取得する（簡易版）
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <returns>閾値最適化推奨結果</returns>
    Task<ThresholdRecommendationResultDto> GetThresholdRecommendationsAsync(Guid detectionLogicId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// 検出ロジックの精度評価メトリクスを取得する（簡易版）
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <returns>検出精度評価メトリクス</returns>
    Task<DetectionAccuracyMetricsDto> GetDetectionAccuracyMetricsAsync(Guid detectionLogicId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨を取得する
    /// </summary>
    /// <param name="request">推奨リクエスト</param>
    /// <returns>ML-based閾値最適化推奨結果</returns>
    Task<ThresholdRecommendationResultDto> GetAdvancedThresholdRecommendationsAsync(ThresholdRecommendationRequestDto request);

    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨を取得する（簡易版）
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <returns>ML-based閾値最適化推奨結果</returns>
    Task<ThresholdRecommendationResultDto> GetAdvancedThresholdRecommendationsAsync(Guid detectionLogicId, DateTime startDate, DateTime endDate);
}