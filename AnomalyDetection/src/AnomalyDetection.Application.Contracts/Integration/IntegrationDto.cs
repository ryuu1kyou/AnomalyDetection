using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.Integration;

public class IntegrationEndpointDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int Type { get; set; }
    public string BaseUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public int Timeout { get; set; }

    public DateTime? LastSyncDate { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class CreateIntegrationEndpointDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
    public int? Timeout { get; set; }
}

public class UpdateIntegrationEndpointDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? Timeout { get; set; }
}

public class WebhookSubscriptionDto
{
    public Guid Id { get; set; }
    public string TargetUrl { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int MaxRetries { get; set; }
    public int TimeoutSeconds { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int DeliverySuccessCount { get; set; }
    public int DeliveryFailureCount { get; set; }
}

public class CreateWebhookSubscriptionDto
{
    public string TargetUrl { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class IntegrationLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int Level { get; set; }
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; }
    public long DurationMs { get; set; }
}

public class DataImportRequestDto : FullAuditedEntityDto<Guid>
{
    public Guid EndpointId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public int RecordsImported { get; set; }
    public string? ErrorMessage { get; set; }
    public string Filter { get; set; } = string.Empty;
}

public class CreateDataImportRequestDto
{
    public Guid EndpointId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
}

public class ImportResultDto
{
    public bool Success { get; set; }
    public int RecordsImported { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class WebhookEventDto
{
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public object? Data { get; set; }
}
