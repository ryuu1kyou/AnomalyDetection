using System;
using System.Collections.Generic;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class CanAnomalyDetectionLogicDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    
    // Identity
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public OemCode OemCode { get; set; } = new();
    
    // Specification
    public DetectionType DetectionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    
    // Implementation
    public string LogicContent { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public bool IsExecutable { get; set; }
    
    // Safety Classification
    public AsilLevel AsilLevel { get; set; }
    public string SafetyRequirementId { get; set; } = string.Empty;
    public string SafetyGoalId { get; set; } = string.Empty;
    
    // Status and Sharing
    public DetectionLogicStatus Status { get; set; }
    public SharingLevel SharingLevel { get; set; }
    public Guid? SourceLogicId { get; set; }
    public Guid? VehiclePhaseId { get; set; }
    
    // Approval Information
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string ApprovalNotes { get; set; } = string.Empty;
    
    // Execution Statistics
    public int ExecutionCount { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public double? LastExecutionTimeMs { get; set; }
    
    // Related Entities
    public List<DetectionParameterDto> Parameters { get; set; } = new();
    public List<CanSignalMappingDto> SignalMappings { get; set; } = new();

    // トレサビ
    public string? FeatureId { get; set; }
    public string? DecisionId { get; set; }

    // 資産共通化分類
    public CommonalityStatus CommonalityStatus { get; set; }
    public DateTime? UnknownResolutionDueDate { get; set; }

    // 設計意図
    public string? DesignRationale { get; set; }
    public string? Assumptions { get; set; }
    public string? Constraints { get; set; }
    public string? PurposeShort { get; set; }

    // 文書同期
    public DocSyncStatus DocSyncStatus { get; set; }
    public string? DocVersion { get; set; }
}