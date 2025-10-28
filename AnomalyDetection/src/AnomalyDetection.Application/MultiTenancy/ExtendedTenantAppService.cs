using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.MultiTenancy.Dtos;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.MultiTenancy;

[Authorize(AnomalyDetectionPermissions.TenantManagement.Default)]
public class ExtendedTenantAppService : ApplicationService, IExtendedTenantAppService
{
    private readonly IExtendedTenantRepository _extendedTenantRepository;
    private readonly IOemMasterRepository _oemMasterRepository;
    private readonly ICurrentTenant _currentTenant;

    public ExtendedTenantAppService(
        IExtendedTenantRepository extendedTenantRepository,
        IOemMasterRepository oemMasterRepository,
        ICurrentTenant currentTenant)
    {
        _extendedTenantRepository = extendedTenantRepository;
        _oemMasterRepository = oemMasterRepository;
        _currentTenant = currentTenant;
    }

    public async Task<PagedResultDto<ExtendedTenantDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _extendedTenantRepository.GetQueryableAsync();
        var query = queryable.OrderBy(x => x.Name);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var dtos = ObjectMapper.Map<List<ExtendedTenant>, List<ExtendedTenantDto>>(items);
        
        // Set additional properties
        foreach (var dto in dtos)
        {
            var tenant = items.First(x => x.Id == dto.Id);
            dto.IsExpired = tenant.IsExpired();
            dto.IsValidForUse = tenant.IsValidForUse();
        }

