using System;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Events;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.RealTime;
using AnomalyDetection.Application.Monitoring;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed; // potential future distributed use
using Volo.Abp.EventBus;
using Volo.Abp.ObjectMapping;

namespace AnomalyDetection.AnomalyDetection.EventHandlers;

/// <summary>
/// Handles DetectionResultCreatedEvent by loading DTO and invoking realtime notification service.
/// Also records metrics for creation-to-broadcast latency. (Req9)
/// </summary>
public class DetectionResultCreatedEventHandler : ILocalEventHandler<DetectionResultCreatedEvent>
{
    private readonly IRepository<AnomalyDetectionResult, Guid> _resultRepository;
    private readonly IRealTimeNotificationService _realTime;
    private readonly IMonitoringService _monitoringService;
    private readonly IObjectMapper _objectMapper;

    public DetectionResultCreatedEventHandler(
        IRepository<AnomalyDetectionResult, Guid> resultRepository,
        IRealTimeNotificationService realTime,
        IMonitoringService monitoringService,
        IObjectMapper objectMapper)
    {
        _resultRepository = resultRepository;
        _realTime = realTime;
        _monitoringService = monitoringService;
        _objectMapper = objectMapper;
    }

    public async Task HandleEventAsync(DetectionResultCreatedEvent eventData)
    {
        var entity = await _resultRepository.GetAsync(eventData.ResultId);
        var dto = _objectMapper.Map<AnomalyDetectionResult, AnomalyDetectionResultDto>(entity);

        var latencyMs = eventData.ProcessingLatencyMs ?? (DateTime.UtcNow - entity.DetectedAt).TotalMilliseconds;
        var slaMet = latencyMs <= 5000; // consistent SLA placeholder

        var context = new RealTimeDetectionNotificationContext(
            entity.DetectionLogicId,
            entity.CanSignalId,
            null,
            latencyMs,
            slaMet,
            RealTimeDetectionChangeTypes.Created);

        const int maxRetries = 3;
        int attempt = 0;
        bool success = false;

        while (attempt < maxRetries && !success)
        {
            try
            {
                attempt++;
                await _realTime.NotifyDetectionCreatedAsync(dto, context);
                success = true;
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                {
                    _monitoringService.TrackBroadcastFailure("DetectionCreated", "Hub", ex);
                    // Log error but don't throw to avoid re-queueing if we want to just fail gracefully after retries
                    // Or throw if we want the event bus to handle it (but we already retried)
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt)); // Simple backoff
                }
            }
        }
    }
}
