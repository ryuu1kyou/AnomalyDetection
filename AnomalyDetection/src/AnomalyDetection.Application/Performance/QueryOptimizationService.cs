using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.OemTraceability;
using AnomalyDetection.Projects;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.Performance;

/// <summary>
/// クエリ最適化サービス - N+1問題の解消とページネーション実装
/// </summary>
public class QueryOptimizationService : ApplicationService
{
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;
    private readonly IRepository<AnomalyDetectionResult, Guid> _detectionResultRepository;
    private readonly IRepository<AnomalyDetectionProject, Guid> _projectRepository;
    private readonly IRepository<OemCustomization, Guid> _customizationRepository;

    public QueryOptimizationService(
        IRepository<CanSignal, Guid> canSignalRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IRepository<AnomalyDetectionResult, Guid> detectionResultRepository,
        IRepository<AnomalyDetectionProject, Guid> projectRepository,
        IRepository<OemCustomization, Guid> customizationRepository)
    {
        _canSignalRepository = canSignalRepository;
        _detectionLogicRepository = detectionLogicRepository;
        _detectionResultRepository = detectionResultRepository;
        _projectRepository = projectRepository;
        _customizationRepository = customizationRepository;
    }

    /// <summary>
    /// CAN信号の最適化されたページネーション取得
    /// </summary>
    public async Task<PagedResultDto<CanSignal>> GetCanSignalsOptimizedAsync(
        int skipCount = 0,
        int maxResultCount = 10,
        string? filter = null,
        CanSystemType? systemType = null,
        SignalStatus? status = null,
        bool? isStandard = null)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();

        // フィルタリング
        if (!string.IsNullOrEmpty(filter))
        {
            queryable = queryable.Where(s =>
                EF.Functions.Like(s.Identifier.SignalName, $"%{filter}%") ||
                EF.Functions.Like(s.Description, $"%{filter}%"));
        }

        if (systemType.HasValue)
        {
            queryable = queryable.Where(s => s.SystemType == systemType.Value);
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(s => s.Status == status.Value);
        }

        if (isStandard.HasValue)
        {
            queryable = queryable.Where(s => s.IsStandard == isStandard.Value);
        }

        // 総件数を取得（フィルタ適用後）
        var totalCount = await queryable.CountAsync();

