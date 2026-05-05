using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnomalyDetection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AnomalyDetection.ChangeTracking;

public class EfCoreChangeBundleRepository :
    EfCoreRepository<AnomalyDetectionDbContext, ChangeBundle, Guid>,
    IChangeBundleRepository
{
    public EfCoreChangeBundleRepository(IDbContextProvider<AnomalyDetectionDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<ChangeBundle>> GetListByFeatureIdAsync(
        string featureId,
        bool includeItems = true,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(x => x.FeatureId == featureId);
        if (includeItems)
            query = query.Include(x => x.Items);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<ChangeBundle>> GetListByDecisionIdAsync(
        string decisionId,
        bool includeItems = true,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(x => x.DecisionId == decisionId);
        if (includeItems)
            query = query.Include(x => x.Items);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<ChangeBundle?> GetWithItemsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
