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

namespace AnomalyDetection.MultiTenancy;

[Authorize(AnomalyDetectionPermissions.TenantManagement.ManageOemMaster)]
public class OemMasterAppService : ApplicationService, IOemMasterAppService
{
    private readonly IOemMasterRepository _oemMasterRepository;

    public OemMasterAppService(IOemMasterRepository oemMasterRepository)
    {
        _oemMasterRepository = oemMasterRepository;
    }

    public async Task<PagedResultDto<OemMasterDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _oemMasterRepository.GetQueryableAsync();
        var query = queryable.OrderBy(x => x.OemCode.Code);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        return new PagedResultDto<OemMasterDto>
        {
            TotalCount = totalCount,
            Items = ObjectMapper.Map<List<OemMaster>, List<OemMasterDto>>(items)
        };
    }

    public async Task<OemMasterDto> GetAsync(Guid id)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<OemMasterDto> CreateAsync(CreateOemMasterDto input)
    {
        // Check if OEM code already exists
        var existingOem = await _oemMasterRepository.FindByOemCodeAsync(input.OemCode);
        if (existingOem != null)
        {
            throw new BusinessException("OEM code already exists");
        }

        var oemCode = new OemCode(input.OemCode, input.OemName);
        var oemMaster = new OemMaster(
            GuidGenerator.Create(),
            oemCode,
            input.CompanyName,
            input.Country,
            input.ContactEmail,
            input.ContactPhone,
            input.EstablishedDate,
            input.Description
        );

        await _oemMasterRepository.InsertAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<OemMasterDto> UpdateAsync(Guid id, UpdateOemMasterDto input)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        
        oemMaster.UpdateBasicInfo(
            input.CompanyName,
            input.Country,
            input.ContactEmail,
            input.ContactPhone,
            input.Description
        );

        await _oemMasterRepository.UpdateAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _oemMasterRepository.DeleteAsync(id);
    }

    public async Task<OemMasterDto> ActivateAsync(Guid id)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        oemMaster.Activate();
        await _oemMasterRepository.UpdateAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<OemMasterDto> DeactivateAsync(Guid id)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        oemMaster.Deactivate();
        await _oemMasterRepository.UpdateAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<OemMasterDto> AddFeatureAsync(Guid id, CreateOemFeatureDto input)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        oemMaster.AddFeature(input.FeatureName, input.FeatureValue, input.IsEnabled);
        await _oemMasterRepository.UpdateAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<OemMasterDto> UpdateFeatureAsync(Guid id, string featureName, UpdateOemFeatureDto input)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        oemMaster.UpdateFeature(featureName, input.FeatureValue, input.IsEnabled);
        await _oemMasterRepository.UpdateAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<OemMasterDto> RemoveFeatureAsync(Guid id, string featureName)
    {
        var oemMaster = await _oemMasterRepository.GetAsync(id);
        oemMaster.RemoveFeature(featureName);
        await _oemMasterRepository.UpdateAsync(oemMaster);
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }

    public async Task<ListResultDto<OemMasterDto>> GetActiveOemsAsync()
    {
        var activeOems = await _oemMasterRepository.GetActiveOemsAsync();
        return new ListResultDto<OemMasterDto>(
            ObjectMapper.Map<List<OemMaster>, List<OemMasterDto>>(activeOems)
        );
    }

    public async Task<OemMasterDto> GetByOemCodeAsync(string oemCode)
    {
        var oemMaster = await _oemMasterRepository.FindByOemCodeAsync(oemCode);
        if (oemMaster == null)
        {
            throw new EntityNotFoundException(typeof(OemMaster), oemCode);
        }
        return ObjectMapper.Map<OemMaster, OemMasterDto>(oemMaster);
    }
}