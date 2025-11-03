using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.CanSpecification;

public class CompatibilityAnalysisDto : FullAuditedEntityDto<Guid>
{
    public Guid OldSpecId { get; set; }
    public Guid NewSpecId { get; set; }

    public DateTime AnalysisDate { get; set; }
    public string AnalyzedBy { get; set; } = string.Empty;

    public int CompatibilityLevel { get; set; }
    public int BreakingChangeCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }

    public double CompatibilityScore { get; set; }
    public int MigrationRisk { get; set; }

    public string? Summary { get; set; }
    public string? Recommendations { get; set; }

    public List<CompatibilityIssueDto> Issues { get; set; } = new();
    public List<ImpactAssessmentDto> Impacts { get; set; } = new();
}

public class CompatibilityIssueDto
{
    public Guid Id { get; set; }
    public int Severity { get; set; }
    public int Category { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public uint? MessageId { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Recommendation { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class ImpactAssessmentDto
{
    public Guid Id { get; set; }
    public string AffectedArea { get; set; } = string.Empty;
    public int AffectedMessageCount { get; set; }
    public int AffectedSignalCount { get; set; }

    public int Risk { get; set; }
    public string Impact { get; set; } = string.Empty;
    public string? MitigationStrategy { get; set; }

    public int EstimatedEffortHours { get; set; }
}

public class CreateCompatibilityAnalysisDto
{
    public Guid OldSpecId { get; set; }
    public Guid NewSpecId { get; set; }
}

public class CompatibilityAnalysisResultDto
{
    public Guid AnalysisId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public int CompatibilityLevel { get; set; }
    public double CompatibilityScore { get; set; }
    public int MigrationRisk { get; set; }

    public int BreakingChangeCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }

    public string? Summary { get; set; }
    public List<CompatibilityIssueDto> Issues { get; set; } = new();
    public List<ImpactAssessmentDto> Impacts { get; set; } = new();
}

public class CompatibilityAssessmentRequestDto
{
    public Guid OldSpecId { get; set; }
    public Guid NewSpecId { get; set; }
    public string Context { get; set; } = string.Empty;
    public bool ForceRefresh { get; set; }
}

public class CompatibilityStatusDto
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
}
