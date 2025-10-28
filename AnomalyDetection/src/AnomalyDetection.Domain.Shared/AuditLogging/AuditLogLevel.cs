namespace AnomalyDetection.AuditLogging;

/// <summary>
/// 監査ログの重要度レベル
/// </summary>
public enum AuditLogLevel
{
    /// <summary>
    /// 情報
    /// </summary>
    Information = 1,
    
    /// <summary>
    /// 警告
    /// </summary>
    Warning = 2,
    
    /// <summary>
    /// エラー
    /// </summary>
    Error = 3,
    
    /// <summary>
    /// 重要
    /// </summary>
    Critical = 4
}