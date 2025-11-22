using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Volo.Abp.Guids;
using AnomalyDetection.CanSpecification;
using AnomalyDetection.Shared.Export;

namespace AnomalyDetection.CanSpecification.AppServices;

/// <summary>
/// Application service for importing CAN specification files (DBC/CSV minimal stub) and generating diffs.
/// </summary>
public class CanSpecificationImportAppService : ApplicationService
{
    private readonly IRepository<CanSpecImport, Guid> _specRepository;
    private readonly ExportService _exportService;
    private readonly IGuidGenerator _guidGenerator;

    public CanSpecificationImportAppService(
        IRepository<CanSpecImport, Guid> specRepository,
        ExportService exportService,
        IGuidGenerator guidGenerator)
    {
        _specRepository = specRepository;
        _exportService = exportService;
        _guidGenerator = guidGenerator;
    }

    public async Task<CanSpecImportDto> ImportAsync(CanSpecImportInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FileName))
            throw new UserFriendlyException("FileName is required.");
        if (input.Content == null || input.Content.Length == 0)
            throw new UserFriendlyException("File content is empty.");

        var hash = ComputeSha256(input.Content);
        var entity = new CanSpecImport(
            _guidGenerator.Create(),
            input.FileName,
            DetectFormat(input.FileName),
            input.Content.LongLength,
            hash,
            CurrentUser.UserName ?? "system")
        {
            Description = input.Description,
            Manufacturer = input.Manufacturer,
            Version = input.Version
        };

        entity.Status = ImportStatus.Parsing;

        try
        {
            var parseResult = ParseSpecification(input.Content, entity.FileFormat);
            foreach (var msg in parseResult.Messages)
            {
                entity.AddMessage(msg);
            }

            entity.MarkAsCompleted(parseResult.Messages.Count, parseResult.Messages.Sum(m => m.Signals.Count));

            // Diff vs last completed import (simple heuristic by message & signal name existence)
            var previous = await FindLatestCompletedAsync();
            if (previous != null)
            {
                var diffResult = GenerateDiff(previous, entity);
                foreach (var diff in diffResult.Diffs)
                {
                    entity.AddDiff(diff);
                }
            }

            await _specRepository.InsertAsync(entity, autoSave: true);
        }
        catch (Exception ex)
        {
            entity.MarkAsFailed(ex.Message);
            await _specRepository.InsertAsync(entity, autoSave: true);
        }

        return ObjectMapper.Map<CanSpecImport, CanSpecImportDto>(entity);
    }

    public async Task<CanSpecDiffSummaryDto> GetDiffSummaryAsync(Guid specImportId)
    {
        var entity = await _specRepository.GetAsync(specImportId);
        var summary = new CanSpecDiffSummary();
        foreach (var diff in entity.Diffs)
        {
            summary.IncrementEntityCounter(diff.EntityType, diff.Type);
            summary.IncrementSeverity(diff.Severity);
            summary.TrackSubsystem(diff.ImpactedSubsystem);
        }
        summary.SummaryText = $"Messages Added:{summary.MessageAddedCount} Removed:{summary.MessageRemovedCount} Modified:{summary.MessageModifiedCount} | Signals Added:{summary.SignalAddedCount} Removed:{summary.SignalRemovedCount} Modified:{summary.SignalModifiedCount}";
        return ObjectMapper.Map<CanSpecDiffSummary, CanSpecDiffSummaryDto>(summary);
    }

    public async Task<ExportResultDto> ExportDiffsAsync(Guid specImportId, int format)
    {
        var entity = await _specRepository.GetAsync(specImportId);
        var rows = entity.Diffs.Select(d => new
        {
            d.Type,
            d.EntityType,
            d.EntityName,
            d.MessageId,
            Severity = d.Severity.ToString(),
            d.ChangeCategory,
            d.ImpactedSubsystem,
            d.OldValue,
            d.NewValue,
            d.ChangeSummary,
            d.Details,
            d.ComparisonDate
        }).ToList();

        var request = new ExportDetectionRequest
        {
            Results = rows,
            Format = (ExportService.ExportFormat)format,
            FileNamePrefix = "can_spec_diffs",
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions { IncludeHeader = true }
        };
        var result = await _exportService.ExportDetectionResultsAsync(request);
        return new ExportResultDto
        {
            FileName = result.FileName,
            ContentType = result.ContentType,
            RecordCount = result.Metadata.RecordCount,
            Format = result.Metadata.Format,
            ExportedAt = result.Metadata.ExportedAt,
            Data = result.Data
        };
    }

    private static string DetectFormat(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".dbc" => "DBC",
            ".xml" => "XML",
            ".json" => "JSON",
            ".csv" => "CSV",
            _ => "UNKNOWN"
        };
    }

    private static string ComputeSha256(byte[] data)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(data));
    }

    private async Task<CanSpecImport?> FindLatestCompletedAsync()
    {
        var queryable = await _specRepository.GetQueryableAsync();
        return queryable
            .Where(x => x.Status == ImportStatus.Completed)
            .OrderByDescending(x => x.ImportDate)
            .FirstOrDefault();
    }

    private static ParseResult ParseSpecification(byte[] content, string format)
    {
        // Minimal stub: If CSV treat each line as MessageId,MessageName,Dlc,SignalName,StartBit,Length
        var result = new ParseResult();
        if (format == "CSV")
        {
            using var ms = new MemoryStream(content);
            using var reader = new StreamReader(ms, Encoding.UTF8, true, 1024, leaveOpen: false);
            string? line;
            var messages = new Dictionary<uint, CanSpecMessage>();
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 6) continue;
                if (!uint.TryParse(parts[0], out var msgId)) continue;
                var msgName = parts[1].Trim();
                if (!int.TryParse(parts[2], out var dlc)) dlc = 8;
                var signalName = parts[3].Trim();
                if (!int.TryParse(parts[4], out var startBit)) startBit = 0;
                if (!int.TryParse(parts[5], out var length)) length = 8;

                if (!messages.TryGetValue(msgId, out var message))
                {
                    message = new CanSpecMessage(msgId, msgName, dlc);
                    messages[msgId] = message;
                }
                var signal = new CanSpecSignal(signalName, startBit, length);
                message.Signals.Add(signal);
            }
            result.Messages.AddRange(messages.Values);
        }
        else
        {
            // For non-CSV formats placeholder (future implementation)
        }
        return result;
    }

    private static CanSpecDiffResult GenerateDiff(CanSpecImport previous, CanSpecImport current)
    {
        var diffResult = new CanSpecDiffResult();
    // Use anonymous key string concatenation for dictionary since tuple + custom comparer inference fails
    var prevMessages = previous.Messages.ToDictionary(m => $"{m.MessageId}:{m.Name}", StringComparer.OrdinalIgnoreCase);
    var currMessages = current.Messages.ToDictionary(m => $"{m.MessageId}:{m.Name}", StringComparer.OrdinalIgnoreCase);

        // Added / Removed Messages
        foreach (var kv in currMessages)
        {
            if (!prevMessages.ContainsKey(kv.Key))
            {
                var message = kv.Value;
                diffResult.Diffs.Add(new CanSpecDiff(DiffType.Added, "Message", message.Name)
                {
                    MessageId = message.MessageId,
                    ChangeCategory = "MessageAdded",
                    Severity = ChangeSeverity.Informational,
                    ChangeSummary = "Message added"
                });
            }
        }
        foreach (var kv in prevMessages)
        {
            if (!currMessages.ContainsKey(kv.Key))
            {
                var message = kv.Value;
                diffResult.Diffs.Add(new CanSpecDiff(DiffType.Removed, "Message", message.Name)
                {
                    MessageId = message.MessageId,
                    ChangeCategory = "MessageRemoved",
                    Severity = ChangeSeverity.Low,
                    ChangeSummary = "Message removed"
                });
            }
        }

        // Signal level comparisons
        foreach (var curr in currMessages.Values)
        {
            if (!prevMessages.TryGetValue($"{curr.MessageId}:{curr.Name}", out var prev)) continue;
            var prevSignals = prev.Signals.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            var currSignals = curr.Signals.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var s in currSignals.Values)
            {
                if (!prevSignals.ContainsKey(s.Name))
                {
                    diffResult.Diffs.Add(new CanSpecDiff(DiffType.Added, "Signal", s.Name)
                    {
                        MessageId = curr.MessageId,
                        ChangeCategory = "SignalAdded",
                        Severity = ChangeSeverity.Informational,
                        ChangeSummary = "Signal added"
                    });
                }
                else
                {
                    var prevSignal = prevSignals[s.Name];
                    if (prevSignal.BitLength != s.BitLength || prevSignal.StartBit != s.StartBit)
                    {
                        diffResult.Diffs.Add(new CanSpecDiff(DiffType.Modified, "Signal", s.Name)
                        {
                            MessageId = curr.MessageId,
                            ChangeCategory = "SignalLayoutChanged",
                            Severity = ChangeSeverity.Medium,
                            OldValue = $"StartBit={prevSignal.StartBit};Length={prevSignal.BitLength}",
                            NewValue = $"StartBit={s.StartBit};Length={s.BitLength}",
                            ChangeSummary = "Signal layout modified"
                        });
                    }
                }
            }
            foreach (var s in prevSignals.Values)
            {
                if (!currSignals.ContainsKey(s.Name))
                {
                    diffResult.Diffs.Add(new CanSpecDiff(DiffType.Removed, "Signal", s.Name)
                    {
                        MessageId = curr.MessageId,
                        ChangeCategory = "SignalRemoved",
                        Severity = ChangeSeverity.Low,
                        ChangeSummary = "Signal removed"
                    });
                }
            }
        }

        // Summary counters & severity aggregation
        foreach (var d in diffResult.Diffs)
        {
            diffResult.Summary.IncrementEntityCounter(d.EntityType, d.Type);
            diffResult.Summary.IncrementSeverity(d.Severity);
        }
        diffResult.Summary.SummaryText = $"Messages(+/-/~): {diffResult.Summary.MessageAddedCount}/{diffResult.Summary.MessageRemovedCount}/{diffResult.Summary.MessageModifiedCount}; Signals(+/-/~): {diffResult.Summary.SignalAddedCount}/{diffResult.Summary.SignalRemovedCount}/{diffResult.Summary.SignalModifiedCount}";
        return diffResult;
    }

    private class ParseResult
    {
        public List<CanSpecMessage> Messages { get; } = new();
    }
}

public class CanSpecImportInput
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? Version { get; set; }
}

public class ExportResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string Format { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}