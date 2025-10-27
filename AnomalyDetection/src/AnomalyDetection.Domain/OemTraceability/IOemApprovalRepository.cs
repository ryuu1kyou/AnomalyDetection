using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEM承認リポジトリインターフェース
/// </summary>
public interface IOemApprovalRepository : IRepository<OemApproval, Guid>
{
    /// <summary>
    /// エンティティとOEMで承認を取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認リスト</returns>
    Task<List<OemApproval>> GetByEntityAndOemAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティで承認を取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認リスト</returns>
    Task<List<OemApproval>> GetByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 承認待ちの承認を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認待ちリスト</returns>
    Task<List<OemApproval>> GetPendingApprovalsAsync(
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 最新の承認を取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>最新の承認（存在しない場合はnull）</returns>
    Task<OemApproval?> GetLatestApprovalAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// OEMで承認を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認リスト</returns>
    Task<List<OemApproval>> GetByOemAsync(
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 承認タイプで承認を取得する
    /// </summary>
    /// <param name="approvalType">承認タイプ</param>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認リスト</returns>
    Task<List<OemApproval>> GetByTypeAsync(
        ApprovalType approvalType, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 期限切れの承認を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>期限切れ承認リスト</returns>
    Task<List<OemApproval>> GetOverdueApprovalsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 緊急承認を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>緊急承認リスト</returns>
    Task<List<OemApproval>> GetUrgentApprovalsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 期間内の承認を取得する
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認リスト</returns>
    Task<List<OemApproval>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 承認統計を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>統計情報</returns>
    Task<Dictionary<ApprovalStatus, int>> GetApprovalStatisticsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 承認者別統計を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認者別統計</returns>
    Task<Dictionary<Guid, int>> GetApprovalsByApproverAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default);
}