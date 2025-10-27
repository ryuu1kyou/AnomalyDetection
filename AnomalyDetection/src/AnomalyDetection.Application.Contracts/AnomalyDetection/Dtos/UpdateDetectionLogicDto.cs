using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
}