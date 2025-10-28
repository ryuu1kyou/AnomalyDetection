using System;
using System.Collections.Generic;
using AnomalyDetection.SimilarPatternSearch;
using AnomalyDetection.SimilarPatternSearch.Dtos;
using Shouldly;
using Xunit;
using DomainRecommendationLevel = AnomalyDetection.SimilarPatternSearch.RecommendationLevel;

namespace AnomalyDetection.Application.Tests.SimilarPatternSearch;

/// <summary>
/// 類似パターン検索の統合テスト（簡易版）
/// 実際の統合テストは、完全なABPテスト環境が必要
/// </summary>
public class SimilarPatternSearchIntegrationTests
{
    [Fact]
    public void SimilaritySearchCriteria_Should_Create_Valid_Instances()
    {
        // Arrange & Act
        var defaultCriteria = SimilaritySearchCriteria.CreateDefault();
        var strictCriteria = SimilaritySearchCriteria.CreateStrict();
        var looseCriteria = SimilaritySearchCriteria.CreateLoose();

        // Assert
        defaultCriteria.ShouldNotBeNull();
        defaultCriteria.HasValidComparisons().ShouldBeTrue();
        
        strictCriteria.ShouldNotBeNull();
        strictCriteria.MinimumSimilarity.ShouldBe(0.9);
        
        looseCriteria.ShouldNotBeNull();
        looseCriteria.MinimumSimilarity.ShouldBe(0.3);
    }

    [Fact]
    public void SimilarSignalResult_Should_Create_Valid_Instance()
    {
        // Arrange
        var signalId = Guid.NewGuid();
        var breakdown = new SimilarityBreakdown(0.8, 0.7, 1.0);
        var matchedAttributes = new[] { "SystemType", "SignalName" };
        var differences = new List<AttributeDifference>();

        // Act
        var result = new SimilarSignalResult(
            signalId, 0.85, breakdown, matchedAttributes, differences, 
            DomainRecommendationLevel.High, "High similarity detected");

        // Assert
        result.ShouldNotBeNull();
        result.SignalId.ShouldBe(signalId);
        result.SimilarityScore.ShouldBe(0.85);
        result.IsHighSimilarity().ShouldBeTrue();
        result.IsRecommended().ShouldBeTrue();
    }

    [Fact]
    public void TestDataComparison_Should_Create_Valid_Instance()
    {
        // Arrange
        var sourceSignalId = Guid.NewGuid();
        var targetSignalId = Guid.NewGuid();
        var thresholdDifferences = new List<ThresholdDifference>();
        var conditionDifferences = new List<DetectionConditionDifference>();
        var resultDifferences = new List<ResultDifference>();
        var recommendations = new List<ComparisonRecommendation>();

        // Act
        var comparison = new TestDataComparison(
            sourceSignalId, targetSignalId, thresholdDifferences, 
            conditionDifferences, resultDifferences, recommendations, 
            0.75, "Test comparison");

        // Assert
        comparison.ShouldNotBeNull();
        comparison.SourceSignalId.ShouldBe(sourceSignalId);
        comparison.TargetSignalId.ShouldBe(targetSignalId);
        comparison.OverallSimilarityScore.ShouldBe(0.75);
        comparison.HasHighSimilarity().ShouldBeFalse(); // 0.75 < 0.8
    }

    [Fact]
    public void SimilaritySearchCriteriaDto_Should_Have_Valid_Defaults()
    {
        // Arrange & Act
        var dto = new SimilaritySearchCriteriaDto();

        // Assert
        dto.CompareCanId.ShouldBeTrue();
        dto.CompareSignalName.ShouldBeTrue();
        dto.CompareSystemType.ShouldBeTrue();
        dto.MinimumSimilarity.ShouldBe(0.5);
        dto.MaxResults.ShouldBe(50);
        dto.ActiveSignalsOnly.ShouldBeTrue();
    }

    [Fact]
    public void SimilarSignalResultDto_Should_Initialize_Collections()
    {
        // Arrange & Act
        var dto = new SimilarSignalResultDto();

        // Assert
        dto.MatchedAttributes.ShouldNotBeNull();
        dto.Differences.ShouldNotBeNull();
        dto.Breakdown.ShouldNotBeNull();
        dto.SignalInfo.ShouldNotBeNull();
    }

    [Fact]
    public void TestDataComparisonDto_Should_Initialize_Collections()
    {
        // Arrange & Act
        var dto = new TestDataComparisonDto();

        // Assert
        dto.ThresholdDifferences.ShouldNotBeNull();
        dto.DetectionConditionDifferences.ShouldNotBeNull();
        dto.ResultDifferences.ShouldNotBeNull();
        dto.Recommendations.ShouldNotBeNull();
        dto.Statistics.ShouldNotBeNull();
    }
}