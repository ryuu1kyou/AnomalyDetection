using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Safety;

public interface ISafetyTraceAuditReportAppService : IApplicationService
{
    Task<SafetyTraceAuditAggregateDto> GetAggregateAsync(SafetyTraceAuditFilterDto input);
    Task<ExportedFileDto> ExportAsync(SafetyTraceAuditExportDto input);
}

public class SafetyTraceAuditFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? AsilLevel { get; set; }
    public List<int>? ApprovalStatuses { get; set; }
    public Guid? ProjectId { get; set; }
}

public class SafetyTraceAuditAggregateDto
{
    public int TotalRecords { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int SubmittedCount { get; set; }
    public int DraftCount { get; set; }
    public Dictionary<int, int> AsilDistribution { get; set; } = new();
    public double AverageVerifications { get; set; }
    public double AverageValidations { get; set; }
    public int HighRiskPending { get; set; }
}

public class SafetyTraceAuditExportDto : SafetyTraceAuditFilterDto
{
    public string Format { get; set; } = "csv"; // csv,json,pdf,excel
}

public class ExportedFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}