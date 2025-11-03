using System;
using System.Collections.Generic;

namespace AnomalyDetection.CanSpecification;

/// <summary>
/// Cache item for compatibility quick assessment results.
/// </summary>
[Serializable]
public class CompatibilityStatusCacheItem
{
    public Guid OldSpecId { get; set; }
    public Guid NewSpecId { get; set; }
    public string Context { get; set; } = string.Empty;

    public bool IsCompatible { get; set; }
    public int HighestSeverity { get; set; }
    public int BreakingChangeCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public double CompatibilityScore { get; set; }

    public string Summary { get; set; } = string.Empty;
    public List<string> ImpactedSubsystems { get; set; } = new();
    public List<string> KeyFindings { get; set; } = new();
    public DateTime GeneratedAt { get; set; }

    public static CompatibilityStatusCacheItem FromDto(CompatibilityStatusDto dto)
    {
        return new CompatibilityStatusCacheItem
        {
            OldSpecId = dto.OldSpecId,
            NewSpecId = dto.NewSpecId,
            Context = dto.Context,
            IsCompatible = dto.IsCompatible,
            HighestSeverity = dto.HighestSeverity,
            BreakingChangeCount = dto.BreakingChangeCount,
            WarningCount = dto.WarningCount,
            InfoCount = dto.InfoCount,
            CompatibilityScore = dto.CompatibilityScore,
            Summary = dto.Summary,
            ImpactedSubsystems = new List<string>(dto.ImpactedSubsystems ?? new List<string>()),
            KeyFindings = new List<string>(dto.KeyFindings ?? new List<string>()),
            GeneratedAt = dto.GeneratedAt
        };
    }

    public CompatibilityStatusDto ToDto()
    {
        return new CompatibilityStatusDto
        {
            OldSpecId = OldSpecId,
            NewSpecId = NewSpecId,
            Context = Context,
            IsCompatible = IsCompatible,
            HighestSeverity = HighestSeverity,
            BreakingChangeCount = BreakingChangeCount,
            WarningCount = WarningCount,
            InfoCount = InfoCount,
            CompatibilityScore = CompatibilityScore,
            Summary = Summary,
            ImpactedSubsystems = new List<string>(ImpactedSubsystems),
            KeyFindings = new List<string>(KeyFindings),
            GeneratedAt = GeneratedAt
        };
    }
}
