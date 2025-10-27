using System;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class DetectionParameterDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    
    public ParameterDataType DataType { get; set; }
    
    public string Value { get; set; } = string.Empty;
    
    public string DefaultValue { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool IsRequired { get; set; }
    
    public string Unit { get; set; } = string.Empty;
    
    // Constraints
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public int? MinLength { get; set; }
    
    public int? MaxLength { get; set; }
    
    public string Pattern { get; set; } = string.Empty;
    
    public string AllowedValues { get; set; } = string.Empty; // Comma-separated values
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}