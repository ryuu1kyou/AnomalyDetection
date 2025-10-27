using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.Projects.Dtos;

public class UpdateProjectMilestoneDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime DueDate { get; set; }
    
    public MilestoneStatus Status { get; set; }
    
    public int DisplayOrder { get; set; }
    
    // Configuration
    public bool IsCritical { get; set; }
    
    public bool RequiresApproval { get; set; }
    
    public List<string> Dependencies { get; set; } = new();
    
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}