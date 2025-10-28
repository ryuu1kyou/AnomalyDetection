using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.SimilarPatternSearch.Dtos;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 類似パターン検索アプリケーションサービスの実装
/// </summary>
[Authorize(AnomalyDetectionPermissions.Analysis.Default)]
public class SimilarPatternSearchAppService : ApplicationService, ISimilarPatternSearchAppService
{
    private readonly ISimilarPatternSearchService _similarPatternSearchService;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly IRepository<AnomalyDetectionResult, Guid> _anomalyDetectionResultRepository;

    public SimilarPatternSearchAppService(
        ISimilarPatternSearchService similarPatternSearchService,
        IRepository<CanSignal, Guid> canSignalRepository,
        IRepository<AnomalyDetectionResult, Guid> anomalyDetectionResultRepository)
    {
        _similarPatternSearchService = similarPatternSearchService;
        _canSignalRepository = canSignalRepository;
        _anomalyDetectionResultRepository = anomalyDetectionResultRepository;
    }

    /// <summary>
    /// 類似CAN信号を検索する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.SearchSimilarSignals)]
    public async Task<List<SimilarSignalResultDto>> SearchSimilarSignalsAsync(SimilarSignalSearchRequestDto request)
    {
        // 対象信号を取得
        var targetSignal = await _canSignalRepository.GetAsync(request.TargetSignalId);
        
        // 候補信号を最適化されたクエリで取得
        IEnumerable<CanSignal> candidateSignals;
        if (request.CandidateSignalIds?.Any() == true)
        {
            var queryable = await _canSignalRepository.GetQueryableAsync();
            candidateSignals = await queryable
                .Where(s => request.CandidateSignalIds.Contains(s.Id))
                .ToListAsync();
        }
        else
        {
            // アクティブな信号のみを効率的に取得
            var queryable = await _canSignalRepository.GetQueryableAsync();
            candidateSignals = await queryable
                .Where(s => s.Status == SignalStatus.Active)
                .OrderBy(s => s.SystemType)
                .ThenBy(s => s.Identifier.SignalName)
                .Take(1000) // 大量データ対策として制限
                .ToListAsync();
        }

        // 検索条件をドメインオブジェクトに変換
        var criteria = MapToDomainCriteria(request.Criteria);

        // 類似信号検索を実行
        var searchResults = await _similarPatternSearchService.SearchSimilarSignalsAsync(
            targetSignal, criteria, candidateSignals);

        // DTOに変換
        var resultDtos = new List<SimilarSignalResultDto>();
        foreach (var result in searchResults)
        {
            var signalInfo = await GetSignalInfoAsync(result.SignalId);
            var dto = MapToSimilarSignalResultDto(result, signalInfo);
            resultDtos.Add(dto);
        }

        return resultDtos;
    }

    /// <summary>
    /// 検査データを比較する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.CompareTestData)]
    public async Task<TestDataComparisonDto> CompareTestDataAsync(TestDataComparisonRequestDto request)
    {
        // 信号を取得
        var sourceSignal = await _canSignalRepository.GetAsync(request.SourceSignalId);
        var targetSignal = await _canSignalRepository.GetAsync(request.TargetSignalId);

        // 検出結果を取得
        var sourceResults = await GetAnomalyDetectionResultsAsync(
            request.SourceSignalId, request.StartDate, request.EndDate, 
            request.AnomalyLevelFilter, request.MaxResults);
            
        var targetResults = await GetAnomalyDetectionResultsAsync(
            request.TargetSignalId, request.StartDate, request.EndDate, 
            request.AnomalyLevelFilter, request.MaxResults);

        // 比較を実行
        var comparison = await _similarPatternSearchService.CompareTestDataAsync(
            sourceResults, targetResults, sourceSignal, targetSignal);

        // DTOに変換
        return MapToTestDataComparisonDto(comparison);
    }

