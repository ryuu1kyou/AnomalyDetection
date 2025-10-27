using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.SimilarPatternSearch.Dtos;

/// <summary>
/// 類似CAN信号検索リクエストDTO
/// </summary>
public class SimilarSignalSearchRequestDto
{
    /// <summary>
    /// 検索対象の信号ID
    /// </summary>
    [Required]
    public Guid TargetSignalId { get; set; }
    
    /// <summary>
    /// 検索条件
    /// </summary>
    [Required]
    public SimilaritySearchCriteriaDto Criteria { get; set; } = new();
    
    /// <summary>
    /// 候補信号IDリスト（nullの場合は全信号から検索）
    /// </summary>
    public List<Guid>? CandidateSignalIds { get; set; }
}

/// <summary>
/// 類似度検索条件DTO
/// </summary>
public class SimilaritySearchCriteriaDto
{
    /// <summary>
    /// CAN IDで比較するかどうか
    /// </summary>
    public bool CompareCanId { get; set; } = true;
    
    /// <summary>
    /// 信号名で比較するかどうか
    /// </summary>
    public bool CompareSignalName { get; set; } = true;
    
    /// <summary>
    /// システム種別で比較するかどうか
    /// </summary>
    public bool CompareSystemType { get; set; } = true;
    
    /// <summary>
    /// 最小類似度（0.0-1.0）
    /// </summary>
    [Range(0.0, 1.0)]
    public double MinimumSimilarity { get; set; } = 0.5;
    
    /// <summary>
    /// 最大結果数
    /// </summary>
    [Range(1, 1000)]
    public int MaxResults { get; set; } = 50;
    
    /// <summary>
    /// 物理値範囲で比較するかどうか
    /// </summary>
    public bool CompareValueRange { get; set; } = false;
    
    /// <summary>
    /// データ長で比較するかどうか
    /// </summary>
    public bool CompareDataLength { get; set; } = false;
    
    /// <summary>
    /// 周期で比較するかどうか
    /// </summary>
    public bool CompareCycle { get; set; } = false;
    
    /// <summary>
    /// OEMコードで比較するかどうか
    /// </summary>
    public bool CompareOemCode { get; set; } = false;
    
    /// <summary>
    /// 標準信号のみを対象とするかどうか
    /// </summary>
    public bool StandardSignalsOnly { get; set; } = false;
    
    /// <summary>
    /// アクティブな信号のみを対象とするかどうか
    /// </summary>
    public bool ActiveSignalsOnly { get; set; } = true;
}

/// <summary>
/// 類似信号検索結果DTO
/// </summary>
public class SimilarSignalResultDto
{
    /// <summary>
    /// 類似信号のID
    /// </summary>
    public Guid SignalId { get; set; }
    
    /// <summary>
    /// 類似度スコア（0.0-1.0）
    /// </summary>
    public double SimilarityScore { get; set; }
    
    /// <summary>
    /// 類似度の詳細内訳
    /// </summary>
    public SimilarityBreakdownDto Breakdown { get; set; } = new();
    
    /// <summary>
    /// 一致した属性リスト
    /// </summary>
    public List<string> MatchedAttributes { get; set; } = new();
    
    /// <summary>
    /// 差異のある属性リスト
    /// </summary>
    public List<AttributeDifferenceDto> Differences { get; set; } = new();
    
    /// <summary>
    /// 推奨度
    /// </summary>
    public RecommendationLevel RecommendationLevel { get; set; }
    
    /// <summary>
    /// 推奨理由
    /// </summary>
    public string RecommendationReason { get; set; } = string.Empty;
    
    /// <summary>
    /// 信号の基本情報
    /// </summary>
    public SimilarSignalInfoDto SignalInfo { get; set; } = new();
}

/// <summary>
/// 類似度内訳DTO
/// </summary>
public class SimilarityBreakdownDto
{
    /// <summary>
    /// CAN ID類似度
    /// </summary>
    public double CanIdSimilarity { get; set; }
    
    /// <summary>
    /// 信号名類似度
    /// </summary>
    public double SignalNameSimilarity { get; set; }
    
    /// <summary>
    /// システム種別類似度
    /// </summary>
    public double SystemTypeSimilarity { get; set; }
    
    /// <summary>
    /// 値範囲類似度
    /// </summary>
    public double ValueRangeSimilarity { get; set; }
    
    /// <summary>
    /// データ長類似度
    /// </summary>
    public double DataLengthSimilarity { get; set; }
    
