using System;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace AnomalyDetection.Controllers.AnomalyDetection;

/// <summary>
/// 異常検出分析 HTTP API Controller
/// </summary>
[RemoteService(Name = "Default")]
[Area("app")]
[Route("api/app/anomaly-analysis")]
public class AnomalyAnalysisController : AbpControllerBase
{
    private readonly IAnomalyAnalysisAppService _anomalyAnalysisAppService;

    public AnomalyAnalysisController(IAnomalyAnalysisAppService anomalyAnalysisAppService)
    {
        _anomalyAnalysisAppService = anomalyAnalysisAppService;
    }

    /// <summary>
    /// 異常パターン分析を実行する
    /// </summary>
    [HttpPost("pattern-analysis")]
    public virtual async Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(AnomalyAnalysisRequestDto request)
    {
        return await _anomalyAnalysisAppService.AnalyzeAnomalyPatternAsync(request);
    }

    /// <summary>
    /// 異常パターン分析を実行する（簡易版）
    /// </summary>
    [HttpGet("pattern-analysis")]
    public virtual async Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(
        [FromQuery] Guid canSignalId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return await _anomalyAnalysisAppService.AnalyzeAnomalyPatternAsync(canSignalId, startDate, endDate);
    }

    /// <summary>
    /// 閾値最適化推奨を取得する
    /// </summary>
    [HttpPost("threshold-recommendations")]
    public virtual async Task<ThresholdRecommendationResultDto> GetThresholdRecommendationsAsync(ThresholdRecommendationRequestDto request)
    {
        return await _anomalyAnalysisAppService.GetThresholdRecommendationsAsync(request);
    }

    /// <summary>
    /// 閾値最適化推奨を取得する（簡易版）
    /// </summary>
    [HttpGet("threshold-recommendations")]
    public virtual async Task<ThresholdRecommendationResultDto> GetThresholdRecommendationsAsync(
        [FromQuery] Guid detectionLogicId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return await _anomalyAnalysisAppService.GetThresholdRecommendationsAsync(detectionLogicId, startDate, endDate);
    }

    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨を取得する
    /// </summary>
    [HttpPost("advanced-threshold-recommendations")]
    public virtual async Task<ThresholdRecommendationResultDto> GetAdvancedThresholdRecommendationsAsync(ThresholdRecommendationRequestDto request)
    {
        return await _anomalyAnalysisAppService.GetAdvancedThresholdRecommendationsAsync(request);
    }

    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨を取得する（簡易版）
    /// </summary>
    [HttpGet("advanced-threshold-recommendations")]
    public virtual async Task<ThresholdRecommendationResultDto> GetAdvancedThresholdRecommendationsAsync(
        [FromQuery] Guid detectionLogicId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return await _anomalyAnalysisAppService.GetAdvancedThresholdRecommendationsAsync(detectionLogicId, startDate, endDate);
    }

    /// <summary>
    /// 検出精度評価メトリクスを取得する
    /// </summary>
    [HttpPost("detection-accuracy-metrics")]
    public virtual async Task<DetectionAccuracyMetricsDto> GetDetectionAccuracyMetricsAsync(DetectionAccuracyRequestDto request)
    {
        return await _anomalyAnalysisAppService.GetDetectionAccuracyMetricsAsync(request);
    }

    /// <summary>
    /// 検出精度評価メトリクスを取得する（簡易版）
    /// </summary>
    [HttpGet("detection-accuracy-metrics")]
    public virtual async Task<DetectionAccuracyMetricsDto> GetDetectionAccuracyMetricsAsync(
        [FromQuery] Guid detectionLogicId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return await _anomalyAnalysisAppService.GetDetectionAccuracyMetricsAsync(detectionLogicId, startDate, endDate);
    }
}