    /// <summary>
    /// 類似信号の推奨事項を取得する
    /// </summary>
    public async Task<List<SimilarSignalRecommendationDto>> GetSimilarSignalRecommendationsAsync(
        SimilarSignalRecommendationRequestDto request)
    {
        // 対象信号を取得
        var targetSignal = await _canSignalRepository.GetAsync(request.SignalId);
        
        // デフォルトの検索条件で類似信号を検索
        var criteria = SimilaritySearchCriteria.CreateDefault()
            .WithMaxResults(request.MaxRecommendations * 2); // 多めに取得してフィルタリング

        var candidateSignals = await _canSignalRepository.GetListAsync();
        var searchResults = await _similarPatternSearchService.SearchSimilarSignalsAsync(
            targetSignal, criteria, candidateSignals);

        // 推奨事項を生成
        var recommendations = new List<SimilarSignalRecommendationDto>();
        foreach (var result in searchResults.Take(request.MaxRecommendations))
        {
            var signalInfo = await GetSignalInfoAsync(result.SignalId);
            var recommendation = new SimilarSignalRecommendationDto
            {
                RecommendedSignalId = result.SignalId,
                RecommendationScore = result.SimilarityScore,
                Reason = result.RecommendationReason,
                RecommendedSettings = GenerateRecommendedSettings(result, request.RecommendationType),
                SignalInfo = signalInfo
            };
            recommendations.Add(recommendation);
        }

        return recommendations;
    }

