using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AnomalyDetection.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AnomalyDetectionDbContextFactory : IDesignTimeDbContextFactory<AnomalyDetectionDbContext>
{
    public AnomalyDetectionDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        AnomalyDetectionEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<AnomalyDetectionDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new AnomalyDetectionDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../AnomalyDetection.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
