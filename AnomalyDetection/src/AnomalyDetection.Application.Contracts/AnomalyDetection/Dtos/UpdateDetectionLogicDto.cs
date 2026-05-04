using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class UpdateDetectionLogicDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public DetectionType DetectionType { get; set; }
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;
    
    [StringLength(10000)]
    public string LogicContent { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Algorithm { get; set; } = string.Empty;
    
    // Safety Classification
    public AsilLevel AsilLevel { get; set; }
    
    [StringLength(100)]
    public string SafetyRequirementId { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string SafetyGoalId { get; set; } = string.Empty;
    
    public SharingLevel SharingLevel { get; set; }
    
    public Guid? VehiclePhaseId { get; set; }
    
    [StringLength(500)]
    public string ChangeReason { get; set; } = string.Empty;
    
    // Parameters and Signal Mappings
    public List<UpdateDetectionParameterDto> Parameters { get; set; } = new();

    public List<UpdateCanSignalMappingDto> SignalMappings { get; set; } = new();

    // トレサビ
    [StringLength(50)]
    public string? FeatureId { get; set; }

    [StringLength(50)]
    public string? DecisionId { get; set; }

    // 資産共通化分類
    public CommonalityStatus? CommonalityStatus { get; set; }
    public DateTime? UnknownResolutionDueDate { get; set; }

    // 設計意図
    [StringLength(2000)]
    public string? DesignRationale { get; set; }

    [StringLength(2000)]
    public string? Assumptions { get; set; }

    [StringLength(2000)]
    public string? Constraints { get; set; }

    [StringLength(200)]
    public string? PurposeShort { get; set; }

    // 文書同期
    public DocSyncStatus? DocSyncStatus { get; set; }

    [StringLength(100)]
    public string? DocVersion { get; set; }
}