using System.Diagnostics;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.CanSignals.Dtos;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AnomalyDetection.Application.Tests.Performance;

/// <summary>
/// クエリパフォーマンステスト - NFR 1.1-1.5
/// </summary>
public class QueryPerformanceTests : PerformanceTestBase
{
    private readonly ICanSignalAppService _canSignalAppService;

    public QueryPerformanceTests(ITestOutputHelper output) : base(output) 
    {
        _canSignalAppService = GetRequiredService<ICanSignalAppService>();
    }

    [Fact]
    public async Task CanSignal_Query_ShouldCompleteWithin500ms()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var input = new GetCanSignalsInput { MaxResultCount = 100 };

        // Act
        var result = await _canSignalAppService.GetListAsync(input);
        stopwatch.Stop();

        // Assert
        result.ShouldNotBeNull();
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(500);
        Output.WriteLine($"CAN Signal query completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task Dashboard_Load_ShouldCompleteWithin1000ms()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var signalsTask = _canSignalAppService.GetListAsync(new GetCanSignalsInput { MaxResultCount = 10 });
        await Task.WhenAll(signalsTask);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000);
        Output.WriteLine($"Dashboard load completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact(Skip = "Requires load testing infrastructure")]
    public async Task System_ShouldHandle100ConcurrentUsers()
    {
        await Task.CompletedTask;
    }
}
