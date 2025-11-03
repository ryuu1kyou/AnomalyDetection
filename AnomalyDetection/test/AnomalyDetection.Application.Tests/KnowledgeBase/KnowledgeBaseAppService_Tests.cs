using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.KnowledgeBase;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace AnomalyDetection.KnowledgeBase;

public class KnowledgeBaseAppService_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly IKnowledgeBaseAppService _knowledgeBaseAppService;
    private readonly IAnomalyDetectionResultAppService _anomalyDetectionResultAppService;

    public KnowledgeBaseAppService_Tests()
    {
        _knowledgeBaseAppService = GetRequiredService<IKnowledgeBaseAppService>();
        _anomalyDetectionResultAppService = GetRequiredService<IAnomalyDetectionResultAppService>();
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
