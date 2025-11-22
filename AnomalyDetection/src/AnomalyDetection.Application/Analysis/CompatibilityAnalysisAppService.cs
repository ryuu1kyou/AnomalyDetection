using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using AnomalyDetection.CanSpecification;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.Analysis;

/// <summary>
/// Stub application service for compatibility / inheritance impact analysis across CAN specification revisions
/// (Req6 情報流用・継承 / Req7 車両フェーズ履歴管理 / Req16 最新 CAN 仕様連携 対応強化予定).
/// Provides aggregation & scoring of changes once full diff semantics (value ranges, scaling, subsystem) are available.
/// </summary>
public class CompatibilityAnalysisAppService : ApplicationService, ICompatibilityAnalysisAppService
{
    private readonly IRepository<CanSpecImport, Guid> _specImportRepository;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;

    public CompatibilityAnalysisAppService(
        IRepository<CanSpecImport, Guid> specImportRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository)
    {
        _specImportRepository = specImportRepository;
        _detectionLogicRepository = detectionLogicRepository;
    }

    public Task<CompatibilityAnalysisResultDto> AnalyzeAsync(CompatibilityAnalysisRequestDto input)
    {
        // TODO: Implement compatibility scoring:
        // 1. For each changed Message/Signal classify impact (Layout/Range/Frequency)
        // 2. Map affected DetectionLogics & SafetyTraceRecords
        // 3. Suggest required template adjustments & threshold recalculations
        // 4. Provide inheritance viability rating (High/Medium/Low)
        var result = new CompatibilityAnalysisResultDto
        {
            BaseSpecImportId = input.BaseSpecImportId,
            TargetSpecImportId = input.TargetSpecImportId,
            TotalChangedSignals = 0,
            HighImpactChanges = 0,
            RecommendedActions = new List<string>
            {
                "Implement diff -> logic mapping",
                "Collect historical false-positive rate pre/post change",
                "Trigger threshold re-optimization for impacted logics"
            },
            InheritanceRecommendation = "PendingData"
        };
        return Task.FromResult(result);
    }

    /// <summary>
    /// Returns impacted detection logics given a CAN spec import diff set (Req6/7/16 初期対応).
    /// Heuristic: A logic is impacted if any mapped signal's MessageId or Signal name matches a diff entry (Added/Removed/Modified).
    /// </summary>
    public async Task<DetectionLogicImpactResultDto> GetImpactedDetectionLogicsAsync(Guid specImportId)
    {
        var specImport = await _specImportRepository.GetAsync(specImportId);
        // Collect affected message IDs and signal names from diffs
        var affectedMessageIds = specImport.Diffs
            .Where(d => d.MessageId.HasValue)
            .Select(d => d.MessageId!.Value)
            .Distinct()
            .ToHashSet();
        var affectedSignalNames = specImport.Diffs
            .Where(d => d.EntityType.Equals("Signal", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.EntityName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Load all detection logics – in future optimize with join to CanSignal via ID
        var queryable = await _detectionLogicRepository.GetQueryableAsync();
        var allLogics = await AsyncExecuter.ToListAsync(queryable);

        var impacted = new List<DetectionLogicImpactDto>();
        foreach (var logic in allLogics)
        {
            // We don't have direct MessageId / SignalName on mapping; need signal repository lookups ideally.
            // For initial heuristic assume CanSignalId encodes message context via external join (deferred optimization).
            // Mark logic impacted if any mapping exists AND diffs contain Signal name found in Implementation JSON payload.
            // Use Implementation.Content (JSON/config or script) to attempt naive signal name match
            var implementationJson = logic.Implementation?.Content ?? string.Empty;
            var signalNameHits = affectedSignalNames.Where(name => implementationJson.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (signalNameHits.Count > 0)
            {
                impacted.Add(new DetectionLogicImpactDto
                {
                    DetectionLogicId = logic.Id,
                    Name = logic.Identity?.Name ?? string.Empty,
                    ImpactedSignals = signalNameHits,
                    ChangeCategories = specImport.Diffs
                        .Where(d => signalNameHits.Contains(d.EntityName, StringComparer.OrdinalIgnoreCase))
                        .Select(d => d.ChangeCategory)
                        .Distinct()
                        .ToList(),
                    SeverityScore = CalculateSeverityScore(specImport.Diffs, signalNameHits)
                });
            }
            else if (logic.SignalMappings.Any() && affectedMessageIds.Count > 0)
            {
                // TODO: When CanSignal entity exposes MessageId, cross-reference here.
            }
        }

        return new DetectionLogicImpactResultDto
        {
            SpecImportId = specImportId,
            ImpactedCount = impacted.Count,
            Items = impacted
        };
    }

    private static double CalculateSeverityScore(IEnumerable<CanSpecDiff> diffs, List<string> signalNames)
    {
        double score = 0;
        foreach (var diff in diffs.Where(d => signalNames.Contains(d.EntityName, StringComparer.OrdinalIgnoreCase)))
        {
            score += diff.Severity switch
            {
                ChangeSeverity.Critical => 5,
                ChangeSeverity.High => 3,
                ChangeSeverity.Medium => 2,
                ChangeSeverity.Low => 1,
                _ => 0.5
            };
        }
        return Math.Round(score, 2);
    }
}

public interface ICompatibilityAnalysisAppService
{
    Task<CompatibilityAnalysisResultDto> AnalyzeAsync(CompatibilityAnalysisRequestDto input);
    Task<DetectionLogicImpactResultDto> GetImpactedDetectionLogicsAsync(Guid specImportId);
}

public class CompatibilityAnalysisRequestDto
{
    public Guid BaseSpecImportId { get; set; }
    public Guid TargetSpecImportId { get; set; }
    public List<Guid>? RelatedDetectionLogicIds { get; set; }
}

public class CompatibilityAnalysisResultDto
{
    public Guid BaseSpecImportId { get; set; }
    public Guid TargetSpecImportId { get; set; }
    public int TotalChangedSignals { get; set; }
    public int HighImpactChanges { get; set; }
    public string InheritanceRecommendation { get; set; } = string.Empty; // High / Medium / Low / PendingData
    public List<string> RecommendedActions { get; set; } = new();
}

public class DetectionLogicImpactResultDto
{
    public Guid SpecImportId { get; set; }
    public int ImpactedCount { get; set; }
    public List<DetectionLogicImpactDto> Items { get; set; } = new();
}

public class DetectionLogicImpactDto
{
    public Guid DetectionLogicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> ImpactedSignals { get; set; } = new();
    public List<string> ChangeCategories { get; set; } = new();
    public double SeverityScore { get; set; }
}
