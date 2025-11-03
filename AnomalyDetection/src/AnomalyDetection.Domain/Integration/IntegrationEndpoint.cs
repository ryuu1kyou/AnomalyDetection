using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.Integration;

/// <summary>
/// External integration endpoint configuration
/// Supports data import from external systems
/// </summary>
public class IntegrationEndpoint : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public IntegrationType Type { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }

    public bool IsActive { get; set; }
    public int Timeout { get; set; } = 30;
    public bool RequireAuthentication { get; set; }

    public string? AuthenticationScheme { get; set; } // Bearer, ApiKey, Basic
    public string? Configuration { get; set; } // JSON configuration

    public DateTime? LastSyncDate { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }

    public List<WebhookSubscription> Webhooks { get; set; } = new();
    public List<IntegrationLog> Logs { get; set; } = new();
    public List<DataImportRequest> ImportRequests { get; set; } = new();

    private IntegrationEndpoint() { }

    public IntegrationEndpoint(
        Guid id,
        string name,
        IntegrationType type,
        string baseUrl,
        string? description = null
    ) : base(id)
    {
        Name = name;
        Type = type;
        BaseUrl = baseUrl;
        EndpointUrl = baseUrl;
        Description = description ?? string.Empty;
        IsActive = true;
    }

    public void RecordSuccess()
    {
        SuccessCount++;
        LastSyncDate = DateTime.UtcNow;
    }

    public void RecordFailure()
    {
        FailureCount++;
    }

    public WebhookSubscription AddWebhook(Guid id, string eventType, string targetUrl, bool isActive = true)
    {
        var webhook = new WebhookSubscription(id, eventType, targetUrl, isActive);
        Webhooks.Add(webhook);
        return webhook;
    }

    public void AddLog(IntegrationLog log)
    {
        Logs.Add(log);
    }

    public DataImportRequest CreateImportRequest(Guid id, string dataType, string filter)
    {
        var request = new DataImportRequest(id, Id, dataType, filter);
        ImportRequests.Add(request);
        return request;
    }
}

public enum IntegrationType
{
    RestApi = 0,
    GraphQL = 1,
    Mqtt = 2,
    WebSocket = 3,
    Database = 4,
    FileSystem = 5
}

/// <summary>
/// Webhook subscription for event notifications
/// </summary>
public class WebhookSubscription
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }

    public string TargetUrl { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // AnomalyDetected, SpecImported, etc.

    public bool IsActive { get; set; }
    public string? Secret { get; set; } // For signature verification

    public int MaxRetries { get; set; }
    public int TimeoutSeconds { get; set; }

    public DateTime? LastTriggeredAt { get; set; }
    public int DeliverySuccessCount { get; set; }
    public int DeliveryFailureCount { get; set; }

    public WebhookSubscription(Guid id, string eventType, string targetUrl, bool isActive = true)
    {
        Id = id;
        TargetUrl = targetUrl;
        WebhookUrl = targetUrl;
        EventType = eventType;
        IsActive = isActive;
        MaxRetries = 3;
        TimeoutSeconds = 30;
    }

    private WebhookSubscription() { Id = Guid.NewGuid(); }

    public void RecordSuccess()
    {
        DeliverySuccessCount++;
        LastTriggeredAt = DateTime.UtcNow;
    }

    public void RecordFailure()
    {
        DeliveryFailureCount++;
    }
}

/// <summary>
/// Integration activity log
/// </summary>
public class IntegrationLog
{
    public Guid Id { get; set; }
    public Guid EndpointId { get; set; }

    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Operation { get; set; } = string.Empty;

    public bool Success { get; set; }
    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public string? ErrorMessage { get; set; }

    public int? StatusCode { get; set; }
    public long DurationMs { get; set; }

    public IntegrationLog(string operation, bool success)
    {
        Id = Guid.NewGuid();
        Operation = operation;
        Success = success;
        Timestamp = DateTime.UtcNow;
        Level = success ? LogLevel.Info : LogLevel.Error;
    }

    private IntegrationLog() { Id = Guid.NewGuid(); }
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

/// <summary>
/// Data import request from external system
/// </summary>
public class DataImportRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid EndpointId { get; set; }
    public string DataType { get; set; } = string.Empty; // CanMessage, AnomalyResult, etc.

    public ImportStatus Status { get; set; }
    public string Filter { get; set; } = string.Empty;
    public string? Data { get; set; } // JSON data

    public DateTime RequestedAt { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public int RecordsImported { get; set; }
    public string? ErrorMessage { get; set; }

    private DataImportRequest() { }

    public DataImportRequest(Guid id, Guid endpointId, string dataType, string filter) : base(id)
    {
        EndpointId = endpointId;
        DataType = dataType;
        Filter = filter;
        RequestedAt = DateTime.UtcNow;
        RequestDate = DateTime.UtcNow;
        Status = ImportStatus.Pending;
    }

    public void MarkAsProcessing()
    {
        Status = ImportStatus.Parsing;
    }

    public void Complete(int recordCount)
    {
        Status = ImportStatus.Completed;
        RecordsImported = recordCount;
        ProcessedDate = DateTime.UtcNow;
    }

    public void MarkAsCompleted(int recordCount)
    {
        Status = ImportStatus.Completed;
        RecordsImported = recordCount;
        ProcessedDate = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ImportStatus.Failed;
        ErrorMessage = errorMessage;
        ProcessedDate = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ImportStatus.Failed;
        ErrorMessage = errorMessage;
        ProcessedDate = DateTime.UtcNow;
    }
}

public enum ImportStatus
{
    Pending = 0,
    Processing = 1,
    Parsing = 2,
    Completed = 3,
    Failed = 4
}