        // ページネーション適用とソート
        var items = await queryable
            .OrderBy(s => s.SystemType)
            .ThenBy(s => s.Identifier.SignalName)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<CanSignal>(totalCount, items);
    }

    /// <summary>
    /// 検出ロジックの最適化されたページネーション取得（子エンティティを含む）
    /// </summary>
    public async Task<PagedResultDto<CanAnomalyDetectionLogic>> GetDetectionLogicsOptimizedAsync(
        int skipCount = 0,
        int maxResultCount = 10,
        string? filter = null,
        DetectionLogicStatus? status = null,
        SharingLevel? sharingLevel = null,
        bool includeParameters = false,
        bool includeSignalMappings = false)
    {
        var queryable = await _detectionLogicRepository.GetQueryableAsync();

        // Eager Loading - N+1問題の解消
        if (includeParameters)
        {
            queryable = queryable.Include("Parameters");
        }

        if (includeSignalMappings)
        {
            queryable = queryable.Include("SignalMappings");
        }

        // フィルタリング
        if (!string.IsNullOrEmpty(filter))
        {
            queryable = queryable.Where(l =>
                EF.Functions.Like(l.Identity.Name, $"%{filter}%") ||
                EF.Functions.Like(l.Specification.Description, $"%{filter}%"));
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(l => l.Status == status.Value);
        }

        if (sharingLevel.HasValue)
        {
            queryable = queryable.Where(l => l.SharingLevel == sharingLevel.Value);
        }

        // 総件数を取得
        var totalCount = await queryable.CountAsync();

        // ページネーション適用
        var items = await queryable
            .OrderByDescending(l => l.LastModificationTime ?? l.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<CanAnomalyDetectionLogic>(totalCount, items);
    }

    /// <summary>
    /// 異常検出結果の最適化されたページネーション取得
    /// </summary>
    public async Task<PagedResultDto<AnomalyDetectionResult>> GetDetectionResultsOptimizedAsync(
        int skipCount = 0,
        int maxResultCount = 10,
        DateTime? startDate = null,
        DateTime? endDate = null,
        AnomalyLevel? anomalyLevel = null,
        ResolutionStatus? resolutionStatus = null,
        Guid? canSignalId = null,
        Guid? detectionLogicId = null)
    {
        var queryable = await _detectionResultRepository.GetQueryableAsync();

        // フィルタリング
        if (startDate.HasValue)
        {
            queryable = queryable.Where(r => r.DetectedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            queryable = queryable.Where(r => r.DetectedAt <= endDate.Value);
        }

        if (anomalyLevel.HasValue)
        {
            queryable = queryable.Where(r => r.AnomalyLevel == anomalyLevel.Value);
        }

        if (resolutionStatus.HasValue)
        {
            queryable = queryable.Where(r => r.ResolutionStatus == resolutionStatus.Value);
        }

        if (canSignalId.HasValue)
        {
            queryable = queryable.Where(r => r.CanSignalId == canSignalId.Value);
        }

        if (detectionLogicId.HasValue)
        {
            queryable = queryable.Where(r => r.DetectionLogicId == detectionLogicId.Value);
        }

        // 総件数を取得
        var totalCount = await queryable.CountAsync();

        // ページネーション適用
        var items = await queryable
            .OrderByDescending(r => r.DetectedAt)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<AnomalyDetectionResult>(totalCount, items);
    }

    /// <summary>
    /// プロジェクトの最適化されたページネーション取得（子エンティティを含む）
    /// </summary>
    public async Task<PagedResultDto<AnomalyDetectionProject>> GetProjectsOptimizedAsync(
        int skipCount = 0,
        int maxResultCount = 10,
        string? filter = null,
        ProjectStatus? status = null,
        bool includeMilestones = false,
        bool includeMembers = false)
    {
        var queryable = await _projectRepository.GetQueryableAsync();

        // Eager Loading
        if (includeMilestones)
        {
            queryable = queryable.Include("Milestones");
        }

        if (includeMembers)
        {
            queryable = queryable.Include("Members");
        }

        // フィルタリング
        if (!string.IsNullOrEmpty(filter))
        {
            queryable = queryable.Where(p =>
                EF.Functions.Like(p.Name, $"%{filter}%") ||
                EF.Functions.Like(p.ProjectCode, $"%{filter}%") ||
                EF.Functions.Like(p.VehicleModel, $"%{filter}%"));
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(p => p.Status == status.Value);
        }

        // 総件数を取得
        var totalCount = await queryable.CountAsync();

        // ページネーション適用
        var items = await queryable
            .OrderByDescending(p => p.StartDate)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<AnomalyDetectionProject>(totalCount, items);
    }

    /// <summary>
    /// OEMカスタマイズの最適化されたページネーション取得
    /// </summary>
    public async Task<PagedResultDto<OemCustomization>> GetOemCustomizationsOptimizedAsync(
        int skipCount = 0,
        int maxResultCount = 10,
        string? oemCode = null,
        string? entityType = null,
        CustomizationStatus? status = null,
        CustomizationType? type = null)
    {
        var queryable = await _customizationRepository.GetQueryableAsync();

        // フィルタリング
        if (!string.IsNullOrEmpty(oemCode))
        {
            queryable = queryable.Where(c => c.OemCode.Code == oemCode);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            queryable = queryable.Where(c => c.EntityType == entityType);
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(c => c.Status == status.Value);
        }

        if (type.HasValue)
        {
            queryable = queryable.Where(c => c.Type == type.Value);
        }

        // 総件数を取得
        var totalCount = await queryable.CountAsync();

        // ページネーション適用
        var items = await queryable
            .OrderByDescending(c => c.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

        return new PagedResultDto<OemCustomization>(totalCount, items);
    }

    /// <summary>
    /// 類似信号検索の最適化されたクエリ
    /// </summary>
    public async Task<List<CanSignal>> GetSimilarSignalsOptimizedAsync(
        Guid targetSignalId,
        CanSystemType? systemType = null,
        int maxResults = 100)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();

        // 対象信号を除外
        queryable = queryable.Where(s => s.Id != targetSignalId);

        // システムタイプでフィルタ
        if (systemType.HasValue)
        {
            queryable = queryable.Where(s => s.SystemType == systemType.Value);
        }

        // アクティブな信号のみ
        queryable = queryable.Where(s => s.Status == SignalStatus.Active);

        // 最適化されたソートと制限
        return await queryable
            .OrderBy(s => s.SystemType)
            .ThenBy(s => s.Identifier.SignalName)
            .Take(maxResults)
            .ToListAsync();
    }

    /// <summary>
    /// ダッシュボード用の統計データを最適化されたクエリで取得
    /// </summary>
    public async Task<DashboardStatistics> GetDashboardStatisticsOptimizedAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        // 並列実行で複数の統計を同時取得
        var task1 = GetSignalCountBySystemTypeAsync();
        var task2 = GetDetectionResultCountByLevelAsync(start, end);
        var task3 = GetActiveProjectCountAsync();
        var task4 = GetPendingCustomizationCountAsync();
        var task5 = GetRecentDetectionResultsAsync(10);

        await Task.WhenAll(task1, task2, task3, task4, task5);

        return new DashboardStatistics
        {
            SignalCountBySystemType = await task1,
            DetectionResultCountByLevel = await task2,
            ActiveProjectCount = await task3,
            PendingCustomizationCount = await task4,
            RecentDetectionResults = await task5,
            GeneratedAt = DateTime.UtcNow
        };
    }

    #region Private Helper Methods

    private async Task<Dictionary<CanSystemType, int>> GetSignalCountBySystemTypeAsync()
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();
        return await queryable
            .Where(s => s.Status == SignalStatus.Active)
            .GroupBy(s => s.SystemType)
            .Select(g => new { SystemType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SystemType, x => x.Count);
    }

    private async Task<Dictionary<AnomalyLevel, int>> GetDetectionResultCountByLevelAsync(DateTime start, DateTime end)
    {
        var queryable = await _detectionResultRepository.GetQueryableAsync();
        return await queryable
            .Where(r => r.DetectedAt >= start && r.DetectedAt <= end)
            .GroupBy(r => r.AnomalyLevel)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Level, x => x.Count);
    }

    private async Task<int> GetActiveProjectCountAsync()
    {
        var queryable = await _projectRepository.GetQueryableAsync();
        return await queryable
            .CountAsync(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Planning);
    }

    private async Task<int> GetPendingCustomizationCountAsync()
    {
        var queryable = await _customizationRepository.GetQueryableAsync();
        return await queryable
            .CountAsync(c => c.Status == CustomizationStatus.PendingApproval);
    }

    private async Task<List<AnomalyDetectionResult>> GetRecentDetectionResultsAsync(int count)
    {
        var queryable = await _detectionResultRepository.GetQueryableAsync();
        return await queryable
            .OrderByDescending(r => r.DetectedAt)
            .Take(count)
            .ToListAsync();
    }

    #endregion
}

/// <summary>
/// ダッシュボード統計データ
/// </summary>
public class DashboardStatistics
{
    public Dictionary<CanSystemType, int> SignalCountBySystemType { get; set; } = new();
    public Dictionary<AnomalyLevel, int> DetectionResultCountByLevel { get; set; } = new();
    public int ActiveProjectCount { get; set; }
    public int PendingCustomizationCount { get; set; }
    public List<AnomalyDetectionResult> RecentDetectionResults { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}