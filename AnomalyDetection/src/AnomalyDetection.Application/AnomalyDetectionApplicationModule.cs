using AnomalyDetection.Performance;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
// using Volo.Abp.Caching.StackExchangeRedis; // Redisを無効化

namespace AnomalyDetection;

[DependsOn(
    typeof(AnomalyDetectionDomainModule),
    typeof(AnomalyDetectionApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    // typeof(AbpCachingStackExchangeRedisModule) // Redisを無効化
    )]
public class AnomalyDetectionApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AnomalyDetectionApplicationModule>();
        });

        // キャッシュ設定 (Redisは無効化されているためメモリキャッシュを使用)
        // CacheConfiguration.ConfigureDistributedCache(context); // Redisキャッシュをコメントアウト
        CacheConfiguration.ConfigureMemoryCache(context);
        CacheConfiguration.ConfigureCacheServices(context);
    }
}

