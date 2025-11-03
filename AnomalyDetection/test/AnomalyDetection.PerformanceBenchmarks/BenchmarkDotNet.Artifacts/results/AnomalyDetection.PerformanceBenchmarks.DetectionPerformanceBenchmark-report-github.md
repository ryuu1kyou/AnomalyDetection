```

BenchmarkDotNet v0.15.5, Windows 11 (10.0.22631.6060/23H2/2023Update/SunValley3)
Intel Core i5-8250U CPU 1.60GHz (Max: 1.80GHz) (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  Job-FFJRMS : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

IterationCount=5  LaunchCount=1  RunStrategy=Throughput  
WarmupCount=3  

```
| Method                    | SampleSize | Mean       | Error      | StdDev    | Gen0    | Allocated |
|-------------------------- |----------- |-----------:|-----------:|----------:|--------:|----------:|
| **CalculateOptimalThreshold** | **100**        |  **12.483 μs** |  **1.7048 μs** | **0.2638 μs** |  **1.7395** |   **5.35 KB** |
| DetectOutliers_IQR        | 100        |   3.879 μs |  0.8611 μs | 0.1333 μs |  1.2703 |    3.9 KB |
| DetectOutliers_ZScore     | 100        |   6.608 μs |  1.3629 μs | 0.3539 μs |  0.4578 |   1.42 KB |
| CalculateStatistics       | 100        |   7.993 μs |  1.1092 μs | 0.2881 μs |  1.0376 |   3.22 KB |
| **CalculateOptimalThreshold** | **500**        |  **64.954 μs** |  **7.0887 μs** | **1.8409 μs** |  **6.2256** |  **19.26 KB** |
| DetectOutliers_IQR        | 500        |  22.845 μs |  4.6903 μs | 1.2181 μs |  5.3711 |  16.51 KB |
| DetectOutliers_ZScore     | 500        |  37.913 μs | 16.8873 μs | 4.3856 μs |  1.5259 |   4.81 KB |
| CalculateStatistics       | 500        |  43.219 μs |  3.8928 μs | 1.0110 μs |  4.5776 |  14.16 KB |
| **CalculateOptimalThreshold** | **1000**       | **161.155 μs** | **13.4359 μs** | **3.4893 μs** | **11.7188** |  **36.64 KB** |
| DetectOutliers_IQR        | 1000       |  72.178 μs |  6.3911 μs | 0.9890 μs | 10.4980 |  32.27 KB |
| DetectOutliers_ZScore     | 1000       |  65.681 μs |  9.6167 μs | 2.4974 μs |  2.9297 |   9.05 KB |
| CalculateStatistics       | 1000       | 126.579 μs | 32.1699 μs | 8.3544 μs |  9.0332 |  27.83 KB |
