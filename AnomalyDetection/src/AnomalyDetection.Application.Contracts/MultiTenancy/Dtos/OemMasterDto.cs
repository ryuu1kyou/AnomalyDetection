using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.MultiTenancy.Dtos;

public class OemMasterDto : FullAuditedEntityDto<Guid>
{
    public string OemCode { get; set; } = string.Empty;
    public string OemName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<OemFeatureDto> Features { get; set; } = new();
}

public class CreateOemMasterDto
{
    public string OemCode { get; set; } = string.Empty;
    public string OemName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public DateTime? EstablishedDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateOemMasterDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class OemFeatureDto
{
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureValue { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOemFeatureDto
{
    public string FeatureName { get; set; } = string.Empty;
    public string FeatureValue { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

public class UpdateOemFeatureDto
{
    public string FeatureValue { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}