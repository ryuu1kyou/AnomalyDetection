using System;
using System.Collections.Generic;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;

namespace AnomalyDetection.Statistics.Dtos;

public class SystemAnomalyReportDto
{
    // Report Metadata
    public Guid ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedBy { get; set; }
    public string GeneratedByUserName { get; set; } = string.Empty;
    public OemCode OemCode { get; set; } = new();
    
    // Report Parameters
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<CanSystemType> IncludedSystems { get; set; } = new();
    public List<AnomalyLevel> IncludedAnomalyLevels { get; set; } = new();
    public bool IncludeResolvedDetections { get; set; }
    public bool IncludeFalsePositives { get; set; }
    
    // Executive Summary
    public SystemAnomalySummaryDto ExecutiveSummary { get; set; } = new();
    
    // System-specific Reports
    public List<SystemSpecificReportDto> SystemReports { get; set; } = new();
    
    // Cross-system Analysis
    public List<CrossSystemAnalysisDto> CrossSystemAnalysis { get; set; } = new();
    
    // Recommendations
    public List<RecommendationDto> Recommendations { get; set; } = new();
    
    // Appendices
    public List<ReportAppendixDto> Appendices { get; set; } = new();
}

public class SystemAnomalySummaryDto
{
    public int TotalSystems { get; set; }
    public int SystemsWithAnomalies { get; set; }
    public int TotalDetections { get; set; }
    public int CriticalDetections { get; set; }
    public int UnresolvedDetections { get; set; }
    public double OverallHealthScore { get; set; } // 0-100 scale
    public CanSystemType MostProblematicSystem { get; set; }
    public CanSystemType MostReliableSystem { get; set; }
    public List<string> KeyFindings { get; set; } = new();
}

public class SystemSpecificReportDto
{
    public CanSystemType SystemType { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public double HealthScore { get; set; } // 0-100 scale
    
    // Detection Statistics
    public int TotalDetections { get; set; }
    public int ResolvedDetections { get; set; }
    public int UnresolvedDetections { get; set; }
    public int FalsePositives { get; set; }
    public double ResolutionRate { get; set; }
    public double FalsePositiveRate { get; set; }
    
    // Anomaly Level Breakdown
    public Dictionary<AnomalyLevel, int> DetectionsByLevel { get; set; } = new();
    public Dictionary<AnomalyLevel, TimeSpan> AverageResolutionTimeByLevel { get; set; } = new();
    
    // Signal Analysis
    public List<SignalAnomalyAnalysisDto> SignalAnalysis { get; set; } = new();
    
    // Detection Logic Performance
    public List<LogicPerformanceDto> LogicPerformance { get; set; } = new();
    
    // Trends
    public List<SystemTrendDto> Trends { get; set; } = new();
    
    // Issues and Concerns
    public List<string> IdentifiedIssues { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
}

public class SignalAnomalyAnalysisDto
{
    public Guid CanSignalId { get; set; }
    public string SignalName { get; set; } = string.Empty;
    public string CanId { get; set; } = string.Empty;
    public int DetectionCount { get; set; }
    public double AverageConfidence { get; set; }
    public AnomalyLevel HighestAnomalyLevel { get; set; }
    public DateTime LastDetection { get; set; }
    public bool IsFrequentlyDetected { get; set; }
    public List<string> CommonPatterns { get; set; } = new();
}

public class LogicPerformanceDto
{
    public Guid DetectionLogicId { get; set; }
    public string LogicName { get; set; } = string.Empty;
    public DetectionType DetectionType { get; set; }
    public int ExecutionCount { get; set; }
    public int DetectionCount { get; set; }
    public double DetectionRate { get; set; }
    public double AverageExecutionTime { get; set; }
    public double AverageConfidence { get; set; }
    public int FalsePositives { get; set; }
    public double Accuracy { get; set; }
    public string PerformanceRating { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
}

public class SystemTrendDto
{
    public DateTime Date { get; set; }
    public int DetectionCount { get; set; }
    public double AverageConfidence { get; set; }
    public double HealthScore { get; set; }
    public Dictionary<AnomalyLevel, int> DetectionsByLevel { get; set; } = new();
}

public class CrossSystemAnalysisDto
{
    public string AnalysisType { get; set; } = string.Empty; // Correlation, Cascade, Pattern
    public List<CanSystemType> InvolvedSystems { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public double CorrelationStrength { get; set; }
    public List<string> Observations { get; set; } = new();
    public List<string> Implications { get; set; } = new();
}

public class RecommendationDto
{
    public string Category { get; set; } = string.Empty; // Immediate, Short-term, Long-term
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CanSystemType> AffectedSystems { get; set; } = new();
    public string ExpectedImpact { get; set; } = string.Empty;
    public string ImplementationEffort { get; set; } = string.Empty;
}

public class ReportAppendixDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Data, Chart, Table, Technical
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}