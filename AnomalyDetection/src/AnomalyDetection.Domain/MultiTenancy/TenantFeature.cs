using System;

namespace AnomalyDetection.MultiTenancy;

// Owned type - should not inherit from Entity
public class TenantFeature
{
    public string FeatureName { get; private set; } = string.Empty;
    public string FeatureValue { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    protected TenantFeature() { }

    public TenantFeature(string featureName, string featureValue, bool isEnabled = true, string? createdBy = null)
    {
        FeatureName = ValidateFeatureName(featureName);
        FeatureValue = featureValue ?? string.Empty;
        IsEnabled = isEnabled;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void Update(string featureValue, bool isEnabled, string? updatedBy = null)
    {
        FeatureValue = featureValue ?? string.Empty;
        IsEnabled = isEnabled;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Enable(string? updatedBy = null)
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Disable(string? updatedBy = null)
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    private static string ValidateFeatureName(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
            throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));
            
        if (featureName.Length > 100)
            throw new ArgumentException("Feature name cannot exceed 100 characters", nameof(featureName));
            
        return featureName.Trim();
    }
}

// 定義済みのテナント機能
public static class TenantFeatureNames
{
    public const string MaxCanSignals = "MaxCanSignals";
    public const string MaxDetectionLogics = "MaxDetectionLogics";
    public const string MaxUsers = "MaxUsers";
    public const string EnableAdvancedAnalytics = "EnableAdvancedAnalytics";
    public const string EnableDataSharing = "EnableDataSharing";
    public const string EnableApiAccess = "EnableApiAccess";
    public const string StorageQuotaGB = "StorageQuotaGB";
    public const string EnableRealtimeDetection = "EnableRealtimeDetection";
    public const string MaxVehiclePhases = "MaxVehiclePhases";
    public const string EnableCrossOemAnalysis = "EnableCrossOemAnalysis";
}