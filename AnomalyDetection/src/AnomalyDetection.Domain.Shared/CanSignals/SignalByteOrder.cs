namespace AnomalyDetection.CanSignals;

/// <summary>
/// CAN信号のバイトオーダー
/// </summary>
public enum SignalByteOrder
{
    /// <summary>
    /// リトルエンディアン（Intel形式）
    /// </summary>
    LittleEndian = 0,
    
    /// <summary>
    /// ビッグエンディアン（Motorola形式）
    /// </summary>
    BigEndian = 1,
    
    /// <summary>
    /// Motorola形式（BigEndianのエイリアス）
    /// </summary>
    Motorola = 1
}
