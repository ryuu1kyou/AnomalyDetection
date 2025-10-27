using System;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.Projects.Dtos;

public class GetProjectsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    
    public string? ProjectCode { get; set; }
    
    public string? Name { get; set; }
    
    public ProjectStatus? Status { get; set; }
    
    public string? VehicleModel { get; set; }
    
    public string? ModelYear { get; set; }
    
    public CanSystemType? PrimarySystem { get; set; }
    
    public OemCode? OemCode { get; set; }
    
    public Guid? ProjectManagerId { get; set; }
    
    public DateTime? StartDateFrom { get; set; }
    
    public DateTime? StartDateTo { get; set; }
    
    public DateTime? EndDateFrom { get; set; }
    
    public DateTime? EndDateTo { get; set; }
    
    public double? MinProgressPercentage { get; set; }
    
    public double? MaxProgressPercentage { get; set; }
    
    public bool? IsActive { get; set; }
    
    public bool? IsOverdue { get; set; }
    
    public bool? HasOverdueMilestones { get; set; }
}