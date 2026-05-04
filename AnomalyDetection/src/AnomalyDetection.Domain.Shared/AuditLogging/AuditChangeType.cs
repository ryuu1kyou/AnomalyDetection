namespace AnomalyDetection.AuditLogging;

/// <summary>
/// 変更の性質を分類する。IF変更の見落とし防止（06_loophole-rating TOP2）に使用する。
/// </summary>
public enum AuditChangeType
{
    /// <summary>
    /// 変更種別なし / 非該当
    /// </summary>
    NotApplicable = 0,

    /// <summary>
    /// 信号仕様・コンポ間インターフェース定義の変更
    /// </summary>
    IfChange = 1,

    /// <summary>
    /// 検出ロジック・アルゴリズムの変更
    /// </summary>
    LogicChange = 2,

    /// <summary>
    /// 閾値・キャリブレーション・パラメータの変更
    /// </summary>
    CalibChange = 3,

    /// <summary>
    /// バグ修正
    /// </summary>
    Bugfix = 4,

    /// <summary>
    /// リファクタリングのみ（機能変更なし）
    /// </summary>
    RefactorOnly = 5,

    /// <summary>
    /// ドキュメントのみの変更
    /// </summary>
    DocOnly = 6,

    /// <summary>
    /// 要求整合のための変更
    /// </summary>
    RequirementAlignment = 7
}
