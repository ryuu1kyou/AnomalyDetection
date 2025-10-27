using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;

namespace AnomalyDetection.Statistics.Dtos;

public class GenerateSystemReportInput
{
    [Required]
    [StringLength(200)]
    public string ReportName { get; set; } = string.Empty;
    
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);
    
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
    
    public List<CanSystemType> IncludedSystems { get; set; } = new();
    
    public List<AnomalyLevel> IncludedAnomalyLevels { get; set; } = new();
    
    public bool IncludeResolvedDetections { get; set; } = true;
    
    public bool IncludeFalsePositives { get; set; } = false;
    
    public bool IncludeExecutiveSummary { get; set; } = true;
    
    public bool IncludeSystemSpecificReports { get; set; } = true;
    
    public bool IncludeCrossSystemAnalysis { get; set; } = true;
    
    public bool IncludeRecommendations { get; set; } = true;
    
    public bool IncludeTrendAnalysis { get; set; } = true;
    
    public bool IncludeDetailedCharts { get; set; } = true;
    
    public bool IncludeRawData { get; set; } = false;
    
    public ReportFormat Format { get; set; } = ReportFormat.Pdf;
    
    public ReportTemplate Template { get; set; } = ReportTemplate.Standard;
    
    [StringLength(1000)]
    public string AdditionalNotes { get; set; } = string.Empty;
}

public enum ReportFormat
{
    Pdf = 1,
    Excel = 2,
    Word = 3,
    Html = 4,
    Json = 5
}

public enum ReportTemplate
{
    Standard = 1,
    Executive = 2,
    Technical = 3,
    Compliance = 4,
    Custom = 5
}