namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 資産の共通化分類。「共通核か OEM 固有か」を明示することで、
/// 情報漏洩リスクを防ぐ（06_loophole-rating シナリオ7対応）。
/// Unknown は期限付き保留として扱い、無期限放置を禁止する（TOP3対応）。
/// </summary>
public enum CommonalityStatus
{
    /// <summary>
    /// 判断保留。UnknownResolutionDueDate の設定が必須。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// OEM 非依存の共通核。共通プラットフォームに寄せる対象。
    /// </summary>
    Core = 1,

    /// <summary>
    /// 参照実装ベースの派生。共通モデルのつもりで特定 OEM 事情が入っている場合はこちら。
    /// </summary>
    BaselineDerived = 2,

    /// <summary>
    /// OEM 固有の実装。他 OEM への公開不可。
    /// </summary>
    CustomerSpecific = 3,

    /// <summary>
    /// 車両型式固有の実装。特定車種にのみ適用される。
    /// </summary>
    VehicleSpecific = 4
}
