using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.SimilarPatternSearch;
using AnomalyDetection.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace AnomalyDetection.Domain.Tests.SimilarPatternSearch;

public class SimilarPatternSearchServiceTests
{
    private readonly ISimilarPatternSearchService _similarPatternSearchService;

    public SimilarPatternSearchServiceTests()
    {
        var logger = NullLogger<SimilarPatternSearchService>.Instance;
        _similarPatternSearchService = new SimilarPatternSearchService(logger);
    }

    [Fact]
    public async Task SearchSimilarSignalsAsync_Should_Find_Similar_Signals()
    {
        // Arrange
        var targetSignal = CreateTestCanSignal("ENGINE_RPM", "0x123", CanSystemType.Engine);
        var candidateSignals = new List<CanSignal>
        {
            CreateTestCanSignal("ENGINE_SPEED", "0x124", CanSystemType.Engine), // 類似
            CreateTestCanSignal("ENGINE_TEMP", "0x125", CanSystemType.Engine),  // 部分的に類似
            CreateTestCanSignal("BRAKE_PRESSURE", "0x200", CanSystemType.Brake) // 非類似
        };

        var criteria = SimilaritySearchCriteria.CreateDefault();

        // Act
        var results = await _similarPatternSearchService.SearchSimilarSignalsAsync(
            targetSignal, criteria, candidateSignals);

        // Assert
        results.ShouldNotBeNull();
        var resultsList = results.ToList();
        resultsList.Count.ShouldBeGreaterThan(0);
        
        // ENGINE_SPEEDが最も類似度が高いはず
        var topResult = resultsList.OrderByDescending(r => r.SimilarityScore).First();
        topResult.SignalId.ShouldBe(candidateSignals[0].Id); // ENGINE_SPEED
        topResult.SimilarityScore.ShouldBeGreaterThan(0.5);
    }

