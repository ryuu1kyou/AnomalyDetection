using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AnomalyDetection.CanSpecification;
using Microsoft.Extensions.Logging;

namespace AnomalyDetection.Services;

/// <summary>
/// Compare CAN specifications and generate differences with severity and summary metadata
/// </summary>
public class CanSpecDiffService
{
    private const double NumericTolerance = 0.0001;

    private readonly ILogger<CanSpecDiffService> _logger;

    public CanSpecDiffService(ILogger<CanSpecDiffService> logger)
    {
        _logger = logger;
    }

    public CanSpecDiffResult CompareSpecifications(
        CanSpecImport oldSpec,
        CanSpecImport newSpec)
    {
        var result = new CanSpecDiffResult();

        var oldMessages = oldSpec.Messages.ToDictionary(m => m.MessageId);
        var newMessages = newSpec.Messages.ToDictionary(m => m.MessageId);

        CompareMessages(oldSpec, newSpec, oldMessages, newMessages, result);
        CompareSignals(oldSpec, newSpec, result);

        result.Summary.SummaryText = BuildSummaryText(result.Summary);

        _logger.LogInformation(
            "Spec comparison completed: {DiffCount} differences found (Messages: +{MsgAdded}/-{MsgRemoved}/{MsgModified}, Signals: +{SigAdded}/-{SigRemoved}/{SigModified}, Critical+High: {Sev})",
            result.Diffs.Count,
            result.Summary.MessageAddedCount,
            result.Summary.MessageRemovedCount,
            result.Summary.MessageModifiedCount,
            result.Summary.SignalAddedCount,
            result.Summary.SignalRemovedCount,
            result.Summary.SignalModifiedCount,
            result.Summary.SeverityCriticalCount + result.Summary.SeverityHighCount);

        return result;
    }

    private void CompareMessages(
        CanSpecImport oldSpec,
        CanSpecImport newSpec,
        Dictionary<uint, CanSpecMessage> oldMessages,
        Dictionary<uint, CanSpecMessage> newMessages,
        CanSpecDiffResult result)
    {
        foreach (var newMsg in newMessages.Values)
        {
            if (!oldMessages.ContainsKey(newMsg.MessageId))
            {
                var diff = CreateMessageAddedDiff(newSpec: newSpec, newMsg: newMsg, previousSpecId: oldSpec.Id);
                AddDiff(result, diff);
            }
        }

        foreach (var oldMsg in oldMessages.Values)
        {
            if (!newMessages.ContainsKey(oldMsg.MessageId))
            {
                var diff = CreateMessageRemovedDiff(oldSpec, oldMsg, newSpec.Id);
                AddDiff(result, diff);
            }
        }

        foreach (var newMsg in newMessages.Values)
        {
            if (!oldMessages.TryGetValue(newMsg.MessageId, out var oldMsg))
            {
                continue;
            }

            var diff = CreateMessageModifiedDiff(oldSpec.Id, newSpec.Id, oldMsg, newMsg);
            if (diff != null)
            {
                AddDiff(result, diff);
            }
        }
    }

    private void CompareSignals(
        CanSpecImport oldSpec,
        CanSpecImport newSpec,
        CanSpecDiffResult result)
    {
        var oldSignals = BuildSignalDictionary(oldSpec);
        var newSignals = BuildSignalDictionary(newSpec);

        foreach (var kvp in newSignals)
        {
            if (oldSignals.ContainsKey(kvp.Key))
            {
                continue;
            }

            var (newMsg, newSig) = kvp.Value;
            var diff = CreateSignalAddedDiff(oldSpec.Id, newSpec.Id, newMsg, newSig);
            AddDiff(result, diff);
        }

        foreach (var kvp in oldSignals)
        {
            if (newSignals.ContainsKey(kvp.Key))
            {
                continue;
            }

            var (oldMsg, oldSig) = kvp.Value;
            var diff = CreateSignalRemovedDiff(oldSpec.Id, newSpec.Id, oldMsg, oldSig);
            AddDiff(result, diff);
        }

        foreach (var kvp in newSignals)
        {
            if (!oldSignals.TryGetValue(kvp.Key, out var oldPair))
            {
                continue;
            }

            var (newMsg, newSig) = kvp.Value;
            var (_, oldSig) = oldPair;

            var diff = CreateSignalModifiedDiff(oldSpec.Id, newSpec.Id, newMsg, oldSig, newSig);
            if (diff != null)
            {
                AddDiff(result, diff);
            }
        }
    }

