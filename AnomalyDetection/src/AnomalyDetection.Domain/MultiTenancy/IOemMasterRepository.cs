using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.MultiTenancy;

public interface IOemMasterRepository : IRepository<OemMaster, Guid>
{
    Task<OemMaster?> FindByOemCodeAsync(string oemCode, CancellationToken cancellationToken = default);
    Task<List<OemMaster>> GetActiveOemsAsync(CancellationToken cancellationToken = default);
    Task<List<OemMaster>> GetByCountryAsync(string country, CancellationToken cancellationToken = default);
    Task<bool> IsOemCodeExistsAsync(string oemCode, Guid? excludeId = null, CancellationToken cancellationToken = default);
}