using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 類似CAN信号検索の条件を定義する値オブジェクト
/// </summary>
public class SimilaritySearchCriteria : ValueObject
{
    /// <summary>
    /// CAN IDで比較するかどうか
    /// </summary>
    public bool CompareCanId { get; private set; }
    
    /// <summary>
    /// 信号名で比較するかどうか
    /// </summary>
    public bool CompareSignalName { get; private set; }
    
    /// <summary>
    /// システム種別で比較するかどうか
    /// </summary>
    public bool CompareSystemType { get; private set; }
    
    /// <summary>
    /// 最小類似度（0.0-1.0）
    /// </summary>
    public double MinimumSimilarity { get; private set; }
    
    /// <summary>
    /// 最大結果数
    /// </summary>
    public int MaxResults { get; private set; }
    
    /// <summary>
    /// 物理値範囲で比較するかどうか
    /// </summary>
    public bool CompareValueRange { get; private set; }
    
    /// <summary>
    /// データ長で比較するかどうか
    /// </summary>
    public bool CompareDataLength { get; private set; }
    
    /// <summary>
    /// 周期で比較するかどうか
    /// </summary>
    public bool CompareCycle { get; private set; }
    
    /// <summary>
    /// OEMコードで比較するかどうか
    /// </summary>
    public bool CompareOemCode { get; private set; }
    
    /// <summary>
    /// 標準信号のみを対象とするかどうか
    /// </summary>
    public bool StandardSignalsOnly { get; private set; }
    
    /// <summary>
    /// アクティブな信号のみを対象とするかどうか
    /// </summary>
    public bool ActiveSignalsOnly { get; private set; }

    protected SimilaritySearchCriteria() { }

    public SimilaritySearchCriteria(
        bool compareCanId = true,
        bool compareSignalName = true,
        bool compareSystemType = true,
        double minimumSimilarity = 0.5,
        int maxResults = 50,
        bool compareValueRange = false,
        bool compareDataLength = false,
        bool compareCycle = false,
        bool compareOemCode = false,
        bool standardSignalsOnly = false,
        bool activeSignalsOnly = true)
    {
        CompareCanId = compareCanId;
        CompareSignalName = compareSignalName;
        CompareSystemType = compareSystemType;
        MinimumSimilarity = ValidateMinimumSimilarity(minimumSimilarity);
        MaxResults = ValidateMaxResults(maxResults);
        CompareValueRange = compareValueRange;
        CompareDataLength = compareDataLength;
        CompareCycle = compareCycle;
        CompareOemCode = compareOemCode;
        StandardSignalsOnly = standardSignalsOnly;
        ActiveSignalsOnly = activeSignalsOnly;
        
        ValidateCriteria();
    }

    /// <summary>
    /// デフォルトの検索条件を作成
    /// </summary>
    public static SimilaritySearchCriteria CreateDefault()
    {
        return new SimilaritySearchCriteria(
            compareCanId: true,
            compareSignalName: true,
            compareSystemType: true,
            minimumSimilarity: 0.7,
            maxResults: 20,
            activeSignalsOnly: true);
    }

    /// <summary>
    /// 厳密な検索条件を作成（すべての属性を比較）
    /// </summary>
    public static SimilaritySearchCriteria CreateStrict()
    {
        return new SimilaritySearchCriteria(
            compareCanId: true,
            compareSignalName: true,
            compareSystemType: true,
            minimumSimilarity: 0.9,
            maxResults: 10,
            compareValueRange: true,
            compareDataLength: true,
            compareCycle: true,
            activeSignalsOnly: true);
    }

    /// <summary>
    /// 緩い検索条件を作成（基本属性のみ比較）
    /// </summary>
    public static SimilaritySearchCriteria CreateLoose()
    {
        return new SimilaritySearchCriteria(
            compareCanId: false,
            compareSignalName: true,
            compareSystemType: true,
            minimumSimilarity: 0.3,
            maxResults: 100,
            activeSignalsOnly: false);
    }

    /// <summary>
    /// システム種別のみで検索する条件を作成
    /// </summary>
    public static SimilaritySearchCriteria CreateBySystemType()
    {
        return new SimilaritySearchCriteria(
            compareCanId: false,
            compareSignalName: false,
            compareSystemType: true,
            minimumSimilarity: 0.5,
            maxResults: 50,
            activeSignalsOnly: true);
    }