    /// <summary>
    /// 周期類似度
    /// </summary>
    public double CycleSimilarity { get; set; }
    
    /// <summary>
    /// OEMコード類似度
    /// </summary>
    public double OemCodeSimilarity { get; set; }
}

/// <summary>
/// 属性差異DTO
/// </summary>
public class AttributeDifferenceDto
{
    /// <summary>
    /// 属性名
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較元の値
    /// </summary>
    public string SourceValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較先の値
    /// </summary>
    public string TargetValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 差異の重要度
    /// </summary>
    public bool IsSignificant { get; set; }
    
    /// <summary>
    /// 差異の説明
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 類似信号情報DTO
/// </summary>
public class SimilarSignalInfoDto
{
    /// <summary>
    /// 信号名
    /// </summary>
    public string SignalName { get; set; } = string.Empty;
    
    /// <summary>
    /// CAN ID
    /// </summary>
    public string CanId { get; set; } = string.Empty;
    
    /// <summary>
    /// システム種別
    /// </summary>
    public string SystemType { get; set; } = string.Empty;
    
    /// <summary>
    /// OEMコード
    /// </summary>
    public string OemCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 説明
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 標準信号かどうか
    /// </summary>
    public bool IsStandard { get; set; }
    
    /// <summary>
    /// ステータス
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// 検査データ比較リクエストDTO
/// </summary>
public class TestDataComparisonRequestDto
{
    /// <summary>
    /// 比較元の信号ID
    /// </summary>
    [Required]
    public Guid SourceSignalId { get; set; }
    
    /// <summary>
    /// 比較先の信号ID
    /// </summary>
    [Required]
    public Guid TargetSignalId { get; set; }
    
    /// <summary>
    /// 比較期間の開始日時
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 比較期間の終了日時
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 異常レベルフィルター
    /// </summary>
    public List<AnomalyLevel>? AnomalyLevelFilter { get; set; }
    
    /// <summary>
    /// 最大結果数
    /// </summary>
    [Range(1, 10000)]
    public int MaxResults { get; set; } = 1000;
}

/// <summary>
/// 検査データ比較結果DTO
/// </summary>
public class TestDataComparisonDto
{
    /// <summary>
    /// 比較対象の信号ID（比較元）
    /// </summary>
    public Guid SourceSignalId { get; set; }
    
    /// <summary>
    /// 比較対象の信号ID（比較先）
    /// </summary>
    public Guid TargetSignalId { get; set; }
    
    /// <summary>
    /// 比較実行日時
    /// </summary>
    public DateTime ComparedAt { get; set; }
    
    /// <summary>
    /// 全体的な類似度スコア（0.0-1.0）
    /// </summary>
    public double OverallSimilarityScore { get; set; }
    
    /// <summary>
    /// 比較サマリー
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// 閾値の差異リスト
    /// </summary>
    public List<ThresholdDifferenceDto> ThresholdDifferences { get; set; } = new();
    
    /// <summary>
    /// 検出条件の差異リスト
    /// </summary>
    public List<DetectionConditionDifferenceDto> DetectionConditionDifferences { get; set; } = new();
    
    /// <summary>
    /// 検出結果の差異リスト
    /// </summary>
    public List<ResultDifferenceDto> ResultDifferences { get; set; } = new();
    
    /// <summary>
    /// 推奨事項リスト
    /// </summary>
    public List<ComparisonRecommendationDto> Recommendations { get; set; } = new();
    
    /// <summary>
    /// 比較統計情報
    /// </summary>
    public ComparisonStatisticsDto Statistics { get; set; } = new();
}

/// <summary>
/// 閾値差異DTO
/// </summary>
public class ThresholdDifferenceDto
{
    /// <summary>
    /// パラメータ名
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較元の値
    /// </summary>
    public double SourceValue { get; set; }
    
    /// <summary>
    /// 比較先の値
    /// </summary>
    public double TargetValue { get; set; }
    
    /// <summary>
    /// 差異の絶対値
    /// </summary>
    public double AbsoluteDifference { get; set; }
    
    /// <summary>
    /// 差異の相対値（パーセンテージ）
    /// </summary>
    public double RelativeDifference { get; set; }
    
    /// <summary>
    /// 重要な差異かどうか
    /// </summary>
    public bool IsSignificant { get; set; }
    
