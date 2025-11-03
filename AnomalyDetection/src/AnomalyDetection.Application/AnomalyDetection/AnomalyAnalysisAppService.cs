using System;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.AnomalyDetection.Services;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 異常検出分析アプリケーションサービス
/// </summary>
[Authorize(AnomalyDetectionPermissions.Analysis.Default)]
public class AnomalyAnalysisAppService : ApplicationService, IAnomalyAnalysisAppService
{
    private readonly IAnomalyAnalysisService _anomalyAnalysisService;
    private readonly ILogger<AnomalyAnalysisAppService> _logger;

    public AnomalyAnalysisAppService(
        IAnomalyAnalysisService anomalyAnalysisService,
        ILogger<AnomalyAnalysisAppService> logger)
    {
        _anomalyAnalysisService = anomalyAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// 異常パターンを分析する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.AnalyzePatterns)]
    public async Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(AnomalyAnalysisRequestDto request)
    {
        _logger.LogInformation("Starting anomaly pattern analysis for CAN signal {CanSignalId}", request.CanSignalId);

        var result = await _anomalyAnalysisService.AnalyzePatternAsync(
            request.CanSignalId,
            request.AnalysisStartDate,
            request.AnalysisEndDate);

        var dto = ObjectMapper.Map<AnomalyPatternAnalysisResult, AnomalyPatternAnalysisDto>(result);

        _logger.LogInformation("Completed anomaly pattern analysis for CAN signal {CanSignalId}. Found {TotalAnomalies} anomalies",
            request.CanSignalId, dto.TotalAnomalies);

        return dto;
    }

    /// <summary>
    /// 閾値最適化推奨を取得する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.GenerateRecommendations)]
    public async Task<ThresholdRecommendationResultDto> GetThresholdRecommendationsAsync(ThresholdRecommendationRequestDto request)
    {
        _logger.LogInformation("Generating threshold recommendations for detection logic {DetectionLogicId}", request.DetectionLogicId);

        var result = await _anomalyAnalysisService.GenerateThresholdRecommendationsAsync(
            request.DetectionLogicId,
            request.AnalysisStartDate,
            request.AnalysisEndDate);

        var dto = ObjectMapper.Map<ThresholdRecommendationResult, ThresholdRecommendationResultDto>(result);

        _logger.LogInformation("Generated {RecommendationCount} threshold recommendations for detection logic {DetectionLogicId}",
            dto.Recommendations.Count, request.DetectionLogicId);

        return dto;
    }

    /// <summary>
    /// 検出精度評価メトリクスを取得する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.ViewMetrics)]
    public async Task<DetectionAccuracyMetricsDto> GetDetectionAccuracyMetricsAsync(DetectionAccuracyRequestDto request)
    {
        _logger.LogInformation("Calculating detection accuracy metrics for logic {DetectionLogicId}", request.DetectionLogicId);

        var result = await _anomalyAnalysisService.CalculateDetectionAccuracyAsync(
            request.DetectionLogicId,
            request.AnalysisStartDate,
            request.AnalysisEndDate);

        var dto = ObjectMapper.Map<DetectionAccuracyMetrics, DetectionAccuracyMetricsDto>(result);

        _logger.LogInformation("Calculated detection accuracy metrics for logic {DetectionLogicId}. F1 Score: {F1Score:F3}",
            request.DetectionLogicId, dto.F1Score);

        return dto;
    }

    /// <summary>
    /// CAN信号の異常パターン分析を実行する（簡易版）
    /// </summary>
    public async Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(Guid canSignalId, DateTime startDate, DateTime endDate)
    {
        var request = new AnomalyAnalysisRequestDto
        {
            CanSignalId = canSignalId,
            AnalysisStartDate = startDate,
            AnalysisEndDate = endDate
        };

        return await AnalyzeAnomalyPatternAsync(request);
    }

    /// <summary>
    /// 検出ロジックの閾値最適化推奨を取得する（簡易版）
    /// </summary>
    public async Task<ThresholdRecommendationResultDto> GetThresholdRecommendationsAsync(Guid detectionLogicId, DateTime startDate, DateTime endDate)
    {
        var request = new ThresholdRecommendationRequestDto
        {
            DetectionLogicId = detectionLogicId,
            AnalysisStartDate = startDate,
            AnalysisEndDate = endDate
        };

        return await GetThresholdRecommendationsAsync(request);
    }

    /// <summary>
    /// 検出ロジックの精度評価メトリクスを取得する（簡易版）
    /// </summary>
    public async Task<DetectionAccuracyMetricsDto> GetDetectionAccuracyMetricsAsync(Guid detectionLogicId, DateTime startDate, DateTime endDate)
    {
        var request = new DetectionAccuracyRequestDto
        {
            DetectionLogicId = detectionLogicId,
            AnalysisStartDate = startDate,
            AnalysisEndDate = endDate
        };

        return await GetDetectionAccuracyMetricsAsync(request);
    }

    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨を取得する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.GenerateRecommendations)]
    public async Task<ThresholdRecommendationResultDto> GetAdvancedThresholdRecommendationsAsync(ThresholdRecommendationRequestDto request)
    {
        _logger.LogInformation("Generating ML-based advanced threshold recommendations for detection logic {DetectionLogicId}", request.DetectionLogicId);

        var result = await _anomalyAnalysisService.GenerateAdvancedThresholdRecommendationsAsync(
            request.DetectionLogicId,
            request.AnalysisStartDate,
            request.AnalysisEndDate);

        var dto = ObjectMapper.Map<ThresholdRecommendationResult, ThresholdRecommendationResultDto>(result);

        _logger.LogInformation("Generated {RecommendationCount} ML-based advanced threshold recommendations for detection logic {DetectionLogicId}",
            dto.Recommendations.Count, request.DetectionLogicId);

        return dto;
    }

    /// <summary>
    /// ML-based統計的最適化による高度な閾値推奨を取得する（簡易版）
    /// </summary>
    public async Task<ThresholdRecommendationResultDto> GetAdvancedThresholdRecommendationsAsync(Guid detectionLogicId, DateTime startDate, DateTime endDate)
    {
        var request = new ThresholdRecommendationRequestDto
        {
            DetectionLogicId = detectionLogicId,
            AnalysisStartDate = startDate,
            AnalysisEndDate = endDate
        };

        return await GetAdvancedThresholdRecommendationsAsync(request);
    }
}