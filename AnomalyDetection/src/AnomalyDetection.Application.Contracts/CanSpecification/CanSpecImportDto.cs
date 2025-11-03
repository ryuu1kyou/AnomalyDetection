using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.CanSpecification;

public class CanSpecImportDto : FullAuditedEntityDto<Guid>
{
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty;

    public DateTime ImportDate { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public int Status { get; set; }

    public string? ErrorMessage { get; set; }
    public int ParsedMessageCount { get; set; }
    public int ParsedSignalCount { get; set; }

    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }

    public CanSpecDiffSummaryDto? DiffSummary { get; set; }
}

public class CanSpecMessageDto
{
    public Guid Id { get; set; }
    public uint MessageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Dlc { get; set; }
    public string? Transmitter { get; set; }
    public int? CycleTime { get; set; }

    public List<CanSpecSignalDto> Signals { get; set; } = new();
}

public class CanSpecSignalDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartBit { get; set; }
    public int BitLength { get; set; }
    public bool IsBigEndian { get; set; }
    public bool IsSigned { get; set; }

    public double Factor { get; set; }
    public double Offset { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public string? Unit { get; set; }

    public string? Receiver { get; set; }
    public string? Description { get; set; }
}

public class CanSpecDiffDto
{
    public Guid Id { get; set; }
    public Guid? PreviousSpecId { get; set; }
    public DateTime ComparisonDate { get; set; }

    public int Type { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public uint? MessageId { get; set; }

    public string ChangeCategory { get; set; } = string.Empty;
    public int Severity { get; set; }
    public string? ImpactedSubsystem { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangeSummary { get; set; }
    public string? Details { get; set; }
}

public class CanSpecDiffSummaryDto
{
    public int MessageAddedCount { get; set; }
    public int MessageRemovedCount { get; set; }
    public int MessageModifiedCount { get; set; }
    public int SignalAddedCount { get; set; }
    public int SignalRemovedCount { get; set; }
    public int SignalModifiedCount { get; set; }

    public int SeverityInformationalCount { get; set; }
    public int SeverityLowCount { get; set; }
    public int SeverityMediumCount { get; set; }
    public int SeverityHighCount { get; set; }
    public int SeverityCriticalCount { get; set; }

    public List<string> ImpactedSubsystems { get; set; } = new();
    public string SummaryText { get; set; } = string.Empty;
}

public class CreateCanSpecImportDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? Description { get; set; }
}

public class CanSpecImportResultDto
{
    public Guid ImportId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int MessageCount { get; set; }
    public int SignalCount { get; set; }
    public List<CanSpecDiffDto> Diffs { get; set; } = new();
    public CanSpecDiffSummaryDto? DiffSummary { get; set; }
}
