using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AnomalyDetection.CanSpecification;

/// <summary>
/// Analyzes compatibility between CAN specification versions
/// Detects breaking changes, warnings, and provides migration recommendations
/// </summary>
public class CompatibilityAnalyzer
{
    private readonly ILogger<CompatibilityAnalyzer> _logger;

    public CompatibilityAnalyzer(ILogger<CompatibilityAnalyzer> logger)
    {
        _logger = logger;
    }

    public CompatibilityAnalysis Analyze(
        CanSpecImport oldSpec,
        CanSpecImport newSpec,
        string analyzedBy)
    {
        var analysis = new CompatibilityAnalysis(
            Guid.NewGuid(),
            oldSpec.Id,
            newSpec.Id,
            analyzedBy
        );

        AnalyzeMessages(oldSpec, newSpec, analysis);
        AnalyzeSignals(oldSpec, newSpec, analysis);
        AssessImpacts(analysis);
        GenerateRecommendations(analysis);

        analysis.CalculateCompatibility();

        _logger.LogInformation(
            "Compatibility analysis completed: Score={Score}, Breaking={Breaking}, Warnings={Warnings}",
            analysis.CompatibilityScore, analysis.BreakingChangeCount, analysis.WarningCount);

        return analysis;
    }

    private void AnalyzeMessages(
        CanSpecImport oldSpec,
        CanSpecImport newSpec,
        CompatibilityAnalysis analysis)
    {
        var oldMessages = oldSpec.Messages.ToDictionary(m => m.MessageId);
        var newMessages = newSpec.Messages.ToDictionary(m => m.MessageId);

        // Removed messages = BREAKING
        foreach (var oldMsg in oldMessages.Values)
        {
            if (!newMessages.ContainsKey(oldMsg.MessageId))
            {
                var issue = new CompatibilityIssue(
                    IssueSeverity.Breaking,
                    IssueCategory.MessageRemoved,
                    "Message",
                    oldMsg.Name,
                    $"Message '{oldMsg.Name}' (ID: 0x{oldMsg.MessageId:X}) has been removed"
                )
                {
                    AnalysisId = analysis.Id,
                    MessageId = oldMsg.MessageId,
                    Details = $"DLC: {oldMsg.Dlc}, Signals: {oldMsg.Signals.Count}",
                    Recommendation = "Update all code that depends on this message. Consider adding compatibility layer."
                };

                analysis.AddIssue(issue);
            }
        }

        // Added messages = INFO
        foreach (var newMsg in newMessages.Values)
        {
            if (!oldMessages.ContainsKey(newMsg.MessageId))
            {
                var issue = new CompatibilityIssue(
                    IssueSeverity.Info,
                    IssueCategory.MessageAdded,
                    "Message",
                    newMsg.Name,
                    $"New message '{newMsg.Name}' (ID: 0x{newMsg.MessageId:X}) has been added"
                )
                {
                    AnalysisId = analysis.Id,
                    MessageId = newMsg.MessageId,
                    Details = $"DLC: {newMsg.Dlc}, Signals: {newMsg.Signals.Count}",
                    Recommendation = "Consider adding support for this new message."
                };

                analysis.AddIssue(issue);
            }
        }

        // Modified messages
        foreach (var newMsg in newMessages.Values)
        {
            if (oldMessages.TryGetValue(newMsg.MessageId, out var oldMsg))
            {
                if (oldMsg.Dlc != newMsg.Dlc)
                {
                    var issue = new CompatibilityIssue(
                        IssueSeverity.Breaking,
                        IssueCategory.MessageModified,
                        "Message",
                        newMsg.Name,
                        $"Message DLC changed from {oldMsg.Dlc} to {newMsg.Dlc} bytes"
                    )
                    {
                        AnalysisId = analysis.Id,
                        MessageId = newMsg.MessageId,
                        OldValue = $"DLC: {oldMsg.Dlc}",
                        NewValue = $"DLC: {newMsg.Dlc}",
                        Recommendation = "Update message parsing logic to handle new DLC."
                    };

                    analysis.AddIssue(issue);
                }
            }
        }
    }

