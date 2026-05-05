using System;
using System.Collections.Generic;
using System.Linq;
using AnomalyDetection.AuditLogging;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.ChangeTracking;

/// <summary>
/// 複数エンティティにまたがる変更を FeatureId / DecisionId で束ねる「実装接続台帳」
/// </summary>
public class ChangeBundle : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    public string FeatureId { get; private set; } = string.Empty;
    public string? DecisionId { get; private set; }
    public AuditChangeType ChangeType { get; private set; }
    public string ChangeReason { get; private set; } = string.Empty;
    public DocSyncStatus DocSyncStatus { get; private set; } = DocSyncStatus.NotRequired;
    public string? DocVersion { get; private set; }

    private readonly List<ChangeBundleItem> _items = new();
    public IReadOnlyList<ChangeBundleItem> Items => _items.AsReadOnly();

    protected ChangeBundle() { }

    public ChangeBundle(
        Guid id,
        Guid? tenantId,
        string featureId,
        string changeReason,
        AuditChangeType changeType = AuditChangeType.NotApplicable,
        string? decisionId = null) : base(id)
    {
        TenantId = tenantId;
        FeatureId = Check.NotNullOrWhiteSpace(featureId, nameof(featureId));
        ChangeReason = Check.NotNullOrWhiteSpace(changeReason, nameof(changeReason));
        ChangeType = changeType;
        DecisionId = decisionId?.Trim();
    }

    public void AddItem(Guid entityId, string entityType)
    {
        Check.NotNullOrWhiteSpace(entityType, nameof(entityType));

        if (_items.Any(i => i.EntityId == entityId && i.EntityType == entityType))
            return;

        _items.Add(new ChangeBundleItem(Guid.NewGuid(), Id, entityId, entityType));
    }

    public void RemoveItem(Guid entityId, string entityType)
    {
        var item = _items.FirstOrDefault(i => i.EntityId == entityId && i.EntityType == entityType);
        if (item != null)
            _items.Remove(item);
    }

    public void UpdateDecision(string? decisionId)
    {
        DecisionId = decisionId?.Trim();
    }

    public void UpdateDocSync(DocSyncStatus status, string? docVersion = null)
    {
        DocSyncStatus = status;
        DocVersion = docVersion;
    }
}

public class ChangeBundleItem : Entity<Guid>
{
    public Guid ChangeBundleId { get; private set; }
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public DateTime LinkedAt { get; private set; }

    protected ChangeBundleItem() { }

    internal ChangeBundleItem(Guid id, Guid changeBundleId, Guid entityId, string entityType) : base(id)
    {
        ChangeBundleId = changeBundleId;
        EntityId = entityId;
        EntityType = entityType;
        LinkedAt = DateTime.UtcNow;
    }
}
