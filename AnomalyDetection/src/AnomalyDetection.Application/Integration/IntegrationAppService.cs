using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.Integration;

public class IntegrationEndpointAppService : ApplicationService, IIntegrationEndpointAppService
{
    private readonly IRepository<IntegrationEndpoint, Guid> _endpointRepository;

    public IntegrationEndpointAppService(IRepository<IntegrationEndpoint, Guid> endpointRepository)
    {
        _endpointRepository = endpointRepository;
    }

    public async Task<IntegrationEndpointDto> CreateAsync(CreateIntegrationEndpointDto input)
    {
        var endpoint = new IntegrationEndpoint(
            GuidGenerator.Create(),
            input.Name,
            (IntegrationType)input.Type,
            input.BaseUrl,
            input.Description
        );

        if (input.IsActive.HasValue)
        {
            endpoint.IsActive = input.IsActive.Value;
        }

        if (input.Timeout.HasValue)
        {
            endpoint.Timeout = input.Timeout.Value;
        }

        var created = await _endpointRepository.InsertAsync(endpoint, autoSave: true);
        return ObjectMapper.Map<IntegrationEndpoint, IntegrationEndpointDto>(created);
    }

    public async Task<IntegrationEndpointDto> UpdateAsync(Guid id, UpdateIntegrationEndpointDto input)
    {
        var endpoint = await _endpointRepository.GetAsync(id);

        endpoint.Name = input.Name;
        endpoint.Description = input.Description;
        endpoint.BaseUrl = input.BaseUrl;
        endpoint.IsActive = input.IsActive;

        if (input.Timeout.HasValue)
        {
            endpoint.Timeout = input.Timeout.Value;
        }

        var updated = await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
        return ObjectMapper.Map<IntegrationEndpoint, IntegrationEndpointDto>(updated);
    }

    public async Task<PagedResultDto<IntegrationEndpointDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();

