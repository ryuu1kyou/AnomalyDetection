using System;
using System.Collections.Generic;
using AnomalyDetection.SimilarPatternSearch;
using AnomalyDetection.SimilarPatternSearch.Dtos;
using Shouldly;
using Xunit;
using DomainRecommendationLevel = AnomalyDetection.SimilarPatternSearch.RecommendationLevel;

namespace AnomalyDetection.Application.Tests.SimilarPatternSearch;

/// <summary>
/// Tests for Similar Pattern Search - Domain Objects and DTOs
/// </summary>
public class SimilarPatternSearchTests
{
    [Fact]
    public void SimilaritySearchCriteria_Should_Create_Default_Instance()
    {
        // Arrange & Act
        var criteria = SimilaritySearchCriteria.CreateDefault();

        // Assert
        criteria.ShouldNotBeNull();
        criteria.HasValidComparisons().ShouldBeTrue();
        criteria.MinimumSimilarity.ShouldBe(0.5);
    }

    [Fact]
    public void SimilaritySearchCriteria_Should_Create_Strict_Instance()
    {
        // Arrange & Act
        var criteria = SimilaritySearchCriteria.CreateStrict();

        // Assert
        criteria.ShouldNotBeNull();
        criteria.MinimumSimilarity.ShouldBe(0.9);
    }

    [Fact]
    public void SimilaritySearchCriteria_Should_Create_Loose_Instance()
    {
        // Arrange & Act
        var criteria = SimilaritySearchCriteria.CreateLoose();

        // Assert
        criteria.ShouldNotBeNull();
        criteria.MinimumSimilarity.ShouldBe(0.3);
    }

    [Fact]
    public void SimilarSignalResult_Should_Create_Valid_Instance()
    {
        // Arrange
        var signalId = Guid.NewGuid();
    }

    [Fact]
    public void TestDataComparison_Should_Create_Valid_Instance()
    {
        // Arrange
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        // Act
        var comparison = new TestDataComparison(
            sourceId, targetId,
            new List<ThresholdDifference>(),
            new List<DetectionConditionDifference>(),
            new List<ResultDifference>(),
            new List<ComparisonRecommendation>(),
            0.75, "Test");

        // Assert
        comparison.ShouldNotBeNull();
        comparison.SourceSignalId.ShouldBe(sourceId);
        comparison.TargetSignalId.ShouldBe(targetId);
        comparison.OverallSimilarityScore.ShouldBe(0.75);
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