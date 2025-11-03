using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using AnomalyDetection.AnomalyDetection;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.DetectionTemplates;

/// <summary>
/// Factory for creating detection logic templates
/// </summary>
public class DetectionTemplateFactory : DomainService
{
    public enum TemplateType
    {
        OutOfRange = 1,
        RateOfChange = 2,
        Timeout = 3,
        StuckValue = 4
    }

    public List<TemplateInfo> GetAvailableTemplates()
    {
        return new List<TemplateInfo>
        {
            BuildTemplate(
                TemplateType.OutOfRange,
                "Out of Range",
                "Detects when signal value exceeds configured thresholds"),
            BuildTemplate(
                TemplateType.RateOfChange,
                "Rate of Change",
                "Detects abnormal acceleration or deceleration in signal value"),
            BuildTemplate(
                TemplateType.Timeout,
                "Timeout",
                "Detects missing or delayed signal updates"),
            BuildTemplate(
                TemplateType.StuckValue,
                "Stuck Value",
                "Detects when signal values stop changing beyond tolerated delta")
        };
    }

    public bool ValidateTemplateParameters(TemplateType templateType, Dictionary<string, object> parameters, out List<string> errors)
    {
        errors = new List<string>();

        if (!TemplateParameterRules.TryGetValue(templateType, out var rules))
        {
            errors.Add($"Validation rules are not defined for template type '{templateType}'.");
            return false;
        }

        var inputParameters = parameters ?? new Dictionary<string, object>();
        var lookup = inputParameters.ToDictionary(
            kvp => kvp.Key,
            kvp => (object?)kvp.Value,
            StringComparer.OrdinalIgnoreCase);

        var numericValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var integerValues = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var booleanValues = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var stringValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in rules)
        {
            if (!TryResolveParameter(lookup, rule, out var rawValue))
            {
                if (rule.Required)
                {
                    errors.Add($"Missing required parameter '{rule.Name}'.");
                }
                continue;
            }

            if (rawValue == null)
            {
                errors.Add($"Parameter '{rule.Name}' cannot be null.");
                continue;
            }

            switch (rule.ValueType)
            {
                case ParameterValueType.Number:
                    if (!TryConvertToDouble(rawValue, out var doubleValue))
                    {
                        errors.Add($"Parameter '{rule.Name}' must be a numeric value.");
                        break;
                    }

                    if (rule.Min.HasValue && doubleValue < rule.Min.Value)
                    {
                        errors.Add($"Parameter '{rule.Name}' must be greater than or equal to {rule.Min.Value}.");
                    }

                    if (rule.Max.HasValue && doubleValue > rule.Max.Value)
                    {
                        errors.Add($"Parameter '{rule.Name}' must be less than or equal to {rule.Max.Value}.");
                    }

                    numericValues[rule.Name] = doubleValue;
                    break;

                case ParameterValueType.Integer:
                    if (!TryConvertToInt(rawValue, out var intValue))
                    {
                        errors.Add($"Parameter '{rule.Name}' must be an integer value.");
                        break;
                    }

                    if (rule.MinInt.HasValue && intValue < rule.MinInt.Value)
                    {
                        errors.Add($"Parameter '{rule.Name}' must be greater than or equal to {rule.MinInt.Value}.");
                    }

                    if (rule.MaxInt.HasValue && intValue > rule.MaxInt.Value)
                    {
                        errors.Add($"Parameter '{rule.Name}' must be less than or equal to {rule.MaxInt.Value}.");
                    }

                    integerValues[rule.Name] = intValue;
                    break;

                case ParameterValueType.Boolean:
                    if (!TryConvertToBool(rawValue, out var boolValue))
                    {
                        errors.Add($"Parameter '{rule.Name}' must be a boolean value.");
                        break;
                    }

                    booleanValues[rule.Name] = boolValue;
                    break;

                case ParameterValueType.String:
                    if (!TryConvertToString(rawValue, out var stringValue))
                    {
                        errors.Add($"Parameter '{rule.Name}' must be a string value.");
                        break;
                    }

                    if (rule.MinLength.HasValue && stringValue.Length < rule.MinLength.Value)
                    {
                        errors.Add($"Parameter '{rule.Name}' must have length greater than or equal to {rule.MinLength.Value}.");
                    }

                    if (rule.MaxLength.HasValue && stringValue.Length > rule.MaxLength.Value)
                    {
                        errors.Add($"Parameter '{rule.Name}' must have length less than or equal to {rule.MaxLength.Value}.");
                    }

                    if (rule.AllowedValues.Count > 0 &&
                        !rule.AllowedValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
                    {
                        errors.Add($"Parameter '{rule.Name}' must be one of: {string.Join(", ", rule.AllowedValues)}.");
                    }

                    stringValues[rule.Name] = stringValue;
                    break;
            }
        }

        ApplyTemplateSpecificValidation(templateType, numericValues, integerValues, booleanValues, stringValues, errors);

        return errors.Count == 0;
    }

