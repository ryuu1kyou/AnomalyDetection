using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.DetectionTemplates;

public class DetectionTemplateAppService : ApplicationService, IDetectionTemplateAppService
{
    private readonly DetectionTemplateFactory _templateFactory;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;

    public DetectionTemplateAppService(
        DetectionTemplateFactory templateFactory,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IRepository<CanSignal, Guid> canSignalRepository)
    {
        _templateFactory = templateFactory;
        _detectionLogicRepository = detectionLogicRepository;
        _canSignalRepository = canSignalRepository;
    }

    public Task<ListResultDto<DetectionTemplateDto>> GetAvailableTemplatesAsync()
    {
        var templates = _templateFactory.GetAvailableTemplates();

        var dtos = templates.Select(t => new DetectionTemplateDto
        {
            Type = (int)t.Type,
            Name = t.Name,
            Description = t.Description,
            DetectionType = (int)t.DetectionType,
            DefaultParameters = new Dictionary<string, object>(t.DefaultParameters, StringComparer.OrdinalIgnoreCase),
            ParameterDefinitions = t.ParameterDefinitions.Select(MapParameterDefinition).ToList()
        }).ToList();

        return Task.FromResult(new ListResultDto<DetectionTemplateDto>(dtos));
    }

    public Task<DetectionTemplateDto> GetTemplateAsync(int templateType)
    {
        var type = (DetectionTemplateFactory.TemplateType)templateType;
        var templates = _templateFactory.GetAvailableTemplates();
        var template = templates.FirstOrDefault(t => t.Type == type);

        if (template == null)
        {
            throw new Volo.Abp.BusinessException("TEMPLATE_NOT_FOUND");
        }

        var dto = new DetectionTemplateDto
        {
            Type = (int)template.Type,
            Name = template.Name,
            Description = template.Description,
            DetectionType = (int)template.DetectionType,
            DefaultParameters = new Dictionary<string, object>(template.DefaultParameters, StringComparer.OrdinalIgnoreCase),
            ParameterDefinitions = template.ParameterDefinitions.Select(MapParameterDefinition).ToList()
        };

        return Task.FromResult(dto);
    }

