using System;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.CanSignals.Dtos;

public class GetCanSignalsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    
    public string? SignalName { get; set; }
    
    public string? CanId { get; set; }
    
    public CanSystemType? SystemType { get; set; }
    
    public OemCode? OemCode { get; set; }
    
    public bool? IsStandard { get; set; }
    
    public SignalStatus? Status { get; set; }
    
    public DateTime? EffectiveDateFrom { get; set; }
    
    public DateTime? EffectiveDateTo { get; set; }
}