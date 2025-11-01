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

[Authorize(AnomalyDetectionPermissions.DetectionResults.Default)]
public class AnomalyDetectionResultAppService : ApplicationService, IAnomalyDetectionResultAppService
{
    private readonly IRepository<AnomalyDetectionResult, Guid> _resultRepository;

    public AnomalyDetectionResultAppService(
        IRepository<AnomalyDetectionResult, Guid> resultRepository)
    {
        _resultRepository = resultRepository;
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.View)]
    public async Task<PagedResultDto<AnomalyDetectionResultDto>> GetListAsync(GetDetectionResultsInput input)
    {
        var queryable = await _resultRepository.GetQueryableAsync();

        // Apply filters
        if (input.DetectionLogicId.HasValue)
        {
            queryable = queryable.Where(x => x.DetectionLogicId == input.DetectionLogicId.Value);
        }

        if (input.CanSignalId.HasValue)
        {
            queryable = queryable.Where(x => x.CanSignalId == input.CanSignalId.Value);
        }

        if (input.AnomalyLevel.HasValue)
        {
            queryable = queryable.Where(x => x.AnomalyLevel == input.AnomalyLevel.Value);
        }

        if (input.ResolutionStatus.HasValue)
        {
            queryable = queryable.Where(x => x.ResolutionStatus == input.ResolutionStatus.Value);
        }

        if (input.DetectedFrom.HasValue)
        {
            queryable = queryable.Where(x => x.DetectedAt >= input.DetectedFrom.Value);
        }

        if (input.DetectedTo.HasValue)
        {
            queryable = queryable.Where(x => x.DetectedAt <= input.DetectedTo.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderByDescending(x => x.DetectedAt);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);

        return new PagedResultDto<AnomalyDetectionResultDto>(totalCount, dtos);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.View)]
    public async Task<AnomalyDetectionResultDto> GetAsync(Guid id)
    {
        var result = await _resultRepository.GetAsync(id);
        return ObjectMapper.Map<AnomalyDetectionResult, AnomalyDetectionResultDto>(result);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Create)]
    public async Task<AnomalyDetectionResultDto> CreateAsync(CreateDetectionResultDto input)
    {
        var result = ObjectMapper.Map<CreateDetectionResultDto, AnomalyDetectionResult>(input);

        result = await _resultRepository.InsertAsync(result, autoSave: true);
        return ObjectMapper.Map<AnomalyDetectionResult, AnomalyDetectionResultDto>(result);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Edit)]
    public async Task<AnomalyDetectionResultDto> UpdateAsync(Guid id, UpdateDetectionResultDto input)
    {
        var result = await _resultRepository.GetAsync(id);

        result.UpdateAnomalyLevel(input.AnomalyLevel, input.UpdateReason);
        result.UpdateConfidenceScore(input.ConfidenceScore, input.UpdateReason);

        result = await _resultRepository.UpdateAsync(result, autoSave: true);
        return ObjectMapper.Map<AnomalyDetectionResult, AnomalyDetectionResultDto>(result);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _resultRepository.DeleteAsync(id);
    }

    public async Task<PagedResultDto<AnomalyDetectionResultDto>> GetByDetectionLogicAsync(Guid detectionLogicId, PagedAndSortedResultRequestDto input)
    {
        var queryable = await _resultRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.DetectionLogicId == detectionLogicId);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);
        return new PagedResultDto<AnomalyDetectionResultDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<AnomalyDetectionResultDto>> GetByCanSignalAsync(Guid canSignalId, PagedAndSortedResultRequestDto input)
    {
        var queryable = await _resultRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.CanSignalId == canSignalId);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);
        return new PagedResultDto<AnomalyDetectionResultDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<AnomalyDetectionResultDto>> GetByAnomalyLevelAsync(AnomalyLevel anomalyLevel, PagedAndSortedResultRequestDto input)
    {
        var queryable = await _resultRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.AnomalyLevel == anomalyLevel);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);
        return new PagedResultDto<AnomalyDetectionResultDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<AnomalyDetectionResultDto>> GetByResolutionStatusAsync(ResolutionStatus status, PagedAndSortedResultRequestDto input)
    {
        var queryable = await _resultRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.ResolutionStatus == status);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);
        return new PagedResultDto<AnomalyDetectionResultDto>(totalCount, dtos);
    }

    public async Task<ListResultDto<AnomalyDetectionResultDto>> GetRecentAsync(int count = 10)
    {
        var queryable = await _resultRepository.GetQueryableAsync();
        var items = await AsyncExecuter.ToListAsync(
            queryable.OrderByDescending(x => x.DetectedAt).Take(count));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);
        return new ListResultDto<AnomalyDetectionResultDto>(dtos);
    }

    public async Task<ListResultDto<AnomalyDetectionResultDto>> GetHighPriorityAsync()
    {
        var results = await _resultRepository.GetListAsync(x =>
            x.AnomalyLevel >= AnomalyLevel.Critical &&
            x.ResolutionStatus == ResolutionStatus.Open);

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(results);
        return new ListResultDto<AnomalyDetectionResultDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Resolve)]
    public async Task MarkAsInvestigatingAsync(Guid id, string? notes = null)
    {
        var result = await _resultRepository.GetAsync(id);
        result.MarkAsInvestigating(CurrentUser.Id ?? Guid.Empty, notes);
        await _resultRepository.UpdateAsync(result, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Resolve)]
    public async Task MarkAsFalsePositiveAsync(Guid id, MarkAsFalsePositiveDto input)
    {
        var result = await _resultRepository.GetAsync(id);
        result.MarkAsFalsePositive(CurrentUser.Id ?? Guid.Empty, input.Reason);
        await _resultRepository.UpdateAsync(result, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Resolve)]
    public async Task ResolveAsync(Guid id, ResolveDetectionResultDto input)
    {
        var result = await _resultRepository.GetAsync(id);
        result.Resolve(CurrentUser.Id ?? Guid.Empty, input.ResolutionNotes);
        await _resultRepository.UpdateAsync(result, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Resolve)]
    public async Task ReopenAsync(Guid id, ReopenDetectionResultDto input)
    {
        var result = await _resultRepository.GetAsync(id);
        result.Reopen(CurrentUser.Id ?? Guid.Empty, input.Reason);
        await _resultRepository.UpdateAsync(result, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Share)]
    public async Task ShareResultAsync(Guid id, ShareDetectionResultDto input)
    {
        var result = await _resultRepository.GetAsync(id);
        result.ShareResult(input.SharingLevel, CurrentUser.Id ?? Guid.Empty);
        await _resultRepository.UpdateAsync(result, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.Share)]
    public async Task RevokeSharingAsync(Guid id)
    {
        var result = await _resultRepository.GetAsync(id);
        result.RevokeSharing();
        await _resultRepository.UpdateAsync(result, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.ViewShared)]
    public async Task<PagedResultDto<AnomalyDetectionResultDto>> GetSharedResultsAsync(GetDetectionResultsInput input)
    {
        var queryable = await _resultRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.IsShared);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<AnomalyDetectionResult>, List<AnomalyDetectionResultDto>>(items);
        return new PagedResultDto<AnomalyDetectionResultDto>(totalCount, dtos);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.BulkOperations)]
    public async Task BulkUpdateResolutionStatusAsync(List<Guid> ids, ResolutionStatus status, string? notes = null)
    {
        foreach (var id in ids)
        {
            var result = await _resultRepository.GetAsync(id);

            switch (status)
            {
                case ResolutionStatus.InProgress:
                    result.MarkAsInvestigating(CurrentUser.Id ?? Guid.Empty, notes);
                    break;
                case ResolutionStatus.Resolved:
                    result.Resolve(CurrentUser.Id ?? Guid.Empty, notes ?? string.Empty);
                    break;
            }

            await _resultRepository.UpdateAsync(result);
        }
    }

    [Authorize(AnomalyDetectionPermissions.DetectionResults.BulkOperations)]
    public async Task BulkMarkAsFalsePositiveAsync(List<Guid> ids, string reason)
    {
        foreach (var id in ids)
        {
            var result = await _resultRepository.GetAsync(id);
            result.MarkAsFalsePositive(CurrentUser.Id ?? Guid.Empty, reason);
            await _resultRepository.UpdateAsync(result);
        }
    }

    public Task<Dictionary<string, object>> GetStatisticsAsync(GetDetectionResultsInput input)
    {
        // TODO: Implement statistics calculation
        return Task.FromResult(new Dictionary<string, object>());
    }

    public Task<byte[]> ExportAsync(GetDetectionResultsInput input, string format)
    {
        // TODO: Implement export functionality
        throw new NotImplementedException();
    }

    public Task<List<Dictionary<string, object>>> GetTimelineAsync(Guid? canSignalId = null, Guid? detectionLogicId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        // TODO: Implement timeline functionality
        return Task.FromResult(new List<Dictionary<string, object>>());
    }

    public Task<Dictionary<string, object>> GetCorrelationAnalysisAsync(List<Guid> resultIds)
    {
        // TODO: Implement correlation analysis
        return Task.FromResult(new Dictionary<string, object>());
    }

    public Task<ListResultDto<AnomalyDetectionResultDto>> GetSimilarResultsAsync(Guid id, int count = 5)
    {
        // TODO: Implement similar results logic
        return Task.FromResult(new ListResultDto<AnomalyDetectionResultDto>(new List<AnomalyDetectionResultDto>()));
    }
}