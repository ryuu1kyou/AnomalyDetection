using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.ChangeTracking.Dtos;
using AutoMapper;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.ChangeTracking;

public class ChangeBundleAppService : ApplicationService, IChangeBundleAppService
{
    private readonly IChangeBundleRepository _repository;
    private readonly IMapper _mapper;

    public ChangeBundleAppService(IChangeBundleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ChangeBundleDto> CreateAsync(CreateChangeBundleDto input)
    {
        var bundle = new ChangeBundle(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.FeatureId,
            input.ChangeReason,
            input.ChangeType,
            input.DecisionId);

        foreach (var item in input.Items)
            bundle.AddItem(item.EntityId, item.EntityType);

        await _repository.InsertAsync(bundle, autoSave: true);
        return _mapper.Map<ChangeBundle, ChangeBundleDto>(bundle);
    }

    public async Task<ChangeBundleDto> GetAsync(Guid id)
    {
        var bundle = await _repository.GetWithItemsAsync(id)
            ?? throw new EntityNotFoundException(typeof(ChangeBundle), id);
        return _mapper.Map<ChangeBundle, ChangeBundleDto>(bundle);
    }

    public async Task<List<ChangeBundleDto>> GetListByFeatureIdAsync(string featureId)
    {
        Check.NotNullOrWhiteSpace(featureId, nameof(featureId));
        var bundles = await _repository.GetListByFeatureIdAsync(featureId);
        return _mapper.Map<List<ChangeBundle>, List<ChangeBundleDto>>(bundles);
    }

    public async Task<List<ChangeBundleDto>> GetListByDecisionIdAsync(string decisionId)
    {
        Check.NotNullOrWhiteSpace(decisionId, nameof(decisionId));
        var bundles = await _repository.GetListByDecisionIdAsync(decisionId);
        return _mapper.Map<List<ChangeBundle>, List<ChangeBundleDto>>(bundles);
    }

    public async Task<ChangeBundleDto> AddItemAsync(Guid id, AddChangeBundleItemDto input)
    {
        var bundle = await _repository.GetWithItemsAsync(id)
            ?? throw new EntityNotFoundException(typeof(ChangeBundle), id);

        bundle.AddItem(input.EntityId, input.EntityType);
        await _repository.UpdateAsync(bundle, autoSave: true);
        return _mapper.Map<ChangeBundle, ChangeBundleDto>(bundle);
    }

    public async Task<ChangeBundleDto> RemoveItemAsync(Guid id, Guid entityId, string entityType)
    {
        var bundle = await _repository.GetWithItemsAsync(id)
            ?? throw new EntityNotFoundException(typeof(ChangeBundle), id);

        bundle.RemoveItem(entityId, entityType);
        await _repository.UpdateAsync(bundle, autoSave: true);
        return _mapper.Map<ChangeBundle, ChangeBundleDto>(bundle);
    }

    public async Task<ChangeBundleDto> UpdateDocSyncAsync(Guid id, UpdateChangeBundleDocSyncDto input)
    {
        var bundle = await _repository.GetWithItemsAsync(id)
            ?? throw new EntityNotFoundException(typeof(ChangeBundle), id);

        bundle.UpdateDocSync(input.DocSyncStatus, input.DocVersion);
        await _repository.UpdateAsync(bundle, autoSave: true);
        return _mapper.Map<ChangeBundle, ChangeBundleDto>(bundle);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }
}
