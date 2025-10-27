using System;
using System.Collections.Generic;

namespace AnomalyDetection.Projects.Dtos;

public class ProjectMilestoneDto
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime DueDate { get; set; }
    
    public MilestoneStatus Status { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
    public Guid? CompletedBy { get; set; }
    
    public string CompletedByUserName { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    // Configuration
    public bool IsCritical { get; set; }
    
    public bool RequiresApproval { get; set; }
    
    public List<string> Dependencies { get; set; } = new();
    
    public Dictionary<string, object> CustomProperties { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Calculated Properties
    public bool IsOverdue { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public bool IsActive { get; set; }
    
    public TimeSpan? TimeToDeadline { get; set; }
    
    public TimeSpan? CompletionTime { get; set; }
}