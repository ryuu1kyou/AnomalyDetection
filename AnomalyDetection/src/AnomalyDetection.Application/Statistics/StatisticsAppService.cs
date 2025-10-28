using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.Statistics.Dtos;
using AnomalyDetection.Permissions;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;
using AnomalyDetection.Projects;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.Statistics;

[Authorize(AnomalyDetectionPermissions.Statistics.Default)]
public class StatisticsAppService : ApplicationService, IStatisticsAppService
{
    private readonly IRepository<AnomalyDetectionResult, Guid> _detectionResultRepository;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;
    private readonly IRepository<AnomalyDetectionProject, Guid> _projectRepository;

    public StatisticsAppService(
        IRepository<AnomalyDetectionResult, Guid> detectionResultRepository,
        IRepository<CanSignal, Guid> canSignalRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IRepository<AnomalyDetectionProject, Guid> projectRepository)
    {
        _detectionResultRepository = detectionResultRepository;
        _canSignalRepository = canSignalRepository;
        _detectionLogicRepository = detectionLogicRepository;
        _projectRepository = projectRepository;
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ViewReports)]
    public async Task<DetectionStatisticsDto> GetDetectionStatisticsAsync(GetDetectionStatisticsInput input)
    {
        var queryable = await _detectionResultRepository.GetQueryableAsync();
        
        // Apply filters
        if (input.StartDate.HasValue)
        {
            queryable = queryable.Where(x => x.DetectedAt >= input.StartDate.Value);
        }
        
        if (input.EndDate.HasValue)
        {
            queryable = queryable.Where(x => x.DetectedAt <= input.EndDate.Value);
        }
        
        if (input.CanSignalId.HasValue)
        {
            queryable = queryable.Where(x => x.CanSignalId == input.CanSignalId.Value);
        }
        
        if (input.DetectionLogicId.HasValue)
        {
            queryable = queryable.Where(x => x.DetectionLogicId == input.DetectionLogicId.Value);
        }
        
        if (input.AnomalyLevel.HasValue)
        {
            queryable = queryable.Where(x => x.AnomalyLevel == input.AnomalyLevel.Value);
        }

        var results = await AsyncExecuter.ToListAsync(queryable);
        
        return new DetectionStatisticsDto
        {
            TotalDetections = results.Count,
            AnomalyLevelCounts = results.GroupBy(x => x.AnomalyLevel)
                .ToDictionary(g => g.Key, g => g.Count()),
            ResolutionStatusCounts = results.GroupBy(x => x.ResolutionStatus)
                .ToDictionary(g => g.Key, g => g.Count()),
            DetectionsByDate = results.GroupBy(x => x.DetectedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageConfidenceScore = results.Any() ? results.Average(x => x.ConfidenceScore) : 0,
            TopSignalsByDetectionCount = results.GroupBy(x => x.CanSignalId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopLogicsByDetectionCount = results.GroupBy(x => x.DetectionLogicId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.GenerateReports)]
    public async Task<SystemAnomalyReportDto> GenerateSystemReportAsync(GenerateSystemReportInput input)
    {
        var reportId = Guid.NewGuid();
        var generatedAt = DateTime.UtcNow;
        
        // Get detection statistics
        var detectionStats = await GetDetectionStatisticsAsync(new GetDetectionStatisticsInput
        {
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            CanSignalId = input.CanSignalId,
            DetectionLogicId = input.DetectionLogicId,
            AnomalyLevel = input.AnomalyLevel
        });
        
        // Get signal statistics
        var signalCount = await _canSignalRepository.CountAsync();
        var activeSignalCount = await _canSignalRepository.CountAsync(x => x.Status == SignalStatus.Active);
        
        // Get logic statistics
        var logicCount = await _detectionLogicRepository.CountAsync();
        var approvedLogicCount = await _detectionLogicRepository.CountAsync(x => x.Status == DetectionLogicStatus.Approved);
        
        // Get project statistics
        var projectCount = await _projectRepository.CountAsync();
        var activeProjectCount = await _projectRepository.CountAsync(x => x.Status == ProjectStatus.Active);
        
        return new SystemAnomalyReportDto
        {
            ReportId = reportId,
            ReportName = input.ReportName ?? $"System Anomaly Report - {generatedAt:yyyy-MM-dd}",
            GeneratedAt = generatedAt,
            GeneratedBy = CurrentUser.UserName ?? "System",
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            DetectionStatistics = detectionStats,
            SystemOverview = new SystemOverviewDto
            {
                TotalSignals = signalCount,
                ActiveSignals = activeSignalCount,
                TotalDetectionLogics = logicCount,
                ApprovedDetectionLogics = approvedLogicCount,
                TotalProjects = projectCount,
                ActiveProjects = activeProjectCount
            },
            Summary = GenerateReportSummary(detectionStats, signalCount, logicCount, projectCount)
        };
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ViewDashboard)]
    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var last30Days = today.AddDays(-30);
        var last7Days = today.AddDays(-7);
        
        // Get recent detection counts
        var totalDetections = await _detectionResultRepository.CountAsync();
        var detectionsLast30Days = await _detectionResultRepository.CountAsync(x => x.DetectedAt >= last30Days);
        var detectionsLast7Days = await _detectionResultRepository.CountAsync(x => x.DetectedAt >= last7Days);
        var detectionsToday = await _detectionResultRepository.CountAsync(x => x.DetectedAt >= today);
        
        // Get critical anomalies
        var criticalAnomalies = await _detectionResultRepository.CountAsync(x => 
            x.AnomalyLevel == AnomalyLevel.Critical && x.ResolutionStatus == ResolutionStatus.Unresolved);
        
        // Get system counts
        var totalSignals = await _canSignalRepository.CountAsync();
        var activeSignals = await _canSignalRepository.CountAsync(x => x.Status == SignalStatus.Active);
        var totalLogics = await _detectionLogicRepository.CountAsync();
        var approvedLogics = await _detectionLogicRepository.CountAsync(x => x.Status == DetectionLogicStatus.Approved);
        var totalProjects = await _projectRepository.CountAsync();
        var activeProjects = await _projectRepository.CountAsync(x => x.Status == ProjectStatus.Active);
        
        // Get trend data for the last 7 days
        var trendData = new Dictionary<DateTime, int>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var count = await _detectionResultRepository.CountAsync(x => 
                x.DetectedAt >= date && x.DetectedAt < date.AddDays(1));
            trendData[date] = count;
        }
        
        return new DashboardStatisticsDto
        {
            TotalDetections = totalDetections,
            DetectionsLast30Days = detectionsLast30Days,
            DetectionsLast7Days = detectionsLast7Days,
            DetectionsToday = detectionsToday,
            CriticalAnomalies = criticalAnomalies,
            TotalSignals = totalSignals,
            ActiveSignals = activeSignals,
            TotalDetectionLogics = totalLogics,
            ApprovedDetectionLogics = approvedLogics,
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            DetectionTrend = trendData,
            LastUpdated = DateTime.UtcNow
        };
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ExportData)]
    public async Task<byte[]> ExportSystemReportAsync(Guid reportId, ReportFormat format)
    {
        // TODO: Implement report export functionality
        await Task.CompletedTask;
        throw new NotImplementedException("Report export functionality will be implemented in a future version");
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ViewReports)]
    public async Task<PagedResultDto<SystemAnomalyReportDto>> GetSavedReportsAsync(PagedAndSortedResultRequestDto input)
    {
        // TODO: Implement saved reports functionality with a dedicated repository
        await Task.CompletedTask;
        return new PagedResultDto<SystemAnomalyReportDto>(0, new List<SystemAnomalyReportDto>());
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.GenerateReports)]
    public async Task<Guid> SaveReportAsync(SystemAnomalyReportDto report)
    {
        // TODO: Implement report saving functionality
        await Task.CompletedTask;
        return Guid.NewGuid();
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.GenerateReports)]
    public async Task DeleteReportAsync(Guid reportId)
    {
        // TODO: Implement report deletion functionality
        await Task.CompletedTask;
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ViewDashboard)]
    public async Task<DashboardStatisticsDto> GetRealTimeStatisticsAsync()
    {
        // For now, return the same as regular dashboard statistics
        // In a real implementation, this might use caching or real-time data sources
        return await GetDashboardStatisticsAsync();
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ScheduleReports)]
    public async Task ScheduleReportAsync(GenerateSystemReportInput input, string cronExpression)
    {
        // TODO: Implement report scheduling functionality using background jobs
        await Task.CompletedTask;
        throw new NotImplementedException("Report scheduling functionality will be implemented in a future version");
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ScheduleReports)]
    public async Task<ListResultDto<Dictionary<string, object>>> GetScheduledReportsAsync()
    {
        // TODO: Implement scheduled reports retrieval
        await Task.CompletedTask;
        return new ListResultDto<Dictionary<string, object>>(new List<Dictionary<string, object>>());
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ScheduleReports)]
    public async Task CancelScheduledReportAsync(Guid scheduleId)
    {
        // TODO: Implement scheduled report cancellation
        await Task.CompletedTask;
    }

    private static string GenerateReportSummary(DetectionStatisticsDto stats, int signalCount, int logicCount, int projectCount)
    {
        var summary = $"System Report Summary:\n";
        summary += $"- Total Detections: {stats.TotalDetections}\n";
        summary += $"- Average Confidence Score: {stats.AverageConfidenceScore:F2}\n";
        summary += $"- Total CAN Signals: {signalCount}\n";
        summary += $"- Total Detection Logics: {logicCount}\n";
        summary += $"- Total Projects: {projectCount}\n";
        
        if (stats.AnomalyLevelCounts.Any())
        {
            summary += "\nAnomaly Level Distribution:\n";
            foreach (var kvp in stats.AnomalyLevelCounts.OrderByDescending(x => x.Value))
            {
                summary += $"- {kvp.Key}: {kvp.Value} ({(double)kvp.Value / stats.TotalDetections * 100:F1}%)\n";
            }
        }
        
        if (stats.ResolutionStatusCounts.Any())
        {
            summary += "\nResolution Status Distribution:\n";
            foreach (var kvp in stats.ResolutionStatusCounts.OrderByDescending(x => x.Value))
            {
                summary += $"- {kvp.Key}: {kvp.Value} ({(double)kvp.Value / stats.TotalDetections * 100:F1}%)\n";
            }
        }
        
        return summary;
    }
}