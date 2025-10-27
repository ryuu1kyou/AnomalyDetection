using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class MarkAsFalsePositiveDto
{
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Notes { get; set; } = string.Empty;
}