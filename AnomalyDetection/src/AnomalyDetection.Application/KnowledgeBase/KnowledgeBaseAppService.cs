using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Knowledge base application service implementation
/// </summary>
public class KnowledgeBaseAppService : ApplicationService, IKnowledgeBaseAppService
{
    private readonly IRepository<KnowledgeArticle, Guid> _knowledgeArticleRepository;
    private readonly IRepository<KnowledgeArticleComment, Guid> _commentRepository;
    private readonly IKnowledgeArticleRecommendationService _recommendationService;

    public KnowledgeBaseAppService(
        IRepository<KnowledgeArticle, Guid> knowledgeArticleRepository,
        IRepository<KnowledgeArticleComment, Guid> commentRepository,
        IKnowledgeArticleRecommendationService recommendationService)
    {
        _knowledgeArticleRepository = knowledgeArticleRepository;
        _commentRepository = commentRepository;
        _recommendationService = recommendationService;
    }

    public async Task<KnowledgeArticleDto> GetAsync(Guid id)
    {
        var article = await _knowledgeArticleRepository.GetAsync(id);
        article.IncrementViewCount();
        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);

        var dto = ObjectMapper.Map<KnowledgeArticle, KnowledgeArticleDto>(article);

        var commentQueryable = await _commentRepository.GetQueryableAsync();
        var comments = await AsyncExecuter.ToListAsync(
            commentQueryable
                .Where(x => x.KnowledgeArticleId == id)
                .OrderByDescending(x => x.CreationTime));

        dto.Comments = ObjectMapper.Map<List<KnowledgeArticleComment>, List<KnowledgeArticleCommentDto>>(comments);

