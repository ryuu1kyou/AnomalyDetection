using System;
using Volo.Abp.Domain.Entities.Events;

namespace AnomalyDetection.OemTraceability.Events;

/// <summary>
/// OEMカスタマイズ承認イベント
/// </summary>
public class OemCustomizationApprovedEvent : EntityUpdatedEventData<OemCustomization>
{
    public Guid ApprovedBy { get; }
    public string? ApprovalNotes { get; }

    public OemCustomizationApprovedEvent(OemCustomization entity, Guid approvedBy, string? approvalNotes) 
        : base(entity)
    {
        ApprovedBy = approvedBy;
        ApprovalNotes = approvalNotes;
    }
}