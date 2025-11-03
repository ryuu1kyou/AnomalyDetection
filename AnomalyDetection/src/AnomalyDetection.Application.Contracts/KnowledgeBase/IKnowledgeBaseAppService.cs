using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Application service for knowledge base management
/// </summary>
public interface IKnowledgeBaseAppService : IApplicationService
{
    /// <summary>
    /// Get knowledge article by ID
    /// </summary>
    Task<KnowledgeArticleDto> GetAsync(Guid id);

    /// <summary>
    /// Get paginated list of articles
    /// </summary>
    Task<PagedResultDto<KnowledgeArticleDto>> GetListAsync(GetKnowledgeArticlesInput input);

    /// <summary>
    /// Search knowledge articles
    /// </summary>
    Task<PagedResultDto<KnowledgeArticleDto>> SearchAsync(SearchKnowledgeArticlesInput input);

    /// <summary>
    /// Create new article
    /// </summary>
    Task<KnowledgeArticleDto> CreateAsync(CreateKnowledgeArticleDto input);

    /// <summary>
    /// Update existing article
    /// </summary>
    Task<KnowledgeArticleDto> UpdateAsync(Guid id, UpdateKnowledgeArticleDto input);

    /// <summary>
    /// Delete article
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Publish article
    /// </summary>
    Task PublishAsync(Guid id);

    /// <summary>
    /// Unpublish article
    /// </summary>
    Task UnpublishAsync(Guid id);

    /// <summary>
    /// Mark article as useful
    /// </summary>
    Task MarkAsUsefulAsync(Guid id);

    /// <summary>
    /// Get related articles for an anomaly
    /// </summary>
    Task<ListResultDto<KnowledgeArticleDto>> GetRelatedArticlesAsync(Guid anomalyId);

    /// <summary>
    /// Get suggested articles based on tags
    /// </summary>
    Task<ListResultDto<KnowledgeArticleDto>> GetSuggestedArticlesAsync(List<string> tags);

    /// <summary>
    /// Get popular articles
    /// </summary>
    Task<ListResultDto<KnowledgeArticleDto>> GetPopularArticlesAsync(int count = 10);

    /// <summary>
    /// Get aggregated knowledge base statistics
    /// </summary>
    Task<KnowledgeBaseStatisticsDto> GetStatisticsAsync();

    /// <summary>
    /// Add comment/feedback to an article
    /// </summary>
    Task<KnowledgeArticleCommentDto> AddCommentAsync(Guid articleId, CreateKnowledgeArticleCommentDto input);

    /// <summary>
    /// Update an existing comment
    /// </summary>
    Task<KnowledgeArticleCommentDto> UpdateCommentAsync(Guid commentId, UpdateKnowledgeArticleCommentDto input);

    /// <summary>
    /// Delete a comment from an article
    /// </summary>
    Task DeleteCommentAsync(Guid commentId);

    /// <summary>
    /// Get comments for a knowledge article
    /// </summary>
    Task<ListResultDto<KnowledgeArticleCommentDto>> GetCommentsAsync(Guid articleId);

    /// <summary>
    /// Get recommended articles for a detection context
    /// </summary>
    Task<ListResultDto<KnowledgeArticleSummaryDto>> GetRecommendationsAsync(KnowledgeBaseRecommendationInput input);
}

/// <summary>
/// Knowledge article DTO
/// </summary>
public class KnowledgeArticleDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public int ViewCount { get; set; }
    public int UsefulCount { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid? RelatedAnomalyId { get; set; }
    public Guid? DetectionLogicId { get; set; }
    public Guid? CanSignalId { get; set; }
    public string? AnomalyType { get; set; }
    public string? SignalName { get; set; }
    public string Symptom { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public string Countermeasure { get; set; } = string.Empty;
    public bool HasSolution { get; set; }
    public string? SolutionSteps { get; set; }
    public string? PreventionMeasures { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<KnowledgeArticleCommentDto> Comments { get; set; } = new();
}

/// <summary>
/// Get knowledge articles input
/// </summary>
public class GetKnowledgeArticlesInput : PagedAndSortedResultRequestDto
{
    public int? Category { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPublished { get; set; }
}

/// <summary>
/// Search knowledge articles input
/// </summary>
public class SearchKnowledgeArticlesInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; } = string.Empty;
    public int? Category { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Create knowledge article DTO
/// </summary>
public class CreateKnowledgeArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public Guid? RelatedAnomalyId { get; set; }
    public Guid? DetectionLogicId { get; set; }
    public Guid? CanSignalId { get; set; }
    public string? AnomalyType { get; set; }
    public string? SignalName { get; set; }
    public string Symptom { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public string Countermeasure { get; set; } = string.Empty;
    public bool HasSolution { get; set; }
    public string? SolutionSteps { get; set; }
    public string? PreventionMeasures { get; set; }
}

/// <summary>
/// Update knowledge article DTO
/// </summary>
public class UpdateKnowledgeArticleDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public Guid? RelatedAnomalyId { get; set; }
    public Guid? DetectionLogicId { get; set; }
    public Guid? CanSignalId { get; set; }
    public string? AnomalyType { get; set; }
    public string? SignalName { get; set; }
    public string Symptom { get; set; } = string.Empty;
    public string Cause { get; set; } = string.Empty;
    public string Countermeasure { get; set; } = string.Empty;
    public bool HasSolution { get; set; }
    public string? SolutionSteps { get; set; }
    public string? PreventionMeasures { get; set; }
}

/// <summary>
/// Knowledge article comment DTO
/// </summary>
public class KnowledgeArticleCommentDto : FullAuditedEntityDto<Guid>
{
    public Guid KnowledgeArticleId { get; set; }
    public Guid? AuthorUserId { get; set; }
    public string? AuthorName { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
}

public class CreateKnowledgeArticleCommentDto
{
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public Guid? AuthorUserId { get; set; }
    public string? AuthorName { get; set; }
}

public class UpdateKnowledgeArticleCommentDto
{
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
}

public class KnowledgeBaseStatisticsDto
{
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public int TotalComments { get; set; }
    public double AverageRating { get; set; }
    public List<string> TopTags { get; set; } = new();
    public List<KnowledgeArticleSummaryDto> PopularArticles { get; set; } = new();
}

public class KnowledgeBaseRecommendationInput
{
    public Guid? DetectionLogicId { get; set; }
    public Guid? CanSignalId { get; set; }
    public string? AnomalyType { get; set; }
    public string? SignalName { get; set; }
    public List<string> Tags { get; set; } = new();
    public int MaxResults { get; set; } = 5;
}

public class KnowledgeArticleSummaryDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int UsefulCount { get; set; }
    public double AverageRating { get; set; }
    public double RelevanceScore { get; set; }
}
