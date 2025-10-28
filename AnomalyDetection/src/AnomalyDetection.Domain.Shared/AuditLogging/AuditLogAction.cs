namespace AnomalyDetection.AuditLogging;

/// <summary>
/// 監査ログのアクション種別
/// </summary>
public enum AuditLogAction
{
    /// <summary>
    /// 作成
    /// </summary>
    Create = 1,
    
    /// <summary>
    /// 更新
    /// </summary>
    Update = 2,
    
    /// <summary>
    /// 削除
    /// </summary>
    Delete = 3,
    
    /// <summary>
    /// 承認
    /// </summary>
    Approve = 4,
    
    /// <summary>
    /// 却下
    /// </summary>
    Reject = 5,
    
    /// <summary>
    /// 実行
    /// </summary>
    Execute = 6,
    
    /// <summary>
    /// エクスポート
    /// </summary>
    Export = 7,
    
    /// <summary>
    /// インポート
    /// </summary>
    Import = 8,
    
    /// <summary>
    /// ログイン
    /// </summary>
    Login = 9,
    
    /// <summary>
    /// ログアウト
    /// </summary>
    Logout = 10,
    
    /// <summary>
    /// 設定変更
    /// </summary>
    ConfigurationChange = 11,
    
    /// <summary>
    /// 権限変更
    /// </summary>
    PermissionChange = 12
}