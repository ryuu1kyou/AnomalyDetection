using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;
using AnomalyDetection.CanSignals;

namespace AnomalyDetection.Performance;

/// <summary>
/// キャッシュ設定クラス
/// </summary>
public static class CacheConfiguration
{
    /// <summary>
    /// 分散キャッシュを設定
    /// </summary>
    public static void ConfigureDistributedCache(ServiceConfigurationContext context)
    {
        // Redis分散キャッシュの設定
        context.Services.AddStackExchangeRedisCache(options =>
        {
            // 設定は appsettings.json から読み込み
            options.Configuration = "localhost:6379"; // デフォルト設定
            options.InstanceName = "AnomalyDetection";
        });

        // ABP分散キャッシュの設定
        context.Services.Configure<AbpDistributedCacheOptions>(options =>
        {
            // キャッシュキーのプレフィックス
            options.KeyPrefix = "AnomalyDetection:";

            // デフォルトの有効期限
            options.GlobalCacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(20);
            options.GlobalCacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        });
    }

    /// <summary>
    /// メモリキャッシュを設定
    /// </summary>
    public static void ConfigureMemoryCache(ServiceConfigurationContext context)
    {
        context.Services.AddMemoryCache(options =>
        {
            // メモリキャッシュのサイズ制限
            options.SizeLimit = 1000;

            // 圧縮レベル
            options.CompactionPercentage = 0.25;
        });
    }

    /// <summary>
    /// キャッシュ関連サービスを登録
    /// </summary>
    public static void ConfigureCacheServices(ServiceConfigurationContext context)
    {
        // キャッシングサービスを登録
        context.Services.AddTransient<CachingService>();
        context.Services.AddTransient<CacheInvalidationService>();

        // キャッシュイベントハンドラーを登録
        context.Services.AddTransient<OemMasterCacheEventHandler>();
        context.Services.AddTransient<CanSignalCacheEventHandler>();
        context.Services.AddTransient<SystemCategoryCacheEventHandler>();
    }
}

/// <summary>
/// キャッシュ無効化サービス
/// </summary>
public class CacheInvalidationService
{
    private readonly CachingService _cachingService;

    public CacheInvalidationService(CachingService cachingService)
    {
        _cachingService = cachingService;
    }

    /// <summary>
    /// OEMマスター関連のキャッシュを無効化
    /// </summary>
    public async Task InvalidateOemMasterRelatedCacheAsync(Guid oemMasterId)
    {
        await _cachingService.InvalidateOemMasterCacheAsync(oemMasterId);
        // 関連する統計キャッシュも無効化
        await _cachingService.InvalidateSignalStatisticsCacheAsync();
    }

    /// <summary>
    /// CAN信号関連のキャッシュを無効化
    /// </summary>
    public async Task InvalidateCanSignalRelatedCacheAsync(CanSystemType systemType, Guid? tenantId = null)
    {
        await _cachingService.InvalidateSignalStatisticsCacheAsync(systemType, tenantId);
        await _cachingService.InvalidateSystemCategoryCacheAsync(systemType);
    }

    /// <summary>
    /// システムカテゴリ関連のキャッシュを無効化
    /// </summary>
    public async Task InvalidateSystemCategoryRelatedCacheAsync(CanSystemType systemType)
    {
        await _cachingService.InvalidateSystemCategoryCacheAsync(systemType);
        await _cachingService.InvalidateSignalStatisticsCacheAsync(systemType);
    }

    /// <summary>
    /// 全キャッシュを無効化（管理者用）
    /// </summary>
    public async Task InvalidateAllCacheAsync()
    {
        // 各システムタイプの統計キャッシュを無効化
        foreach (CanSystemType systemType in Enum.GetValues<CanSystemType>())
        {
            await _cachingService.InvalidateSignalStatisticsCacheAsync(systemType);
            await _cachingService.InvalidateSystemCategoryCacheAsync(systemType);
        }
    }
}
