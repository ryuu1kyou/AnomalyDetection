using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnomalyDetection.AnomalyDetection.Services;

/// <summary>
/// 統計的閾値最適化サービスのインターフェース
/// 機械学習ベースの閾値最適化アルゴリズムを提供
/// </summary>
public interface IStatisticalThresholdOptimizer
{
    /// <summary>
    /// 履歴データから最適閾値を計算（分位数ベース）
    /// </summary>
    Task<OptimalThresholdResult> CalculateOptimalThresholdAsync(
        List<double> historicalValues,
        ThresholdOptimizationConfig config);

    /// <summary>
    /// 外れ値検出による異常閾値推定（IQRメソッド）
    /// </summary>
    Task<OutlierDetectionResult> DetectOutliersAsync(
        List<double> values,
        OutlierDetectionMethod method = OutlierDetectionMethod.IQR);

    /// <summary>
    /// トレンド分析による動的閾値調整
    /// </summary>
    Task<DynamicThresholdResult> CalculateDynamicThresholdAsync(
        List<TimeSeriesDataPoint> timeSeriesData,
        int windowSize = 100);

    /// <summary>
    /// 多変量分析による閾値最適化（複数信号の相関考慮）
    /// </summary>
    Task<MultivariateThresholdResult> OptimizeMultivariateThresholdAsync(
        Dictionary<string, List<double>> signalValues,
        double correlationThreshold = 0.7);
}

/// <summary>
/// 閾値最適化設定
/// </summary>
public class ThresholdOptimizationConfig
{
    /// <summary>
    /// 目標誤検出率（False Positive Rate）
    /// </summary>
    public double TargetFalsePositiveRate { get; set; } = 0.05; // 5%

    /// <summary>
    /// 目標検出率（True Positive Rate）
    /// </summary>
    public double TargetTruePositiveRate { get; set; } = 0.95; // 95%

    /// <summary>
    /// 信頼区間レベル
    /// </summary>
    public double ConfidenceLevel { get; set; } = 0.95; // 95%

    /// <summary>
    /// 分位数パーセンタイル（上限閾値用）
    /// </summary>
    public double UpperPercentile { get; set; } = 0.95; // 95th percentile

    /// <summary>
    /// 分位数パーセンタイル（下限閾値用）
    /// </summary>
    public double LowerPercentile { get; set; } = 0.05; // 5th percentile

    /// <summary>
    /// 最小サンプルサイズ
    /// </summary>
    public int MinimumSampleSize { get; set; } = 100;

    /// <summary>
    /// 季節性を考慮するか
    /// </summary>
    public bool ConsiderSeasonality { get; set; } = true;
}

/// <summary>
/// 最適閾値計算結果
/// </summary>
public class OptimalThresholdResult
{
    public double RecommendedUpperThreshold { get; set; }
    public double RecommendedLowerThreshold { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double Median { get; set; }
    public int SampleSize { get; set; }
    public double ConfidenceLevel { get; set; }
    public double ExpectedFalsePositiveRate { get; set; }
    public double ExpectedTruePositiveRate { get; set; }
    public string OptimizationMethod { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// 外れ値検出結果
/// </summary>
public class OutlierDetectionResult
{
    public List<OutlierDataPoint> Outliers { get; set; } = new();
    public double UpperBound { get; set; }
    public double LowerBound { get; set; }
    public OutlierDetectionMethod Method { get; set; }
    public int TotalSamples { get; set; }
    public int OutlierCount { get; set; }
    public double OutlierPercentage { get; set; }
}

/// <summary>
/// 外れ値データポイント
/// </summary>
public class OutlierDataPoint
{
    public int Index { get; set; }
    public double Value { get; set; }
    public DateTime? Timestamp { get; set; }
    public double DeviationScore { get; set; } // Z-score or IQR score
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 外れ値検出手法
/// </summary>
public enum OutlierDetectionMethod
{
    /// <summary>
    /// 四分位範囲（Interquartile Range）
    /// </summary>
    IQR,

    /// <summary>
    /// Z-score（標準偏差ベース）
    /// </summary>
    ZScore,

    /// <summary>
    /// Modified Z-score（中央値絶対偏差ベース）
    /// </summary>
    ModifiedZScore,

    /// <summary>
    /// 移動平均ベース
    /// </summary>
    MovingAverage
}

/// <summary>
/// 動的閾値計算結果
/// </summary>
public class DynamicThresholdResult
{
    public List<TimeSeriesThreshold> Thresholds { get; set; } = new();
    public TrendAnalysis TrendInfo { get; set; } = new();
    public double BaselineValue { get; set; }
    public int WindowSize { get; set; }
}

/// <summary>
/// 時系列閾値
/// </summary>
public class TimeSeriesThreshold
{
    public DateTime Timestamp { get; set; }
    public double UpperThreshold { get; set; }
    public double LowerThreshold { get; set; }
    public double PredictedValue { get; set; }
    public double ConfidenceInterval { get; set; }
}

/// <summary>
/// トレンド分析結果
/// </summary>
public class TrendAnalysis
{
    public TrendDirection Direction { get; set; }
    public double Slope { get; set; } // 傾き
    public double Volatility { get; set; } // ボラティリティ（標準偏差）
    public bool IsStationary { get; set; } // 定常性
    public double AutocorrelationLag1 { get; set; } // 自己相関（lag=1）
}

public enum TrendDirection
{
    Increasing,
    Decreasing,
    Stable,
    Oscillating
}

/// <summary>
/// 多変量閾値最適化結果
/// </summary>
public class MultivariateThresholdResult
{
    public Dictionary<string, OptimalThresholdResult> SignalThresholds { get; set; } = new();
    public List<SignalCorrelation> Correlations { get; set; } = new();
    public List<string> RecommendedSignalGroups { get; set; } = new();
}

/// <summary>
/// 信号相関情報
/// </summary>
public class SignalCorrelation
{
    public string Signal1 { get; set; } = string.Empty;
    public string Signal2 { get; set; } = string.Empty;
    public double CorrelationCoefficient { get; set; }
    public bool IsSignificant { get; set; }
}

/// <summary>
/// 時系列データポイント
/// </summary>
public class TimeSeriesDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
