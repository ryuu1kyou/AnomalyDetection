using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using AnomalyDetection.Services;
using AnomalyDetection.Shared.Export;
using Microsoft.AspNetCore.Authorization;
using AnomalyDetection.Permissions;

namespace AnomalyDetection.CanSpecification;

public class CanSpecImportAppService : ApplicationService, ICanSpecImportAppService
{
    private readonly IRepository<CanSpecImport, Guid> _specRepository;
    private readonly DbcParser _dbcParser;
    private readonly CanSpecDiffService _diffService;
    private readonly ExportService _exportService;

    public CanSpecImportAppService(
        IRepository<CanSpecImport, Guid> specRepository,
        DbcParser dbcParser,
        CanSpecDiffService diffService,
        ExportService exportService)
    {
        _specRepository = specRepository;
        _dbcParser = dbcParser;
        _diffService = diffService;
        _exportService = exportService;
    }

    [Authorize(AnomalyDetectionPermissions.CanSpecification.Import)]
    public async Task<CanSpecImportResultDto> ImportSpecificationAsync(
        Stream fileStream,
        CreateCanSpecImportDto input)
    {
        var result = new CanSpecImportResultDto();

        try
        {
            // Calculate file hash
            var fileHash = await ComputeFileHashAsync(fileStream);
            fileStream.Position = 0;

            // Check for duplicate
            var existing = await _specRepository.FirstOrDefaultAsync(s => s.FileHash == fileHash);
            if (existing != null)
            {
                result.Success = false;
                result.ErrorMessage = "This specification file has already been imported";
                result.ImportId = existing.Id;
                return result;
            }

            // Parse file
            DbcParseResult parseResult;
            if (input.FileFormat.ToUpper() == "DBC")
            {
                parseResult = await _dbcParser.ParseAsync(fileStream);
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = $"Unsupported file format: {input.FileFormat}";
                return result;
            }

            if (!parseResult.Success)
            {
                result.Success = false;
                result.ErrorMessage = parseResult.ErrorMessage;
                return result;
            }

            // Create import entity
            var import = new CanSpecImport(
                GuidGenerator.Create(),
                input.FileName,
                input.FileFormat,
                fileStream.Length,
                fileHash,
                CurrentUser.Id?.ToString() ?? "System"
            )
            {
                Version = input.Version ?? parseResult.Version,
                Description = input.Description
            };

            // Add messages and signals
            foreach (var message in parseResult.Messages)
            {
                message.CanSpecImportId = import.Id;
                foreach (var signal in message.Signals)
                {
                    signal.MessageId = message.Id;
                }
                import.AddMessage(message);
            }

            import.MarkAsCompleted(parseResult.MessageCount, parseResult.SignalCount);

            // Compare with latest spec
            var queryable = await _specRepository.GetQueryableAsync();
            var latestSpec = await AsyncExecuter.FirstOrDefaultAsync(
                queryable
                    .Where(s => s.Status == ImportStatus.Completed)
                    .OrderByDescending(s => s.ImportDate)
            );

            if (latestSpec != null && latestSpec.Id != import.Id)
            {
                var diffResult = _diffService.CompareSpecifications(latestSpec, import);
                foreach (var diff in diffResult.Diffs)
                {
                    import.AddDiff(diff);
                }

                result.Diffs = ObjectMapper.Map<List<CanSpecDiff>, List<CanSpecDiffDto>>(diffResult.Diffs);
                result.DiffSummary = ObjectMapper.Map<CanSpecDiffSummary, CanSpecDiffSummaryDto>(diffResult.Summary);
            }

            await _specRepository.InsertAsync(import);

            result.Success = true;
            result.ImportId = import.Id;
            result.MessageCount = parseResult.MessageCount;
            result.SignalCount = parseResult.SignalCount;

            Logger.LogInformation(
                "CAN spec imported: {FileName}, Messages: {MessageCount}, Signals: {SignalCount}",
                input.FileName, parseResult.MessageCount, parseResult.SignalCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Import failed: {FileName}", input.FileName);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<PagedResultDto<CanSpecImportDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        // Optional view permission check
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.View);
        var query = await _specRepository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(s => s.ImportDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<CanSpecImportDto>(
            totalCount,
            ObjectMapper.Map<List<CanSpecImport>, List<CanSpecImportDto>>(items)
        );
    }

    public async Task<CanSpecImportDto> GetAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.View);
        var spec = await _specRepository.GetAsync(id);
        var dto = ObjectMapper.Map<CanSpecImport, CanSpecImportDto>(spec);
        var summary = BuildDiffSummary(spec.Diffs);
        dto.DiffSummary = ObjectMapper.Map<CanSpecDiffSummary, CanSpecDiffSummaryDto>(summary);
        return dto;
    }

