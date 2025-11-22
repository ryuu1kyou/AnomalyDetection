using System;
using Shouldly;
using Xunit;
using AnomalyDetection.Safety;
using AnomalyDetection.AnomalyDetection;

namespace AnomalyDetection.Domain.Tests.Safety;

public class SafetyTraceRecord_AsilLevel_Tests : AnomalyDetectionDomainTestBase<AnomalyDetectionDomainTestModule>
{
    [Fact]
    public void UpdateAsilLevel_SameValue_Should_Audit_And_Not_Change_State()
    {
        var userId = Guid.NewGuid();
        var record = new SafetyTraceRecord(Guid.NewGuid(), "REQ-100", "SG-1", AsilLevel.B, "Test Record")
        {
            ApprovalStatus = ApprovalStatus.Submitted
        };
        var initialVersion = record.Version;
        var initialAuditCount = record.AuditTrail.Count;

        record.UpdateAsilLevel(AsilLevel.B, userId, "No change");

        record.AsilLevel.ShouldBe(AsilLevel.B);
        record.Version.ShouldBe(initialVersion); // version unchanged
        record.AuditTrail.Count.ShouldBe(initialAuditCount + 1);
    var lastAction = record.AuditTrail[^1].Action;
    (lastAction == "ASIL level updated" || lastAction == "ASIL level change attempted with same value").ShouldBeTrue();
    }

    [Fact]
    public void UpdateAsilLevel_From_Approved_HigherLevel_Should_Trigger_ReReview()
    {
        var approverId = Guid.NewGuid();
        var record = new SafetyTraceRecord(Guid.NewGuid(), "REQ-200", "SG-2", AsilLevel.B, "Approved Record")
        {
            ApprovalStatus = ApprovalStatus.Approved,
            ApprovedAt = DateTime.UtcNow.AddMinutes(-5),
            ApprovedBy = approverId,
            ApprovalComments = "Initial approval"
        };

        record.UpdateAsilLevel(AsilLevel.C, Guid.NewGuid(), "Risk increased");

        record.AsilLevel.ShouldBe(AsilLevel.C);
        record.ApprovalStatus.ShouldBe(ApprovalStatus.UnderReview);
        record.ApprovedAt.ShouldBeNull();
        record.ApprovedBy.ShouldBeNull();
    }

    [Fact]
    public void UpdateAsilLevel_From_Rejected_LowerLevel_Should_Set_Submitted()
    {
        var record = new SafetyTraceRecord(Guid.NewGuid(), "REQ-300", "SG-3", AsilLevel.C, "Rejected Record")
        {
            ApprovalStatus = ApprovalStatus.Rejected
        };

        record.UpdateAsilLevel(AsilLevel.B, Guid.NewGuid(), "Downgraded risk");

        record.AsilLevel.ShouldBe(AsilLevel.B);
        record.ApprovalStatus.ShouldBe(ApprovalStatus.Submitted);
    }
}
