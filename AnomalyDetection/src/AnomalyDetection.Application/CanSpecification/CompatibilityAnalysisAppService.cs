using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Volo.Abp.Caching;
using AnomalyDetection.Services;

namespace AnomalyDetection.CanSpecification;

public class CompatibilityAnalysisAppService : ApplicationService, ICompatibilityAnalysisAppService
{
    private readonly IRepository<CompatibilityAnalysis, Guid> _analysisRepository;
    private readonly IRepository<CanSpecImport, Guid> _specRepository;
    private readonly CompatibilityAnalyzer _analyzer;
    private readonly CanSpecDiffService _diffService;
    private readonly IDistributedCache<CompatibilityStatusCacheItem> _statusCache;

    private const string CompatibilityStatusCacheKeyFormat = "CanSpec:CompatStatus:{0}:{1}:{2}";

    public CompatibilityAnalysisAppService(
        IRepository<CompatibilityAnalysis, Guid> analysisRepository,
        IRepository<CanSpecImport, Guid> specRepository,
        CompatibilityAnalyzer analyzer,
        CanSpecDiffService diffService,
        IDistributedCache<CompatibilityStatusCacheItem> statusCache)
    {
        _analysisRepository = analysisRepository;
        _specRepository = specRepository;
        _analyzer = analyzer;
        _diffService = diffService;
        _statusCache = statusCache;
    }

