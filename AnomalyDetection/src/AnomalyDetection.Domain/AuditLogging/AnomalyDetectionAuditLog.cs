using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.AuditLogging;

/// <summary>
/// CAN異常検出システム専用の監査ログエンティティ
/// </summary>
public class AnomalyDetectionAuditLog : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    /// <summary>
    /// 対象エンティティのID
    /// </summary>
    public Guid? EntityId { get; private set; }
    
    /// <summary>
    /// 対象エンティティの種類
    /// </summary>
    public string EntityType { get; private set; }
    
    /// <summary>
    /// 実行されたアクション
    /// </summary>
    public AuditLogAction Action { get; private set; }
    
    /// <summary>
    /// ログレベル
    /// </summary>
    public AuditLogLevel Level { get; private set; }
    
    /// <summary>
    /// アクションの説明
    /// </summary>
    public string Description { get; private set; }
    
    /// <summary>
    /// 変更前の値（JSON形式）
    /// </summary>
    public string? OldValues { get; private set; }
    
    /// <summary>
    /// 変更後の値（JSON形式）
    /// </summary>
    public string? NewValues { get; private set; }
    
    /// <summary>
    /// 追加のメタデータ
    /// </summary>
    public Dictionary<string, object> Metadata { get; private set; }
    
    /// <summary>
    /// IPアドレス
    /// </summary>
    public string? IpAddress { get; private set; }
    
    /// <summary>
    /// ユーザーエージェント
    /// </summary>
    public string? UserAgent { get; private set; }
    
    /// <summary>
    /// セッションID
    /// </summary>
    public string? SessionId { get; private set; }
    
    /// <summary>
    /// 実行時間（ミリ秒）
    /// </summary>
    public long? ExecutionDuration { get; private set; }
    
    /// <summary>
    /// 例外情報
    /// </summary>
    public string? Exception { get; private set; }

    protected AnomalyDetectionAuditLog() 
    {
        Metadata = [];
    }

    public AnomalyDetectionAuditLog(
        Guid? tenantId,
        Guid? entityId,
        string entityType,
        AuditLogAction action,
        AuditLogLevel level,
        string description,
        string? oldValues = null,
        string? newValues = null,
        Dictionary<string, object>? metadata = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? sessionId = null,
        long? executionDuration = null,
        string? exception = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        EntityId = entityId;
        EntityType = entityType ?? string.Empty;
        Action = action;
        Level = level;
        Description = description ?? string.Empty;
        OldValues = oldValues;
        NewValues = newValues;
        Metadata = metadata ?? [];
        IpAddress = ipAddress;
        UserAgent = userAgent;
        SessionId = sessionId;
        ExecutionDuration = executionDuration;
        Exception = exception;
    }

    /// <summary>
    /// メタデータを追加する
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="value">値</param>
    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }

    /// <summary>
    /// 実行時間を設定する
    /// </summary>
    /// <param name="duration">実行時間（ミリ秒）</param>
    public void SetExecutionDuration(long duration)
    {
        ExecutionDuration = duration;
    }

    /// <summary>
    /// 例外情報を設定する
    /// </summary>
    /// <param name="exception">例外情報</param>
    public void SetException(string exception)
    {
        Exception = exception;
        Level = AuditLogLevel.Error;
    }
}