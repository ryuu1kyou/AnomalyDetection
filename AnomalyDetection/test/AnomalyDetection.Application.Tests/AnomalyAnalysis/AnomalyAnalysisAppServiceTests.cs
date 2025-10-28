using System;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.AnomalyDetection.Dtos;
using Shouldly;
using Xunit;

namespace AnomalyDetection.AnomalyAnalysis;

public class AnomalyAnalysisAppServiceTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly IAnomalyAnalysisAppService _anomalyAnalysisAppService;

    public AnomalyAnalysisAppServiceTests()
    {
        _anomalyAnalysisAppService = GetRequiredService<IAnomalyAnalysisAppService>();
    }

    [Fact]
    public async Task AnalyzeAnomalyPatternAsync_Should_Return_Valid_Result()
    {
        // Arrange
        var canSignalId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _anomalyAnalysisAppService.AnalyzeAnomalyPatternAsync(canSignalId, startDate, endDate);

        // Assert
        result.ShouldNotBeNull();
        result.CanSignalId.ShouldBe(canSignalId);
        result.AnalysisStartDate.ShouldBe(startDate);
        result.AnalysisEndDate.ShouldBe(endDate);
        result.TotalAnomalies.ShouldBeGreaterThanOrEqualTo(0);
        result.AnomalyTypeDistribution.ShouldNotBeNull();
        result.AnomalyLevelDistribution.ShouldNotBeNull();
        result.FrequencyPatterns.ShouldNotBeNull();
        result.Correlations.ShouldNotBeNull();
        result.AverageDetectionDurationMs.ShouldBeGreaterThanOrEqualTo(0);
        result.FalsePositiveRate.ShouldBeGreaterThanOrEqualTo(0);
        result.FalsePositiveRate.ShouldBeLessThanOrEqualTo(1);
        result.AnalysisSummary.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task AnalyzeAnomalyPatternAsync_With_Request_Should_Return_Valid_Result()
    {
        // Arrange
        var request = new AnomalyAnalysisRequestDto
        {
            CanSignalId = Guid.NewGuid(),
            AnalysisStartDate = DateTime.UtcNow.AddDays(-7),
            AnalysisEndDate = DateTime.UtcNow
        };

        // Act
        var result = await _anomalyAnalysisAppService.AnalyzeAnomalyPatternAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.CanSignalId.ShouldBe(request.CanSignalId);
        result.AnalysisStartDate.ShouldBe(request.AnalysisStartDate);
        result.AnalysisEndDate.ShouldBe(request.AnalysisEndDate);
    }

    [Fact]
    public async Task GetDetectionAccuracyMetricsAsync_Should_Return_Valid_Result()
    {
        // Arrange
        var detectionLogicId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _anomalyAnalysisAppService.GetDetectionAccuracyMetricsAsync(detectionLogicId, startDate, endDate);

        // Assert
        result.ShouldNotBeNull();
        result.DetectionLogicId.ShouldBe(detectionLogicId);
        result.AnalysisStartDate.ShouldBe(startDate);
        result.AnalysisEndDate.ShouldBe(endDate);
        result.TotalDetections.ShouldBeGreaterThanOrEqualTo(0);
        result.TruePositives.ShouldBeGreaterThanOrEqualTo(0);
        result.FalsePositives.ShouldBeGreaterThanOrEqualTo(0);
        result.TrueNegatives.ShouldBeGreaterThanOrEqualTo(0);
        result.FalseNegatives.ShouldBeGreaterThanOrEqualTo(0);
        result.Precision.ShouldBeGreaterThanOrEqualTo(0);
        result.Precision.ShouldBeLessThanOrEqualTo(1);
        result.Recall.ShouldBeGreaterThanOrEqualTo(0);
        result.Recall.ShouldBeLessThanOrEqualTo(1);
        result.F1Score.ShouldBeGreaterThanOrEqualTo(0);
        result.F1Score.ShouldBeLessThanOrEqualTo(1);
        result.Accuracy.ShouldBeGreaterThanOrEqualTo(0);
        result.Accuracy.ShouldBeLessThanOrEqualTo(1);
        result.AccuracyByType.ShouldNotBeNull();
        result.AccuracyByTime.ShouldNotBeNull();
        result.PerformanceSummary.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetDetectionAccuracyMetricsAsync_With_Request_Should_Return_Valid_Result()
    {
        // Arrange
        var request = new DetectionAccuracyRequestDto
        {
            DetectionLogicId = Guid.NewGuid(),
            AnalysisStartDate = DateTime.UtcNow.AddDays(-7),
            AnalysisEndDate = DateTime.UtcNow
        };

        // Act
        var result = await _anomalyAnalysisAppService.GetDetectionAccuracyMetricsAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.DetectionLogicId.ShouldBe(request.DetectionLogicId);
        result.AnalysisStartDate.ShouldBe(request.AnalysisStartDate);
        result.AnalysisEndDate.ShouldBe(request.AnalysisEndDate);
    }

    [Fact]
    public async Task GetThresholdRecommendationsAsync_Should_Handle_NonExistent_Logic()
    {
        // Arrange
        var detectionLogicId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act & Assert - This should not throw but return empty recommendations
        var result = await _anomalyAnalysisAppService.GetThresholdRecommendationsAsync(detectionLogicId, startDate, endDate);
        
        // The service should handle non-existent logic gracefully
        // This might throw an exception or return empty results depending on implementation
        // We'll test that it doesn't crash the application
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetThresholdRecommendationsAsync_With_Request_Should_Handle_NonExistent_Logic()
    {
        // Arrange
        var request = new ThresholdRecommendationRequestDto
        {
            DetectionLogicId = Guid.NewGuid(),
            AnalysisStartDate = DateTime.UtcNow.AddDays(-7),
            AnalysisEndDate = DateTime.UtcNow
        };

        // Act & Assert
        var result = await _anomalyAnalysisAppService.GetThresholdRecommendationsAsync(request);
        
        result.ShouldNotBeNull();
        result.DetectionLogicId.ShouldBe(request.DetectionLogicId);
        result.AnalysisStartDate.ShouldBe(request.AnalysisStartDate);
        result.AnalysisEndDate.ShouldBe(request.AnalysisEndDate);
    }

    [Fact]
    public async Task Analysis_Methods_Should_Handle_Invalid_Date_Range()
    {
        // Arrange
        var canSignalId = Guid.NewGuid();
        var detectionLogicId = Guid.NewGuid();
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1); // End date before start date

        // Act & Assert - Should handle invalid date ranges gracefully
        var patternResult = await _anomalyAnalysisAppService.AnalyzeAnomalyPatternAsync(canSignalId, startDate, endDate);
        patternResult.ShouldNotBeNull();

        var accuracyResult = await _anomalyAnalysisAppService.GetDetectionAccuracyMetricsAsync(detectionLogicId, startDate, endDate);
        accuracyResult.ShouldNotBeNull();
    }

    [Fact]
    public async Task Analysis_Methods_Should_Handle_Empty_Guid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act & Assert - Should handle empty GUIDs gracefully
        var patternResult = await _anomalyAnalysisAppService.AnalyzeAnomalyPatternAsync(emptyGuid, startDate, endDate);
        patternResult.ShouldNotBeNull();

        var accuracyResult = await _anomalyAnalysisAppService.GetDetectionAccuracyMetricsAsync(emptyGuid, startDate, endDate);
        accuracyResult.ShouldNotBeNull();
    }
}