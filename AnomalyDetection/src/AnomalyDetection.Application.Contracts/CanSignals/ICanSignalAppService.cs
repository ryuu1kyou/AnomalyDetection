using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.CanSignals;

public interface ICanSignalAppService : IApplicationService
{
    /// <summary>
    /// Get a paginated list of CAN signals with filtering and sorting
    /// </summary>
    Task<PagedResultDto<CanSignalDto>> GetListAsync(GetCanSignalsInput input);
    
    /// <summary>
    /// Get a specific CAN signal by ID
    /// </summary>
    Task<CanSignalDto> GetAsync(Guid id);
    
    /// <summary>
    /// Create a new CAN signal
    /// </summary>
    Task<CanSignalDto> CreateAsync(CreateCanSignalDto input);
    
    /// <summary>
    /// Update an existing CAN signal
    /// </summary>
    Task<CanSignalDto> UpdateAsync(Guid id, UpdateCanSignalDto input);
    
    /// <summary>
    /// Delete a CAN signal
    /// </summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Check if a CAN signal can be deleted (not used by any detection logic)
    /// </summary>
    Task<bool> CanDeleteAsync(Guid id);
    
    /// <summary>
    /// Get CAN signals by system type
    /// </summary>
    Task<ListResultDto<CanSignalDto>> GetBySystemTypeAsync(CanSystemType systemType);
    
    /// <summary>
    /// Get CAN signals by OEM code
    /// </summary>
    Task<ListResultDto<CanSignalDto>> GetByOemCodeAsync(string oemCode);
    
    /// <summary>
    /// Search CAN signals by name or CAN ID
    /// </summary>
    Task<ListResultDto<CanSignalDto>> SearchAsync(string searchTerm);
    
    /// <summary>
    /// Check for CAN ID conflicts
    /// </summary>
    Task<ListResultDto<CanSignalDto>> CheckCanIdConflictsAsync(string canId, Guid? excludeId = null);
    
    /// <summary>
    /// Get compatible CAN signals for a given signal
    /// </summary>
    Task<ListResultDto<CanSignalDto>> GetCompatibleSignalsAsync(Guid signalId);
    
    /// <summary>
    /// Mark a CAN signal as standard
    /// </summary>
    Task MarkAsStandardAsync(Guid id);
    
    /// <summary>
    /// Remove standard status from a CAN signal
    /// </summary>
    Task RemoveStandardStatusAsync(Guid id);
    
    /// <summary>
    /// Activate a CAN signal
    /// </summary>
    Task ActivateAsync(Guid id);
    
    /// <summary>
    /// Deactivate a CAN signal
    /// </summary>
    Task DeactivateAsync(Guid id);
    
    /// <summary>
    /// Deprecate a CAN signal with reason
    /// </summary>
    Task DeprecateAsync(Guid id, string reason);
    
    /// <summary>
    /// Convert raw value to physical value
    /// </summary>
    Task<double> ConvertRawToPhysicalAsync(Guid id, double rawValue);
    
    /// <summary>
    /// Convert physical value to raw value
    /// </summary>
    Task<double> ConvertPhysicalToRawAsync(Guid id, double physicalValue);
    
    /// <summary>
    /// Import CAN signals from file (CSV, DBC, etc.)
    /// </summary>
    Task<ListResultDto<CanSignalDto>> ImportFromFileAsync(byte[] fileContent, string fileName);
    
    /// <summary>
    /// Export CAN signals to file
    /// </summary>
    Task<byte[]> ExportToFileAsync(GetCanSignalsInput input, string format);
}