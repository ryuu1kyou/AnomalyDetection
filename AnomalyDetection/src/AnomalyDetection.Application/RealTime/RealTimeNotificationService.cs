using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.RealTime;

/// <summary>
/// Default no-op implementation. Replaced by the host layer when SignalR is available.
/// </summary>
public class NullRealTimeNotificationService : IRealTimeNotificationService, ITransientDependency
{
    public Task NotifyDetectionCreatedAsync(AnomalyDetectionResultDto detection, RealTimeDetectionNotificationContext context)
    {
        return Task.CompletedTask;
    }

    public Task NotifyDetectionUpdatedAsync(AnomalyDetectionResultDto detection, RealTimeDetectionNotificationContext context)
    {
        return Task.CompletedTask;
    }

    public Task NotifyDetectionDeletedAsync(Guid detectionId, RealTimeDetectionNotificationContext context)
    {
        return Task.CompletedTask;
    }

    public Task NotifyDetectionBatchAsync(IEnumerable<RealTimeDetectionBatchItem> detections)
    {
        return Task.CompletedTask;
    }
}
