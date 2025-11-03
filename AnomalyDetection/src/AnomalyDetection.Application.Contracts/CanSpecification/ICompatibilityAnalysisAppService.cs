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
}
