using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEMカスタマイズリポジトリインターフェース
/// </summary>
public interface IOemCustomizationRepository : IRepository<OemCustomization, Guid>
{
    /// <summary>
    /// エンティティとOEMでカスタマイズを取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>カスタマイズリスト</returns>
    Task<List<OemCustomization>> GetByEntityAndOemAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// OEMでカスタマイズを取得する
    /// </summary>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>カスタマイズリスト</returns>
    Task<List<OemCustomization>> GetByOemAsync(
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティでカスタマイズを取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>カスタマイズリスト</returns>
    Task<List<OemCustomization>> GetByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 最新のカスタマイズを取得する
    /// </summary>
    /// <param name="entityId">エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>最新のカスタマイズ（存在しない場合はnull）</returns>
    Task<OemCustomization?> GetLatestCustomizationAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 承認待ちのカスタマイズを取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>承認待ちカスタマイズリスト</returns>
    Task<List<OemCustomization>> GetPendingCustomizationsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// カスタマイズタイプでカスタマイズを取得する
    /// </summary>
    /// <param name="customizationType">カスタマイズタイプ</param>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>カスタマイズリスト</returns>
    Task<List<OemCustomization>> GetByTypeAsync(
        CustomizationType customizationType, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 期間内のカスタマイズを取得する
    /// </summary>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>カスタマイズリスト</returns>
    Task<List<OemCustomization>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// カスタマイズ統計を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>統計情報</returns>
    Task<Dictionary<CustomizationType, int>> GetCustomizationStatisticsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default);
}