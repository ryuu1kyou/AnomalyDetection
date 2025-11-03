using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;

namespace AnomalyDetection.RealTime;

public interface IRealTimeNotificationService
{
    Task NotifyDetectionCreatedAsync(AnomalyDetectionResultDto detection, RealTimeDetectionNotificationContext context);

    Task NotifyDetectionUpdatedAsync(AnomalyDetectionResultDto detection, RealTimeDetectionNotificationContext context);

    Task NotifyDetectionDeletedAsync(Guid detectionId, RealTimeDetectionNotificationContext context);

    Task NotifyDetectionBatchAsync(IEnumerable<RealTimeDetectionBatchItem> detections);
}

public sealed class RealTimeDetectionNotificationContext
{
    public RealTimeDetectionNotificationContext(
        Guid detectionLogicId,
        Guid canSignalId,
        Guid? projectId,
        double? deliveryLatencyMs,
        bool? slaMet,
        string changeType)
    {
        DetectionLogicId = detectionLogicId;
        CanSignalId = canSignalId;
        ProjectId = projectId;
        DeliveryLatencyMs = deliveryLatencyMs;
        SlaMet = slaMet;
        ChangeType = changeType;
    }

    public Guid DetectionLogicId { get; }

    public Guid CanSignalId { get; }

    public Guid? ProjectId { get; }

    public double? DeliveryLatencyMs { get; }

    public bool? SlaMet { get; }

    public string ChangeType { get; }
}

public sealed record RealTimeDetectionBatchItem(
    AnomalyDetectionResultDto Detection,
    RealTimeDetectionNotificationContext Context);

public static class RealTimeDetectionGroupNames
{
    public const string AllDetections = "all_detections";

    public static string ForProject(Guid projectId) => $"project_{projectId:D}";

    public static string ForSignal(Guid signalId) => $"signal_{signalId:D}";
}

public static class RealTimeDetectionChangeTypes
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
}
