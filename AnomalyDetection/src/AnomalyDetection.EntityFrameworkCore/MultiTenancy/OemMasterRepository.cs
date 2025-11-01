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

public class OemMasterRepository : EfCoreRepository<AnomalyDetectionDbContext, OemMaster, Guid>, IOemMasterRepository
{
    public OemMasterRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<OemMaster?> FindByOemCodeAsync(string oemCode, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.OemCode.Code == oemCode, cancellationToken);
    }

    public async Task<List<OemMaster>> GetActiveOemsAsync(CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.OemCode.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OemMaster>> GetByCountryAsync(string country, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Country == country)
            .OrderBy(x => x.OemCode.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsOemCodeExistsAsync(string oemCode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(x => x.OemCode.Code == oemCode);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}