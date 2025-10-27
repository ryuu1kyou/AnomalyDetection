using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.MultiTenancy;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class CreateDetectionLogicDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public OemCode OemCode { get; set; } = new();
    
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
    public AsilLevel AsilLevel { get; set; } = AsilLevel.QM;
    
    [StringLength(100)]
    public string SafetyRequirementId { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string SafetyGoalId { get; set; } = string.Empty;
    
    public SharingLevel SharingLevel { get; set; } = SharingLevel.Private;
    
    public Guid? SourceLogicId { get; set; }
    
    public Guid? VehiclePhaseId { get; set; }
    
    // Parameters and Signal Mappings
    public List<CreateDetectionParameterDto> Parameters { get; set; } = new();
    
    public List<CreateCanSignalMappingDto> SignalMappings { get; set; } = new();
}