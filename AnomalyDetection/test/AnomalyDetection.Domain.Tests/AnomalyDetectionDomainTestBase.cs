using Volo.Abp.Modularity;

namespace AnomalyDetection;

/* Inherit from this class for your domain layer tests. */
public abstract class AnomalyDetectionDomainTestBase<TStartupModule> : AnomalyDetectionTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
