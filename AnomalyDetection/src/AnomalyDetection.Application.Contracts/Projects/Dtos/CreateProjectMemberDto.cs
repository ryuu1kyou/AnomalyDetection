using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.Projects;

namespace AnomalyDetection.Projects.Dtos;

public class CreateProjectMemberDto
{
    [Required]
    public Guid UserId { get; set; }
    
    public ProjectRole Role { get; set; } = ProjectRole.Engineer;
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    // Configuration
    public List<string> Permissions { get; set; } = new();
    
    public Dictionary<string, object> Settings { get; set; } = new();
    
    public bool CanReceiveNotifications { get; set; } = true;
    
    public bool CanAccessReports { get; set; } = true;
}