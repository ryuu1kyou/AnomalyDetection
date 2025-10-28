using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.OemTraceability.Events;
using AnomalyDetection.OemTraceability;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace AnomalyDetection.AuditLogging.Handlers;

/// <summary>
/// OEMトレーサビリティ関連の監査ログイベントハンドラー
/// </summary>
public class OemTraceabilityAuditEventHandler : 
    ILocalEventHandler<OemCustomizationApprovedEvent>,
    ILocalEventHandler<OemCustomizationRejectedEvent>,
    ILocalEventHandler<OemApprovalCompletedEvent>,
    ITransientDependency
{
    private readonly IAuditLogService _auditLogService;

    public OemTraceabilityAuditEventHandler(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task HandleEventAsync(OemCustomizationApprovedEvent eventData)
    {
        var metadata = new Dictionary<string, object>
        {
            ["ApprovedBy"] = eventData.ApprovedBy,
            ["EntityType"] = eventData.Entity.EntityType,
            ["CustomizationType"] = eventData.Entity.Type.ToString(),
            ["OemCode"] = eventData.Entity.OemCode.Code
        };

        if (!string.IsNullOrEmpty(eventData.ApprovalNotes))
        {
            metadata["ApprovalNotes"] = eventData.ApprovalNotes;
        }

        await _auditLogService.LogAsync(
            entityId: eventData.Entity.Id,
            entityType: "OemCustomization",
            action: AuditLogAction.Approve,
            description: $"OEM customization approved for {eventData.Entity.EntityType} (ID: {eventData.Entity.EntityId})",
            level: AuditLogLevel.Information,
            oldValues: new { Status = "PendingApproval" },
            newValues: new { Status = "Approved", ApprovedBy = eventData.ApprovedBy, ApprovedAt = DateTime.UtcNow },
            metadata: metadata
        );
    }

    public async Task HandleEventAsync(OemCustomizationRejectedEvent eventData)
    {
        var metadata = new Dictionary<string, object>
        {
            ["RejectedBy"] = eventData.RejectedBy,
            ["RejectionReason"] = eventData.RejectionNotes,
            ["EntityType"] = eventData.Entity.EntityType,
            ["CustomizationType"] = eventData.Entity.Type.ToString(),
            ["OemCode"] = eventData.Entity.OemCode.Code
        };

        await _auditLogService.LogAsync(
            entityId: eventData.Entity.Id,
            entityType: "OemCustomization",
            action: AuditLogAction.Reject,
            description: $"OEM customization rejected for {eventData.Entity.EntityType} (ID: {eventData.Entity.EntityId}): {eventData.RejectionNotes}",
            level: AuditLogLevel.Warning,
            oldValues: new { Status = "PendingApproval" },
            newValues: new { Status = "Rejected", RejectedBy = eventData.RejectedBy, RejectedAt = DateTime.UtcNow },
            metadata: metadata
        );
    }

    public async Task HandleEventAsync(OemApprovalCompletedEvent eventData)
    {
        var action = eventData.Status switch
        {
            ApprovalStatus.Approved => AuditLogAction.Approve,
            ApprovalStatus.Rejected => AuditLogAction.Reject,
            ApprovalStatus.Cancelled => AuditLogAction.Delete,
            _ => AuditLogAction.Update
        };

        var level = eventData.Status switch
        {
            ApprovalStatus.Approved => AuditLogLevel.Information,
            ApprovalStatus.Rejected => AuditLogLevel.Warning,
            ApprovalStatus.Cancelled => AuditLogLevel.Warning,
            _ => AuditLogLevel.Information
        };

        var metadata = new Dictionary<string, object>
        {
            ["CompletedBy"] = eventData.CompletedBy,
            ["ApprovalType"] = eventData.Entity.Type.ToString(),
            ["EntityType"] = eventData.Entity.EntityType,
            ["OemCode"] = eventData.Entity.OemCode.Code,
            ["Priority"] = eventData.Entity.Priority
        };

        if (!string.IsNullOrEmpty(eventData.Notes))
        {
            metadata["Notes"] = eventData.Notes;
        }

        await _auditLogService.LogAsync(
            entityId: eventData.Entity.Id,
            entityType: "OemApproval",
            action: action,
            description: $"OEM approval {eventData.Status.ToString().ToLower()} for {eventData.Entity.EntityType} (ID: {eventData.Entity.EntityId})",
            level: level,
            oldValues: new { Status = "Pending" },
            newValues: new { Status = eventData.Status.ToString(), CompletedBy = eventData.CompletedBy, CompletedAt = DateTime.UtcNow },
            metadata: metadata
        );
    }
}