    [Fact]
    public async Task CompareTestDataAsync_Should_Compare_Detection_Results()
    {
        // Arrange
        var sourceSignal = CreateTestCanSignal("ENGINE_RPM", "0x123", CanSystemType.Engine);
        var targetSignal = CreateTestCanSignal("ENGINE_SPEED", "0x124", CanSystemType.Engine);

        var sourceResults = new List<AnomalyDetectionResult>
        {
            CreateTestAnomalyDetectionResult(sourceSignal.Id, AnomalyLevel.Warning),
            CreateTestAnomalyDetectionResult(sourceSignal.Id, AnomalyLevel.Error)
        };

        var targetResults = new List<AnomalyDetectionResult>
        {
            CreateTestAnomalyDetectionResult(targetSignal.Id, AnomalyLevel.Warning),
            CreateTestAnomalyDetectionResult(targetSignal.Id, AnomalyLevel.Critical)
        };

        // Act
        var comparison = await _similarPatternSearchService.CompareTestDataAsync(
            sourceResults, targetResults, sourceSignal, targetSignal);

        // Assert
        comparison.ShouldNotBeNull();
        comparison.SourceSignalId.ShouldBe(sourceSignal.Id);
        comparison.TargetSignalId.ShouldBe(targetSignal.Id);
        comparison.OverallSimilarityScore.ShouldBeGreaterThanOrEqualTo(0.0);
        comparison.OverallSimilarityScore.ShouldBeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void CalculateSimilarity_Should_Return_High_Score_For_Similar_Signals()
    {
        // Arrange
        var signal1 = CreateTestCanSignal("ENGINE_RPM", "0x123", CanSystemType.Engine);
        var signal2 = CreateTestCanSignal("ENGINE_SPEED", "0x123", CanSystemType.Engine); // 同じCAN ID
        var criteria = SimilaritySearchCriteria.CreateDefault();

        // Act
        var similarity = _similarPatternSearchService.CalculateSimilarity(signal1, signal2, criteria);

        // Assert
        similarity.ShouldBeGreaterThan(0.7); // 高い類似度を期待
    }

    [Fact]
    public void CalculateSimilarity_Should_Return_Low_Score_For_Different_Signals()
    {
        // Arrange
        var signal1 = CreateTestCanSignal("ENGINE_RPM", "0x123", CanSystemType.Engine);
        var signal2 = CreateTestCanSignal("BRAKE_PRESSURE", "0x200", CanSystemType.Brake);
        var criteria = SimilaritySearchCriteria.CreateDefault();

        // Act
        var similarity = _similarPatternSearchService.CalculateSimilarity(signal1, signal2, criteria);

        // Assert
        similarity.ShouldBeLessThan(0.5); // 低い類似度を期待
    }

    [Fact]
    public void CalculateSimilarityBreakdown_Should_Provide_Detailed_Scores()
    {
        // Arrange
        var signal1 = CreateTestCanSignal("ENGINE_RPM", "0x123", CanSystemType.Engine);
        var signal2 = CreateTestCanSignal("ENGINE_SPEED", "0x124", CanSystemType.Engine);
        var criteria = SimilaritySearchCriteria.CreateDefault();

        // Act
        var breakdown = _similarPatternSearchService.CalculateSimilarityBreakdown(signal1, signal2, criteria);

        // Assert
        breakdown.ShouldNotBeNull();
        breakdown.SystemTypeSimilarity.ShouldBe(1.0); // 同じシステム種別
        breakdown.SignalNameSimilarity.ShouldBeGreaterThan(0.5); // 部分的に類似した名前
        breakdown.CanIdSimilarity.ShouldBe(0.0); // 異なるCAN ID
    }

    [Fact]
    public void DetermineRecommendationLevel_Should_Return_Appropriate_Level()
    {
        // Arrange
        var signal1 = CreateTestCanSignal("ENGINE_RPM", "0x123", CanSystemType.Engine);
        var signal2 = CreateTestCanSignal("ENGINE_SPEED", "0x123", CanSystemType.Engine);
        var criteria = SimilaritySearchCriteria.CreateDefault();
        
        var breakdown = _similarPatternSearchService.CalculateSimilarityBreakdown(signal1, signal2, criteria);
        var differences = new List<AttributeDifference>();

        // Act
        var recommendationLevel = _similarPatternSearchService.DetermineRecommendationLevel(
            0.9, breakdown, differences);

        // Assert
        recommendationLevel.ShouldBeOneOf(RecommendationLevel.High, RecommendationLevel.Highly);
    }

    [Theory]
    [InlineData("ENGINE_RPM", "ENGINE_SPEED", 0.6)] // 部分的に類似
    [InlineData("ENGINE_RPM", "ENGINE_RPM", 1.0)]   // 完全一致
    [InlineData("ENGINE_RPM", "BRAKE_PRESSURE", 0.2)] // 非類似
    public void String_Similarity_Should_Work_Correctly(string str1, string str2, double expectedMinSimilarity)
    {
        // Arrange
        var signal1 = CreateTestCanSignal(str1, "0x123", CanSystemType.Engine);
        var signal2 = CreateTestCanSignal(str2, "0x124", CanSystemType.Engine);
        var criteria = SimilaritySearchCriteria.CreateDefault();

        // Act
        var breakdown = _similarPatternSearchService.CalculateSimilarityBreakdown(signal1, signal2, criteria);

        // Assert
        breakdown.SignalNameSimilarity.ShouldBeGreaterThanOrEqualTo(expectedMinSimilarity);
    }

    #region Helper Methods

    private CanSignal CreateTestCanSignal(string signalName, string canId, CanSystemType systemType)
    {
        var identifier = new SignalIdentifier(signalName, canId);
        var valueRange = new SignalValueRange(0, 100);
        var specification = new SignalSpecification(0, 16, SignalDataType.Unsigned, valueRange);
        var oemCode = new OemCode("TEST", "Test OEM");

        return new CanSignal(
            Guid.NewGuid(),
            null, // tenantId
            identifier,
            specification,
            systemType,
            oemCode,
            $"Test signal: {signalName}");
    }

    private AnomalyDetectionResult CreateTestAnomalyDetectionResult(Guid signalId, AnomalyLevel level)
    {
        var inputData = new DetectionInputData(50.0, DateTime.UtcNow);
        var details = new DetectionDetails(DetectionType.OutOfRange, "Test condition");

        return new AnomalyDetectionResult(
            Guid.NewGuid(),
            null, // tenantId
            Guid.NewGuid(), // detectionLogicId
            signalId,
            level,
            0.8, // confidenceScore
            "Test anomaly detection result",
            inputData,
            details);
    }

    #endregion
}