    private void AddDiff(CanSpecDiffResult result, CanSpecDiff diff)
    {
        result.Diffs.Add(diff);
        result.Summary.IncrementSeverity(diff.Severity);
        result.Summary.IncrementEntityCounter(diff.EntityType, diff.Type);
        result.Summary.TrackSubsystem(diff.ImpactedSubsystem);
    }

    private CanSpecDiff CreateMessageAddedDiff(CanSpecImport newSpec, CanSpecMessage newMsg, Guid previousSpecId)
    {
        return new CanSpecDiff(DiffType.Added, "Message", newMsg.Name)
        {
            CanSpecImportId = newSpec.Id,
            PreviousSpecId = previousSpecId,
            MessageId = newMsg.MessageId,
            ChangeCategory = "MessageAdded",
            Severity = ChangeSeverity.High,
            ImpactedSubsystem = DetermineSubsystem(newMsg, null),
            NewValue = FormatMessageDetails(newMsg),
            ChangeSummary = $"Message {FormatMessageIdentifier(newMsg)} added",
            Details = BuildMessageDetailText(newMsg)
        };
    }

    private CanSpecDiff CreateMessageRemovedDiff(CanSpecImport oldSpec, CanSpecMessage oldMsg, Guid newSpecId)
    {
        return new CanSpecDiff(DiffType.Removed, "Message", oldMsg.Name)
        {
            CanSpecImportId = newSpecId,
            PreviousSpecId = oldSpec.Id,
            MessageId = oldMsg.MessageId,
            ChangeCategory = "MessageRemoved",
            Severity = ChangeSeverity.Critical,
            ImpactedSubsystem = DetermineSubsystem(oldMsg, null),
            OldValue = FormatMessageDetails(oldMsg),
            ChangeSummary = $"Message {FormatMessageIdentifier(oldMsg)} removed",
            Details = BuildMessageDetailText(oldMsg)
        };
    }

    private CanSpecDiff? CreateMessageModifiedDiff(
        Guid previousSpecId,
        Guid newSpecId,
        CanSpecMessage oldMsg,
        CanSpecMessage newMsg)
    {
        var changeDetails = new List<string>();
        var severity = ChangeSeverity.Informational;

        if (!string.Equals(oldMsg.Name, newMsg.Name, StringComparison.Ordinal))
        {
            changeDetails.Add($"Name '{oldMsg.Name}' → '{newMsg.Name}'");
            severity = ElevateSeverity(severity, ChangeSeverity.Medium);
        }

        if (oldMsg.Dlc != newMsg.Dlc)
        {
            changeDetails.Add($"DLC {oldMsg.Dlc} → {newMsg.Dlc}");
            severity = ElevateSeverity(severity, ChangeSeverity.High);
        }

        if (!string.Equals(oldMsg.Transmitter ?? string.Empty, newMsg.Transmitter ?? string.Empty, StringComparison.OrdinalIgnoreCase))
        {
            changeDetails.Add($"Transmitter '{oldMsg.Transmitter ?? ""}' → '{newMsg.Transmitter ?? ""}'");
            severity = ElevateSeverity(severity, ChangeSeverity.High);
        }

        if (oldMsg.CycleTime != newMsg.CycleTime)
        {
            changeDetails.Add($"CycleTime {oldMsg.CycleTime?.ToString() ?? "n/a"} → {newMsg.CycleTime?.ToString() ?? "n/a"}");
            severity = ElevateSeverity(severity, ChangeSeverity.Medium);
        }

        if (!changeDetails.Any())
        {
            return null;
        }

        var summary = $"Message {FormatMessageIdentifier(newMsg)} metadata updated: {string.Join(", ", changeDetails)}";

        return new CanSpecDiff(DiffType.Modified, "Message", newMsg.Name)
        {
            CanSpecImportId = newSpecId,
            PreviousSpecId = previousSpecId,
            MessageId = newMsg.MessageId,
            ChangeCategory = "MessageMetadataChanged",
            Severity = severity,
            ImpactedSubsystem = DetermineSubsystem(newMsg, oldMsg),
            OldValue = FormatMessageDetails(oldMsg),
            NewValue = FormatMessageDetails(newMsg),
            ChangeSummary = summary,
            Details = BuildMessageDetailText(newMsg)
        };
    }

