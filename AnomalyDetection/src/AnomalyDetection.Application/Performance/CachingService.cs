using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.Performance;

/// <summary>
/// キャッシングサービス - マスターデータとよく使用されるデータのキャッシング
/// </summary>
public class CachingService : ApplicationService
{
    private readonly IDistributedCache<OemMasterCacheItem> _oemMasterSingleCache;
    private readonly IDistributedCache<OemMasterListCacheItem> _oemMasterListCache;
    private readonly IDistributedCache<CanSystemCategoryCacheItem> _systemCategorySingleCache;
    private readonly IDistributedCache<CanSystemCategoryListCacheItem> _systemCategoryListCache;
    private readonly IDistributedCache<SignalStatisticsCacheItem> _signalStatisticsCache;
    private readonly IRepository<OemMaster, Guid> _oemMasterRepository;
    private readonly IRepository<CanSystemCategory, Guid> _systemCategoryRepository;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly ILogger<CachingService> _logger;

    // キャッシュキーの定数
    private const string OEM_MASTER_CACHE_KEY = "OemMaster:{0}";
    private const string SYSTEM_CATEGORY_CACHE_KEY = "SystemCategory:{0}";
    private const string SIGNAL_STATISTICS_CACHE_KEY = "SignalStats:{0}:{1}";
    private const string ALL_OEM_MASTERS_CACHE_KEY = "AllOemMasters";
    private const string ALL_SYSTEM_CATEGORIES_CACHE_KEY = "AllSystemCategories";

    // キャッシュ有効期限
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MasterDataCacheExpiration = TimeSpan.FromHours(2);
    private static readonly TimeSpan StatisticsCacheExpiration = TimeSpan.FromMinutes(15);

    public CachingService(
        IDistributedCache<OemMasterCacheItem> oemMasterSingleCache,
        IDistributedCache<OemMasterListCacheItem> oemMasterListCache,
        IDistributedCache<CanSystemCategoryCacheItem> systemCategorySingleCache,
        IDistributedCache<CanSystemCategoryListCacheItem> systemCategoryListCache,
        IDistributedCache<SignalStatisticsCacheItem> signalStatisticsCache,
        IRepository<OemMaster, Guid> oemMasterRepository,
        IRepository<CanSystemCategory, Guid> systemCategoryRepository,
        IRepository<CanSignal, Guid> canSignalRepository,
        ILogger<CachingService> logger)
    {
        _oemMasterSingleCache = oemMasterSingleCache;
        _oemMasterListCache = oemMasterListCache;
        _systemCategorySingleCache = systemCategorySingleCache;
        _systemCategoryListCache = systemCategoryListCache;
        _signalStatisticsCache = signalStatisticsCache;
        _oemMasterRepository = oemMasterRepository;
        _systemCategoryRepository = systemCategoryRepository;
        _canSignalRepository = canSignalRepository;
        _logger = logger;
    }

