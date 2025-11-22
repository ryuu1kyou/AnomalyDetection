using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using AnomalyDetection.Safety;
using AnomalyDetection.AnomalyDetection; // AsilLevel enum
using AnomalyDetection.Shared.Export;
using AnomalyDetection.Safety.AppServices; // For ExportResultDto reuse
using Microsoft.AspNetCore.Authorization;
using AnomalyDetection.Permissions;

namespace AnomalyDetection.Safety.Audit;

/// <summary>
/// Generates aggregated Safety Trace audit reports & exports.
/// </summary>
public class SafetyTraceAuditReportAppService : ApplicationService, ISafetyTraceAuditReportAppService
{
    private readonly IRepository<SafetyTraceRecord, Guid> _repository;
    private readonly ExportService _exportService;

    public SafetyTraceAuditReportAppService(
        IRepository<SafetyTraceRecord, Guid> repository,
        ExportService exportService)
    {
        _repository = repository;
        _exportService = exportService;
    }

    public async Task<SafetyTraceAuditAggregateDto> GetAggregateAsync(SafetyTraceAuditFilterDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        var internalFilter = new SafetyTraceAuditFilterInput
        {
            From = input.From,
            To = input.To,
            AsilLevel = input.AsilLevel,
            ApprovalStatuses = input.ApprovalStatuses,
            ProjectId = input.ProjectId
        };
        queryable = ApplyFilter(queryable, internalFilter);
        var records = await AsyncExecuter.ToListAsync(queryable);

        var aggregate = new SafetyTraceAuditAggregateDto
        {
            TotalRecords = records.Count,
            ApprovedCount = records.Count(r => r.ApprovalStatus == ApprovalStatus.Approved),
            RejectedCount = records.Count(r => r.ApprovalStatus == ApprovalStatus.Rejected),
            UnderReviewCount = records.Count(r => r.ApprovalStatus == ApprovalStatus.UnderReview),
            SubmittedCount = records.Count(r => r.ApprovalStatus == ApprovalStatus.Submitted),
            DraftCount = records.Count(r => r.ApprovalStatus == ApprovalStatus.Draft),
            AsilDistribution = records
                .GroupBy(r => r.AsilLevel)
                .ToDictionary(g => (int)g.Key, g => g.Count()),
            AverageVerifications = records.Count > 0 ? Math.Round(records.Average(r => r.Verifications.Count), 2) : 0,
            AverageValidations = records.Count > 0 ? Math.Round(records.Average(r => r.Validations.Count), 2) : 0,
            HighRiskPending = records.Count(r => r.AsilLevel >= AsilLevel.C && (r.ApprovalStatus == ApprovalStatus.Submitted || r.ApprovalStatus == ApprovalStatus.UnderReview))
        };

        return aggregate;
    }

