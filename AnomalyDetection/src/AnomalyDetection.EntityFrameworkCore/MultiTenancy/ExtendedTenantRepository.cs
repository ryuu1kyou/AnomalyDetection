using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnomalyDetection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AnomalyDetection.MultiTenancy;

public class ExtendedTenantRepository : EfCoreRepository<AnomalyDetectionDbContext, ExtendedTenant, Guid>, IExtendedTenantRepository
{
    public ExtendedTenantRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<ExtendedTenant> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<List<ExtendedTenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExtendedTenant>> GetByOemCodeAsync(string oemCode, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.OemCode.Code == oemCode)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsTenantNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(x => x.Name == name);
        
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<ExtendedTenant>> GetExpiredTenantsAsync(CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var now = DateTime.UtcNow;
        
        return await dbSet
            .Where(x => x.ExpirationDate.HasValue && x.ExpirationDate.Value < now)
            .OrderBy(x => x.ExpirationDate)
            .ToListAsync(cancellationToken);
    }
}