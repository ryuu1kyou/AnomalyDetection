using System;
using System.Collections.Generic;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Values;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.CanSignals;

public class CanSignal : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    // 値オブジェクト
    public SignalIdentifier Identifier { get; private set; } = default!;
    public SignalSpecification Specification { get; private set; } = default!;
    public PhysicalValueConversion Conversion { get; private set; } = default!;
    public SignalTiming Timing { get; private set; } = default!;

    // エンティティ属性
    public CanSystemType SystemType { get; private set; }
    public string? Description { get; private set; }
    public OemCode OemCode { get; private set; } = default!;
    public bool IsStandard { get; private set; }
    public SignalVersion Version { get; private set; } = default!;
    public DateTime? EffectiveDate { get; private set; }
    public SignalStatus Status { get; private set; }

    // メタデータ
    public string? SourceDocument { get; private set; }
    public string? Notes { get; private set; }

    // トレサビ（06_loophole-rating TOP1: feature_id なし防止）
    public string? FeatureId { get; private set; }
    public string? DecisionId { get; private set; }

    // 資産共通化分類（06 TOP3: Unknown 無期限放置防止）
    public CommonalityStatus CommonalityStatus { get; private set; } = CommonalityStatus.Unknown;
    public DateTime? UnknownResolutionDueDate { get; private set; }

    protected CanSignal() { }

    public CanSignal(
        Guid id,
        Guid? tenantId,
        SignalIdentifier identifier,
        SignalSpecification specification,
        CanSystemType systemType,
        OemCode oemCode,
        string? description = null) : base(id)
    {
        TenantId = tenantId;
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Specification = specification ?? throw new ArgumentNullException(nameof(specification));
        SystemType = systemType;
        OemCode = oemCode ?? throw new ArgumentNullException(nameof(oemCode));
        Description = ValidateDescription(description);

        // デフォルト値の設定
        Conversion = new PhysicalValueConversion(1.0, 0.0, "");
        Timing = new SignalTiming(100, 300, SignalSendType.Cyclic); // 100ms cycle, 300ms timeout
        IsStandard = false;
        Version = SignalVersion.Initial();
        Status = SignalStatus.Testing; // Default to Testing instead of Draft
        EffectiveDate = DateTime.UtcNow;
    }

    // ビジネスメソッド
    public void UpdateIdentifier(SignalIdentifier newIdentifier)
    {
        Identifier = newIdentifier ?? throw new ArgumentNullException(nameof(newIdentifier));
    }

    public void UpdateSystemType(CanSystemType newSystemType)
    {
        SystemType = newSystemType;
    }

    public void UpdateOemCode(OemCode newOemCode)
    {
        OemCode = newOemCode ?? throw new ArgumentNullException(nameof(newOemCode));
    }

    public void UpdateSpecification(SignalSpecification newSpecification, string changeReason)
    {
        if (newSpecification == null)
            throw new ArgumentNullException(nameof(newSpecification));

        if (Specification.Equals(newSpecification))
            return;

        if (Status == SignalStatus.Deprecated)
            throw new InvalidOperationException("Cannot update specification of deprecated signal");

        Specification = newSpecification;
        Version = Version.Increment();

        // ドメインイベントを発行（実装は後で）
        // AddDomainEvent(new CanSignalSpecificationUpdatedDomainEvent(this, changeReason));
    }

    public void UpdateConversion(PhysicalValueConversion newConversion)
    {
        Conversion = newConversion ?? throw new ArgumentNullException(nameof(newConversion));
    }

    public void UpdateTiming(SignalTiming newTiming)
    {
        Timing = newTiming ?? throw new ArgumentNullException(nameof(newTiming));
    }

    public void UpdateDescription(string? description)
    {
        Description = ValidateDescription(description);
    }

    public void SetAsStandard()
    {
        if (IsStandard)
            return;

        IsStandard = true;
        // AddDomainEvent(new CanSignalMarkedAsStandardDomainEvent(this));
    }

    public void RemoveStandardStatus()
    {
        if (!IsStandard)
            return;

        IsStandard = false;
    }

    public void Activate()
    {
        if (Status == SignalStatus.Active)
            return;

        Status = SignalStatus.Active;
        EffectiveDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = SignalStatus.Inactive;
    }

    public void Deprecate(string reason)
    {
        Status = SignalStatus.Deprecated;
        Notes = string.IsNullOrEmpty(Notes) ? reason : $"{Notes}; Deprecated: {reason}";
    }

    public void SetSourceDocument(string? sourceDocument)
    {
        SourceDocument = ValidateSourceDocument(sourceDocument);
    }

    public void AddNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return;

        Notes = string.IsNullOrEmpty(Notes)
            ? note
            : $"{Notes}; {note}";
    }

    public bool IsCompatibleWith(CanSignal otherSignal)
    {
        if (otherSignal == null)
            return false;

        return Identifier.CanId == otherSignal.Identifier.CanId &&
               Specification.IsCompatibleWith(otherSignal.Specification) &&
               SystemType == otherSignal.SystemType;
    }

    public bool HasConflictWith(CanSignal otherSignal)
    {
        if (otherSignal == null || otherSignal.Id == Id)
            return false;

        // 同じCAN IDで異なる仕様の場合は競合
        return Identifier.CanId == otherSignal.Identifier.CanId &&
               !Specification.IsCompatibleWith(otherSignal.Specification);
    }

    public double ConvertRawToPhysical(double rawValue)
    {
        // 値範囲チェック
        if (!Specification.ValueRange.IsInRange(rawValue))
            throw new ArgumentOutOfRangeException(nameof(rawValue),
                $"Raw value {rawValue} is outside valid range {Specification.ValueRange}");

        return Conversion.ConvertToPhysical(rawValue);
    }

    public double ConvertPhysicalToRaw(double physicalValue)
    {
        var rawValue = Conversion.ConvertToRaw(physicalValue);

        // 変換後の値が仕様範囲内かチェック
        if (!Specification.ValueRange.IsInRange(rawValue))
            throw new ArgumentOutOfRangeException(nameof(physicalValue),
                $"Physical value {physicalValue} converts to raw value {rawValue} which is outside valid range");

        return rawValue;
    }

    public bool IsActive()
    {
        return Status == SignalStatus.Active;
    }

    public bool IsDeprecated()
    {
        return Status == SignalStatus.Deprecated;
    }

    public void UpdateTraceability(string? featureId, string? decisionId)
    {
        FeatureId = featureId?.Trim();
        DecisionId = decisionId?.Trim();
    }

    public void UpdateCommonalityStatus(CommonalityStatus status, DateTime? resolutionDueDate = null)
    {
        if (status == CommonalityStatus.Unknown && resolutionDueDate == null)
            throw new BusinessException("AnomalyDetection:UnknownRequiresDueDate")
                .WithData("message", "Unknown classification requires a resolution due date.");
        CommonalityStatus = status;
        UnknownResolutionDueDate = status == CommonalityStatus.Unknown ? resolutionDueDate : null;
    }

    private static string? ValidateDescription(string? description)
    {
        if (description != null && description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));

        return description?.Trim();
    }

    private static string? ValidateSourceDocument(string? sourceDocument)
    {
        if (sourceDocument != null && sourceDocument.Length > 500)
            throw new ArgumentException("Source document cannot exceed 500 characters", nameof(sourceDocument));

        return sourceDocument?.Trim();
    }
}

public class SignalVersion : ValueObject
{
    public int Major { get; private set; }
    public int Minor { get; private set; }

    protected SignalVersion() { }

    public SignalVersion(int major, int minor)
    {
        Major = ValidateVersionNumber(major, nameof(major));
        Minor = ValidateVersionNumber(minor, nameof(minor));
    }

    public static SignalVersion Initial()
    {
        return new SignalVersion(1, 0);
    }

    public SignalVersion Increment()
    {
        return new SignalVersion(Major, Minor + 1);
    }

    public SignalVersion IncrementMajor()
    {
        return new SignalVersion(Major + 1, 0);
    }

    private static int ValidateVersionNumber(int version, string paramName)
    {
        if (version < 0)
            throw new ArgumentOutOfRangeException(paramName, "Version number cannot be negative");

        return version;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Major;
        yield return Minor;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}";
    }
}