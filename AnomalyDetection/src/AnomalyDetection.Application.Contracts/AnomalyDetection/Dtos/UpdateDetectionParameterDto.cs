using System;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class UpdateDetectionParameterDto
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public ParameterDataType DataType { get; set; }
    
    [StringLength(500)]
    public string Value { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string DefaultValue { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsRequired { get; set; }
    
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    // Constraints
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public int? MinLength { get; set; }
    
    public int? MaxLength { get; set; }
    
    [StringLength(200)]
    public string Pattern { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string AllowedValues { get; set; } = string.Empty; // Comma-separated values
}