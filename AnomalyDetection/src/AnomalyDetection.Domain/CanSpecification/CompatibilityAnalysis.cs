using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.CanSpecification;

/// <summary>
/// Compatibility analysis result between CAN specification versions
/// Tracks breaking changes, impact assessment, and migration recommendations
/// </summary>
public class CompatibilityAnalysis : FullAuditedAggregateRoot<Guid>
{
    public Guid OldSpecId { get; set; }
    public Guid NewSpecId { get; set; }

    public DateTime AnalysisDate { get; set; }
    public string AnalyzedBy { get; set; } = string.Empty;

    public CompatibilityLevel CompatibilityLevel { get; set; }
    public int BreakingChangeCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }

    public double CompatibilityScore { get; set; } // 0-100
    public RiskLevel MigrationRisk { get; set; }

    public string? Summary { get; set; }
    public string? Recommendations { get; set; }

    public List<CompatibilityIssue> Issues { get; set; } = new();
    public List<ImpactAssessment> Impacts { get; set; } = new();

    private CompatibilityAnalysis() { }

    public CompatibilityAnalysis(
        Guid id,
        Guid oldSpecId,
        Guid newSpecId,
        string analyzedBy
    ) : base(id)
    {
        OldSpecId = oldSpecId;
        NewSpecId = newSpecId;
        AnalyzedBy = analyzedBy;
        AnalysisDate = DateTime.UtcNow;
    }

    public void AddIssue(CompatibilityIssue issue)
    {
        Issues.Add(issue);

        // Update counters
        switch (issue.Severity)
        {
            case IssueSeverity.Breaking:
                BreakingChangeCount++;
                break;
            case IssueSeverity.Warning:
                WarningCount++;
                break;
            case IssueSeverity.Info:
                InfoCount++;
                break;
        }
    }

    public void AddImpact(ImpactAssessment impact)
    {
        Impacts.Add(impact);
    }

    public void CalculateCompatibility()
    {
        // Calculate compatibility score (100 = fully compatible, 0 = incompatible)
        int totalIssues = BreakingChangeCount + WarningCount + InfoCount;
        if (totalIssues == 0)
        {
            CompatibilityScore = 100;
            CompatibilityLevel = CompatibilityLevel.FullyCompatible;
            MigrationRisk = RiskLevel.Low;
            return;
        }

        // Weighted scoring: Breaking=-10, Warning=-3, Info=-1
        double penalty = (BreakingChangeCount * 10) + (WarningCount * 3) + InfoCount;
        CompatibilityScore = Math.Max(0, 100 - penalty);

        // Determine compatibility level
        if (BreakingChangeCount == 0)
        {
            CompatibilityLevel = WarningCount == 0
                ? CompatibilityLevel.FullyCompatible
                : CompatibilityLevel.Compatible;
        }
        else if (BreakingChangeCount <= 5)
        {
            CompatibilityLevel = CompatibilityLevel.MinorIncompatibility;
        }
        else
        {
            CompatibilityLevel = CompatibilityLevel.MajorIncompatibility;
        }

        // Assess migration risk
        MigrationRisk = BreakingChangeCount switch
        {
            0 => RiskLevel.Low,
            <= 3 => RiskLevel.Medium,
            <= 10 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }
}

public enum CompatibilityLevel
{
    FullyCompatible = 0,
    Compatible = 1,
    MinorIncompatibility = 2,
    MajorIncompatibility = 3
}

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Individual compatibility issue found during analysis
/// </summary>
public class CompatibilityIssue
{
    public Guid Id { get; set; }
    public Guid AnalysisId { get; set; }

    public IssueSeverity Severity { get; set; }
    public IssueCategory Category { get; set; }

    public string EntityType { get; set; } = string.Empty; // Message, Signal
    public string EntityName { get; set; } = string.Empty;
    public uint? MessageId { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Recommendation { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public CompatibilityIssue(
        IssueSeverity severity,
        IssueCategory category,
        string entityType,
        string entityName,
        string description)
    {
        Id = Guid.NewGuid();
        Severity = severity;
        Category = category;
        EntityType = entityType;
        EntityName = entityName;
        Description = description;
    }

    private CompatibilityIssue() { Id = Guid.NewGuid(); }
}

public enum IssueSeverity
{
    Info = 0,
    Warning = 1,
    Breaking = 2
}

public enum IssueCategory
{
    MessageRemoved = 0,
    MessageAdded = 1,
    MessageModified = 2,
    SignalRemoved = 3,
    SignalAdded = 4,
    SignalModified = 5,
    DataTypeChanged = 6,
    RangeChanged = 7,
    BitLayoutChanged = 8,
    ScalingChanged = 9
}

/// <summary>
/// Impact assessment for detected changes
/// </summary>
public class ImpactAssessment
{
    public Guid Id { get; set; }
    public Guid AnalysisId { get; set; }

    public string AffectedArea { get; set; } = string.Empty;
    public int AffectedMessageCount { get; set; }
    public int AffectedSignalCount { get; set; }

    public RiskLevel Risk { get; set; }
    public string Impact { get; set; } = string.Empty;
    public string? MitigationStrategy { get; set; }

    public int EstimatedEffortHours { get; set; }

    public ImpactAssessment(string affectedArea, string impact)
    {
        Id = Guid.NewGuid();
        AffectedArea = affectedArea;
        Impact = impact;
    }

    private ImpactAssessment() { Id = Guid.NewGuid(); }
}
