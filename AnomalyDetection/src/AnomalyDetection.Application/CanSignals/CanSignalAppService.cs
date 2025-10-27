using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals.Dtos;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.CanSignals;

[Authorize(AnomalyDetectionPermissions.CanSignals.Default)]
public class CanSignalAppService : ApplicationService, ICanSignalAppService
{
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;

    public CanSignalAppService(IRepository<CanSignal, Guid> canSignalRepository)
    {
        _canSignalRepository = canSignalRepository;
    }

    public async Task<PagedResultDto<CanSignalDto>> GetListAsync(GetCanSignalsInput input)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(x => 
                x.Identifier.SignalName.Contains(input.Filter) ||
                x.Identifier.CanId.Contains(input.Filter) ||
                x.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrEmpty(input.SignalName))
        {
            queryable = queryable.Where(x => x.Identifier.SignalName.Contains(input.SignalName));
        }

        if (!string.IsNullOrEmpty(input.CanId))
        {
            queryable = queryable.Where(x => x.Identifier.CanId.Contains(input.CanId));
        }

        if (input.SystemType.HasValue)
        {
            queryable = queryable.Where(x => x.SystemType == input.SystemType.Value);
        }

        if (input.OemCode != null)
        {
            queryable = queryable.Where(x => x.OemCode.Equals(input.OemCode));
        }

        if (input.IsStandard.HasValue)
        {
            queryable = queryable.Where(x => x.IsStandard == input.IsStandard.Value);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.EffectiveDateFrom.HasValue)
        {
            queryable = queryable.Where(x => x.EffectiveDate >= input.EffectiveDateFrom.Value);
        }

