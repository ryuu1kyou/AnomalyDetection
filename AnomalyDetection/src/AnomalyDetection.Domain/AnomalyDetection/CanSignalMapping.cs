using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

public class CanSignalMapping : Entity
{
    public Guid CanSignalId { get; private set; }
    public string SignalRole { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
    public string? Description { get; private set; }
    public SignalMappingConfiguration Configuration { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected CanSignalMapping() { }

    public CanSignalMapping(
        Guid canSignalId,
        string signalRole,
        bool isRequired = true,
    string? description = null,
    SignalMappingConfiguration? configuration = null)
    {
        CanSignalId = canSignalId;
        SignalRole = ValidateSignalRole(signalRole);
        IsRequired = isRequired;
        Description = ValidateDescription(description);
        Configuration = configuration ?? new SignalMappingConfiguration();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(string newRole)
    {
        SignalRole = ValidateSignalRole(newRole);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? newDescription)
    {
        Description = ValidateDescription(newDescription);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConfiguration(SignalMappingConfiguration newConfiguration)
    {
        Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRequired(bool required)
    {
        IsRequired = required;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsPrimarySignal()
    {
        return SignalRole.Equals("Primary", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsReferenceSignal()
    {
        return SignalRole.Equals("Reference", StringComparison.OrdinalIgnoreCase);
    }

    private static string ValidateSignalRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Signal role cannot be null or empty", nameof(role));

        if (role.Length > 50)
            throw new ArgumentException("Signal role cannot exceed 50 characters", nameof(role));

        return role.Trim();
    }

    private static string? ValidateDescription(string? description)
    {
        if (description != null && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        return description?.Trim();
    }

    public override object[] GetKeys()
    {
        return new object[] { CanSignalId };
    }
}

public class SignalMappingConfiguration : ValueObject
{
    public double? ScalingFactor { get; private set; }
    public double? Offset { get; private set; }
    public string? FilterExpression { get; private set; }
    public Dictionary<string, object> CustomProperties { get; private set; } = new();

    protected SignalMappingConfiguration() { }

    public SignalMappingConfiguration(
        double? scalingFactor = null,
        double? offset = null,
        string? filterExpression = null,
        Dictionary<string, object>? customProperties = null)
    {
        ScalingFactor = scalingFactor;
        Offset = offset;
        FilterExpression = ValidateFilterExpression(filterExpression);
        CustomProperties = customProperties ?? new Dictionary<string, object>();
    }

    public double ApplyScaling(double value)
    {
        var scaledValue = value;

        if (ScalingFactor.HasValue)
        {
            scaledValue *= ScalingFactor.Value;
        }

        if (Offset.HasValue)
        {
            scaledValue += Offset.Value;
        }

        return scaledValue;
    }

    public bool HasScaling()
    {
        return ScalingFactor.HasValue || Offset.HasValue;
    }

    public bool HasFilter()
    {
        return !string.IsNullOrEmpty(FilterExpression);
    }

    public T GetCustomProperty<T>(string key)
    {
        if (CustomProperties.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default!;
    }

    public void SetCustomProperty(string key, object value)
    {
        CustomProperties[key] = value;
    }

    private static string? ValidateFilterExpression(string? expression)
    {
        if (expression != null && expression.Length > 1000)
            throw new ArgumentException("Filter expression cannot exceed 1000 characters", nameof(expression));

        return expression?.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ScalingFactor ?? 0;
        yield return Offset ?? 0;
        yield return FilterExpression ?? string.Empty;
        yield return string.Join(",", CustomProperties.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }
}

// 定義済みの信号ロール
public static class SignalRoles
{
    public const string Primary = "Primary";           // 主要監視対象信号
    public const string Reference = "Reference";       // 参照信号
    public const string Trigger = "Trigger";          // トリガー信号
    public const string Context = "Context";           // コンテキスト信号
    public const string Validation = "Validation";    // 検証信号
    public const string Correlation = "Correlation";  // 相関分析用信号
}