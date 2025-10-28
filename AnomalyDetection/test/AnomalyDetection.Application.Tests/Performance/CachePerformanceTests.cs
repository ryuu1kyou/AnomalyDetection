using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.Performance;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace AnomalyDetection.Performance;

/// <summary>
/// キャッシュパフォーマンステスト
/// </summary>
public class CachePerformanceTests : PerformanceTestBase
{
    private readonly CachingService _cachingService;
    private readonly IRepository<OemMaster, Guid> _oemMasterRepository;
    private readonly IRepository<CanSystemCategory, Guid> _systemCategoryRepository;

    public CachePerformanceTests(ITestOutputHelper output) : base(output)
    {
        _cachingService = GetRequiredService<CachingService>();
        _oemMasterRepository = GetRequiredService<IRepository<OemMaster, Guid>>();
        _systemCategoryRepository = GetRequiredService<IRepository<CanSystemCategory, Guid>>();
    }

    /// <summary>
    /// キャッシュヒット時の性能テスト
    /// </summary>
    [Fact]
    public async Task Cache_Hit_ShouldBeFasterThanDatabaseQuery()
    {
        // Arrange
        var oemMaster = await CreateTestOemMasterAsync();
        
        // 最初の呼び出しでキャッシュに保存
        await _cachingService.GetOemMasterAsync(oemMaster.Id);

        // Act & Assert - キャッシュヒット
        var cacheResult = await MeasureExecutionTimeAsync(
            async () =>
            {
                var cached = await _cachingService.GetOemMasterAsync(oemMaster.Id);
                cached.ShouldNotBeNull();
                cached.Id.ShouldBe(oemMaster.Id);
            },
            "Cache Hit - OEM Master",
            expectedMaxMilliseconds: 50); // キャッシュは50ms以内

        // データベース直接アクセス
        var dbResult = await MeasureExecutionTimeAsync(
            async () =>
            {
                var fromDb = await _oemMasterRepository.GetAsync(oemMaster.Id);
                fromDb.ShouldNotBeNull();
            },
            "Database Query - OEM Master",
            expectedMaxMilliseconds: 200);

        // キャッシュの方が高速であることを確認
        cacheResult.ExecutionTimeMs.ShouldBeLessThan(dbResult.ExecutionTimeMs);
        
        AssertPerformanceRequirement(cacheResult);
        Output.WriteLine($"Cache speedup: {(double)dbResult.ExecutionTimeMs / cacheResult.ExecutionTimeMs:F1}x faster");
    }

    /// <summary>
    /// キャッシュミス時の性能テスト
    /// </summary>
    [Fact]
    public async Task Cache_Miss_ShouldHaveMinimalOverhead()
    {
        // Arrange
        var oemMaster = await CreateTestOemMasterAsync();

        // Act & Assert - キャッシュミス（初回アクセス）
        var cacheMissResult = await MeasureExecutionTimeAsync(
            async () =>
            {
                var result = await _cachingService.GetOemMasterAsync(oemMaster.Id);
                result.ShouldNotBeNull();
            },
            "Cache Miss - OEM Master",
            expectedMaxMilliseconds: 300); // キャッシュミスでも300ms以内

        // データベース直接アクセス
        var dbResult = await MeasureExecutionTimeAsync(
            async () =>
            {
                var fromDb = await _oemMasterRepository.GetAsync(oemMaster.Id);
                fromDb.ShouldNotBeNull();
            },
            "Database Query - OEM Master",
            expectedMaxMilliseconds: 200);

        AssertPerformanceRequirement(cacheMissResult);
        
        // キャッシュミスのオーバーヘッドが最小限であることを確認
        var overhead = cacheMissResult.ExecutionTimeMs - dbResult.ExecutionTimeMs;
        overhead.ShouldBeLessThan(100); // オーバーヘッドは100ms未満
        
        Output.WriteLine($"Cache miss overhead: {overhead}ms");
    }