    private static void ApplyTemplateSpecificValidation(
        TemplateType templateType,
        IDictionary<string, double> numericValues,
        IDictionary<string, int> integerValues,
        IDictionary<string, bool> booleanValues,
        IDictionary<string, string> stringValues,
        List<string> errors)
    {
        switch (templateType)
        {
            case TemplateType.OutOfRange:
                if (numericValues.TryGetValue("MinThreshold", out var minThreshold) &&
                    numericValues.TryGetValue("MaxThreshold", out var maxThreshold) &&
                    minThreshold >= maxThreshold)
                {
                    errors.Add("MinThreshold must be less than MaxThreshold.");
                }

                if (numericValues.TryGetValue("WindowSeconds", out var windowSeconds) && windowSeconds <= 0)
                {
                    errors.Add("WindowSeconds must be greater than zero when provided.");
                }
                break;

            case TemplateType.RateOfChange:
                if (numericValues.TryGetValue("RateThreshold", out var rateThreshold) && rateThreshold <= 0)
                {
                    errors.Add("RateThreshold must be greater than zero.");
                }

                if (numericValues.TryGetValue("WindowSeconds", out var rocWindow) && rocWindow <= 0)
                {
                    errors.Add("WindowSeconds must be greater than zero.");
                }

                if (integerValues.TryGetValue("SampleCount", out var sampleCount) && sampleCount < 2)
                {
                    errors.Add("SampleCount must be at least 2 to evaluate rate of change.");
                }
                break;

            case TemplateType.Timeout:
                if (numericValues.TryGetValue("TimeoutSeconds", out var timeout) && timeout <= 0)
                {
                    errors.Add("TimeoutSeconds must be greater than zero.");
                }

                if (numericValues.TryGetValue("GracePeriodSeconds", out var gracePeriod) && gracePeriod < 0)
                {
                    errors.Add("GracePeriodSeconds cannot be negative.");
                }

                if (integerValues.TryGetValue("AllowedMissCount", out var missCount) && missCount < 0)
                {
                    errors.Add("AllowedMissCount cannot be negative.");
                }
                break;

            case TemplateType.StuckValue:
                if (numericValues.TryGetValue("WindowSeconds", out var stuckWindow) && stuckWindow <= 0)
                {
                    errors.Add("WindowSeconds must be greater than zero.");
                }

                if (numericValues.TryGetValue("Tolerance", out var tolerance) && tolerance < 0)
                {
                    errors.Add("Tolerance cannot be negative.");
                }

                if (numericValues.TryGetValue("MinDelta", out var minDelta) && minDelta <= 0)
                {
                    errors.Add("MinDelta must be greater than zero when provided.");
                }

                if (integerValues.TryGetValue("SampleCount", out var stuckSamples) && stuckSamples < 1)
                {
                    errors.Add("SampleCount must be at least 1 for stuck value detection.");
                }
                break;
        }
    }

