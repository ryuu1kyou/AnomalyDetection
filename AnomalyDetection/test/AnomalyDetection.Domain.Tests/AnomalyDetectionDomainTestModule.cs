using Volo.Abp.Modularity;

namespace AnomalyDetection;

[DependsOn(
    typeof(AnomalyDetectionDomainModule),
    typeof(AnomalyDetectionTestBaseModule)
)]
public class AnomalyDetectionDomainTestModule : AbpModule
{

}
