using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.MultiTenancy.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.MultiTenancy;

public interface IExtendedTenantAppService : IApplicationService
{
    Task<PagedResultDto<ExtendedTenantDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ExtendedTenantDto> GetAsync(Guid id);
    Task<ExtendedTenantDto> CreateAsync(CreateExtendedTenantDto input);
    Task<ExtendedTenantDto> UpdateAsync(Guid id, UpdateExtendedTenantDto input);
    Task DeleteAsync(Guid id);
    Task<ExtendedTenantDto> ActivateAsync(Guid id);
    Task<ExtendedTenantDto> DeactivateAsync(Guid id);
    
    // Feature management
    Task<ExtendedTenantDto> AddFeatureAsync(Guid id, CreateTenantFeatureDto input);
    Task<ExtendedTenantDto> UpdateFeatureAsync(Guid id, string featureName, UpdateTenantFeatureDto input);
    Task<ExtendedTenantDto> RemoveFeatureAsync(Guid id, string featureName);
    
    // Tenant switching
    Task<List<TenantSwitchDto>> GetAvailableTenantsAsync();
    Task<TenantSwitchDto> GetCurrentTenantAsync();
    Task SwitchTenantAsync(Guid? tenantId);
    
    // Lookup methods
    Task<ListResultDto<ExtendedTenantDto>> GetActiveTenantsAsync();
    Task<ExtendedTenantDto> GetByNameAsync(string name);
}