using System.Threading.Tasks;

namespace AnomalyDetection.Data;

public interface IAnomalyDetectionDbSchemaMigrator
{
    Task MigrateAsync();
}
