namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEMカスタマイズの種類を表す列挙型
/// </summary>
public enum CustomizationType
{
    /// <summary>
    /// パラメータ調整
    /// </summary>
    ParameterAdjustment = 1,
    
    /// <summary>
    /// 閾値変更
    /// </summary>
    ThresholdChange = 2,
    
    /// <summary>
    /// ロジック修正
    /// </summary>
    LogicModification = 3,
    
    /// <summary>
    /// 仕様変更
    /// </summary>
    SpecificationChange = 4,
    
    /// <summary>
    /// その他
    /// </summary>
    Other = 99
}