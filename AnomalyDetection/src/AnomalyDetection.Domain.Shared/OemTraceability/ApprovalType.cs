namespace AnomalyDetection.OemTraceability;

/// <summary>
/// OEM承認の種類を表す列挙型
/// </summary>
public enum ApprovalType
{
    /// <summary>
    /// 新規作成
    /// </summary>
    NewEntity = 1,
    
    /// <summary>
    /// 修正
    /// </summary>
    Modification = 2,
    
    /// <summary>
    /// カスタマイズ
    /// </summary>
    Customization = 3,
    
    /// <summary>
    /// 継承
    /// </summary>
    Inheritance = 4,
    
    /// <summary>
    /// 共有
    /// </summary>
    Sharing = 5,
    
    /// <summary>
    /// 削除
    /// </summary>
    Deletion = 6
}