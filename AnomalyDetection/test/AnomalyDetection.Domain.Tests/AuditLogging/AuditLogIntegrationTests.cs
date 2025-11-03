using System;
using AnomalyDetection.AuditLogging.Handlers;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.OemTraceability;
using AnomalyDetection.OemTraceability.Events;
using Shouldly;
using Volo.Abp.EventBus.Local;
using Xunit;

namespace AnomalyDetection.Domain.Tests.AuditLogging;

public class AuditLogIntegrationTests : AnomalyDetectionDomainTestBase<AnomalyDetectionDomainTestModule>
{
    private readonly ILocalEventBus _localEventBus;

    public AuditLogIntegrationTests()
    {
        _localEventBus = GetRequiredService<ILocalEventBus>();
    }

    [Fact]
    public void OemCustomization_Approve_Should_Trigger_AuditLog_Event()
    {
        // Arrange
        var oemCode = new OemCode("TEST", "Test OEM");
        var customization = new OemCustomization(
            tenantId: null,
            entityId: Guid.NewGuid(),
            entityType: "TestEntity",
            oemCode: oemCode,
            type: CustomizationType.ParameterAdjustment,
            customParameters: new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "value1" },
            originalParameters: new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "original" },
            customizationReason: "Test customization"
        );

        // Submit for approval first
        customization.SubmitForApproval();

        var approvedBy = Guid.NewGuid();
        var approvalNotes = "Test approval";

        // Act
        customization.Approve(approvedBy, approvalNotes);

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Approved);
        customization.ApprovedBy.ShouldBe(approvedBy);
        customization.ApprovalNotes.ShouldBe(approvalNotes);

        // Verify that the domain event was added
        var domainEvents = customization.GetLocalEvents();
        domainEvents.ShouldNotBeEmpty();
        domainEvents.ShouldContain(e => e.GetType() == typeof(OemCustomizationApprovedEvent));
    }

    [Fact]
    public void OemCustomization_Reject_Should_Trigger_AuditLog_Event()
    {
        // Arrange
        var oemCode = new OemCode("TEST", "Test OEM");
        var customization = new OemCustomization(
            tenantId: null,
            entityId: Guid.NewGuid(),
            entityType: "TestEntity",
            oemCode: oemCode,
            type: CustomizationType.ParameterAdjustment,
            customParameters: new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "value1" },
            originalParameters: new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "original" },
            customizationReason: "Test customization"
        );

        // Submit for approval first
        customization.SubmitForApproval();

        var rejectedBy = Guid.NewGuid();
        var rejectionNotes = "Test rejection";

        // Act
        customization.Reject(rejectedBy, rejectionNotes);

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Rejected);
        customization.ApprovedBy.ShouldBe(rejectedBy);
        customization.ApprovalNotes.ShouldBe(rejectionNotes);

        // Verify that the domain event was added
        var domainEvents = customization.GetLocalEvents();
        domainEvents.ShouldNotBeEmpty();
        domainEvents.ShouldContain(e => e.GetType() == typeof(OemCustomizationRejectedEvent));
    }

    [Fact]
    public void OemApproval_Approve_Should_Trigger_AuditLog_Event()
    {
        // Arrange
        var oemCode = new OemCode("TEST", "Test OEM");
        var approval = new OemApproval(
            tenantId: null,
            entityId: Guid.NewGuid(),
            entityType: "TestEntity",
            oemCode: oemCode,
            type: ApprovalType.Customization,
            requestedBy: Guid.NewGuid(),
            approvalReason: "Test approval request"
        );

        var approvedBy = Guid.NewGuid();
        var approvalNotes = "Test approval";

        // Act
        approval.Approve(approvedBy, approvalNotes);

        // Assert
        approval.Status.ShouldBe(ApprovalStatus.Approved);
        approval.ApprovedBy.ShouldBe(approvedBy);
        approval.ApprovalNotes.ShouldBe(approvalNotes);

        // Verify that the domain event was added
        var domainEvents = approval.GetLocalEvents();
        domainEvents.ShouldNotBeEmpty();
        domainEvents.ShouldContain(e => e.GetType() == typeof(OemApprovalCompletedEvent));
    }
}