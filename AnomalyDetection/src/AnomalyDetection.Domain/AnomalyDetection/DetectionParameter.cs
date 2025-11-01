using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

public class DetectionParameter : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public ParameterDataType DataType { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string DefaultValue { get; private set; } = string.Empty;
    public ParameterConstraints Constraints { get; private set; } = new ParameterConstraints();
    public string Description { get; private set; } = string.Empty;
    public bool IsRequired { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected DetectionParameter() { }

    public DetectionParameter(
        string name,
        ParameterDataType dataType,
        string? defaultValue = null,
        ParameterConstraints? constraints = null,
        string? description = null,
        bool isRequired = false,
        string? unit = null)
    {
        Id = Guid.NewGuid();
        Name = ValidateName(name);
        DataType = dataType;
        DefaultValue = defaultValue ?? string.Empty;
        Value = defaultValue ?? string.Empty; // 初期値はデフォルト値
        Constraints = constraints ?? new ParameterConstraints();
        Description = ValidateDescription(description);
        IsRequired = isRequired;
        Unit = ValidateUnit(unit);
        CreatedAt = DateTime.UtcNow;

        ValidateDefaultValue();
    }

    public void UpdateValue(string newValue)
    {
        ValidateValue(newValue);
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDefaultValue(string newDefaultValue)
    {
        ValidateValue(newDefaultValue);
        DefaultValue = newDefaultValue;

        // 現在の値がnullまたは空の場合、新しいデフォルト値を設定
        if (string.IsNullOrEmpty(Value))
        {
            Value = newDefaultValue;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateConstraints(ParameterConstraints newConstraints)
    {
        Constraints = newConstraints;

        // 現在の値が新しい制約に適合するかチェック
        if (!string.IsNullOrEmpty(Value))
        {
            ValidateValue(Value);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetToDefault()
    {
        Value = DefaultValue;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasValue()
    {
        return !string.IsNullOrEmpty(Value);
    }

    public bool IsValid()
    {
        if (IsRequired && string.IsNullOrEmpty(Value))
            return false;

        if (!string.IsNullOrEmpty(Value))
        {
            try
            {
                ValidateValue(Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    public T? GetTypedValue<T>()
    {
        if (string.IsNullOrEmpty(Value))
        {
            if (IsRequired)
                throw new InvalidOperationException($"Required parameter '{Name}' has no value");
            return default(T);
        }

        return DataType switch
        {
            ParameterDataType.Integer => (T)(object)int.Parse(Value),
            ParameterDataType.Double => (T)(object)double.Parse(Value),
            ParameterDataType.Boolean => (T)(object)bool.Parse(Value),
            ParameterDataType.String => (T)(object)Value,
            ParameterDataType.DateTime => (T)(object)DateTime.Parse(Value),
            ParameterDataType.Json => (T)(object)Value,
            _ => throw new NotSupportedException($"Data type {DataType} is not supported")
        };
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Parameter name cannot exceed 100 characters", nameof(name));

        // パラメータ名の命名規則チェック
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
            throw new ArgumentException("Parameter name must start with a letter and contain only alphanumeric characters and underscores", nameof(name));

        return name.Trim();
    }

    private static string ValidateDescription(string? description)
    {
        if (description != null && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        return description?.Trim() ?? string.Empty;
    }

    private static string ValidateUnit(string? unit)
    {
        if (unit != null && unit.Length > 20)
            throw new ArgumentException("Unit cannot exceed 20 characters", nameof(unit));

        return unit?.Trim() ?? string.Empty;
    }
    private void ValidateDefaultValue()
    {
        if (!string.IsNullOrEmpty(DefaultValue))
        {
            ValidateValue(DefaultValue);
        }
    }

    private void ValidateValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        // データ型による検証
        switch (DataType)
        {
            case ParameterDataType.Integer:
                if (!int.TryParse(value, out var intValue))
                    throw new ArgumentException($"Value '{value}' is not a valid integer");
                if (Constraints != null)
                    Constraints.ValidateInteger(intValue);
                break;

            case ParameterDataType.Double:
                if (!double.TryParse(value, out var doubleValue))
                    throw new ArgumentException($"Value '{value}' is not a valid double");
                if (Constraints != null)
                    Constraints.ValidateDouble(doubleValue);
                break;

            case ParameterDataType.Boolean:
                if (!bool.TryParse(value, out _))
                    throw new ArgumentException($"Value '{value}' is not a valid boolean");
                break;

            case ParameterDataType.String:
                if (Constraints != null)
                    Constraints.ValidateString(value);
                break;

            case ParameterDataType.DateTime:
                if (!DateTime.TryParse(value, out _))
                    throw new ArgumentException($"Value '{value}' is not a valid datetime");
                break;

            case ParameterDataType.Json:
                // Basic JSON validation could be added here
                if (Constraints != null)
                    Constraints.ValidateString(value);
                break;
        }
    }
}

public class ParameterConstraints : ValueObject
{
    public double? MinValue { get; private set; }
    public double? MaxValue { get; private set; }
    public int? MinLength { get; private set; }
    public int? MaxLength { get; private set; }
    public string Pattern { get; private set; } = string.Empty;
    public List<string> AllowedValues { get; private set; } = new List<string>();

    protected ParameterConstraints() { }

    public ParameterConstraints(
        double? minValue = null,
        double? maxValue = null,
        int? minLength = null,
        int? maxLength = null,
        string? pattern = null,
        List<string>? allowedValues = null)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        MinLength = minLength;
        MaxLength = maxLength;
        Pattern = pattern ?? string.Empty;
        AllowedValues = allowedValues ?? new List<string>();

        ValidateConstraints();
    }

    public void ValidateInteger(int value)
    {
        if (MinValue.HasValue && value < MinValue.Value)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is below minimum {MinValue.Value}");

        if (MaxValue.HasValue && value > MaxValue.Value)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is above maximum {MaxValue.Value}");
    }

    public void ValidateDouble(double value)
    {
        if (MinValue.HasValue && value < MinValue.Value)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is below minimum {MinValue.Value}");

        if (MaxValue.HasValue && value > MaxValue.Value)
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is above maximum {MaxValue.Value}");
    }

    public void ValidateString(string value)
    {
        if (MinLength.HasValue && value.Length < MinLength.Value)
            throw new ArgumentException($"String length {value.Length} is below minimum {MinLength.Value}");

        if (MaxLength.HasValue && value.Length > MaxLength.Value)
            throw new ArgumentException($"String length {value.Length} is above maximum {MaxLength.Value}");

        if (!string.IsNullOrEmpty(Pattern) && !System.Text.RegularExpressions.Regex.IsMatch(value, Pattern))
            throw new ArgumentException($"String '{value}' does not match required pattern '{Pattern}'");
    }

    public void ValidateEnum(string value)
    {
        if (AllowedValues.Any() && !AllowedValues.Contains(value))
            throw new ArgumentException($"Value '{value}' is not in allowed values: {string.Join(", ", AllowedValues)}");
    }

    private void ValidateConstraints()
    {
        if (MinValue.HasValue && MaxValue.HasValue && MinValue.Value > MaxValue.Value)
            throw new ArgumentException("MinValue cannot be greater than MaxValue");

        if (MinLength.HasValue && MaxLength.HasValue && MinLength.Value > MaxLength.Value)
            throw new ArgumentException("MinLength cannot be greater than MaxLength");

        if (MinLength.HasValue && MinLength.Value < 0)
            throw new ArgumentException("MinLength cannot be negative");

        if (MaxLength.HasValue && MaxLength.Value < 0)
            throw new ArgumentException("MaxLength cannot be negative");
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return MinValue ?? 0;
        yield return MaxValue ?? 0;
        yield return MinLength ?? 0;
        yield return MaxLength ?? 0;
        yield return Pattern ?? string.Empty;
        yield return string.Join(",", AllowedValues);
    }
}