        return dto;
    }

    public async Task<PagedResultDto<KnowledgeArticleDto>> GetListAsync(GetKnowledgeArticlesInput input)
    {
        var queryable = await _knowledgeArticleRepository.GetQueryableAsync();

        if (input.Category.HasValue)
        {
            queryable = queryable.Where(x => (int)x.Category == input.Category.Value);
        }

        if (input.IsPublished.HasValue)
        {
            queryable = queryable.Where(x => x.IsPublished == input.IsPublished.Value);
        }

        if (input.Tags != null && input.Tags.Any())
        {
            var normalizedTags = NormalizeTags(input.Tags).Select(tag => tag.ToLowerInvariant()).ToList();
            if (normalizedTags.Count > 0)
            {
                queryable = queryable.Where(x => x.Tags.Any(t => normalizedTags.Contains(t.ToLower())));
            }
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable
            .OrderBy(input.Sorting ?? "CreationTime desc")
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var articles = await AsyncExecuter.ToListAsync(queryable);

        return new PagedResultDto<KnowledgeArticleDto>(
            totalCount,
            ObjectMapper.Map<List<KnowledgeArticle>, List<KnowledgeArticleDto>>(articles)
        );
    }

    public async Task<PagedResultDto<KnowledgeArticleDto>> SearchAsync(SearchKnowledgeArticlesInput input)
    {
        var queryable = await _knowledgeArticleRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.ToLower();
            queryable = queryable.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                x.Summary.ToLower().Contains(keyword) ||
                x.Content.ToLower().Contains(keyword) ||
                x.Symptom.ToLower().Contains(keyword) ||
                x.Cause.ToLower().Contains(keyword) ||
                x.Countermeasure.ToLower().Contains(keyword) ||
                x.Tags.Any(t => t.ToLower().Contains(keyword))
            );
        }

        if (input.Category.HasValue)
        {
            queryable = queryable.Where(x => (int)x.Category == input.Category.Value);
        }

        if (input.Tags != null && input.Tags.Any())
        {
            var normalizedTags = NormalizeTags(input.Tags).Select(tag => tag.ToLowerInvariant()).ToList();
            if (normalizedTags.Count > 0)
            {
                queryable = queryable.Where(x => x.Tags.Any(t => normalizedTags.Contains(t.ToLower())));
            }
        }

        queryable = queryable.Where(x => x.IsPublished);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        queryable = queryable
            .OrderByDescending(x => x.UsefulCount)
            .ThenByDescending(x => x.ViewCount)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var articles = await AsyncExecuter.ToListAsync(queryable);

        return new PagedResultDto<KnowledgeArticleDto>(
            totalCount,
            ObjectMapper.Map<List<KnowledgeArticle>, List<KnowledgeArticleDto>>(articles)
        );
    }

    public async Task<KnowledgeArticleDto> CreateAsync(CreateKnowledgeArticleDto input)
    {
        var normalizedTags = NormalizeTags(input.Tags);

        var article = new KnowledgeArticle(
            GuidGenerator.Create(),
            input.Title,
            input.Content,
            (KnowledgeCategory)input.Category)
        {
            Summary = input.Summary,
            Tags = normalizedTags,
            RelatedAnomalyId = input.RelatedAnomalyId,
            DetectionLogicId = input.DetectionLogicId,
            CanSignalId = input.CanSignalId,
            AnomalyType = input.AnomalyType,
            SignalName = input.SignalName,
            Symptom = input.Symptom,
            Cause = input.Cause,
            Countermeasure = input.Countermeasure,
            HasSolution = input.HasSolution,
            SolutionSteps = input.SolutionSteps,
            PreventionMeasures = input.PreventionMeasures
        };

        await _knowledgeArticleRepository.InsertAsync(article, autoSave: true);

        return ObjectMapper.Map<KnowledgeArticle, KnowledgeArticleDto>(article);
    }

    public async Task<KnowledgeArticleDto> UpdateAsync(Guid id, UpdateKnowledgeArticleDto input)
    {
        var article = await _knowledgeArticleRepository.GetAsync(id);
        var normalizedTags = NormalizeTags(input.Tags);

        article.Title = input.Title;
        article.Content = input.Content;
        article.Summary = input.Summary;
        article.Category = (KnowledgeCategory)input.Category;
        article.Tags = normalizedTags;
        article.RelatedAnomalyId = input.RelatedAnomalyId;
        article.DetectionLogicId = input.DetectionLogicId;
        article.CanSignalId = input.CanSignalId;
        article.AnomalyType = input.AnomalyType;
        article.SignalName = input.SignalName;
        article.Symptom = input.Symptom;
        article.Cause = input.Cause;
        article.Countermeasure = input.Countermeasure;
        article.HasSolution = input.HasSolution;
        article.SolutionSteps = input.SolutionSteps;
        article.PreventionMeasures = input.PreventionMeasures;

        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);

        return ObjectMapper.Map<KnowledgeArticle, KnowledgeArticleDto>(article);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _knowledgeArticleRepository.DeleteAsync(id);
    }

    public async Task PublishAsync(Guid id)
    {
        var article = await _knowledgeArticleRepository.GetAsync(id);
        article.Publish();
        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);
    }

    public async Task UnpublishAsync(Guid id)
    {
        var article = await _knowledgeArticleRepository.GetAsync(id);
        article.Unpublish();
        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);
    }

    public async Task MarkAsUsefulAsync(Guid id)
    {
        var article = await _knowledgeArticleRepository.GetAsync(id);
        article.IncrementUsefulCount();
        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);
    }

    public async Task<ListResultDto<KnowledgeArticleDto>> GetRelatedArticlesAsync(Guid anomalyId)
    {
        var queryable = await _knowledgeArticleRepository.GetQueryableAsync();

        var articles = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.IsPublished && x.RelatedAnomalyId == anomalyId)
                .OrderByDescending(x => x.UsefulCount)
                .Take(5));

        return new ListResultDto<KnowledgeArticleDto>(
            ObjectMapper.Map<List<KnowledgeArticle>, List<KnowledgeArticleDto>>(articles));
    }

    public async Task<ListResultDto<KnowledgeArticleDto>> GetSuggestedArticlesAsync(List<string> tags)
    {
        var queryable = await _knowledgeArticleRepository.GetQueryableAsync();
        var normalizedTags = NormalizeTags(tags).Select(tag => tag.ToLowerInvariant()).ToList();

        if (normalizedTags.Count == 0)
        {
            return new ListResultDto<KnowledgeArticleDto>(new List<KnowledgeArticleDto>());
        }

        var articles = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.IsPublished && x.Tags.Any(t => normalizedTags.Contains(t.ToLower())))
                .OrderByDescending(x => x.UsefulCount)
                .ThenByDescending(x => x.ViewCount)
                .Take(10));

        return new ListResultDto<KnowledgeArticleDto>(
            ObjectMapper.Map<List<KnowledgeArticle>, List<KnowledgeArticleDto>>(articles));
    }

    public async Task<ListResultDto<KnowledgeArticleDto>> GetPopularArticlesAsync(int count = 10)
    {
        var queryable = await _knowledgeArticleRepository.GetQueryableAsync();

        var articles = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.UsefulCount)
                .ThenByDescending(x => x.ViewCount)
                .Take(count));

        return new ListResultDto<KnowledgeArticleDto>(
            ObjectMapper.Map<List<KnowledgeArticle>, List<KnowledgeArticleDto>>(articles));
    }

    public async Task<KnowledgeBaseStatisticsDto> GetStatisticsAsync()
    {
        var articles = await _knowledgeArticleRepository.GetListAsync();
        var commentsCount = await _commentRepository.CountAsync();

        var totalArticles = articles.Count;
        var publishedArticles = articles.Count(x => x.IsPublished);
        var draftArticles = totalArticles - publishedArticles;

        var totalRatings = articles.Sum(x => x.RatingCount);
        var averageRating = totalRatings > 0
            ? Math.Round(articles.Sum(x => x.AverageRating * x.RatingCount) / totalRatings, 2)
            : 0;

        var topTags = articles
            .SelectMany(x => x.Tags)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .GroupBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .Take(5)
            .Select(group => group.Key)
            .ToList();

        var popularArticles = articles
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.UsefulCount)
            .ThenByDescending(x => x.ViewCount)
            .Take(5)
            .Select(article => new KnowledgeArticleSummaryDto
            {
                Id = article.Id,
                Title = article.Title,
                Summary = article.Summary,
                UsefulCount = article.UsefulCount,
                AverageRating = article.AverageRating,
                RelevanceScore = 0
            })
            .ToList();

        return new KnowledgeBaseStatisticsDto
        {
            TotalArticles = totalArticles,
            PublishedArticles = publishedArticles,
            DraftArticles = draftArticles,
            TotalComments = commentsCount,
            AverageRating = averageRating,
            TopTags = topTags,
            PopularArticles = popularArticles
        };
    }

    public async Task<KnowledgeArticleCommentDto> AddCommentAsync(Guid articleId, CreateKnowledgeArticleCommentDto input)
    {
        var article = await _knowledgeArticleRepository.GetAsync(articleId);
        var comment = new KnowledgeArticleComment(
            GuidGenerator.Create(),
            articleId,
            input.Content,
            input.Rating,
            input.AuthorUserId,
            input.AuthorName);

        await _commentRepository.InsertAsync(comment, autoSave: true);
        article.ApplyRating(comment.Rating);
        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);

        return ObjectMapper.Map<KnowledgeArticleComment, KnowledgeArticleCommentDto>(comment);
    }

    public async Task<KnowledgeArticleCommentDto> UpdateCommentAsync(Guid commentId, UpdateKnowledgeArticleCommentDto input)
    {
        var comment = await _commentRepository.GetAsync(commentId);
        var article = await _knowledgeArticleRepository.GetAsync(comment.KnowledgeArticleId);

        var oldRating = comment.Rating;
        comment.UpdateContent(input.Content);
        var newRating = comment.UpdateRating(input.Rating);

        if (oldRating != newRating)
        {
            article.UpdateRating(oldRating, newRating);
            await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);
        }

        await _commentRepository.UpdateAsync(comment, autoSave: true);

        return ObjectMapper.Map<KnowledgeArticleComment, KnowledgeArticleCommentDto>(comment);
    }

    public async Task DeleteCommentAsync(Guid commentId)
    {
        var comment = await _commentRepository.GetAsync(commentId);
        var article = await _knowledgeArticleRepository.GetAsync(comment.KnowledgeArticleId);

        article.RemoveRating(comment.Rating);
        await _knowledgeArticleRepository.UpdateAsync(article, autoSave: true);
        await _commentRepository.DeleteAsync(commentId, autoSave: true);
    }

    public async Task<ListResultDto<KnowledgeArticleCommentDto>> GetCommentsAsync(Guid articleId)
    {
        var queryable = await _commentRepository.GetQueryableAsync();
        var comments = await AsyncExecuter.ToListAsync(
            queryable
                .Where(x => x.KnowledgeArticleId == articleId)
                .OrderByDescending(x => x.CreationTime));

        return new ListResultDto<KnowledgeArticleCommentDto>(
            ObjectMapper.Map<List<KnowledgeArticleComment>, List<KnowledgeArticleCommentDto>>(comments));
    }

    public async Task<ListResultDto<KnowledgeArticleSummaryDto>> GetRecommendationsAsync(KnowledgeBaseRecommendationInput input)
    {
        var context = new KnowledgeArticleRecommendationContext
        {
            DetectionLogicId = input.DetectionLogicId,
            CanSignalId = input.CanSignalId,
            AnomalyType = input.AnomalyType,
            SignalName = input.SignalName,
            Tags = NormalizeTags(input.Tags),
            MaxResults = input.MaxResults
        };

        var results = await _recommendationService.GetRecommendationsAsync(context);

        var summaries = results
            .Select(result => new KnowledgeArticleSummaryDto
            {
                Id = result.Article.Id,
                Title = result.Article.Title,
                Summary = result.Article.Summary,
                UsefulCount = result.Article.UsefulCount,
                AverageRating = result.Article.AverageRating,
                RelevanceScore = Math.Round(result.Score, 2)
            })
            .ToList();

        return new ListResultDto<KnowledgeArticleSummaryDto>(summaries);
    }

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        return tags == null
            ? new List<string>()
            : tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
