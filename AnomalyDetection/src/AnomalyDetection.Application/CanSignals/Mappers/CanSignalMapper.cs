using AnomalyDetection.CanSignals.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace AnomalyDetection.CanSignals.Mappers;

[Mapper]
public partial class CanSignalMapper : MapperBase<CanSignal, CanSignalDto>
{
    // Entity to DTO mapping
    public override CanSignalDto Map(CanSignal source)
    {
        if (source == null) return null!;

        var dto = new CanSignalDto
        {
            Id = source.Id,
            TenantId = source.TenantId,

            // Identity mapping - Flatten value object properties
            SignalName = source.Identifier.SignalName,
            CanId = source.Identifier.CanId ?? string.Empty,

            // Specification mapping - Flatten value object properties
            StartBit = source.Specification.StartBit,
            Length = source.Specification.Length,
            DataType = source.Specification.DataType,
            MinValue = source.Specification.ValueRange.MinValue,
            MaxValue = source.Specification.ValueRange.MaxValue,
            ByteOrder = source.Specification.ByteOrder,

            // Conversion mapping - Flatten value object properties
            Factor = source.Conversion.Factor,
            Offset = source.Conversion.Offset,
            Unit = source.Conversion.Unit ?? string.Empty,

            // Timing mapping - Flatten value object properties
            CycleTime = source.Timing.CycleTimeMs,
            TimeoutTime = source.Timing.TimeoutMs,

            // Entity attributes
            SystemType = source.SystemType,
            Description = source.Description ?? string.Empty,
            OemCode = source.OemCode,
            IsStandard = source.IsStandard,
            Version = source.Version.ToString(),
            EffectiveDate = source.EffectiveDate,
            Status = source.Status,

            // Metadata
            SourceDocument = source.SourceDocument ?? string.Empty,
            Notes = source.Notes ?? string.Empty,

            // Audit fields
            CreationTime = source.CreationTime,
            CreatorId = source.CreatorId,
            LastModificationTime = source.LastModificationTime,
            LastModifierId = source.LastModifierId,
            IsDeleted = source.IsDeleted,
            DeleterId = source.DeleterId,
            DeletionTime = source.DeletionTime
        };


        return dto;
    }

    public override void Map(CanSignal source, CanSignalDto destination)
    {
        if (source == null || destination == null) return;

        destination.Id = source.Id;
        destination.TenantId = source.TenantId;

        // Identity mapping - Flatten value object properties
        destination.SignalName = source.Identifier.SignalName;
        destination.CanId = source.Identifier.CanId ?? string.Empty;

        // Specification mapping - Flatten value object properties
        destination.StartBit = source.Specification.StartBit;
        destination.Length = source.Specification.Length;
        destination.DataType = source.Specification.DataType;
        destination.MinValue = source.Specification.ValueRange.MinValue;
        destination.MaxValue = source.Specification.ValueRange.MaxValue;
        destination.ByteOrder = source.Specification.ByteOrder;

        // Conversion mapping - Flatten value object properties
        destination.Factor = source.Conversion.Factor;
        destination.Offset = source.Conversion.Offset;
        destination.Unit = source.Conversion.Unit ?? string.Empty;

        // Timing mapping - Flatten value object properties
        destination.CycleTime = source.Timing.CycleTimeMs;
        destination.TimeoutTime = source.Timing.TimeoutMs;

        // Entity attributes
        destination.SystemType = source.SystemType;
        destination.Description = source.Description ?? string.Empty;
        destination.OemCode = source.OemCode;
        destination.IsStandard = source.IsStandard;
        destination.Version = source.Version.ToString();
        destination.EffectiveDate = source.EffectiveDate;
        destination.Status = source.Status;

        // Metadata
        destination.SourceDocument = source.SourceDocument ?? string.Empty;
        destination.Notes = source.Notes ?? string.Empty;

        // Audit fields
        destination.CreationTime = source.CreationTime;
        destination.CreatorId = source.CreatorId;
        destination.LastModificationTime = source.LastModificationTime;
        destination.LastModifierId = source.LastModifierId;
        destination.IsDeleted = source.IsDeleted;
        destination.DeleterId = source.DeleterId;
        destination.DeletionTime = source.DeletionTime;
    }
}
