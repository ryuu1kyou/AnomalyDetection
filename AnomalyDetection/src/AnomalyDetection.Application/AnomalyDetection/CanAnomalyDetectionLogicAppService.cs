using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection.Dtos;
using AnomalyDetection.DetectionTemplates;
using AnomalyDetection.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AnomalyDetection.AnomalyDetection;

[Authorize(AnomalyDetectionPermissions.DetectionLogics.Default)]
public class CanAnomalyDetectionLogicAppService : ApplicationService, ICanAnomalyDetectionLogicAppService
{
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;
    private readonly IDetectionTemplateAppService _detectionTemplateAppService;

    public CanAnomalyDetectionLogicAppService(
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository,
        IDetectionTemplateAppService detectionTemplateAppService)
    {
        _detectionLogicRepository = detectionLogicRepository;
        _detectionTemplateAppService = detectionTemplateAppService;
    }

    public async Task<PagedResultDto<CanAnomalyDetectionLogicDto>> GetListAsync(GetDetectionLogicsInput input)
    {
        var queryable = await _detectionLogicRepository.GetQueryableAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(input.Filter))
        {
            queryable = queryable.Where(x =>
                x.Identity.Name.Contains(input.Filter) ||
                x.Specification.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrEmpty(input.Name))
        {
            queryable = queryable.Where(x => x.Identity.Name.Contains(input.Name));
        }

        if (input.DetectionType.HasValue)
        {
            // Map DetectionType to AnomalyType for comparison
            var anomalyType = (AnomalyType)(int)input.DetectionType.Value;
            queryable = queryable.Where(x => x.Specification.DetectionType == anomalyType);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        if (input.SharingLevel.HasValue)
        {
            queryable = queryable.Where(x => x.SharingLevel == input.SharingLevel.Value);
        }

        if (input.AsilLevel.HasValue)
        {
            queryable = queryable.Where(x => x.Safety.AsilLevel == input.AsilLevel.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderBy(x => x.Identity.Name);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable.Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(items);

        return new PagedResultDto<CanAnomalyDetectionLogicDto>(totalCount, dtos);
    }

    public async Task<CanAnomalyDetectionLogicDto> GetAsync(Guid id)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(logic);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Create)]
    public async Task<CanAnomalyDetectionLogicDto> CreateAsync(CreateDetectionLogicDto input)
    {
        var logic = ObjectMapper.Map<CreateDetectionLogicDto, CanAnomalyDetectionLogic>(input);

        logic = await _detectionLogicRepository.InsertAsync(logic, autoSave: true);
        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(logic);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Edit)]
    public async Task<CanAnomalyDetectionLogicDto> UpdateAsync(Guid id, UpdateDetectionLogicDto input)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);

        // Update logic properties based on input
        // This is a simplified implementation

        logic = await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(logic);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _detectionLogicRepository.DeleteAsync(id);
    }

    public async Task<bool> CanDeleteAsync(Guid id)
    {
        // Check if logic is used by any detection results
        return await Task.FromResult(true);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByDetectionTypeAsync(DetectionType detectionType)
    {
        // Map DetectionType to AnomalyType
        var anomalyType = (AnomalyType)(int)detectionType;
        var logics = await _detectionLogicRepository.GetListAsync(x => x.Specification.DetectionType == anomalyType);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByShareLevelAsync(SharingLevel sharingLevel)
    {
        var logics = await _detectionLogicRepository.GetListAsync(x => x.SharingLevel == sharingLevel);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByAsilLevelAsync(AsilLevel asilLevel)
    {
        var logics = await _detectionLogicRepository.GetListAsync(x => x.Safety.AsilLevel == asilLevel);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Edit)]
    public async Task SubmitForApprovalAsync(Guid id)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.SubmitForApproval();
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Approve)]
    public async Task ApproveAsync(Guid id, string? notes = null)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.Approve(CurrentUser.Id ?? Guid.Empty, notes ?? string.Empty);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Approve)]
    public async Task RejectAsync(Guid id, string reason)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.Reject(reason);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Edit)]
    public async Task DeprecateAsync(Guid id, string reason)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.Deprecate(reason);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.ManageSharing)]
    public async Task UpdateSharingLevelAsync(Guid id, SharingLevel sharingLevel)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        logic.UpdateSharingLevel(sharingLevel);
        await _detectionLogicRepository.UpdateAsync(logic, autoSave: true);
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Execute)]
    public async Task<Dictionary<string, object>> TestExecutionAsync(Guid id, Dictionary<string, object> testData)
    {
        // TODO: Implement test execution logic
        return await Task.FromResult(new Dictionary<string, object>());
    }

    public Task<List<string>> ValidateImplementationAsync(Guid id)
    {
        // TODO: Implement validation logic
        return Task.FromResult(new List<string>());
    }

    public async Task<Dictionary<string, object>> GetExecutionStatisticsAsync(Guid id)
    {
        var logic = await _detectionLogicRepository.GetAsync(id);
        return new Dictionary<string, object>
        {
            ["ExecutionCount"] = logic.ExecutionCount,
            ["LastExecutedAt"] = logic.LastExecutedAt as object ?? DBNull.Value,
            ["AverageExecutionTime"] = logic.GetAverageExecutionTime()
        };
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Create)]
    public Task<CanAnomalyDetectionLogicDto> CloneAsync(Guid id, string newName)
    {
        // TODO: Implement clone logic
        throw new NotImplementedException();
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.ManageTemplates)]
    public async Task<CanAnomalyDetectionLogicDto> CreateFromTemplateAsync(DetectionType detectionType, Dictionary<string, object> parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        var templateType = MapDetectionTypeToTemplateType(detectionType);
        if (templateType == null)
        {
            throw new BusinessException("DETECTION_TEMPLATE_UNSUPPORTED")
                .WithData("DetectionType", detectionType.ToString());
        }

        var logicName = GetString(parameters, "LogicName", "Name", "DisplayName");
        if (string.IsNullOrWhiteSpace(logicName))
        {
            logicName = $"{detectionType} Logic {Clock.Now:yyyyMMddHHmmss}";
        }

        var canSignalId = GetGuid(parameters, "CanSignalId", "SignalId", "PrimarySignalId");
        if (canSignalId == Guid.Empty)
        {
            throw new BusinessException("DETECTION_TEMPLATE_SIGNAL_REQUIRED");
        }

        var customParameters = ExtractTemplateParameters(parameters);

        var createInput = new CreateFromTemplateDto
        {
            TemplateType = (int)templateType.Value,
            CanSignalId = canSignalId,
            LogicName = logicName,
            CustomParameters = customParameters.Count > 0 ? customParameters : null
        };

        var created = await _detectionTemplateAppService.CreateFromTemplateAsync(createInput);
        var createdEntity = await _detectionLogicRepository.GetAsync(created.Id);

        return ObjectMapper.Map<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>(createdEntity);
    }

    public async Task<List<Dictionary<string, object>>> GetTemplatesAsync(DetectionType detectionType)
    {
        var templates = await _detectionTemplateAppService.GetAvailableTemplatesAsync();
        var filtered = templates.Items
            .Where(t => t.DetectionType == (int)detectionType)
            .Select(t =>
            {
                var template = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["type"] = t.Type,
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["detectionType"] = t.DetectionType,
                    ["defaultParameters"] = new Dictionary<string, object>(t.DefaultParameters, StringComparer.OrdinalIgnoreCase)
                };

                template["parameterDefinitions"] = t.ParameterDefinitions
                    .Select(pd =>
                    {
                        var definition = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["name"] = pd.Name,
                            ["type"] = pd.Type,
                            ["description"] = pd.Description,
                            ["defaultValue"] = pd.DefaultValue ?? string.Empty,
                            ["required"] = pd.Required,
                            ["allowedValues"] = pd.AllowedValues ?? new List<string>()
                        };

                        if (pd.MinValue != null)
                        {
                            definition["minValue"] = pd.MinValue;
                        }

                        if (pd.MaxValue != null)
                        {
                            definition["maxValue"] = pd.MaxValue;
                        }

                        if (pd.MinLength.HasValue)
                        {
                            definition["minLength"] = pd.MinLength.Value;
                        }

                        if (pd.MaxLength.HasValue)
                        {
                            definition["maxLength"] = pd.MaxLength.Value;
                        }

                        return definition;
                    })
                    .ToList();

                return template;
            })
            .ToList();

        return filtered;
    }

    private static DetectionTemplateFactory.TemplateType? MapDetectionTypeToTemplateType(DetectionType detectionType)
    {
        return detectionType switch
        {
            DetectionType.OutOfRange => DetectionTemplateFactory.TemplateType.OutOfRange,
            DetectionType.RateOfChange => DetectionTemplateFactory.TemplateType.RateOfChange,
            DetectionType.Timeout => DetectionTemplateFactory.TemplateType.Timeout,
            DetectionType.Stuck => DetectionTemplateFactory.TemplateType.StuckValue,
            _ => null
        };
    }

    private static string? GetString(IReadOnlyDictionary<string, object> source, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (TryGetValue(source, key, out var value))
            {
                var normalized = NormalizeParameterValue(value);
                if (normalized == null)
                {
                    continue;
                }

                if (normalized is string stringValue)
                {
                    return stringValue;
                }

                return normalized.ToString();
            }
        }

        return null;
    }

    private static Guid GetGuid(IReadOnlyDictionary<string, object> source, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!TryGetValue(source, key, out var value))
            {
                continue;
            }

            var normalized = NormalizeParameterValue(value);

            if (normalized is Guid guidValue)
            {
                return guidValue;
            }

            if (normalized is string stringValue && Guid.TryParse(stringValue, out var parsed))
            {
                return parsed;
            }
        }

        return Guid.Empty;
    }

    private static Dictionary<string, object> ExtractTemplateParameters(Dictionary<string, object> source)
    {
        if (TryGetTemplateParameterContainer(source, out var nested) && nested.Count > 0)
        {
            return nested;
        }

        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var metadataKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "logicname",
            "name",
            "displayname",
            "cansignalid",
            "signalid",
            "primarysignalid",
            "templatetype"
        };

        metadataKeys.UnionWith(TemplateParameterContainerKeys);

        foreach (var kvp in source)
        {
            if (metadataKeys.Contains(kvp.Key))
            {
                continue;
            }

            var normalized = NormalizeParameterValue(kvp.Value);
            if (normalized != null)
            {
                result[kvp.Key] = normalized;
            }
        }

        return result;
    }

    private static bool TryGetTemplateParameterContainer(IReadOnlyDictionary<string, object> source, out Dictionary<string, object> parameters)
    {
        foreach (var key in TemplateParameterContainerKeys)
        {
            if (!TryGetValue(source, key, out var raw))
            {
                continue;
            }

            var dictionary = NormalizeToDictionary(raw);
            if (dictionary != null && dictionary.Count > 0)
            {
                parameters = dictionary;
                return true;
            }
        }

        parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        return false;
    }

    private static Dictionary<string, object>? NormalizeToDictionary(object? value)
    {
        var normalized = NormalizeParameterValue(value);

        switch (normalized)
        {
            case Dictionary<string, object> dict:
                return new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            case System.Collections.IDictionary dictionary:
                var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (System.Collections.DictionaryEntry entry in dictionary)
                {
                    if (entry.Key is not string key)
                    {
                        continue;
                    }

                    var normalizedValue = NormalizeParameterValue(entry.Value);
                    if (normalizedValue != null)
                    {
                        result[key] = normalizedValue;
                    }
                }

                return result;
            default:
                return null;
        }
    }

    private static object? NormalizeParameterValue(object? value)
    {
        if (value is JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var longValue))
                    {
                        return longValue;
                    }

                    if (element.TryGetDouble(out var doubleValue))
                    {
                        return doubleValue;
                    }

                    break;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in element.EnumerateObject())
                    {
                        var normalizedProperty = NormalizeParameterValue(property.Value);
                        if (normalizedProperty != null)
                        {
                            dict[property.Name] = normalizedProperty;
                        }
                    }

                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        var normalizedItem = NormalizeParameterValue(item);
                        if (normalizedItem != null)
                        {
                            list.Add(normalizedItem);
                        }
                    }

                    return list;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
            }
        }

        return value;
    }

    private static bool TryGetValue(IReadOnlyDictionary<string, object> source, string key, out object value)
    {
        foreach (var kvp in source)
        {
            if (string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = kvp.Value;
                return true;
            }
        }

        value = default!;
        return false;
    }

    private static readonly string[] TemplateParameterContainerKeys =
    {
        "TemplateParameters",
        "Parameters",
        "CustomParameters"
    };

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Export)]
    public Task<byte[]> ExportAsync(Guid id, string format)
    {
        // TODO: Implement export logic
        throw new NotImplementedException();
    }

    [Authorize(AnomalyDetectionPermissions.DetectionLogics.Import)]
    public Task<CanAnomalyDetectionLogicDto> ImportAsync(byte[] fileContent, string fileName)
    {
        // TODO: Implement import logic
        throw new NotImplementedException();
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByCanSignalAsync(Guid canSignalId)
    {
        var queryable = await _detectionLogicRepository.GetQueryableAsync();
        var logics = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.SignalMappings.Any(m => m.CanSignalId == canSignalId)));

        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }

    public async Task<ListResultDto<CanAnomalyDetectionLogicDto>> GetByVehiclePhaseAsync(Guid vehiclePhaseId)
    {
        var logics = await _detectionLogicRepository.GetListAsync(x => x.VehiclePhaseId == vehiclePhaseId);
        var dtos = ObjectMapper.Map<List<CanAnomalyDetectionLogic>, List<CanAnomalyDetectionLogicDto>>(logics);
        return new ListResultDto<CanAnomalyDetectionLogicDto>(dtos);
    }
}