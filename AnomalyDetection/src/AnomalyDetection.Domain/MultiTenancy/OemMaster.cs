using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace AnomalyDetection.MultiTenancy;

public class OemMaster : FullAuditedAggregateRoot<Guid>
{
    public OemCode OemCode { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string ContactEmail { get; private set; } = string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime? EstablishedDate { get; private set; }
    public string Description { get; private set; } = string.Empty;

    // Features and capabilities
    private readonly List<OemFeature> _features = new();
    public IReadOnlyList<OemFeature> Features => _features.AsReadOnly();

    protected OemMaster() { }

    public OemMaster(
        Guid id,
        OemCode oemCode,
        string companyName,
        string country,
        string? contactEmail = null,
        string? contactPhone = null,
        DateTime? establishedDate = null,
        string? description = null) : base(id)
    {
        OemCode = oemCode ?? throw new ArgumentNullException(nameof(oemCode));
        CompanyName = ValidateCompanyName(companyName);
        Country = ValidateCountry(country);
        ContactEmail = contactEmail ?? string.Empty;
        ContactPhone = contactPhone ?? string.Empty;
        EstablishedDate = establishedDate;
        Description = description ?? string.Empty;
        IsActive = true;
    }

    public void UpdateBasicInfo(
        string companyName,
        string country,
        string? contactEmail = null,
        string? contactPhone = null,
        string? description = null)
    {
        CompanyName = ValidateCompanyName(companyName);
        Country = ValidateCountry(country);
        ContactEmail = contactEmail ?? string.Empty;
        ContactPhone = contactPhone ?? string.Empty;
        Description = description ?? string.Empty;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void AddFeature(string featureName, string featureValue, bool isEnabled = true)
    {
        if (_features.Exists(f => f.FeatureName == featureName))
            throw new InvalidOperationException($"Feature '{featureName}' already exists");

        _features.Add(new OemFeature(featureName, featureValue, isEnabled));
    }

    public void UpdateFeature(string featureName, string featureValue, bool isEnabled)
    {
        var feature = _features.Find(f => f.FeatureName == featureName);
        if (feature == null)
            throw new InvalidOperationException($"Feature '{featureName}' not found");

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

    private static string ValidateCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be null or empty", nameof(companyName));

        if (companyName.Length > 200)
            throw new ArgumentException("Company name cannot exceed 200 characters", nameof(companyName));

        return companyName.Trim();
    }

    private static string ValidateCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        if (country.Length > 100)
            throw new ArgumentException("Country cannot exceed 100 characters", nameof(country));

        return country.Trim();
    }
}