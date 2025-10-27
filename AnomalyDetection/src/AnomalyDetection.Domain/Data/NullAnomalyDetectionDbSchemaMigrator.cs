using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.Data;

/* This is used if database provider does't define
 * IAnomalyDetectionDbSchemaMigrator implementation.
 */
public class NullAnomalyDetectionDbSchemaMigrator : IAnomalyDetectionDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
