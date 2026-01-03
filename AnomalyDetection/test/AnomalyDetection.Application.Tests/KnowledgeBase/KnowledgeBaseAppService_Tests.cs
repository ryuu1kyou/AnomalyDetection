using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.KnowledgeBase;
using NSubstitute;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace AnomalyDetection.KnowledgeBase;

public class KnowledgeBaseAppService_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly IKnowledgeBaseAppService _knowledgeBaseAppService;
    private readonly IAnomalyDetectionResultAppService _anomalyDetectionResultAppService;
    private readonly IRepository<KnowledgeArticle, Guid> _knowledgeArticleRepository;
    private readonly IRepository<KnowledgeArticleComment, Guid> _commentRepository;

    private readonly Dictionary<Guid, KnowledgeArticle> _articles = new();
    private readonly Dictionary<Guid, KnowledgeArticleComment> _comments = new();

    public KnowledgeBaseAppService_Tests()
    {
        _knowledgeBaseAppService = GetRequiredService<IKnowledgeBaseAppService>();
        _anomalyDetectionResultAppService = GetRequiredService<IAnomalyDetectionResultAppService>();
        _knowledgeArticleRepository = GetRequiredService<IRepository<KnowledgeArticle, Guid>>();
        _commentRepository = GetRequiredService<IRepository<KnowledgeArticleComment, Guid>>();

        SetupKnowledgeArticleRepository();
        SetupCommentRepository();
    }

    private void SetupKnowledgeArticleRepository()
    {
        _knowledgeArticleRepository.InsertAsync(Arg.Any<KnowledgeArticle>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var article = callInfo.Arg<KnowledgeArticle>();
                _articles[article.Id] = article;
                return Task.FromResult(article);
            });

        _knowledgeArticleRepository.UpdateAsync(Arg.Any<KnowledgeArticle>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var article = callInfo.Arg<KnowledgeArticle>();
                _articles[article.Id] = article;
                return Task.FromResult(article);
            });

        _knowledgeArticleRepository.GetAsync(Arg.Any<Guid>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_articles.TryGetValue(id, out var article))
                {
                    return Task.FromResult(article);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(KnowledgeArticle), id);
            });

        _knowledgeArticleRepository.GetAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_articles.TryGetValue(id, out var article))
                {
                    return Task.FromResult(article);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(KnowledgeArticle), id);
            });

        _knowledgeArticleRepository.GetListAsync(includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<KnowledgeArticle>>(_articles.Values.ToList()));

        _knowledgeArticleRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<KnowledgeArticle, bool>>>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<System.Linq.Expressions.Expression<Func<KnowledgeArticle, bool>>>().Compile();
                return Task.FromResult(_articles.Values.AsQueryable().Where(predicate).ToList());
            });

        _knowledgeArticleRepository.GetListAsync()
            .Returns(callInfo => Task.FromResult<List<KnowledgeArticle>>(_articles.Values.ToList()));
    }

    private void SetupCommentRepository()
    {
        _commentRepository.InsertAsync(Arg.Any<KnowledgeArticleComment>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var comment = callInfo.Arg<KnowledgeArticleComment>();
                _comments[comment.Id] = comment;
                return Task.FromResult(comment);
            });

        _commentRepository.CountAsync(Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(_comments.Count));

        _commentRepository.GetQueryableAsync()
            .Returns(callInfo => Task.FromResult(_comments.Values.AsQueryable()));
    }

    [Fact]
    public async Task CreateAsync_Should_Persist_Structured_Article()
    {
        // Arrange
        var detectionLogicId = Guid.NewGuid();
        var canSignalId = Guid.NewGuid();

        var input = new CreateKnowledgeArticleDto
        {
            Title = "Brake Pressure Spike",
            Content = "Detailed description of brake pressure anomalies.",
            Summary = "Brake pressure anomaly root cause and mitigation",
            Category = (int)KnowledgeCategory.Troubleshooting,
            Tags = new List<string> { "Brake", "brake", "Pressure " },
            DetectionLogicId = detectionLogicId,
            CanSignalId = canSignalId,
            Symptom = "Sudden brake pressure spikes",
            Cause = "Air in hydraulic line",
            Countermeasure = "Perform system bleed",
            HasSolution = true,
            SolutionSteps = "1. Inspect\n2. Bleed system",
            PreventionMeasures = "Regular maintenance"
        };

        // Act
        var article = await _knowledgeBaseAppService.CreateAsync(input);

        // Assert
        article.ShouldNotBeNull();
        article.Title.ShouldBe(input.Title);
        article.Tags.Count.ShouldBe(2); // duplicate removed, trimmed
        article.DetectionLogicId.ShouldBe(detectionLogicId);
        article.CanSignalId.ShouldBe(canSignalId);
        article.Symptom.ShouldBe(input.Symptom);
        article.HasSolution.ShouldBeTrue();
    }

    [Fact]
    public async Task AddCommentAsync_Should_Update_Rating()
    {
        // Arrange
        var article = await _knowledgeBaseAppService.CreateAsync(new CreateKnowledgeArticleDto
        {
            Title = "Engine Overheat",
            Content = "Engine overheating troubleshooting.",
            Summary = "Overheat analysis",
            Category = (int)KnowledgeCategory.BestPractice,
            Tags = new List<string> { "Engine" },
            HasSolution = true,
            SolutionSteps = "Inspect coolant",
            PreventionMeasures = "Regular coolant checks"
        });

        await _knowledgeBaseAppService.PublishAsync(article.Id);

        // Act
        var comment = await _knowledgeBaseAppService.AddCommentAsync(article.Id, new CreateKnowledgeArticleCommentDto
        {
            Content = "Very helpful article",
            Rating = 5,
            AuthorName = "QA Engineer"
        });

        var updated = await _knowledgeBaseAppService.GetAsync(article.Id);

        // Assert
        comment.Rating.ShouldBe(5);
        updated.RatingCount.ShouldBe(1);
        updated.AverageRating.ShouldBe(5);
        updated.Comments.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetRecommendationsAsync_Should_Return_Relevant_Articles()
    {
        // Arrange
        var detectionLogicId = Guid.NewGuid();
        var canSignalId = Guid.NewGuid();

        var article = await _knowledgeBaseAppService.CreateAsync(new CreateKnowledgeArticleDto
        {
            Title = "CAN Timeout Handling",
            Content = "Handling timeouts on CAN bus for steering system.",
            Summary = "Timeout mitigation",
            Category = (int)KnowledgeCategory.Troubleshooting,
            Tags = new List<string> { "Timeout", "CAN" },
            DetectionLogicId = detectionLogicId,
            CanSignalId = canSignalId,
            AnomalyType = "Timeout",
            HasSolution = true,
            SolutionSteps = "Adjust timeout threshold",
            PreventionMeasures = "Monitor bus load"
        });

        await _knowledgeBaseAppService.PublishAsync(article.Id);

        var detectionResult = await _anomalyDetectionResultAppService.CreateAsync(new CreateDetectionResultDto
        {
            DetectionLogicId = detectionLogicId,
            CanSignalId = canSignalId,
            AnomalyLevel = AnomalyLevel.Critical,
            ConfidenceScore = 0.92,
            Description = "Steering CAN timeout detected",
            SignalValue = 123.4,
            InputTimestamp = DateTime.UtcNow,
            DetectionType = DetectionType.Timeout,
            TriggerCondition = "timeout > threshold",
            ExecutionTimeMs = 12.5,
            SharingLevel = SharingLevel.Private,
            Tags = new List<string> { "timeout" }
        });

        // Assert
        detectionResult.RecommendedArticles.ShouldNotBeNull();
        detectionResult.RecommendedArticles.Count.ShouldBeGreaterThan(0);
        detectionResult.RecommendedArticles[0].Id.ShouldBe(article.Id);
        detectionResult.RecommendedArticles[0].RelevanceScore.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_Should_Return_Populated_Overview()
    {
        // Arrange
        var created = await _knowledgeBaseAppService.CreateAsync(new CreateKnowledgeArticleDto
        {
            Title = "Signal Noise Diagnosis",
            Content = "Diagnosing high noise on sensor signal.",
            Summary = "Noise analysis",
            Category = (int)KnowledgeCategory.KnownIssue,
            Tags = new List<string> { "Noise", "Sensor" },
            HasSolution = true,
            SolutionSteps = "Apply filter",
            PreventionMeasures = "Shield cabling"
        });

        await _knowledgeBaseAppService.PublishAsync(created.Id);
        await _knowledgeBaseAppService.AddCommentAsync(created.Id, new CreateKnowledgeArticleCommentDto
        {
            Content = "Solved our noise issue",
            Rating = 4
        });

        // Act
        var stats = await _knowledgeBaseAppService.GetStatisticsAsync();

        // Assert
        stats.TotalArticles.ShouldBeGreaterThanOrEqualTo(1);
        stats.PublishedArticles.ShouldBeGreaterThanOrEqualTo(1);
        stats.TotalComments.ShouldBeGreaterThanOrEqualTo(1);
        stats.PopularArticles.ShouldNotBeNull();
        stats.PopularArticles.Count.ShouldBeGreaterThan(0);
    }
}
