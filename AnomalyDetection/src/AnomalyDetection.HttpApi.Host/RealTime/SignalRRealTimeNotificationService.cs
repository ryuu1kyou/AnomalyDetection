using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.CanSignals;
using AnomalyDetection.RealTime;
using AnomalyDetection.Application.Monitoring;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.Hubs;

[Dependency(ServiceLifetime.Transient, ReplaceServices = true)]
[ExposeServices(typeof(IRealTimeNotificationService))]
public class SignalRRealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<RealTimeDetectionHub> _hubContext;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly IMonitoringService _monitoringService;
    private readonly ILogger<SignalRRealTimeNotificationService> _logger;

    public SignalRRealTimeNotificationService(
        IHubContext<RealTimeDetectionHub> hubContext,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IRepository<CanSignal, Guid> canSignalRepository,
        IMonitoringService monitoringService,
        ILogger<SignalRRealTimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _detectionLogicRepository = detectionLogicRepository;
        _canSignalRepository = canSignalRepository;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    public async Task NotifyDetectionCreatedAsync(AnomalyDetectionResultDto detection, RealTimeDetectionNotificationContext context)
    {
        await EnrichDetectionAsync(detection);
        await BroadcastAsync("ReceiveNewDetectionResult", detection, context);
        TrackRealTimeMetrics(context);
    }

    public async Task NotifyDetectionUpdatedAsync(AnomalyDetectionResultDto detection, RealTimeDetectionNotificationContext context)
    {
        await EnrichDetectionAsync(detection);
        await BroadcastAsync("ReceiveDetectionResultUpdate", detection, context);
        TrackRealTimeMetrics(context);
    }

    public async Task NotifyDetectionDeletedAsync(Guid detectionId, RealTimeDetectionNotificationContext context)
    {
        var tasks = CreateGroupBroadcasts("ReceiveDetectionResultDeletion", detectionId, context);
        await Task.WhenAll(tasks);

        TrackRealTimeMetrics(context);
    }

    public Task NotifyDetectionBatchAsync(IEnumerable<RealTimeDetectionBatchItem> detections)
    {
        return BroadcastBatchAsync(detections);
    }

    private async Task BroadcastAsync(string methodName, object payload, RealTimeDetectionNotificationContext context)
    {
        var tasks = CreateGroupBroadcasts(methodName, payload, context);
        await Task.WhenAll(tasks);
        _logger.LogDebug(
            "SignalR broadcast {Method} for DetectionLogic={DetectionLogicId}, Signal={SignalId}, Project={ProjectId}",
            methodName,
            context.DetectionLogicId,
            context.CanSignalId,
            context.ProjectId);
    }

    private List<Task> CreateGroupBroadcasts(string methodName, object payload, RealTimeDetectionNotificationContext context)
    {
        var tasks = new List<Task>
        {
            _hubContext.Clients.Group(RealTimeDetectionGroupNames.AllDetections).SendAsync(methodName, payload),
            _hubContext.Clients.Group(RealTimeDetectionGroupNames.ForSignal(context.CanSignalId)).SendAsync(methodName, payload)
        };

        if (context.ProjectId.HasValue)
        {
            tasks.Add(_hubContext.Clients.Group(RealTimeDetectionGroupNames.ForProject(context.ProjectId.Value)).SendAsync(methodName, payload));
        }

        return tasks;
    }

    private async Task BroadcastBatchAsync(IEnumerable<RealTimeDetectionBatchItem> detections)
    {
        foreach (var item in detections)
        {
            if (item.Context.ChangeType.Equals(RealTimeDetectionChangeTypes.Created, StringComparison.OrdinalIgnoreCase))
            {
                await NotifyDetectionCreatedAsync(item.Detection, item.Context);
            }
            else if (item.Context.ChangeType.Equals(RealTimeDetectionChangeTypes.Deleted, StringComparison.OrdinalIgnoreCase))
            {
                await NotifyDetectionDeletedAsync(item.Detection.Id, item.Context);
            }
            else
            {
                await NotifyDetectionUpdatedAsync(item.Detection, item.Context);
            }
        }
    }

    private async Task EnrichDetectionAsync(AnomalyDetectionResultDto detection)
    {
        if (string.IsNullOrWhiteSpace(detection.DetectionLogicName))
        {
            var logic = await _detectionLogicRepository.FindAsync(detection.DetectionLogicId);
            if (logic != null)
            {
                detection.DetectionLogicName = logic.Identity?.GetFullName() ?? logic.Identity?.Name ?? detection.DetectionLogicId.ToString();
            }
        }

        if (string.IsNullOrWhiteSpace(detection.SignalName) || string.IsNullOrWhiteSpace(detection.CanId))
        {
            var signal = await _canSignalRepository.FindAsync(detection.CanSignalId);
            if (signal != null)
            {
                detection.SignalName = signal.Identifier.SignalName;
                detection.CanId = signal.Identifier.CanId;
                detection.SystemType = signal.SystemType;
            }
        }
    }

    private void TrackRealTimeMetrics(RealTimeDetectionNotificationContext context)
    {
        var changeType = string.IsNullOrWhiteSpace(context.ChangeType)
            ? RealTimeDetectionChangeTypes.Updated
            : context.ChangeType;

        var targetGroup = context.ProjectId.HasValue
            ? RealTimeDetectionGroupNames.ForProject(context.ProjectId.Value)
            : RealTimeDetectionGroupNames.ForSignal(context.CanSignalId);

        TimeSpan? deliveryLatency = context.DeliveryLatencyMs.HasValue
            ? TimeSpan.FromMilliseconds(context.DeliveryLatencyMs.Value)
            : null;

        var slaMet = context.SlaMet ?? true;

        _monitoringService.TrackRealTimeDelivery(
            changeType,
            targetGroup,
            deliveryLatency,
            slaMet);
    }
}
