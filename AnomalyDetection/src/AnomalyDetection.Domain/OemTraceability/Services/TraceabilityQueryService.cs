using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.OemTraceability.Models;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.OemTraceability.Services;

/// <summary>
/// トレーサビリティ照会サービス（拡張版）
/// OEM間トレーサビリティ追跡、カスタマイズ取得、差異分析機能を提供
/// </summary>
public class TraceabilityQueryService : DomainService
{
    private readonly IOemCustomizationRepository _customizationRepository;
    private readonly IOemApprovalRepository _approvalRepository;

    public TraceabilityQueryService(
        IOemCustomizationRepository customizationRepository,
        IOemApprovalRepository approvalRepository)
    {
        _customizationRepository = customizationRepository;
        _approvalRepository = approvalRepository;
    }

    /// <summary>
    /// OEM間トレーサビリティを追跡する
    /// </summary>
    /// <param name="entityId">対象エンティティID</param>
    /// <param name="entityType">エンティティタイプ</param>
    /// <returns>OEM間トレーサビリティ結果</returns>
    public async Task<OemTraceabilityResult> TraceAcrossOemsAsync(Guid entityId, string entityType)
    {
        // 全OEMのカスタマイズ履歴を取得
        var allCustomizations = await _customizationRepository.GetByEntityAsync(entityId, entityType);
        
        // 全OEMの承認記録を取得
        var allApprovals = await _approvalRepository.GetByEntityAsync(entityId, entityType);

        // OEM別にグループ化
        var oemGroups = allCustomizations
            .GroupBy(c => c.OemCode.Code)
            .ToList();

        var oemUsages = new List<OemUsageInfo>();

        foreach (var oemGroup in oemGroups)
        {
            var oemCode = oemGroup.Key;
            var customizations = oemGroup.ToList();
            var approvals = allApprovals.Where(a => a.OemCode.Code == oemCode).ToList();

            var oemUsage = new OemUsageInfo
            {
                OemCode = oemCode,
                UsageCount = customizations.Count,
                Vehicles = await GetVehiclesByOemAndEntityAsync(oemCode, entityId, entityType),
                CustomizationHistory = customizations.Select(c => new OemCustomizationSummary
                {
                    Id = c.Id,
                    Type = c.Type,
                    Status = c.Status,
                    CreatedAt = c.CreationTime,
                    Reason = c.CustomizationReason
                }).ToList(),
                ApprovalRecords = approvals.Select(a => new OemApprovalSummary
                {
                    Id = a.Id,
                    Type = a.Type,
                    Status = a.Status,
                    RequestedAt = a.RequestedAt,
                    ApprovedAt = a.ApprovedAt,
                    Reason = a.ApprovalReason
                }).ToList()
            };

            oemUsages.Add(oemUsage);
        }

        // OEM間差異分析を実行
        var crossOemDifferences = await AnalyzeCrossOemDifferencesAsync(allCustomizations);

        return new OemTraceabilityResult
        {
            EntityId = entityId,
            EntityType = entityType,
            OemUsages = oemUsages,
            CrossOemDifferences = crossOemDifferences
        };
    }

    /// <summary>
    /// OEMカスタマイズ情報を取得する
    /// </summary>
    /// <param name="oemCode">OEMコード</param>
    /// <param name="entityType">エンティティタイプ（オプション）</param>
    /// <returns>OEMカスタマイズリスト</returns>
    public async Task<List<OemCustomization>> GetOemCustomizationsAsync(string oemCode, string? entityType = null)
    {
        if (string.IsNullOrEmpty(entityType))
        {
            return await _customizationRepository.GetByOemAsync(oemCode);
        }
        else
        {
            var allCustomizations = await _customizationRepository.GetByOemAsync(oemCode);
            return allCustomizations.Where(c => c.EntityType == entityType).ToList();
        }
    }

    /// <summary>
    /// OEM間差異分析を実行する
    /// </summary>
    /// <param name="customizations">カスタマイズリスト</param>
    /// <returns>差異分析結果</returns>
    public async Task<CrossOemDifferencesAnalysis> AnalyzeCrossOemDifferencesAsync(
        List<OemCustomization> customizations)
    {
        var analysis = new CrossOemDifferencesAnalysis();

        // パラメータ差異分析
        analysis.ParameterDifferences = AnalyzeParameterDifferences(customizations);

        // 使用パターン差異分析
        analysis.UsagePatternDifferences = AnalyzeUsagePatternDifferences(customizations);

        // 推奨事項生成
        analysis.Recommendations = GenerateRecommendations(analysis);

        return await Task.FromResult(analysis);
    }

