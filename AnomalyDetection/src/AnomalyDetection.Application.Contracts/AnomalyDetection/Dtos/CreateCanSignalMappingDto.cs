using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class CreateCanSignalMappingDto
{
    [Required]
    public Guid CanSignalId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string SignalRole { get; set; } = string.Empty;
    
    public bool IsRequired { get; set; } = true;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    // Configuration
    public double? ScalingFactor { get; set; }
    
    public double? Offset { get; set; }
    
    [StringLength(1000)]
    public string FilterExpression { get; set; } = string.Empty;
    
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}