using System;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;

namespace AnomalyDetection.CanSignals.Dtos;

public class CreateCanSignalDto
{
    [Required]
    [StringLength(100)]
    public string SignalName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(8)]
    public string CanId { get; set; } = string.Empty;
    
    [Range(0, 63)]
    public int StartBit { get; set; }
    
    [Range(1, 64)]
    public int Length { get; set; }
    
    public SignalDataType DataType { get; set; }
    
    public double MinValue { get; set; }
    
    public double MaxValue { get; set; }
    
    public SignalByteOrder ByteOrder { get; set; } = SignalByteOrder.Motorola;
    
    // Physical Value Conversion
    public double Factor { get; set; } = 1.0;
    
    public double Offset { get; set; } = 0.0;
    
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    // Signal Timing
    [Range(1, 10000)]
    public int CycleTime { get; set; } = 100;
    
    [Range(1, 30000)]
    public int TimeoutTime { get; set; } = 300;
    
    public CanSystemType SystemType { get; set; }
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public OemCode OemCode { get; set; } = new();
    
    public bool IsStandard { get; set; } = false;
    
    public DateTime? EffectiveDate { get; set; }
    
    [StringLength(500)]
    public string SourceDocument { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Notes { get; set; } = string.Empty;
}