using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using AnomalyDetection.Application.Monitoring;

namespace AnomalyDetection.Safety;

public class SafetyTraceAppService : ApplicationService, ISafetyTraceAppService
{
    private readonly IRepository<SafetyTraceRecord, Guid> _repository;
    private readonly IRepository<SafetyTraceLink, Guid> _linkRepository;
    private readonly IRepository<SafetyTraceLinkHistory, Guid> _linkHistoryRepository;
    private readonly IMonitoringService _monitoringService;

    public SafetyTraceAppService(
        IRepository<SafetyTraceRecord, Guid> repository,
        IRepository<SafetyTraceLink, Guid> linkRepository,
        IRepository<SafetyTraceLinkHistory, Guid> linkHistoryRepository,
        IMonitoringService monitoringService)
    {
        _repository = repository;
        _linkRepository = linkRepository;
        _linkHistoryRepository = linkHistoryRepository;
        _monitoringService = monitoringService;
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

    public async Task<SafetyTraceRecordDto> UpdateAsilLevelAsync(Guid id, int asilLevel, string reason)
    {
        var record = await _repository.GetAsync(id);
        var oldLevel = (int)record.AsilLevel;
        var oldStatus = record.ApprovalStatus;
        record.UpdateAsilLevel((AsilLevel)asilLevel, CurrentUser.GetId(), reason);
        await _repository.UpdateAsync(record, autoSave: true);
        var reReview = oldStatus == ApprovalStatus.Approved && record.ApprovalStatus != ApprovalStatus.Approved;
        _monitoringService.TrackAsilLevelChange(oldLevel, (int)record.AsilLevel, reReview);
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

    // --- New link persistence & querying methods ---
    public async Task<SafetyTraceLinkPersistenceResultDto> SyncLinkMatrixAsync(SafetyTraceLinkMatrixSyncInput input)
    {
        // Heuristic: all records with DetectionLogicId produce a link of type input.LinkType
        var queryable = await _repository.GetQueryableAsync();
        if (input.OnlyApproved)
        {
            queryable = queryable.Where(r => r.ApprovalStatus == ApprovalStatus.Approved);
        }
        var records = await AsyncExecuter.ToListAsync(queryable);
        var candidates = records.Where(r => r.DetectionLogicId.HasValue)
            .Select(r => (r.Id, r.DetectionLogicId!.Value))
            .Distinct()
            .ToHashSet();

        var existingQueryable = await _linkRepository.GetQueryableAsync();
        var existing = existingQueryable.Where(l => l.LinkType == input.LinkType).ToList();
        var existingSet = existing.Select(e => (e.SourceRecordId, e.TargetRecordId)).ToHashSet();

        var toAdd = candidates.Except(existingSet).ToList();
        var toRemove = existingSet.Except(candidates).ToList();

        var addedDtos = new System.Collections.Generic.List<SafetyTraceLinkDto>();
        var removedDtos = new System.Collections.Generic.List<SafetyTraceLinkDto>();

        foreach (var (sourceId, targetId) in toAdd)
        {
            var link = new SafetyTraceLink(GuidGenerator.Create(), sourceId, targetId, input.LinkType, input.Relation);
            await _linkRepository.InsertAsync(link, autoSave: true);
            await _linkHistoryRepository.InsertAsync(new SafetyTraceLinkHistory(GuidGenerator.Create(), link.Id, "Added", string.Empty, input.LinkType, "Sync add"), autoSave: true);
            addedDtos.Add(new SafetyTraceLinkDto
            {
                Id = link.Id,
                SourceRecordId = link.SourceRecordId,
                TargetRecordId = link.TargetRecordId,
                LinkType = link.LinkType,
                Relation = link.Relation,
                CreationTime = link.CreationTime,
                LastModificationTime = link.LastModificationTime
            });
        }

        foreach (var (sourceId, targetId) in toRemove)
        {
            var link = existing.First(l => l.SourceRecordId == sourceId && l.TargetRecordId == targetId);
            await _linkRepository.DeleteAsync(link, autoSave: true);
            await _linkHistoryRepository.InsertAsync(new SafetyTraceLinkHistory(GuidGenerator.Create(), link.Id, "Removed", link.LinkType, string.Empty, "Sync remove"), autoSave: true);
            removedDtos.Add(new SafetyTraceLinkDto
            {
                Id = link.Id,
                SourceRecordId = link.SourceRecordId,
                TargetRecordId = link.TargetRecordId,
                LinkType = link.LinkType,
                Relation = link.Relation,
                CreationTime = link.CreationTime,
                LastModificationTime = link.LastModificationTime
            });
        }

        return new SafetyTraceLinkPersistenceResultDto
        {
            ExecutedAt = DateTime.UtcNow,
            AddedCount = addedDtos.Count,
            RemovedCount = removedDtos.Count,
            UpdatedCount = 0,
            Diff = new SafetyTraceLinkDiffDto
            {
                Added = addedDtos,
                Removed = removedDtos,
                Updated = new System.Collections.Generic.List<SafetyTraceLinkDto>()
            }
        };
    }

    public async Task<System.Collections.Generic.List<SafetyTraceLinkDto>> GetLinksAsync(SafetyTraceLinkQueryInput input)
    {
        var queryable = await _linkRepository.GetQueryableAsync();
        if (input.SourceRecordId.HasValue) queryable = queryable.Where(x => x.SourceRecordId == input.SourceRecordId.Value);
        if (input.TargetRecordId.HasValue) queryable = queryable.Where(x => x.TargetRecordId == input.TargetRecordId.Value);
        if (!string.IsNullOrWhiteSpace(input.LinkType)) queryable = queryable.Where(x => x.LinkType == input.LinkType);
        var list = await AsyncExecuter.ToListAsync(queryable);
        return list.Select(x => new SafetyTraceLinkDto
        {
            Id = x.Id,
            SourceRecordId = x.SourceRecordId,
            TargetRecordId = x.TargetRecordId,
            LinkType = x.LinkType,
            Relation = x.Relation,
            CreationTime = x.CreationTime,
            LastModificationTime = x.LastModificationTime
        }).ToList();
    }

    public async Task<System.Collections.Generic.List<SafetyTraceLinkHistoryDto>> GetLinkHistoryAsync(Guid linkId)
    {
        var queryable = await _linkHistoryRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.LinkId == linkId).OrderByDescending(x => x.ChangeTime);
        var list = await AsyncExecuter.ToListAsync(queryable);
        return list.Select(x => new SafetyTraceLinkHistoryDto
        {
            Id = x.Id,
            LinkId = x.LinkId,
            ChangeType = x.ChangeType,
            OldLinkType = x.OldLinkType,
            NewLinkType = x.NewLinkType,
            Notes = x.Notes,
            ChangeTime = x.ChangeTime
        }).ToList();
    }
}
