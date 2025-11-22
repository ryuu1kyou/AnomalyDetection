using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using AnomalyDetection.Permissions;
using AnomalyDetection.KnowledgeBase;
using AnomalyDetection.Shared.Export;

namespace AnomalyDetection.KnowledgeBase;

/// <summary>
/// Provides aggregated knowledge base statistics with export capability.
/// </summary>
public class KnowledgeBaseStatisticsExportAppService : ApplicationService, IKnowledgeBaseStatisticsExportAppService
{
    private readonly IRepository<KnowledgeArticle, Guid> _articleRepository;
    private readonly IRepository<KnowledgeArticleComment, Guid> _commentRepository;
    private readonly ExportService _exportService;

    public KnowledgeBaseStatisticsExportAppService(
        IRepository<KnowledgeArticle, Guid> articleRepository,
        IRepository<KnowledgeArticleComment, Guid> commentRepository,
        ExportService exportService)
    {
        _articleRepository = articleRepository;
        _commentRepository = commentRepository;
        _exportService = exportService;
    }

    [Authorize(AnomalyDetectionPermissions.KnowledgeBase.Statistics.View)]
    public async Task<KnowledgeBaseStatisticsDto> GetStatisticsAsync()
    {
        var articles = await _articleRepository.GetListAsync();
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
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
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

    [Authorize(AnomalyDetectionPermissions.KnowledgeBase.Statistics.Export)]
    public async Task<ExportedFileDto> ExportAsync(KnowledgeBaseStatisticsExportDto input)
    {
        var stats = await GetStatisticsAsync();

        var rows = new List<object>
        {
            new { Metric = "TotalArticles", Value = stats.TotalArticles },
            new { Metric = "PublishedArticles", Value = stats.PublishedArticles },
            new { Metric = "DraftArticles", Value = stats.DraftArticles },
            new { Metric = "TotalComments", Value = stats.TotalComments },
            new { Metric = "AverageRating", Value = stats.AverageRating }
        };

        if (input.IncludeTopTags)
        {
            rows.Add(new { Metric = "TopTags", Value = string.Join(", ", stats.TopTags) });
        }
        if (input.IncludePopularArticles)
        {
            foreach (var p in stats.PopularArticles)
            {
                rows.Add(new { Metric = "PopularArticle", Value = p.Title, Useful = p.UsefulCount, AvgRating = p.AverageRating });
            }
        }

        var formatEnum = input.Format?.ToLower() switch
        {
            "json" => ExportService.ExportFormat.Json,
            "pdf" => ExportService.ExportFormat.Pdf,
            "excel" => ExportService.ExportFormat.Excel,
            _ => ExportService.ExportFormat.Csv
        };

        var request = new ExportDetectionRequest
        {
            Results = rows,
            Format = formatEnum,
            FileNamePrefix = "knowledge_base_stats",
            GeneratedBy = CurrentUser.UserName ?? "system",
            CsvOptions = new CsvExportOptions { IncludeHeader = true }
        };

        var result = await _exportService.ExportDetectionResultsAsync(request);
        return new ExportedFileDto
        {
            FileName = result.FileName,
            ContentType = result.ContentType,
            RecordCount = result.Metadata.RecordCount,
            Format = result.Metadata.Format,
            ExportedAt = result.Metadata.ExportedAt,
            FileData = result.Data
        };
    }
}