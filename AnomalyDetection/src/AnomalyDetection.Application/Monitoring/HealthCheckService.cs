using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using AnomalyDetection.EntityFrameworkCore;

namespace AnomalyDetection.Application.Monitoring
{
    public class HealthCheckService : IHealthCheck, ITransientDependency
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IMonitoringService _monitoringService;
        private readonly IDbContextProvider<AnomalyDetectionDbContext> _dbContextProvider;
        private readonly IConnectionMultiplexer _redis;

        public HealthCheckService(
            ILogger<HealthCheckService> logger,
            IMonitoringService monitoringService,
            IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider,
            IConnectionMultiplexer redis)
        {
            _logger = logger;
            _monitoringService = monitoringService;
            _dbContextProvider = dbContextProvider;
            _redis = redis;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthData = new Dictionary<string, object>();
            var isHealthy = true;
            var errors = new List<string>();

            try
            {
                // データベースヘルスチェック
                var dbHealth = await CheckDatabaseHealthAsync(cancellationToken);
                healthData["Database"] = dbHealth;
                if (!dbHealth.IsHealthy)
                {
                    isHealthy = false;
                    errors.Add($"Database: {dbHealth.Error}");
                }

                // Redisヘルスチェック
                var redisHealth = await CheckRedisHealthAsync(cancellationToken);
                healthData["Redis"] = redisHealth;
                if (!redisHealth.IsHealthy)
                {
                    isHealthy = false;
                    errors.Add($"Redis: {redisHealth.Error}");
                }

                // システムメトリクス
                var systemMetrics = await GetSystemMetricsAsync();
                healthData["SystemMetrics"] = systemMetrics;

                // メモリ使用量チェック
                if (systemMetrics.MemoryUsagePercent > 90)
                {
                    isHealthy = false;
                    errors.Add($"High memory usage: {systemMetrics.MemoryUsagePercent:F1}%");
                }

                // アプリケーションメトリクス
                var appMetrics = await _monitoringService.GetHealthMetricsAsync();
                healthData["ApplicationMetrics"] = appMetrics;

                var status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
                var description = isHealthy ? "All systems operational" : string.Join("; ", errors);

                return new HealthCheckResult(status, description, data: healthData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                _monitoringService.TrackException(ex, "HealthCheck");

                return new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    $"Health check failed: {ex.Message}",
                    ex,
                    healthData);
            }
        }

        private async Task<HealthCheckItem> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
        {
            try
            {
                var startTime = DateTimeOffset.UtcNow;

                using var dbContext = await _dbContextProvider.GetDbContextAsync();
                var connection = dbContext.Database.GetDbConnection();

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                }

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.CommandTimeout = 5;

                var result = await command.ExecuteScalarAsync(cancellationToken);
                var duration = DateTimeOffset.UtcNow - startTime;

                _monitoringService.TrackDependency("Database", "HealthCheck", startTime, duration, true);

                return new HealthCheckItem
                {
                    IsHealthy = true,
                    ResponseTimeMs = duration.TotalMilliseconds,
                    Details = "Database connection successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");

                return new HealthCheckItem
                {
                    IsHealthy = false,
                    Error = ex.Message,
                    Details = "Database connection failed"
                };
            }
        }

        private async Task<HealthCheckItem> CheckRedisHealthAsync(CancellationToken cancellationToken)
        {
            try
            {
                var startTime = DateTimeOffset.UtcNow;
                var database = _redis.GetDatabase();

                await database.PingAsync();
                var duration = DateTimeOffset.UtcNow - startTime;

                _monitoringService.TrackDependency("Redis", "Ping", startTime, duration, true);

                return new HealthCheckItem
                {
                    IsHealthy = true,
                    ResponseTimeMs = duration.TotalMilliseconds,
                    Details = "Redis connection successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check failed");

                return new HealthCheckItem
                {
                    IsHealthy = false,
                    Error = ex.Message,
                    Details = "Redis connection failed"
                };
            }
        }

        private async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            return await Task.Run(() =>
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var totalMemory = GC.GetTotalMemory(false);

                // 概算のシステムメモリ使用率（実際の実装では WMI や /proc/meminfo を使用）
                var memoryUsagePercent = (double)process.WorkingSet64 / (1024 * 1024 * 1024) * 100; // GB単位での概算

                return new SystemMetrics
                {
                    MemoryUsageMB = process.WorkingSet64 / 1024 / 1024,
                    MemoryUsagePercent = Math.Min(memoryUsagePercent, 100), // 上限100%
                    CpuTimeMs = process.TotalProcessorTime.TotalMilliseconds,
                    ThreadCount = process.Threads.Count,
                    GCMemoryMB = totalMemory / 1024 / 1024,
                    UptimeSeconds = (DateTimeOffset.UtcNow - process.StartTime).TotalSeconds
                };
            });
        }
    }

    public class HealthCheckItem
    {
        public bool IsHealthy { get; set; }
        public double ResponseTimeMs { get; set; }
        public string? Error { get; set; }
        public string? Details { get; set; }
    }

    public class SystemMetrics
    {
        public long MemoryUsageMB { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double CpuTimeMs { get; set; }
        public int ThreadCount { get; set; }
        public long GCMemoryMB { get; set; }
        public double UptimeSeconds { get; set; }
    }
}