namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEM承認の状態を表す列挙型
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// 承認待ち
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// 承認済み
    /// </summary>
    Approved = 1,
    
    /// <summary>
    /// 却下
    /// </summary>
    Rejected = 2,
    
    /// <summary>
    /// キャンセル
    /// </summary>
    Cancelled = 3
}