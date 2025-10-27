namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 差異タイプ
/// </summary>
public enum DifferenceType
{
    /// <summary>
    /// 値が異なる
    /// </summary>
    ValueDifference = 1,
    
    /// <summary>
    /// 構造が異なる
    /// </summary>
    StructureDifference = 2,
    
    /// <summary>
    /// 条件が異なる
    /// </summary>
    ConditionDifference = 3,
    
    /// <summary>
    /// パラメータが異なる
    /// </summary>
    ParameterDifference = 4,
    
    /// <summary>
    /// 結果が異なる
    /// </summary>
    ResultDifference = 5,
    
    /// <summary>
    /// 設定が異なる
    /// </summary>
    ConfigurationDifference = 6,
    
    /// <summary>
    /// その他の差異
    /// </summary>
    Other = 99
}
