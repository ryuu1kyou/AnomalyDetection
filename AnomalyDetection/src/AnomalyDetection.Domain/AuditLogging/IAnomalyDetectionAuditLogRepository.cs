using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.AuditLogging;

/// <summary>
/// 監査ログリポジトリのインターフェース
/// </summary>
public interface IAnomalyDetectionAuditLogRepository : IRepository<AnomalyDetectionAuditLog, Guid>
{
    /// <summary>
    /// エンティティIDで監査ログを取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AnomalyDetectionAuditLog>> GetByEntityIdAsync(
        Guid entityId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティタイプで監査ログを取得する
    /// </summary>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AnomalyDetectionAuditLog>> GetByEntityTypeAsync(
        string entityType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ユーザーIDで監査ログを取得する
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AnomalyDetectionAuditLog>> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 日付範囲で監査ログを取得する
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AnomalyDetectionAuditLog>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// アクションタイプで監査ログを取得する
    /// </summary>
    /// <param name="action">アクション</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AnomalyDetectionAuditLog>> GetByActionAsync(
        AuditLogAction action, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 重要度レベルで監査ログを取得する
    /// </summary>
    /// <param name="level">重要度レベル</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AnomalyDetectionAuditLog>> GetByLevelAsync(
        AuditLogLevel level, 
        CancellationToken cancellationToken = default);
}