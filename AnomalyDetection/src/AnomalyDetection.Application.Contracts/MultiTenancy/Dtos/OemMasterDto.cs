using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.MultiTenancy.Dtos;

public class OemMasterDto : FullAuditedEntityDto<Guid>
{
    public string OemCode { get; set; }
    public string OemName { get; set; }
    public string CompanyName { get; set; }
    public string Country { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public string Description { get; set; }
    public List<OemFeatureDto> Features { get; set; } = new();
}

public class CreateOemMasterDto
{
    public string OemCode { get; set; }
    public string OemName { get; set; }
    public string CompanyName { get; set; }
    public string Country { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public DateTime? EstablishedDate { get; set; }
    public string Description { get; set; }
}

public class UpdateOemMasterDto
{
    public string CompanyName { get; set; }
    public string Country { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string Description { get; set; }
}

public class OemFeatureDto
{
    public string FeatureName { get; set; }
    public string FeatureValue { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOemFeatureDto
{
    public string FeatureName { get; set; }
    public string FeatureValue { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateOemFeatureDto
{
    public string FeatureValue { get; set; }
    public bool IsEnabled { get; set; }
}