using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using AnomalyDetection.Permissions;
using AnomalyDetection.Shared.Export;
using AnomalyDetection.Analysis;
using Volo.Abp.Domain.Repositories;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.Analysis;

/// <summary>
/// Provides threshold optimization evaluation & export based on candidate metrics.
/// </summary>
public class ThresholdOptimizationAppService : ApplicationService, IThresholdOptimizationAppService
{
    private readonly ExportService _exportService;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _logicRepository;

    public ThresholdOptimizationAppService(ExportService exportService, IRepository<CanAnomalyDetectionLogic, Guid> logicRepository)
    {
        _exportService = exportService;
        _logicRepository = logicRepository;
    }

    [Authorize(AnomalyDetectionPermissions.Analysis.ThresholdOptimization.Calculate)]
    public Task<ThresholdOptimizationResultDto> OptimizeAsync(ThresholdOptimizationRequestDto input)
    {
        if (input.Candidates == null || input.Candidates.Count == 0)
        {
            throw new ArgumentException("No candidate metrics supplied", nameof(input.Candidates));
        }

        var evaluations = EvaluateCandidates(input.Candidates);

        Func<ThresholdEvaluationDto, double> selector = input.Objective.ToLowerInvariant() switch
        {
            "youden" => e => e.YoudenJ,
            "balanced_accuracy" => e => e.BalancedAccuracy,
            _ => e => e.F1
        };

        var best = evaluations.OrderByDescending(selector).First();

        var summary = $"Objective={input.Objective}, Recommended={best.Threshold:F4}, Score={selector(best):F4}";
        var result = new ThresholdOptimizationResultDto
        {
            DetectionLogicId = input.DetectionLogicId,
            RecommendedThreshold = best.Threshold,
            Objective = input.Objective,
            BestScore = Math.Round(selector(best), 4),
            Evaluations = evaluations,
            Summary = summary
        };

        return Task.FromResult(result);
    }