        var query = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);
        var totalCount = await _endpointRepository.GetCountAsync();

        return new PagedResultDto<IntegrationEndpointDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<IntegrationEndpoint>, System.Collections.Generic.List<IntegrationEndpointDto>>(items)
        );
    }

    public async Task<IntegrationEndpointDto> GetAsync(Guid id)
    {
        var endpoint = await _endpointRepository.GetAsync(id);
        return ObjectMapper.Map<IntegrationEndpoint, IntegrationEndpointDto>(endpoint);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _endpointRepository.DeleteAsync(id);
    }

    public async Task<bool> TestConnectionAsync(Guid id)
    {
        var endpoint = await _endpointRepository.GetAsync(id);

        // Basic connectivity test
        try
        {
            switch (endpoint.Type)
            {
                case IntegrationType.RestApi:
                case IntegrationType.GraphQL:
                    using (var httpClient = new System.Net.Http.HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(endpoint.Timeout);
                        var response = await httpClient.GetAsync(endpoint.BaseUrl);
                        return response.IsSuccessStatusCode;
                    }

                case IntegrationType.Database:
                    // Database connection test would require connection string parsing
                    return true;

                case IntegrationType.FileSystem:
                    // File system path validation
                    return System.IO.Directory.Exists(endpoint.BaseUrl) || System.IO.File.Exists(endpoint.BaseUrl);

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<PagedResultDto<IntegrationLogDto>> GetLogsAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        var endpoint = await _endpointRepository.GetAsync(id);

        var logs = endpoint.Logs
            .OrderByDescending(l => l.Timestamp)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<IntegrationLogDto>(
            endpoint.Logs.Count,
            ObjectMapper.Map<System.Collections.Generic.List<IntegrationLog>, System.Collections.Generic.List<IntegrationLogDto>>(logs)
        );
    }
}

public class WebhookAppService : ApplicationService, IWebhookAppService
{
    private readonly IRepository<IntegrationEndpoint, Guid> _endpointRepository;

    public WebhookAppService(IRepository<IntegrationEndpoint, Guid> endpointRepository)
    {
        _endpointRepository = endpointRepository;
    }

    public async Task<WebhookSubscriptionDto> CreateSubscriptionAsync(Guid endpointId, CreateWebhookSubscriptionDto input)
    {
        var endpoint = await _endpointRepository.GetAsync(endpointId);

        var subscription = endpoint.AddWebhook(
            GuidGenerator.Create(),
            input.EventType,
            input.TargetUrl,
            input.IsActive
        );

        await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
        return ObjectMapper.Map<WebhookSubscription, WebhookSubscriptionDto>(subscription);
    }

    public async Task<PagedResultDto<WebhookSubscriptionDto>> GetSubscriptionsAsync(Guid endpointId, PagedAndSortedResultRequestDto input)
    {
        var endpoint = await _endpointRepository.GetAsync(endpointId);

        var subscriptions = endpoint.Webhooks
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<WebhookSubscriptionDto>(
            endpoint.Webhooks.Count,
            ObjectMapper.Map<System.Collections.Generic.List<WebhookSubscription>, System.Collections.Generic.List<WebhookSubscriptionDto>>(subscriptions)
        );
    }

    public async Task<WebhookSubscriptionDto> UpdateSubscriptionAsync(Guid id, CreateWebhookSubscriptionDto input)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();
        var endpoint = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(e => e.Webhooks.Any(w => w.Id == id))
        );

        if (endpoint == null)
        {
            throw new Volo.Abp.BusinessException("Webhook subscription not found");
        }

        var webhook = endpoint.Webhooks.First(w => w.Id == id);
        webhook.EventType = input.EventType;
        webhook.TargetUrl = input.TargetUrl;
        webhook.IsActive = input.IsActive;

        await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
        return ObjectMapper.Map<WebhookSubscription, WebhookSubscriptionDto>(webhook);
    }

    public async Task DeleteSubscriptionAsync(Guid id)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();
        var endpoint = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(e => e.Webhooks.Any(w => w.Id == id))
        );

        if (endpoint != null)
        {
            var webhook = endpoint.Webhooks.First(w => w.Id == id);
            endpoint.Webhooks.Remove(webhook);
            await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
        }
    }

    public async Task<bool> TriggerWebhookAsync(Guid id, WebhookEventDto eventData)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();
        var endpoint = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(e => e.Webhooks.Any(w => w.Id == id))
        );

        if (endpoint == null)
        {
            return false;
        }

        var webhook = endpoint.Webhooks.First(w => w.Id == id);

        try
        {
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(endpoint.Timeout);

            var content = new System.Net.Http.StringContent(
                System.Text.Json.JsonSerializer.Serialize(eventData),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync(webhook.TargetUrl, content);

            if (response.IsSuccessStatusCode)
            {
                webhook.RecordSuccess();
                await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
                return true;
            }
            else
            {
                webhook.RecordFailure();
                await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
                return false;
            }
        }
        catch
        {
            webhook.RecordFailure();
            await _endpointRepository.UpdateAsync(endpoint, autoSave: true);
            return false;
        }
    }
}

public class DataImportAppService : ApplicationService, IDataImportAppService
{
    private readonly IRepository<IntegrationEndpoint, Guid> _endpointRepository;

    public DataImportAppService(IRepository<IntegrationEndpoint, Guid> endpointRepository)
    {
        _endpointRepository = endpointRepository;
    }

    public async Task<ImportResultDto> ImportDataAsync(CreateDataImportRequestDto input)
    {
        var endpoint = await _endpointRepository.GetAsync(input.EndpointId);

        var importRequest = endpoint.CreateImportRequest(
            GuidGenerator.Create(),
            input.DataType,
            input.Filter
        );

        // Process import based on endpoint type
        try
        {
            var importedCount = await ProcessImportAsync(endpoint, importRequest);

            importRequest.Complete(importedCount);
            endpoint.RecordSuccess();

            await _endpointRepository.UpdateAsync(endpoint, autoSave: true);

            return new ImportResultDto
            {
                Success = true,
                RecordsImported = importedCount,
                Message = $"Successfully imported {importedCount} records"
            };
        }
        catch (Exception ex)
        {
            importRequest.Fail(ex.Message);
            endpoint.RecordFailure();

            await _endpointRepository.UpdateAsync(endpoint, autoSave: true);

            return new ImportResultDto
            {
                Success = false,
                RecordsImported = 0,
                Message = $"Import failed: {ex.Message}"
            };
        }
    }

