using Microsoft.Extensions.Localization;
using AnomalyDetection.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AnomalyDetection;

[Dependency(ReplaceServices = true)]
public class AnomalyDetectionBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AnomalyDetectionResource> _localizer;

    public AnomalyDetectionBrandingProvider(IStringLocalizer<AnomalyDetectionResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
