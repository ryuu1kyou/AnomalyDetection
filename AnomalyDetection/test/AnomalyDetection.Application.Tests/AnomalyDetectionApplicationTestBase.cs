using Volo.Abp.Modularity;

namespace AnomalyDetection;

public abstract class AnomalyDetectionApplicationTestBase<TStartupModule> : AnomalyDetectionTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