    private async Task<int> ProcessImportAsync(IntegrationEndpoint endpoint, DataImportRequest request)
    {
        // Placeholder implementation - would be replaced with actual import logic
        switch (endpoint.Type)
        {
            case IntegrationType.RestApi:
                return await ImportFromRestApiAsync(endpoint, request);

            case IntegrationType.Database:
                return await ImportFromDatabaseAsync(endpoint, request);

            case IntegrationType.FileSystem:
                return await ImportFromFileSystemAsync(endpoint, request);

            default:
                throw new NotSupportedException($"Import not supported for {endpoint.Type}");
        }
    }

    private async Task<int> ImportFromRestApiAsync(IntegrationEndpoint endpoint, DataImportRequest request)
    {
        // Placeholder: Fetch data from REST API
        await Task.Delay(100);
        return 0;
    }

    private async Task<int> ImportFromDatabaseAsync(IntegrationEndpoint endpoint, DataImportRequest request)
    {
        // Placeholder: Query database
        await Task.Delay(100);
        return 0;
    }

    private async Task<int> ImportFromFileSystemAsync(IntegrationEndpoint endpoint, DataImportRequest request)
    {
        // Placeholder: Read files
        await Task.Delay(100);
        return 0;
    }

    public async Task<PagedResultDto<DataImportRequestDto>> GetImportHistoryAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();

        var allRequests = await AsyncExecuter.ToListAsync(
            queryable.SelectMany(e => e.ImportRequests)
        );

        var sorted = allRequests
            .OrderByDescending(r => r.RequestedAt)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<DataImportRequestDto>(
            allRequests.Count,
            ObjectMapper.Map<System.Collections.Generic.List<DataImportRequest>, System.Collections.Generic.List<DataImportRequestDto>>(sorted)
        );
    }

    public async Task<DataImportRequestDto> GetImportRequestAsync(Guid id)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();
        var endpoint = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(e => e.ImportRequests.Any(r => r.Id == id))
        );

        if (endpoint == null)
        {
            throw new Volo.Abp.BusinessException("Import request not found");
        }

        var request = endpoint.ImportRequests.First(r => r.Id == id);
        return ObjectMapper.Map<DataImportRequest, DataImportRequestDto>(request);
    }

    public async Task<ImportResultDto> RetryImportAsync(Guid id)
    {
        var queryable = await _endpointRepository.GetQueryableAsync();
        var endpoint = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(e => e.ImportRequests.Any(r => r.Id == id))
        );

        if (endpoint == null)
        {
            throw new Volo.Abp.BusinessException("Import request not found");
        }

        var originalRequest = endpoint.ImportRequests.First(r => r.Id == id);

        // Create new import request with same parameters
        var retryRequest = endpoint.CreateImportRequest(
            GuidGenerator.Create(),
            originalRequest.DataType,
            originalRequest.Filter
        );

        try
        {
            var importedCount = await ProcessImportAsync(endpoint, retryRequest);

            retryRequest.Complete(importedCount);
            endpoint.RecordSuccess();

            await _endpointRepository.UpdateAsync(endpoint, autoSave: true);

            return new ImportResultDto
            {
                Success = true,
                RecordsImported = importedCount,
                Message = $"Retry successful: imported {importedCount} records"
            };
        }
        catch (Exception ex)
        {
            retryRequest.Fail(ex.Message);
            endpoint.RecordFailure();

            await _endpointRepository.UpdateAsync(endpoint, autoSave: true);

            return new ImportResultDto
            {
                Success = false,
                RecordsImported = 0,
                Message = $"Retry failed: {ex.Message}"
            };
        }
    }
}
