using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.Application.Contracts.OemTraceability;
using AnomalyDetection.Application.Contracts.OemTraceability.Dtos;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.OemTraceability;
using AnomalyDetection.OemTraceability.Models;
using AnomalyDetection.OemTraceability.Services;
using AnomalyDetection.Permissions;
using AnomalyDetection.Shared.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace AnomalyDetection.Application.OemTraceability;

/// <summary>
/// OEMトレーサビリティアプリケーションサービス実装
/// </summary>
[Authorize(AnomalyDetectionPermissions.OemTraceability.Default)]
public class OemTraceabilityAppService : ApplicationService, IOemTraceabilityAppService
{
    private readonly TraceabilityQueryService _traceabilityQueryService;
    private readonly IOemCustomizationRepository _customizationRepository;
    private readonly IOemApprovalRepository _approvalRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ExportService _exportService;

    public OemTraceabilityAppService(
        TraceabilityQueryService traceabilityQueryService,
        IOemCustomizationRepository customizationRepository,
        IOemApprovalRepository approvalRepository,
        IAuditLogService auditLogService,
        ExportService exportService)
    {
        _traceabilityQueryService = traceabilityQueryService;
        _customizationRepository = customizationRepository;
        _approvalRepository = approvalRepository;
        _auditLogService = auditLogService;
        _exportService = exportService;
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.ViewTraceability)]
    public async Task<OemTraceabilityDto> GetOemTraceabilityAsync(Guid entityId, string entityType)
    {
        var result = await _traceabilityQueryService.TraceAcrossOemsAsync(entityId, entityType);
        return ObjectMapper.Map<OemTraceabilityResult, OemTraceabilityDto>(result);
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.CreateCustomization)]
    public async Task<Guid> CreateOemCustomizationAsync(CreateOemCustomizationDto input)
    {
        var oemCode = new OemCode(input.OemCode, input.OemCode); // Assuming code and name are the same for simplicity

        var customization = new OemCustomization(
            CurrentTenant.Id,
            input.EntityId,
            input.EntityType,
            oemCode,
            input.Type,
            input.CustomParameters,
            input.OriginalParameters,
            input.CustomizationReason);

        await _customizationRepository.InsertAsync(customization);

        // 監査ログを記録
        await _auditLogService.LogCreateAsync(
            customization.Id,
            "OemCustomization",
            customization,
            new Dictionary<string, object>
            {
                ["EntityType"] = input.EntityType,
                ["EntityId"] = input.EntityId,
                ["OemCode"] = input.OemCode,
                ["CustomizationType"] = input.Type.ToString()
            });

        return customization.Id;
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.EditCustomization)]
    public async Task<OemCustomizationDto> UpdateOemCustomizationAsync(Guid id, UpdateOemCustomizationDto input)
    {
        var customization = await _customizationRepository.GetAsync(id);

        customization.UpdateCustomParameters(input.CustomParameters, input.CustomizationReason);

        await _customizationRepository.UpdateAsync(customization);
        return ObjectMapper.Map<OemCustomization, OemCustomizationDto>(customization);
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.ViewCustomization)]
    public async Task<OemCustomizationDto> GetOemCustomizationAsync(Guid id)
    {
        var customization = await _customizationRepository.GetAsync(id);
        return ObjectMapper.Map<OemCustomization, OemCustomizationDto>(customization);
    }

    public async Task<List<OemCustomizationDto>> GetOemCustomizationsAsync(
        string? oemCode = null,
        string? entityType = null,
        CustomizationStatus? status = null)
    {
        // Use optimized query with proper filtering at database level
        var queryable = await _customizationRepository.GetQueryableAsync();

        if (!string.IsNullOrEmpty(oemCode))
        {
            queryable = queryable.Where(c => c.OemCode.Code == oemCode);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            queryable = queryable.Where(c => c.EntityType == entityType);
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(c => c.Status == status.Value);
        }

        var customizations = await AsyncExecuter.ToListAsync(
            queryable.OrderByDescending(c => c.CreationTime)
        );

        return ObjectMapper.Map<List<OemCustomization>, List<OemCustomizationDto>>(customizations);
    }

    /// <summary>
    /// ページネーション対応のOEMカスタマイズ取得
    /// </summary>
    public async Task<PagedResultDto<OemCustomizationDto>> GetOemCustomizationsPagedAsync(
        int skipCount = 0,
        int maxResultCount = 10,
        string? oemCode = null,
        string? entityType = null,
        CustomizationStatus? status = null)
    {
        var queryable = await _customizationRepository.GetQueryableAsync();

        if (!string.IsNullOrEmpty(oemCode))
        {
            queryable = queryable.Where(c => c.OemCode.Code == oemCode);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            queryable = queryable.Where(c => c.EntityType == entityType);
        }

        if (status.HasValue)
        {
            queryable = queryable.Where(c => c.Status == status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var customizations = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(c => c.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
        );

        var dtos = ObjectMapper.Map<List<OemCustomization>, List<OemCustomizationDto>>(customizations);
        return new PagedResultDto<OemCustomizationDto>(totalCount, dtos);
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.SubmitCustomization)]
    public async Task<OemCustomizationDto> SubmitForApprovalAsync(Guid id)
    {
        var customization = await _customizationRepository.GetAsync(id);

        customization.SubmitForApproval();

        await _customizationRepository.UpdateAsync(customization);
        return ObjectMapper.Map<OemCustomization, OemCustomizationDto>(customization);
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.ApproveCustomization)]
    public async Task<OemCustomizationDto> ApproveCustomizationAsync(Guid id, string? approvalNotes = null)
    {
        var customization = await _customizationRepository.GetAsync(id);

        customization.Approve(CurrentUser.GetId(), approvalNotes);

        await _customizationRepository.UpdateAsync(customization);
        return ObjectMapper.Map<OemCustomization, OemCustomizationDto>(customization);
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.RejectCustomization)]
    public async Task<OemCustomizationDto> RejectCustomizationAsync(Guid id, string rejectionNotes)
    {
        var customization = await _customizationRepository.GetAsync(id);

        customization.Reject(CurrentUser.GetId(), rejectionNotes);

        await _customizationRepository.UpdateAsync(customization);
        return ObjectMapper.Map<OemCustomization, OemCustomizationDto>(customization);
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.ManageApprovals)]
    public async Task<Guid> CreateOemApprovalAsync(CreateOemApprovalDto input)
    {
        var oemCode = new OemCode(input.OemCode, input.OemCode);

        var approval = new OemApproval(
            CurrentTenant.Id,
            input.EntityId,
            input.EntityType,
            oemCode,
            input.Type,
            CurrentUser.GetId(),
            input.ApprovalReason,
            input.ApprovalData,
            input.DueDate,
            input.Priority);

        await _approvalRepository.InsertAsync(approval);

        // 監査ログを記録
        await _auditLogService.LogCreateAsync(
            approval.Id,
            "OemApproval",
            approval,
            new Dictionary<string, object>
            {
                ["EntityType"] = input.EntityType,
                ["EntityId"] = input.EntityId,
                ["OemCode"] = input.OemCode,
                ["ApprovalType"] = input.Type.ToString(),
                ["Priority"] = input.Priority
            });

        return approval.Id;
    }

    public async Task<OemApprovalDto> GetOemApprovalAsync(Guid id)
    {
        var approval = await _approvalRepository.GetAsync(id);
        var dto = ObjectMapper.Map<OemApproval, OemApprovalDto>(approval);

        // Set computed properties
        dto.IsOverdue = approval.IsOverdue();
        dto.IsUrgent = approval.IsUrgent();

        return dto;
    }

    public async Task<List<OemApprovalDto>> GetPendingApprovalsAsync(string oemCode)
    {
        var approvals = await _approvalRepository.GetPendingApprovalsAsync(oemCode);
        var dtos = ObjectMapper.Map<List<OemApproval>, List<OemApprovalDto>>(approvals);

        // Set computed properties
        foreach (var dto in dtos)
        {
            var approval = approvals.First(a => a.Id == dto.Id);
            dto.IsOverdue = approval.IsOverdue();
            dto.IsUrgent = approval.IsUrgent();
        }

        return dtos;
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.ManageApprovals)]
    public async Task<OemApprovalDto> ApproveAsync(Guid id, string? approvalNotes = null)
    {
        var approval = await _approvalRepository.GetAsync(id);

        approval.Approve(CurrentUser.GetId(), approvalNotes);

        await _approvalRepository.UpdateAsync(approval);

        var dto = ObjectMapper.Map<OemApproval, OemApprovalDto>(approval);
        dto.IsOverdue = approval.IsOverdue();
        dto.IsUrgent = approval.IsUrgent();

        return dto;
    }

    [Authorize(AnomalyDetectionPermissions.OemTraceability.ManageApprovals)]
    public async Task<OemApprovalDto> RejectApprovalAsync(Guid id, string rejectionNotes)
    {
        var approval = await _approvalRepository.GetAsync(id);

        approval.Reject(CurrentUser.GetId(), rejectionNotes);

        await _approvalRepository.UpdateAsync(approval);

        var dto = ObjectMapper.Map<OemApproval, OemApprovalDto>(approval);
        dto.IsOverdue = approval.IsOverdue();
        dto.IsUrgent = approval.IsUrgent();

        return dto;
    }

    public async Task<List<OemApprovalDto>> GetUrgentApprovalsAsync(string? oemCode = null)
    {
        var approvals = await _approvalRepository.GetUrgentApprovalsAsync(oemCode);
        if (approvals == null)
        {
            return new List<OemApprovalDto>();
        }

        var dtos = ObjectMapper.Map<List<OemApproval>, List<OemApprovalDto>>(approvals);

        // Set computed properties
        foreach (var dto in dtos)
        {
            var approval = approvals.FirstOrDefault(a => a.Id == dto.Id);
            if (approval != null)
            {
                dto.IsOverdue = approval.IsOverdue();
                dto.IsUrgent = approval.IsUrgent();
            }
        }

        return dtos;
    }

    public async Task<List<OemApprovalDto>> GetOverdueApprovalsAsync(string? oemCode = null)
    {
        var approvals = await _approvalRepository.GetOverdueApprovalsAsync(oemCode);
        if (approvals == null)
        {
            return new List<OemApprovalDto>();
        }

        var dtos = ObjectMapper.Map<List<OemApproval>, List<OemApprovalDto>>(approvals);

        // Set computed properties
        foreach (var dto in dtos)
        {
            var approval = approvals.FirstOrDefault(a => a.Id == dto.Id);
            if (approval != null)
            {
                dto.IsOverdue = approval.IsOverdue();
                dto.IsUrgent = approval.IsUrgent();
            }
        }

        return dtos;
    }

    public async Task<OemTraceabilityReportDto> GenerateOemTraceabilityReportAsync(GenerateOemTraceabilityReportDto input)
    {
        // Query OEM traceability data based on filters
        var queryable = await _customizationRepository.GetQueryableAsync();

        if (input.EntityId.HasValue)
        {
            queryable = queryable.Where(c => c.EntityId == input.EntityId.Value);
        }

        if (!string.IsNullOrEmpty(input.EntityType))
        {
            queryable = queryable.Where(c => c.EntityType == input.EntityType);
        }

        if (!string.IsNullOrEmpty(input.OemCode))
        {
            queryable = queryable.Where(c => c.OemCode.Code == input.OemCode);
        }

        if (input.StartDate.HasValue)
        {
            queryable = queryable.Where(c => c.CreationTime >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            queryable = queryable.Where(c => c.CreationTime <= input.EndDate.Value);
        }

        var customizations = await AsyncExecuter.ToListAsync(
            queryable.OrderByDescending(c => c.CreationTime)
        );

        // Prepare export data
        var exportData = customizations.Select(c => new
        {
            c.Id,
            EntityId = c.EntityId,
            EntityType = c.EntityType,
            OemCode = c.OemCode.Code,
            OemName = c.OemCode.Name,
            CustomizationType = c.Type.ToString(),
            Status = c.Status.ToString(),
            CustomizationReason = c.CustomizationReason,
            OriginalParameters = System.Text.Json.JsonSerializer.Serialize(c.OriginalParameters),
            CustomParameters = System.Text.Json.JsonSerializer.Serialize(c.CustomParameters),
            CreatedAt = c.CreationTime,
            ApprovedBy = c.ApprovedBy,
            ApprovedAt = c.ApprovedAt,
            ApprovalNotes = c.ApprovalNotes
        }).Select(x => (object)x).ToList();

        // Determine export format
        var exportFormat = input.ReportFormat.ToUpperInvariant() switch
        {
            "CSV" => ExportService.ExportFormat.Csv,
            "EXCEL" => ExportService.ExportFormat.Excel,
            "XLSX" => ExportService.ExportFormat.Excel,
            "JSON" => ExportService.ExportFormat.Json,
            "PDF" => ExportService.ExportFormat.Pdf,
            _ => ExportService.ExportFormat.Csv
        };

        // Create export request
        var exportRequest = new ExportDetectionRequest
        {
            Results = exportData,
            Format = exportFormat,
            FileNamePrefix = "oem_traceability_report",
            CsvOptions = new CsvExportOptions
            {
                IncludeHeader = true,
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss",
                ExcludedProperties = new List<string> { "Id" }
            },
            JsonOptions = new JsonExportOptions
            {
                Indented = true,
                CamelCase = true
            },
            ExcelOptions = new ExcelExportOptions
            {
                IncludeHeader = true,
                EnableAutoFilter = true
            },
            GeneratedBy = CurrentUser.UserName ?? CurrentUser.Id?.ToString() ?? "System",
            AdditionalMetadata = new Dictionary<string, string>
            {
                ["export"] = "oem-traceability",
                ["format"] = input.ReportFormat,
                ["total"] = customizations.Count.ToString()
            }
        };

        // Generate export
        var result = await _exportService.ExportDetectionResultsAsync(exportRequest);

        // Return report DTO
        return new OemTraceabilityReportDto
        {
            ReportId = Guid.NewGuid().ToString(),
            FileName = result.FileName,
            ContentType = result.ContentType,
            Content = result.Data,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = CurrentUser.UserName ?? "System"
        };
    }

    public async Task<Dictionary<CustomizationType, int>> GetCustomizationStatisticsAsync(string? oemCode = null)
    {
        return await _customizationRepository.GetCustomizationStatisticsAsync(oemCode);
    }

    public async Task<Dictionary<ApprovalStatus, int>> GetApprovalStatisticsAsync(string? oemCode = null)
    {
        return await _approvalRepository.GetApprovalStatisticsAsync(oemCode);
    }
}