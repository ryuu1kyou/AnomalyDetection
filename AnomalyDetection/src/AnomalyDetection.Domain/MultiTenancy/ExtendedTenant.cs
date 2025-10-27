using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection.MultiTenancy;

public class ExtendedTenant : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public OemCode OemCode { get; private set; } = null!;
    public Guid? OemMasterId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ActivationDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? DatabaseConnectionString { get; private set; }
    public string? Description { get; private set; }
    
    // Tenant-specific features
    private readonly List<TenantFeature> _features = new();
    public IReadOnlyList<TenantFeature> Features => _features.AsReadOnly();

    protected ExtendedTenant() { }

    public ExtendedTenant(
        Guid id,
        string name,
        OemCode oemCode,
        Guid? oemMasterId = null,
        string? databaseConnectionString = null,
        string? description = null) : base(id)
    {
        TenantId = id;
        Name = ValidateName(name);
        OemCode = oemCode ?? throw new ArgumentNullException(nameof(oemCode));
        OemMasterId = oemMasterId;
        DatabaseConnectionString = databaseConnectionString;
        Description = description;
        IsActive = true;
        ActivationDate = DateTime.UtcNow;
    }

    public void UpdateBasicInfo(string name, string? description = null)
    {
        Name = ValidateName(name);
        Description = description;
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            ActivationDate = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetExpiration(DateTime? expirationDate)
    {
        ExpirationDate = expirationDate;
    }

    public void UpdateDatabaseConnectionString(string? connectionString)
    {
        DatabaseConnectionString = connectionString;
    }

    public void AddFeature(string featureName, string featureValue, bool isEnabled = true)
    {
        if (_features.Exists(f => f.FeatureName == featureName))
            throw new InvalidOperationException($"Feature '{featureName}' already exists for this tenant");

        _features.Add(new TenantFeature(featureName, featureValue, isEnabled));
    }

    public void UpdateFeature(string featureName, string featureValue, bool isEnabled)
    {
        var feature = _features.Find(f => f.FeatureName == featureName);
        if (feature == null)
            throw new InvalidOperationException($"Feature '{featureName}' not found for this tenant");

        feature.Update(featureValue, isEnabled);
    }

    public void RemoveFeature(string featureName)
    {
        var feature = _features.Find(f => f.FeatureName == featureName);
        if (feature != null)
        {
            _features.Remove(feature);
        }
    }

    public bool IsExpired()
    {
        return ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
    }

    public bool IsValidForUse()
    {
        return IsActive && !IsExpired();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be null or empty", nameof(name));
            
        if (name.Length > 100)
            throw new ArgumentException("Tenant name cannot exceed 100 characters", nameof(name));
            
        return name.Trim();
    }
}