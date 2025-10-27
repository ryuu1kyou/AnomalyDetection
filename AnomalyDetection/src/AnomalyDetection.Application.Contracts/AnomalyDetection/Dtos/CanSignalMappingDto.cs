using System;
using System.Collections.Generic;
using AnomalyDetection.CanSignals;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class CanSignalMappingDto
{
    public Guid CanSignalId { get; set; }
    
    public string SignalRole { get; set; } = string.Empty;
    
    public bool IsRequired { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    // Configuration
    public double? ScalingFactor { get; set; }
    
    public double? Offset { get; set; }
    
    public string FilterExpression { get; set; } = string.Empty;
    
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Related Signal Information (for display purposes)
    public string SignalName { get; set; } = string.Empty;
    
    public string CanId { get; set; } = string.Empty;
    
    public CanSystemType SystemType { get; set; }
}