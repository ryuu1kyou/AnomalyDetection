using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnomalyDetection.EntityFrameworkCore;
using AnomalyDetection.OemTraceability;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AnomalyDetection.EntityFrameworkCore.OemTraceability;

/// <summary>
/// OEMカスタマイズリポジトリ実装
/// </summary>
public class OemCustomizationRepository : EfCoreRepository<AnomalyDetectionDbContext, OemCustomization, Guid>, IOemCustomizationRepository
{
    public OemCustomizationRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<OemCustomization>> GetByEntityAndOemAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.EntityId == entityId && 
                       c.EntityType == entityType && 
                       c.OemCode.Code == oemCode)
            .OrderByDescending(c => c.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemCustomization>> GetByOemAsync(
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.OemCode.Code == oemCode)
            .OrderByDescending(c => c.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemCustomization>> GetByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.EntityId == entityId && c.EntityType == entityType)
            .OrderByDescending(c => c.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<OemCustomization?> GetLatestCustomizationAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.EntityId == entityId && 
                       c.EntityType == entityType && 
                       c.OemCode.Code == oemCode)
            .OrderByDescending(c => c.CreationTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<OemCustomization>> GetPendingCustomizationsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(c => c.Status == CustomizationStatus.PendingApproval);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(c => c.OemCode.Code == oemCode);
        }

        return await query
            .OrderBy(c => c.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemCustomization>> GetByTypeAsync(
        CustomizationType customizationType, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(c => c.Type == customizationType);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(c => c.OemCode.Code == oemCode);
        }

        return await query
            .OrderByDescending(c => c.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemCustomization>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(c => c.CreationTime >= startDate && c.CreationTime <= endDate);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(c => c.OemCode.Code == oemCode);
        }

        return await query
            .OrderByDescending(c => c.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<CustomizationType, int>> GetCustomizationStatisticsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(c => c.OemCode.Code == oemCode);
        }

        var statistics = await query
            .GroupBy(c => c.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statistics.ToDictionary(s => s.Type, s => s.Count);
    }
}