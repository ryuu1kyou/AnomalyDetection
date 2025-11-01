using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.AnomalyDetection;

[Authorize(AnomalyDetectionPermissions.DetectionLogics.Default)]
public class CanAnomalyDetectionLogicAppService : ApplicationService, ICanAnomalyDetectionLogicAppService
{
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;

    public CanAnomalyDetectionLogicAppService(
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository)
    {
        _detectionLogicRepository = detectionLogicRepository;
    }

    public async Task<PagedResultDto<CanAnomalyDetectionLogicDto>> GetListAsync(GetDetectionLogicsInput input)
    {
        var queryable = await _detectionLogicRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Identity.Name.Contains(input.Filter) ||
                x.Specification.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrEmpty(input.Name))
        {
            queryable = queryable.Where(x => x.Identity.Name.Contains(input.Name));
        }

        if (input.DetectionType.HasValue)
        {
            // Map DetectionType to AnomalyType for comparison
            var anomalyType = (AnomalyType)(int)input.DetectionType.Value;
            queryable = queryable.Where(x => x.Specification.DetectionType == anomalyType);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.SharingLevel.HasValue)
        {
            queryable = queryable.Where(x => x.SharingLevel == input.SharingLevel.Value);
        }

        if (input.AsilLevel.HasValue)
        {
            queryable = queryable.Where(x => x.Safety.AsilLevel == input.AsilLevel.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderBy(x => x.Identity.Name);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(items);

        return new PagedResultDto<CanAnomalyDetectionLogicDto>(totalCount, dtos);
    }

    public async Task<CanAnomalyDetectionLogicDto> GetAsync(Guid id)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(logic);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Create)]
    public async Task<CanAnomalyDetectionLogicDto> CreateAsync(CreateDetectionLogicDto input)
    {
        var logic = ObjectMapper.Map<CreateDetectionLogicDto, CanAnomalyDetectionLogic>(input);

        logic = await _detectionLogicRepository.InsertAsync(logic, autoSave: true);
        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(logic);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Edit)]
    public async Task<CanAnomalyDetectionLogicDto> UpdateAsync(Guid id, UpdateDetectionLogicDto input)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);

        // Update logic properties based on input
        // This is a simplified implementation

        logic = await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(logic);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _detectionLogicRepository.DeleteAsync(id);
    }

    public async Task<bool> CanDeleteAsync(Guid id)
    {
        // Check if logic is used by any detection results
        return await Task.FromResult(true);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByDetectionTypeAsync(DetectionType detectionType)
    {
        // Map DetectionType to AnomalyType
        var anomalyType = (AnomalyType)(int)detectionType;
        var logics = await _detectionLogicRepository.GetListAsync(x => x.Specification.DetectionType == anomalyType);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByShareLevelAsync(SharingLevel sharingLevel)
    {
        var logics = await _detectionLogicRepository.GetListAsync(x => x.SharingLevel == sharingLevel);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByAsilLevelAsync(AsilLevel asilLevel)
    {
        var logics = await _detectionLogicRepository.GetListAsync(x => x.Safety.AsilLevel == asilLevel);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Edit)]
    public async Task SubmitForApprovalAsync(Guid id)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.SubmitForApproval();
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Approve)]
    public async Task ApproveAsync(Guid id, string? notes = null)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.Approve(CurrentUser.Id ?? Guid.Empty, notes ?? string.Empty);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Approve)]
    public async Task RejectAsync(Guid id, string reason)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.Reject(reason);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Edit)]
    public async Task DeprecateAsync(Guid id, string reason)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.Deprecate(reason);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.ManageSharing)]
    public async Task UpdateSharingLevelAsync(Guid id, SharingLevel sharingLevel)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.UpdateSharingLevel(sharingLevel);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Execute)]
    public async Task<Dictionary<string, object>> TestExecutionAsync(Guid id, Dictionary<string, object> testData)
    {
        // TODO: Implement test execution logic
        return await Task.FromResult(new Dictionary<string, object>());
    }

    public Task<List<string>> ValidateImplementationAsync(Guid id)
    {
        // TODO: Implement validation logic
        return Task.FromResult(new List<string>());
    }

    public async Task<Dictionary<string, object>> GetExecutionStatisticsAsync(Guid id)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        return new Dictionary<string, object>
        {
            ["ExecutionCount"] = logic.ExecutionCount,
            ["LastExecutedAt"] = logic.LastExecutedAt as object ?? DBNull.Value,
            ["AverageExecutionTime"] = logic.GetAverageExecutionTime()
        };
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Create)]
    public Task<CanAnomalyDetectionLogicDto> CloneAsync(Guid id, string newName)
    {
        // TODO: Implement clone logic
        throw new NotImplementedException();
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.ManageTemplates)]
    public Task<CanAnomalyDetectionLogicDto> CreateFromTemplateAsync(DetectionType detectionType, Dictionary<string, object> parameters)
    {
        // TODO: Implement template creation logic
        throw new NotImplementedException();
    }

    public Task<List<Dictionary<string, object>>> GetTemplatesAsync(DetectionType detectionType)
    {
        // TODO: Implement template retrieval logic
        return Task.FromResult(new List<Dictionary<string, object>>());
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Export)]
    public Task<byte[]> ExportAsync(Guid id, string format)
    {
        // TODO: Implement export logic
        throw new NotImplementedException();
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Import)]
    public Task<CanAnomalyDetectionLogicDto> ImportAsync(byte[] fileContent, string fileName)
    {
        // TODO: Implement import logic
        throw new NotImplementedException();
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByCanSignalAsync(Guid canSignalId)
    {
        var queryable = await _detectionLogicRepository.GetQueryableAsync();
        var logics = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.SignalMappings.Any(m => m.CanSignalId == canSignalId)));

        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByVehiclePhaseAsync(Guid vehiclePhaseId)
    {
        var logics = await _detectionLogicRepository.GetListAsync(x => x.VehiclePhaseId == vehiclePhaseId);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }
}