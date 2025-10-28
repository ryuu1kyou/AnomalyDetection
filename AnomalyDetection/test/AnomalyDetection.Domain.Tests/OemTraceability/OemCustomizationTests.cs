using System;
using System.Collections.Generic;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.OemTraceability;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace AnomalyDetection.OemTraceability;

public class OemCustomizationTests : AnomalyDetectionDomainTestBase<AnomalyDetectionDomainTestModule>
{
    [Fact]
    public void Should_Create_OemCustomization_With_Valid_Parameters()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var entityType = "CanSignal";
        var oemCode = new OemCode("TOYOTA", "Toyota Motor Corporation");
        var customizationType = CustomizationType.ParameterAdjustment;
        var customParameters = new Dictionary<string, object> { { "threshold", 100 } };
        var originalParameters = new Dictionary<string, object> { { "threshold", 80 } };
        var reason = "Adjust threshold for specific vehicle model";

        // Act
        var customization = new OemCustomization(
            tenantId,
            entityId,
            entityType,
            oemCode,
            customizationType,
            customParameters,
            originalParameters,
            reason
        );

        // Assert
        customization.TenantId.ShouldBe(tenantId);
        customization.EntityId.ShouldBe(entityId);
        customization.EntityType.ShouldBe(entityType);
        customization.OemCode.ShouldBe(oemCode);
        customization.Type.ShouldBe(customizationType);
        customization.CustomParameters.ShouldBe(customParameters);
        customization.OriginalParameters.ShouldBe(originalParameters);
        customization.CustomizationReason.ShouldBe(reason);
        customization.Status.ShouldBe(CustomizationStatus.Draft);
        customization.ApprovedBy.ShouldBeNull();
        customization.ApprovedAt.ShouldBeNull();
    }

    [Fact]
    public void SubmitForApproval_Should_Change_Status_From_Draft_To_PendingApproval()
    {
        // Arrange
        var customization = CreateTestCustomization();

        // Act
        customization.SubmitForApproval();

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.PendingApproval);
    }

    [Fact]
    public void SubmitForApproval_Should_Throw_Exception_When_Not_Draft()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => customization.SubmitForApproval());
        exception.Code.ShouldBe("AnomalyDetection:CustomizationNotDraft");
    }

    [Fact]
    public void Approve_Should_Set_Approval_Details_And_Change_Status()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();
        var approverId = Guid.NewGuid();
        var approvalNotes = "Approved for production use";

        // Act
        customization.Approve(approverId, approvalNotes);

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Approved);
        customization.ApprovedBy.ShouldBe(approverId);
        customization.ApprovedAt.ShouldNotBeNull();
        customization.ApprovalNotes.ShouldBe(approvalNotes);
    }

    [Fact]
    public void Approve_Should_Work_Without_Notes()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();
        var approverId = Guid.NewGuid();

        // Act
        customization.Approve(approverId);

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Approved);
        customization.ApprovedBy.ShouldBe(approverId);
        customization.ApprovalNotes.ShouldBeNull();
    }

    [Fact]
    public void Approve_Should_Throw_Exception_When_Not_PendingApproval()
    {
        // Arrange
        var customization = CreateTestCustomization();
        var approverId = Guid.NewGuid();

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => customization.Approve(approverId));
        exception.Code.ShouldBe("AnomalyDetection:CustomizationNotPendingApproval");
    }

    [Fact]
    public void Reject_Should_Set_Rejection_Details_And_Change_Status()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();
        var rejectedBy = Guid.NewGuid();
        var rejectionNotes = "Does not meet safety requirements";

        // Act
        customization.Reject(rejectedBy, rejectionNotes);

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Rejected);
        customization.ApprovedBy.ShouldBe(rejectedBy);
        customization.ApprovedAt.ShouldNotBeNull();
        customization.ApprovalNotes.ShouldBe(rejectionNotes);
    }

    [Fact]
    public void Reject_Should_Throw_Exception_When_Not_PendingApproval()
    {
        // Arrange
        var customization = CreateTestCustomization();
        var rejectedBy = Guid.NewGuid();

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => 
            customization.Reject(rejectedBy, "Rejection reason"));
        exception.Code.ShouldBe("AnomalyDetection:CustomizationNotPendingApproval");
    }

    [Fact]
    public void Reject_Should_Throw_Exception_When_RejectionNotes_Is_Empty()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();
        var rejectedBy = Guid.NewGuid();

        // Act & Assert
        Should.Throw<ArgumentException>(() => customization.Reject(rejectedBy, ""));
    }

    [Fact]
    public void UpdateCustomParameters_Should_Update_Parameters_And_Reason()
    {
        // Arrange
        var customization = CreateTestCustomization();
        var newParameters = new Dictionary<string, object> { { "threshold", 120 } };
        var newReason = "Updated threshold based on test results";

        // Act
        customization.UpdateCustomParameters(newParameters, newReason);

        // Assert
        customization.CustomParameters.ShouldBe(newParameters);
        customization.CustomizationReason.ShouldBe(newReason);
        customization.Status.ShouldBe(CustomizationStatus.Draft);
    }

    [Fact]
    public void UpdateCustomParameters_Should_Change_Status_From_PendingApproval_To_Draft()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();
        var newParameters = new Dictionary<string, object> { { "threshold", 120 } };
        var newReason = "Updated threshold";

        // Act
        customization.UpdateCustomParameters(newParameters, newReason);

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Draft);
    }

    [Fact]
    public void UpdateCustomParameters_Should_Throw_Exception_When_Approved()
    {
        // Arrange
        var customization = CreateTestCustomization();
        customization.SubmitForApproval();
        customization.Approve(Guid.NewGuid());
        var newParameters = new Dictionary<string, object> { { "threshold", 120 } };

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => 
            customization.UpdateCustomParameters(newParameters, "New reason"));
        exception.Code.ShouldBe("AnomalyDetection:CannotUpdateApprovedCustomization");
    }

    [Fact]
    public void UpdateCustomParameters_Should_Throw_Exception_When_Parameters_Is_Null()
    {
        // Arrange
        var customization = CreateTestCustomization();

        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            customization.UpdateCustomParameters(null!, "New reason"));
    }

    [Fact]
    public void UpdateCustomParameters_Should_Throw_Exception_When_Reason_Is_Empty()
    {
        // Arrange
        var customization = CreateTestCustomization();
        var newParameters = new Dictionary<string, object> { { "threshold", 120 } };

        // Act & Assert
        Should.Throw<ArgumentException>(() => 
            customization.UpdateCustomParameters(newParameters, ""));
    }

    [Fact]
    public void MarkAsObsolete_Should_Change_Status_To_Obsolete()
    {
        // Arrange
        var customization = CreateTestCustomization();

        // Act
        customization.MarkAsObsolete();

        // Assert
        customization.Status.ShouldBe(CustomizationStatus.Obsolete);
    }

    [Fact]
    public void MarkAsObsolete_Should_Work_From_Any_Status()
    {
        // Arrange & Act & Assert
        var draftCustomization = CreateTestCustomization();
        draftCustomization.MarkAsObsolete();
        draftCustomization.Status.ShouldBe(CustomizationStatus.Obsolete);

        var pendingCustomization = CreateTestCustomization();
        pendingCustomization.SubmitForApproval();
        pendingCustomization.MarkAsObsolete();
        pendingCustomization.Status.ShouldBe(CustomizationStatus.Obsolete);

        var approvedCustomization = CreateTestCustomization();
        approvedCustomization.SubmitForApproval();
        approvedCustomization.Approve(Guid.NewGuid());
        approvedCustomization.MarkAsObsolete();
        approvedCustomization.Status.ShouldBe(CustomizationStatus.Obsolete);
    }

    private OemCustomization CreateTestCustomization()
    {
        return new OemCustomization(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            CustomizationType.ParameterAdjustment,
            new Dictionary<string, object> { { "threshold", 100 } },
            new Dictionary<string, object> { { "threshold", 80 } },
            "Test customization reason"
        );
    }
}
