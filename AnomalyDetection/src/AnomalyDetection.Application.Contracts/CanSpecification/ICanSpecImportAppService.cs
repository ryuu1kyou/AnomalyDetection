using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.CanSpecification;

public interface ICanSpecImportAppService : IApplicationService
{
    /// <summary>
    /// Import CAN specification file (DBC/XML format)
    /// </summary>
    Task<CanSpecImportResultDto> ImportSpecificationAsync(Stream fileStream, CreateCanSpecImportDto input);

    /// <summary>
    /// Get import history
    /// </summary>
    Task<PagedResultDto<CanSpecImportDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get import details with messages and signals
    /// </summary>
    Task<CanSpecImportDto> GetAsync(Guid id);

    /// <summary>
    /// Get messages from imported specification
    /// </summary>
    Task<PagedResultDto<CanSpecMessageDto>> GetMessagesAsync(Guid specId, PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Compare two specifications and generate diff
    /// </summary>
    Task<CanSpecImportResultDto> ComparSpecificationsAsync(Guid oldSpecId, Guid newSpecId);

    /// <summary>
    /// Get differences for an import
    /// </summary>
    Task<PagedResultDto<CanSpecDiffDto>> GetDiffsAsync(Guid specId, PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get aggregated diff summary for dashboards
    /// </summary>
    Task<CanSpecDiffSummaryDto> GetDiffSummaryAsync(Guid specId);

    /// <summary>
    /// Delete import
    /// </summary>
    Task DeleteAsync(Guid id);
}
