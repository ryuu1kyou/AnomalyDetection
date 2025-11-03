using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Knowledge base article for anomaly detection best practices and solutions
/// </summary>
public class KnowledgeArticle : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public KnowledgeCategory Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public int ViewCount { get; set; }
    public int UsefulCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Related anomaly information
    public Guid? RelatedAnomalyId { get; set; }
    public Guid? DetectionLogicId { get; set; }
    public Guid? CanSignalId { get; set; }
    public string? AnomalyType { get; set; }
    public string? SignalName { get; set; }

    // Structured knowledge fields
    public string Symptom { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public string Countermeasure { get; set; } = string.Empty;

    // Solution tracking
    public bool HasSolution { get; set; }
    public string? SolutionSteps { get; set; }
    public string? PreventionMeasures { get; set; }

    // Feedback statistics
    public double AverageRating { get; private set; }
    public int RatingCount { get; private set; }

    // Metadata
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Comments
    public List<KnowledgeArticleComment> Comments { get; private set; } = new();

    protected KnowledgeArticle()
    {
    }

    public KnowledgeArticle(
        Guid id,
        string title,
        string content,
        KnowledgeCategory category)
        : base(id)
    {
        Title = title;
        Content = content;
        Category = category;
        IsPublished = false;
    }

    public void Publish()
    {
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void IncrementUsefulCount()
    {
        UsefulCount++;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
        }
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    public void ApplyRating(int rating)
    {
        if (rating <= 0)
        {
            return;
        }

        var total = (AverageRating * RatingCount) + rating;
        RatingCount++;
        AverageRating = Math.Round(total / RatingCount, 2);
    }

    public void UpdateRating(int oldRating, int newRating)
    {
        if (RatingCount <= 0 || oldRating == newRating)
        {
            return;
        }

        var total = (AverageRating * RatingCount) - oldRating + newRating;
        AverageRating = RatingCount == 0 ? 0 : Math.Round(total / RatingCount, 2);
    }

    public void RemoveRating(int rating)
    {
        if (RatingCount <= 0 || rating <= 0)
        {
            return;
        }

        RatingCount--;

        if (RatingCount == 0)
        {
            AverageRating = 0;
            return;
        }

        var total = (AverageRating * (RatingCount + 1)) - rating;
        AverageRating = Math.Round(total / RatingCount, 2);
    }
}

/// <summary>
/// Knowledge category
/// </summary>
public enum KnowledgeCategory
{
    /// <summary>
    /// Best practices and guidelines
    /// </summary>
    BestPractice = 1,

    /// <summary>
    /// Troubleshooting guides
    /// </summary>
    Troubleshooting = 2,

    /// <summary>
    /// Known issues and solutions
    /// </summary>
    KnownIssue = 3,

    /// <summary>
    /// Configuration examples
    /// </summary>
    Configuration = 4,

    /// <summary>
    /// Case studies
    /// </summary>
    CaseStudy = 5,

    /// <summary>
    /// Technical documentation
    /// </summary>
    Documentation = 6,

    /// <summary>
    /// FAQ
    /// </summary>
    Faq = 7
}
