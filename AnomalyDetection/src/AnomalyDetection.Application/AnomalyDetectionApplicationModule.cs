using AnomalyDetection.Performance;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.Caching.StackExchangeRedis;

namespace AnomalyDetection;

[DependsOn(
    typeof(AnomalyDetectionDomainModule),
    typeof(AnomalyDetectionApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpCachingStackExchangeRedisModule)
    )]
public class AnomalyDetectionApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AnomalyDetectionApplicationModule>();
        });

        // キャッシュ設定
        CacheConfiguration.ConfigureDistributedCache(context);
        CacheConfiguration.ConfigureMemoryCache(context);
        CacheConfiguration.ConfigureCacheServices(context);
    }
}
