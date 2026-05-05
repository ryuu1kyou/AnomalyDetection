using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AnomalyDetection.AuditLogging;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.ChangeTracking.Dtos;

public class ChangeBundleDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string FeatureId { get; set; } = string.Empty;
    public string? DecisionId { get; set; }
    public AuditChangeType ChangeType { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DocSyncStatus DocSyncStatus { get; set; }
    public string? DocVersion { get; set; }
    public List<ChangeBundleItemDto> Items { get; set; } = new();
}

public class ChangeBundleItemDto : EntityDto<Guid>
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; }
}

public class CreateChangeBundleDto
{
    [Required]
    [StringLength(50)]
    public string FeatureId { get; set; } = string.Empty;

    [StringLength(50)]
    public string? DecisionId { get; set; }

    public AuditChangeType ChangeType { get; set; } = AuditChangeType.NotApplicable;

    [Required]
    [StringLength(1000)]
    public string ChangeReason { get; set; } = string.Empty;

    public List<AddChangeBundleItemDto> Items { get; set; } = new();
}

public class AddChangeBundleItemDto
{
    [Required]
    public Guid EntityId { get; set; }

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;
}

public class UpdateChangeBundleDocSyncDto
{
    public DocSyncStatus DocSyncStatus { get; set; }

    [StringLength(100)]
    public string? DocVersion { get; set; }
}
