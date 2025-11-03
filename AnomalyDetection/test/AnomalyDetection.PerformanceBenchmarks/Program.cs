using BenchmarkDotNet.Running;

namespace AnomalyDetection.PerformanceBenchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // Run all benchmarks
        BenchmarkRunner.Run<DetectionPerformanceBenchmark>();
        BenchmarkRunner.Run<MessageThroughputBenchmark>();
    }
}
