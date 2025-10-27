using System;
using System.Collections.Generic;

namespace AnomalyDetection.OemTraceability.Models;

/// <summary>
/// OEM間トレーサビリティ結果
/// </summary>
public class OemTraceabilityResult
{
    /// <summary>
    /// 対象エンティティID
    /// </summary>
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// エンティティタイプ
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// OEM別使用情報
    /// </summary>
    public List<OemUsageInfo> OemUsages { get; set; } = new();
    
    /// <summary>
    /// OEM間差異分析結果
    /// </summary>
    public CrossOemDifferencesAnalysis CrossOemDifferences { get; set; } = new();
}

/// <summary>
/// OEM使用情報
/// </summary>
public class OemUsageInfo
{
    /// <summary>
    /// OEMコード
    /// </summary>
    public string OemCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 使用回数
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// 使用車両リスト
    /// </summary>
    public List<string> Vehicles { get; set; } = new();
    
    /// <summary>
    /// カスタマイズ履歴
    /// </summary>
    public List<OemCustomizationSummary> CustomizationHistory { get; set; } = new();
    
    /// <summary>
    /// 承認記録
    /// </summary>
    public List<OemApprovalSummary> ApprovalRecords { get; set; } = new();
}

/// <summary>
/// OEMカスタマイズ概要
/// </summary>
public class OemCustomizationSummary
{
    public Guid Id { get; set; }
    public CustomizationType Type { get; set; }
    public CustomizationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// OEM承認概要
/// </summary>
public class OemApprovalSummary
{
    public Guid Id { get; set; }
    public ApprovalType Type { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// OEM間差異分析
/// </summary>
public class CrossOemDifferencesAnalysis
{
    /// <summary>
    /// パラメータ差異
    /// </summary>
    public Dictionary<string, List<OemParameterDifference>> ParameterDifferences { get; set; } = new();
    
    /// <summary>
    /// 使用パターン差異
    /// </summary>
    public List<UsagePatternDifference> UsagePatternDifferences { get; set; } = new();
    
    /// <summary>
    /// 推奨事項
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// OEMパラメータ差異
/// </summary>
public class OemParameterDifference
{
    public string OemCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public object? OriginalValue { get; set; }
    public object? CustomValue { get; set; }
    public double DifferencePercentage { get; set; }
    public string DifferenceDescription { get; set; } = string.Empty;
}

/// <summary>
/// 使用パターン差異
/// </summary>
public class UsagePatternDifference
{
    public string OemCode { get; set; } = string.Empty;
    public string PatternType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public string Impact { get; set; } = string.Empty;
}