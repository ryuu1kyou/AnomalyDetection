using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.DetectionTemplates;

/// <summary>
/// Application service for detection template management
/// </summary>
public interface IDetectionTemplateAppService : IApplicationService
{
    /// <summary>
    /// Get list of available detection templates
    /// </summary>
    Task<ListResultDto<DetectionTemplateDto>> GetAvailableTemplatesAsync();

    /// <summary>
    /// Get template details with default parameters
    /// </summary>
    Task<DetectionTemplateDto> GetTemplateAsync(int templateType);

    /// <summary>
    /// Create detection logic from template
    /// </summary>
    Task<DetectionLogicDto> CreateFromTemplateAsync(CreateFromTemplateDto input);

    /// <summary>
    /// Validate template parameters
    /// </summary>
    Task<TemplateValidationResultDto> ValidateTemplateParametersAsync(ValidateTemplateDto input);
}

/// <summary>
/// Detection template DTO
/// </summary>
public class DetectionTemplateDto
{
    public int Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DetectionType { get; set; }
    public Dictionary<string, object> DefaultParameters { get; set; } = new();
    public List<ParameterDefinitionDto> ParameterDefinitions { get; set; } = new();
    public int? UseCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// Parameter definition
/// </summary>
public class ParameterDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // string, number, boolean
    public string Description { get; set; } = string.Empty;
    public object DefaultValue { get; set; } = new();
    public bool Required { get; set; }
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public List<string>? AllowedValues { get; set; }
}

/// <summary>
/// Create from template input
/// </summary>
public class CreateFromTemplateDto
{
    public int TemplateType { get; set; }
    public Guid CanSignalId { get; set; }
    public string LogicName { get; set; } = string.Empty;
    public Dictionary<string, object>? CustomParameters { get; set; }
}

/// <summary>
/// Detection logic DTO
/// </summary>
public class DetectionLogicDto
{
    public Guid Id { get; set; }
    public Guid CanSignalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DetectionType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Dictionary<string, double> Thresholds { get; set; } = new();
    public bool IsEnabled { get; set; }
}

/// <summary>
/// Validate template input
/// </summary>
public class ValidateTemplateDto
{
    public int TemplateType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Template validation result
/// </summary>
public class TemplateValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
