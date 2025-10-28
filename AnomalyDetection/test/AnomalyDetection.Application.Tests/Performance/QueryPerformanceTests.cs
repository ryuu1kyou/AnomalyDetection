using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.Performance;
using AnomalyDetection.SimilarPatternSearch;
using AnomalyDetection.SimilarPatternSearch.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace AnomalyDetection.Performance;

/// <summary>
/// クエリパフォーマンステスト
/// NFR 1.1-1.5 の要件を検証
/// </summary>
public class QueryPerformanceTests : PerformanceTestBase
{
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly IRepository<AnomalyDetectionResult, Guid> _detectionResultRepository;
    private readonly ISimilarPatternSearchAppService _similarPatternSearchAppService;
    private readonly QueryOptimizationService _queryOptimizationService;

    public QueryPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _canSignalRepository = GetRequiredService<IRepository<CanSignal, Guid>>();
        _detectionResultRepository = GetRequiredService<IRepository<AnomalyDetectionResult, Guid>>();
        _similarPatternSearchAppService = GetRequiredService<ISimilarPatternSearchAppService>();
        _queryOptimizationService = GetRequiredService<QueryOptimizationService>();
    }

    /// <summary>
    /// NFR 1.1: CAN信号クエリが500ms以内で完了することを検証
    /// </summary>
    [Fact]
    public async Task CanSignal_Query_ShouldCompleteWithin500ms()
    {
        // Arrange - テストデータを作成
        await CreateTestCanSignalsAsync(1000);

        // Act & Assert - NFR 1.1: 500ms以内
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var signals = await _queryOptimizationService.GetCanSignalsOptimizedAsync(
                    skipCount: 0,
                    maxResultCount: 100,
                    systemType: SystemType.Engine);
                
                signals.Items.Count.ShouldBeGreaterThan(0);
            },
            "CAN Signal Query (1000 records)",
            expectedMaxMilliseconds: 500);

        AssertPerformanceRequirement(result);
    }

    /// <summary>
    /// NFR 1.2: 検出ロジック実行が100ms以内で完了することを検証
    /// </summary>
    [Fact]
    public async Task DetectionLogic_Execution_ShouldCompleteWithin100ms()
    {
        // Arrange
        await CreateTestDetectionLogicAsync();

        // Act & Assert - NFR 1.2: 100ms以内
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var logics = await _queryOptimizationService.GetDetectionLogicsOptimizedAsync(
                    skipCount: 0,
                    maxResultCount: 50,
                    status: DetectionLogicStatus.Active);
                
                logics.Items.Count.ShouldBeGreaterThanOrEqualTo(0);
            },
            "Detection Logic Query",
            expectedMaxMilliseconds: 100);

        AssertPerformanceRequirement(result);
    }

    /// <summary>
    /// NFR 1.3: 類似検索が2秒以内で完了することを検証
    /// </summary>
    [Fact]
    public async Task SimilaritySearch_ShouldCompleteWithin2Seconds()
    {
        // Arrange
        var targetSignal = await CreateTestCanSignalAsync();
        await CreateTestCanSignalsAsync(1000); // 候補信号

        var searchRequest = new SimilarSignalSearchRequestDto
        {
            TargetSignalId = targetSignal.Id,
            Criteria = new SimilaritySearchCriteriaDto
            {
                CompareCanId = true,
                CompareSignalName = true,
                CompareSystemType = true,
                MinimumSimilarity = 0.5,
                MaxResults = 100
            }
        };

        // Act & Assert - NFR 1.3: 2秒以内
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var searchResults = await _similarPatternSearchAppService.SearchSimilarSignalsAsync(searchRequest);
                searchResults.ShouldNotBeNull();
            },
            "Similarity Search (1000 candidates)",
            expectedMaxMilliseconds: 2000);

        AssertPerformanceRequirement(result);
    }

    /// <summary>
    /// NFR 1.4: ダッシュボード読み込みが1秒以内で完了することを検証
    /// </summary>
    [Fact]
    public async Task Dashboard_Loading_ShouldCompleteWithin1Second()
    {
        // Arrange
        await CreateTestDataForDashboardAsync();

        // Act & Assert - NFR 1.4: 1秒以内
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var statistics = await _queryOptimizationService.GetDashboardStatisticsOptimizedAsync();
                statistics.ShouldNotBeNull();
                statistics.GeneratedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
            },
            "Dashboard Loading",
            expectedMaxMilliseconds: 1000);

        AssertPerformanceRequirement(result);
    }

    /// <summary>
    /// NFR 1.5: 100並行ユーザーでのパフォーマンス検証
    /// </summary>
    [Fact]
    public async Task ConcurrentUsers_ShouldMaintainPerformance()
    {
        // Arrange
        await CreateTestCanSignalsAsync(5000);

        // Act & Assert - NFR 1.5: 100並行ユーザー
        var loadTestResult = await ExecuteLoadTestAsync(
            async () =>
            {
                var signals = await _queryOptimizationService.GetCanSignalsOptimizedAsync(
                    skipCount: 0,
                    maxResultCount: 20);
                
                signals.Items.Count.ShouldBeGreaterThanOrEqualTo(0);
            },
            "Concurrent CAN Signal Query",
            concurrentUsers: 100,
            operationsPerUser: 3,
            expectedMaxMilliseconds: 1000);

        AssertLoadTestRequirement(loadTestResult, minThroughput: 50.0); // 最小50 ops/sec
    }

    /// <summary>
    /// ページネーション性能テスト
    /// </summary>
    [Fact]
    public async Task Pagination_Performance_ShouldBeConsistent()
    {
        // Arrange
        await CreateTestDetectionResultsAsync(10000);

        // Act & Assert - ページネーションの性能が一定であることを確認
        var statistics = await MeasureAverageExecutionTimeAsync(
            async () =>
            {
                var results = await _queryOptimizationService.GetDetectionResultsOptimizedAsync(
                    skipCount: 5000, // 中間ページ
                    maxResultCount: 50,
                    startDate: DateTime.UtcNow.AddDays(-30));
                
                results.Items.Count.ShouldBeGreaterThanOrEqualTo(0);
            },
            "Pagination Performance (middle page)",
            iterations: 10,
            expectedMaxMilliseconds: 500);

        AssertAveragePerformanceRequirement(statistics);
    }

    /// <summary>
    /// 複合フィルタリング性能テスト
    /// </summary>
    [Fact]
    public async Task ComplexFiltering_Performance_ShouldBeOptimal()
    {
        // Arrange
        await CreateTestDetectionResultsAsync(5000);

        // Act & Assert
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var results = await _queryOptimizationService.GetDetectionResultsOptimizedAsync(
                    skipCount: 0,
                    maxResultCount: 100,
                    startDate: DateTime.UtcNow.AddDays(-7),
                    endDate: DateTime.UtcNow,
                    anomalyLevel: AnomalyLevel.High,
                    resolutionStatus: ResolutionStatus.Open);
                
                results.ShouldNotBeNull();
            },
            "Complex Filtering Query",
            expectedMaxMilliseconds: 300);

        AssertPerformanceRequirement(result);
    }

    #region Test Data Creation Helper Methods

    private async Task<CanSignal> CreateTestCanSignalAsync()
    {
        var signal = new CanSignal(
            new SignalIdentifier("TestSignal", "0x123"),
            new SignalSpecification(0, 8, SignalDataType.Unsigned, ByteOrder.LittleEndian, new ValueRange(0, 255)),
            SystemType.Engine,
            new OemCode("TEST", "Test OEM"));

        return await _canSignalRepository.InsertAsync(signal);
    }

    private async Task CreateTestCanSignalsAsync(int count)
    {
        var signals = new List<CanSignal>();
        
        for (int i = 0; i < count; i++)
        {
            var signal = new CanSignal(
                new SignalIdentifier($"Signal_{i:D4}", $"0x{i:X3}"),
                new SignalSpecification(0, 8, SignalDataType.Unsigned, ByteOrder.LittleEndian, new ValueRange(0, 255)),
                (SystemType)(i % Enum.GetValues<SystemType>().Length),
                new OemCode("TEST", "Test OEM"));
            
            signals.Add(signal);
        }

        await _canSignalRepository.InsertManyAsync(signals);
    }

    private async Task CreateTestDetectionLogicAsync()
    {
        var logic = new CanAnomalyDetectionLogic(
            new DetectionLogicIdentity(
                "TestLogic",
                new LogicVersion(1, 0, 0),
                new OemCode("TEST", "Test OEM")),
            new DetectionLogicSpecification(
                DetectionType.Threshold,
                "Test detection logic",
                SystemType.Engine,
                ComplexityLevel.Low,
                "Test requirements"));

        await _detectionLogicRepository.InsertAsync(logic);
    }

    private async Task CreateTestDetectionResultsAsync(int count)
    {
        var signal = await CreateTestCanSignalAsync();
        var logic = await CreateTestDetectionLogicAsync();
        
        var results = new List<AnomalyDetectionResult>();
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            var result = new AnomalyDetectionResult(
                logic.Id,
                signal.Id,
                DateTime.UtcNow.AddMinutes(-random.Next(0, 43200)), // 過去30日間
                new DetectionInputData(random.NextDouble() * 100, DateTime.UtcNow, new Dictionary<string, object>()),
                new DetectionDetails(DetectionType.Threshold, "Test condition", 10, new Dictionary<string, object>()),
                (AnomalyLevel)(random.Next(0, 4)),
                random.NextDouble(),
                "Test anomaly description");

            results.Add(result);
        }

        await _detectionResultRepository.InsertManyAsync(results);
    }

    private async Task CreateTestDataForDashboardAsync()
    {
        await CreateTestCanSignalsAsync(100);
        await CreateTestDetectionResultsAsync(500);
    }

    #endregion
}