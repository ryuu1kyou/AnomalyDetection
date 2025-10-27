using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AnomalyDetection.Data;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.EntityFrameworkCore;

public class EntityFrameworkCoreAnomalyDetectionDbSchemaMigrator
    : IAnomalyDetectionDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAnomalyDetectionDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AnomalyDetectionDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AnomalyDetectionDbContext>()
            .Database
            .MigrateAsync();
    }
}
