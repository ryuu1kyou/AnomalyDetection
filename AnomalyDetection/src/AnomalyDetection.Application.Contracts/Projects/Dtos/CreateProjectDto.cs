using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;

namespace AnomalyDetection.Projects.Dtos;

public class CreateProjectDto
{
    [Required]
    [StringLength(20)]
    public string ProjectCode { get; set; } = string.Empty;
    
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
    
    [Required]
    public OemCode OemCode { get; set; } = new();
    
    [Required]
    public Guid ProjectManagerId { get; set; }
    
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndDate { get; set; }
    
    // Configuration
    public bool AutoProgressTracking { get; set; } = true;
    
    public bool RequireApprovalForChanges { get; set; } = false;
    
    public Dictionary<string, object> CustomSettings { get; set; } = new();
    
    [StringLength(1000)]
    public string ConfigurationNotes { get; set; } = string.Empty;
    
    // Initial Members and Milestones
    public List<CreateProjectMemberDto> InitialMembers { get; set; } = new();
    
    public List<CreateProjectMilestoneDto> InitialMilestones { get; set; } = new();
}