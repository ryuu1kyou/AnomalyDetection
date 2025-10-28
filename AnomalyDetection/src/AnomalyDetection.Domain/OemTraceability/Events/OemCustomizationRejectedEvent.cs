using System;
using Volo.Abp.Domain.Entities.Events;

namespace AnomalyDetection.OemTraceability.Events;

/// <summary>
/// OEMカスタマイズ却下イベント
/// </summary>
public class OemCustomizationRejectedEvent : EntityUpdatedEventData<OemCustomization>
{
    public Guid RejectedBy { get; }
    public string RejectionNotes { get; }

    public OemCustomizationRejectedEvent(OemCustomization entity, Guid rejectedBy, string rejectionNotes) 
        : base(entity)
    {
        RejectedBy = rejectedBy;
        RejectionNotes = rejectionNotes;
    }
}