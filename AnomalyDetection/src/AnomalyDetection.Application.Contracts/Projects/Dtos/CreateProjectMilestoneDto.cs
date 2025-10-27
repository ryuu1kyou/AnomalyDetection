using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.Projects.Dtos;

public class CreateProjectMilestoneDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime DueDate { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    // Configuration
    public bool IsCritical { get; set; } = false;
    
    public bool RequiresApproval { get; set; } = false;
    
    public List<string> Dependencies { get; set; } = new();
    
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}