    private static bool TryResolveParameter(Dictionary<string, object?> parameters, ParameterRule rule, out object? value)
    {
        foreach (var key in rule.LookupKeys)
        {
            if (parameters.TryGetValue(key, out value))
            {
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool TryConvertToDouble(object? value, out double result)
    {
        switch (value)
        {
            case double d:
                result = d;
                return true;
            case float f:
                result = f;
                return true;
            case decimal m:
                result = (double)m;
                return true;
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetDouble(out var jsonDouble):
                result = jsonDouble;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String && double.TryParse(jsonElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var jsonParsedDouble):
                result = jsonParsedDouble;
                return true;
            case string stringValue when double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
            case null:
                result = 0;
                return false;
            default:
                if (value is IConvertible convertible)
                {
                    try
                    {
                        result = convertible.ToDouble(CultureInfo.InvariantCulture);
                        return true;
                    }
                    catch
                    {
                        // Conversion failed, fall through.
                    }
                }

                result = 0;
                return false;
        }
    }

    private static bool TryConvertToInt(object? value, out int result)
    {
        switch (value)
        {
            case int i:
                result = i;
                return true;
            case long l when l is >= int.MinValue and <= int.MaxValue:
                result = (int)l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var jsonInt):
                result = jsonInt;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String && int.TryParse(jsonElement.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var jsonParsedInt):
                result = jsonParsedInt;
                return true;
            case string stringValue when int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed):
                result = parsed;
                return true;
            case null:
                result = 0;
                return false;
            default:
                if (value is IConvertible convertible)
                {
                    try
                    {
                        result = convertible.ToInt32(CultureInfo.InvariantCulture);
                        return true;
                    }
                    catch
                    {
                        // Conversion failed, fall through.
                    }
                }

                result = 0;
                return false;
        }
    }

    private static bool TryConvertToBool(object? value, out bool result)
    {
        switch (value)
        {
            case bool b:
                result = b;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.True:
                result = true;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.False:
                result = false;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String && bool.TryParse(jsonElement.GetString(), out var jsonParsedBool):
                result = jsonParsedBool;
                return true;
            case string stringValue when bool.TryParse(stringValue, out var parsed):
                result = parsed;
                return true;
            default:
                result = false;
                return false;
        }
    }

    private static bool TryConvertToString(object? value, out string result)
    {
        switch (value)
        {
            case string s:
                result = s;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.String:
                result = jsonElement.GetString() ?? string.Empty;
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind is JsonValueKind.True or JsonValueKind.False:
                result = jsonElement.GetBoolean().ToString();
                return true;
            case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetDouble(out var jsonDouble):
                result = jsonDouble.ToString(CultureInfo.InvariantCulture);
                return true;
            case null:
                result = string.Empty;
                return false;
            default:
                if (value is IFormattable formattable)
                {
                    result = formattable.ToString(null, CultureInfo.InvariantCulture);
                    return true;
                }

                result = value.ToString() ?? string.Empty;
                return true;
        }
    }

    private TemplateInfo BuildTemplate(TemplateType templateType, string name, string description)
    {
        TemplateParameterRules.TryGetValue(templateType, out var rules);
        rules ??= new List<ParameterRule>();

        return new TemplateInfo
        {
            Type = templateType,
            Name = name,
            Description = description,
            DetectionType = MapToDetectionType(templateType),
            DefaultParameters = BuildDefaultParameters(rules),
            ParameterDefinitions = BuildParameterDefinitions(rules)
        };
    }

    private static DetectionType MapToDetectionType(TemplateType templateType)
    {
        return templateType switch
        {
            TemplateType.OutOfRange => DetectionType.OutOfRange,
            TemplateType.RateOfChange => DetectionType.RateOfChange,
            TemplateType.Timeout => DetectionType.Timeout,
            TemplateType.StuckValue => DetectionType.Stuck,
            _ => DetectionType.Custom
        };
    }

    private static Dictionary<string, object> BuildDefaultParameters(IEnumerable<ParameterRule> rules)
    {
        var defaults = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in rules)
        {
            if (rule.DefaultValue != null)
            {
                defaults[rule.Name] = rule.DefaultValue;
            }
        }

        return defaults;
    }

    private static List<TemplateParameterDefinition> BuildParameterDefinitions(IEnumerable<ParameterRule> rules)
    {
        return rules.Select(rule => new TemplateParameterDefinition
        {
            Name = rule.Name,
            Type = MapValueTypeToString(rule.ValueType),
            Description = rule.Description,
            Required = rule.Required,
            DefaultValue = rule.DefaultValue,
            MinValue = rule.Min ?? (object?)rule.MinInt,
            MaxValue = rule.Max ?? (object?)rule.MaxInt,
            MinLength = rule.MinLength,
            MaxLength = rule.MaxLength,
            AllowedValues = rule.AllowedValues.ToList()
        }).ToList();
    }

    private static string MapValueTypeToString(ParameterValueType valueType)
    {
        return valueType switch
        {
            ParameterValueType.Number => "number",
            ParameterValueType.Integer => "integer",
            ParameterValueType.Boolean => "boolean",
            ParameterValueType.String => "string",
            _ => "string"
        };
    }

