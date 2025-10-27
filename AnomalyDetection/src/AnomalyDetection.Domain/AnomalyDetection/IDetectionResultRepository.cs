using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 異常検出結果リポジトリのインターフェース
/// </summary>
public interface IDetectionResultRepository : IRepository<AnomalyDetectionResult, Guid>
{
    /// <summary>
    /// 指定されたCAN信号と期間の検出結果を取得する
    /// </summary>
    /// <param name="canSignalId">CAN信号ID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検出結果のリスト</returns>
    Task<List<AnomalyDetectionResult>> GetByCanSignalAndPeriodAsync(
        Guid canSignalId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定された異常タイプの検出結果を取得する
    /// </summary>
    /// <param name="anomalyType">異常タイプ</param>
    /// <param name="startDate">開始日時（オプション）</param>
    /// <param name="endDate">終了日時（オプション）</param>
    /// <param name="tenantId">テナントID（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検出結果のリスト</returns>
    Task<List<AnomalyDetectionResult>> GetByAnomalyTypeAsync(
        AnomalyType anomalyType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 検出結果の統計情報を取得する
    /// </summary>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="tenantId">テナントID（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>統計情報</returns>
    Task<DetectionResultStatistics> GetStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定された検出ロジックの検出結果を取得する
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検出結果のリスト</returns>
    Task<List<AnomalyDetectionResult>> GetByDetectionLogicAndPeriodAsync(
        Guid detectionLogicId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 異常レベル別の検出結果を取得する
    /// </summary>
    /// <param name="anomalyLevel">異常レベル</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="tenantId">テナントID（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検出結果のリスト</returns>
    Task<List<AnomalyDetectionResult>> GetByAnomalyLevelAsync(
        AnomalyLevel anomalyLevel,
        DateTime startDate,
        DateTime endDate,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 未解決の検出結果を取得する
    /// </summary>
    /// <param name="tenantId">テナントID（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>未解決の検出結果のリスト</returns>
    Task<List<AnomalyDetectionResult>> GetUnresolvedAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 誤検出の検出結果を取得する
    /// </summary>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="tenantId">テナントID（オプション）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>誤検出の検出結果のリスト</returns>
    Task<List<AnomalyDetectionResult>> GetFalsePositivesAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 検出時間の統計を取得する
    /// </summary>
    /// <param name="detectionLogicId">検出ロジックID（オプション）</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検出時間統計</returns>
    Task<DetectionTimeStatistics> GetDetectionTimeStatisticsAsync(
        Guid? detectionLogicId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}