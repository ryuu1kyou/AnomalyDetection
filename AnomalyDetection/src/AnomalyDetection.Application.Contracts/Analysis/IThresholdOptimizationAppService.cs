using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Analysis;

public interface IThresholdOptimizationAppService : IApplicationService
{
    Task<ThresholdOptimizationResultDto> OptimizeAsync(ThresholdOptimizationRequestDto input);
    Task<ExportedFileDto> ExportAsync(ThresholdOptimizationExportDto input);
    Task<BulkThresholdApplyResultDto> ApplyRecommendedThresholdsAsync(BulkThresholdApplyRequestDto input);
}

public class ThresholdCandidateMetricDto
{
    public double Threshold { get; set; }
    public int TruePositives { get; set; }
    public int FalsePositives { get; set; }
    public int TrueNegatives { get; set; }
    public int FalseNegatives { get; set; }
}

public class ThresholdOptimizationRequestDto
{
    public Guid DetectionLogicId { get; set; }
    public List<ThresholdCandidateMetricDto> Candidates { get; set; } = new();
    public string Objective { get; set; } = "f1"; // f1|youden|balanced_accuracy
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class ThresholdOptimizationResultDto
{
    public Guid DetectionLogicId { get; set; }
    public double RecommendedThreshold { get; set; }
    public string Objective { get; set; } = string.Empty;
    public double BestScore { get; set; }
    public List<ThresholdEvaluationDto> Evaluations { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class ThresholdEvaluationDto
{
    public double Threshold { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1 { get; set; }
    public double TruePositiveRate { get; set; }
    public double FalsePositiveRate { get; set; }
    public double BalancedAccuracy { get; set; }
    public double YoudenJ { get; set; }
    public int Support { get; set; }
}

public class ThresholdOptimizationExportDto
{
    public Guid DetectionLogicId { get; set; }
    public string Format { get; set; } = "csv"; // csv|json|pdf|excel
    public string Objective { get; set; } = "f1";
    public List<ThresholdCandidateMetricDto> Candidates { get; set; } = new();
}

public class BulkThresholdApplyRequestDto
{
    public List<ThresholdApplyItemDto> Items { get; set; } = new();
    public bool RequireApprovedStatus { get; set; } = true; // safety: optionally limit to approved logics
}

public class ThresholdApplyItemDto
{
    public Guid DetectionLogicId { get; set; }
    public double Threshold { get; set; }
    public string Objective { get; set; } = string.Empty; // audit trail
}

public class BulkThresholdApplyResultDto
{
    public int Total { get; set; }
    public int Applied { get; set; }
    public int Skipped { get; set; }
    public List<ThresholdApplyOutcomeDto> Outcomes { get; set; } = new();
}

public class ThresholdApplyOutcomeDto
{
    public Guid DetectionLogicId { get; set; }
    public bool Applied { get; set; }
    public string Message { get; set; } = string.Empty;
    public double? NewThreshold { get; set; }
}

public class ExportedFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public string Format { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = Array.Empty<byte>();
}