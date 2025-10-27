using System;
using System.Collections.Generic;
using AnomalyDetection.Projects;

namespace AnomalyDetection.Projects.Dtos;

public class ProjectMemberDto
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public ProjectRole Role { get; set; }
    
    public DateTime JoinedDate { get; set; }
    
    public DateTime? LeftDate { get; set; }
    
    public bool IsActive { get; set; }
    
    public string Notes { get; set; } = string.Empty;
    
    // Configuration
    public List<string> Permissions { get; set; } = new();
    
    public Dictionary<string, object> Settings { get; set; } = new();
    
    public bool CanReceiveNotifications { get; set; }
    
    public bool CanAccessReports { get; set; }
    
    // Calculated Properties
    public TimeSpan MembershipDuration { get; set; }
    
    public bool IsManager { get; set; }
    
    public bool IsLeader { get; set; }
    
    public bool CanManageProject { get; set; }
    
    public bool CanEditDetectionLogics { get; set; }
}