    private CanSpecDiff CreateSignalAddedDiff(
        Guid previousSpecId,
        Guid newSpecId,
        CanSpecMessage newMsg,
        CanSpecSignal newSig)
    {
        return new CanSpecDiff(DiffType.Added, "Signal", newSig.Name)
        {
            CanSpecImportId = newSpecId,
            PreviousSpecId = previousSpecId,
            MessageId = newMsg.MessageId,
            ChangeCategory = "SignalAdded",
            Severity = ChangeSeverity.Medium,
            ImpactedSubsystem = DetermineSubsystem(newMsg, null, newSig),
            NewValue = FormatSignalDetails(newSig),
            ChangeSummary = $"Signal '{newSig.Name}' added to {FormatMessageIdentifier(newMsg)}",
            Details = BuildSignalDetailText(newSig, newMsg)
        };
    }

    private CanSpecDiff CreateSignalRemovedDiff(
        Guid previousSpecId,
        Guid newSpecId,
        CanSpecMessage oldMsg,
        CanSpecSignal oldSig)
    {
        return new CanSpecDiff(DiffType.Removed, "Signal", oldSig.Name)
        {
            CanSpecImportId = newSpecId,
            PreviousSpecId = previousSpecId,
            MessageId = oldMsg.MessageId,
            ChangeCategory = "SignalRemoved",
            Severity = ChangeSeverity.Critical,
            ImpactedSubsystem = DetermineSubsystem(oldMsg, null, oldSig),
            OldValue = FormatSignalDetails(oldSig),
            ChangeSummary = $"Signal '{oldSig.Name}' removed from {FormatMessageIdentifier(oldMsg)}",
            Details = BuildSignalDetailText(oldSig, oldMsg)
        };
    }

    private CanSpecDiff? CreateSignalModifiedDiff(
        Guid previousSpecId,
        Guid newSpecId,
        CanSpecMessage message,
        CanSpecSignal oldSig,
        CanSpecSignal newSig)
    {
        var changes = new List<string>();
        var severity = ChangeSeverity.Informational;

        if (oldSig.StartBit != newSig.StartBit || oldSig.BitLength != newSig.BitLength)
        {
            changes.Add($"bit range {FormatBitRange(oldSig)} → {FormatBitRange(newSig)}");
            severity = ElevateSeverity(severity, ChangeSeverity.High);
        }

        if (HasSignificantDifference(oldSig.Factor, newSig.Factor) || HasSignificantDifference(oldSig.Offset, newSig.Offset))
        {
            changes.Add($"scaling ({oldSig.Factor}x + {oldSig.Offset}) → ({newSig.Factor}x + {newSig.Offset})");
            severity = ElevateSeverity(severity, ChangeSeverity.High);
        }

        if (HasSignificantDifference(oldSig.Min, newSig.Min) || HasSignificantDifference(oldSig.Max, newSig.Max))
        {
            changes.Add($"range [{oldSig.Min}, {oldSig.Max}] → [{newSig.Min}, {newSig.Max}]");
            severity = ElevateSeverity(severity, ChangeSeverity.Medium);
        }

        if (!string.Equals(oldSig.Unit ?? string.Empty, newSig.Unit ?? string.Empty, StringComparison.OrdinalIgnoreCase))
        {
            changes.Add($"unit '{oldSig.Unit ?? ""}' → '{newSig.Unit ?? ""}'");
            severity = ElevateSeverity(severity, ChangeSeverity.Low);
        }

        if (!string.Equals(oldSig.Description ?? string.Empty, newSig.Description ?? string.Empty, StringComparison.OrdinalIgnoreCase))
        {
            changes.Add("description updated");
            severity = ElevateSeverity(severity, ChangeSeverity.Informational);
        }

        if (oldSig.IsBigEndian != newSig.IsBigEndian)
        {
            changes.Add($"byte order {(oldSig.IsBigEndian ? "Big" : "Little")} → {(newSig.IsBigEndian ? "Big" : "Little")}");
            severity = ElevateSeverity(severity, ChangeSeverity.High);
        }

        if (oldSig.IsSigned != newSig.IsSigned)
        {
            changes.Add($"signed flag {(oldSig.IsSigned ? "signed" : "unsigned")} → {(newSig.IsSigned ? "signed" : "unsigned")}");
            severity = ElevateSeverity(severity, ChangeSeverity.High);
        }

        if (!changes.Any())
        {
            return null;
        }

        var summary = new StringBuilder();
        summary.Append($"Signal '{newSig.Name}' in {FormatMessageIdentifier(message)} updated: ");
        summary.Append(string.Join(", ", changes));

        return new CanSpecDiff(DiffType.Modified, "Signal", newSig.Name)
        {
            CanSpecImportId = newSpecId,
            PreviousSpecId = previousSpecId,
            MessageId = message.MessageId,
            ChangeCategory = "SignalModified",
            Severity = severity,
            ImpactedSubsystem = DetermineSubsystem(message, null, newSig),
            OldValue = FormatSignalDetails(oldSig),
            NewValue = FormatSignalDetails(newSig),
            ChangeSummary = summary.ToString(),
            Details = BuildSignalDetailText(newSig, message)
        };
    }