    private static readonly Dictionary<TemplateType, List<ParameterRule>> TemplateParameterRules = new()
    {
        [TemplateType.OutOfRange] = new List<ParameterRule>
        {
            new ParameterRule(
                name: "MinThreshold",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Minimum acceptable signal value before an anomaly is raised.",
                aliases: new[] { "LowerBound", "MinValue", "Min" }),
            new ParameterRule(
                name: "MaxThreshold",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Maximum acceptable signal value before an anomaly is raised.",
                aliases: new[] { "UpperBound", "MaxValue", "Max" }),
            new ParameterRule(
                name: "Hysteresis",
                valueType: ParameterValueType.Number,
                required: false,
                description: "Optional hysteresis band to avoid rapid toggling around thresholds.",
                defaultValue: 0d,
                min: 0,
                aliases: new[] { "Deadband" }),
            new ParameterRule(
                name: "WindowSeconds",
                valueType: ParameterValueType.Number,
                required: false,
                description: "Window length in seconds for smoothing or aggregation.",
                min: 0,
                aliases: new[] { "WindowSize", "Window" })
        },
        [TemplateType.RateOfChange] = new List<ParameterRule>
        {
            new ParameterRule(
                name: "RateThreshold",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Minimum absolute rate of change considered abnormal.",
                min: 0,
                aliases: new[] { "Threshold", "MaxDelta" }),
            new ParameterRule(
                name: "WindowSeconds",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Window length in seconds used to compute rate of change.",
                min: 0,
                aliases: new[] { "WindowSize", "Window" }),
            new ParameterRule(
                name: "SampleCount",
                valueType: ParameterValueType.Integer,
                required: false,
                description: "Number of samples required for rate evaluation.",
                defaultValue: 2,
                minInt: 1,
                aliases: new[] { "SampleWindow" })
        },
        [TemplateType.Timeout] = new List<ParameterRule>
        {
            new ParameterRule(
                name: "TimeoutSeconds",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Maximum allowed interval between signal updates.",
                min: 0,
                aliases: new[] { "Timeout", "TimeoutMs" }),
            new ParameterRule(
                name: "GracePeriodSeconds",
                valueType: ParameterValueType.Number,
                required: false,
                description: "Additional grace period before alerting on timeout.",
                defaultValue: 0d,
                min: 0,
                aliases: new[] { "GracePeriod" }),
            new ParameterRule(
                name: "AllowedMissCount",
                valueType: ParameterValueType.Integer,
                required: false,
                description: "Number of consecutive misses allowed before triggering.",
                defaultValue: 0,
                minInt: 0,
                aliases: new[] { "AllowedMisses", "MaxMissCount" })
        },
        [TemplateType.StuckValue] = new List<ParameterRule>
        {
            new ParameterRule(
                name: "WindowSeconds",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Window length in seconds used to evaluate stuck condition.",
                min: 0,
                aliases: new[] { "WindowSize", "Window" }),
            new ParameterRule(
                name: "Tolerance",
                valueType: ParameterValueType.Number,
                required: true,
                description: "Maximum absolute change allowed before signal is considered stuck.",
                min: 0,
                aliases: new[] { "DeltaTolerance" }),
            new ParameterRule(
                name: "MinDelta",
                valueType: ParameterValueType.Number,
                required: false,
                description: "Minimum expected delta when signal behaves normally.",
                min: 0,
                aliases: new[] { "MinChange", "DeltaThreshold" }),
            new ParameterRule(
                name: "SampleCount",
                valueType: ParameterValueType.Integer,
                required: false,
                description: "Number of samples evaluated for stuck detection.",
                defaultValue: 3,
                minInt: 1,
                aliases: new[] { "SampleWindow" })
        }
    };

    private enum ParameterValueType
    {
        Number,
        Integer,
        Boolean,
        String
    }

    private sealed class ParameterRule
    {
        public ParameterRule(
            string name,
            ParameterValueType valueType,
            bool required,
            string description,
            object? defaultValue = null,
            double? min = null,
            double? max = null,
            int? minInt = null,
            int? maxInt = null,
            string[]? aliases = null,
            int? minLength = null,
            int? maxLength = null,
            IEnumerable<string>? allowedValues = null)
        {
            Name = name;
            ValueType = valueType;
            Required = required;
            Description = description;
            DefaultValue = defaultValue;
            Min = min;
            Max = max;
            MinInt = minInt;
            MaxInt = maxInt;
            Aliases = aliases ?? Array.Empty<string>();
            MinLength = minLength;
            MaxLength = maxLength;
            AllowedValues = allowedValues?.ToArray() ?? Array.Empty<string>();
            LookupKeys = new[] { name }.Concat(Aliases).ToArray();
        }

        public string Name { get; }
        public ParameterValueType ValueType { get; }
        public bool Required { get; }
        public string Description { get; }
        public object? DefaultValue { get; }
        public double? Min { get; }
        public double? Max { get; }
        public int? MinInt { get; }
        public int? MaxInt { get; }
        public int? MinLength { get; }
        public int? MaxLength { get; }
        public string[] Aliases { get; }
        public string[] LookupKeys { get; }
        public IReadOnlyList<string> AllowedValues { get; }
    }
}

public class TemplateInfo
{
    public DetectionTemplateFactory.TemplateType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DetectionType DetectionType { get; set; }
    public Dictionary<string, object> DefaultParameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<TemplateParameterDefinition> ParameterDefinitions { get; set; } = new();
}

public class TemplateParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public List<string> AllowedValues { get; set; } = new();
}
