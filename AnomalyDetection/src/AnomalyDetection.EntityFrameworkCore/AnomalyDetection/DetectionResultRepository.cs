using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using AnomalyDetection.EntityFrameworkCore;

namespace AnomalyDetection.AnomalyDetection;

/// <summary>
/// 異常検出結果リポジトリの実装
/// </summary>
public class DetectionResultRepository : EfCoreRepository<AnomalyDetectionDbContext, AnomalyDetectionResult, Guid>, IDetectionResultRepository
{
    public DetectionResultRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    /// <summary>
    /// 指定されたCAN信号と期間の検出結果を取得する
    /// </summary>
    public async Task<List<AnomalyDetectionResult>> GetByCanSignalAndPeriodAsync(
        Guid canSignalId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(r => r.CanSignalId == canSignalId &&
                       r.DetectedAt >= startDate &&
                       r.DetectedAt <= endDate)
            .OrderBy(r => r.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 指定された異常タイプの検出結果を取得する
    /// </summary>
    public async Task<List<AnomalyDetectionResult>> GetByAnomalyTypeAsync(
        AnomalyType anomalyType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => r.AnomalyType == anomalyType);

        if (startDate.HasValue)
            query = query.Where(r => r.DetectedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.DetectedAt <= endDate.Value);

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        return await query
            .OrderBy(r => r.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 検出結果の統計情報を取得する
    /// </summary>
    public async Task<DetectionResultStatistics> GetStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => r.DetectedAt >= startDate && r.DetectedAt <= endDate);

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        var results = await query.ToListAsync(cancellationToken);

        if (!results.Any())
        {
            return new DetectionResultStatistics(
                startDate, endDate, 0, 0, 0, 0, 0,
                new Dictionary<AnomalyType, int>(),
                new Dictionary<AnomalyLevel, int>(),
                new Dictionary<ResolutionStatus, int>(),
                0.0, 0.0);
        }

        var totalDetections = results.Count;
        var resolvedDetections = results.Count(r => r.IsResolved());
        var unresolvedDetections = totalDetections - resolvedDetections;
        var falsePositives = results.Count(r => r.IsFalsePositive());
        var validatedDetections = results.Count(r => r.IsValidated);

        var detectionsByType = results
            .GroupBy(r => r.AnomalyType)
            .ToDictionary(g => g.Key, g => g.Count());

        var detectionsByLevel = results
            .GroupBy(r => r.AnomalyLevel)
            .ToDictionary(g => g.Key, g => g.Count());

        var detectionsByStatus = results
            .GroupBy(r => r.ResolutionStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        var detectionTimes = results
            .Where(r => r.DetectionDuration.TotalMilliseconds > 0)
            .Select(r => r.DetectionDuration.TotalMilliseconds)
            .ToList();

        var averageDetectionTime = detectionTimes.Any() ? detectionTimes.Average() : 0.0;
        var medianDetectionTime = detectionTimes.Any() ? CalculateMedian(detectionTimes) : 0.0;

        return new DetectionResultStatistics(
            startDate, endDate, totalDetections, resolvedDetections, unresolvedDetections,
            falsePositives, validatedDetections, detectionsByType, detectionsByLevel,
            detectionsByStatus, averageDetectionTime, medianDetectionTime);
    }

    /// <summary>
    /// 指定された検出ロジックの検出結果を取得する
    /// </summary>
    public async Task<List<AnomalyDetectionResult>> GetByDetectionLogicAndPeriodAsync(
        Guid detectionLogicId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(r => r.DetectionLogicId == detectionLogicId &&
                       r.DetectedAt >= startDate &&
                       r.DetectedAt <= endDate)
            .OrderBy(r => r.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 異常レベル別の検出結果を取得する
    /// </summary>
    public async Task<List<AnomalyDetectionResult>> GetByAnomalyLevelAsync(
        AnomalyLevel anomalyLevel,
        DateTime startDate,
        DateTime endDate,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => r.AnomalyLevel == anomalyLevel &&
                                    r.DetectedAt >= startDate &&
                                    r.DetectedAt <= endDate);

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        return await query
            .OrderBy(r => r.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 未解決の検出結果を取得する
    /// </summary>
    public async Task<List<AnomalyDetectionResult>> GetUnresolvedAsync(
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => r.ResolutionStatus == ResolutionStatus.Open ||
                                    r.ResolutionStatus == ResolutionStatus.InProgress ||
                                    r.ResolutionStatus == ResolutionStatus.Reopened);

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        return await query
            .OrderByDescending(r => r.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 誤検出の検出結果を取得する
    /// </summary>
    public async Task<List<AnomalyDetectionResult>> GetFalsePositivesAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => (r.IsFalsePositiveFlag || r.ResolutionStatus == ResolutionStatus.FalsePositive) &&
                                    r.DetectedAt >= startDate &&
                                    r.DetectedAt <= endDate);

        if (tenantId.HasValue)
            query = query.Where(r => r.TenantId == tenantId.Value);

        return await query
            .OrderBy(r => r.DetectedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 検出時間の統計を取得する
    /// </summary>
    public async Task<DetectionTimeStatistics> GetDetectionTimeStatisticsAsync(
        Guid? detectionLogicId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => r.DetectedAt >= startDate &&
                                    r.DetectedAt <= endDate &&
                                    r.DetectionDuration.TotalMilliseconds > 0);

        if (detectionLogicId.HasValue)
            query = query.Where(r => r.DetectionLogicId == detectionLogicId.Value);

        var results = await query.ToListAsync(cancellationToken);

        if (!results.Any())
        {
            return new DetectionTimeStatistics(
                startDate, endDate, 0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                new Dictionary<AnomalyType, double>());
        }

        var detectionTimes = results
            .Select(r => r.DetectionDuration.TotalMilliseconds)
            .OrderBy(t => t)
            .ToList();

        var totalMeasurements = detectionTimes.Count;
        var averageTime = detectionTimes.Average();
        var medianTime = CalculateMedian(detectionTimes);
        var minTime = detectionTimes.Min();
        var maxTime = detectionTimes.Max();
        var standardDeviation = CalculateStandardDeviation(detectionTimes, averageTime);
        var percentile95 = CalculatePercentile(detectionTimes, 0.95);
        var percentile99 = CalculatePercentile(detectionTimes, 0.99);

        var averageTimeByType = results
            .GroupBy(r => r.AnomalyType)
            .ToDictionary(g => g.Key, g => g.Average(r => r.DetectionDuration.TotalMilliseconds));

        return new DetectionTimeStatistics(
            startDate, endDate, totalMeasurements, averageTime, medianTime,
            minTime, maxTime, standardDeviation, percentile95, percentile99,
            averageTimeByType);
    }

    #region Private Helper Methods

    private static double CalculateMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;
        
        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    private static double CalculateStandardDeviation(List<double> values, double mean)
    {
        if (values.Count <= 1)
            return 0.0;

        var sumOfSquaredDifferences = values.Sum(value => Math.Pow(value - mean, 2));
        return Math.Sqrt(sumOfSquaredDifferences / (values.Count - 1));
    }

    private static double CalculatePercentile(List<double> sortedValues, double percentile)
    {
        if (!sortedValues.Any())
            return 0.0;

        var index = percentile * (sortedValues.Count - 1);
        var lowerIndex = (int)Math.Floor(index);
        var upperIndex = (int)Math.Ceiling(index);

        if (lowerIndex == upperIndex)
            return sortedValues[lowerIndex];

        var lowerValue = sortedValues[lowerIndex];
        var upperValue = sortedValues[upperIndex];
        var weight = index - lowerIndex;

        return lowerValue + weight * (upperValue - lowerValue);
    }

    #endregion
}