    private static List<ThresholdEvaluationDto> EvaluateCandidates(IEnumerable<ThresholdCandidateMetricDto> candidates)
    {
        var evaluations = new List<ThresholdEvaluationDto>();
        foreach (var c in candidates.OrderBy(x => x.Threshold))
        {
            var precision = c.TruePositives + c.FalsePositives == 0 ? 0 : (double)c.TruePositives / (c.TruePositives + c.FalsePositives);
            var recall = c.TruePositives + c.FalseNegatives == 0 ? 0 : (double)c.TruePositives / (c.TruePositives + c.FalseNegatives);
            var f1 = precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall);
            var tpr = recall;
            var fpr = c.FalsePositives + c.TrueNegatives == 0 ? 0 : (double)c.FalsePositives / (c.FalsePositives + c.TrueNegatives);
            var balancedAccuracy = (tpr + (c.TrueNegatives + c.FalsePositives == 0 ? 0 : (double)c.TrueNegatives / (c.TrueNegatives + c.FalsePositives))) / 2.0;
            var youden = tpr - fpr;

            evaluations.Add(new ThresholdEvaluationDto
            {
                Threshold = c.Threshold,
                Precision = Math.Round(precision, 4),
                Recall = Math.Round(recall, 4),
                F1 = Math.Round(f1, 4),
                TruePositiveRate = Math.Round(tpr, 4),
                FalsePositiveRate = Math.Round(fpr, 4),
                BalancedAccuracy = Math.Round(balancedAccuracy, 4),
                YoudenJ = Math.Round(youden, 4),
                Support = c.TruePositives + c.TrueNegatives + c.FalsePositives + c.FalseNegatives
            });
        }
        return evaluations;
    }

    [Authorize(AnomalyDetectionPermissions.Analysis.ThresholdOptimization.Export)]
    public async Task<ExportedFileDto> ExportAsync(ThresholdOptimizationExportDto input)
    {
        if (input.Candidates == null || input.Candidates.Count == 0)
        {
            throw new ArgumentException("Candidates required for export", nameof(input.Candidates));
        }

        // Reuse optimization to get evaluations & best threshold
        var optimizeResult = await OptimizeAsync(new ThresholdOptimizationRequestDto
        {
            DetectionLogicId = input.DetectionLogicId,
            Objective = input.Objective,
            Candidates = input.Candidates
        });

        var rows = optimizeResult.Evaluations
            .OrderBy(e => e.Threshold)
            .Select(e => new
            {
                e.Threshold,
                e.Precision,
                e.Recall,
                e.F1,
                e.TruePositiveRate,
                e.FalsePositiveRate,
                e.BalancedAccuracy,
                e.YoudenJ,
                e.Support,
                optimizeResult.Objective,
                optimizeResult.RecommendedThreshold,
                optimizeResult.BestScore
            }).ToList();

        var formatEnum = input.Format?.ToLower() switch
        {
            "json" => ExportService.ExportFormat.Json,
            "pdf" => ExportService.ExportFormat.Pdf,
            "excel" => ExportService.ExportFormat.Excel,
            _ => ExportService.ExportFormat.Csv
        };

        var request = new ExportDetectionRequest
        {
            Results = rows,
            Format = formatEnum,
            FileNamePrefix = $"threshold_opt_{input.DetectionLogicId}" ,
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions { IncludeHeader = true }
        };

        var result = await _exportService.ExportDetectionResultsAsync(request);
        return new ExportedFileDto
        {
            FileName = result.FileName,
            ContentType = result.ContentType,
            RecordCount = result.Metadata.RecordCount,
            Format = result.Metadata.Format,
            ExportedAt = result.Metadata.ExportedAt,
            FileData = result.Data
        };
    }

    [Authorize(AnomalyDetectionPermissions.Analysis.ThresholdOptimization.Apply)]
    public async Task<BulkThresholdApplyResultDto> ApplyRecommendedThresholdsAsync(BulkThresholdApplyRequestDto input)
    {
        if (input.Items == null || input.Items.Count == 0)
            throw new ArgumentException("No thresholds to apply", nameof(input.Items));

        var outcomes = new List<ThresholdApplyOutcomeDto>();
        foreach (var item in input.Items)
        {
            try
            {
                var logic = await _logicRepository.GetAsync(item.DetectionLogicId);
                if (input.RequireApprovedStatus && logic.Status != DetectionLogicStatus.Approved)
                {
                    outcomes.Add(new ThresholdApplyOutcomeDto
                    {
                        DetectionLogicId = item.DetectionLogicId,
                        Applied = false,
                        Message = "Skipped: logic not approved"
                    });
                    continue;
                }

                // Store threshold as parameter 'threshold' if exists, else create
                var param = logic.Parameters.FirstOrDefault(p => p.Name.Equals("threshold", StringComparison.OrdinalIgnoreCase));
                if (param != null)
                {
                    param.UpdateValue(item.Threshold.ToString("F4"));
                }
                else
                {
                    // Create parameter with DataType=Double, defaultValue=threshold, required=true
                    var constraints = new ParameterConstraints(minValue: 0); // simple lower bound
                    var newParam = new DetectionParameter(
                        name: "threshold",
                        dataType: ParameterDataType.Double,
                        defaultValue: item.Threshold.ToString("F4"),
                        constraints: constraints,
                        description: "Auto-applied recommended threshold",
                        isRequired: true,
                        unit: null);
                    logic.AddParameter(newParam);
                }

                // NOTE: Domain aggregate modifications saved via repository update
                await _logicRepository.UpdateAsync(logic);

                outcomes.Add(new ThresholdApplyOutcomeDto
                {
                    DetectionLogicId = item.DetectionLogicId,
                    Applied = true,
                    Message = $"Applied threshold {item.Threshold:F4} (Objective={item.Objective})",
                    NewThreshold = item.Threshold
                });
            }
            catch (Exception ex)
            {
                outcomes.Add(new ThresholdApplyOutcomeDto
                {
                    DetectionLogicId = item.DetectionLogicId,
                    Applied = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        return new BulkThresholdApplyResultDto
        {
            Total = input.Items.Count,
            Applied = outcomes.Count(o => o.Applied),
            Skipped = outcomes.Count(o => !o.Applied),
            Outcomes = outcomes
        };
    }
}