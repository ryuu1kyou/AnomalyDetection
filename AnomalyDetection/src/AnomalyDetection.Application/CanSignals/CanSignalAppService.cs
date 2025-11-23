using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals.Dtos;
using AnomalyDetection.CanSignals.Mappers;
using AnomalyDetection.CanSpecification;
using AnomalyDetection.Permissions;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.Shared.Export;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.IO;

namespace AnomalyDetection.CanSignals;

// TODO: Re-enable authorization in production
// [Authorize(AnomalyDetectionPermissions.CanSignals.Default)]
[AllowAnonymous]  // Temporarily allow anonymous access for development
public class CanSignalAppService : ApplicationService, ICanSignalAppService
{
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly ICanSpecificationParser _parser;
    private readonly ExportService _exportService;
    private readonly CanSignalMapper _mapper;

    public CanSignalAppService(
        IRepository<CanSignal, Guid> canSignalRepository,
        ICanSpecificationParser parser,
        ExportService exportService,
        CanSignalMapper mapper)
    {
        _canSignalRepository = canSignalRepository;
        _parser = parser;
        _exportService = exportService;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<CanSignalDto>> GetListAsync(GetCanSignalsInput input)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Identifier.SignalName.Contains(input.Filter) ||
                (x.Identifier.CanId != null && x.Identifier.CanId.Contains(input.Filter)) ||
                (x.Description != null && x.Description.Contains(input.Filter)));
        }

        if (!string.IsNullOrEmpty(input.SignalName))
        {
            queryable = queryable.Where(x => x.Identifier.SignalName.Contains(input.SignalName));
        }

        if (!string.IsNullOrEmpty(input.CanId))
        {
            queryable = queryable.Where(x => x.Identifier.CanId != null && x.Identifier.CanId.Contains(input.CanId));
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

        var dtos = items.Select(item => _mapper.Map(item)).ToList();

        return new PagedResultDto<CanSignalDto>(totalCount, dtos);
    }

    public async Task<CanSignalDto> GetAsync(Guid id)
    {
        var canSignal = await _canSignalRepository.GetAsync(id);
        return _mapper.Map(canSignal);
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
        return _mapper.Map(canSignal);
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
        return _mapper.Map(canSignal);
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
        var dtos = signals.Select(item => _mapper.Map(item)).ToList();
        return new ListResultDto<CanSignalDto>(dtos);
    }

    public async Task<ListResultDto<CanSignalDto>> GetByOemCodeAsync(string oemCode)
    {
        var signals = await _canSignalRepository.GetListAsync(x => x.OemCode.Code == oemCode);
        var dtos = signals.Select(item => _mapper.Map(item)).ToList();
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
            (x.Identifier.CanId != null && x.Identifier.CanId.Contains(searchTerm)) ||
            (x.Description != null && x.Description.Contains(searchTerm)));

        var dtos = signals.Select(item => _mapper.Map(item)).ToList();
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
        var dtos = conflicts.Select(item => _mapper.Map(item)).ToList();
        return new ListResultDto<CanSignalDto>(dtos);
    }

    public async Task<ListResultDto<CanSignalDto>> GetCompatibleSignalsAsync(Guid signalId)
    {
        var signal = await _canSignalRepository.GetAsync(signalId);
        var allSignals = await _canSignalRepository.GetListAsync();

        var compatibleSignals = allSignals
            .Where(x => x.Id != signalId && signal.IsCompatibleWith(x))
            .ToList();

        var dtos = compatibleSignals.Select(item => _mapper.Map(item)).ToList();
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
        var format = Path.GetExtension(fileName)?.TrimStart('.').ToUpperInvariant() ?? "UNKNOWN";
        var parseResult = _parser.Parse(fileContent, format);

        var importedSignals = new List<CanSignal>();
        var existingSignals = await _canSignalRepository.GetListAsync();
        var existingMap = existingSignals.ToDictionary(x => x.Identifier.CanId, x => x);

        foreach (var msg in parseResult.Messages)
        {
            foreach (var sig in msg.Signals)
            {
                var uniqueCanId = GenerateUniqueCanId(msg.MessageId, sig.Name);

                if (existingMap.TryGetValue(uniqueCanId, out var existing))
                {
                    // Update
                    var spec = new SignalSpecification(
                        sig.StartBit,
                        sig.BitLength,
                        sig.IsSigned ? SignalDataType.Signed : SignalDataType.Unsigned,
                        new SignalValueRange(sig.Min, sig.Max),
                        sig.IsBigEndian ? SignalByteOrder.BigEndian : SignalByteOrder.LittleEndian
                    );
                    existing.UpdateSpecification(spec, "Imported update");
                    existing.UpdateConversion(new PhysicalValueConversion(sig.Factor, sig.Offset, sig.Unit ?? ""));
                    await _canSignalRepository.UpdateAsync(existing);
                    importedSignals.Add(existing);
                }
                else
                {
                    // Insert
                    var identifier = new SignalIdentifier(sig.Name, uniqueCanId);
                    var spec = new SignalSpecification(
                        sig.StartBit,
                        sig.BitLength,
                        sig.IsSigned ? SignalDataType.Signed : SignalDataType.Unsigned,
                        new SignalValueRange(sig.Min, sig.Max),
                        sig.IsBigEndian ? SignalByteOrder.BigEndian : SignalByteOrder.LittleEndian
                    );

                    var newSignal = new CanSignal(
                        GuidGenerator.Create(),
                        CurrentTenant.Id,
                        identifier,
                        spec,
                        CanSystemType.Body, // Default or infer?
                        new OemCode("GENERIC", "Generic OEM"), // Default
                        $"Imported from {fileName}"
                    );
                    newSignal.UpdateConversion(new PhysicalValueConversion(sig.Factor, sig.Offset, sig.Unit ?? ""));

                    await _canSignalRepository.InsertAsync(newSignal);
                    importedSignals.Add(newSignal);
                }
            }
        }

        return new ListResultDto<CanSignalDto>(importedSignals.Select(item => _mapper.Map(item)).ToList());
    }

    private string GenerateUniqueCanId(uint messageId, string signalName)
    {
        // Construct CanId as "{MessageId}_{SignalName}" to ensure uniqueness if the system requires unique CanId.
        // Using Hex format for MessageId for readability.
        return $"0x{messageId:X}_{signalName}";
    }

    [Authorize(AnomalyDetectionPermissions.CanSignals.Export)]
    public async Task<byte[]> ExportToFileAsync(GetCanSignalsInput input, string format)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();

        // Apply filters (same as GetListAsync)
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Identifier.SignalName.Contains(input.Filter) ||
                (x.Identifier.CanId != null && x.Identifier.CanId.Contains(input.Filter)) ||
                (x.Description != null && x.Description.Contains(input.Filter)));
        }

        if (!string.IsNullOrEmpty(input.SignalName))
        {
            queryable = queryable.Where(x => x.Identifier.SignalName.Contains(input.SignalName));
        }

        if (!string.IsNullOrEmpty(input.CanId))
        {
            queryable = queryable.Where(x => x.Identifier.CanId != null && x.Identifier.CanId.Contains(input.CanId));
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

        var items = await AsyncExecuter.ToListAsync(queryable);
        var dtos = items.Select(item => _mapper.Map(item)).ToList();

        var exportFormat = format?.ToLowerInvariant() == "json" ? ExportService.ExportFormat.Json : ExportService.ExportFormat.Csv;

        var request = new ExportDetectionRequest
        {
            Results = dtos,
            Format = exportFormat,
            FileNamePrefix = "CanSignals",
            GeneratedBy = CurrentUser.UserName ?? "System"
        };

        var result = await _exportService.ExportDetectionResultsAsync(request);
        return result.Data;
    }
}

