```

BenchmarkDotNet v0.15.5, Windows 11 (10.0.22631.6060/23H2/2023Update/SunValley3)
Intel Core i5-8250U CPU 1.60GHz (Max: 1.80GHz) (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3
  Job-MLSUAM : .NET 9.0.9 (9.0.9, 9.0.925.41916), X64 RyuJIT x86-64-v3

IterationCount=3  LaunchCount=1  RunStrategy=Throughput  
WarmupCount=2  

```
| Method                         | MessageCount | Mean        | Error       | StdDev     | Gen0     | Gen1    | Gen2    | Allocated |
|------------------------------- |------------- |------------:|------------:|-----------:|---------:|--------:|--------:|----------:|
| **MessageSerializationThroughput** | **100**          |   **154.16 μs** |   **672.85 μs** |  **36.881 μs** |   **6.3477** |  **5.8594** |  **0.4883** |  **40.98 KB** |
| ParallelMessageSerialization   | 100          |   112.41 μs |   656.33 μs |  35.975 μs |   7.3242 |  6.8359 |  0.4883 |  45.14 KB |
| SequentialMessageProcessing    | 100          |   189.08 μs |   282.81 μs |  15.502 μs |  12.6953 |       - |       - |  39.17 KB |
| ParallelMessageProcessing      | 100          |    68.28 μs |    25.91 μs |   1.420 μs |  17.8223 |       - |       - |  53.15 KB |
| BatchMessageProcessing         | 100          |   138.88 μs |    66.20 μs |   3.629 μs |  21.9727 |       - |       - |  65.69 KB |
| **MessageSerializationThroughput** | **500**          |   **562.49 μs** | **1,171.19 μs** |  **64.197 μs** |  **32.2266** | **31.2500** | **11.7188** | **203.14 KB** |
| ParallelMessageSerialization   | 500          |   314.72 μs |   256.00 μs |  14.032 μs |  34.1797 | 33.2031 |  8.7891 | 207.98 KB |
| SequentialMessageProcessing    | 500          |   754.81 μs | 1,719.46 μs |  94.250 μs |  63.4766 |       - |       - | 195.09 KB |
| ParallelMessageProcessing      | 500          |   269.00 μs |    79.71 μs |   4.369 μs |  67.3828 | 19.5313 |       - | 262.05 KB |
| BatchMessageProcessing         | 500          |   948.83 μs | 2,110.13 μs | 115.663 μs | 109.3750 |       - |       - | 326.89 KB |
| **MessageSerializationThroughput** | **1000**         | **1,057.83 μs** |   **693.72 μs** |  **38.025 μs** |  **64.4531** | **62.5000** | **31.2500** | **405.98 KB** |
| ParallelMessageSerialization   | 1000         |   668.63 μs |   978.86 μs |  53.655 μs |  68.3594 | 66.4063 | 29.2969 | 414.36 KB |
| SequentialMessageProcessing    | 1000         | 1,213.77 μs |   663.20 μs |  36.353 μs | 126.9531 |       - |       - | 390.01 KB |
| ParallelMessageProcessing      | 1000         |   624.50 μs | 1,133.95 μs |  62.156 μs | 111.3281 | 64.4531 |       - | 523.46 KB |
| BatchMessageProcessing         | 1000         | 1,697.83 μs | 3,118.03 μs | 170.910 μs | 218.7500 |       - |       - | 653.25 KB |
