namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 推奨タイプ
/// </summary>
public enum RecommendationType
{
    /// <summary>
    /// 閾値調整
    /// </summary>
    ThresholdAdjustment = 1,
    
    /// <summary>
    /// パラメータ最適化
    /// </summary>
    ParameterOptimization = 2,
    
    /// <summary>
    /// 検出条件変更
    /// </summary>
    DetectionConditionChange = 3,
    
    /// <summary>
    /// ロジック改善
    /// </summary>
    LogicImprovement = 4,
    
    /// <summary>
    /// 設定変更
    /// </summary>
    ConfigurationChange = 5,
    
    /// <summary>
    /// 類似信号参照
    /// </summary>
    SimilarSignalReference = 6,
    
    /// <summary>
    /// テストデータ活用
    /// </summary>
    TestDataUtilization = 7,
    
    /// <summary>
    /// その他
    /// </summary>
    Other = 99
}
