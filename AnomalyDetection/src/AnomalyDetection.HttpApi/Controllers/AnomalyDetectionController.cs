using AnomalyDetection.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AnomalyDetection.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class AnomalyDetectionController : AbpControllerBase
{
    protected AnomalyDetectionController()
    {
        LocalizationResource = typeof(AnomalyDetectionResource);
    }
}
