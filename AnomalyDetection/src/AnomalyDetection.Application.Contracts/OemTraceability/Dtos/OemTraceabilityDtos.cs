using System;
using System.Collections.Generic;
using AnomalyDetection.OemTraceability;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.Application.Contracts.OemTraceability.Dtos;

/// <summary>
/// OEMトレーサビリティ結果DTO
/// </summary>
public class OemTraceabilityDto
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public List<OemUsageInfoDto> OemUsages { get; set; } = new();
    public CrossOemDifferencesAnalysisDto CrossOemDifferences { get; set; } = new();
}

/// <summary>
/// OEM使用情報DTO
/// </summary>
public class OemUsageInfoDto
{
    public string OemCode { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public List<string> Vehicles { get; set; } = new();
    public List<OemCustomizationSummaryDto> CustomizationHistory { get; set; } = new();
    public List<OemApprovalSummaryDto> ApprovalRecords { get; set; } = new();
}

/// <summary>
/// OEMカスタマイズ概要DTO
/// </summary>
public class OemCustomizationSummaryDto
{
    public Guid Id { get; set; }
    public CustomizationType Type { get; set; }
    public CustomizationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// OEM承認概要DTO
/// </summary>
public class OemApprovalSummaryDto
{
    public Guid Id { get; set; }
    public ApprovalType Type { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// OEM間差異分析DTO
/// </summary>
public class CrossOemDifferencesAnalysisDto
{
    public Dictionary<string, List<OemParameterDifferenceDto>> ParameterDifferences { get; set; } = new();
    public List<UsagePatternDifferenceDto> UsagePatternDifferences { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// OEMパラメータ差異DTO
/// </summary>
public class OemParameterDifferenceDto
{
    public string OemCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public object? OriginalValue { get; set; }
    public object? CustomValue { get; set; }
    public double DifferencePercentage { get; set; }
    public string DifferenceDescription { get; set; } = string.Empty;
}

/// <summary>
/// 使用パターン差異DTO
/// </summary>
public class UsagePatternDifferenceDto
{
    public string OemCode { get; set; } = string.Empty;
    public string PatternType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public string Impact { get; set; } = string.Empty;
}

/// <summary>
/// OEMカスタマイズ作成DTO
/// </summary>
public class CreateOemCustomizationDto
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
    public CustomizationType Type { get; set; }
    public Dictionary<string, object> CustomParameters { get; set; } = new();
    public Dictionary<string, object> OriginalParameters { get; set; } = new();
    public string CustomizationReason { get; set; } = string.Empty;
}

/// <summary>
/// OEMカスタマイズ更新DTO
/// </summary>
public class UpdateOemCustomizationDto
{
    public Dictionary<string, object> CustomParameters { get; set; } = new();
    public string CustomizationReason { get; set; } = string.Empty;
}

/// <summary>
/// OEMカスタマイズDTO
/// </summary>
public class OemCustomizationDto : EntityDto<Guid>
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
    public CustomizationType Type { get; set; }
    public Dictionary<string, object> CustomParameters { get; set; } = new();
    public Dictionary<string, object> OriginalParameters { get; set; } = new();
    public string CustomizationReason { get; set; } = string.Empty;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public CustomizationStatus Status { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// OEM承認作成DTO
/// </summary>
public class CreateOemApprovalDto
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
    public ApprovalType Type { get; set; }
    public string ApprovalReason { get; set; } = string.Empty;
    public Dictionary<string, object> ApprovalData { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; } = 2;
}

/// <summary>
/// OEM承認DTO
/// </summary>
public class OemApprovalDto : EntityDto<Guid>
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
    public ApprovalType Type { get; set; }
    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ApprovalStatus Status { get; set; }
    public string ApprovalReason { get; set; } = string.Empty;
    public string? ApprovalNotes { get; set; }
    public Dictionary<string, object> ApprovalData { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsUrgent { get; set; }
}

/// <summary>
/// OEMトレーサビリティレポート生成DTO
/// </summary>
public class GenerateOemTraceabilityReportDto
{
    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? OemCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> IncludeSections { get; set; } = new();
    public string ReportFormat { get; set; } = "PDF"; // PDF, Excel, CSV
}

/// <summary>
/// OEMトレーサビリティレポート結果DTO
/// </summary>
public class OemTraceabilityReportDto
{
    public string ReportId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
}