using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.CanSignals;

namespace AnomalyDetection.Projects.Dtos;

public class UpdateProjectDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string VehicleModel { get; set; } = string.Empty;
    
    [Required]
    [StringLength(4)]
    public string ModelYear { get; set; } = string.Empty;
    
    public CanSystemType PrimarySystem { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public Guid ProjectManagerId { get; set; }
    
    // Configuration
    public bool AutoProgressTracking { get; set; }
    
    public bool RequireApprovalForChanges { get; set; }
    
    public Dictionary<string, object> CustomSettings { get; set; } = new();
    
    [StringLength(1000)]
    public string ConfigurationNotes { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string UpdateReason { get; set; } = string.Empty;
}