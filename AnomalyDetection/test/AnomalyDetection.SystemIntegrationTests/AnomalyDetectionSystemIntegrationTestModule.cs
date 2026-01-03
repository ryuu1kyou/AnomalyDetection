using Volo.Abp.Modularity;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.TestBase;

namespace AnomalyDetection;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreTestBaseModule),
    typeof(AnomalyDetectionHttpApiHostModule),
    typeof(AnomalyDetectionTestBaseModule)
)]
public class AnomalyDetectionSystemIntegrationTestModule : AbpModule
{
}