    /// <summary>
    /// 差異の説明
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 検出条件差異DTO
/// </summary>
public class DetectionConditionDifferenceDto
{
    /// <summary>
    /// 条件名
    /// </summary>
    public string ConditionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較元の条件
    /// </summary>
    public string SourceCondition { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較先の条件
    /// </summary>
    public string TargetCondition { get; set; } = string.Empty;
    
    /// <summary>
    /// 差異タイプ
    /// </summary>
    public DifferenceType DifferenceType { get; set; }
    
    /// <summary>
    /// 重要な差異かどうか
    /// </summary>
    public bool IsSignificant { get; set; }
    
    /// <summary>
    /// 差異の説明
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 結果差異DTO
/// </summary>
public class ResultDifferenceDto
{
    /// <summary>
    /// 結果項目名
    /// </summary>
    public string ResultItem { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較元の値
    /// </summary>
    public string SourceValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 比較先の値
    /// </summary>
    public string TargetValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 差異タイプ
    /// </summary>
    public DifferenceType DifferenceType { get; set; }
    
    /// <summary>
    /// 重要な差異かどうか
    /// </summary>
    public bool IsSignificant { get; set; }
    
    /// <summary>
    /// 影響度
    /// </summary>
    public ImpactLevel ImpactLevel { get; set; }
}

/// <summary>
/// 比較推奨事項DTO
/// </summary>
public class ComparisonRecommendationDto
{
    /// <summary>
    /// 推奨事項のタイプ
    /// </summary>
    public RecommendationType Type { get; set; }
    
    /// <summary>
    /// 優先度
    /// </summary>
    public RecommendationPriority Priority { get; set; }
    
    /// <summary>
    /// 推奨事項の内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// 根拠・理由
    /// </summary>
    public string Rationale { get; set; } = string.Empty;
    
    /// <summary>
    /// 推奨される値・設定
    /// </summary>
    public string RecommendedValue { get; set; } = string.Empty;
}

/// <summary>
/// 比較統計情報DTO
/// </summary>
public class ComparisonStatisticsDto
{
    /// <summary>
    /// 比較元の結果数
    /// </summary>
    public int SourceResultCount { get; set; }
    
    /// <summary>
    /// 比較先の結果数
    /// </summary>
    public int TargetResultCount { get; set; }
    
    /// <summary>
    /// 閾値差異数
    /// </summary>
    public int ThresholdDifferenceCount { get; set; }
    
    /// <summary>
    /// 重要な閾値差異数
    /// </summary>
    public int SignificantThresholdDifferenceCount { get; set; }
    
    /// <summary>
    /// 検出条件差異数
    /// </summary>
    public int DetectionConditionDifferenceCount { get; set; }
    
    /// <summary>
    /// 結果差異数
    /// </summary>
    public int ResultDifferenceCount { get; set; }
    
    /// <summary>
    /// 推奨事項数
    /// </summary>
    public int RecommendationCount { get; set; }
    
    /// <summary>
    /// 高優先度推奨事項数
    /// </summary>
    public int HighPriorityRecommendationCount { get; set; }
}

/// <summary>
/// 類似信号推奨リクエストDTO
/// </summary>
public class SimilarSignalRecommendationRequestDto
{
    /// <summary>
    /// 対象信号ID
    /// </summary>
    [Required]
    public Guid SignalId { get; set; }
    
    /// <summary>
    /// 推奨タイプ
    /// </summary>
    public RecommendationType RecommendationType { get; set; } = RecommendationType.ThresholdAdjustment;
    
    /// <summary>
    /// 最大推奨数
    /// </summary>
    [Range(1, 100)]
    public int MaxRecommendations { get; set; } = 10;
}

/// <summary>
/// 類似信号推奨結果DTO
/// </summary>
public class SimilarSignalRecommendationDto
{
    /// <summary>
    /// 推奨信号ID
    /// </summary>
    public Guid RecommendedSignalId { get; set; }
    
    /// <summary>
    /// 推奨スコア（0.0-1.0）
    /// </summary>
    public double RecommendationScore { get; set; }
    
    /// <summary>
    /// 推奨理由
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// 推奨される設定・パラメータ
    /// </summary>
    public Dictionary<string, object> RecommendedSettings { get; set; } = new();
    
    /// <summary>
    /// 信号情報
    /// </summary>
    public SimilarSignalInfoDto SignalInfo { get; set; } = new();
}

/// <summary>
/// エクスポートリクエストDTO
/// </summary>
public class ComparisonExportRequestDto
{
    /// <summary>
    /// 比較結果ID
    /// </summary>
    [Required]
    public Guid ComparisonId { get; set; }
    
    /// <summary>
    /// エクスポート形式
    /// </summary>
    [Required]
    public ExportFormat Format { get; set; }
    
    /// <summary>
    /// 含める項目
    /// </summary>
    public ExportOptions Options { get; set; } = new();
}

/// <summary>
/// エクスポートオプションDTO
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// 閾値差異を含める
    /// </summary>
    public bool IncludeThresholdDifferences { get; set; } = true;
    
    /// <summary>
    /// 検出条件差異を含める
    /// </summary>
    public bool IncludeDetectionConditionDifferences { get; set; } = true;
    
    /// <summary>
    /// 結果差異を含める
    /// </summary>
    public bool IncludeResultDifferences { get; set; } = true;
    
    /// <summary>
    /// 推奨事項を含める
    /// </summary>
    public bool IncludeRecommendations { get; set; } = true;
    
    /// <summary>
    /// 統計情報を含める
    /// </summary>
    public bool IncludeStatistics { get; set; } = true;
    
    /// <summary>
    /// 詳細情報を含める
    /// </summary>
    public bool IncludeDetailedInfo { get; set; } = false;
}

/// <summary>
/// エクスポート形式
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// CSV形式
    /// </summary>
    Csv = 1,
    
    /// <summary>
    /// Excel形式
    /// </summary>
    Excel = 2,
    
    /// <summary>
    /// PDF形式
    /// </summary>
    Pdf = 3,
    
    /// <summary>
    /// JSON形式
    /// </summary>
    Json = 4
}

/// <summary>
/// 推奨レベル
/// </summary>
public enum RecommendationLevel
{
    /// <summary>
    /// 推奨しない
    /// </summary>
    NotRecommended = 0,
    