    /// <summary>
    /// パラメータ差異を分析する
    /// </summary>
    private Dictionary<string, List<OemParameterDifference>> AnalyzeParameterDifferences(
        List<OemCustomization> customizations)
    {
        var parameterDifferences = new Dictionary<string, List<OemParameterDifference>>();

        // パラメータ名でグループ化
        var parameterGroups = customizations
            .SelectMany(c => c.CustomParameters.Keys)
            .Distinct()
            .ToList();

        foreach (var parameterName in parameterGroups)
        {
            var differences = new List<OemParameterDifference>();

            var customizationsWithParam = customizations
                .Where(c => c.CustomParameters.ContainsKey(parameterName))
                .ToList();

            foreach (var customization in customizationsWithParam)
            {
                var originalValue = customization.OriginalParameters.GetValueOrDefault(parameterName);
                var customValue = customization.CustomParameters[parameterName];

                var difference = new OemParameterDifference
                {
                    OemCode = customization.OemCode.Code,
                    ParameterName = parameterName,
                    OriginalValue = originalValue,
                    CustomValue = customValue,
                    DifferencePercentage = CalculateDifferencePercentage(originalValue, customValue),
                    DifferenceDescription = GenerateDifferenceDescription(originalValue, customValue)
                };

                differences.Add(difference);
            }

            if (differences.Any())
            {
                parameterDifferences[parameterName] = differences;
            }
        }

        return parameterDifferences;
    }

    /// <summary>
    /// 使用パターン差異を分析する
    /// </summary>
    private List<UsagePatternDifference> AnalyzeUsagePatternDifferences(
        List<OemCustomization> customizations)
    {
        var patternDifferences = new List<UsagePatternDifference>();

        // カスタマイズタイプ別の使用頻度分析
        var typeFrequency = customizations
            .GroupBy(c => new { c.OemCode.Code, c.Type })
            .Select(g => new
            {
                OemCode = g.Key.Code,
                Type = g.Key.Type,
                Count = g.Count()
            })
            .ToList();

        var avgFrequency = typeFrequency.GroupBy(tf => tf.Type)
            .ToDictionary(g => g.Key, g => g.Average(x => x.Count));

        foreach (var freq in typeFrequency)
        {
            var avgForType = avgFrequency[freq.Type];
            var deviation = Math.Abs(freq.Count - avgForType) / avgForType;

            if (deviation > 0.5) // 50%以上の偏差がある場合
            {
                patternDifferences.Add(new UsagePatternDifference
                {
                    OemCode = freq.OemCode,
                    PatternType = $"CustomizationType_{freq.Type}",
                    Description = $"カスタマイズタイプ {freq.Type} の使用頻度が平均から {deviation:P0} 偏差",
                    Frequency = freq.Count,
                    Impact = deviation > 1.0 ? "High" : "Medium"
                });
            }
        }

        return patternDifferences;
    }

    /// <summary>
    /// 推奨事項を生成する
    /// </summary>
    private List<string> GenerateRecommendations(CrossOemDifferencesAnalysis analysis)
    {
        var recommendations = new List<string>();

        // パラメータ差異に基づく推奨事項
        foreach (var paramDiff in analysis.ParameterDifferences)
        {
            var paramName = paramDiff.Key;
            var differences = paramDiff.Value;

            if (differences.Count > 1)
            {
                var maxDiff = differences.Max(d => d.DifferencePercentage);
                if (maxDiff > 50) // 50%以上の差異
                {
                    recommendations.Add($"パラメータ '{paramName}' でOEM間に大きな差異があります。標準化を検討してください。");
                }
            }
        }

        // 使用パターン差異に基づく推奨事項
        var highImpactPatterns = analysis.UsagePatternDifferences
            .Where(p => p.Impact == "High")
            .ToList();

        if (highImpactPatterns.Any())
        {
            recommendations.Add("一部のOEMで使用パターンに大きな偏りがあります。ベストプラクティスの共有を検討してください。");
        }

        return recommendations;
    }

    /// <summary>
    /// 差異パーセンテージを計算する
    /// </summary>
    private double CalculateDifferencePercentage(object? originalValue, object? customValue)
    {
        if (originalValue == null || customValue == null)
            return 0;

        if (originalValue.Equals(customValue))
            return 0;

        // 数値の場合
        if (double.TryParse(originalValue.ToString(), out var origNum) &&
            double.TryParse(customValue.ToString(), out var custNum))
        {
            if (Math.Abs(origNum) < double.Epsilon)
                return custNum == 0 ? 0 : 100;

            return Math.Abs((custNum - origNum) / origNum) * 100;
        }

        // 文字列の場合は単純に異なるかどうか
        return originalValue.ToString() != customValue.ToString() ? 100 : 0;
    }

    /// <summary>
    /// 差異の説明を生成する
    /// </summary>
    private string GenerateDifferenceDescription(object? originalValue, object? customValue)
    {
        if (originalValue == null && customValue == null)
            return "両方とも未設定";

        if (originalValue == null)
            return $"元の値: 未設定 → カスタム値: {customValue}";

        if (customValue == null)
            return $"元の値: {originalValue} → カスタム値: 未設定";

        return $"元の値: {originalValue} → カスタム値: {customValue}";
    }

    /// <summary>
    /// OEMとエンティティに関連する車両リストを取得する（プレースホルダー実装）
    /// </summary>
    private async Task<List<string>> GetVehiclesByOemAndEntityAsync(string oemCode, Guid entityId, string entityType)
    {
        // 実際の実装では、車両フェーズリポジトリなどから取得
        // ここではプレースホルダーとして空のリストを返す
        await Task.CompletedTask;
        return new List<string>();
    }
}