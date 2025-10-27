using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class ShareDetectionResultDto
{
    [Required]
    public SharingLevel SharingLevel { get; set; }
    
    [StringLength(2000)]
    public string ShareReason { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string ShareNotes { get; set; } = string.Empty;
    
    public bool RequireApproval { get; set; } = true;
}