    /// <summary>
    /// 低推奨
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// 中推奨
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// 高推奨
    /// </summary>
    High = 3,
    
    /// <summary>
    /// 強く推奨
    /// </summary>
    Highly = 4
}

/// <summary>
/// 過去検査データDTO
/// </summary>
public class HistoricalTestDataDto
{
    /// <summary>
    /// 検出結果ID
    /// </summary>
    public Guid ResultId { get; set; }
    
    /// <summary>
    /// 信号ID
    /// </summary>
    public Guid SignalId { get; set; }
    
    /// <summary>
    /// 検出日時
    /// </summary>
    public DateTime DetectedAt { get; set; }
    
    /// <summary>
    /// 異常レベル
    /// </summary>
    public AnomalyLevel AnomalyLevel { get; set; }
    
    /// <summary>
    /// 信頼度スコア
    /// </summary>
    public double ConfidenceScore { get; set; }
    
    /// <summary>
    /// 異常タイプ
    /// </summary>
    public AnomalyType AnomalyType { get; set; }
    
    /// <summary>
    /// 検出条件
    /// </summary>
    public string DetectionCondition { get; set; } = string.Empty;
    
    /// <summary>
    /// 入力データ
    /// </summary>
    public TestInputDataDto InputData { get; set; } = new();
    
    /// <summary>
    /// 検出詳細
    /// </summary>
    public TestDetectionDetailsDto Details { get; set; } = new();
    
    /// <summary>
    /// 解決状況
    /// </summary>
    public ResolutionStatus ResolutionStatus { get; set; }
    
    /// <summary>
    /// 検証済みかどうか
    /// </summary>
    public bool IsValidated { get; set; }
    
    /// <summary>
    /// 誤検出フラグ
    /// </summary>
    public bool IsFalsePositive { get; set; }
}

/// <summary>
/// テスト入力データDTO
/// </summary>
public class TestInputDataDto
{
    /// <summary>
    /// 信号値
    /// </summary>
    public double SignalValue { get; set; }
    
    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 追加データ
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// テスト検出詳細DTO
/// </summary>
public class TestDetectionDetailsDto
{
    /// <summary>
    /// 検出タイプ
    /// </summary>
    public DetectionType DetectionType { get; set; }
    
    /// <summary>
    /// トリガー条件
    /// </summary>
    public string TriggerCondition { get; set; } = string.Empty;
    
    /// <summary>
    /// パラメータ
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
    
    /// <summary>
    /// 実行時間（ミリ秒）
    /// </summary>
    public double ExecutionTimeMs { get; set; }
}

/// <summary>
/// 類似度計算詳細DTO
/// </summary>
public class SimilarityCalculationDetailDto
{
    /// <summary>
    /// 比較対象信号1のID
    /// </summary>
    public Guid Signal1Id { get; set; }
    
    /// <summary>
    /// 比較対象信号2のID
    /// </summary>
    public Guid Signal2Id { get; set; }
    
