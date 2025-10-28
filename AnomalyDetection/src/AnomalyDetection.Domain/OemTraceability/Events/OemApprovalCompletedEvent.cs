using System;
using Volo.Abp.Domain.Entities.Events;

namespace AnomalyDetection.OemTraceability.Events;

/// <summary>
/// OEM承認完了イベント
/// </summary>
public class OemApprovalCompletedEvent : EntityUpdatedEventData<OemApproval>
{
    public Guid CompletedBy { get; }
    public ApprovalStatus Status { get; }
    public string? Notes { get; }

    public OemApprovalCompletedEvent(OemApproval entity, Guid completedBy, ApprovalStatus status, string? notes) 
        : base(entity)
    {
        CompletedBy = completedBy;
        Status = status;
        Notes = notes;
    }
}