using AnomalyDetection.Localization;
using Volo.Abp.Application.Services;

namespace AnomalyDetection;

/* Inherit your application services from this class.
 */
public abstract class AnomalyDetectionAppService : ApplicationService
{
    protected AnomalyDetectionAppService()
    {
        LocalizationResource = typeof(AnomalyDetectionResource);
    }
}