    public async Task<CompatibilityAnalysisResultDto> AnalyzeCompatibilityAsync(
        CreateCompatibilityAnalysisDto input)
    {
        var result = new CompatibilityAnalysisResultDto();

        try
        {
            // Load specifications
            var oldSpec = await _specRepository.GetAsync(input.OldSpecId);
            var newSpec = await _specRepository.GetAsync(input.NewSpecId);

            if (oldSpec.Status != ImportStatus.Completed || newSpec.Status != ImportStatus.Completed)
            {
                result.Success = false;
                result.ErrorMessage = "Both specifications must be successfully imported";
                return result;
            }

            // Perform analysis
            var analysis = _analyzer.Analyze(
                oldSpec,
                newSpec,
                CurrentUser.Id?.ToString() ?? "System"
            );

            // Save analysis
            await _analysisRepository.InsertAsync(analysis);

            // Map result
            result.Success = true;
            result.AnalysisId = analysis.Id;
            result.CompatibilityLevel = (int)analysis.CompatibilityLevel;
            result.CompatibilityScore = analysis.CompatibilityScore;
            result.MigrationRisk = (int)analysis.MigrationRisk;
            result.BreakingChangeCount = analysis.BreakingChangeCount;
            result.WarningCount = analysis.WarningCount;
            result.InfoCount = analysis.InfoCount;
            result.Summary = analysis.Summary;

            result.Issues = ObjectMapper.Map<List<CompatibilityIssue>, List<CompatibilityIssueDto>>(
                analysis.Issues);
            result.Impacts = ObjectMapper.Map<List<ImpactAssessment>, List<ImpactAssessmentDto>>(
                analysis.Impacts);

            Logger.LogInformation(
                "Compatibility analysis completed: {OldSpec} -> {NewSpec}, Score: {Score}",
                oldSpec.FileName, newSpec.FileName, analysis.CompatibilityScore);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Compatibility analysis failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<CompatibilityStatusDto> AssessCompatibilityAsync(CompatibilityAssessmentRequestDto input)
    {
        var context = string.IsNullOrWhiteSpace(input.Context)
            ? "General"
            : input.Context.Trim();

        var cacheKey = string.Format(
            CompatibilityStatusCacheKeyFormat,
            context.ToUpperInvariant(),
            input.OldSpecId,
            input.NewSpecId);

        if (!input.ForceRefresh)
        {
            var cached = await _statusCache.GetAsync(cacheKey);
            if (cached != null)
            {
                Logger.LogDebug("Compatibility status cache hit for {Context} {OldSpec}->{NewSpec}", context, input.OldSpecId, input.NewSpecId);
                return cached.ToDto();
            }
        }

        var oldSpec = await _specRepository.GetAsync(input.OldSpecId);
        var newSpec = await _specRepository.GetAsync(input.NewSpecId);

        if (oldSpec.Status != ImportStatus.Completed || newSpec.Status != ImportStatus.Completed)
        {
            throw new BusinessException("AnomalyDetection:CompatibilityAssessment")
                .WithData("message", "Both specifications must be successfully imported before assessment");
        }

        var diffResult = _diffService.CompareSpecifications(oldSpec, newSpec);
        var status = BuildCompatibilityStatusDto(context, oldSpec, newSpec, diffResult);

        var cacheItem = CompatibilityStatusCacheItem.FromDto(status);
        await _statusCache.SetAsync(cacheKey, cacheItem, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        Logger.LogInformation(
            "Compatibility quick assessment generated for {Context}: compatible={IsCompatible}, highestSeverity={Severity}",
            context,
            status.IsCompatible,
            status.HighestSeverity);

        return status;
    }

    public async Task<PagedResultDto<CompatibilityAnalysisDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        var query = await _analysisRepository.GetQueryableAsync();

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(a => a.AnalysisDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        return new PagedResultDto<CompatibilityAnalysisDto>(
            totalCount,
            ObjectMapper.Map<List<CompatibilityAnalysis>, List<CompatibilityAnalysisDto>>(items)
        );
    }

    public async Task<CompatibilityAnalysisDto> GetAsync(Guid id)
    {
        var analysis = await _analysisRepository.GetAsync(id);
        return ObjectMapper.Map<CompatibilityAnalysis, CompatibilityAnalysisDto>(analysis);
    }

    public async Task<PagedResultDto<CompatibilityIssueDto>> GetIssuesBySeverityAsync(
        Guid analysisId,
        int severity,
        PagedAndSortedResultRequestDto input)
    {
        var analysis = await _analysisRepository.GetAsync(analysisId);

        var filteredIssues = analysis.Issues
            .Where(i => (int)i.Severity == severity)
            .ToList();

        var pagedIssues = filteredIssues
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<CompatibilityIssueDto>(
            filteredIssues.Count,
            ObjectMapper.Map<List<CompatibilityIssue>, List<CompatibilityIssueDto>>(pagedIssues)
        );
    }

    public async Task<PagedResultDto<ImpactAssessmentDto>> GetImpactsAsync(
        Guid analysisId,
        PagedAndSortedResultRequestDto input)
    {
        var analysis = await _analysisRepository.GetAsync(analysisId);

        var pagedImpacts = analysis.Impacts
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<ImpactAssessmentDto>(
            analysis.Impacts.Count,
            ObjectMapper.Map<List<ImpactAssessment>, List<ImpactAssessmentDto>>(pagedImpacts)
        );
    }

    public async Task DeleteAsync(Guid id)
    {
        await _analysisRepository.DeleteAsync(id);
    }

    private CompatibilityStatusDto BuildCompatibilityStatusDto(
        string context,
        CanSpecImport oldSpec,
        CanSpecImport newSpec,
        CanSpecDiffResult diffResult)
    {
        var summary = diffResult.Summary;
        var highestSeverity = diffResult.Diffs.Any()
            ? diffResult.Diffs.Max(d => d.Severity)
            : ChangeSeverity.Informational;

        var status = new CompatibilityStatusDto
        {
            OldSpecId = oldSpec.Id,
            NewSpecId = newSpec.Id,
            Context = context,
            IsCompatible = DetermineCompatibility(context, highestSeverity, summary),
            HighestSeverity = (int)highestSeverity,
            BreakingChangeCount = summary.SeverityCriticalCount + summary.SeverityHighCount,
            WarningCount = summary.SeverityMediumCount + summary.SeverityLowCount,
            InfoCount = summary.SeverityInformationalCount,
            CompatibilityScore = CalculateCompatibilityScore(summary),
            Summary = summary.SummaryText,
            ImpactedSubsystems = new List<string>(summary.ImpactedSubsystems),
            KeyFindings = BuildKeyFindings(diffResult.Diffs),
            GeneratedAt = Clock.Now
        };

        return status;
    }

    private bool DetermineCompatibility(string context, ChangeSeverity highestSeverity, CanSpecDiffSummary summary)
    {
        var normalized = context.ToUpperInvariant();

        return normalized switch
        {
            "REGULATORY" => highestSeverity <= ChangeSeverity.Medium && summary.MessageRemovedCount == 0,
            "SAFETY" => highestSeverity <= ChangeSeverity.Medium && summary.MessageRemovedCount == 0 && summary.SignalRemovedCount == 0,
            "DIAGNOSTICS" => highestSeverity <= ChangeSeverity.High,
            _ => highestSeverity <= ChangeSeverity.High
        };
    }

    private double CalculateCompatibilityScore(CanSpecDiffSummary summary)
    {
        var penalty = (summary.SeverityCriticalCount * 25) +
                      (summary.SeverityHighCount * 12) +
                      (summary.SeverityMediumCount * 6) +
                      (summary.SeverityLowCount * 2) +
                      summary.SeverityInformationalCount;

        return Math.Max(0, 100 - penalty);
    }

    private List<string> BuildKeyFindings(IEnumerable<CanSpecDiff> diffs)
    {
        return diffs
            .OrderByDescending(d => d.Severity)
            .ThenByDescending(d => d.ComparisonDate)
            .Take(5)
            .Select(d =>
            {
                var summary = string.IsNullOrWhiteSpace(d.ChangeSummary)
                    ? $"{d.EntityType} '{d.EntityName}' change ({d.ChangeCategory})"
                    : d.ChangeSummary;

                return $"{summary} [Severity: {d.Severity}]";
            })
            .ToList();
    }
}
