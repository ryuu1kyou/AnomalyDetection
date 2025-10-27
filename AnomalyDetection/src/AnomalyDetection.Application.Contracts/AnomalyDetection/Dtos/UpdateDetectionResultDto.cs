using System;
using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class UpdateDetectionResultDto
{
    public AnomalyLevel AnomalyLevel { get; set; }
    
    [Range(0.0, 1.0)]
    public double ConfidenceScore { get; set; }
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public SharingLevel SharingLevel { get; set; }
    
    [StringLength(500)]
    public string UpdateReason { get; set; } = string.Empty;
}