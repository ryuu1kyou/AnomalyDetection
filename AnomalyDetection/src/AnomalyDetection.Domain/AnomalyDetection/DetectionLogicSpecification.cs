using System;
using System.Collections.Generic;
using AnomalyDetection.CanSignals;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

public class DetectionLogicSpecification : ValueObject
{
    public AnomalyType DetectionType { get; private set; }
    public string Description { get; private set; } = default!;
    public CanSystemType TargetSystemType { get; private set; }
    public LogicComplexity Complexity { get; private set; }
    public string? Requirements { get; private set; }

    protected DetectionLogicSpecification() { }

    public DetectionLogicSpecification(
        AnomalyType detectionType,
        string description,
        CanSystemType targetSystemType,
        LogicComplexity complexity = LogicComplexity.Simple,
        string? requirements = null)
    {
        DetectionType = detectionType;
        Description = ValidateDescription(description);
        TargetSystemType = targetSystemType;
        Complexity = complexity;
        Requirements = ValidateRequirements(requirements);
    }

    public bool IsApplicableToSystem(CanSystemType systemType)
    {
        return TargetSystemType == systemType || TargetSystemType == CanSystemType.Gateway;
    }

    public bool RequiresMultipleSignals()
    {
        return DetectionType == AnomalyType.CorrelationAnomaly ||
               DetectionType == AnomalyType.PatternAnomaly ||
               Complexity == LogicComplexity.Complex;
    }

    public bool IsRealTimeCapable()
    {
        return DetectionType != AnomalyType.OutOfRange &&
               Complexity != LogicComplexity.Complex;
    }

    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
            
        if (description.Length > 1000)
            throw new ArgumentException("Description cannot exceed 1000 characters", nameof(description));
            
        return description.Trim();
    }

    private static string? ValidateRequirements(string? requirements)
    {
        if (requirements != null && requirements.Length > 2000)
            throw new ArgumentException("Requirements cannot exceed 2000 characters", nameof(requirements));
            
        return requirements?.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DetectionType;
        yield return Description;
        yield return TargetSystemType;
        yield return Complexity;
        yield return Requirements ?? string.Empty;
    }
}

public enum LogicComplexity
{
    Simple = 1,     // 単一信号、単純な閾値チェック
    Medium = 2,     // 複数信号、時系列分析
    Complex = 3     // 機械学習、統計分析
}