    private void AnalyzeSignals(
        CanSpecImport oldSpec,
        CanSpecImport newSpec,
        CompatibilityAnalysis analysis)
    {
        var oldSignals = BuildSignalDictionary(oldSpec);
        var newSignals = BuildSignalDictionary(newSpec);

        // Removed signals = BREAKING
        foreach (var kvp in oldSignals)
        {
            if (!newSignals.ContainsKey(kvp.Key))
            {
                var sig = kvp.Value;
                var issue = new CompatibilityIssue(
                    IssueSeverity.Breaking,
                    IssueCategory.SignalRemoved,
                    "Signal",
                    sig.Name,
                    $"Signal '{sig.Name}' has been removed"
                )
                {
                    AnalysisId = analysis.Id,
                    Details = $"StartBit: {sig.StartBit}, Length: {sig.BitLength} bits",
                    Recommendation = "Remove or update code that depends on this signal."
                };

                analysis.AddIssue(issue);
            }
        }

        // Added signals = INFO
        foreach (var kvp in newSignals)
        {
            if (!oldSignals.ContainsKey(kvp.Key))
            {
                var sig = kvp.Value;
                var issue = new CompatibilityIssue(
                    IssueSeverity.Info,
                    IssueCategory.SignalAdded,
                    "Signal",
                    sig.Name,
                    $"New signal '{sig.Name}' has been added"
                )
                {
                    AnalysisId = analysis.Id,
                    Details = $"StartBit: {sig.StartBit}, Length: {sig.BitLength} bits",
                    Recommendation = "Consider adding support for this new signal."
                };

                analysis.AddIssue(issue);
            }
        }

        // Modified signals
        foreach (var kvp in newSignals)
        {
            if (oldSignals.TryGetValue(kvp.Key, out var oldSig))
            {
                var newSig = kvp.Value;
                AnalyzeSignalChanges(oldSig, newSig, analysis);
            }
        }
    }

    private void AnalyzeSignalChanges(
        CanSpecSignal oldSig,
        CanSpecSignal newSig,
        CompatibilityAnalysis analysis)
    {
        // Bit layout change = BREAKING
        if (oldSig.StartBit != newSig.StartBit || oldSig.BitLength != newSig.BitLength)
        {
            var issue = new CompatibilityIssue(
                IssueSeverity.Breaking,
                IssueCategory.BitLayoutChanged,
                "Signal",
                newSig.Name,
                $"Signal bit layout changed"
            )
            {
                AnalysisId = analysis.Id,
                OldValue = $"StartBit: {oldSig.StartBit}, Length: {oldSig.BitLength}",
                NewValue = $"StartBit: {newSig.StartBit}, Length: {newSig.BitLength}",
                Recommendation = "Update signal extraction logic with new bit positions."
            };

            analysis.AddIssue(issue);
        }

        // Scaling change = WARNING
        if (Math.Abs(oldSig.Factor - newSig.Factor) > 0.0001 ||
            Math.Abs(oldSig.Offset - newSig.Offset) > 0.0001)
        {
            var issue = new CompatibilityIssue(
                IssueSeverity.Warning,
                IssueCategory.ScalingChanged,
                "Signal",
                newSig.Name,
                $"Signal scaling changed"
            )
            {
                AnalysisId = analysis.Id,
                OldValue = $"Factor: {oldSig.Factor}, Offset: {oldSig.Offset}",
                NewValue = $"Factor: {newSig.Factor}, Offset: {newSig.Offset}",
                Recommendation = "Update signal value calculations with new scaling factors."
            };

            analysis.AddIssue(issue);
        }

        // Range change = WARNING
        if (Math.Abs(oldSig.Min - newSig.Min) > 0.0001 ||
            Math.Abs(oldSig.Max - newSig.Max) > 0.0001)
        {
            var issue = new CompatibilityIssue(
                IssueSeverity.Warning,
                IssueCategory.RangeChanged,
                "Signal",
                newSig.Name,
                $"Signal value range changed"
            )
            {
                AnalysisId = analysis.Id,
                OldValue = $"Range: [{oldSig.Min}, {oldSig.Max}]",
                NewValue = $"Range: [{newSig.Min}, {newSig.Max}]",
                Recommendation = "Review validation logic and adjust range checks."
            };

            analysis.AddIssue(issue);
        }

        // Byte order change = BREAKING
        if (oldSig.IsBigEndian != newSig.IsBigEndian)
        {
            var issue = new CompatibilityIssue(
                IssueSeverity.Breaking,
                IssueCategory.DataTypeChanged,
                "Signal",
                newSig.Name,
                $"Signal byte order changed"
            )
            {
                AnalysisId = analysis.Id,
                OldValue = oldSig.IsBigEndian ? "Big Endian" : "Little Endian",
                NewValue = newSig.IsBigEndian ? "Big Endian" : "Little Endian",
                Recommendation = "Update byte order handling in signal parsing."
            };

            analysis.AddIssue(issue);
        }
    }