    /// <summary>
    /// OEMマスターをキャッシュから取得（キャッシュミス時はDBから取得してキャッシュ）
    /// </summary>
    public async Task<OemMaster?> GetOemMasterAsync(Guid id)
    {
        var cacheKey = string.Format(OEM_MASTER_CACHE_KEY, id);

        try
        {
            var cachedItem = await _oemMasterSingleCache.GetAsync(cacheKey);
            if (cachedItem != null)
            {
                _logger.LogDebug("OEM Master cache hit for ID: {Id}", id);
                return cachedItem.ToEntity();
            }

            _logger.LogDebug("OEM Master cache miss for ID: {Id}", id);
            var entity = await _oemMasterRepository.FindAsync(id);
            if (entity != null)
            {
                var cacheItem = OemMasterCacheItem.FromEntity(entity);
                await _oemMasterSingleCache.SetAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = MasterDataCacheExpiration
                });
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OEM Master from cache for ID: {Id}", id);
            // キャッシュエラー時はDBから直接取得
            return await _oemMasterRepository.FindAsync(id);
        }
    }

    /// <summary>
    /// 全OEMマスターをキャッシュから取得
    /// </summary>
    public async Task<List<OemMaster>> GetAllOemMastersAsync()
    {
        try
        {
            var cachedItems = await _oemMasterListCache.GetAsync(ALL_OEM_MASTERS_CACHE_KEY);
            if (cachedItems != null)
            {
                _logger.LogDebug("All OEM Masters cache hit");
                return cachedItems.ToEntityList();
            }

            _logger.LogDebug("All OEM Masters cache miss");
            var entities = await _oemMasterRepository.GetListAsync();
            if (entities.Any())
            {
                var cacheItem = OemMasterListCacheItem.FromEntityList(entities);
                await _oemMasterListCache.SetAsync(ALL_OEM_MASTERS_CACHE_KEY, cacheItem, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = MasterDataCacheExpiration
                });
            }

            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all OEM Masters from cache");
            return await _oemMasterRepository.GetListAsync();
        }
    }

    /// <summary>
    /// システムカテゴリをキャッシュから取得
    /// </summary>
    public async Task<CanSystemCategory?> GetSystemCategoryAsync(CanSystemType systemType)
    {
        var cacheKey = string.Format(SYSTEM_CATEGORY_CACHE_KEY, systemType);

        try
        {
            var cachedItem = await _systemCategorySingleCache.GetAsync(cacheKey);
            if (cachedItem != null)
            {
                _logger.LogDebug("System Category cache hit for type: {SystemType}", systemType);
                return cachedItem.ToEntity();
            }

            _logger.LogDebug("System Category cache miss for type: {SystemType}", systemType);
            var entity = await _systemCategoryRepository.FirstOrDefaultAsync(c => c.SystemType == systemType);
            if (entity != null)
            {
                var cacheItem = CanSystemCategoryCacheItem.FromEntity(entity);
                await _systemCategorySingleCache.SetAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = MasterDataCacheExpiration
                });
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting System Category from cache for type: {SystemType}", systemType);
            return await _systemCategoryRepository.FirstOrDefaultAsync(c => c.SystemType == systemType);
        }
    }

    /// <summary>
    /// 全システムカテゴリをキャッシュから取得
    /// </summary>
    public async Task<List<CanSystemCategory>> GetAllSystemCategoriesAsync()
    {
        try
        {
            var cachedItems = await _systemCategoryListCache.GetAsync(ALL_SYSTEM_CATEGORIES_CACHE_KEY);
            if (cachedItems != null)
            {
                _logger.LogDebug("All System Categories cache hit");
                return cachedItems.ToEntityList();
            }

            _logger.LogDebug("All System Categories cache miss");
            var entities = await _systemCategoryRepository.GetListAsync();
            if (entities.Any())
            {
                var cacheItem = CanSystemCategoryListCacheItem.FromEntityList(entities);
                await _systemCategoryListCache.SetAsync(ALL_SYSTEM_CATEGORIES_CACHE_KEY, cacheItem, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = MasterDataCacheExpiration
                });
            }

            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all System Categories from cache");
            return await _systemCategoryRepository.GetListAsync();
        }
    }

    /// <summary>
    /// 信号統計をキャッシュから取得
    /// </summary>
    public async Task<SignalStatistics> GetSignalStatisticsAsync(CanSystemType systemType, Guid? tenantId = null)
    {
        var cacheKey = string.Format(SIGNAL_STATISTICS_CACHE_KEY, systemType, tenantId ?? Guid.Empty);

        try
        {
            var cachedItem = await _signalStatisticsCache.GetAsync(cacheKey);
            if (cachedItem != null)
            {
                _logger.LogDebug("Signal Statistics cache hit for type: {SystemType}, tenant: {TenantId}", systemType, tenantId);
                return cachedItem.ToStatistics();
            }

            _logger.LogDebug("Signal Statistics cache miss for type: {SystemType}, tenant: {TenantId}", systemType, tenantId);
            var statistics = await CalculateSignalStatisticsAsync(systemType, tenantId);

            var cacheItem = SignalStatisticsCacheItem.FromStatistics(statistics);
            await _signalStatisticsCache.SetAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = StatisticsCacheExpiration
            });

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Signal Statistics from cache for type: {SystemType}, tenant: {TenantId}", systemType, tenantId);
            return await CalculateSignalStatisticsAsync(systemType, tenantId);
        }
    }

    /// <summary>
    /// キャッシュを無効化
    /// </summary>
    public async Task InvalidateOemMasterCacheAsync(Guid id)
    {
        var cacheKey = string.Format(OEM_MASTER_CACHE_KEY, id);
        await _oemMasterSingleCache.RemoveAsync(cacheKey);
        await _oemMasterListCache.RemoveAsync(ALL_OEM_MASTERS_CACHE_KEY);
        _logger.LogInformation("Invalidated OEM Master cache for ID: {Id}", id);
    }

    /// <summary>
    /// システムカテゴリキャッシュを無効化
    /// </summary>
    public async Task InvalidateSystemCategoryCacheAsync(CanSystemType systemType)
    {
        var cacheKey = string.Format(SYSTEM_CATEGORY_CACHE_KEY, systemType);
        await _systemCategorySingleCache.RemoveAsync(cacheKey);
        await _systemCategoryListCache.RemoveAsync(ALL_SYSTEM_CATEGORIES_CACHE_KEY);
        _logger.LogInformation("Invalidated System Category cache for type: {SystemType}", systemType);
    }

    /// <summary>
    /// 信号統計キャッシュを無効化
    /// </summary>
    public async Task InvalidateSignalStatisticsCacheAsync(CanSystemType? systemType = null, Guid? tenantId = null)
    {
        if (systemType.HasValue)
        {
            var cacheKey = string.Format(SIGNAL_STATISTICS_CACHE_KEY, systemType.Value, tenantId ?? Guid.Empty);
            await _signalStatisticsCache.RemoveAsync(cacheKey);
        }
        else
        {
            // 全ての統計キャッシュを無効化（パターンマッチングが必要な場合）
            foreach (CanSystemType type in Enum.GetValues<CanSystemType>())
            {
                var cacheKey = string.Format(SIGNAL_STATISTICS_CACHE_KEY, type, tenantId ?? Guid.Empty);
                await _signalStatisticsCache.RemoveAsync(cacheKey);
            }
        }

        _logger.LogInformation("Invalidated Signal Statistics cache for type: {SystemType}, tenant: {TenantId}", systemType, tenantId);
    }

    #region Private Methods

    private async Task<SignalStatistics> CalculateSignalStatisticsAsync(CanSystemType systemType, Guid? tenantId)
    {
        var queryable = await _canSignalRepository.GetQueryableAsync();
        var query = queryable.Where(s => s.SystemType == systemType);

        if (tenantId.HasValue)
        {
            query = query.Where(s => s.TenantId == tenantId.Value);
        }

        var signals = await query.ToListAsync();

        return new SignalStatistics
        {
            SystemType = systemType,
            TenantId = tenantId,
            TotalCount = signals.Count,
            ActiveCount = signals.Count(s => s.Status == SignalStatus.Active),
            StandardCount = signals.Count(s => s.IsStandard),
            LastUpdated = DateTime.UtcNow
        };
    }

    #endregion
}

