using System;
using System.Threading.Tasks;
using AnomalyDetection.MultiTenancy.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.MultiTenancy;

public interface IOemMasterAppService : IApplicationService
{
    Task<PagedResultDto<OemMasterDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<OemMasterDto> GetAsync(Guid id);
    Task<OemMasterDto> CreateAsync(CreateOemMasterDto input);
    Task<OemMasterDto> UpdateAsync(Guid id, UpdateOemMasterDto input);
    Task DeleteAsync(Guid id);
    Task<OemMasterDto> ActivateAsync(Guid id);
    Task<OemMasterDto> DeactivateAsync(Guid id);
    
    // Feature management
    Task<OemMasterDto> AddFeatureAsync(Guid id, CreateOemFeatureDto input);
    Task<OemMasterDto> UpdateFeatureAsync(Guid id, string featureName, UpdateOemFeatureDto input);
    Task<OemMasterDto> RemoveFeatureAsync(Guid id, string featureName);
    
    // Lookup methods
    Task<ListResultDto<OemMasterDto>> GetActiveOemsAsync();
    Task<OemMasterDto> GetByOemCodeAsync(string oemCode);
}