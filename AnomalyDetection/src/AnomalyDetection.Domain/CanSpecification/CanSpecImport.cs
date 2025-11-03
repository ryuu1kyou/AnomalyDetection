using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.CanSpecification;

/// <summary>
/// CAN specification import entity
/// Tracks imported CAN specification files (DBC, XML, etc.)
/// </summary>
public class CanSpecImport : FullAuditedAggregateRoot<Guid>
{
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty; // DBC, XML, JSON
    public long FileSize { get; set; }
    public string FileHash { get; set; } = string.Empty; // SHA256 for duplicate detection

    public DateTime ImportDate { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public ImportStatus Status { get; set; }

    public string? ErrorMessage { get; set; }
    public int ParsedMessageCount { get; set; }
    public int ParsedSignalCount { get; set; }

    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }

    public List<CanSpecMessage> Messages { get; set; } = new();
    public List<CanSpecDiff> Diffs { get; set; } = new();

    private CanSpecImport() { }

    public CanSpecImport(
        Guid id,
        string fileName,
        string fileFormat,
        long fileSize,
        string fileHash,
        string importedBy
    ) : base(id)
    {
        FileName = fileName;
        FileFormat = fileFormat;
        FileSize = fileSize;
        FileHash = fileHash;
        ImportedBy = importedBy;
        ImportDate = DateTime.UtcNow;
        Status = ImportStatus.Pending;
    }

    public void MarkAsCompleted(int messageCount, int signalCount)
    {
        Status = ImportStatus.Completed;
        ParsedMessageCount = messageCount;
        ParsedSignalCount = signalCount;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = ImportStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void AddMessage(CanSpecMessage message)
    {
        Messages.Add(message);
    }

    public void AddDiff(CanSpecDiff diff)
    {
        Diffs.Add(diff);
    }
}

public enum ImportStatus
{
    Pending = 0,
    Parsing = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>
/// CAN message definition from specification
/// </summary>
public class CanSpecMessage
{
    public Guid Id { get; set; }
    public Guid CanSpecImportId { get; set; }

    public uint MessageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Dlc { get; set; } // Data Length Code (0-8 bytes)
    public string? Transmitter { get; set; }
    public int? CycleTime { get; set; } // ms

    public List<CanSpecSignal> Signals { get; set; } = new();

    public CanSpecMessage(uint messageId, string name, int dlc)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        Name = name;
        Dlc = dlc;
    }

    private CanSpecMessage() { Id = Guid.NewGuid(); }
}

/// <summary>
/// CAN signal definition from specification
/// </summary>
public class CanSpecSignal
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }

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

    public CanSpecSignal(string name, int startBit, int bitLength)
    {
        Id = Guid.NewGuid();
        Name = name;
        StartBit = startBit;
        BitLength = bitLength;
    }

    private CanSpecSignal() { Id = Guid.NewGuid(); }
}

/// <summary>
/// Tracks differences between spec versions
/// </summary>
public class CanSpecDiff
{
    public Guid Id { get; set; }
    public Guid CanSpecImportId { get; set; }

    public Guid? PreviousSpecId { get; set; }
    public DateTime ComparisonDate { get; set; }

    public DiffType Type { get; set; }
    public string EntityType { get; set; } = string.Empty; // Message, Signal
    public string EntityName { get; set; } = string.Empty;
    public uint? MessageId { get; set; }

    public string ChangeCategory { get; set; } = string.Empty;
    public ChangeSeverity Severity { get; set; } = ChangeSeverity.Informational;
    public string? ImpactedSubsystem { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangeSummary { get; set; }
    public string? Details { get; set; }

    public CanSpecDiff(DiffType type, string entityType, string entityName)
    {
        Id = Guid.NewGuid();
        Type = type;
        EntityType = entityType;
        EntityName = entityName;
        ComparisonDate = DateTime.UtcNow;
    }

    private CanSpecDiff() { Id = Guid.NewGuid(); }
}

public enum DiffType
{
    Added = 0,
    Removed = 1,
    Modified = 2
}

public enum ChangeSeverity
{
    Informational = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public class CanSpecDiffSummary
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

    public void IncrementSeverity(ChangeSeverity severity)
    {
        switch (severity)
        {
            case ChangeSeverity.Critical:
                SeverityCriticalCount++;
                break;
            case ChangeSeverity.High:
                SeverityHighCount++;
                break;
            case ChangeSeverity.Medium:
                SeverityMediumCount++;
                break;
            case ChangeSeverity.Low:
                SeverityLowCount++;
                break;
            default:
                SeverityInformationalCount++;
                break;
        }
    }

    public void IncrementEntityCounter(string entityType, DiffType diffType)
    {
        if (string.Equals(entityType, "Message", StringComparison.OrdinalIgnoreCase))
        {
            switch (diffType)
            {
                case DiffType.Added:
                    MessageAddedCount++;
                    break;
                case DiffType.Removed:
                    MessageRemovedCount++;
                    break;
                case DiffType.Modified:
                    MessageModifiedCount++;
                    break;
            }
        }
        else if (string.Equals(entityType, "Signal", StringComparison.OrdinalIgnoreCase))
        {
            switch (diffType)
            {
                case DiffType.Added:
                    SignalAddedCount++;
                    break;
                case DiffType.Removed:
                    SignalRemovedCount++;
                    break;
                case DiffType.Modified:
                    SignalModifiedCount++;
                    break;
            }
        }
    }

    public void TrackSubsystem(string? subsystem)
    {
        if (string.IsNullOrWhiteSpace(subsystem))
        {
            return;
        }

        if (!ImpactedSubsystems.Any(s => string.Equals(s, subsystem, StringComparison.OrdinalIgnoreCase)))
        {
            ImpactedSubsystems.Add(subsystem);
        }
    }
}

public class CanSpecDiffResult
{
    public List<CanSpecDiff> Diffs { get; set; } = new();
    public CanSpecDiffSummary Summary { get; set; } = new();
}
