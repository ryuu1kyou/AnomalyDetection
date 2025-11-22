using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using AnomalyDetection.Safety;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.Shared.Export;

namespace AnomalyDetection.Safety.AppServices;

/// <summary>
/// Application service for managing SafetyTraceRecord lifecycle & approvals.
/// Implements minimal CRUD + workflow + export.
/// </summary>
public class SafetyTraceRecordAppService : ApplicationService
{
    private readonly IRepository<SafetyTraceRecord, Guid> _repository;
    private readonly IRepository<SafetyTraceLink, Guid> _linkRepository;
    private readonly IRepository<SafetyTraceLinkHistory, Guid> _linkHistoryRepository;
    private readonly ExportService _exportService;

    public SafetyTraceRecordAppService(
        IRepository<SafetyTraceRecord, Guid> repository,
        IRepository<SafetyTraceLink, Guid> linkRepository,
        IRepository<SafetyTraceLinkHistory, Guid> linkHistoryRepository,
        ExportService exportService)
    {
        _repository = repository;
        _linkRepository = linkRepository;
        _linkHistoryRepository = linkHistoryRepository;
        _exportService = exportService;
    }

    public async Task<SafetyTraceRecordDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<SafetyTraceRecord, SafetyTraceRecordDto>(entity);
    }

    public async Task<PagedResultDto<SafetyTraceRecordDto>> GetListAsync(SafetyTraceRecordListInput input)
    {
        var queryable = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.RequirementId))
            queryable = queryable.Where(x => x.RequirementId.Contains(input.RequirementId));
        if (input.AsilLevel.HasValue)
            queryable = queryable.Where(x => (int)x.AsilLevel == input.AsilLevel.Value);
        if (input.ApprovalStatus.HasValue)
            queryable = queryable.Where(x => (int)x.ApprovalStatus == input.ApprovalStatus.Value);
        if (input.DetectionLogicId.HasValue)
            queryable = queryable.Where(x => x.DetectionLogicId == input.DetectionLogicId.Value);

        var total = await AsyncExecuter.CountAsync(queryable);

        // Use dynamic LINQ for sorting similar to other app services
        queryable = queryable
            .OrderBy(input.Sorting ?? "CreationTime desc")
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var list = await AsyncExecuter.ToListAsync(queryable);
        var dtoList = ObjectMapper.Map<List<SafetyTraceRecord>, List<SafetyTraceRecordDto>>(list);
        return new PagedResultDto<SafetyTraceRecordDto>(total, dtoList);
    }

    public async Task<SafetyTraceRecordDto> CreateAsync(CreateSafetyTraceRecordInput input)
    {
        var entity = new SafetyTraceRecord(
            GuidGenerator.Create(),
            input.RequirementId,
            input.SafetyGoalId,
            (AsilLevel)input.AsilLevel,
            input.Title)
        {
            Description = input.Description ?? string.Empty,
            DetectionLogicId = input.DetectionLogicId,
            ProjectId = input.ProjectId
        };

        await _repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<SafetyTraceRecord, SafetyTraceRecordDto>(entity);
    }

    public async Task<SafetyTraceRecordDto> UpdateAsync(Guid id, UpdateSafetyTraceRecordInput input)
    {
        var entity = await _repository.GetAsync(id);
        if (entity.ApprovalStatus == ApprovalStatus.Approved)
        {
            throw new UserFriendlyException("Approved records cannot be modified.");
        }
        entity.Title = input.Title ?? entity.Title;
        entity.Description = input.Description ?? entity.Description;
        if (input.AsilLevel.HasValue)
            entity.AsilLevel = (AsilLevel)input.AsilLevel.Value;

        await _repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<SafetyTraceRecord, SafetyTraceRecordDto>(entity);
    }

    public async Task SubmitAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.Submit(CurrentUser.Id ?? Guid.Empty);
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public async Task ApproveAsync(Guid id, string comments)
    {
        var entity = await _repository.GetAsync(id);
        entity.Approve(CurrentUser.Id ?? Guid.Empty, comments);
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public async Task RejectAsync(Guid id, string comments)
    {
        var entity = await _repository.GetAsync(id);
        entity.Reject(CurrentUser.Id ?? Guid.Empty, comments);
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public async Task<SafetyTraceAuditSnapshotDto> GetAuditSnapshotAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        var snapshot = entity.CreateAuditSnapshot();
        return ObjectMapper.Map<SafetyTraceAuditSnapshot, SafetyTraceAuditSnapshotDto>(snapshot);
    }

    public async Task<ExportResultDto> ExportAsync(SafetyTraceExportInput input)
    {
        var queryable = await _repository.GetQueryableAsync();
        if (input.OnlyApproved)
            queryable = queryable.Where(x => x.ApprovalStatus == ApprovalStatus.Approved);
        if (input.AsilLevel.HasValue)
            queryable = queryable.Where(x => x.AsilLevel == (AsilLevel)input.AsilLevel.Value);

        var list = await AsyncExecuter.ToListAsync(queryable.Take(input.MaxRecords ?? 500));
        var minimal = list.Select(x => new
        {
            x.Id,
            x.RequirementId,
            x.SafetyGoalId,
            x.AsilLevel,
            x.Title,
            x.ApprovalStatus,
            x.Version,
            x.DetectionLogicId,
            x.ProjectId,
            x.CreationTime
        }).ToList();

        var request = new ExportDetectionRequest
        {
            Results = minimal,
            Format = (ExportService.ExportFormat)input.Format,
            FileNamePrefix = "safety_trace",
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions
            {
                ExcludedProperties = new List<string> { } // keep all minimal fields
            }
        };

        var result = await _exportService.ExportDetectionResultsAsync(request);

        return new ExportResultDto
        {
            FileName = result.FileName,
            ContentType = result.ContentType,
            RecordCount = result.Metadata.RecordCount,
            Format = result.Metadata.Format,
            ExportedAt = result.Metadata.ExportedAt,
            Data = result.Data
        };
    }

    /// <summary>
    /// Generates bidirectional pseudo-links between SafetyTraceRecords and DetectionLogics (heuristic matching by SafetyGoalId / RequirementId / DetectionLogicId).
    /// Returns an in-memory matrix (no persistence yet). Req18 強化: 双方向リンク自動化 / マトリクス生成.
    /// </summary>
    public async Task<SafetyTraceLinkMatrixDto> GenerateLinkMatrixAsync(SafetyTraceLinkMatrixInput input)
    {
        var queryable = await _repository.GetQueryableAsync();
        if (input.OnlyApproved) queryable = queryable.Where(x => x.ApprovalStatus == ApprovalStatus.Approved);
        var records = await AsyncExecuter.ToListAsync(queryable);

        // Group by SafetyGoalId and RequirementId for matrix axes
        var goals = records.Select(r => r.SafetyGoalId).Distinct().OrderBy(x => x).ToList();
        var requirements = records.Select(r => r.RequirementId).Distinct().OrderBy(x => x).ToList();

        // Build cell map
        var cells = new List<SafetyTraceMatrixCellDto>();
        foreach (var goal in goals)
        {
            foreach (var req in requirements)
            {
                var matching = records.Where(r => r.SafetyGoalId == goal && r.RequirementId == req).ToList();
                if (matching.Count == 0) continue;
                var logicIds = matching.Where(m => m.DetectionLogicId.HasValue).Select(m => m.DetectionLogicId!.Value).Distinct().ToList();
                // Re-review heuristic: Approved & (ASIL C+ AND (Version >1 OR Pending change requests OR LastModificationTime > ApprovedAt))
                bool needsReReview = matching.Any(m =>
                    m.ApprovalStatus == ApprovalStatus.Approved &&
                    m.AsilLevel >= AsilLevel.C &&
                    (
                        m.Version > 1 ||
                        m.ChangeRequests.Any(cr => cr.Status == ChangeApprovalStatus.Submitted) ||
                        (m.ApprovedAt.HasValue && m.LastModificationTime.HasValue && m.LastModificationTime.Value > m.ApprovedAt.Value.AddDays(7))
                    ));
                cells.Add(new SafetyTraceMatrixCellDto
                {
                    SafetyGoalId = goal,
                    RequirementId = req,
                    RecordIds = matching.Select(m => m.Id).ToList(),
                    DetectionLogicIds = logicIds,
                    AsilLevels = matching.Select(m => (int)m.AsilLevel).Distinct().ToList(),
                    ApprovedCount = matching.Count(m => m.ApprovalStatus == ApprovalStatus.Approved),
                    NeedsReReview = needsReReview
                });
            }
        }

        return new SafetyTraceLinkMatrixDto
        {
            GeneratedAt = DateTime.UtcNow,
            TotalRecords = records.Count,
            Cells = cells,
            SafetyGoalCount = goals.Count,
            RequirementCount = requirements.Count
        };
    }

    /// <summary>
    /// Exports the generated safety traceability matrix using the existing ExportService (CSV/JSON/pdf/excel).
    /// </summary>
    public async Task<ExportResultDto> ExportTraceabilityMatrixAsync(SafetyTraceMatrixExportInput input)
    {
        var matrix = await GenerateLinkMatrixAsync(new SafetyTraceLinkMatrixInput { OnlyApproved = input.OnlyApproved });
        var rows = matrix.Cells.Select(c => new
        {
            c.SafetyGoalId,
            c.RequirementId,
            RecordIds = string.Join(";", c.RecordIds),
            DetectionLogicIds = string.Join(";", c.DetectionLogicIds),
            AsilLevels = string.Join(";", c.AsilLevels),
            c.ApprovedCount
        }).ToList();
        var request = new ExportDetectionRequest
        {
            Results = rows,
            Format = (ExportService.ExportFormat)input.Format,
            FileNamePrefix = "safety_trace_matrix",
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions { IncludeHeader = true }
        };
        var result = await _exportService.ExportDetectionResultsAsync(request);
        return new ExportResultDto
        {
            FileName = result.FileName,
            ContentType = result.ContentType,
            RecordCount = result.Metadata.RecordCount,
            Format = result.Metadata.Format,
            ExportedAt = result.Metadata.ExportedAt,
            Data = result.Data
        };
    }

    /// <summary>
    /// Persists detection logic links derived from the heuristic matrix generation.
    /// Creates new links, removes obsolete, records history entries for diff tracking.
    /// </summary>
    public async Task<SafetyTraceLinkPersistenceResultDto> SyncLinkMatrixAsync(SafetyTraceLinkMatrixSyncInput input)
    {
        var matrix = await GenerateLinkMatrixAsync(new SafetyTraceLinkMatrixInput { OnlyApproved = input.OnlyApproved });
        var candidatePairs = new List<(Guid recordId, Guid logicId)>();
        foreach (var cell in matrix.Cells)
        {
            foreach (var recordId in cell.RecordIds)
            {
                foreach (var logicId in cell.DetectionLogicIds)
                {
                    candidatePairs.Add((recordId, logicId));
                }
            }
        }

        // Load existing links of type DetectionLogic
        var existingQueryable = await _linkRepository.GetQueryableAsync();
        var existing = existingQueryable.Where(x => x.LinkType == input.LinkType).ToList();

        var existingSet = existing.Select(e => (e.SourceRecordId, e.TargetRecordId)).ToHashSet();
        var candidateSet = candidatePairs.Distinct().ToHashSet();

        var toAdd = candidateSet.Except(existingSet).ToList();
        var toRemove = existingSet.Except(candidateSet).ToList();
        var addedDtos = new List<SafetyTraceLinkDto>();
        var removedDtos = new List<SafetyTraceLinkDto>();
        var updatedDtos = new List<SafetyTraceLinkDto>();

        // Add new links
        foreach (var (recordId, logicId) in toAdd)
        {
            var link = new SafetyTraceLink(GuidGenerator.Create(), recordId, logicId, input.LinkType, input.Relation);
            await _linkRepository.InsertAsync(link, autoSave: true);
            await _linkHistoryRepository.InsertAsync(new SafetyTraceLinkHistory(GuidGenerator.Create(), link.Id, "Added", string.Empty, input.LinkType, "Matrix sync add"), autoSave: true);
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

        // Remove obsolete links
        foreach (var (recordId, logicId) in toRemove)
        {
            var link = existing.First(e => e.SourceRecordId == recordId && e.TargetRecordId == logicId);
            await _linkRepository.DeleteAsync(link, autoSave: true);
            await _linkHistoryRepository.InsertAsync(new SafetyTraceLinkHistory(GuidGenerator.Create(), link.Id, "Removed", link.LinkType, string.Empty, "Matrix sync remove"), autoSave: true);
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

        // For now no update logic (link type constant). Placeholder for future modifications.

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
                Updated = updatedDtos
            }
        };
    }

    public async Task<List<SafetyTraceLinkDto>> GetLinksAsync(SafetyTraceLinkQueryInput input)
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

    public async Task<List<SafetyTraceLinkHistoryDto>> GetLinkHistoryAsync(Guid linkId)
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

public class SafetyTraceRecordListInput : PagedAndSortedResultRequestDto
{
    public string? RequirementId { get; set; }
    public int? AsilLevel { get; set; }
    public int? ApprovalStatus { get; set; }
    public Guid? DetectionLogicId { get; set; }
}

public class CreateSafetyTraceRecordInput
{
    public string RequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    public int AsilLevel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DetectionLogicId { get; set; }
    public Guid? ProjectId { get; set; }
}

public class UpdateSafetyTraceRecordInput
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? AsilLevel { get; set; }
}

