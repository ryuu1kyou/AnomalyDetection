using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AuditLogging;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Application.Contracts.AuditLogging;

/// <summary>
/// 監査ログアプリケーションサービスのインターフェース
/// </summary>
public interface IAuditLogAppService : IApplicationService
{
    /// <summary>
    /// エンティティの監査ログを取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AuditLogDto>> GetEntityAuditLogsAsync(Guid entityId, string? entityType = null);

    /// <summary>
    /// ユーザーの監査ログを取得する
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AuditLogDto>> GetUserAuditLogsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 重要度レベル別の監査ログを取得する
    /// </summary>
    /// <param name="level">重要度レベル</param>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AuditLogDto>> GetAuditLogsByLevelAsync(AuditLogLevel level, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// アクション別の監査ログを取得する
    /// </summary>
    /// <param name="action">アクション</param>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AuditLogDto>> GetAuditLogsByActionAsync(AuditLogAction action, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// セキュリティ関連の監査ログを取得する
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <returns>監査ログのリスト</returns>
    Task<List<AuditLogDto>> GetSecurityAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
}