    [Authorize(AnomalyDetectionPermissions.SafetyTrace.Audit.Export)]
    public async Task<ExportedFileDto> ExportAsync(SafetyTraceAuditExportDto input)
    {
        var queryable = await _repository.GetQueryableAsync();
        queryable = ApplyFilter(queryable, new SafetyTraceAuditFilterInput
        {
            From = input.From,
            To = input.To,
            AsilLevel = input.AsilLevel,
            ApprovalStatuses = input.ApprovalStatuses,
            ProjectId = input.ProjectId
        });
        var records = await AsyncExecuter.ToListAsync(queryable);

        var rows = records.Select(r => new
        {
            r.Id,
            r.RequirementId,
            r.SafetyGoalId,
            AsilLevel = r.AsilLevel.ToString(),
            ApprovalStatus = r.ApprovalStatus.ToString(),
            Verifications = r.Verifications.Count,
            Validations = r.Validations.Count,
            ChangeRequests = r.ChangeRequests.Count,
            LifecycleEvents = r.LifecycleEvents.Count,
            r.Version,
            r.ProjectId,
            r.DetectionLogicId,
            r.CreationTime,
            r.LastModificationTime
        }).ToList();

        // Append aggregate summary row (Req18 enhancement)
        if (records.Any())
        {
            var total = records.Count;
            var approved = records.Count(r => r.ApprovalStatus == ApprovalStatus.Approved);
            var underReview = records.Count(r => r.ApprovalStatus == ApprovalStatus.UnderReview);
            var submitted = records.Count(r => r.ApprovalStatus == ApprovalStatus.Submitted);
            var rejected = records.Count(r => r.ApprovalStatus == ApprovalStatus.Rejected);
            var draft = records.Count(r => r.ApprovalStatus == ApprovalStatus.Draft);
            var verificationsTotal = records.Sum(r => r.Verifications.Count);
            var validationsTotal = records.Sum(r => r.Validations.Count);
            var changeRequestsTotal = records.Sum(r => r.ChangeRequests.Count);
            var lifecycleEventsTotal = records.Sum(r => r.LifecycleEvents.Count);
            double pct(int c) => Math.Round(c * 100.0 / Math.Max(1, total), 1);

            rows.Add(new
            {
                Id = Guid.Empty,
                RequirementId = "SUMMARY",
                SafetyGoalId = string.Empty,
                AsilLevel = "N/A",
                ApprovalStatus = $"Approved:{approved}({pct(approved)}%) UnderReview:{underReview}({pct(underReview)}%) Submitted:{submitted}({pct(submitted)}%) Rejected:{rejected}({pct(rejected)}%) Draft:{draft}({pct(draft)}%)",
                Verifications = verificationsTotal,
                Validations = validationsTotal,
                ChangeRequests = changeRequestsTotal,
                LifecycleEvents = lifecycleEventsTotal,
                Version = 0,
                ProjectId = (Guid?)null,
                DetectionLogicId = (Guid?)null,
                CreationTime = DateTime.UtcNow,
                LastModificationTime = (DateTime?)DateTime.UtcNow
            });
        }

        var formatEnum = input.Format?.ToLower() switch
        {
            "json" => ExportService.ExportFormat.Json,
            "pdf" => ExportService.ExportFormat.Pdf,
            "excel" => ExportService.ExportFormat.Excel,
            _ => ExportService.ExportFormat.Csv
        };

        var request = new ExportDetectionRequest
        {
            Results = rows,
            Format = formatEnum,
            FileNamePrefix = "safety_audit",
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions { IncludeHeader = true }
        };

        var result = await _exportService.ExportDetectionResultsAsync(request);

        return new ExportedFileDto
        {
            FileName = result.FileName,
            ContentType = result.ContentType,
            RecordCount = result.Metadata.RecordCount,
            Format = result.Metadata.Format,
            ExportedAt = result.Metadata.ExportedAt,
            FileData = result.Data
        };
    }

    private static IQueryable<SafetyTraceRecord> ApplyFilter(IQueryable<SafetyTraceRecord> queryable, SafetyTraceAuditFilterInput input)
    {
        if (input.From.HasValue)
            queryable = queryable.Where(r => r.CreationTime >= input.From.Value);
        if (input.To.HasValue)
            queryable = queryable.Where(r => r.CreationTime <= input.To.Value);
        if (input.AsilLevel.HasValue)
            queryable = queryable.Where(r => (int)r.AsilLevel == input.AsilLevel.Value);
        if (input.ApprovalStatuses?.Any() == true)
        {
            var statuses = input.ApprovalStatuses.Select(s => (ApprovalStatus)s).ToList();
            queryable = queryable.Where(r => statuses.Contains(r.ApprovalStatus));
        }
        if (input.ProjectId.HasValue)
            queryable = queryable.Where(r => r.ProjectId == input.ProjectId.Value);
        return queryable;
    }
}

public class SafetyTraceAuditFilterInput
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? AsilLevel { get; set; }
    public List<int>? ApprovalStatuses { get; set; }
    public Guid? ProjectId { get; set; }
}
