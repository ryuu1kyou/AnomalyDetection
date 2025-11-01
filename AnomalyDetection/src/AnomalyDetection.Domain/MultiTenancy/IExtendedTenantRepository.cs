using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.MultiTenancy;

public interface IExtendedTenantRepository : IRepository<ExtendedTenant, Guid>
{
    Task<ExtendedTenant?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<ExtendedTenant>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
    Task<List<ExtendedTenant>> GetByOemCodeAsync(string oemCode, CancellationToken cancellationToken = default);
    Task<bool> IsTenantNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<List<ExtendedTenant>> GetExpiredTenantsAsync(CancellationToken cancellationToken = default);
}