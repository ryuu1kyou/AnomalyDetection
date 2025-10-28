using System;
using System.Collections.Generic;
using AnomalyDetection.AuditLogging;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.Application.Contracts.AuditLogging;

/// <summary>
/// 監査ログDTO
/// </summary>
public class AuditLogDto : EntityDto<Guid>
{
    /// <summary>
    /// テナントID
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 対象エンティティのID
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// 対象エンティティの種類
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// 実行されたアクション
    /// </summary>
    public AuditLogAction Action { get; set; }

    /// <summary>
    /// ログレベル
    /// </summary>
    public AuditLogLevel Level { get; set; }

    /// <summary>
    /// アクションの説明
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 変更前の値（JSON形式）
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// 変更後の値（JSON形式）
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// 追加のメタデータ
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = [];

    /// <summary>
    /// IPアドレス
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// ユーザーエージェント
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// セッションID
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 実行時間（ミリ秒）
    /// </summary>
    public long? ExecutionDuration { get; set; }

    /// <summary>
    /// 例外情報
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// 作成者ID
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    /// 作成者名
    /// </summary>
    public string? CreatorName { get; set; }
}