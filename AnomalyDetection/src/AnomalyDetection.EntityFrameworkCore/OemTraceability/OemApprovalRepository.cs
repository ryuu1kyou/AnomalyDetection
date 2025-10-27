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
/// OEM承認リポジトリ実装
/// </summary>
public class OemApprovalRepository : EfCoreRepository<AnomalyDetectionDbContext, OemApproval, Guid>, IOemApprovalRepository
{
    public OemApprovalRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<OemApproval>> GetByEntityAndOemAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(a => a.EntityId == entityId && 
                       a.EntityType == entityType && 
                       a.OemCode.Code == oemCode)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetByEntityAsync(
        Guid entityId, 
        string entityType, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(a => a.EntityId == entityId && a.EntityType == entityType)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetPendingApprovalsAsync(
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(a => a.OemCode.Code == oemCode && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<OemApproval?> GetLatestApprovalAsync(
        Guid entityId, 
        string entityType, 
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(a => a.EntityId == entityId && 
                       a.EntityType == entityType && 
                       a.OemCode.Code == oemCode)
            .OrderByDescending(a => a.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetByOemAsync(
        string oemCode, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(a => a.OemCode.Code == oemCode)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetByTypeAsync(
        ApprovalType approvalType, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(a => a.Type == approvalType);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(a => a.OemCode.Code == oemCode);
        }

        return await query
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetOverdueApprovalsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var now = DateTime.UtcNow;
        
        var query = dbSet.Where(a => a.Status == ApprovalStatus.Pending && 
                                    a.DueDate.HasValue && 
                                    a.DueDate.Value < now);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(a => a.OemCode.Code == oemCode);
        }

        return await query
            .OrderBy(a => a.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetUrgentApprovalsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var tomorrow = DateTime.UtcNow.AddDays(1);
        
        var query = dbSet.Where(a => a.Status == ApprovalStatus.Pending && 
                                    (a.Priority >= 4 || 
                                     (a.DueDate.HasValue && a.DueDate.Value <= tomorrow)));

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(a => a.OemCode.Code == oemCode);
        }

        return await query
            .OrderByDescending(a => a.Priority)
            .ThenBy(a => a.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemApproval>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(a => a.RequestedAt >= startDate && a.RequestedAt <= endDate);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(a => a.OemCode.Code == oemCode);
        }

        return await query
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<ApprovalStatus, int>> GetApprovalStatisticsAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(a => a.OemCode.Code == oemCode);
        }

        var statistics = await query
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statistics.ToDictionary(s => s.Status, s => s.Count);
    }

    public async Task<Dictionary<Guid, int>> GetApprovalsByApproverAsync(
        string? oemCode = null, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(a => a.ApprovedBy.HasValue);

        if (!string.IsNullOrEmpty(oemCode))
        {
            query = query.Where(a => a.OemCode.Code == oemCode);
        }

        var statistics = await query
            .GroupBy(a => a.ApprovedBy!.Value)
            .Select(g => new { ApproverId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statistics.ToDictionary(s => s.ApproverId, s => s.Count);
    }
}