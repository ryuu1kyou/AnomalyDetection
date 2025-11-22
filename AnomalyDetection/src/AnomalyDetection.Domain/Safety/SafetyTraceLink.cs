using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.Safety;

/// <summary>
/// Persistent link between SafetyTraceRecords (or record and DetectionLogic).
/// Source/Target both reference a SafetyTraceRecord for now; Target may later reference external artifacts.
/// LinkType example: "DetectionLogic", "DerivedRequirement", etc.
/// </summary>
public class SafetyTraceLink : FullAuditedAggregateRoot<Guid>
{
    public Guid SourceRecordId { get; set; }
    public Guid TargetRecordId { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty; // e.g. implements, verifies, traces

    protected SafetyTraceLink() {}

    public SafetyTraceLink(Guid id, Guid sourceRecordId, Guid targetRecordId, string linkType, string relation)
        : base(id)
    {
        SourceRecordId = sourceRecordId;
        TargetRecordId = targetRecordId;
        LinkType = linkType;
        Relation = relation;
    }

    public void Update(string linkType, string relation)
    {
        LinkType = linkType;
        Relation = relation;
        LastModificationTime = DateTime.UtcNow;
    }
}

/// <summary>
/// History entry recording changes to a SafetyTraceLink for diff tracking.
/// </summary>
public class SafetyTraceLinkHistory : CreationAuditedAggregateRoot<Guid>
{
    public Guid LinkId { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Added, Removed, Updated
    public string OldLinkType { get; set; } = string.Empty;
    public string NewLinkType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime ChangeTime { get; set; } = DateTime.UtcNow;

    protected SafetyTraceLinkHistory() {}

    public SafetyTraceLinkHistory(Guid id, Guid linkId, string changeType, string oldType, string newType, string notes)
        : base(id)
    {
        LinkId = linkId;
        ChangeType = changeType;
        OldLinkType = oldType;
        NewLinkType = newType;
        Notes = notes;
        ChangeTime = DateTime.UtcNow;
    }
}