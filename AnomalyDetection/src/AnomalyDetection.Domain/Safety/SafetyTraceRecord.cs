using System;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.Safety;

/// <summary>
/// Safety traceability record for ASIL B+ compliance
/// </summary>
public class SafetyTraceRecord : FullAuditedAggregateRoot<Guid>
{
    public string RequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    public AsilLevel AsilLevel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaselineId { get; set; } = string.Empty;
    public int Version { get; set; } = 1;

    // Traceability links
    public Guid? DetectionLogicId { get; set; }
    public Guid? ProjectId { get; set; }
    public List<string> RelatedDocuments { get; set; } = new();
    public List<TraceabilityLinkRecord> TraceabilityLinks { get; set; } = new();
    public List<LifecycleEvent> LifecycleEvents { get; set; } = new();
    public List<ChangeRequestRecord> ChangeRequests { get; set; } = new();

    // Approval workflow
    public ApprovalStatus ApprovalStatus { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string ApprovalComments { get; set; } = string.Empty;

    // Verification & Validation
    public List<VerificationRecord> Verifications { get; set; } = new();
    public List<ValidationRecord> Validations { get; set; } = new();

    // Audit trail
    public List<AuditEntry> AuditTrail { get; set; } = new();

    protected SafetyTraceRecord() { }

    public SafetyTraceRecord(
        Guid id,
        string requirementId,
        string safetyGoalId,
        AsilLevel asilLevel,
        string title)
        : base(id)
    {
        RequirementId = requirementId;
        SafetyGoalId = safetyGoalId;
        AsilLevel = asilLevel;
        Title = title;
        ApprovalStatus = ApprovalStatus.Draft;
    }

    public void Submit(Guid submittedBy)
    {
        if (ApprovalStatus != ApprovalStatus.Draft)
            throw new InvalidOperationException("Only draft records can be submitted");

        ApprovalStatus = AsilLevel >= AsilLevel.C
            ? ApprovalStatus.UnderReview
            : ApprovalStatus.Submitted;

        AddAuditEntry("Submitted for approval", submittedBy);
        AddLifecycleEvent(LifecycleStage.Validation, "Trace record submitted", submittedBy);
    }

    public void Approve(Guid approverUserId, string comments)
    {
        if (ApprovalStatus != ApprovalStatus.Submitted && ApprovalStatus != ApprovalStatus.UnderReview)
            throw new InvalidOperationException("Only submitted records can be approved");

        ApprovalStatus = ApprovalStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approverUserId;
        ApprovalComments = comments;
        AddAuditEntry("Approved", approverUserId, comments);
        AddLifecycleEvent(LifecycleStage.Operation, "Trace record approved", approverUserId, comments);
    }

    public void Reject(Guid approverUserId, string comments)
    {
        if (ApprovalStatus != ApprovalStatus.Submitted && ApprovalStatus != ApprovalStatus.UnderReview)
            throw new InvalidOperationException("Only submitted records can be rejected");

        ApprovalStatus = ApprovalStatus.Rejected;
        ApprovalComments = comments;
        AddAuditEntry("Rejected", approverUserId, comments);
        AddLifecycleEvent(LifecycleStage.Validation, "Trace record rejected", approverUserId, comments);
    }

    public void AddVerification(string method, string result, Guid verifierId)
    {
        Verifications.Add(new VerificationRecord
        {
            Method = method,
            Result = result,
            VerifiedBy = verifierId,
            VerifiedAt = DateTime.UtcNow
        });
        AddAuditEntry("Verification added", verifierId, method);
        AddLifecycleEvent(LifecycleStage.Verification, $"Verification performed: {method}", verifierId, result);
    }

    public void AddValidation(string criteria, bool passed, Guid validatorId)
    {
        Validations.Add(new ValidationRecord
        {
            Criteria = criteria,
            Passed = passed,
            ValidatedBy = validatorId,
            ValidatedAt = DateTime.UtcNow
        });
        var outcome = passed ? "Passed" : "Failed";
        AddAuditEntry("Validation added", validatorId, $"{criteria}: {outcome}");
        AddLifecycleEvent(LifecycleStage.Validation, $"Validation recorded: {criteria}", validatorId, outcome);
    }

    public void RecordBaseline(string baselineId, int? version = null)
    {
        BaselineId = baselineId;
        if (version.HasValue)
        {
            Version = version.Value;
        }

        AddAuditEntry("Baseline recorded", null, baselineId);
    }

    public void RecordChangeRequest(string changeId, string reason, string impactAnalysis, Guid requestedBy)
    {
        if (string.IsNullOrWhiteSpace(changeId))
            throw new ArgumentException("Change identifier is required", nameof(changeId));

        if (ChangeRequests.Any(x => x.ChangeId.Equals(changeId, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Change request '{changeId}' already exists");

        ChangeRequests.Add(new ChangeRequestRecord
        {
            ChangeId = changeId,
            Reason = reason,
            ImpactAnalysis = impactAnalysis,
            RequestedBy = requestedBy,
            RequestedAt = DateTime.UtcNow,
            Status = ChangeApprovalStatus.Submitted
        });

        AddAuditEntry("Change request submitted", requestedBy, changeId);
    }

    public void ApproveChangeRequest(string changeId, Guid approverId, string? notes = null)
    {
        var request = FindChangeRequest(changeId);
        request.Status = ChangeApprovalStatus.Approved;
        request.ApprovedBy = approverId;
        request.ApprovedAt = DateTime.UtcNow;
        request.Notes = notes ?? string.Empty;

        AddAuditEntry("Change request approved", approverId, changeId);
    }

    public void RejectChangeRequest(string changeId, Guid approverId, string reason)
    {
        var request = FindChangeRequest(changeId);
        request.Status = ChangeApprovalStatus.Rejected;
        request.ApprovedBy = approverId;
        request.ApprovedAt = DateTime.UtcNow;
        request.Notes = reason;

        AddAuditEntry("Change request rejected", approverId, changeId);
    }

    public void LinkTraceability(string sourceId, TraceabilityArtifactType sourceType, string targetId, TraceabilityArtifactType targetType, string relation)
    {
        TraceabilityLinks.Add(new TraceabilityLinkRecord
        {
            SourceId = sourceId,
            SourceType = sourceType,
            TargetId = targetId,
            TargetType = targetType,
            Relation = relation,
            LinkedAt = DateTime.UtcNow
        });

        AddAuditEntry("Traceability link recorded", null, $"{sourceType}:{sourceId}->{targetType}:{targetId}");
    }

    public SafetyTraceAuditSnapshot CreateAuditSnapshot()
    {
        return new SafetyTraceAuditSnapshot
        {
            RecordId = Id,
            RequirementId = RequirementId,
            SafetyGoalId = SafetyGoalId,
            DetectionLogicId = DetectionLogicId,
            AsilLevel = AsilLevel,
            ApprovalStatus = ApprovalStatus,
            Version = Version,
            ChangeRequests = ChangeRequests.ToList(),
            LifecycleEvents = LifecycleEvents.ToList(),
            AuditTrail = AuditTrail.ToList(),
            Verifications = Verifications.ToList(),
            Validations = Validations.ToList()
        };
    }

    public ChangeImpactSummary CalculateChangeImpact()
    {
        return new ChangeImpactSummary
        {
            RequiresSafetyApproval = AsilLevel >= AsilLevel.C,
            PendingHighRiskChanges = ChangeRequests.Count(x => x.Status == ChangeApprovalStatus.Submitted),
            OutstandingVerifications = Verifications.Count,
            OutstandingValidations = Validations.Count(x => !x.Passed)
        };
    }

    public void AddLifecycleEvent(LifecycleStage stage, string description, Guid performedBy, string? notes = null)
    {
        LifecycleEvents.Add(new LifecycleEvent
        {
            Stage = stage,
            Description = description,
            PerformedBy = performedBy,
            PerformedAt = DateTime.UtcNow,
            Notes = notes ?? string.Empty
        });
    }

    private ChangeRequestRecord FindChangeRequest(string changeId)
    {
        var request = ChangeRequests.FirstOrDefault(x => x.ChangeId.Equals(changeId, StringComparison.OrdinalIgnoreCase));
        if (request == null)
        {
            throw new KeyNotFoundException($"Change request '{changeId}' does not exist");
        }

        return request;
    }

    private void AddAuditEntry(string action, Guid? userId = null, string? notes = null)
    {
        AuditTrail.Add(new AuditEntry
        {
            Action = action,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Notes = notes ?? string.Empty
        });
    }
}

public enum ApprovalStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4
}

public class VerificationRecord
{
    public string Method { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public Guid VerifiedBy { get; set; }
    public DateTime VerifiedAt { get; set; }
}

public class ValidationRecord
{
    public string Criteria { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public Guid ValidatedBy { get; set; }
    public DateTime ValidatedAt { get; set; }
}

public class AuditEntry
{
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid? UserId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public enum LifecycleStage
{
    RequirementsDefinition = 0,
    Design = 1,
    Implementation = 2,
    Testing = 3,
    Verification = 4,
    Validation = 5,
    Operation = 6
}

public class LifecycleEvent
{
    public LifecycleStage Stage { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public enum ChangeApprovalStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3
}

public class ChangeRequestRecord
{
    public string ChangeId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ImpactAnalysis { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }
    public ChangeApprovalStatus Status { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public enum TraceabilityArtifactType
{
    SafetyRequirement = 1,
    DetectionLogic = 2,
    TestCase = 3,
    ValidationReport = 4,
    Project = 5,
    Evidence = 6
}

public class TraceabilityLinkRecord
{
    public string SourceId { get; set; } = string.Empty;
    public TraceabilityArtifactType SourceType { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public TraceabilityArtifactType TargetType { get; set; }
    public string Relation { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; }
}

public class SafetyTraceAuditSnapshot
{
    public Guid RecordId { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    public Guid? DetectionLogicId { get; set; }
    public AsilLevel AsilLevel { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public int Version { get; set; }
    public List<ChangeRequestRecord> ChangeRequests { get; set; } = new();
    public List<LifecycleEvent> LifecycleEvents { get; set; } = new();
    public List<AuditEntry> AuditTrail { get; set; } = new();
    public List<VerificationRecord> Verifications { get; set; } = new();
    public List<ValidationRecord> Validations { get; set; } = new();
}

public class ChangeImpactSummary
{
    public bool RequiresSafetyApproval { get; set; }
    public int PendingHighRiskChanges { get; set; }
    public int OutstandingVerifications { get; set; }
    public int OutstandingValidations { get; set; }
}
