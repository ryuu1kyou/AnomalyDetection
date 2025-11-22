using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.CanSpecification;

public interface ICompatibilityAnalysisAppService : IApplicationService
{
    /// <summary>
    /// Analyze compatibility between two CAN specifications
    /// </summary>
    Task<CompatibilityAnalysisResultDto> AnalyzeCompatibilityAsync(CreateCompatibilityAnalysisDto input);

    /// <summary>
    /// Quickly assess compatibility for a specific context using cached summaries
    /// </summary>
    Task<CompatibilityStatusDto> AssessCompatibilityAsync(CompatibilityAssessmentRequestDto input);

    /// <summary>
    /// Get compatibility analysis history
    /// </summary>
    Task<PagedResultDto<CompatibilityAnalysisDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get detailed analysis result
    /// </summary>
    Task<CompatibilityAnalysisDto> GetAsync(Guid id);

    /// <summary>
    /// Get issues by severity
    /// </summary>
    Task<PagedResultDto<CompatibilityIssueDto>> GetIssuesBySeverityAsync(
        Guid analysisId,
        int severity,
        PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get impact assessments
    /// </summary>
    Task<PagedResultDto<ImpactAssessmentDto>> GetImpactsAsync(
        Guid analysisId,
        PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Delete analysis
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Export compatibility analysis details (issues & impacts)
    /// </summary>
    Task<ExportedFileDto> ExportAsync(CompatibilityAnalysisExportDto input);
}

public class CompatibilityAnalysisExportDto
{
    public Guid AnalysisId { get; set; }
    public string Format { get; set; } = "csv"; // csv|json|pdf|excel
    public bool IncludeIssues { get; set; } = true;
    public bool IncludeImpacts { get; set; } = true;
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
