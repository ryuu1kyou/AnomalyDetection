using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class ResolveDetectionResultDto
{
    public ResolutionStatus ResolutionStatus { get; set; }
    
    [Required]
    [StringLength(1000)]
    public string ResolutionNotes { get; set; } = string.Empty;
}