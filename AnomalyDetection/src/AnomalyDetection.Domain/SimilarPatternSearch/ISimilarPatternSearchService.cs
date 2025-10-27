using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 類似パターン検索サービスのインターフェース
/// </summary>
public interface ISimilarPatternSearchService
{
    /// <summary>
    /// 類似CAN信号を検索する
    /// </summary>
    /// <param name="targetSignal">検索対象の信号</param>
    /// <param name="criteria">検索条件</param>
    /// <param name="candidateSignals">候補信号リスト（nullの場合は全信号から検索）</param>
    /// <returns>類似信号検索結果のリスト</returns>
    Task<IEnumerable<SimilarSignalResult>> SearchSimilarSignalsAsync(
        CanSignal targetSignal,
        SimilaritySearchCriteria criteria,
        IEnumerable<CanSignal>? candidateSignals = null);

    /// <summary>
    /// 検査データを比較する
    /// </summary>
    /// <param name="sourceResults">比較元の検出結果リスト</param>
    /// <param name="targetResults">比較先の検出結果リスト</param>
    /// <param name="sourceSignal">比較元の信号</param>
    /// <param name="targetSignal">比較先の信号</param>
    /// <returns>検査データ比較結果</returns>
    Task<TestDataComparison> CompareTestDataAsync(
        IEnumerable<AnomalyDetectionResult> sourceResults,
        IEnumerable<AnomalyDetectionResult> targetResults,
        CanSignal sourceSignal,
        CanSignal targetSignal);

    /// <summary>
    /// 類似度を計算する
    /// </summary>
    /// <param name="signal1">比較対象信号1</param>
    /// <param name="signal2">比較対象信号2</param>
    /// <param name="criteria">比較条件</param>
    /// <returns>類似度スコア（0.0-1.0）</returns>
    double CalculateSimilarity(
        CanSignal signal1,
        CanSignal signal2,
        SimilaritySearchCriteria criteria);

    /// <summary>
    /// 類似度の詳細内訳を計算する
    /// </summary>
    /// <param name="signal1">比較対象信号1</param>
    /// <param name="signal2">比較対象信号2</param>
    /// <param name="criteria">比較条件</param>
    /// <returns>類似度の詳細内訳</returns>
    SimilarityBreakdown CalculateSimilarityBreakdown(
        CanSignal signal1,
        CanSignal signal2,
        SimilaritySearchCriteria criteria);

    /// <summary>
    /// 推奨レベルを決定する
    /// </summary>
    /// <param name="similarityScore">類似度スコア</param>
    /// <param name="breakdown">類似度内訳</param>
    /// <param name="differences">差異リスト</param>
    /// <returns>推奨レベル</returns>
    RecommendationLevel DetermineRecommendationLevel(
        double similarityScore,
        SimilarityBreakdown breakdown,
        IEnumerable<AttributeDifference> differences);
}