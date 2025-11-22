using System;
using Volo.Abp.EventBus;

namespace AnomalyDetection.AnomalyDetection.Events;

/// <summary>
/// Domain/local event raised when a new anomaly detection result has been persisted.
/// Used to decouple persistence from realtime broadcast & metrics aggregation (Req9).
/// </summary>
[EventName("AnomalyDetection.DetectionResultCreated")]
public class DetectionResultCreatedEvent
{
    public Guid ResultId { get; }
    public Guid DetectionLogicId { get; }
    public Guid CanSignalId { get; }
    public DateTime DetectedAt { get; }
    public double? ProcessingLatencyMs { get; }

    public DetectionResultCreatedEvent(Guid resultId, Guid detectionLogicId, Guid canSignalId, DateTime detectedAt, double? processingLatencyMs)
    {
        ResultId = resultId;
        DetectionLogicId = detectionLogicId;
        CanSignalId = canSignalId;
        DetectedAt = detectedAt;
        ProcessingLatencyMs = processingLatencyMs;
    }
}