    private static Dictionary<string, (CanSpecMessage Message, CanSpecSignal Signal)> BuildSignalDictionary(CanSpecImport spec)
    {
        var dict = new Dictionary<string, (CanSpecMessage, CanSpecSignal)>();

        foreach (var msg in spec.Messages)
        {
            foreach (var sig in msg.Signals)
            {
                var key = BuildSignalKey(msg.MessageId, sig.Name);
                dict[key] = (msg, sig);
            }
        }

        return dict;
    }

    private static string BuildSignalKey(uint messageId, string signalName)
    {
        return string.Create(CultureInfo.InvariantCulture, $"{messageId:X}:{signalName}");
    }

    private static string FormatMessageIdentifier(CanSpecMessage message)
    {
        return $"0x{message.MessageId:X} '{message.Name}'";
    }

    private static string FormatMessageDetails(CanSpecMessage message)
    {
        return $"ID:0x{message.MessageId:X}, DLC:{message.Dlc}, Tx:{message.Transmitter ?? "n/a"}, Cycle:{message.CycleTime?.ToString() ?? "n/a"}";
    }

    private static string BuildMessageDetailText(CanSpecMessage message)
    {
        return $"Message {FormatMessageIdentifier(message)} | DLC {message.Dlc} | Tx: {message.Transmitter ?? "n/a"} | Cycle: {message.CycleTime?.ToString() ?? "n/a"}";
    }

    private static string FormatSignalDetails(CanSpecSignal signal)
    {
        return $"BitRange:{FormatBitRange(signal)}, Factor:{signal.Factor}, Offset:{signal.Offset}, Range:[{signal.Min}, {signal.Max}], Unit:{signal.Unit ?? ""}";
    }

    private static string BuildSignalDetailText(CanSpecSignal signal, CanSpecMessage message)
    {
        return $"Signal '{signal.Name}' in message {FormatMessageIdentifier(message)} | {FormatSignalDetails(signal)}";
    }

    private static string FormatBitRange(CanSpecSignal signal)
    {
        var endBit = signal.StartBit + signal.BitLength - 1;
        return $"{signal.StartBit}-{endBit}";
    }

    private static ChangeSeverity ElevateSeverity(ChangeSeverity current, ChangeSeverity candidate)
    {
        return candidate > current ? candidate : current;
    }

    private static bool HasSignificantDifference(double a, double b)
    {
        return Math.Abs(a - b) > NumericTolerance;
    }

