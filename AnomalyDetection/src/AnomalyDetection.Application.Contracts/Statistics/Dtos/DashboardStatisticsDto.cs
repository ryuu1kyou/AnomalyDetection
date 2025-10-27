using System;
using System.Collections.Generic;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;

namespace AnomalyDetection.Statistics.Dtos;

public class DashboardStatisticsDto
{
    // Key Performance Indicators
    public List<KpiCardDto> KpiCards { get; set; } = new();
    
    // Recent Activity
    public List<RecentDetectionDto> RecentDetections { get; set; } = new();
    
    // System Health Overview
    public List<SystemHealthDto> SystemHealth { get; set; } = new();
    
    // Charts Data
    public DetectionTrendChartDto DetectionTrends { get; set; } = new();
    public AnomalyLevelDistributionDto AnomalyLevelDistribution { get; set; } = new();
    public SystemTypeDistributionDto SystemTypeDistribution { get; set; } = new();
    
    // Alerts and Notifications
    public List<AlertDto> ActiveAlerts { get; set; } = new();
    
    // Quick Actions
    public List<QuickActionDto> QuickActions { get; set; } = new();
    
    // Last Updated
    public DateTime LastUpdated { get; set; }
}

public class KpiCardDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public double? PercentageChange { get; set; }
    public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
    public string Color { get; set; } = string.Empty; // Success, Warning, Danger, Info
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RecentDetectionDto
{
    public Guid Id { get; set; }
    public DateTime DetectedAt { get; set; }
    public string SignalName { get; set; } = string.Empty;
    public string CanId { get; set; } = string.Empty;
    public CanSystemType SystemType { get; set; }
    public AnomalyLevel AnomalyLevel { get; set; }
    public double ConfidenceScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public ResolutionStatus Status { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class SystemHealthDto
{
    public CanSystemType SystemType { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public double HealthScore { get; set; } // 0-100
    public string HealthStatus { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
    public int ActiveDetections { get; set; }
    public int RecentDetections { get; set; }
    public DateTime LastDetection { get; set; }
    public string StatusColor { get; set; } = string.Empty;
}

public class DetectionTrendChartDto
{
    public List<string> Labels { get; set; } = new(); // Date labels
    public List<TrendDatasetDto> Datasets { get; set; } = new();
}

public class TrendDatasetDto
{
    public string Label { get; set; } = string.Empty;
    public List<int> Data { get; set; } = new();
    public string BorderColor { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public bool Fill { get; set; } = false;
}

public class AnomalyLevelDistributionDto
{
    public List<string> Labels { get; set; } = new();
    public List<int> Data { get; set; } = new();
    public List<string> BackgroundColors { get; set; } = new();
}

public class SystemTypeDistributionDto
{
    public List<string> Labels { get; set; } = new();
    public List<int> Data { get; set; } = new();
    public List<string> BackgroundColors { get; set; } = new();
}

public class AlertDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // System, Detection, Performance
    public string Severity { get; set; } = string.Empty; // High, Medium, Low
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
}

public class QuickActionDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}