public class SafetyTraceExportInput
{
    public bool OnlyApproved { get; set; } = true;
    public int? AsilLevel { get; set; }
    public int Format { get; set; } = (int)ExportService.ExportFormat.Csv;
    public int? MaxRecords { get; set; } = 500;
}

public class ExportResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

public class SafetyTraceLinkMatrixInput
{
    public bool OnlyApproved { get; set; } = true;
}

public class SafetyTraceMatrixExportInput
{
    public bool OnlyApproved { get; set; } = true;
    public int Format { get; set; } = (int)ExportService.ExportFormat.Csv;
}

public class SafetyTraceLinkMatrixDto
{
    public DateTime GeneratedAt { get; set; }
    public int TotalRecords { get; set; }
    public int SafetyGoalCount { get; set; }
    public int RequirementCount { get; set; }
    public List<SafetyTraceMatrixCellDto> Cells { get; set; } = new();
}

public class SafetyTraceMatrixCellDto
{
    public string SafetyGoalId { get; set; } = string.Empty;
    public string RequirementId { get; set; } = string.Empty;
    public List<Guid> RecordIds { get; set; } = new();
    public List<Guid> DetectionLogicIds { get; set; } = new();
    public List<int> AsilLevels { get; set; } = new();
    public int ApprovedCount { get; set; }
    public bool NeedsReReview { get; set; }
}