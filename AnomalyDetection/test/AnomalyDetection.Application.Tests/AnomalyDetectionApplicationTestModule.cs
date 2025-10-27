using Volo.Abp.Modularity;

namespace AnomalyDetection;

[DependsOn(
    typeof(AnomalyDetectionApplicationModule),
    typeof(AnomalyDetectionDomainTestModule)
)]
public class AnomalyDetectionApplicationTestModule : AbpModule
{

}
