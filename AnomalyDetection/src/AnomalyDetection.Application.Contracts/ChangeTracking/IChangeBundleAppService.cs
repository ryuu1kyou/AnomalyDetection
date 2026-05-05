using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.ChangeTracking.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.ChangeTracking;

public interface IChangeBundleAppService : IApplicationService
{
    Task<ChangeBundleDto> CreateAsync(CreateChangeBundleDto input);
    Task<ChangeBundleDto> GetAsync(Guid id);
    Task<List<ChangeBundleDto>> GetListByFeatureIdAsync(string featureId);
    Task<List<ChangeBundleDto>> GetListByDecisionIdAsync(string decisionId);
    Task<ChangeBundleDto> AddItemAsync(Guid id, AddChangeBundleItemDto input);
    Task<ChangeBundleDto> RemoveItemAsync(Guid id, Guid entityId, string entityType);
    Task<ChangeBundleDto> UpdateDocSyncAsync(Guid id, UpdateChangeBundleDocSyncDto input);
    Task DeleteAsync(Guid id);
}
