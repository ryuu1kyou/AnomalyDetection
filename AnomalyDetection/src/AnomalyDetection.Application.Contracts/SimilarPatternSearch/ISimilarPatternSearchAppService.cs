using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.SimilarPatternSearch.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 類似パターン検索アプリケーションサービスのインターフェース
/// </summary>
public interface ISimilarPatternSearchAppService : IApplicationService
{
    /// <summary>
    /// 類似CAN信号を検索する
    /// </summary>
    /// <param name="request">検索リクエスト</param>
    /// <returns>類似信号検索結果のリスト</returns>
    Task<List<SimilarSignalResultDto>> SearchSimilarSignalsAsync(SimilarSignalSearchRequestDto request);

    /// <summary>
    /// 検査データを比較する
    /// </summary>
    /// <param name="request">比較リクエスト</param>
    /// <returns>検査データ比較結果</returns>
    Task<TestDataComparisonDto> CompareTestDataAsync(TestDataComparisonRequestDto request);

    /// <summary>
    /// 類似信号の推奨事項を取得する
    /// </summary>
    /// <param name="request">推奨リクエスト</param>
    /// <returns>推奨事項のリスト</returns>
    Task<List<SimilarSignalRecommendationDto>> GetSimilarSignalRecommendationsAsync(
        SimilarSignalRecommendationRequestDto request);

    /// <summary>
    /// 比較結果をエクスポートする
    /// </summary>
    /// <param name="request">エクスポートリクエスト</param>
    /// <returns>エクスポートされたファイルのバイト配列</returns>
    Task<byte[]> ExportComparisonResultAsync(ComparisonExportRequestDto request);

    /// <summary>
    /// 過去の検査データ一覧を取得する
    /// </summary>
    /// <param name="signalId">信号ID</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <param name="maxResults">最大結果数</param>
    /// <returns>検査データ一覧</returns>
    Task<List<HistoricalTestDataDto>> GetHistoricalTestDataAsync(
        Guid signalId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxResults = 1000);

    /// <summary>
    /// 類似度計算の詳細を取得する
    /// </summary>
    /// <param name="signal1Id">比較対象信号1のID</param>
    /// <param name="signal2Id">比較対象信号2のID</param>
    /// <param name="criteria">比較条件</param>
    /// <returns>類似度計算の詳細結果</returns>
    Task<SimilarityCalculationDetailDto> GetSimilarityCalculationDetailAsync(
        Guid signal1Id,
        Guid signal2Id,
        SimilaritySearchCriteriaDto criteria);

    /// <summary>
    /// 類似パターン検索の統計情報を取得する
    /// </summary>
    /// <param name="signalId">対象信号ID</param>
    /// <param name="period">集計期間（日数）</param>
    /// <returns>検索統計情報</returns>
    Task<SimilarPatternSearchStatisticsDto> GetSearchStatisticsAsync(
        Guid signalId,
        int period = 30);
}