    /// <summary>
    /// 比較結果をエクスポートする
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.ExportAnalysisData)]
    public async Task<byte[]> ExportComparisonResultAsync(ComparisonExportRequestDto request)
    {
        // 実装は後で追加（CSV、Excel、PDF、JSON形式のエクスポート）
        await Task.CompletedTask;
        throw new NotImplementedException("Export functionality will be implemented in a future version");
    }

    /// <summary>
    /// 過去の検査データ一覧を取得する
    /// </summary>
    public async Task<List<HistoricalTestDataDto>> GetHistoricalTestDataAsync(
        Guid signalId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxResults = 1000)
    {
        var results = await GetAnomalyDetectionResultsAsync(
            signalId, startDate, endDate, null, maxResults);

        return results.Select(MapToHistoricalTestDataDto).ToList();
    }

    /// <summary>
    /// 類似度計算の詳細を取得する
    /// </summary>
    [Authorize(AnomalyDetectionPermissions.Analysis.CalculateSimilarity)]
    public async Task<SimilarityCalculationDetailDto> GetSimilarityCalculationDetailAsync(
        Guid signal1Id,
        Guid signal2Id,
        SimilaritySearchCriteriaDto criteria)
    {
        var signal1 = await _canSignalRepository.GetAsync(signal1Id);
        var signal2 = await _canSignalRepository.GetAsync(signal2Id);
        var domainCriteria = MapToDomainCriteria(criteria);

        var startTime = DateTime.UtcNow;
        var breakdown = _similarPatternSearchService.CalculateSimilarityBreakdown(signal1, signal2, domainCriteria);
        var calculationTime = DateTime.UtcNow - startTime;

        return new SimilarityCalculationDetailDto
        {
            Signal1Id = signal1Id,
            Signal2Id = signal2Id,
            OverallSimilarityScore = breakdown.CalculateWeightedSimilarity(),
            Breakdown = MapToSimilarityBreakdownDto(breakdown),
            UsedCriteria = criteria,
            DetailedComparisons = GenerateDetailedComparisons(signal1, signal2, breakdown),
            CalculationTimeMs = calculationTime.TotalMilliseconds,
            CalculatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 類似パターン検索の統計情報を取得する
    /// </summary>
    public async Task<SimilarPatternSearchStatisticsDto> GetSearchStatisticsAsync(
        Guid signalId,
        int period = 30)
    {
        // 実装は後で追加（検索履歴の統計分析）
        await Task.CompletedTask;
        
        return new SimilarPatternSearchStatisticsDto
        {
            SignalId = signalId,
            PeriodDays = period,
            StartDate = DateTime.UtcNow.AddDays(-period),
            EndDate = DateTime.UtcNow,
            TotalSearchCount = 0,
            SimilarSignalsFoundCount = 0,
            AverageSimilarityScore = 0.0,
            MaxSimilarityScore = 0.0,
            RecommendationLevelCounts = new Dictionary<Dtos.RecommendationLevel, int>(),
            SystemTypeCounts = new Dictionary<string, int>(),
            CriteriaUsageStatistics = new SearchCriteriaUsageStatisticsDto()
        };
    }

    #region Private Helper Methods

    private async Task<List<AnomalyDetectionResult>> GetAnomalyDetectionResultsAsync(
        Guid signalId,
        DateTime? startDate,
        DateTime? endDate,
        List<AnomalyLevel>? anomalyLevelFilter,
        int maxResults)
    {
        var queryable = await _anomalyDetectionResultRepository.GetQueryableAsync();
        
        var query = queryable.Where(r => r.CanSignalId == signalId);

        if (startDate.HasValue)
            query = query.Where(r => r.DetectedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.DetectedAt <= endDate.Value);

        if (anomalyLevelFilter?.Any() == true)
            query = query.Where(r => anomalyLevelFilter.Contains(r.AnomalyLevel));

        // Use async execution for better performance
        return await query.OrderByDescending(r => r.DetectedAt)
                   .Take(maxResults)
                   .ToListAsync();
    }

    private async Task<SimilarSignalInfoDto> GetSignalInfoAsync(Guid signalId)
    {
        var signal = await _canSignalRepository.GetAsync(signalId);
        
        return new SimilarSignalInfoDto
        {
            SignalName = signal.Identifier.SignalName,
            CanId = signal.Identifier.CanId,
            SystemType = signal.SystemType.ToString(),
            OemCode = signal.OemCode.ToString(),
            Description = signal.Description ?? string.Empty,
            IsStandard = signal.IsStandard,
            Status = signal.Status.ToString()
        };
    }

    private static SimilaritySearchCriteria MapToDomainCriteria(SimilaritySearchCriteriaDto dto)
    {
        return new SimilaritySearchCriteria(
            dto.CompareCanId,
            dto.CompareSignalName,
            dto.CompareSystemType,
            dto.MinimumSimilarity,
            dto.MaxResults,
            dto.CompareValueRange,
            dto.CompareDataLength,
            dto.CompareCycle,
            dto.CompareOemCode,
            dto.StandardSignalsOnly,
            dto.ActiveSignalsOnly);
    }

    private static SimilarSignalResultDto MapToSimilarSignalResultDto(
        SimilarSignalResult result, 
        SimilarSignalInfoDto signalInfo)
    {
        return new SimilarSignalResultDto
        {
            SignalId = result.SignalId,
            SimilarityScore = result.SimilarityScore,
            Breakdown = MapToSimilarityBreakdownDto(result.Breakdown),
            MatchedAttributes = result.MatchedAttributes.ToList(),
            Differences = result.Differences.Select(MapToAttributeDifferenceDto).ToList(),
            RecommendationLevel = MapToRecommendationLevelDto(result.RecommendationLevel),
            RecommendationReason = result.RecommendationReason,
            SignalInfo = signalInfo
        };
    }

    private static SimilarityBreakdownDto MapToSimilarityBreakdownDto(SimilarityBreakdown breakdown)
    {
        return new SimilarityBreakdownDto
        {
            CanIdSimilarity = breakdown.CanIdSimilarity,
            SignalNameSimilarity = breakdown.SignalNameSimilarity,
            SystemTypeSimilarity = breakdown.SystemTypeSimilarity,
            ValueRangeSimilarity = breakdown.ValueRangeSimilarity,
            DataLengthSimilarity = breakdown.DataLengthSimilarity,
            CycleSimilarity = breakdown.CycleSimilarity,
            OemCodeSimilarity = breakdown.OemCodeSimilarity
        };
    }

    private static AttributeDifferenceDto MapToAttributeDifferenceDto(AttributeDifference difference)
    {
        return new AttributeDifferenceDto
        {
            AttributeName = difference.AttributeName,
            SourceValue = difference.SourceValue,
            TargetValue = difference.TargetValue,
            IsSignificant = difference.IsSignificant,
            Description = difference.Description
        };
    }

    private static TestDataComparisonDto MapToTestDataComparisonDto(TestDataComparison comparison)
    {
        return new TestDataComparisonDto
        {
            SourceSignalId = comparison.SourceSignalId,
            TargetSignalId = comparison.TargetSignalId,
            ComparedAt = comparison.ComparedAt,
            OverallSimilarityScore = comparison.OverallSimilarityScore,
            Summary = comparison.Summary,
            ThresholdDifferences = comparison.ThresholdDifferences.Select(MapToThresholdDifferenceDto).ToList(),
            DetectionConditionDifferences = comparison.DetectionConditionDifferences.Select(MapToDetectionConditionDifferenceDto).ToList(),
            ResultDifferences = comparison.ResultDifferences.Select(MapToResultDifferenceDto).ToList(),
            Recommendations = comparison.Recommendations.Select(MapToComparisonRecommendationDto).ToList(),
            Statistics = new ComparisonStatisticsDto
            {
                SourceResultCount = 0, // Will be populated from actual data
                TargetResultCount = 0,
                ThresholdDifferenceCount = comparison.GetThresholdDifferenceCount(),
                SignificantThresholdDifferenceCount = comparison.GetSignificantThresholdDifferenceCount(),
                DetectionConditionDifferenceCount = comparison.GetDetectionConditionDifferenceCount(),
                ResultDifferenceCount = comparison.GetResultDifferenceCount(),
                RecommendationCount = comparison.Recommendations.Count,
                HighPriorityRecommendationCount = comparison.GetRecommendationsByPriority(RecommendationPriority.High).Count()
            }
        };
    }

    private static ThresholdDifferenceDto MapToThresholdDifferenceDto(ThresholdDifference difference)
    {
        return new ThresholdDifferenceDto
        {
            ParameterName = difference.ParameterName,
            SourceValue = difference.SourceValue,
            TargetValue = difference.TargetValue,
            AbsoluteDifference = difference.AbsoluteDifference,
            RelativeDifference = difference.RelativeDifference,
            IsSignificant = difference.IsSignificant,
            Description = difference.Description
        };
    }

    private static DetectionConditionDifferenceDto MapToDetectionConditionDifferenceDto(DetectionConditionDifference difference)
    {
        return new DetectionConditionDifferenceDto
        {
            ConditionName = difference.ConditionName,
            SourceCondition = difference.SourceCondition,
            TargetCondition = difference.TargetCondition,
            DifferenceType = difference.DifferenceType,
            IsSignificant = difference.IsSignificant,
            Description = difference.Description
        };
    }

    private static ResultDifferenceDto MapToResultDifferenceDto(ResultDifference difference)
    {
        return new ResultDifferenceDto
        {
            ResultItem = difference.ResultItem,
            SourceValue = difference.SourceValue,
            TargetValue = difference.TargetValue,
            DifferenceType = difference.DifferenceType,
            IsSignificant = difference.IsSignificant,
            ImpactLevel = difference.ImpactLevel
        };
    }

    private static ComparisonRecommendationDto MapToComparisonRecommendationDto(ComparisonRecommendation recommendation)
    {
        return new ComparisonRecommendationDto
        {
            Type = recommendation.Type,
            Priority = recommendation.Priority,
            Content = recommendation.Content,
            Rationale = recommendation.Rationale,
            RecommendedValue = recommendation.RecommendedValue
        };
    }

    private static HistoricalTestDataDto MapToHistoricalTestDataDto(AnomalyDetectionResult result)
    {
        return new HistoricalTestDataDto
        {
            ResultId = result.Id,
            SignalId = result.CanSignalId,
            DetectedAt = result.DetectedAt,
            AnomalyLevel = result.AnomalyLevel,
            ConfidenceScore = result.ConfidenceScore,
            AnomalyType = result.AnomalyType,
            DetectionCondition = result.DetectionCondition,
            InputData = new TestInputDataDto
            {
                SignalValue = result.InputData.SignalValue,
                Timestamp = result.InputData.Timestamp,
                AdditionalData = result.InputData.AdditionalData
            },
            Details = new TestDetectionDetailsDto
            {
                DetectionType = result.Details.DetectionType,
                TriggerCondition = result.Details.TriggerCondition,
                Parameters = result.Details.Parameters,
                ExecutionTimeMs = result.Details.ExecutionTimeMs
            },
            ResolutionStatus = result.ResolutionStatus,
            IsValidated = result.IsValidated,
            IsFalsePositive = result.IsFalsePositiveFlag
        };
    }

    private static Dictionary<string, object> GenerateRecommendedSettings(
        SimilarSignalResult result, 
        RecommendationType recommendationType)
    {
        var settings = new Dictionary<string, object>();
        
        // 推奨タイプに基づいて設定を生成
        switch (recommendationType)
        {
            case RecommendationType.ThresholdAdjustment:
                settings["SimilarityThreshold"] = result.SimilarityScore;
                settings["RecommendationLevel"] = result.RecommendationLevel.ToString();
                break;
            case RecommendationType.ParameterOptimization:
                settings["OptimizationScore"] = result.SimilarityScore;
                break;
            default:
                settings["GeneralRecommendation"] = result.RecommendationReason;
                break;
        }
        
        return settings;
    }

    private static List<DetailedComparisonItemDto> GenerateDetailedComparisons(
        CanSignal signal1, 
        CanSignal signal2, 
        SimilarityBreakdown breakdown)
    {
        var items = new List<DetailedComparisonItemDto>();

        // CAN ID比較
        items.Add(new DetailedComparisonItemDto
        {
            ItemName = "CAN ID",
            Signal1Value = signal1.Identifier.CanId,
            Signal2Value = signal2.Identifier.CanId,
            SimilarityScore = breakdown.CanIdSimilarity,
            Weight = breakdown.Weights.CanIdWeight,
            WeightedScore = breakdown.CanIdSimilarity * breakdown.Weights.CanIdWeight,
            ComparisonMethod = "Exact Match",
            Notes = signal1.Identifier.CanId == signal2.Identifier.CanId ? "Identical" : "Different"
        });

        // 信号名比較
        items.Add(new DetailedComparisonItemDto
        {
            ItemName = "Signal Name",
            Signal1Value = signal1.Identifier.SignalName,
            Signal2Value = signal2.Identifier.SignalName,
            SimilarityScore = breakdown.SignalNameSimilarity,
            Weight = breakdown.Weights.SignalNameWeight,
            WeightedScore = breakdown.SignalNameSimilarity * breakdown.Weights.SignalNameWeight,
            ComparisonMethod = "Levenshtein Distance",
            Notes = $"String similarity: {breakdown.SignalNameSimilarity:P1}"
        });

        // システム種別比較
        items.Add(new DetailedComparisonItemDto
        {
            ItemName = "System Type",
            Signal1Value = signal1.SystemType.ToString(),
            Signal2Value = signal2.SystemType.ToString(),
            SimilarityScore = breakdown.SystemTypeSimilarity,
            Weight = breakdown.Weights.SystemTypeWeight,
            WeightedScore = breakdown.SystemTypeSimilarity * breakdown.Weights.SystemTypeWeight,
            ComparisonMethod = "Exact Match",
            Notes = signal1.SystemType == signal2.SystemType ? "Same system type" : "Different system type"
        });

        return items;
    }

    private static Dtos.RecommendationLevel MapToRecommendationLevelDto(RecommendationLevel domainLevel)
    {
        return domainLevel switch
        {
            RecommendationLevel.NotRecommended => Dtos.RecommendationLevel.NotRecommended,
            RecommendationLevel.Low => Dtos.RecommendationLevel.Low,
            RecommendationLevel.Medium => Dtos.RecommendationLevel.Medium,
            RecommendationLevel.High => Dtos.RecommendationLevel.High,
            RecommendationLevel.Highly => Dtos.RecommendationLevel.Highly,
            _ => Dtos.RecommendationLevel.Medium
        };
    }

    #endregion
}