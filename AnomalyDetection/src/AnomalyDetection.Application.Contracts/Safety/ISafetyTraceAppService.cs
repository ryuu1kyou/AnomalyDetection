using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Safety;

public interface ISafetyTraceAppService : IApplicationService
{
    Task<SafetyTraceRecordDto> GetAsync(Guid id);
    Task<PagedResultDto<SafetyTraceRecordDto>> GetListAsync(GetSafetyTraceRecordsInput input);
    Task<SafetyTraceRecordDto> CreateAsync(CreateSafetyTraceRecordDto input);
    Task<SafetyTraceRecordDto> UpdateAsync(Guid id, UpdateSafetyTraceRecordDto input);
    /// <summary>
    /// Updates only the ASIL level for a record with audit + re-review logic and returns the updated DTO.
    /// </summary>
    /// <param name="id">Record identifier</param>
    /// <param name="asilLevel">New ASIL level (int enum value)</param>
    /// <param name="reason">Business justification (stored in audit trail)</param>
    Task<SafetyTraceRecordDto> UpdateAsilLevelAsync(Guid id, int asilLevel, string reason);
    Task DeleteAsync(Guid id);
    Task SubmitAsync(Guid id);
    Task ApproveAsync(Guid id, ApprovalDto input);
    Task RejectAsync(Guid id, ApprovalDto input);
    Task AddVerificationAsync(Guid id, AddVerificationDto input);
    Task AddValidationAsync(Guid id, AddValidationDto input);
    Task RecordBaselineAsync(Guid id, RecordBaselineDto input);
    Task RecordChangeRequestAsync(Guid id, SubmitChangeRequestDto input);
    Task ApproveChangeRequestAsync(Guid id, string changeId, ChangeRequestDecisionDto input);
    Task RejectChangeRequestAsync(Guid id, string changeId, ChangeRequestDecisionDto input);
    Task LinkTraceabilityAsync(Guid id, TraceabilityLinkInputDto input);
    Task<SafetyTraceAuditSnapshotDto> GetAuditSnapshotAsync(Guid id);
    Task<ChangeImpactSummaryDto> GetChangeImpactAsync(Guid id);
    Task<SafetyTraceLinkPersistenceResultDto> SyncLinkMatrixAsync(SafetyTraceLinkMatrixSyncInput input);
    Task<List<SafetyTraceLinkDto>> GetLinksAsync(SafetyTraceLinkQueryInput input);
    Task<List<SafetyTraceLinkHistoryDto>> GetLinkHistoryAsync(Guid linkId);
}

public class SafetyTraceRecordDto : FullAuditedEntityDto<Guid>
{
    public string RequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    public int AsilLevel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaselineId { get; set; } = string.Empty;
    public int Version { get; set; }
    public int ApprovalStatus { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string ApprovalComments { get; set; } = string.Empty;
    public Guid? DetectionLogicId { get; set; }
    public Guid? ProjectId { get; set; }
    public List<string> RelatedDocuments { get; set; } = new();
    public List<VerificationRecordDto> Verifications { get; set; } = new();
    public List<ValidationRecordDto> Validations { get; set; } = new();
    public List<AuditEntryDto> AuditTrail { get; set; } = new();
    public List<LifecycleEventDto> LifecycleEvents { get; set; } = new();
    public List<ChangeRequestRecordDto> ChangeRequests { get; set; } = new();
    public List<TraceabilityLinkRecordDto> TraceabilityLinks { get; set; } = new();
}

public class GetSafetyTraceRecordsInput : PagedAndSortedResultRequestDto
{
    public int? AsilLevel { get; set; }
    public int? ApprovalStatus { get; set; }
    public Guid? ProjectId { get; set; }
}

public class CreateSafetyTraceRecordDto
{
    public string RequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    public int AsilLevel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? DetectionLogicId { get; set; }
    public Guid? ProjectId { get; set; }
    public string BaselineId { get; set; } = string.Empty;
    public int? Version { get; set; }
}

public class UpdateSafetyTraceRecordDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RelatedDocuments { get; set; } = new();
    public string BaselineId { get; set; } = string.Empty;
    public int? Version { get; set; }
}