/// <summary>
/// キャッシュアイテム基底クラス
/// </summary>
public abstract class CacheItemBase
{
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// OEMマスターキャッシュアイテム
/// </summary>
public class OemMasterCacheItem : CacheItemBase
{
    public Guid Id { get; set; }
    public string OemCode { get; set; } = string.Empty;
    public string OemName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public static OemMasterCacheItem FromEntity(OemMaster entity)
    {
        return new OemMasterCacheItem
        {
            Id = entity.Id,
            OemCode = entity.OemCode.Code,
            OemName = entity.OemCode.Name,
            CompanyName = entity.CompanyName,
            Country = entity.Country,
            IsActive = entity.IsActive
        };
    }

    public OemMaster ToEntity()
    {
        // 注意: これは簡略化された変換です。実際の実装では完全なエンティティ復元が必要
        var entity = new OemMaster(
            Id,
            new OemCode(OemCode, OemName),
            CompanyName,
            Country);

        if (!IsActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}

/// <summary>
/// OEMマスターリストキャッシュアイテム
/// </summary>
public class OemMasterListCacheItem : CacheItemBase
{
    public List<OemMasterCacheItem> Items { get; set; } = new();

    public static OemMasterListCacheItem FromEntityList(List<OemMaster> entities)
    {
        return new OemMasterListCacheItem
        {
            Items = entities.Select(OemMasterCacheItem.FromEntity).ToList()
        };
    }

    public List<OemMaster> ToEntityList()
    {
        return Items.Select(item => item.ToEntity()).ToList();
    }
}

/// <summary>
/// システムカテゴリキャッシュアイテム
/// </summary>
public class CanSystemCategoryCacheItem : CacheItemBase
{
    public Guid Id { get; set; }
    public CanSystemType SystemType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    public static CanSystemCategoryCacheItem FromEntity(CanSystemCategory entity)
    {
        return new CanSystemCategoryCacheItem
        {
            Id = entity.Id,
            SystemType = entity.SystemType,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description ?? string.Empty,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder
        };
    }

    public CanSystemCategory ToEntity()
    {
        var entity = new CanSystemCategory(
            Id,
            null, // tenantId
            SystemType,
            Name,
            Description);

        entity.UpdateDisplayOrder(DisplayOrder);

        if (!IsActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}

/// <summary>
/// システムカテゴリリストキャッシュアイテム
/// </summary>
public class CanSystemCategoryListCacheItem : CacheItemBase
{
    public List<CanSystemCategoryCacheItem> Items { get; set; } = new();

    public static CanSystemCategoryListCacheItem FromEntityList(List<CanSystemCategory> entities)
    {
        return new CanSystemCategoryListCacheItem
        {
            Items = entities.Select(CanSystemCategoryCacheItem.FromEntity).ToList()
        };
    }

    public List<CanSystemCategory> ToEntityList()
    {
        return Items.Select(item => item.ToEntity()).ToList();
    }
}

/// <summary>
/// 信号統計キャッシュアイテム
/// </summary>
public class SignalStatisticsCacheItem : CacheItemBase
{
    public CanSystemType SystemType { get; set; }
    public Guid? TenantId { get; set; }
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int StandardCount { get; set; }
    public DateTime LastUpdated { get; set; }

    public static SignalStatisticsCacheItem FromStatistics(SignalStatistics statistics)
    {
        return new SignalStatisticsCacheItem
        {
            SystemType = statistics.SystemType,
            TenantId = statistics.TenantId,
            TotalCount = statistics.TotalCount,
            ActiveCount = statistics.ActiveCount,
            StandardCount = statistics.StandardCount,
            LastUpdated = statistics.LastUpdated
        };
    }

    public SignalStatistics ToStatistics()
    {
        return new SignalStatistics
        {
            SystemType = SystemType,
            TenantId = TenantId,
            TotalCount = TotalCount,
            ActiveCount = ActiveCount,
            StandardCount = StandardCount,
            LastUpdated = LastUpdated
        };
    }
}

/// <summary>
/// 信号統計データ
/// </summary>
public class SignalStatistics
{
    public CanSystemType SystemType { get; set; }
    public Guid? TenantId { get; set; }
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int StandardCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
