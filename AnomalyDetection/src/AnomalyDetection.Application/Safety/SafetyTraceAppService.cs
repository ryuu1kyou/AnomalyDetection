using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace AnomalyDetection.Safety;

public class SafetyTraceAppService : ApplicationService, ISafetyTraceAppService
{
    private readonly IRepository<SafetyTraceRecord, Guid> _repository;

    public SafetyTraceAppService(IRepository<SafetyTraceRecord, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<SafetyTraceRecordDto> GetAsync(Guid id)
    {
        var record = await _repository.GetAsync(id);
        return ObjectMapper.Map<SafetyTraceRecord, SafetyTraceRecordDto>(record);
    }

    public async Task<PagedResultDto<SafetyTraceRecordDto>> GetListAsync(GetSafetyTraceRecordsInput input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (input.AsilLevel.HasValue)
        {
            queryable = queryable.Where(x => (int)x.AsilLevel == input.AsilLevel.Value);
        }

        if (input.ApprovalStatus.HasValue)
        {
            queryable = queryable.Where(x => (int)x.ApprovalStatus == input.ApprovalStatus.Value);
        }

        if (input.ProjectId.HasValue)
        {
            queryable = queryable.Where(x => x.ProjectId == input.ProjectId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable
            .OrderBy(input.Sorting ?? "CreationTime desc")
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var records = await AsyncExecuter.ToListAsync(queryable);

        return new PagedResultDto<SafetyTraceRecordDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<SafetyTraceRecord>, System.Collections.Generic.List<SafetyTraceRecordDto>>(records)
        );
    }

    public async Task<SafetyTraceRecordDto> CreateAsync(CreateSafetyTraceRecordDto input)
    {
        var record = new SafetyTraceRecord(
            GuidGenerator.Create(),
            input.RequirementId,
            input.SafetyGoalId,
            (AsilLevel)input.AsilLevel,
            input.Title
        )
        {
            Description = input.Description,
            DetectionLogicId = input.DetectionLogicId,
            ProjectId = input.ProjectId,
            BaselineId = input.BaselineId,
            Version = input.Version ?? 1
        };

        await _repository.InsertAsync(record, autoSave: true);
        return ObjectMapper.Map<SafetyTraceRecord, SafetyTraceRecordDto>(record);
    }

    public async Task<SafetyTraceRecordDto> UpdateAsync(Guid id, UpdateSafetyTraceRecordDto input)
    {
        var record = await _repository.GetAsync(id);
        record.Title = input.Title;
        record.Description = input.Description;
        record.RelatedDocuments = input.RelatedDocuments;
        if (!string.IsNullOrWhiteSpace(input.BaselineId))
        {
            record.BaselineId = input.BaselineId;
        }

        if (input.Version.HasValue)
        {
            record.Version = input.Version.Value;
        }

        await _repository.UpdateAsync(record, autoSave: true);
        return ObjectMapper.Map<SafetyTraceRecord, SafetyTraceRecordDto>(record);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task SubmitAsync(Guid id)
    {
        var record = await _repository.GetAsync(id);
        record.Submit(CurrentUser.GetId());
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task ApproveAsync(Guid id, ApprovalDto input)
    {
        var record = await _repository.GetAsync(id);
        record.Approve(CurrentUser.Id ?? Guid.Empty, input.Comments);
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task RejectAsync(Guid id, ApprovalDto input)
    {
        var record = await _repository.GetAsync(id);
        record.Reject(CurrentUser.Id ?? Guid.Empty, input.Comments);
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task AddVerificationAsync(Guid id, AddVerificationDto input)
    {
        var record = await _repository.GetAsync(id);
        record.AddVerification(input.Method, input.Result, CurrentUser.GetId());
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task AddValidationAsync(Guid id, AddValidationDto input)
    {
        var record = await _repository.GetAsync(id);
        record.AddValidation(input.Criteria, input.Passed, CurrentUser.GetId());
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task RecordBaselineAsync(Guid id, RecordBaselineDto input)
    {
        var record = await _repository.GetAsync(id);
        record.RecordBaseline(input.BaselineId, input.Version);
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task RecordChangeRequestAsync(Guid id, SubmitChangeRequestDto input)
    {
        var record = await _repository.GetAsync(id);
        record.RecordChangeRequest(input.ChangeId, input.Reason, input.ImpactAnalysis, CurrentUser.GetId());
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task ApproveChangeRequestAsync(Guid id, string changeId, ChangeRequestDecisionDto input)
    {
        var record = await _repository.GetAsync(id);
        record.ApproveChangeRequest(changeId, CurrentUser.GetId(), input.Notes);
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task RejectChangeRequestAsync(Guid id, string changeId, ChangeRequestDecisionDto input)
    {
        var record = await _repository.GetAsync(id);
        record.RejectChangeRequest(changeId, CurrentUser.GetId(), input.Notes);
        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task LinkTraceabilityAsync(Guid id, TraceabilityLinkInputDto input)
    {
        var record = await _repository.GetAsync(id);
        record.LinkTraceability(
            input.SourceId,
            (TraceabilityArtifactType)input.SourceType,
            input.TargetId,
            (TraceabilityArtifactType)input.TargetType,
            input.Relation);

        await _repository.UpdateAsync(record, autoSave: true);
    }

    public async Task<SafetyTraceAuditSnapshotDto> GetAuditSnapshotAsync(Guid id)
    {
        var record = await _repository.GetAsync(id);
        var snapshot = record.CreateAuditSnapshot();
        return ObjectMapper.Map<SafetyTraceAuditSnapshot, SafetyTraceAuditSnapshotDto>(snapshot);
    }

    public async Task<ChangeImpactSummaryDto> GetChangeImpactAsync(Guid id)
    {
        var record = await _repository.GetAsync(id);
        var summary = record.CalculateChangeImpact();
        return ObjectMapper.Map<ChangeImpactSummary, ChangeImpactSummaryDto>(summary);
    }
}
