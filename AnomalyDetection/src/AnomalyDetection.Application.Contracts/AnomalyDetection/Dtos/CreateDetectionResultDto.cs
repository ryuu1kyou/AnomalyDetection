using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnomalyDetection.AnomalyDetection.Dtos;

public class CreateDetectionResultDto
{
    [Required]
    public Guid DetectionLogicId { get; set; }

    [Required]
    public Guid CanSignalId { get; set; }

    public AnomalyLevel AnomalyLevel { get; set; }

    [Range(0.0, 1.0)]
    public double ConfidenceScore { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Input Data
    public double SignalValue { get; set; }

    public DateTime InputTimestamp { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object> AdditionalInputData { get; set; } = new();

    // Detection Details
    public DetectionType DetectionType { get; set; }

    [Required]
    [StringLength(500)]
    public string TriggerCondition { get; set; } = string.Empty;

    public Dictionary<string, object> DetectionParameters { get; set; } = new();

    public double ExecutionTimeMs { get; set; }

    // Sharing Settings
    public SharingLevel SharingLevel { get; set; } = SharingLevel.Private;

    // Knowledge base correlation tags
    public List<string> Tags { get; set; } = new();
}