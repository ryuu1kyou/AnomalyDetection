using System;

namespace AnomalyDetection.MultiTenancy;

// Owned type - should not inherit from Entity
public class OemFeature
{
    public string FeatureName { get; private set; } = string.Empty;
    public string FeatureValue { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected OemFeature() { }

    public OemFeature(string featureName, string featureValue, bool isEnabled = true)
    {
        FeatureName = ValidateFeatureName(featureName);
        FeatureValue = featureValue ?? string.Empty;
        IsEnabled = isEnabled;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string featureValue, bool isEnabled)
    {
        FeatureValue = featureValue ?? string.Empty;
        IsEnabled = isEnabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdatedAt = DateTime.UtcNow;
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