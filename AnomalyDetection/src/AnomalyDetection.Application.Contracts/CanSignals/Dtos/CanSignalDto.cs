using System;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.CanSignals.Dtos;

public class CanSignalDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    
    // Signal Identity
    public string SignalName { get; set; } = string.Empty;
    public string CanId { get; set; } = string.Empty;
    
    // Signal Specification
    public int StartBit { get; set; }
    public int Length { get; set; }
    public SignalDataType DataType { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public SignalByteOrder ByteOrder { get; set; }
    
    // Physical Value Conversion
    public double Factor { get; set; }
    public double Offset { get; set; }
    public string Unit { get; set; } = string.Empty;
    
    // Signal Timing
    public int CycleTime { get; set; }
    public int TimeoutTime { get; set; }
    
    // Entity Attributes
    public CanSystemType SystemType { get; set; }
    public string Description { get; set; } = string.Empty;
    public OemCode OemCode { get; set; } = new();
    public bool IsStandard { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
    public SignalStatus Status { get; set; }
    
    // Metadata
    public string SourceDocument { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}