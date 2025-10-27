using System;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class GetDetectionLogicsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    
    public string? Name { get; set; }
    
    public DetectionType? DetectionType { get; set; }
    
    public DetectionLogicStatus? Status { get; set; }
    
    public SharingLevel? SharingLevel { get; set; }
    
    public AsilLevel? AsilLevel { get; set; }
    
    public OemCode? OemCode { get; set; }
    
    public Guid? VehiclePhaseId { get; set; }
    
    public DateTime? CreatedFrom { get; set; }
    
    public DateTime? CreatedTo { get; set; }
    
    public DateTime? ApprovedFrom { get; set; }
    
    public DateTime? ApprovedTo { get; set; }
    
    public bool? HasImplementation { get; set; }
    
    public bool? IsExecutable { get; set; }
}