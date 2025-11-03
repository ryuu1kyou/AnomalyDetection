using System;
using System.Collections.Generic;
using AnomalyDetection.CanSignals;
using AnomalyDetection.KnowledgeBase;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class AnomalyDetectionResultDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    // Related Entities
    public Guid DetectionLogicId { get; set; }
    public Guid CanSignalId { get; set; }

    // Detection Result Basic Information
    public DateTime DetectedAt { get; set; }
    public AnomalyLevel AnomalyLevel { get; set; }
    public double ConfidenceScore { get; set; }
    public string Description { get; set; } = string.Empty;

    // Input Data
    public double SignalValue { get; set; }
    public DateTime InputTimestamp { get; set; }
    public Dictionary<string, object> AdditionalInputData { get; set; } = new();

    // Detection Details
    public DetectionType DetectionType { get; set; }
    public string TriggerCondition { get; set; } = string.Empty;
    public Dictionary<string, object> DetectionParameters { get; set; } = new();
    public double ExecutionTimeMs { get; set; }

    // Resolution Status
    public ResolutionStatus ResolutionStatus { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string ResolutionNotes { get; set; } = string.Empty;

    // Sharing Settings
    public SharingLevel SharingLevel { get; set; }
    public bool IsShared { get; set; }
    public DateTime? SharedAt { get; set; }
    public Guid? SharedBy { get; set; }

    // Related Information (for display purposes)
    public string DetectionLogicName { get; set; } = string.Empty;
    public string SignalName { get; set; } = string.Empty;
    public string CanId { get; set; } = string.Empty;
    public CanSystemType SystemType { get; set; }
    public string ResolvedByUserName { get; set; } = string.Empty;
    public string SharedByUserName { get; set; } = string.Empty;
    public List<KnowledgeArticleSummaryDto> RecommendedArticles { get; set; } = new();
}