using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.Application.Monitoring
{
    public class MonitoringService : IMonitoringService, ISingletonDependency
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<MonitoringService> _logger;
        private readonly Meter _meter;

        // Metrics
        private readonly Counter<long> _detectionExecutionsCounter;
        private readonly Histogram<double> _detectionLatencyHistogram;
        private readonly ObservableGauge<int> _activeSessionsGauge;
        private readonly ObservableGauge<int> _databaseConnectionsGauge;
        private readonly ObservableGauge<double> _cacheHitRateGauge;
        private readonly Counter<long> _apiRequestsCounter;
        private readonly Histogram<double> _apiResponseTimeHistogram;
        private readonly Counter<long> _errorsCounter;
        private readonly Counter<long> _realTimeNotificationsCounter;
        private readonly Histogram<double> _realTimeProcessingLatencyHistogram;
    private readonly ObservableGauge<int> _signalRConnectionsGauge;
    private readonly Counter<long> _detectionResultsCounter;
    private readonly Counter<long> _broadcastFailuresCounter;
    private readonly Counter<long> _asilLevelChangesCounter;
    private readonly Counter<long> _asilReReviewCounter;

        // State tracking
        private int _activeSessions = 0;
        private int _activeDatabaseConnections = 0;
        private double _cacheHitRate = 0.0;
    private int _activeSignalRConnections = 0;

        public MonitoringService(
            TelemetryClient telemetryClient,
            ILogger<MonitoringService> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;

            // Initialize meter
            _meter = new Meter("AnomalyDetection.Monitoring", "1.0.0");

            // Initialize metrics
            _detectionExecutionsCounter = _meter.CreateCounter<long>(
                "anomaly_detection_executions_total",
                "executions",
                "Total number of anomaly detection executions");

            _detectionLatencyHistogram = _meter.CreateHistogram<double>(
                "anomaly_detection_latency_seconds",
                "seconds",
                "Anomaly detection execution latency in seconds");

            _activeSessionsGauge = _meter.CreateObservableGauge<int>(
                "active_user_sessions",
                () => _activeSessions,
                "sessions",
                "Number of active user sessions");

            _databaseConnectionsGauge = _meter.CreateObservableGauge<int>(
                "database_connections_active",
                () => _activeDatabaseConnections,
                "connections",
                "Number of active database connections");

            _cacheHitRateGauge = _meter.CreateObservableGauge<double>(
                "cache_hit_rate",
                () => _cacheHitRate,
                "percentage",
                "Cache hit rate percentage");

            _apiRequestsCounter = _meter.CreateCounter<long>(
                "api_requests_total",
                "requests",
                "Total number of API requests");

            _apiResponseTimeHistogram = _meter.CreateHistogram<double>(
                "api_response_time_seconds",
                "seconds",
                "API response time in seconds");

            _errorsCounter = _meter.CreateCounter<long>(
                "errors_total",
                "errors",
                "Total number of errors");

            _realTimeNotificationsCounter = _meter.CreateCounter<long>(
                "realtime_notifications_total",
                "notifications",
                "Total number of realtime notifications emitted");

            _realTimeProcessingLatencyHistogram = _meter.CreateHistogram<double>(
                "realtime_notification_processing_seconds",
                "seconds",
                "Latency between data change and realtime notification");

            _signalRConnectionsGauge = _meter.CreateObservableGauge<int>(
                "signalr_active_connections",
                () => _activeSignalRConnections,
                "connections",
                "Number of active SignalR realtime connections");

            _detectionResultsCounter = _meter.CreateCounter<long>(
                "detection_results_total",
                "results",
                "Total number of detection results created (derive per-minute rate in Grafana)");

            _broadcastFailuresCounter = _meter.CreateCounter<long>(
                "broadcast_failures_total",
                "failures",
                "Total number of realtime broadcast failures");

            _asilLevelChangesCounter = _meter.CreateCounter<long>(
                "asil_level_change_total",
                "changes",
                "Total number of ASIL level changes (labels: old_level, new_level)");

            _asilReReviewCounter = _meter.CreateCounter<long>(
                "asil_re_review_trigger_total",
                "triggers",
                "Total number of ASIL re-review triggers caused by ASIL level changes");
        }

        public void TrackDetectionExecution(string logicName, string signalName, double executionTimeMs, bool success)
        {
            var tags = new Dictionary<string, object?>
            {
                ["logic_name"] = logicName,
                ["signal_name"] = signalName,
                ["success"] = success.ToString()
            };

            var tagArray = tags.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();

            // Increment counter
            _detectionExecutionsCounter.Add(1, tagArray);

            // Record latency
            _detectionLatencyHistogram.Record(executionTimeMs / 1000.0, tagArray);

            // Application Insights
            var telemetry = new EventTelemetry("DetectionExecution");
            telemetry.Properties["LogicName"] = logicName;
            telemetry.Properties["SignalName"] = signalName;
            telemetry.Properties["Success"] = success.ToString();
            telemetry.Metrics["ExecutionTimeMs"] = executionTimeMs;

            _telemetryClient.TrackEvent(telemetry);

            _logger.LogInformation(
                "Detection execution tracked: Logic={LogicName}, Signal={SignalName}, Time={ExecutionTimeMs}ms, Success={Success}",
                logicName, signalName, executionTimeMs, success);
        }

        public void TrackApiRequest(string endpoint, string method, int statusCode, double responseTimeMs)
        {
            var tags = new Dictionary<string, object?>
            {
                ["endpoint"] = endpoint,
                ["method"] = method,
                ["status_code"] = statusCode.ToString()
            };

            var tagArray = tags.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();

            // Increment API requests counter
            _apiRequestsCounter.Add(1, tagArray);

            // Record response time
            _apiResponseTimeHistogram.Record(responseTimeMs / 1000.0, tagArray);

            // Track errors
            if (statusCode >= 400)
            {
                _errorsCounter.Add(1, tagArray);
            }

            // Application Insights
            var requestTelemetry = new RequestTelemetry
            {
                Name = $"{method} {endpoint}",
                Duration = TimeSpan.FromMilliseconds(responseTimeMs),
                ResponseCode = statusCode.ToString(),
                Success = statusCode < 400
            };

            _telemetryClient.TrackRequest(requestTelemetry);
        }
        public void TrackException(Exception exception, string context, Dictionary<string, string>? properties = null)
        {
            var tags = new Dictionary<string, object?>
            {
                ["context"] = context,
                ["exception_type"] = exception.GetType().Name
            };

            var tagArray = tags.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();

            _errorsCounter.Add(1, tagArray);

            // Application Insights
            var exceptionTelemetry = new ExceptionTelemetry(exception);
            exceptionTelemetry.Properties["Context"] = context;

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    exceptionTelemetry.Properties[prop.Key] = prop.Value;
                }
            }

            _telemetryClient.TrackException(exceptionTelemetry);

            _logger.LogError(exception, "Exception tracked in context: {Context}", context);
        }

        public void TrackCustomMetric(string metricName, double value, Dictionary<string, string>? properties = null)
        {
            // Application Insights
            var metricTelemetry = new MetricTelemetry(metricName, value);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    metricTelemetry.Properties[prop.Key] = prop.Value;
                }
            }

            _telemetryClient.TrackMetric(metricTelemetry);

            _logger.LogDebug("Custom metric tracked: {MetricName} = {Value}", metricName, value);
        }

        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            var dependencyTelemetry = new DependencyTelemetry
            {
                Name = dependencyName,
                Data = commandName,
                Timestamp = startTime,
                Duration = duration,
                Success = success
            };

            _telemetryClient.TrackDependency(dependencyTelemetry);
        }

        public void UpdateActiveSessions(int count)
        {
            _activeSessions = count;
            _logger.LogDebug("Active sessions updated: {Count}", count);
        }

        public void UpdateDatabaseConnections(int count)
        {
            _activeDatabaseConnections = count;
            _logger.LogDebug("Database connections updated: {Count}", count);
        }

        public void UpdateCacheHitRate(double hitRate)
        {
            _cacheHitRate = hitRate;
            _logger.LogDebug("Cache hit rate updated: {HitRate:P2}", hitRate);
        }

        public Task<Dictionary<string, object>> GetHealthMetricsAsync()
        {
            var metrics = new Dictionary<string, object>
            {
                ["ActiveSessions"] = _activeSessions,
                ["DatabaseConnections"] = _activeDatabaseConnections,
                ["CacheHitRate"] = _cacheHitRate,
                ["Timestamp"] = DateTimeOffset.UtcNow
            };

            // Add system metrics
            var process = Process.GetCurrentProcess();
            metrics["MemoryUsageMB"] = process.WorkingSet64 / 1024 / 1024;
            metrics["CpuTimeMs"] = process.TotalProcessorTime.TotalMilliseconds;
            metrics["ThreadCount"] = process.Threads.Count;

            return Task.FromResult(metrics);
        }

        public void TrackBusinessMetric(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null)
        {
            var eventTelemetry = new EventTelemetry(eventName);

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    eventTelemetry.Properties[prop.Key] = prop.Value;
                }
            }

            if (metrics != null)
            {
                foreach (var metric in metrics)
                {
                    eventTelemetry.Metrics[metric.Key] = metric.Value;
                }
            }

            _telemetryClient.TrackEvent(eventTelemetry);

            _logger.LogInformation("Business metric tracked: {EventName}", eventName);
        }

        public void TrackRealTimeDelivery(string changeType, string targetGroup, TimeSpan? processingLatency, bool success)
        {
            var tags = new Dictionary<string, object?>
            {
                ["change_type"] = changeType,
                ["target_group"] = targetGroup,
                ["success"] = success.ToString()
            };

            var tagArray = tags.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).ToArray();

            _realTimeNotificationsCounter.Add(1, tagArray);

            if (processingLatency.HasValue)
            {
                _realTimeProcessingLatencyHistogram.Record(processingLatency.Value.TotalSeconds, tagArray);
            }

            var eventTelemetry = new EventTelemetry("RealTimeDelivery")
            {
                Properties =
                {
                    ["ChangeType"] = changeType,
                    ["TargetGroup"] = targetGroup,
                    ["Success"] = success.ToString()
                }
            };

            if (processingLatency.HasValue)
            {
                eventTelemetry.Metrics["ProcessingLatencySeconds"] = processingLatency.Value.TotalSeconds;
            }

            _telemetryClient.TrackEvent(eventTelemetry);

            _logger.LogInformation(
                "Realtime delivery tracked: ChangeType={ChangeType}, Group={Group}, Latency={Latency}s, Success={Success}",
                changeType,
                targetGroup,
                processingLatency?.TotalSeconds ?? 0,
                success);
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }

        public void UpdateSignalRConnections(int count)
        {
            _activeSignalRConnections = count;
            _logger.LogDebug("SignalR active connections updated: {Count}", count);
        }

        public void TrackDetectionResultCreated(string detectionLogicId, string canSignalId, double latencyMs)
        {
            var tags = new KeyValuePair<string, object?>[]
            {
                new("detection_logic_id", detectionLogicId),
                new("can_signal_id", canSignalId)
            };
            _detectionResultsCounter.Add(1, tags);
            var telemetry = new EventTelemetry("DetectionResultCreated");
            telemetry.Properties["DetectionLogicId"] = detectionLogicId;
            telemetry.Properties["CanSignalId"] = canSignalId;
            telemetry.Metrics["CreationLatencyMs"] = latencyMs;
            _telemetryClient.TrackEvent(telemetry);
            _logger.LogInformation("Detection result created tracked: Logic={Logic} Signal={Signal} Latency={Latency}ms", detectionLogicId, canSignalId, latencyMs);
        }

        public void TrackBroadcastFailure(string changeType, string targetGroup, Exception ex)
        {
            var tags = new KeyValuePair<string, object?>[]
            {
                new("change_type", changeType),
                new("target_group", targetGroup),
                new("exception", ex.GetType().Name)
            };
            _broadcastFailuresCounter.Add(1, tags);
            var telemetry = new ExceptionTelemetry(ex);
            telemetry.Properties["ChangeType"] = changeType;
            telemetry.Properties["TargetGroup"] = targetGroup;
            _telemetryClient.TrackException(telemetry);
            _logger.LogError(ex, "Broadcast failure: ChangeType={ChangeType} Group={Group}", changeType, targetGroup);
        }

        public void TrackAsilLevelChange(int oldLevel, int newLevel, bool reReviewTriggered)
        {
            var tags = new KeyValuePair<string, object?>[]
            {
                new("old_level", oldLevel),
                new("new_level", newLevel),
                new("re_review", reReviewTriggered)
            };
            _asilLevelChangesCounter.Add(1, tags);
            if (reReviewTriggered)
            {
                var triggerTags = new KeyValuePair<string, object?>[]
                {
                    new("old_level", oldLevel),
                    new("new_level", newLevel)
                };
                _asilReReviewCounter.Add(1, triggerTags);
            }

            var evt = new EventTelemetry("AsilLevelChange");
            evt.Properties["OldLevel"] = oldLevel.ToString();
            evt.Properties["NewLevel"] = newLevel.ToString();
            evt.Properties["ReReview"] = reReviewTriggered.ToString();
            _telemetryClient.TrackEvent(evt);
            _logger.LogInformation("ASIL level change tracked {Old}->{New} ReReview={ReReview}", oldLevel, newLevel, reReviewTriggered);
        }
    }
}