    /// <summary>
    /// 全体類似度スコア
    /// </summary>
    public double OverallSimilarityScore { get; set; }
    
    /// <summary>
    /// 類似度内訳
    /// </summary>
    public SimilarityBreakdownDto Breakdown { get; set; } = new();
    
    /// <summary>
    /// 計算に使用された条件
    /// </summary>
    public SimilaritySearchCriteriaDto UsedCriteria { get; set; } = new();
    
    /// <summary>
    /// 詳細な比較結果
    /// </summary>
    public List<DetailedComparisonItemDto> DetailedComparisons { get; set; } = new();
    
    /// <summary>
    /// 計算実行時間（ミリ秒）
    /// </summary>
    public double CalculationTimeMs { get; set; }
    
    /// <summary>
    /// 計算日時
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// 詳細比較項目DTO
/// </summary>
public class DetailedComparisonItemDto
{
    /// <summary>
    /// 比較項目名
    /// </summary>
    public string ItemName { get; set; } = string.Empty;
    
    /// <summary>
    /// 信号1の値
    /// </summary>
    public string Signal1Value { get; set; } = string.Empty;
    
    /// <summary>
    /// 信号2の値
    /// </summary>
    public string Signal2Value { get; set; } = string.Empty;
    
    /// <summary>
    /// 類似度スコア
    /// </summary>
    public double SimilarityScore { get; set; }
    
    /// <summary>
    /// 重み
    /// </summary>
    public double Weight { get; set; }
    
    /// <summary>
    /// 重み付きスコア
    /// </summary>
    public double WeightedScore { get; set; }
    
    /// <summary>
    /// 比較方法
    /// </summary>
    public string ComparisonMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// 備考
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// 類似パターン検索統計DTO
/// </summary>
public class SimilarPatternSearchStatisticsDto
{
    /// <summary>
    /// 対象信号ID
    /// </summary>
    public Guid SignalId { get; set; }
    
    /// <summary>
    /// 統計期間（日数）
    /// </summary>
    public int PeriodDays { get; set; }
    
    /// <summary>
    /// 統計開始日
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// 統計終了日
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// 総検索回数
    /// </summary>
    public int TotalSearchCount { get; set; }
    
    /// <summary>
    /// 類似信号発見数
    /// </summary>
    public int SimilarSignalsFoundCount { get; set; }
    
    /// <summary>
    /// 平均類似度スコア
    /// </summary>
    public double AverageSimilarityScore { get; set; }
    
    /// <summary>
    /// 最高類似度スコア
    /// </summary>
    public double MaxSimilarityScore { get; set; }
    
    /// <summary>
    /// 最も類似した信号ID
    /// </summary>
    public Guid? MostSimilarSignalId { get; set; }
    
    /// <summary>
    /// 推奨レベル別統計
    /// </summary>
    public Dictionary<RecommendationLevel, int> RecommendationLevelCounts { get; set; } = new();
    
    /// <summary>
    /// システム種別別統計
    /// </summary>
    public Dictionary<string, int> SystemTypeCounts { get; set; } = new();
    
    /// <summary>
    /// 検索条件使用統計
    /// </summary>
    public SearchCriteriaUsageStatisticsDto CriteriaUsageStatistics { get; set; } = new();
}

/// <summary>
/// 検索条件使用統計DTO
/// </summary>
public class SearchCriteriaUsageStatisticsDto
{
    /// <summary>
    /// CAN ID比較使用回数
    /// </summary>
    public int CanIdComparisonUsageCount { get; set; }
    
    /// <summary>
    /// 信号名比較使用回数
    /// </summary>
    public int SignalNameComparisonUsageCount { get; set; }
    
    /// <summary>
    /// システム種別比較使用回数
    /// </summary>
    public int SystemTypeComparisonUsageCount { get; set; }
    
    /// <summary>
    /// 値範囲比較使用回数
    /// </summary>
    public int ValueRangeComparisonUsageCount { get; set; }
    
    /// <summary>
    /// データ長比較使用回数
    /// </summary>
    public int DataLengthComparisonUsageCount { get; set; }
    
    /// <summary>
    /// 周期比較使用回数
    /// </summary>
    public int CycleComparisonUsageCount { get; set; }
    
    /// <summary>
    /// 平均最小類似度
    /// </summary>
    public double AverageMinimumSimilarity { get; set; }
    
    /// <summary>
    /// 平均最大結果数
    /// </summary>
    public double AverageMaxResults { get; set; }
}