    /// <summary>
    /// 最小類似度を更新した新しい条件を作成
    /// </summary>
    public SimilaritySearchCriteria WithMinimumSimilarity(double minimumSimilarity)
    {
        return new SimilaritySearchCriteria(
            CompareCanId,
            CompareSignalName,
            CompareSystemType,
            minimumSimilarity,
            MaxResults,
            CompareValueRange,
            CompareDataLength,
            CompareCycle,
            CompareOemCode,
            StandardSignalsOnly,
            ActiveSignalsOnly);
    }

    /// <summary>
    /// 最大結果数を更新した新しい条件を作成
    /// </summary>
    public SimilaritySearchCriteria WithMaxResults(int maxResults)
    {
        return new SimilaritySearchCriteria(
            CompareCanId,
            CompareSignalName,
            CompareSystemType,
            MinimumSimilarity,
            maxResults,
            CompareValueRange,
            CompareDataLength,
            CompareCycle,
            CompareOemCode,
            StandardSignalsOnly,
            ActiveSignalsOnly);
    }

    /// <summary>
    /// 比較対象を追加した新しい条件を作成
    /// </summary>
    public SimilaritySearchCriteria WithAdditionalComparisons(
        bool compareValueRange = false,
        bool compareDataLength = false,
        bool compareCycle = false,
        bool compareOemCode = false)
    {
        return new SimilaritySearchCriteria(
            CompareCanId,
            CompareSignalName,
            CompareSystemType,
            MinimumSimilarity,
            MaxResults,
            compareValueRange,
            compareDataLength,
            compareCycle,
            compareOemCode,
            StandardSignalsOnly,
            ActiveSignalsOnly);
    }

    /// <summary>
    /// フィルター条件を更新した新しい条件を作成
    /// </summary>
    public SimilaritySearchCriteria WithFilters(
        bool standardSignalsOnly = false,
        bool activeSignalsOnly = true)
    {
        return new SimilaritySearchCriteria(
            CompareCanId,
            CompareSignalName,
            CompareSystemType,
            MinimumSimilarity,
            MaxResults,
            CompareValueRange,
            CompareDataLength,
            CompareCycle,
            CompareOemCode,
            standardSignalsOnly,
            activeSignalsOnly);
    }

    /// <summary>
    /// 有効な比較条件が設定されているかチェック
    /// </summary>
    public bool HasValidComparisons()
    {
        return CompareCanId || CompareSignalName || CompareSystemType || 
               CompareValueRange || CompareDataLength || CompareCycle || CompareOemCode;
    }

    /// <summary>
    /// 詳細比較が有効かチェック
    /// </summary>
    public bool HasDetailedComparisons()
    {
        return CompareValueRange || CompareDataLength || CompareCycle;
    }

    /// <summary>
    /// 基本比較のみかチェック
    /// </summary>
    public bool IsBasicComparisonOnly()
    {
        return (CompareCanId || CompareSignalName || CompareSystemType) &&
               !HasDetailedComparisons();
    }

    private static double ValidateMinimumSimilarity(double minimumSimilarity)
    {
        if (minimumSimilarity < 0.0 || minimumSimilarity > 1.0)
            throw new ArgumentOutOfRangeException(nameof(minimumSimilarity), 
                "Minimum similarity must be between 0.0 and 1.0");
                
        return minimumSimilarity;
    }

    private static int ValidateMaxResults(int maxResults)
    {
        if (maxResults <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxResults), 
                "Max results must be greater than 0");
                
        if (maxResults > 1000)
            throw new ArgumentOutOfRangeException(nameof(maxResults), 
                "Max results cannot exceed 1000");
                
        return maxResults;
    }

    private void ValidateCriteria()
    {
        if (!HasValidComparisons())
            throw new ArgumentException("At least one comparison criteria must be enabled");
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CompareCanId;
        yield return CompareSignalName;
        yield return CompareSystemType;
        yield return MinimumSimilarity;
        yield return MaxResults;
        yield return CompareValueRange;
        yield return CompareDataLength;
        yield return CompareCycle;
        yield return CompareOemCode;
        yield return StandardSignalsOnly;
        yield return ActiveSignalsOnly;
    }
}