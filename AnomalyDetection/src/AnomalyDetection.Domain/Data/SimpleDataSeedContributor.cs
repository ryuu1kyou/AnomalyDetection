using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.Data;

public class SimpleDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ILogger<SimpleDataSeedContributor> _logger;

    public SimpleDataSeedContributor(ILogger<SimpleDataSeedContributor> logger)
    {
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        _logger.LogInformation("Starting simple data seeding...");
        
        // TODO: Implement actual data seeding once domain models are stable
        // This is a placeholder for the data seeding functionality
        
        _logger.LogInformation("Simple data seeding completed.");
        
        await Task.CompletedTask;
    }
}