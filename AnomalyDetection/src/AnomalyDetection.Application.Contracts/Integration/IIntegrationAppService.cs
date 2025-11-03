using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Integration;

public interface IIntegrationEndpointAppService : IApplicationService
{
    /// <summary>
    /// Create integration endpoint
    /// </summary>
    Task<IntegrationEndpointDto> CreateAsync(CreateIntegrationEndpointDto input);

    /// <summary>
    /// Update integration endpoint
    /// </summary>
    Task<IntegrationEndpointDto> UpdateAsync(Guid id, UpdateIntegrationEndpointDto input);

    /// <summary>
    /// Get endpoint list
    /// </summary>
    Task<PagedResultDto<IntegrationEndpointDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get endpoint details
    /// </summary>
    Task<IntegrationEndpointDto> GetAsync(Guid id);

    /// <summary>
    /// Delete endpoint
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Test endpoint connectivity
    /// </summary>
    Task<bool> TestConnectionAsync(Guid id);

    /// <summary>
    /// Get endpoint logs
    /// </summary>
    Task<PagedResultDto<IntegrationLogDto>> GetLogsAsync(Guid id, PagedAndSortedResultRequestDto input);
}

public interface IWebhookAppService : IApplicationService
{
    /// <summary>
    /// Create webhook subscription
    /// </summary>
    Task<WebhookSubscriptionDto> CreateSubscriptionAsync(Guid endpointId, CreateWebhookSubscriptionDto input);

    /// <summary>
    /// Get webhook subscriptions for endpoint
    /// </summary>
    Task<PagedResultDto<WebhookSubscriptionDto>> GetSubscriptionsAsync(Guid endpointId, PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Update webhook subscription
    /// </summary>
    Task<WebhookSubscriptionDto> UpdateSubscriptionAsync(Guid id, CreateWebhookSubscriptionDto input);

    /// <summary>
    /// Delete webhook subscription
    /// </summary>
    Task DeleteSubscriptionAsync(Guid id);

    /// <summary>
    /// Trigger webhook manually for testing
    /// </summary>
    Task<bool> TriggerWebhookAsync(Guid id, WebhookEventDto eventData);
}

public interface IDataImportAppService : IApplicationService
{
    /// <summary>
    /// Import data from external system
    /// </summary>
    Task<ImportResultDto> ImportDataAsync(CreateDataImportRequestDto input);

    /// <summary>
    /// Get import request history
    /// </summary>
    Task<PagedResultDto<DataImportRequestDto>> GetImportHistoryAsync(PagedAndSortedResultRequestDto input);

    /// <summary>
    /// Get import request details
    /// </summary>
    Task<DataImportRequestDto> GetImportRequestAsync(Guid id);

    /// <summary>
    /// Retry failed import
    /// </summary>
    Task<ImportResultDto> RetryImportAsync(Guid id);
}
