using System;
using AnomalyDetection.CanSignals;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class GetDetectionResultsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    
    public Guid? DetectionLogicId { get; set; }
    
    public Guid? CanSignalId { get; set; }
    
    public AnomalyLevel? AnomalyLevel { get; set; }
    
    public ResolutionStatus? ResolutionStatus { get; set; }
    
    public SharingLevel? SharingLevel { get; set; }
    
    public DetectionType? DetectionType { get; set; }
    
    public CanSystemType? SystemType { get; set; }
    
    public DateTime? DetectedFrom { get; set; }
    
    public DateTime? DetectedTo { get; set; }
    
    public DateTime? ResolvedFrom { get; set; }
    
    public DateTime? ResolvedTo { get; set; }
    
    public double? MinConfidenceScore { get; set; }
    
    public double? MaxConfidenceScore { get; set; }
    
    public bool? IsShared { get; set; }
    
    public bool? IsHighPriority { get; set; }
    
    public TimeSpan? MaxAge { get; set; }
}