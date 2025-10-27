namespace AnomalyDetection.SimilarPatternSearch;

/// <summary>
/// 影響度レベル
/// </summary>
public enum ImpactLevel
{
    /// <summary>
    /// 影響なし
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 低影響
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// 中影響
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// 高影響
    /// </summary>
    High = 3,
    
    /// <summary>
    /// 重大な影響
    /// </summary>
    Critical = 4
}