        if (input.EffectiveDateTo.HasValue)
        {
            queryable = queryable.Where(x => x.EffectiveDate <= input.EffectiveDateTo.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderBy(x => x.Identifier.SignalName);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<CanSignal>, List<CanSignalDto>>(items);

        return new PagedResultDto<CanSignalDto>(totalCount, dtos);
    }

    public async Task<CanSignalDto> GetAsync(Guid id)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        return ObjectMapper.Map<CanSignal, CanSignalDto>(canSignal);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Create)]
    public async Task<CanSignalDto> CreateAsync(CreateCanSignalDto input)
    {
        // Check for CAN ID conflicts
        var existingSignal = await _canSignalRepository.FirstOrDefaultAsync(x => 
            x.Identifier.CanId == input.CanId && x.TenantId == CurrentTenant.Id);
        
        if (existingSignal != null)
        {
            throw new BusinessException("CanSignal:DuplicateCanId")
                .WithData("CanId", input.CanId);
        }

        // Create the signal manually to pass TenantId
        var identifier = new SignalIdentifier(input.SignalName, input.CanId);
        var valueRange = new SignalValueRange(input.MinValue, input.MaxValue);
        var specification = new SignalSpecification(input.StartBit, input.Length, input.DataType, valueRange, input.ByteOrder);
        
        var canSignal = new CanSignal(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            identifier,
            specification,
            input.SystemType,
            input.OemCode,
            input.Description);
            
        var conversion = new PhysicalValueConversion(input.Factor, input.Offset, input.Unit);
        var timing = new SignalTiming(input.CycleTime, input.TimeoutTime, SignalSendType.Cyclic);
        
        canSignal.UpdateConversion(conversion);
        canSignal.UpdateTiming(timing);
        
        if (input.IsStandard)
            canSignal.SetAsStandard();
            
        if (!string.IsNullOrEmpty(input.SourceDocument))
            canSignal.SetSourceDocument(input.SourceDocument);
            
        if (!string.IsNullOrEmpty(input.Notes))
            canSignal.AddNote(input.Notes);

        canSignal = await _canSignalRepository.InsertAsync(canSignal, autoSave: true);
        return ObjectMapper.Map<CanSignal, CanSignalDto>(canSignal);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Edit)]
    public async Task<CanSignalDto> UpdateAsync(Guid id, UpdateCanSignalDto input)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);

        // Check for CAN ID conflicts (excluding current signal)
        var existingSignal = await _canSignalRepository.FirstOrDefaultAsync(x => 
            x.Identifier.CanId == input.CanId && x.Id != id && x.TenantId == CurrentTenant.Id);
        
        if (existingSignal != null)
        {
            throw new BusinessException("CanSignal:DuplicateCanId")
                .WithData("CanId", input.CanId);
        }

        // Update signal properties
        var identifier = new SignalIdentifier(input.SignalName, input.CanId);
        var valueRange = new SignalValueRange(input.MinValue, input.MaxValue);
        var specification = new SignalSpecification(
            input.StartBit, input.Length, input.DataType, valueRange, input.ByteOrder);
        var conversion = new PhysicalValueConversion(input.Factor, input.Offset, input.Unit);
        var timing = new SignalTiming(input.CycleTime, input.TimeoutTime);

        canSignal.UpdateSpecification(specification, input.ChangeReason);
        canSignal.UpdateConversion(conversion);
        canSignal.UpdateTiming(timing);
        canSignal.UpdateDescription(input.Description);

        if (input.IsStandard && !canSignal.IsStandard)
        {
            canSignal.SetAsStandard();
        }
        else if (!input.IsStandard && canSignal.IsStandard)
        {
            canSignal.RemoveStandardStatus();
        }

        if (!string.IsNullOrEmpty(input.SourceDocument))
        {
            canSignal.SetSourceDocument(input.SourceDocument);
        }

        if (!string.IsNullOrEmpty(input.Notes))
        {
            canSignal.AddNote(input.Notes);
        }

        canSignal = await _canSignalRepository.UpdateAsync(canSignal, autoSave: true);
        return ObjectMapper.Map<CanSignal, CanSignalDto>(canSignal);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var canDelete = await CanDeleteAsync(id);
        if (!canDelete)
        {
            throw new BusinessException("CanSignal:CannotDelete")
                .WithData("Id", id);
        }

        await _canSignalRepository.DeleteAsync(id);
    }

    public async Task<bool> CanDeleteAsync(Guid id)
    {
        // Check if signal is used by any detection logic
        // This would require checking the detection logic repository
        // For now, return true - implement proper check when detection logic repository is available
        return await Task.FromResult(true);
    }

    public async Task<ListResultDto<CanSignalDto>> GetBySystemTypeAsync(CanSystemType systemType)
    {
        var signals = await _canSignalRepository.GetListAsync(x => x.SystemType == systemType);
        var dtos = ObjectMapper.Map<List<CanSignal>, List<CanSignalDto>>(signals);
        return new ListResultDto<CanSignalDto>(dtos);
    }

    public async Task<ListResultDto<CanSignalDto>> GetByOemCodeAsync(string oemCode)
    {
        var signals = await _canSignalRepository.GetListAsync(x => x.OemCode.Code == oemCode);
        var dtos = ObjectMapper.Map<List<CanSignal>, List<CanSignalDto>>(signals);
        return new ListResultDto<CanSignalDto>(dtos);
    }

    public async Task<ListResultDto<CanSignalDto>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new ListResultDto<CanSignalDto>(new List<CanSignalDto>());
        }

        var signals = await _canSignalRepository.GetListAsync(x => 
            x.Identifier.SignalName.Contains(searchTerm) ||
            x.Identifier.CanId.Contains(searchTerm) ||
            x.Description.Contains(searchTerm));

        var dtos = ObjectMapper.Map<List<CanSignal>, List<CanSignalDto>>(signals);
        return new ListResultDto<CanSignalDto>(dtos);
    }

    public async Task<ListResultDto<CanSignalDto>> CheckCanIdConflictsAsync(string canId, Guid? excludeId = null)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.Identifier.CanId == canId);

        if (excludeId.HasValue)
        {
            queryable = queryable.Where(x => x.Id != excludeId.Value);
        }

        var conflicts = await AsyncExecuter.ToListAsync(queryable);
        var dtos = ObjectMapper.Map<List<CanSignal>, List<CanSignalDto>>(conflicts);
        return new ListResultDto<CanSignalDto>(dtos);
    }

    public async Task<ListResultDto<CanSignalDto>> GetCompatibleSignalsAsync(Guid signalId)
    {
        var signal = await _canSignalRepository.GetAsync(signalId);
        var allSignals = await _canSignalRepository.GetListAsync();
        
        var compatibleSignals = allSignals
            .Where(x => x.Id != signalId && signal.IsCompatibleWith(x))
            .ToList();

        var dtos = ObjectMapper.Map<List<CanSignal>, List<CanSignalDto>>(compatibleSignals);
        return new ListResultDto<CanSignalDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.ManageStandard)]
    public async Task MarkAsStandardAsync(Guid id)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        canSignal.SetAsStandard();
        await _canSignalRepository.UpdateAsync(canSignal, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.ManageStandard)]
    public async Task RemoveStandardStatusAsync(Guid id)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        canSignal.RemoveStandardStatus();
        await _canSignalRepository.UpdateAsync(canSignal, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Edit)]
    public async Task ActivateAsync(Guid id)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        canSignal.Activate();
        await _canSignalRepository.UpdateAsync(canSignal, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Edit)]
    public async Task DeactivateAsync(Guid id)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        canSignal.Deactivate();
        await _canSignalRepository.UpdateAsync(canSignal, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Edit)]
    public async Task DeprecateAsync(Guid id, string reason)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        canSignal.Deprecate(reason);
        await _canSignalRepository.UpdateAsync(canSignal, autoSave: true);
    }

    public async Task<double> ConvertRawToPhysicalAsync(Guid id, double rawValue)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        return canSignal.ConvertRawToPhysical(rawValue);
    }

    public async Task<double> ConvertPhysicalToRawAsync(Guid id, double physicalValue)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        return canSignal.ConvertPhysicalToRaw(physicalValue);
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Import)]
    public async Task<ListResultDto<CanSignalDto>> ImportFromFileAsync(byte[] fileContent, string fileName)
    {
        // TODO: Implement file import logic (CSV, DBC, etc.)
        throw new NotImplementedException("File import functionality will be implemented in a future version");
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Export)]
    public async Task<byte[]> ExportToFileAsync(GetCanSignalsInput input, string format)
    {
        // TODO: Implement file export logic
        throw new NotImplementedException("File export functionality will be implemented in a future version");
    }
}