    public async Task<PagedResultDto<CanSpecMessageDto>> GetMessagesAsync(
        Guid specId,
        PagedAndSortedResultRequestDto input)
    {
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.View);
        var spec = await _specRepository.GetAsync(specId);

        var messages = spec.Messages
            .OrderBy(m => m.MessageId)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<CanSpecMessageDto>(
            spec.Messages.Count,
            ObjectMapper.Map<List<CanSpecMessage>, List<CanSpecMessageDto>>(messages)
        );
    }

    public async Task<CanSpecImportResultDto> ComparSpecificationsAsync(
        Guid oldSpecId,
        Guid newSpecId)
    {
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.View);
        var oldSpec = await _specRepository.GetAsync(oldSpecId);
        var newSpec = await _specRepository.GetAsync(newSpecId);

        var diffResult = _diffService.CompareSpecifications(oldSpec, newSpec);

        // Save diffs to new spec
        foreach (var diff in diffResult.Diffs)
        {
            newSpec.AddDiff(diff);
        }
        await _specRepository.UpdateAsync(newSpec);

        return new CanSpecImportResultDto
        {
            Success = true,
            ImportId = newSpecId,
            Diffs = ObjectMapper.Map<List<CanSpecDiff>, List<CanSpecDiffDto>>(diffResult.Diffs),
            DiffSummary = ObjectMapper.Map<CanSpecDiffSummary, CanSpecDiffSummaryDto>(diffResult.Summary)
        };
    }

    public async Task<PagedResultDto<CanSpecDiffDto>> GetDiffsAsync(
        Guid specId,
        PagedAndSortedResultRequestDto input)
    {
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.Diff.View);
        var spec = await _specRepository.GetAsync(specId);

        var diffs = spec.Diffs
            .OrderByDescending(d => d.ComparisonDate)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<CanSpecDiffDto>(
            spec.Diffs.Count,
            ObjectMapper.Map<List<CanSpecDiff>, List<CanSpecDiffDto>>(diffs)
        );
    }

    public async Task<CanSpecDiffSummaryDto> GetDiffSummaryAsync(Guid specId)
    {
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.Diff.View);
        var spec = await _specRepository.GetAsync(specId);
        var summary = BuildDiffSummary(spec.Diffs);
        return ObjectMapper.Map<CanSpecDiffSummary, CanSpecDiffSummaryDto>(summary);
    }

    public async Task DeleteAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(AnomalyDetectionPermissions.CanSpecification.View);
        await _specRepository.DeleteAsync(id);
    }

    [Authorize(AnomalyDetectionPermissions.CanSpecification.Diff.Export)]
    public async Task<byte[]> ExportDiffsAsync(Guid specId, string format)
    {
        var spec = await _specRepository.GetAsync(specId);
        var rows = spec.Diffs.Select(d => new
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

        var exportFormat = format?.ToLower() switch
        {
            "json" => ExportService.ExportFormat.Json,
            "pdf" => ExportService.ExportFormat.Pdf,
            "excel" => ExportService.ExportFormat.Excel,
            _ => ExportService.ExportFormat.Csv
        };

        var request = new ExportDetectionRequest
        {
            Results = rows,
            Format = exportFormat,
            FileNamePrefix = $"can_spec_diffs_{specId}" ,
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions { IncludeHeader = true }
        };

        var result = await _exportService.ExportDetectionResultsAsync(request);
        return result.Data;
    }

    private async Task<string> ComputeFileHashAsync(Stream stream)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private CanSpecDiffSummary BuildDiffSummary(IEnumerable<CanSpecDiff> diffs)
    {
        var summary = new CanSpecDiffSummary();

        foreach (var diff in diffs)
        {
            summary.IncrementEntityCounter(diff.EntityType, diff.Type);
            summary.IncrementSeverity(diff.Severity);
            summary.TrackSubsystem(diff.ImpactedSubsystem);
        }

        summary.SummaryText = BuildSummaryText(summary);
        return summary;
    }

    private static string BuildSummaryText(CanSpecDiffSummary summary)
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