    private string DetermineSubsystem(CanSpecMessage? message, CanSpecMessage? fallbackMessage, CanSpecSignal? signal = null)
    {
        var targetMessage = message ?? fallbackMessage;
        if (targetMessage == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(targetMessage.Transmitter))
        {
            var tx = targetMessage.Transmitter!
                .Trim()
                .ToUpperInvariant();

            if (tx.Contains("ADAS", StringComparison.Ordinal))
            {
                return "ADAS";
            }

            if (tx.Contains("PT", StringComparison.Ordinal) || tx.Contains("PWR", StringComparison.Ordinal) || tx.Contains("ENGINE", StringComparison.Ordinal))
            {
                return "Powertrain";
            }

            if (tx.Contains("BCM", StringComparison.Ordinal) || tx.Contains("BODY", StringComparison.Ordinal))
            {
                return "Body";
            }

            if (tx.Contains("ABS", StringComparison.Ordinal) || tx.Contains("BRAKE", StringComparison.Ordinal))
            {
                return "Chassis";
            }
        }

        var name = targetMessage.Name?.ToUpperInvariant() ?? string.Empty;
        if (name.Contains("ADAS", StringComparison.Ordinal))
        {
            return "ADAS";
        }

        if (name.Contains("DRIVE", StringComparison.Ordinal) || name.Contains("ENGINE", StringComparison.Ordinal) || name.Contains("MOTOR", StringComparison.Ordinal))
        {
            return "Powertrain";
        }

        if (name.Contains("BRAKE", StringComparison.Ordinal) || name.Contains("STEER", StringComparison.Ordinal))
        {
            return "Chassis";
        }

        if (name.Contains("HVAC", StringComparison.Ordinal) || name.Contains("CLIMATE", StringComparison.Ordinal))
        {
            return "Body";
        }

        if (signal != null && !string.IsNullOrWhiteSpace(signal.Receiver))
        {
            var receiver = signal.Receiver!.ToUpperInvariant();
            if (receiver.Contains("DIAG", StringComparison.Ordinal))
            {
                return "Diagnostics";
            }
        }

        return "General";
    }

    private string BuildSummaryText(CanSpecDiffSummary summary)
    {
        var changeParts = new List<string>();
        if (summary.MessageAddedCount > 0)
        {
            changeParts.Add($"{summary.MessageAddedCount} message add(s)");
        }
        if (summary.MessageRemovedCount > 0)
        {
            changeParts.Add($"{summary.MessageRemovedCount} message removal(s)");
        }
        if (summary.MessageModifiedCount > 0)
        {
            changeParts.Add($"{summary.MessageModifiedCount} message metadata update(s)");
        }
        if (summary.SignalAddedCount > 0)
        {
            changeParts.Add($"{summary.SignalAddedCount} signal add(s)");
        }
        if (summary.SignalRemovedCount > 0)
        {
            changeParts.Add($"{summary.SignalRemovedCount} signal removal(s)");
        }
        if (summary.SignalModifiedCount > 0)
        {
            changeParts.Add($"{summary.SignalModifiedCount} signal update(s)");
        }

        var severityParts = new List<string>();
        if (summary.SeverityCriticalCount > 0)
        {
            severityParts.Add($"{summary.SeverityCriticalCount} critical");
        }
        if (summary.SeverityHighCount > 0)
        {
            severityParts.Add($"{summary.SeverityHighCount} high");
        }
        if (summary.SeverityMediumCount > 0)
        {
            severityParts.Add($"{summary.SeverityMediumCount} medium");
        }
        if (summary.SeverityLowCount > 0)
        {
            severityParts.Add($"{summary.SeverityLowCount} low");
        }
        if (summary.SeverityInformationalCount > 0)
        {
            severityParts.Add($"{summary.SeverityInformationalCount} info");
        }

        var builder = new StringBuilder();
        if (changeParts.Any())
        {
            builder.Append(string.Join(", ", changeParts));
        }
        else
        {
            builder.Append("No structural changes detected");
        }

        if (severityParts.Any())
        {
            builder.Append(" | Severity: ");
            builder.Append(string.Join(", ", severityParts));
        }

        if (summary.ImpactedSubsystems.Any())
        {
            builder.Append(" | Impacted: ");
            builder.Append(string.Join(", ", summary.ImpactedSubsystems));
        }

        return builder.ToString();
    }
}
