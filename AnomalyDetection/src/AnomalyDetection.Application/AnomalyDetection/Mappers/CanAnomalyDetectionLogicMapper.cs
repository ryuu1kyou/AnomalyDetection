using System.Collections.Generic;
using System.Linq;
using AnomalyDetection.AnomalyDetection.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace AnomalyDetection.AnomalyDetection.Mappers;

[Mapper]
public partial class CanAnomalyDetectionLogicMapper :
    MapperBase<CanAnomalyDetectionLogic, CanAnomalyDetectionLogicDto>
{
    public override CanAnomalyDetectionLogicDto Map(CanAnomalyDetectionLogic source)
    {
        if (source == null) return null!;

        return new CanAnomalyDetectionLogicDto
        {
            Id = source.Id,

            // Identity properties
            Name = source.Identity.Name,
            Version = source.Identity.Version.ToString(),
            OemCode = source.Identity.OemCode,

            // Specification properties
            DetectionType = ConvertAnomalyTypeToDetectionType(source.Specification.DetectionType),
            Description = source.Specification.Description,
            Purpose = source.Specification.Requirements ?? string.Empty,

            // Implementation properties
            LogicContent = source.Implementation.Content,
            Algorithm = source.Implementation.Type.ToString(),
            IsExecutable = source.Implementation.IsExecutable(),

            // Safety properties
            AsilLevel = source.Safety.AsilLevel,
            SafetyRequirementId = source.Safety.SafetyRequirementId ?? string.Empty,
            SafetyGoalId = source.Safety.SafetyGoalId ?? string.Empty,            // Status and metadata
            Status = source.Status,
            SharingLevel = source.SharingLevel,
            SourceLogicId = source.SourceLogicId,
            VehiclePhaseId = source.VehiclePhaseId,
            ApprovedAt = source.ApprovedAt,
            ApprovedBy = source.ApprovedBy,
            ApprovalNotes = source.ApprovalNotes,

            // Execution statistics
            ExecutionCount = source.ExecutionCount,
            LastExecutedAt = source.LastExecutedAt,
            LastExecutionTimeMs = source.LastExecutionTimeMs,

            // Collections
            Parameters = MapParameters(source.Parameters),
            SignalMappings = MapSignalMappings(source.SignalMappings),

            // Audit properties
            CreationTime = source.CreationTime,
            CreatorId = source.CreatorId,
            LastModificationTime = source.LastModificationTime,
            LastModifierId = source.LastModifierId,
            DeletionTime = source.DeletionTime,
            DeleterId = source.DeleterId,
            IsDeleted = source.IsDeleted
        };
    }

    public override void Map(CanAnomalyDetectionLogic source, CanAnomalyDetectionLogicDto destination)
    {
        if (source == null || destination == null) return;

        destination.Id = source.Id;

        // Identity properties
        destination.Name = source.Identity.Name;
        destination.Version = source.Identity.Version.ToString();
        destination.OemCode = source.Identity.OemCode;

        // Specification properties
        destination.DetectionType = ConvertAnomalyTypeToDetectionType(source.Specification.DetectionType);
        destination.Description = source.Specification.Description;
        destination.Purpose = source.Specification.Requirements ?? string.Empty;

        // Implementation properties
        destination.LogicContent = source.Implementation.Content;
        destination.Algorithm = source.Implementation.Type.ToString();
        destination.IsExecutable = source.Implementation.IsExecutable();

        // Safety properties
        destination.AsilLevel = source.Safety.AsilLevel;
        destination.SafetyRequirementId = source.Safety.SafetyRequirementId ?? string.Empty;
        destination.SafetyGoalId = source.Safety.SafetyGoalId ?? string.Empty;

        // Status and metadata
        destination.Status = source.Status;
        destination.SharingLevel = source.SharingLevel;
        destination.SourceLogicId = source.SourceLogicId;
        destination.VehiclePhaseId = source.VehiclePhaseId;
        destination.ApprovedAt = source.ApprovedAt;
        destination.ApprovedBy = source.ApprovedBy;
        destination.ApprovalNotes = source.ApprovalNotes;

        // Execution statistics
        destination.ExecutionCount = source.ExecutionCount;
        destination.LastExecutedAt = source.LastExecutedAt;
        destination.LastExecutionTimeMs = source.LastExecutionTimeMs;

        // Collections
        destination.Parameters = MapParameters(source.Parameters);
        destination.SignalMappings = MapSignalMappings(source.SignalMappings);

        // Audit properties
        destination.CreationTime = source.CreationTime;
        destination.CreatorId = source.CreatorId;
        destination.LastModificationTime = source.LastModificationTime;
        destination.LastModifierId = source.LastModifierId;
        destination.DeletionTime = source.DeletionTime;
        destination.DeleterId = source.DeleterId;
        destination.IsDeleted = source.IsDeleted;
    }

    private List<DetectionParameterDto> MapParameters(IReadOnlyList<DetectionParameter> parameters)
    {
        return parameters.Select(param => new DetectionParameterDto
        {
            Id = param.Id,
            Name = param.Name,
            DataType = param.DataType,
            Value = param.Value,
            DefaultValue = param.DefaultValue,
            Description = param.Description,
            IsRequired = param.IsRequired,
            Unit = param.Unit,
            MinValue = param.Constraints.MinValue,
            MaxValue = param.Constraints.MaxValue,
            MinLength = param.Constraints.MinLength,
            MaxLength = param.Constraints.MaxLength,
            Pattern = param.Constraints.Pattern,
            AllowedValues = string.Join(",", param.Constraints.AllowedValues),
            CreatedAt = param.CreatedAt,
            UpdatedAt = param.UpdatedAt
        }).ToList();
    }

    private List<CanSignalMappingDto> MapSignalMappings(IReadOnlyList<CanSignalMapping> mappings)
    {
        return mappings.Select(mapping => new CanSignalMappingDto
        {
            CanSignalId = mapping.CanSignalId,
            SignalRole = mapping.SignalRole,
            IsRequired = mapping.IsRequired,
            Description = mapping.Description ?? string.Empty,
            ScalingFactor = mapping.Configuration.ScalingFactor,
            Offset = mapping.Configuration.Offset,
            FilterExpression = mapping.Configuration.FilterExpression ?? string.Empty,
            CustomProperties = mapping.Configuration.CustomProperties,
            CreatedAt = mapping.CreatedAt,
            UpdatedAt = mapping.UpdatedAt
        }).ToList();
    }

    private DetectionType ConvertAnomalyTypeToDetectionType(AnomalyType anomalyType)
    {
        return anomalyType switch
        {
            AnomalyType.Timeout => DetectionType.Timeout,
            AnomalyType.OutOfRange => DetectionType.OutOfRange,
            AnomalyType.RateOfChange => DetectionType.RateOfChange,
            AnomalyType.Stuck => DetectionType.Stuck,
            AnomalyType.PeriodicAnomaly => DetectionType.Periodic,
            AnomalyType.Custom => DetectionType.Custom,
            _ => DetectionType.Custom
        };
    }
}
