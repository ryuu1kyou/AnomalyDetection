using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
        
        // State tracking
        private int _activeSessions = 0;
        private int _activeDatabaseConnections = 0;
        private double _cacheHitRate = 0.0;

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
        }

        public void TrackDetectionExecution(string logicName, string signalName, double executionTimeMs, bool success)
        {
            var tags = new Dictionary<string, object?>
            {
                ["logic_name"] = logicName,
                ["signal_name"] = signalName,
                ["success"] = success.ToString()
            };

            // Increment counter
            _detectionExecutionsCounter.Add(1, tags.ToArray());
            
            // Record latency
            _detectionLatencyHistogram.Record(executionTimeMs / 1000.0, tags.ToArray());

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

            // Increment API requests counter
            _apiRequestsCounter.Add(1, tags.ToArray());
            
            // Record response time
            _apiResponseTimeHistogram.Record(responseTimeMs / 1000.0, tags.ToArray());

            // Track errors
            if (statusCode >= 400)
            {
                _errorsCounter.Add(1, tags.ToArray());
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

            _errorsCounter.Add(1, tags.ToArray());

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

        public async Task<Dictionary<string, object>> GetHealthMetricsAsync()
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

            return metrics;
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

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }
}