public class ApprovalDto
{
    public string Comments { get; set; } = string.Empty;
}

public class AddVerificationDto
{
    public string Method { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

public class AddValidationDto
{
    public string Criteria { get; set; } = string.Empty;
    public bool Passed { get; set; }
}

public class RecordBaselineDto
{
    public string BaselineId { get; set; } = string.Empty;
    public int? Version { get; set; }
}

public class SubmitChangeRequestDto
{
    public string ChangeId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ImpactAnalysis { get; set; } = string.Empty;
}

public class ChangeRequestDecisionDto
{
    public string Notes { get; set; } = string.Empty;
}

public class TraceabilityLinkInputDto
{
    public string SourceId { get; set; } = string.Empty;
    public int SourceType { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public int TargetType { get; set; }
    public string Relation { get; set; } = string.Empty;
}

public class VerificationRecordDto
{
    public string Method { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public Guid VerifiedBy { get; set; }
    public DateTime VerifiedAt { get; set; }
}

public class ValidationRecordDto
{
    public string Criteria { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public Guid ValidatedBy { get; set; }
    public DateTime ValidatedAt { get; set; }
}

public class AuditEntryDto
{
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class LifecycleEventDto
{
    public int Stage { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ChangeRequestRecordDto
{
    public string ChangeId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ImpactAnalysis { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public int Status { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class TraceabilityLinkRecordDto
{
    public string SourceId { get; set; } = string.Empty;
    public int SourceType { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public int TargetType { get; set; }
    public string Relation { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; }
}

public class SafetyTraceAuditSnapshotDto
{
    public Guid RecordId { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    public Guid? DetectionLogicId { get; set; }
    public int AsilLevel { get; set; }
    public int ApprovalStatus { get; set; }
    public int Version { get; set; }
    public List<ChangeRequestRecordDto> ChangeRequests { get; set; } = new();
    public List<LifecycleEventDto> LifecycleEvents { get; set; } = new();
    public List<AuditEntryDto> AuditTrail { get; set; } = new();
    public List<VerificationRecordDto> Verifications { get; set; } = new();
    public List<ValidationRecordDto> Validations { get; set; } = new();
}

public class ChangeImpactSummaryDto
{
    public bool RequiresSafetyApproval { get; set; }
    public int PendingHighRiskChanges { get; set; }
    public int OutstandingVerifications { get; set; }
    public int OutstandingValidations { get; set; }
}

public class SafetyTraceLinkDto : EntityDto<Guid>
{
    public Guid SourceRecordId { get; set; }
    public Guid TargetRecordId { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class SafetyTraceLinkHistoryDto : EntityDto<Guid>
{
    public Guid LinkId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string OldLinkType { get; set; } = string.Empty;
    public string NewLinkType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime ChangeTime { get; set; }
}

public class SafetyTraceLinkMatrixSyncInput
{
    public bool OnlyApproved { get; set; } = true;
    public string Relation { get; set; } = "implements"; // default relation label
    public string LinkType { get; set; } = "DetectionLogic";
}

public class SafetyTraceLinkQueryInput
{
    public Guid? SourceRecordId { get; set; }
    public Guid? TargetRecordId { get; set; }
    public string? LinkType { get; set; }
}

public class SafetyTraceLinkDiffDto
{
    public List<SafetyTraceLinkDto> Added { get; set; } = new();
    public List<SafetyTraceLinkDto> Removed { get; set; } = new();
    public List<SafetyTraceLinkDto> Updated { get; set; } = new();
}

public class SafetyTraceLinkPersistenceResultDto
{
    public DateTime ExecutedAt { get; set; }
    public int AddedCount { get; set; }
    public int RemovedCount { get; set; }
    public int UpdatedCount { get; set; }
    public SafetyTraceLinkDiffDto Diff { get; set; } = new();
}
