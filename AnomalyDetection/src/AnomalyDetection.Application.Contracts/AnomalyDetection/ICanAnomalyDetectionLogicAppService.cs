using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.AnomalyDetection;

public interface ICanAnomalyDetectionLogicAppService : IApplicationService
{
    /// <summary>
    /// Get a paginated list of detection logics with filtering and sorting
    /// </summary>
    Task<PagedResultDto<CanAnomalyDetectionLogicDto>> GetListAsync(GetDetectionLogicsInput input);
    
    /// <summary>
    /// Get a specific detection logic by ID
    /// </summary>
    Task<CanAnomalyDetectionLogicDto> GetAsync(Guid id);
    
    /// <summary>
    /// Create a new detection logic
    /// </summary>
    Task<CanAnomalyDetectionLogicDto> CreateAsync(CreateDetectionLogicDto input);
    
    /// <summary>
    /// Update an existing detection logic
    /// </summary>
    Task<CanAnomalyDetectionLogicDto> UpdateAsync(Guid id, UpdateDetectionLogicDto input);
    
    /// <summary>
    /// Delete a detection logic
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Check if a detection logic can be deleted
    /// </summary>
    Task<bool> CanDeleteAsync(Guid id);
    
    /// <summary>
    /// Get detection logics by detection type
    /// </summary>
    Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByDetectionTypeAsync(DetectionType detectionType);
    
    /// <summary>
    /// Get detection logics by sharing level
    /// </summary>
    Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByShareLevelAsync(SharingLevel sharingLevel);
    
    /// <summary>
    /// Get detection logics by ASIL level
    /// </summary>
    Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByAsilLevelAsync(AsilLevel asilLevel);
    
    /// <summary>
    /// Submit detection logic for approval
    /// </summary>
    Task SubmitForApprovalAsync(Guid id);
    
    /// <summary>
    /// Approve a detection logic
    /// </summary>
    Task ApproveAsync(Guid id, string notes = null);
    
    /// <summary>
    /// Reject a detection logic
    /// </summary>
    Task RejectAsync(Guid id, string reason);
    
    /// <summary>
    /// Deprecate a detection logic
    /// </summary>
    Task DeprecateAsync(Guid id, string reason);
    
    /// <summary>
    /// Update sharing level of a detection logic
    /// </summary>
    Task UpdateSharingLevelAsync(Guid id, SharingLevel sharingLevel);
    
    /// <summary>
    /// Test execution of a detection logic with sample data
    /// </summary>
    Task<Dictionary<string, object>> TestExecutionAsync(Guid id, Dictionary<string, object> testData);
    
    /// <summary>
    /// Validate detection logic implementation
    /// </summary>
    Task<List<string>> ValidateImplementationAsync(Guid id);
    
    /// <summary>
    /// Get execution statistics for a detection logic
    /// </summary>
    Task<Dictionary<string, object>> GetExecutionStatisticsAsync(Guid id);
    
    /// <summary>
    /// Clone a detection logic
    /// </summary>
    Task<CanAnomalyDetectionLogicDto> CloneAsync(Guid id, string newName);
    
    /// <summary>
    /// Create detection logic from template
    /// </summary>
    Task<CanAnomalyDetectionLogicDto> CreateFromTemplateAsync(DetectionType detectionType, Dictionary<string, object> parameters);
    
    /// <summary>
    /// Get available templates for detection type
    /// </summary>
    Task<List<Dictionary<string, object>>> GetTemplatesAsync(DetectionType detectionType);
    
    /// <summary>
    /// Export detection logic to file
    /// </summary>
    Task<byte[]> ExportAsync(Guid id, string format);
    
    /// <summary>
    /// Import detection logic from file
    /// </summary>
    Task<CanAnomalyDetectionLogicDto> ImportAsync(byte[] fileContent, string fileName);
    
    /// <summary>
    /// Get detection logics that use a specific CAN signal
    /// </summary>
    Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByCanSignalAsync(Guid canSignalId);
    
    /// <summary>
    /// Get detection logics by vehicle phase
    /// </summary>
    Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByVehiclePhaseAsync(Guid vehiclePhaseId);
}