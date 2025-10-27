using System;
using System.Collections.Generic;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;

namespace AnomalyDetection.Statistics.Dtos;

public class GetDetectionStatisticsInput
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);
    
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
    
    public List<CanSystemType>? SystemTypes { get; set; }
    
    public List<AnomalyLevel>? AnomalyLevels { get; set; }
    
    public List<DetectionType>? DetectionTypes { get; set; }
    
    public List<ResolutionStatus>? ResolutionStatuses { get; set; }
    
    public OemCode? OemCode { get; set; }
    
    public Guid? DetectionLogicId { get; set; }
    
    public Guid? CanSignalId { get; set; }
    
    public bool IncludeTrends { get; set; } = true;
    
    public bool IncludeHourlyBreakdown { get; set; } = false;
    
    public bool IncludeDailyBreakdown { get; set; } = true;
    
    public bool IncludeTopStatistics { get; set; } = true;
    
    public int TopResultsLimit { get; set; } = 10;
    
    public StatisticsGroupBy GroupBy { get; set; } = StatisticsGroupBy.Day;
    
    public bool CompareWithPreviousPeriod { get; set; } = true;
}

public enum StatisticsGroupBy
{
    Hour = 1,
    Day = 2,
    Week = 3,
    Month = 4
}