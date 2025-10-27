using Volo.Abp.Settings;

namespace AnomalyDetection.Settings;

public class AnomalyDetectionSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AnomalyDetectionSettings.MySetting1));
    }
}
