using System;
using System.Collections.Generic;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Context information used for knowledge article recommendation.
/// </summary>
public class KnowledgeArticleRecommendationContext
{
    public Guid? DetectionLogicId { get; set; }
    public Guid? CanSignalId { get; set; }
    public string? AnomalyType { get; set; }
    public string? SignalName { get; set; }
    public List<string> Tags { get; set; } = new();
    public int MaxResults { get; set; } = 5;
}
