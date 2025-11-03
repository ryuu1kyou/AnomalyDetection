using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.Statistics.Dtos;
using AnomalyDetection.Permissions;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;
using AnomalyDetection.Projects;
using AnomalyDetection.Shared.Export;
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
    private readonly ExportService _exportService;

    public StatisticsAppService(
        IRepository<AnomalyDetectionResult, Guid> detectionResultRepository,
        IRepository<CanSignal, Guid> canSignalRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IRepository<AnomalyDetectionProject, Guid> projectRepository,
        ExportService exportService)
    {
        _detectionResultRepository = detectionResultRepository;
        _canSignalRepository = canSignalRepository;
        _detectionLogicRepository = detectionLogicRepository;
        _projectRepository = projectRepository;
        _exportService = exportService;
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ViewReports)]
    public async Task<DetectionStatisticsDto> GetDetectionStatisticsAsync(GetDetectionStatisticsInput input)
    {
        var queryable = await _detectionResultRepository.GetQueryableAsync();

        // Apply filters
        queryable = queryable.Where(x => x.DetectedAt >= input.FromDate);
        queryable = queryable.Where(x => x.DetectedAt <= input.ToDate);

        if (input.CanSignalId.HasValue)
        {
            queryable = queryable.Where(x => x.CanSignalId == input.CanSignalId.Value);
        }

        if (input.DetectionLogicId.HasValue)
        {
            queryable = queryable.Where(x => x.DetectionLogicId == input.DetectionLogicId.Value);
        }

        if (input.AnomalyLevels?.Any() == true)
        {
            queryable = queryable.Where(x => input.AnomalyLevels.Contains(x.AnomalyLevel));
        }

        var results = await AsyncExecuter.ToListAsync(queryable);

        return new DetectionStatisticsDto
        {
            TotalDetections = results.Count,
            FromDate = input.FromDate,
            ToDate = input.ToDate,
            DetectionsByAnomalyLevel = results.GroupBy(x => x.AnomalyLevel)
                .ToDictionary(g => g.Key, g => g.Count()),
            TotalResolvedDetections = results.Count(x => x.ResolutionStatus == ResolutionStatus.Resolved),
            TotalFalsePositives = results.Count(x => x.ResolutionStatus == ResolutionStatus.FalsePositive),
            TotalOpenDetections = results.Count(x => x.ResolutionStatus == ResolutionStatus.Open),
            ResolutionRate = results.Any() ? (double)results.Count(x => x.ResolutionStatus == ResolutionStatus.Resolved) / results.Count * 100 : 0,
            FalsePositiveRate = results.Any() ? (double)results.Count(x => x.ResolutionStatus == ResolutionStatus.FalsePositive) / results.Count * 100 : 0
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
            FromDate = input.FromDate,
            ToDate = input.ToDate,
            SystemTypes = input.IncludedSystems,
            AnomalyLevels = input.IncludedAnomalyLevels
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
            ReportName = input.ReportName,
            GeneratedAt = generatedAt,
            GeneratedBy = CurrentUser.Id ?? Guid.Empty,
            GeneratedByUserName = CurrentUser.UserName ?? "System",
            FromDate = input.FromDate,
            ToDate = input.ToDate,
            IncludedSystems = input.IncludedSystems,
            IncludedAnomalyLevels = input.IncludedAnomalyLevels,
            IncludeResolvedDetections = input.IncludeResolvedDetections,
            IncludeFalsePositives = input.IncludeFalsePositives
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
            x.AnomalyLevel == AnomalyLevel.Critical && x.ResolutionStatus == ResolutionStatus.Open);

        // Get system counts
        var totalSignals = await _canSignalRepository.CountAsync();
        var activeSignals = await _canSignalRepository.CountAsync(x => x.Status == SignalStatus.Active);
        var totalLogics = await _detectionLogicRepository.CountAsync();
        var approvedLogics = await _detectionLogicRepository.CountAsync(x => x.Status == DetectionLogicStatus.Approved);
        var totalProjects = await _projectRepository.CountAsync();
        var activeProjects = await _projectRepository.CountAsync(x => x.Status == ProjectStatus.Active);

        // Build KPI cards
        var kpiCards = new List<KpiCardDto>
        {
            new KpiCardDto
            {
                Title = "Total Detections",
                Value = totalDetections.ToString(),
                TrendDirection = "Stable",
                Color = "Info",
                Icon = "chart-line"
            },
            new KpiCardDto
            {
                Title = "Critical Anomalies",
                Value = criticalAnomalies.ToString(),
                TrendDirection = "Down",
                Color = criticalAnomalies > 0 ? "Danger" : "Success",
                Icon = "exclamation-triangle"
            },
            new KpiCardDto
            {
                Title = "Active Signals",
                Value = activeSignals.ToString(),
                TrendDirection = "Up",
                Color = "Success",
                Icon = "signal"
            }
        };

        return new DashboardStatisticsDto
        {
            KpiCards = kpiCards,
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

    [Authorize(AnomalyDetectionPermissions.Statistics.ExportData)]
    public async Task<ExportFileResult> ExportDetectionStatisticsAsync(GetDetectionStatisticsInput input, string format)
    {
        // Get detection statistics
        var stats = await GetDetectionStatisticsAsync(input);

        // Get detailed detection results for export
        var queryable = await _detectionResultRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.DetectedAt >= input.FromDate);
        queryable = queryable.Where(x => x.DetectedAt <= input.ToDate);

        if (input.CanSignalId.HasValue)
            queryable = queryable.Where(x => x.CanSignalId == input.CanSignalId.Value);

        if (input.DetectionLogicId.HasValue)
            queryable = queryable.Where(x => x.DetectionLogicId == input.DetectionLogicId.Value);

        var results = await AsyncExecuter.ToListAsync(queryable);

        // Prepare export data
        var exportData = results.Select(r => new
        {
            r.Id,
            DetectedAt = r.DetectedAt,
            r.CanSignalId,
            r.DetectionLogicId,
            AnomalyLevel = r.AnomalyLevel.ToString(),
            AnomalyType = r.AnomalyType.ToString(),
            ConfidenceScore = r.ConfidenceScore,
            ResolutionStatus = r.ResolutionStatus.ToString(),
            Description = r.Description,
            DetectionCondition = r.DetectionCondition,
            r.DetectionDuration,
            r.IsValidated,
            r.IsFalsePositiveFlag
        }).Select(x => (object)x).ToList();

        // Parse format
        var exportFormat = format.ToLowerInvariant() switch
        {
            "csv" => ExportService.ExportFormat.Csv,
            "json" => ExportService.ExportFormat.Json,
            "pdf" => ExportService.ExportFormat.Pdf,
            "excel" => ExportService.ExportFormat.Excel,
            "xlsx" => ExportService.ExportFormat.Excel,
            _ => ExportService.ExportFormat.Csv
        };

        // Export using ExportService
        var exportRequest = new ExportDetectionRequest
        {
            Results = exportData,
            Format = exportFormat,
            FileNamePrefix = "detection_statistics",
            CsvOptions = new CsvExportOptions
            {
                IncludeHeader = true,
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            },
            JsonOptions = new JsonExportOptions
            {
                Indented = true,
                CamelCase = true
            },
            ExcelOptions = new ExcelExportOptions
            {
                IncludeHeader = true,
                EnableAutoFilter = true
            },
            GeneratedBy = CurrentUser.UserName ?? CurrentUser.Id?.ToString() ?? "System",
            AdditionalMetadata = new Dictionary<string, string>
            {
                ["fromDate"] = input.FromDate.ToString("o"),
                ["toDate"] = input.ToDate.ToString("o"),
                ["format"] = format
            }
        };

        var result = await _exportService.ExportDetectionResultsAsync(exportRequest);

        return new ExportFileResult
        {
            FileData = result.Data,
            ContentType = result.ContentType,
            FileName = result.FileName,
            RecordCount = results.Count,
            ExportedAt = DateTime.UtcNow,
            Format = format
        };
    }

    [Authorize(AnomalyDetectionPermissions.Statistics.ExportData)]
    public async Task<ExportFileResult> ExportDashboardDataAsync(string format)
    {
        // Get dashboard statistics
        var dashboardStats = await GetDashboardStatisticsAsync();

        // Prepare export data
        var exportData = new List<object>
        {
            new
            {
                ReportType = "Dashboard Statistics",
                GeneratedAt = DateTime.UtcNow,
                LastUpdated = dashboardStats.LastUpdated,
                KpiCards = dashboardStats.KpiCards
            }
        };

        // Parse format
        var exportFormat = format.ToLowerInvariant() switch
        {
            "csv" => ExportService.ExportFormat.Csv,
            "json" => ExportService.ExportFormat.Json,
            "pdf" => ExportService.ExportFormat.Pdf,
            "excel" => ExportService.ExportFormat.Excel,
            "xlsx" => ExportService.ExportFormat.Excel,
            _ => ExportService.ExportFormat.Csv
        };

        // Export using ExportService
        var exportRequest = new ExportDetectionRequest
        {
            Results = exportData,
            Format = exportFormat,
            FileNamePrefix = "dashboard_data",
            CsvOptions = new CsvExportOptions
            {
                IncludeHeader = true,
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
            },
            JsonOptions = new JsonExportOptions
            {
                Indented = true,
                CamelCase = true
            },
            ExcelOptions = new ExcelExportOptions
            {
                IncludeHeader = true,
                EnableAutoFilter = true
            },
            GeneratedBy = CurrentUser.UserName ?? CurrentUser.Id?.ToString() ?? "System",
            AdditionalMetadata = new Dictionary<string, string>
            {
                ["export"] = "dashboard",
                ["format"] = format
            }
        };

        var result = await _exportService.ExportDetectionResultsAsync(exportRequest);

        return new ExportFileResult
        {
            FileData = result.Data,
            ContentType = result.ContentType,
            FileName = result.FileName,
            RecordCount = dashboardStats.KpiCards.Count,
            ExportedAt = DateTime.UtcNow,
            Format = format
        };
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
        summary += $"- Resolution Rate: {stats.ResolutionRate:F2}%\n";
        summary += $"- False Positive Rate: {stats.FalsePositiveRate:F2}%\n";
        summary += $"- Total CAN Signals: {signalCount}\n";
        summary += $"- Total Detection Logics: {logicCount}\n";
        summary += $"- Total Projects: {projectCount}\n";

        if (stats.DetectionsByAnomalyLevel.Any())
        {
            summary += "\nAnomaly Level Distribution:\n";
            foreach (var kvp in stats.DetectionsByAnomalyLevel.OrderByDescending(x => x.Value))
            {
                summary += $"- {kvp.Key}: {kvp.Value} ({(double)kvp.Value / stats.TotalDetections * 100:F1}%)\n";
            }
        }

        return summary;
    }
}