using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.AnomalyDetection;

public interface IAnomalyDetectionResultAppService : IApplicationService
{
    /// <summary>
    /// Get a paginated list of detection results with filtering and sorting
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionResultDto>> GetListAsync(GetDetectionResultsInput input);
    
    /// <summary>
    /// Get a specific detection result by ID
    /// </summary>
    Task<AnomalyDetectionResultDto> GetAsync(Guid id);
    
    /// <summary>
    /// Create a new detection result (typically called by detection engine)
    /// </summary>
    Task<AnomalyDetectionResultDto> CreateAsync(CreateDetectionResultDto input);
    
    /// <summary>
    /// Update a detection result
    /// </summary>
    Task<AnomalyDetectionResultDto> UpdateAsync(Guid id, UpdateDetectionResultDto input);
    
    /// <summary>
    /// Delete a detection result
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Get detection results by detection logic
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionResultDto>> GetByDetectionLogicAsync(Guid detectionLogicId, PagedAndSortedResultRequestDto input);
    
    /// <summary>
    /// Get detection results by CAN signal
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionResultDto>> GetByCanSignalAsync(Guid canSignalId, PagedAndSortedResultRequestDto input);
    
    /// <summary>
    /// Get detection results by anomaly level
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionResultDto>> GetByAnomalyLevelAsync(AnomalyLevel anomalyLevel, PagedAndSortedResultRequestDto input);
    
    /// <summary>
    /// Get detection results by resolution status
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionResultDto>> GetByResolutionStatusAsync(ResolutionStatus status, PagedAndSortedResultRequestDto input);
    
    /// <summary>
    /// Get recent detection results
    /// </summary>
    Task<ListResultDto<AnomalyDetectionResultDto>> GetRecentAsync(int count = 10);
    
    /// <summary>
    /// Get high priority detection results (Critical and Fatal)
    /// </summary>
    Task<ListResultDto<AnomalyDetectionResultDto>> GetHighPriorityAsync();
    
    /// <summary>
    /// Mark detection result as investigating
    /// </summary>
    Task MarkAsInvestigatingAsync(Guid id, string notes = null);
    
    /// <summary>
    /// Mark detection result as false positive
    /// </summary>
    Task MarkAsFalsePositiveAsync(Guid id, MarkAsFalsePositiveDto input);
    
    /// <summary>
    /// Resolve detection result
    /// </summary>
    Task ResolveAsync(Guid id, ResolveDetectionResultDto input);
    
    /// <summary>
    /// Reopen detection result
    /// </summary>
    Task ReopenAsync(Guid id, ReopenDetectionResultDto input);
    
    /// <summary>
    /// Share detection result
    /// </summary>
    Task ShareResultAsync(Guid id, ShareDetectionResultDto input);
    
    /// <summary>
    /// Revoke sharing of detection result
    /// </summary>
    Task RevokeSharingAsync(Guid id);
    
    /// <summary>
    /// Get shared detection results from other OEMs
    /// </summary>
    Task<PagedResultDto<AnomalyDetectionResultDto>> GetSharedResultsAsync(GetDetectionResultsInput input);
    
    /// <summary>
    /// Bulk update resolution status
    /// </summary>
    Task BulkUpdateResolutionStatusAsync(List<Guid> ids, ResolutionStatus status, string notes = null);
    
    /// <summary>
    /// Bulk mark as false positive
    /// </summary>
    Task BulkMarkAsFalsePositiveAsync(List<Guid> ids, string reason);
    
    /// <summary>
    /// Get detection result statistics
    /// </summary>
    Task<Dictionary<string, object>> GetStatisticsAsync(GetDetectionResultsInput input);
    
    /// <summary>
    /// Export detection results to file
    /// </summary>
    Task<byte[]> ExportAsync(GetDetectionResultsInput input, string format);
    
    /// <summary>
    /// Get detection results timeline
    /// </summary>
    Task<List<Dictionary<string, object>>> GetTimelineAsync(Guid? canSignalId = null, Guid? detectionLogicId = null, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Get correlation analysis between detection results
    /// </summary>
    Task<Dictionary<string, object>> GetCorrelationAnalysisAsync(List<Guid> resultIds);
    
    /// <summary>
    /// Get similar detection results
    /// </summary>
    Task<ListResultDto<AnomalyDetectionResultDto>> GetSimilarResultsAsync(Guid id, int count = 5);
}