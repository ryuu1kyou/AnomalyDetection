using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Domain.Repositories;
using AnomalyDetection.AnomalyDetection;
using NSubstitute;

namespace AnomalyDetection;

[DependsOn(
    typeof(AnomalyDetectionDomainModule),
    typeof(AnomalyDetectionTestBaseModule)
)]
public class AnomalyDetectionDomainTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register mock repositories for testing
        context.Services.AddTransient(provider => Substitute.For<IRepository<CanAnomalyDetectionLogic, Guid>>());
        context.Services.AddTransient(provider => Substitute.For<IRepository<AnomalyDetectionResult, Guid>>());
    }
}
