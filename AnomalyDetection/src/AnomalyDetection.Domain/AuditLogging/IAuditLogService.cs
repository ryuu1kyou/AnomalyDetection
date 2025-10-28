using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.AuditLogging;

/// <summary>
/// 監査ログサービスのインターフェース
/// </summary>
public interface IAuditLogService : ITransientDependency
{
    /// <summary>
    /// 監査ログを記録する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="action">アクション</param>
    /// <param name="description">説明</param>
    /// <param name="level">重要度レベル</param>
    /// <param name="oldValues">変更前の値</param>
    /// <param name="newValues">変更後の値</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogAsync(
        Guid? entityId,
        string entityType,
        AuditLogAction action,
        string description,
        AuditLogLevel level = AuditLogLevel.Information,
        object? oldValues = null,
        object? newValues = null,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// エンティティ作成の監査ログを記録する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="entity">作成されたエンティティ</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogCreateAsync(
        Guid entityId,
        string entityType,
        object entity,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// エンティティ更新の監査ログを記録する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="oldEntity">更新前のエンティティ</param>
    /// <param name="newEntity">更新後のエンティティ</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogUpdateAsync(
        Guid entityId,
        string entityType,
        object oldEntity,
        object newEntity,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// エンティティ削除の監査ログを記録する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="entity">削除されたエンティティ</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogDeleteAsync(
        Guid entityId,
        string entityType,
        object entity,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// 承認の監査ログを記録する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="approvedBy">承認者ID</param>
    /// <param name="notes">承認メモ</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogApprovalAsync(
        Guid entityId,
        string entityType,
        Guid approvedBy,
        string? notes = null,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// 却下の監査ログを記録する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="rejectedBy">却下者ID</param>
    /// <param name="reason">却下理由</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogRejectionAsync(
        Guid entityId,
        string entityType,
        Guid rejectedBy,
        string reason,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// 検出ロジック実行の監査ログを記録する
    /// </summary>
    /// <param name="logicId">検出ロジックID</param>
    /// <param name="signalId">信号ID</param>
    /// <param name="executionTime">実行時間</param>
    /// <param name="result">実行結果</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogDetectionExecutionAsync(
        Guid logicId,
        Guid signalId,
        long executionTime,
        object result,
        Dictionary<string, object>? metadata = null);

    /// <summary>
    /// セキュリティ関連の監査ログを記録する
    /// </summary>
    /// <param name="action">アクション</param>
    /// <param name="description">説明</param>
    /// <param name="level">重要度レベル</param>
    /// <param name="metadata">メタデータ</param>
    /// <returns>作成された監査ログ</returns>
    Task<AnomalyDetectionAuditLog> LogSecurityEventAsync(
        AuditLogAction action,
        string description,
        AuditLogLevel level = AuditLogLevel.Warning,
        Dictionary<string, object>? metadata = null);
}