using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AnomalyDetection.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace AnomalyDetection.Application.AuditLogging;

/// <summary>
/// 監査ログサービスの実装
/// </summary>
public class AuditLogService : IAuditLogService, ITransientDependency
{
    private readonly IAnomalyDetectionAuditLogRepository _auditLogRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        IAnomalyDetectionAuditLogRepository auditLogRepository,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AnomalyDetectionAuditLog> LogAsync(
        Guid? entityId,
        string entityType,
        AuditLogAction action,
        string description,
        AuditLogLevel level = AuditLogLevel.Information,
        object? oldValues = null,
        object? newValues = null,
        Dictionary<string, object>? metadata = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var auditLog = new AnomalyDetectionAuditLog(
            tenantId: _currentTenant.Id,
            entityId: entityId,
            entityType: entityType,
            action: action,
            level: level,
            description: description,
            oldValues: oldValues != null ? SanitizeJson(JsonSerializer.Serialize(oldValues)) : null,
            newValues: newValues != null ? SanitizeJson(JsonSerializer.Serialize(newValues)) : null,
            metadata: metadata,
            ipAddress: httpContext?.Connection?.RemoteIpAddress?.ToString(),
            userAgent: httpContext?.Request?.Headers["User-Agent"].ToString(),
            sessionId: httpContext?.Session?.Id
        );

        // ユーザー情報をメタデータに追加
        if (_currentUser.IsAuthenticated)
        {
            auditLog.AddMetadata("UserId", _currentUser.Id!.Value);
            auditLog.AddMetadata("UserName", _currentUser.UserName ?? "Unknown");
            auditLog.AddMetadata("Email", _currentUser.Email ?? "Unknown");
        }

        // テナント情報をメタデータに追加
        if (_currentTenant.Id.HasValue)
        {
            auditLog.AddMetadata("TenantId", _currentTenant.Id.Value);
            auditLog.AddMetadata("TenantName", _currentTenant.Name ?? "Unknown");
        }

        return await _auditLogRepository.InsertAsync(auditLog, autoSave: true);
    }

    public async Task<AnomalyDetectionAuditLog> LogCreateAsync(
        Guid entityId,
        string entityType,
        object entity,
        Dictionary<string, object>? metadata = null)
    {
        return await LogAsync(
            entityId: entityId,
            entityType: entityType,
            action: AuditLogAction.Create,
            description: $"{entityType} created with ID: {entityId}",
            level: AuditLogLevel.Information,
            oldValues: null,
            newValues: entity,
            metadata: metadata
        );
    }

    public async Task<AnomalyDetectionAuditLog> LogUpdateAsync(
        Guid entityId,
        string entityType,
        object oldEntity,
        object newEntity,
        Dictionary<string, object>? metadata = null)
    {
        return await LogAsync(
            entityId: entityId,
            entityType: entityType,
            action: AuditLogAction.Update,
            description: $"{entityType} updated with ID: {entityId}",
            level: AuditLogLevel.Information,
            oldValues: oldEntity,
            newValues: newEntity,
            metadata: metadata
        );
    }

    public async Task<AnomalyDetectionAuditLog> LogDeleteAsync(
        Guid entityId,
        string entityType,
        object entity,
        Dictionary<string, object>? metadata = null)
    {
        return await LogAsync(
            entityId: entityId,
            entityType: entityType,
            action: AuditLogAction.Delete,
            description: $"{entityType} deleted with ID: {entityId}",
            level: AuditLogLevel.Warning,
            oldValues: entity,
            newValues: null,
            metadata: metadata
        );
    }

    public async Task<AnomalyDetectionAuditLog> LogApprovalAsync(
        Guid entityId,
        string entityType,
        Guid approvedBy,
        string? notes = null,
        Dictionary<string, object>? metadata = null)
    {
        var auditMetadata = metadata ?? [];
        auditMetadata["ApprovedBy"] = approvedBy;
        if (!string.IsNullOrEmpty(notes))
        {
            auditMetadata["ApprovalNotes"] = notes;
        }

        return await LogAsync(
            entityId: entityId,
            entityType: entityType,
            action: AuditLogAction.Approve,
            description: $"{entityType} approved by user {approvedBy}",
            level: AuditLogLevel.Information,
            metadata: auditMetadata
        );
    }

    public async Task<AnomalyDetectionAuditLog> LogRejectionAsync(
        Guid entityId,
        string entityType,
        Guid rejectedBy,
        string reason,
        Dictionary<string, object>? metadata = null)
    {
        var auditMetadata = metadata ?? [];
        auditMetadata["RejectedBy"] = rejectedBy;
        auditMetadata["RejectionReason"] = reason;

        return await LogAsync(
            entityId: entityId,
            entityType: entityType,
            action: AuditLogAction.Reject,
            description: $"{entityType} rejected by user {rejectedBy}: {reason}",
            level: AuditLogLevel.Warning,
            metadata: auditMetadata
        );
    }

    public async Task<AnomalyDetectionAuditLog> LogDetectionExecutionAsync(
        Guid logicId,
        Guid signalId,
        long executionTime,
        object result,
        Dictionary<string, object>? metadata = null)
    {
        var auditMetadata = metadata ?? [];
        auditMetadata["DetectionLogicId"] = logicId;
        auditMetadata["SignalId"] = signalId;
        auditMetadata["ExecutionTimeMs"] = executionTime;

        var auditLog = await LogAsync(
            entityId: logicId,
            entityType: "DetectionExecution",
            action: AuditLogAction.Execute,
            description: $"Detection logic {logicId} executed on signal {signalId}",
            level: AuditLogLevel.Information,
            newValues: result,
            metadata: auditMetadata
        );

        auditLog.SetExecutionDuration(executionTime);
        await _auditLogRepository.UpdateAsync(auditLog);

        return auditLog;
    }

    public async Task<AnomalyDetectionAuditLog> LogSecurityEventAsync(
        AuditLogAction action,
        string description,
        AuditLogLevel level = AuditLogLevel.Warning,
        Dictionary<string, object>? metadata = null)
    {
        var auditMetadata = metadata ?? [];
        auditMetadata["SecurityEvent"] = true;

        return await LogAsync(
            entityId: null,
            entityType: "Security",
            action: action,
            description: description,
            level: level,
            metadata: auditMetadata
        );
    }
    private string SanitizeJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return json;
        // Regex to find "key": "value" where key is sensitive
        // Case insensitive matching for Password, Secret, Token, Key, Auth, Credential
        return System.Text.RegularExpressions.Regex.Replace(
            json,
            @"(?i)""(password|secret|token|apikey|auth|credential|access_?token)""\s*:\s*""[^""]*""",
            @"""$1"": ""***"""
        );
    }
}