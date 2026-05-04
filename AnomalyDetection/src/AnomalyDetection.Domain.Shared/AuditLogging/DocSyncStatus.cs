namespace AnomalyDetection.AuditLogging;

/// <summary>
/// ドキュメントの同期状態。実装が進んでも文書が未追従の状態を検知するために使用する。
/// </summary>
public enum DocSyncStatus
{
    /// <summary>
    /// 文書更新不要
    /// </summary>
    NotRequired = 0,

    /// <summary>
    /// 実装済みだが文書が未追従。文書更新が必要な状態。
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 文書更新済み
    /// </summary>
    Updated = 2,

    /// <summary>
    /// 文書レビュー完了・承認済み
    /// </summary>
    Reviewed = 3
}
