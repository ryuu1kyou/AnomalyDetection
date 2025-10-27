namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEMカスタマイズの状態を表す列挙型
/// </summary>
public enum CustomizationStatus
{
    /// <summary>
    /// 下書き
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// 承認待ち
    /// </summary>
    PendingApproval = 1,
    
    /// <summary>
    /// 承認済み
    /// </summary>
    Approved = 2,
    
    /// <summary>
    /// 却下
    /// </summary>
    Rejected = 3,
    
    /// <summary>
    /// 廃止
    /// </summary>
    Obsolete = 4
}