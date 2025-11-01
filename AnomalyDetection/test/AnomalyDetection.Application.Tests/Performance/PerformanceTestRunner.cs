using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AnomalyDetection.Application.Tests.Performance;

/// <summary>
/// パフォーマンステスト実行とレポート生成
/// TODO: 依存サービスの実装完了後に有効化
/// </summary>
public class PerformanceTestRunner : PerformanceTestBase
{
    public PerformanceTestRunner(ITestOutputHelper output) : base(output) { }

    [Fact(Skip = "Requires implementation of dependent services")]
    public async Task RunAllPerformanceTests_AndGenerateReport()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires implementation of dependent services")]
    public async Task SystemIntegration_PerformanceTest()
    {
        await Task.CompletedTask;
    }
}
