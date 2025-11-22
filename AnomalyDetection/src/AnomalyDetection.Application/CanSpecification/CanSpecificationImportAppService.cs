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
    private readonly ICanSpecificationParser _parser;

    public CanSpecificationImportAppService(
        IRepository<CanSpecImport, Guid> specRepository,
        ExportService exportService,
        IGuidGenerator guidGenerator,
        ICanSpecificationParser parser)
    {
        _specRepository = specRepository;
        _exportService = exportService;
        _guidGenerator = guidGenerator;
        _parser = parser;
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
            var parseResult = _parser.Parse(input.Content, entity.FileFormat);
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

    private string DetectFormat(string fileName)
    {
        return Path.GetExtension(fileName)?.TrimStart('.').ToUpperInvariant() ?? "UNKNOWN";
    }

    private static string ComputeSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private async Task<CanSpecImport?> FindLatestCompletedAsync()
    {
        var queryable = await _specRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            queryable
                .Where(x => x.Status == ImportStatus.Completed)
                .OrderByDescending(x => x.CreationTime)
        );
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