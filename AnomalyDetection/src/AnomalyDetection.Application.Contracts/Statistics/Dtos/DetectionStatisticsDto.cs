using System;
using System.Collections.Generic;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;

namespace AnomalyDetection.Statistics.Dtos;

public class DetectionStatisticsDto
{
    // Overall Statistics
    public int TotalDetections { get; set; }
    public int TotalResolvedDetections { get; set; }
    public int TotalFalsePositives { get; set; }
    public int TotalOpenDetections { get; set; }
    public double ResolutionRate { get; set; }
    public double FalsePositiveRate { get; set; }
    
    // Time Period
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Anomaly Level Statistics
    public Dictionary<AnomalyLevel, int> DetectionsByAnomalyLevel { get; set; } = new();
    public Dictionary<AnomalyLevel, double> AverageConfidenceByLevel { get; set; } = new();
    
    // System Type Statistics
    public Dictionary<CanSystemType, int> DetectionsBySystemType { get; set; } = new();
    public Dictionary<CanSystemType, double> ResolutionRateBySystemType { get; set; } = new();
    
    // Detection Type Statistics
    public Dictionary<DetectionType, int> DetectionsByType { get; set; } = new();
    public Dictionary<DetectionType, TimeSpan> AverageResolutionTimeByType { get; set; } = new();
    
    // Time-based Statistics
    public List<DailyDetectionStatDto> DailyStatistics { get; set; } = new();
    public List<HourlyDetectionStatDto> HourlyStatistics { get; set; } = new();
    
    // Top Statistics
    public List<TopDetectionLogicStatDto> TopDetectionLogics { get; set; } = new();
    public List<TopSignalStatDto> TopSignals { get; set; } = new();
    
    // Performance Statistics
    public double AverageDetectionTime { get; set; }
    public double AverageResolutionTime { get; set; }
    public int TotalDetectionLogics { get; set; }
    public int ActiveDetectionLogics { get; set; }
    
    // Trend Analysis
    public double DetectionTrend { get; set; } // Percentage change from previous period
    public double ResolutionTrend { get; set; } // Percentage change from previous period
    public double ConfidenceTrend { get; set; } // Average confidence trend
}

public class DailyDetectionStatDto
{
    public DateTime Date { get; set; }
    public int TotalDetections { get; set; }
    public int ResolvedDetections { get; set; }
    public int FalsePositives { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<AnomalyLevel, int> DetectionsByLevel { get; set; } = new();
}

public class HourlyDetectionStatDto
{
    public int Hour { get; set; }
    public int TotalDetections { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<AnomalyLevel, int> DetectionsByLevel { get; set; } = new();
}

public class TopDetectionLogicStatDto
{
    public Guid DetectionLogicId { get; set; }
    public string LogicName { get; set; } = string.Empty;
    public int DetectionCount { get; set; }
    public double AverageConfidence { get; set; }
    public double ResolutionRate { get; set; }
    public double FalsePositiveRate { get; set; }
    public TimeSpan AverageResolutionTime { get; set; }
}

public class TopSignalStatDto
{
    public Guid CanSignalId { get; set; }
    public string SignalName { get; set; } = string.Empty;
    public string CanId { get; set; } = string.Empty;
    public CanSystemType SystemType { get; set; }
    public int DetectionCount { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<AnomalyLevel, int> DetectionsByLevel { get; set; } = new();
}