    public async Task<DetectionLogicDto> CreateFromTemplateAsync(CreateFromTemplateDto input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.LogicName))
        {
            throw new Volo.Abp.BusinessException("DETECTION_LOGIC_NAME_REQUIRED");
        }

        var templateType = (DetectionTemplateFactory.TemplateType)input.TemplateType;
        var template = _templateFactory
            .GetAvailableTemplates()
            .FirstOrDefault(t => t.Type == templateType);

        if (template == null)
        {
            throw new Volo.Abp.BusinessException("TEMPLATE_NOT_FOUND");
        }

        var canSignal = await _canSignalRepository.GetAsync(input.CanSignalId);

        var mergedParameters = MergeTemplateParameters(template, input.CustomParameters);

        if (!_templateFactory.ValidateTemplateParameters(templateType, mergedParameters, out var errors))
        {
            var validationException = new Volo.Abp.BusinessException("TEMPLATE_PARAMETERS_INVALID");
            validationException.WithData("Errors", errors);
            throw validationException;
        }

        var logic = BuildLogicFromTemplate(template, mergedParameters, input.LogicName, canSignal);

        foreach (var parameter in CreateDetectionParameters(template.ParameterDefinitions, mergedParameters))
        {
            logic.AddParameter(parameter);
        }

        logic.AddSignalMapping(new CanSignalMapping(
            input.CanSignalId,
            SignalRoles.Primary,
            true,
            $"Primary signal generated from template {template.Name}",
            new SignalMappingConfiguration()));

        var implementationPayload = new
        {
            Template = template.Name,
            TemplateType = template.Type.ToString(),
            Parameters = mergedParameters,
            GeneratedAt = Clock.Now,
            GeneratedBy = CurrentUser.Id
        };

        var implementationJson = JsonSerializer.Serialize(implementationPayload);

        logic.UpdateImplementation(new LogicImplementation(
            ImplementationType.Configuration,
            implementationJson,
            language: "json",
            entryPoint: null,
            createdBy: CurrentUser?.UserName));

        logic.UpdateSharingLevel(SharingLevel.Private);

        await _detectionLogicRepository.InsertAsync(logic, autoSave: true);

        return new DetectionLogicDto
        {
            Id = logic.Id,
            CanSignalId = input.CanSignalId,
            Name = logic.Identity.Name,
            Description = logic.Specification.Description,
            DetectionType = (int)template.DetectionType,
            Parameters = new Dictionary<string, object>(mergedParameters, StringComparer.OrdinalIgnoreCase),
            Thresholds = ExtractThresholds(mergedParameters),
            IsEnabled = logic.Status == DetectionLogicStatus.Approved
        };
    }

    public Task<TemplateValidationResultDto> ValidateTemplateParametersAsync(ValidateTemplateDto input)
    {
        var templateType = (DetectionTemplateFactory.TemplateType)input.TemplateType;

        var isValid = _templateFactory.ValidateTemplateParameters(
            templateType,
            input.Parameters,
            out var errors
        );

        var result = new TemplateValidationResultDto
        {
            IsValid = isValid,
            Errors = errors,
            Warnings = new List<string>()
        };

        return Task.FromResult(result);
    }

    private static ParameterDefinitionDto MapParameterDefinition(TemplateParameterDefinition definition)
    {
        return new ParameterDefinitionDto
        {
            Name = definition.Name,
            Type = definition.Type,
            Description = definition.Description,
            DefaultValue = definition.DefaultValue ?? string.Empty,
            Required = definition.Required,
            MinValue = definition.MinValue,
            MaxValue = definition.MaxValue,
            MinLength = definition.MinLength,
            MaxLength = definition.MaxLength,
            AllowedValues = definition.AllowedValues.Any() ? new List<string>(definition.AllowedValues) : null
        };
    }

    private static Dictionary<string, object> MergeTemplateParameters(
        TemplateInfo template,
        Dictionary<string, object>? customParameters)
    {
        var merged = new Dictionary<string, object>(template.DefaultParameters, StringComparer.OrdinalIgnoreCase);

        if (customParameters != null)
        {
            foreach (var kvp in customParameters)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    continue;
                }

                var definition = template.ParameterDefinitions
                    .FirstOrDefault(d => d.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));

                if (definition == null)
                {
                    var normalized = NormalizeLooseValue(kvp.Value);
                    if (normalized == null)
                    {
                        merged.Remove(kvp.Key);
                    }
                    else
                    {
                        merged[kvp.Key] = normalized;
                    }

                    continue;
                }

                var targetKey = definition.Name;
                var converted = ConvertToParameterType(definition.Type, kvp.Value);
                if (converted == null)
                {
                    merged.Remove(targetKey);
                }
                else
                {
                    merged[targetKey] = converted;
                }
            }
        }

        foreach (var definition in template.ParameterDefinitions)
        {
            if (merged.TryGetValue(definition.Name, out var existing))
            {
                merged[definition.Name] = ConvertToParameterType(definition.Type, existing);
            }
        }

        return merged;
    }

    private static object ConvertToParameterType(string type, object? rawValue)
    {
        var normalizedValue = NormalizeLooseValue(rawValue);

        switch (type.ToLowerInvariant())
        {
            case "number":
                return Convert.ToDouble(normalizedValue ?? 0d, CultureInfo.InvariantCulture);
            case "integer":
                return Convert.ToInt32(normalizedValue ?? 0, CultureInfo.InvariantCulture);
            case "boolean":
                if (normalizedValue is bool boolValue)
                {
                    return boolValue;
                }

                if (normalizedValue is string stringValue)
                {
                    if (bool.TryParse(stringValue, out var parsedBool))
                    {
                        return parsedBool;
                    }

                    if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var numericBool))
                    {
                        return Math.Abs(numericBool) > double.Epsilon;
                    }

                    return !string.IsNullOrWhiteSpace(stringValue);
                }

                if (normalizedValue == null)
                {
                    return false;
                }

                return Convert.ToBoolean(normalizedValue, CultureInfo.InvariantCulture);
            default:
                return normalizedValue?.ToString() ?? string.Empty;
        }
    }

    private static object? NormalizeLooseValue(object? value)
    {
        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.TryGetInt64(out var longValue)
                    ? longValue
                    : element.GetDouble(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                JsonValueKind.Object => element.GetRawText(),
                JsonValueKind.Array => element
                    .EnumerateArray()
                    .Select(item => NormalizeLooseValue(item))
                    .ToArray(),
                _ => element.GetRawText()
            };
        }

        return value;
    }

    private CanAnomalyDetectionLogic BuildLogicFromTemplate(
        TemplateInfo template,
        IReadOnlyDictionary<string, object> parameters,
        string logicName,
        CanSignal signal)
    {
        var tenantId = CurrentTenant.Id;
        var identity = new DetectionLogicIdentity(logicName, LogicVersion.Initial(), signal.OemCode);
        var anomalyType = (AnomalyType)(int)template.DetectionType;
        var specification = new DetectionLogicSpecification(
            anomalyType,
            template.Description,
            signal.SystemType);

        var safety = new SafetyClassification(AsilLevel.QM, null, null);

        return new CanAnomalyDetectionLogic(
            GuidGenerator.Create(),
            tenantId,
            identity,
            specification,
            safety);
    }

    private static IEnumerable<DetectionParameter> CreateDetectionParameters(
        IEnumerable<TemplateParameterDefinition> definitions,
        IReadOnlyDictionary<string, object> parameters)
    {
        foreach (var definition in definitions)
        {
            parameters.TryGetValue(definition.Name, out var value);

            var dataType = MapToParameterDataType(definition.Type);
            var constraints = new ParameterConstraints(
                ConvertToNullableDouble(definition.MinValue),
                ConvertToNullableDouble(definition.MaxValue),
                definition.MinLength,
                definition.MaxLength,
                null,
                definition.AllowedValues);

            var defaultValue = definition.DefaultValue?.ToString() ?? string.Empty;
            var parameter = new DetectionParameter(
                definition.Name,
                dataType,
                defaultValue,
                constraints,
                definition.Description,
                definition.Required);

            if (value != null)
            {
                parameter.UpdateValue(FormatParameterValue(value, dataType));
            }

            yield return parameter;
        }
    }

    private static ParameterDataType MapToParameterDataType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "number" => ParameterDataType.Double,
            "integer" => ParameterDataType.Integer,
            "boolean" => ParameterDataType.Boolean,
            _ => ParameterDataType.String
        };
    }

    private static string FormatParameterValue(object value, ParameterDataType dataType)
    {
        return dataType switch
        {
            ParameterDataType.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            ParameterDataType.Integer => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            ParameterDataType.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString(),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static double? ConvertToNullableDouble(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is double doubleValue)
        {
            return doubleValue;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        if (value is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static Dictionary<string, double> ExtractThresholds(IDictionary<string, object> parameters)
    {
        var thresholds = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in parameters)
        {
            if (!kvp.Key.Contains("threshold", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryConvertToDouble(kvp.Value, out var numericValue))
            {
                thresholds[kvp.Key] = numericValue;
            }
        }

        return thresholds;
    }

    private static bool TryConvertToDouble(object? value, out double numeric)
    {
        value = NormalizeLooseValue(value);

        switch (value)
        {
            case null:
                numeric = 0;
                return false;
            case double doubleValue:
                numeric = doubleValue;
                return true;
            case float floatValue:
                numeric = Convert.ToDouble(floatValue, CultureInfo.InvariantCulture);
                return true;
            case int intValue:
                numeric = intValue;
                return true;
            case long longValue:
                numeric = longValue;
                return true;
            case decimal decimalValue:
                numeric = Convert.ToDouble(decimalValue, CultureInfo.InvariantCulture);
                return true;
            default:
                return double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out numeric);
        }
    }
}
