using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AnomalyDetection.Data;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.EntityFrameworkCore;

public class EntityFrameworkCoreAnomalyDetectionDbSchemaMigrator
    : IAnomalyDetectionDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntityFrameworkCoreAnomalyDetectionDbSchemaMigrator> _logger;

    public EntityFrameworkCoreAnomalyDetectionDbSchemaMigrator(IServiceProvider serviceProvider, ILogger<EntityFrameworkCoreAnomalyDetectionDbSchemaMigrator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AnomalyDetectionDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        var context = _serviceProvider.GetRequiredService<AnomalyDetectionDbContext>();
        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Skipping migration: connection string is not configured.");
            return;
        }

        try
        {
            await context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed.");
            throw;
        }
    }
}
