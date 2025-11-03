using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Default implementation of knowledge base article recommendation service.
/// </summary>
public class KnowledgeArticleRecommendationService : DomainService, IKnowledgeArticleRecommendationService
{
    private readonly IRepository<KnowledgeArticle, Guid> _articleRepository;

    public KnowledgeArticleRecommendationService(IRepository<KnowledgeArticle, Guid> articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public virtual async Task<List<KnowledgeArticleRecommendationResult>> GetRecommendationsAsync(KnowledgeArticleRecommendationContext context)
    {
        var queryable = await _articleRepository.GetQueryableAsync();
        queryable = queryable.Where(x => x.IsPublished);

        // Prefilter by detection logic or CAN signal when possible to reduce load
        if (context.DetectionLogicId.HasValue)
        {
            queryable = queryable.Where(x => x.DetectionLogicId == context.DetectionLogicId);
        }
        else if (context.CanSignalId.HasValue)
        {
            queryable = queryable.Where(x => x.CanSignalId == context.CanSignalId);
        }

        // Materialize a reasonable set for scoring
        var candidates = await AsyncExecuter.ToListAsync(
            queryable.Take(200));

        if (candidates.Count == 0)
        {
            return new List<KnowledgeArticleRecommendationResult>();
        }

        var normalizedTags = context.Tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim().ToLowerInvariant())
            .ToHashSet();

        var results = candidates
            .Select(article => new KnowledgeArticleRecommendationResult(article, CalculateScore(article, context, normalizedTags)))
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Article.UsefulCount)
            .ThenByDescending(result => result.Article.ViewCount)
            .Take(Math.Max(1, context.MaxResults))
            .ToList();

        return results;
    }

    private static double CalculateScore(
        KnowledgeArticle article,
        KnowledgeArticleRecommendationContext context,
        HashSet<string> tags)
    {
        double score = 0;

        if (context.DetectionLogicId.HasValue && article.DetectionLogicId == context.DetectionLogicId)
        {
            score += 60;
        }

        if (context.CanSignalId.HasValue && article.CanSignalId == context.CanSignalId)
        {
            score += 40;
        }

        if (!string.IsNullOrWhiteSpace(context.AnomalyType) &&
            !string.IsNullOrWhiteSpace(article.AnomalyType) &&
            string.Equals(article.AnomalyType, context.AnomalyType, StringComparison.OrdinalIgnoreCase))
        {
            score += 30;
        }

        if (!string.IsNullOrWhiteSpace(context.SignalName) &&
            !string.IsNullOrWhiteSpace(article.SignalName) &&
            string.Equals(article.SignalName, context.SignalName, StringComparison.OrdinalIgnoreCase))
        {
            score += 25;
        }

        if (tags.Count > 0 && article.Tags.Any())
        {
            var articleTags = article.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim().ToLowerInvariant())
                .ToHashSet();

            var matches = tags.Intersect(articleTags).Count();
            if (matches > 0)
            {
                score += matches * 10;
            }
        }

        // Boost by community feedback
        score += article.UsefulCount * 0.5;
        score += article.AverageRating * 2;

        return score;
    }
}
