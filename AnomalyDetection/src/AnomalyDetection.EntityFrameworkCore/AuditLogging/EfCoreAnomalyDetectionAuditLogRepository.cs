using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnomalyDetection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AnomalyDetection.AuditLogging;

/// <summary>
/// 監査ログリポジトリのEntity Framework Core実装
/// </summary>
public class EfCoreAnomalyDetectionAuditLogRepository : 
    EfCoreRepository<AnomalyDetectionDbContext, AnomalyDetectionAuditLog, Guid>, 
    IAnomalyDetectionAuditLogRepository
{
    public EfCoreAnomalyDetectionAuditLogRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<AnomalyDetectionAuditLog>> GetByEntityIdAsync(
        Guid entityId, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.EntityId == entityId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AnomalyDetectionAuditLog>> GetByEntityTypeAsync(
        string entityType, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.EntityType == entityType)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AnomalyDetectionAuditLog>> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.CreatorId == userId)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AnomalyDetectionAuditLog>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.CreationTime >= startDate && x.CreationTime <= endDate)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AnomalyDetectionAuditLog>> GetByActionAsync(
        AuditLogAction action, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Action == action)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AnomalyDetectionAuditLog>> GetByLevelAsync(
        AuditLogLevel level, 
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Level == level)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);
    }
}