using System;
using System.Net.Http;
using Xunit;

namespace AnomalyDetection;

public abstract class SystemIntegrationTestBase : IClassFixture<AnomalyDetectionWebApplicationFactory>
{
    protected HttpClient Client { get; }
    protected AnomalyDetectionWebApplicationFactory Factory { get; }
    protected IServiceProvider ServiceProvider => Factory.Services;

    protected SystemIntegrationTestBase(AnomalyDetectionWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
