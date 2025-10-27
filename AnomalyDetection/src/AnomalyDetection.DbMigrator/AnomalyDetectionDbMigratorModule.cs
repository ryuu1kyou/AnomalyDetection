using AnomalyDetection.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace AnomalyDetection.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AnomalyDetectionEntityFrameworkCoreModule),
    typeof(AnomalyDetectionApplicationContractsModule)
)]
public class AnomalyDetectionDbMigratorModule : AbpModule
{
}
