using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.Application.Contracts.OemTraceability.Dtos;
using AnomalyDetection.OemTraceability;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Application.Contracts.OemTraceability;

/// <summary>
/// OEMトレーサビリティアプリケーションサービスインターフェース
/// </summary>
public interface IOemTraceabilityAppService : IApplicationService
{
    /// <summary>
    /// OEMトレーサビリティを取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <returns>OEMトレーサビリティ結果</returns>
    Task<OemTraceabilityDto> GetOemTraceabilityAsync(Guid entityId, string entityType);

    /// <summary>
    /// OEMカスタマイズを作成する
    /// </summary>
    /// <param name="input">作成DTO</param>
    /// <returns>作成されたカスタマイズのID</returns>
    Task<Guid> CreateOemCustomizationAsync(CreateOemCustomizationDto input);

    /// <summary>
    /// OEMカスタマイズを更新する
    /// </summary>
    /// <param name="id">カスタマイズID</param>
    /// <param name="input">更新DTO</param>
    /// <returns>更新されたカスタマイズ</returns>
    Task<OemCustomizationDto> UpdateOemCustomizationAsync(Guid id, UpdateOemCustomizationDto input);

    /// <summary>
    /// OEMカスタマイズを取得する
    /// </summary>
    /// <param name="id">カスタマイズID</param>
    /// <returns>カスタマイズ詳細</returns>
    Task<OemCustomizationDto> GetOemCustomizationAsync(Guid id);

    /// <summary>
    /// OEMカスタマイズリストを取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="entityType">エンティティタイプ（オプション）</param>
    /// <param name="status">ステータス（オプション）</param>
    /// <returns>カスタマイズリスト</returns>
    Task<List<OemCustomizationDto>> GetOemCustomizationsAsync(
        string? oemCode = null, 
        string? entityType = null, 
        CustomizationStatus? status = null);

    /// <summary>
    /// カスタマイズを承認申請する
    /// </summary>
    /// <param name="id">カスタマイズID</param>
    /// <returns>更新されたカスタマイズ</returns>
    Task<OemCustomizationDto> SubmitForApprovalAsync(Guid id);

    /// <summary>
    /// カスタマイズを承認する
    /// </summary>
    /// <param name="id">カスタマイズID</param>
    /// <param name="approvalNotes">承認コメント</param>
    /// <returns>更新されたカスタマイズ</returns>
    Task<OemCustomizationDto> ApproveCustomizationAsync(Guid id, string? approvalNotes = null);

    /// <summary>
    /// カスタマイズを却下する
    /// </summary>
    /// <param name="id">カスタマイズID</param>
    /// <param name="rejectionNotes">却下理由</param>
    /// <returns>更新されたカスタマイズ</returns>
    Task<OemCustomizationDto> RejectCustomizationAsync(Guid id, string rejectionNotes);

    /// <summary>
    /// OEM承認を作成する
    /// </summary>
    /// <param name="input">作成DTO</param>
    /// <returns>作成された承認のID</returns>
    Task<Guid> CreateOemApprovalAsync(CreateOemApprovalDto input);

    /// <summary>
    /// OEM承認を取得する
    /// </summary>
    /// <param name="id">承認ID</param>
    /// <returns>承認詳細</returns>
    Task<OemApprovalDto> GetOemApprovalAsync(Guid id);

    /// <summary>
    /// 承認待ちリストを取得する
    /// </summary>
    /// <param name="oemCode">OEMコード</param>
    /// <returns>承認待ちリスト</returns>
    Task<List<OemApprovalDto>> GetPendingApprovalsAsync(string oemCode);

    /// <summary>
    /// 承認を実行する
    /// </summary>
    /// <param name="id">承認ID</param>
    /// <param name="approvalNotes">承認コメント</param>
    /// <returns>更新された承認</returns>
    Task<OemApprovalDto> ApproveAsync(Guid id, string? approvalNotes = null);

    /// <summary>
    /// 承認を却下する
    /// </summary>
    /// <param name="id">承認ID</param>
    /// <param name="rejectionNotes">却下理由</param>
    /// <returns>更新された承認</returns>
    Task<OemApprovalDto> RejectApprovalAsync(Guid id, string rejectionNotes);

    /// <summary>
    /// 緊急承認リストを取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <returns>緊急承認リスト</returns>
    Task<List<OemApprovalDto>> GetUrgentApprovalsAsync(string? oemCode = null);

    /// <summary>
    /// 期限切れ承認リストを取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <returns>期限切れ承認リスト</returns>
    Task<List<OemApprovalDto>> GetOverdueApprovalsAsync(string? oemCode = null);

    /// <summary>
    /// OEMトレーサビリティレポートを生成する
    /// </summary>
    /// <param name="input">レポート生成DTO</param>
    /// <returns>生成されたレポート</returns>
    Task<OemTraceabilityReportDto> GenerateOemTraceabilityReportAsync(GenerateOemTraceabilityReportDto input);

    /// <summary>
    /// カスタマイズ統計を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <returns>統計情報</returns>
    Task<Dictionary<CustomizationType, int>> GetCustomizationStatisticsAsync(string? oemCode = null);

    /// <summary>
    /// 承認統計を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <returns>統計情報</returns>
    Task<Dictionary<ApprovalStatus, int>> GetApprovalStatisticsAsync(string? oemCode = null);
}