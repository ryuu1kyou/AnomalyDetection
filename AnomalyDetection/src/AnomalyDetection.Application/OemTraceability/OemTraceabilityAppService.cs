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

    public OemTraceabilityAppService(
        TraceabilityQueryService traceabilityQueryService,
        IOemCustomizationRepository customizationRepository,
        IOemApprovalRepository approvalRepository,
        IAuditLogService auditLogService)
    {
        _traceabilityQueryService = traceabilityQueryService;
        _customizationRepository = customizationRepository;
        _approvalRepository = approvalRepository;
        _auditLogService = auditLogService;
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

        var customizations = await queryable
            .OrderByDescending(c => c.CreationTime)
            .ToListAsync();

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

        var totalCount = await queryable.CountAsync();

        var customizations = await queryable
            .OrderByDescending(c => c.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync();

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

    public async Task<List<OemApprovalDto>> GetOverdueApprovalsAsync(string? oemCode = null)
    {
        var approvals = await _approvalRepository.GetOverdueApprovalsAsync(oemCode);
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

    public async Task<OemTraceabilityReportDto> GenerateOemTraceabilityReportAsync(GenerateOemTraceabilityReportDto input)
    {
        // This is a placeholder implementation
        // In a real implementation, you would use a reporting library like FastReport, Crystal Reports, or generate PDF/Excel files

        var reportId = Guid.NewGuid().ToString();
        var fileName = $"OemTraceabilityReport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{input.ReportFormat.ToLower()}";

        // Generate report content based on input parameters
        var reportContent = await GenerateReportContentAsync(input);

        return new OemTraceabilityReportDto
        {
            ReportId = reportId,
            FileName = fileName,
            ContentType = GetContentType(input.ReportFormat),
            Content = reportContent,
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

    private Task<byte[]> GenerateReportContentAsync(GenerateOemTraceabilityReportDto input)
    {
        // Placeholder implementation - generate simple text report
        var content = $"OEM Traceability Report\nGenerated: {DateTime.UtcNow}\n";

        if (input.EntityId.HasValue)
        {
            content += $"Entity ID: {input.EntityId}\n";
        }

        if (!string.IsNullOrEmpty(input.EntityType))
        {
            content += $"Entity Type: {input.EntityType}\n";
        }

        if (!string.IsNullOrEmpty(input.OemCode))
        {
            content += $"OEM Code: {input.OemCode}\n";
        }

        // In a real implementation, you would gather data and format it properly
        content += "\n[Report content would be generated here based on the input parameters]";

        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(content));
    }

    private static string GetContentType(string format)
    {
        return format.ToUpper() switch
        {
            "PDF" => "application/pdf",
            "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "CSV" => "text/csv",
            _ => "text/plain"
        };
    }
}