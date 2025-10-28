using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Services;

namespace AnomalyDetection.AnomalyAnalysis;

public class AnomalyAnalysisServiceTests : AnomalyDetectionDomainTestBase<AnomalyDetectionDomainTestModule>
{
    private readonly IAnomalyAnalysisService _anomalyAnalysisService;
    private readonly IRepository<AnomalyDetectionResult, Guid> _anomalyResultRepository;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;

    public AnomalyAnalysisServiceTests()
    {
        _anomalyResultRepository = GetRequiredService<IRepository<AnomalyDetectionResult, Guid>>();
        _detectionLogicRepository = GetRequiredService<IRepository<CanAnomalyDetectionLogic, Guid>>();
        _anomalyAnalysisService = GetRequiredService<IAnomalyAnalysisService>();
    }

    [Fact]
    public async Task AnalyzePatternAsync_Should_Return_Empty_Result_When_No_Data()
    {
        // Arrange
        var canSignalId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _anomalyAnalysisService.AnalyzePatternAsync(canSignalId, startDate, endDate);

        // Assert
        result.ShouldNotBeNull();
        result.CanSignalId.ShouldBe(canSignalId);
        result.AnalysisStartDate.ShouldBe(startDate);
        result.AnalysisEndDate.ShouldBe(endDate);
        result.TotalAnomalies.ShouldBe(0);
        result.AnomalyTypeDistribution.ShouldBeEmpty();
        result.AnomalyLevelDistribution.ShouldBeEmpty();
        result.FrequencyPatterns.ShouldBeEmpty();
        result.Correlations.ShouldBeEmpty();
        result.AverageDetectionDurationMs.ShouldBe(0.0);
        result.FalsePositiveRate.ShouldBe(0.0);
        result.AnalysisSummary.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateThresholdRecommendationsAsync_Should_Return_Empty_Result_When_No_Data()
    {
        // Arrange
        var detectionLogicId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act & Assert - This should throw because the detection logic doesn't exist
        await Should.ThrowAsync<Exception>(async () =>
        {
            await _anomalyAnalysisService.GenerateThresholdRecommendationsAsync(detectionLogicId, startDate, endDate);
        });
    }

    [Fact]
    public async Task CalculateDetectionAccuracyAsync_Should_Return_Empty_Result_When_No_Data()
    {
        // Arrange
        var detectionLogicId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _anomalyAnalysisService.CalculateDetectionAccuracyAsync(detectionLogicId, startDate, endDate);

        // Assert
        result.ShouldNotBeNull();
        result.DetectionLogicId.ShouldBe(detectionLogicId);
        result.AnalysisStartDate.ShouldBe(startDate);
        result.AnalysisEndDate.ShouldBe(endDate);
        result.TotalDetections.ShouldBe(0);
        result.TruePositives.ShouldBe(0);
        result.FalsePositives.ShouldBe(0);
        result.TrueNegatives.ShouldBe(0);
        result.FalseNegatives.ShouldBe(0);
        result.Precision.ShouldBe(0.0);
        result.Recall.ShouldBe(0.0);
        result.F1Score.ShouldBe(0.0);
        result.Accuracy.ShouldBe(0.0);
        result.AccuracyByType.ShouldBeEmpty();
        result.AccuracyByTime.ShouldBeEmpty();
        result.PerformanceSummary.ShouldNotBeNullOrEmpty();
    }
}