    private void AssessImpacts(CompatibilityAnalysis analysis)
    {
        if (analysis.BreakingChangeCount > 0)
        {
            var breakingIssues = analysis.Issues
                .Where(i => i.Severity == IssueSeverity.Breaking)
                .ToList();

            var messageImpact = new ImpactAssessment(
                "Message Parsing",
                $"{breakingIssues.Count(i => i.Category <= IssueCategory.MessageModified)} message-level breaking changes detected"
            )
            {
                AnalysisId = analysis.Id,
                AffectedMessageCount = breakingIssues
                    .Where(i => i.Category <= IssueCategory.MessageModified)
                    .Select(i => i.MessageId)
                    .Distinct()
                    .Count(),
                Risk = analysis.BreakingChangeCount <= 3 ? RiskLevel.Medium : RiskLevel.High,
                EstimatedEffortHours = analysis.BreakingChangeCount * 2,
                MitigationStrategy = "Implement compatibility layer with version detection"
            };

            analysis.AddImpact(messageImpact);

            var signalImpact = new ImpactAssessment(
                "Signal Processing",
                $"{breakingIssues.Count(i => i.Category >= IssueCategory.SignalRemoved)} signal-level breaking changes detected"
            )
            {
                AnalysisId = analysis.Id,
                AffectedSignalCount = breakingIssues.Count(i => i.Category >= IssueCategory.SignalRemoved),
                Risk = RiskLevel.High,
                EstimatedEffortHours = breakingIssues.Count(i => i.Category >= IssueCategory.SignalRemoved) * 1,
                MitigationStrategy = "Update signal extraction and validation logic"
            };

            analysis.AddImpact(signalImpact);
        }
    }

    private void GenerateRecommendations(CompatibilityAnalysis analysis)
    {
        var recommendations = new List<string>();

        if (analysis.BreakingChangeCount == 0)
        {
            recommendations.Add("No breaking changes detected. Migration should be straightforward.");
        }
        else
        {
            recommendations.Add($"⚠️ {analysis.BreakingChangeCount} breaking change(s) detected.");
            recommendations.Add("1. Review all breaking changes and update affected code.");
            recommendations.Add("2. Implement version detection to support both old and new specs.");
            recommendations.Add("3. Add integration tests to verify compatibility.");
        }

        if (analysis.WarningCount > 0)
        {
            recommendations.Add($"⚠️ {analysis.WarningCount} warning(s) found - review these changes carefully.");
        }

        recommendations.Add($"Estimated migration effort: {analysis.Impacts.Sum(i => i.EstimatedEffortHours)} hours.");

        analysis.Recommendations = string.Join("\n", recommendations);
        analysis.Summary = $"Compatibility Score: {analysis.CompatibilityScore:F1}/100 | " +
                          $"Breaking: {analysis.BreakingChangeCount} | " +
                          $"Warnings: {analysis.WarningCount} | " +
                          $"Info: {analysis.InfoCount}";
    }

    private Dictionary<string, CanSpecSignal> BuildSignalDictionary(CanSpecImport spec)
    {
        var dict = new Dictionary<string, CanSpecSignal>();

        foreach (var msg in spec.Messages)
        {
            foreach (var sig in msg.Signals)
            {
                dict[$"{msg.MessageId}_{sig.Name}"] = sig;
            }
        }

        return dict;
    }
}
