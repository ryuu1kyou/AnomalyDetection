namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 異常の種類を表す列挙型
/// </summary>
public enum AnomalyType
{
    /// <summary>
    /// 通信タイムアウト
    /// </summary>
    Timeout = 1,
    
    /// <summary>
    /// 値が範囲外
    /// </summary>
    OutOfRange = 2,
    
    /// <summary>
    /// 変化率異常
    /// </summary>
    RateOfChange = 3,
    
    /// <summary>
    /// 信号固着
    /// </summary>
    Stuck = 4,
    
    /// <summary>
    /// 周期異常
    /// </summary>
    PeriodicAnomaly = 5,
    
    /// <summary>
    /// データ欠損
    /// </summary>
    DataLoss = 6,
    
    /// <summary>
    /// ノイズ異常
    /// </summary>
    Noise = 7,
    
    /// <summary>
    /// パターン異常
    /// </summary>
    PatternAnomaly = 8,
    
    /// <summary>
    /// 相関異常
    /// </summary>
    CorrelationAnomaly = 9,
    
    /// <summary>
    /// カスタム異常
    /// </summary>
    Custom = 99
}