    /// <summary>
    /// 大量データのキャッシュ性能テスト
    /// </summary>
    [Fact]
    public async Task Cache_BulkData_ShouldMaintainPerformance()
    {
        // Arrange
        await CreateTestSystemCategoriesAsync();

        // Act & Assert - 全システムカテゴリの取得
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var categories = await _cachingService.GetAllSystemCategoriesAsync();
                categories.ShouldNotBeNull();
                categories.Count.ShouldBeGreaterThan(0);
            },
            "Cache Bulk Data - System Categories",
            expectedMaxMilliseconds: 100);

        AssertPerformanceRequirement(result);
    }

    /// <summary>
    /// 並行キャッシュアクセスの性能テスト
    /// </summary>
    [Fact]
    public async Task Cache_ConcurrentAccess_ShouldMaintainPerformance()
    {
        // Arrange
        var oemMaster = await CreateTestOemMasterAsync();

        // Act & Assert - 並行アクセス
        var loadTestResult = await ExecuteLoadTestAsync(
            async () =>
            {
                var result = await _cachingService.GetOemMasterAsync(oemMaster.Id);
                result.ShouldNotBeNull();
            },
            "Concurrent Cache Access",
            concurrentUsers: 50,
            operationsPerUser: 10,
            expectedMaxMilliseconds: 100);

        AssertLoadTestRequirement(loadTestResult, minThroughput: 100.0);
    }

    /// <summary>
    /// 統計キャッシュの性能テスト
    /// </summary>
    [Fact]
    public async Task Cache_Statistics_ShouldBeEfficient()
    {
        // Arrange
        await CreateTestCanSignalsForStatisticsAsync(1000);

        // Act & Assert - 統計データのキャッシュ
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                var stats = await _cachingService.GetSignalStatisticsAsync(SystemType.Engine);
                stats.ShouldNotBeNull();
                stats.TotalCount.ShouldBeGreaterThan(0);
            },
            "Cache Statistics",
            expectedMaxMilliseconds: 200);

        AssertPerformanceRequirement(result);

        // 2回目のアクセス（キャッシュヒット）
        var cachedResult = await MeasureExecutionTimeAsync(
            async () =>
            {
                var stats = await _cachingService.GetSignalStatisticsAsync(SystemType.Engine);
                stats.ShouldNotBeNull();
            },
            "Cache Statistics Hit",
            expectedMaxMilliseconds: 50);

        AssertPerformanceRequirement(cachedResult);
        cachedResult.ExecutionTimeMs.ShouldBeLessThan(result.ExecutionTimeMs);
    }

    /// <summary>
    /// キャッシュ無効化の性能テスト
    /// </summary>
    [Fact]
    public async Task Cache_Invalidation_ShouldBeQuick()
    {
        // Arrange
        var oemMaster = await CreateTestOemMasterAsync();
        await _cachingService.GetOemMasterAsync(oemMaster.Id); // キャッシュに保存

        // Act & Assert - キャッシュ無効化
        var result = await MeasureExecutionTimeAsync(
            async () =>
            {
                await _cachingService.InvalidateOemMasterCacheAsync(oemMaster.Id);
            },
            "Cache Invalidation",
            expectedMaxMilliseconds: 100);

        AssertPerformanceRequirement(result);
    }

    /// <summary>
    /// 平均キャッシュ性能の測定
    /// </summary>
    [Fact]
    public async Task Cache_AveragePerformance_ShouldBeConsistent()
    {
        // Arrange
        var oemMasters = await CreateMultipleOemMastersAsync(10);

        // 全てをキャッシュに保存
        foreach (var master in oemMasters)
        {
            await _cachingService.GetOemMasterAsync(master.Id);
        }

        // Act & Assert - 平均性能測定
        var statistics = await MeasureAverageExecutionTimeAsync(
            async () =>
            {
                var randomMaster = oemMasters[new Random().Next(oemMasters.Count)];
                var result = await _cachingService.GetOemMasterAsync(randomMaster.Id);
                result.ShouldNotBeNull();
            },
            "Average Cache Performance",
            iterations: 20,
            expectedMaxMilliseconds: 50);

        AssertAveragePerformanceRequirement(statistics);
    }

    #region Test Data Creation Helper Methods

    private async Task<OemMaster> CreateTestOemMasterAsync()
    {
        var oemMaster = new OemMaster(
            new OemCode("PERF", "Performance Test OEM"),
            "Performance Test Company",
            "Japan");

        return await _oemMasterRepository.InsertAsync(oemMaster);
    }

    private async Task<List<OemMaster>> CreateMultipleOemMastersAsync(int count)
    {
        var masters = new List<OemMaster>();
        
        for (int i = 0; i < count; i++)
        {
            var master = new OemMaster(
                new OemCode($"OEM{i:D2}", $"Test OEM {i}"),
                $"Test Company {i}",
                "Japan");
            
            masters.Add(master);
        }

        await _oemMasterRepository.InsertManyAsync(masters);
        return masters;
    }

    private async Task CreateTestSystemCategoriesAsync()
    {
        var categories = new List<CanSystemCategory>();
        
        foreach (SystemType systemType in Enum.GetValues<SystemType>())
        {
            var category = new CanSystemCategory(
                systemType,
                $"{systemType} System",
                $"Test category for {systemType}");
            
            categories.Add(category);
        }

        await _systemCategoryRepository.InsertManyAsync(categories);
    }

    private async Task CreateTestCanSignalsForStatisticsAsync(int count)
    {
        var canSignalRepository = GetRequiredService<IRepository<CanSignal, Guid>>();
        var signals = new List<CanSignal>();
        
        for (int i = 0; i < count; i++)
        {
            var signal = new CanSignal(
                new SignalIdentifier($"StatSignal_{i:D4}", $"0x{i:X3}"),
                new SignalSpecification(0, 8, SignalDataType.Unsigned, ByteOrder.LittleEndian, new ValueRange(0, 255)),
                SystemType.Engine,
                new OemCode("STAT", "Statistics Test OEM"));
            
            signals.Add(signal);
        }

        await canSignalRepository.InsertManyAsync(signals);
    }

    #endregion
}