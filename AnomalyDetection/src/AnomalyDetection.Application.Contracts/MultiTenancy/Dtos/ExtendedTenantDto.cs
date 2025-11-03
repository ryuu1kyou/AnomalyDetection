using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.MultiTenancy.Dtos;

public class ExtendedTenantDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
    public string OemName { get; set; } = string.Empty;
    public Guid? OemMasterId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ActivationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<TenantFeatureDto> Features { get; set; } = new();
    public bool IsExpired { get; set; }
    public bool IsValidForUse { get; set; }
}

public class CreateExtendedTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
    public string OemName { get; set; } = string.Empty;
    public Guid? OemMasterId { get; set; }
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
}

public class UpdateExtendedTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
}

public class TenantFeatureDto
{
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureValue { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class CreateTenantFeatureDto
{
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureValue { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

public class UpdateTenantFeatureDto
{
    public string FeatureValue { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class TenantSwitchDto
{
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string OemCode { get; set; } = string.Empty;
}