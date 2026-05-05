using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.ChangeTracking;

public interface IChangeBundleRepository : IRepository<ChangeBundle, Guid>
{
    Task<List<ChangeBundle>> GetListByFeatureIdAsync(
        string featureId,
        bool includeItems = true,
        CancellationToken cancellationToken = default);

    Task<List<ChangeBundle>> GetListByDecisionIdAsync(
        string decisionId,
        bool includeItems = true,
        CancellationToken cancellationToken = default);

    Task<ChangeBundle?> GetWithItemsAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
