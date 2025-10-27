using System;
using System.Collections.Generic;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.Projects.Dtos;

public class AnomalyDetectionProjectDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    
    // Project Basic Information
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    
    // Vehicle Information
    public string VehicleModel { get; set; } = string.Empty;
    public string ModelYear { get; set; } = string.Empty;
    public CanSystemType PrimarySystem { get; set; }
    public OemCode OemCode { get; set; } = new();
    
    // Project Schedule
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    
    // Project Management
    public Guid ProjectManagerId { get; set; }
    public string ProjectManagerName { get; set; } = string.Empty;
    
    // Configuration
    public bool AutoProgressTracking { get; set; }
    public bool RequireApprovalForChanges { get; set; }
    public Dictionary<string, object> CustomSettings { get; set; } = new();
    public string ConfigurationNotes { get; set; } = string.Empty;
    
    // Progress Statistics
    public double ProgressPercentage { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public DateTime? LastProgressUpdate { get; set; }
    
    // Related Entities
    public List<ProjectMilestoneDto> Milestones { get; set; } = new();
    public List<ProjectMemberDto> Members { get; set; } = new();
    
    // Calculated Properties
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOverdue { get; set; }
    public TimeSpan? RemainingTime { get; set; }
    public TimeSpan ProjectDuration { get; set; }
    public int OverdueMilestonesCount { get; set; }
    public int ActiveMembersCount { get; set; }
}