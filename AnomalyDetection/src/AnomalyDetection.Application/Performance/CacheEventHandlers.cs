using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace AnomalyDetection.Performance;

/// <summary>
/// OEMマスターキャッシュイベントハンドラー
/// </summary>
public class OemMasterCacheEventHandler : 
    ILocalEventHandler<EntityCreatedEventData<OemMaster>>,
    ILocalEventHandler<EntityUpdatedEventData<OemMaster>>,
    ILocalEventHandler<EntityDeletedEventData<OemMaster>>,
    ITransientDependency
{
    private readonly CacheInvalidationService _cacheInvalidationService;
    private readonly ILogger<OemMasterCacheEventHandler> _logger;

    public OemMasterCacheEventHandler(
        CacheInvalidationService cacheInvalidationService,
        ILogger<OemMasterCacheEventHandler> logger)
    {
        _cacheInvalidationService = cacheInvalidationService;
        _logger = logger;
    }

    public async Task HandleEventAsync(EntityCreatedEventData<OemMaster> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateOemMasterRelatedCacheAsync(eventData.Entity.Id);
            _logger.LogInformation("Cache invalidated for created OEM Master: {Id}", eventData.Entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for created OEM Master: {Id}", eventData.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityUpdatedEventData<OemMaster> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateOemMasterRelatedCacheAsync(eventData.Entity.Id);
            _logger.LogInformation("Cache invalidated for updated OEM Master: {Id}", eventData.Entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for updated OEM Master: {Id}", eventData.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityDeletedEventData<OemMaster> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateOemMasterRelatedCacheAsync(eventData.Entity.Id);
            _logger.LogInformation("Cache invalidated for deleted OEM Master: {Id}", eventData.Entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for deleted OEM Master: {Id}", eventData.Entity.Id);
        }
    }
}

/// <summary>
/// CAN信号キャッシュイベントハンドラー
/// </summary>
public class CanSignalCacheEventHandler : 
    ILocalEventHandler<EntityCreatedEventData<CanSignal>>,
    ILocalEventHandler<EntityUpdatedEventData<CanSignal>>,
    ILocalEventHandler<EntityDeletedEventData<CanSignal>>,
    ITransientDependency
{
    private readonly CacheInvalidationService _cacheInvalidationService;
    private readonly ILogger<CanSignalCacheEventHandler> _logger;

    public CanSignalCacheEventHandler(
        CacheInvalidationService cacheInvalidationService,
        ILogger<CanSignalCacheEventHandler> logger)
    {
        _cacheInvalidationService = cacheInvalidationService;
        _logger = logger;
    }

    public async Task HandleEventAsync(EntityCreatedEventData<CanSignal> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateCanSignalRelatedCacheAsync(
                eventData.Entity.SystemType, 
                eventData.Entity.TenantId);
            _logger.LogInformation("Cache invalidated for created CAN Signal: {Id}, System: {SystemType}", 
                eventData.Entity.Id, eventData.Entity.SystemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for created CAN Signal: {Id}", eventData.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityUpdatedEventData<CanSignal> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateCanSignalRelatedCacheAsync(
                eventData.Entity.SystemType, 
                eventData.Entity.TenantId);
            _logger.LogInformation("Cache invalidated for updated CAN Signal: {Id}, System: {SystemType}", 
                eventData.Entity.Id, eventData.Entity.SystemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for updated CAN Signal: {Id}", eventData.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityDeletedEventData<CanSignal> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateCanSignalRelatedCacheAsync(
                eventData.Entity.SystemType, 
                eventData.Entity.TenantId);
            _logger.LogInformation("Cache invalidated for deleted CAN Signal: {Id}, System: {SystemType}", 
                eventData.Entity.Id, eventData.Entity.SystemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for deleted CAN Signal: {Id}", eventData.Entity.Id);
        }
    }
}

/// <summary>
/// システムカテゴリキャッシュイベントハンドラー
/// </summary>
public class SystemCategoryCacheEventHandler : 
    ILocalEventHandler<EntityCreatedEventData<CanSystemCategory>>,
    ILocalEventHandler<EntityUpdatedEventData<CanSystemCategory>>,
    ILocalEventHandler<EntityDeletedEventData<CanSystemCategory>>,
    ITransientDependency
{
    private readonly CacheInvalidationService _cacheInvalidationService;
    private readonly ILogger<SystemCategoryCacheEventHandler> _logger;

    public SystemCategoryCacheEventHandler(
        CacheInvalidationService cacheInvalidationService,
        ILogger<SystemCategoryCacheEventHandler> logger)
    {
        _cacheInvalidationService = cacheInvalidationService;
        _logger = logger;
    }

    public async Task HandleEventAsync(EntityCreatedEventData<CanSystemCategory> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateSystemCategoryRelatedCacheAsync(eventData.Entity.SystemType);
            _logger.LogInformation("Cache invalidated for created System Category: {Id}, Type: {SystemType}", 
                eventData.Entity.Id, eventData.Entity.SystemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for created System Category: {Id}", eventData.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityUpdatedEventData<CanSystemCategory> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateSystemCategoryRelatedCacheAsync(eventData.Entity.SystemType);
            _logger.LogInformation("Cache invalidated for updated System Category: {Id}, Type: {SystemType}", 
                eventData.Entity.Id, eventData.Entity.SystemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for updated System Category: {Id}", eventData.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityDeletedEventData<CanSystemCategory> eventData)
    {
        try
        {
            await _cacheInvalidationService.InvalidateSystemCategoryRelatedCacheAsync(eventData.Entity.SystemType);
            _logger.LogInformation("Cache invalidated for deleted System Category: {Id}, Type: {SystemType}", 
                eventData.Entity.Id, eventData.Entity.SystemType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for deleted System Category: {Id}", eventData.Entity.Id);
        }
    }
}