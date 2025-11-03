using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.Statistics.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Statistics;

public interface IStatisticsAppService : IApplicationService
{
    /// <summary>
    /// Get detection statistics based on input criteria
    /// </summary>
    Task<DetectionStatisticsDto> GetDetectionStatisticsAsync(GetDetectionStatisticsInput input);

    /// <summary>
    /// Generate system anomaly report
    /// </summary>
    Task<SystemAnomalyReportDto> GenerateSystemReportAsync(GenerateSystemReportInput input);

    /// <summary>
    /// Get dashboard statistics for overview
    /// </summary>
    Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();

    /// <summary>
    /// Export system report to file
    /// </summary>
    Task<byte[]> ExportSystemReportAsync(Guid reportId, ReportFormat format);

    /// <summary>
    /// Export detection statistics to file (CSV/PDF/Excel/JSON)
    /// </summary>
    Task<ExportFileResult> ExportDetectionStatisticsAsync(GetDetectionStatisticsInput input, string format);

    /// <summary>
    /// Export dashboard statistics to file
    /// </summary>
    Task<ExportFileResult> ExportDashboardDataAsync(string format);

    /// <summary>
    /// Get saved reports list
    /// </summary>
    Task<PagedResultDto<SystemAnomalyReportDto>> GetSavedReportsAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Save generated report
    /// </summary>
    Task<Guid> SaveReportAsync(SystemAnomalyReportDto report);

    /// <summary>
    /// Delete saved report
    /// </summary>
    Task DeleteReportAsync(Guid reportId);

    /// <summary>
    /// Get real-time statistics
    /// </summary>
    Task<DashboardStatisticsDto> GetRealTimeStatisticsAsync();

    /// <summary>
    /// Schedule automatic report generation
    /// </summary>
    Task ScheduleReportAsync(GenerateSystemReportInput input, string cronExpression);

    /// <summary>
    /// Get scheduled reports
    /// </summary>
    Task<ListResultDto<Dictionary<string, object>>> GetScheduledReportsAsync();

    /// <summary>
    /// Cancel scheduled report
    /// </summary>
    Task CancelScheduledReportAsync(Guid scheduleId);
}