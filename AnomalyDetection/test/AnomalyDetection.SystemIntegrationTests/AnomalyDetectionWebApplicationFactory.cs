using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;

namespace AnomalyDetection;

public class AnomalyDetectionWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        // ABP Initialization is usually handled by the Host's Program.cs,
        // but WebApplicationFactory might need explicit Initialize call if it doesn't run the full pipeline.
        return host;
    }
}