        return new PagedResultDto<ExtendedTenantDto>
        {
            TotalCount = totalCount,
            Items = dtos
        };
    }

    public async Task<ExtendedTenantDto> GetAsync(Guid id)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    [Authorize(AnomalyDetectionPermissions.TenantManagement.ManageTenantFeatures)]
    public async Task<ExtendedTenantDto> CreateAsync(CreateExtendedTenantDto input)
    {
        // Check if tenant name already exists
        var existingTenant = await _extendedTenantRepository.FindByNameAsync(input.Name);
        if (existingTenant != null)
        {
            throw new BusinessException("Tenant name already exists");
        }

        // Validate OEM Master if provided
        OemMaster oemMaster = null;
        if (input.OemMasterId.HasValue)
        {
            oemMaster = await _oemMasterRepository.GetAsync(input.OemMasterId.Value);
        }

        var oemCode = oemMaster?.OemCode ?? new OemCode(input.OemCode, input.OemName);
        
        var tenant = new ExtendedTenant(
            GuidGenerator.Create(),
            input.Name,
            oemCode,
            input.OemMasterId,
            input.DatabaseConnectionString,
            input.Description
        );

        if (input.ExpirationDate.HasValue)
        {
            tenant.SetExpiration(input.ExpirationDate);
        }

        await _extendedTenantRepository.InsertAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    [Authorize(AnomalyDetectionPermissions.TenantManagement.ManageTenantFeatures)]
    public async Task<ExtendedTenantDto> UpdateAsync(Guid id, UpdateExtendedTenantDto input)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        
        // Check if new name conflicts with existing tenant
        if (tenant.Name != input.Name)
        {
            var existingTenant = await _extendedTenantRepository.FindByNameAsync(input.Name);
            if (existingTenant != null && existingTenant.Id != id)
            {
                throw new BusinessException("Tenant name already exists");
            }
        }

        tenant.UpdateBasicInfo(input.Name, input.Description);
        tenant.SetExpiration(input.ExpirationDate);

        await _extendedTenantRepository.UpdateAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    [Authorize(AnomalyDetectionPermissions.TenantManagement.ManageTenantFeatures)]
    public async Task DeleteAsync(Guid id)
    {
        await _extendedTenantRepository.DeleteAsync(id);
    }

    public async Task<ExtendedTenantDto> ActivateAsync(Guid id)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        tenant.Activate();
        await _extendedTenantRepository.UpdateAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    public async Task<ExtendedTenantDto> DeactivateAsync(Guid id)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        tenant.Deactivate();
        await _extendedTenantRepository.UpdateAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    public async Task<ExtendedTenantDto> AddFeatureAsync(Guid id, CreateTenantFeatureDto input)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        tenant.AddFeature(input.FeatureName, input.FeatureValue, input.IsEnabled);
        await _extendedTenantRepository.UpdateAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    public async Task<ExtendedTenantDto> UpdateFeatureAsync(Guid id, string featureName, UpdateTenantFeatureDto input)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        tenant.UpdateFeature(featureName, input.FeatureValue, input.IsEnabled);
        await _extendedTenantRepository.UpdateAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    public async Task<ExtendedTenantDto> RemoveFeatureAsync(Guid id, string featureName)
    {
        var tenant = await _extendedTenantRepository.GetAsync(id);
        tenant.RemoveFeature(featureName);
        await _extendedTenantRepository.UpdateAsync(tenant);
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }

    public async Task<List<TenantSwitchDto>> GetAvailableTenantsAsync()
    {
        var activeTenants = await _extendedTenantRepository.GetActiveTenantsAsync();
        var validTenants = activeTenants.Where(t => t.IsValidForUse()).ToList();
        
        var result = new List<TenantSwitchDto>();
        
        // Add host tenant (null tenant)
        result.Add(new TenantSwitchDto
        {
            TenantId = null,
            TenantName = "Host",
            OemCode = "HOST"
        });
        
        // Add valid tenants
        result.AddRange(validTenants.Select(t => new TenantSwitchDto
        {
            TenantId = t.Id,
            TenantName = t.Name,
            OemCode = t.OemCode.Code
        }));
        
        return result;
    }

    public async Task<TenantSwitchDto> GetCurrentTenantAsync()
    {
        if (_currentTenant.Id == null)
        {
            return new TenantSwitchDto
            {
                TenantId = null,
                TenantName = "Host",
                OemCode = "HOST"
            };
        }

        var tenant = await _extendedTenantRepository.GetAsync(_currentTenant.Id.Value);
        return new TenantSwitchDto
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            OemCode = tenant.OemCode.Code
        };
    }

    [Authorize(AnomalyDetectionPermissions.TenantManagement.SwitchTenant)]
    public async Task SwitchTenantAsync(Guid? tenantId)
    {
        // This method would typically work with a tenant switching service
        // For now, we'll just validate the tenant exists and is valid
        if (tenantId.HasValue)
        {
            var tenant = await _extendedTenantRepository.GetAsync(tenantId.Value);
            if (!tenant.IsValidForUse())
            {
                throw new BusinessException("Cannot switch to inactive or expired tenant");
            }
        }
        
        // The actual tenant switching would be handled by the frontend
        // or a specialized tenant switching service
    }

    public async Task<ListResultDto<ExtendedTenantDto>> GetActiveTenantsAsync()
    {
        var activeTenants = await _extendedTenantRepository.GetActiveTenantsAsync();
        var validTenants = activeTenants.Where(t => t.IsValidForUse()).ToList();
        
        var dtos = ObjectMapper.Map<List<ExtendedTenant>, List<ExtendedTenantDto>>(validTenants);
        
        foreach (var dto in dtos)
        {
            var tenant = validTenants.First(x => x.Id == dto.Id);
            dto.IsExpired = tenant.IsExpired();
            dto.IsValidForUse = tenant.IsValidForUse();
        }
        
        return new ListResultDto<ExtendedTenantDto>(dtos);
    }

    public async Task<ExtendedTenantDto> GetByNameAsync(string name)
    {
        var tenant = await _extendedTenantRepository.FindByNameAsync(name);
        if (tenant == null)
        {
            throw new EntityNotFoundException(typeof(ExtendedTenant), name);
        }
        
        var dto = ObjectMapper.Map<ExtendedTenant, ExtendedTenantDto>(tenant);
        dto.IsExpired = tenant.IsExpired();
        dto.IsValidForUse = tenant.IsValidForUse();
        return dto;
    }
}