using System;
using System.Collections.Generic;
using AnomalyDetection.MultiTenancy;
using AnomalyDetection.OemTraceability;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace AnomalyDetection.OemTraceability;

public class OemApprovalTests : AnomalyDetectionDomainTestBase<AnomalyDetectionDomainTestModule>
{
    [Fact]
    public void Should_Create_OemApproval_With_Valid_Parameters()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var entityType = "CanSignal";
        var oemCode = new OemCode("TOYOTA", "Toyota Motor Corporation");
        var approvalType = ApprovalType.Customization;
        var requestedBy = Guid.NewGuid();
        var approvalReason = "Need approval for customization";
        var approvalData = new Dictionary<string, object> { { "customField", "value" } };
        var dueDate = DateTime.UtcNow.AddDays(7);
        var priority = 3;

        // Act
        var approval = new OemApproval(
            tenantId,
            entityId,
            entityType,
            oemCode,
            approvalType,
            requestedBy,
            approvalReason,
            approvalData,
            dueDate,
            priority
        );

        // Assert
        approval.TenantId.ShouldBe(tenantId);
        approval.EntityId.ShouldBe(entityId);
        approval.EntityType.ShouldBe(entityType);
        approval.OemCode.ShouldBe(oemCode);
        approval.Type.ShouldBe(approvalType);
        approval.RequestedBy.ShouldBe(requestedBy);
        approval.ApprovalReason.ShouldBe(approvalReason);
        approval.ApprovalData.ShouldBe(approvalData);
        approval.DueDate.ShouldBe(dueDate);
        approval.Priority.ShouldBe(priority);
        approval.Status.ShouldBe(ApprovalStatus.Pending);
        approval.ApprovedBy.ShouldBeNull();
        approval.ApprovedAt.ShouldBeNull();
    }

    [Fact]
    public void Should_Create_OemApproval_With_Default_Priority()
    {
        // Arrange & Act
        var approval = CreateTestApproval();

        // Assert
        approval.Priority.ShouldBe(2); // Default priority
    }

    [Fact]
    public void Approve_Should_Set_Approval_Details_And_Change_Status()
    {
        // Arrange
        var approval = CreateTestApproval();
        var approverId = Guid.NewGuid();
        var approvalNotes = "Approved for production use";

        // Act
        approval.Approve(approverId, approvalNotes);

        // Assert
        approval.Status.ShouldBe(ApprovalStatus.Approved);
        approval.ApprovedBy.ShouldBe(approverId);
        approval.ApprovedAt.ShouldNotBeNull();
        approval.ApprovalNotes.ShouldBe(approvalNotes);
    }

    [Fact]
    public void Approve_Should_Work_Without_Notes()
    {
        // Arrange
        var approval = CreateTestApproval();
        var approverId = Guid.NewGuid();

        // Act
        approval.Approve(approverId);

        // Assert
        approval.Status.ShouldBe(ApprovalStatus.Approved);
        approval.ApprovedBy.ShouldBe(approverId);
        approval.ApprovalNotes.ShouldBeNull();
    }

    [Fact]
    public void Approve_Should_Throw_Exception_When_Not_Pending()
    {
        // Arrange
        var approval = CreateTestApproval();
        var approverId = Guid.NewGuid();
        approval.Approve(approverId);

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => approval.Approve(Guid.NewGuid()));
        exception.Code.ShouldBe("AnomalyDetection:ApprovalNotPending");
    }

    [Fact]
    public void Reject_Should_Set_Rejection_Details_And_Change_Status()
    {
        // Arrange
        var approval = CreateTestApproval();
        var rejectedBy = Guid.NewGuid();
        var rejectionNotes = "Does not meet safety requirements";

        // Act
        approval.Reject(rejectedBy, rejectionNotes);

        // Assert
        approval.Status.ShouldBe(ApprovalStatus.Rejected);
        approval.ApprovedBy.ShouldBe(rejectedBy);
        approval.ApprovedAt.ShouldNotBeNull();
        approval.ApprovalNotes.ShouldBe(rejectionNotes);
    }

    [Fact]
    public void Reject_Should_Throw_Exception_When_Not_Pending()
    {
        // Arrange
        var approval = CreateTestApproval();
        var rejectedBy = Guid.NewGuid();
        approval.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() =>
            approval.Reject(rejectedBy, "Rejection reason"));
        exception.Code.ShouldBe("AnomalyDetection:ApprovalNotPending");
    }

    [Fact]
    public void Reject_Should_Throw_Exception_When_Notes_Is_Empty()
    {
        // Arrange
        var approval = CreateTestApproval();
        var rejectedBy = Guid.NewGuid();

        // Act & Assert
        Should.Throw<ArgumentException>(() => approval.Reject(rejectedBy, ""));
    }

    [Fact]
    public void Cancel_Should_Set_Cancellation_Details_And_Change_Status()
    {
        // Arrange
        var approval = CreateTestApproval();
        var cancelledBy = Guid.NewGuid();
        var cancellationReason = "Request withdrawn by requester";

        // Act
        approval.Cancel(cancelledBy, cancellationReason);

        // Assert
        approval.Status.ShouldBe(ApprovalStatus.Cancelled);
        approval.ApprovedBy.ShouldBe(cancelledBy);
        approval.ApprovedAt.ShouldNotBeNull();
        approval.ApprovalNotes.ShouldNotBeNull();
        approval.ApprovalNotes.ShouldContain("Cancelled:");
        approval.ApprovalNotes.ShouldContain(cancellationReason);
    }

    [Fact]
    public void Cancel_Should_Throw_Exception_When_Not_Pending()
    {
        // Arrange
        var approval = CreateTestApproval();
        var cancelledBy = Guid.NewGuid();
        approval.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() =>
            approval.Cancel(cancelledBy, "Cancellation reason"));
        exception.Code.ShouldBe("AnomalyDetection:ApprovalNotPending");
    }

    [Fact]
    public void Cancel_Should_Throw_Exception_When_Reason_Is_Empty()
    {
        // Arrange
        var approval = CreateTestApproval();
        var cancelledBy = Guid.NewGuid();

        // Act & Assert
        Should.Throw<ArgumentException>(() => approval.Cancel(cancelledBy, ""));
    }

    [Fact]
    public void UpdateDueDate_Should_Update_DueDate()
    {
        // Arrange
        var approval = CreateTestApproval();
        var newDueDate = DateTime.UtcNow.AddDays(14);

        // Act
        approval.UpdateDueDate(newDueDate);

        // Assert
        approval.DueDate.ShouldBe(newDueDate);
    }

    [Fact]
    public void UpdateDueDate_Should_Allow_Null()
    {
        // Arrange
        var approval = CreateTestApproval();

        // Act
        approval.UpdateDueDate(null);

        // Assert
        approval.DueDate.ShouldBeNull();
    }

    [Fact]
    public void UpdateDueDate_Should_Throw_Exception_When_Not_Pending()
    {
        // Arrange
        var approval = CreateTestApproval();
        approval.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() =>
            approval.UpdateDueDate(DateTime.UtcNow.AddDays(7)));
        exception.Code.ShouldBe("AnomalyDetection:CannotUpdateCompletedApproval");
    }

    [Fact]
    public void UpdatePriority_Should_Update_Priority()
    {
        // Arrange
        var approval = CreateTestApproval();
        var newPriority = 4;

        // Act
        approval.UpdatePriority(newPriority);

        // Assert
        approval.Priority.ShouldBe(newPriority);
    }

    [Fact]
    public void UpdatePriority_Should_Throw_Exception_When_Priority_Out_Of_Range()
    {
        // Arrange
        var approval = CreateTestApproval();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => approval.UpdatePriority(0));
        Should.Throw<ArgumentOutOfRangeException>(() => approval.UpdatePriority(5));
    }

    [Fact]
    public void UpdatePriority_Should_Throw_Exception_When_Not_Pending()
    {
        // Arrange
        var approval = CreateTestApproval();
        approval.Approve(Guid.NewGuid());

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() => approval.UpdatePriority(3));
        exception.Code.ShouldBe("AnomalyDetection:CannotUpdateCompletedApproval");
    }

    [Fact]
    public void UpdateApprovalData_Should_Update_Data_And_Reason()
    {
        // Arrange
        var approval = CreateTestApproval();
        var newData = new Dictionary<string, object> { { "newField", "newValue" } };
        var updateReason = "Additional information provided";

        // Act
        approval.UpdateApprovalData(newData, updateReason);

        // Assert
        approval.ApprovalData.ShouldBe(newData);
        approval.ApprovalReason.ShouldNotBeNull();
        approval.ApprovalReason.ShouldContain(updateReason);
    }

    [Fact]
    public void UpdateApprovalData_Should_Throw_Exception_When_Not_Pending()
    {
        // Arrange
        var approval = CreateTestApproval();
        approval.Approve(Guid.NewGuid());
        var newData = new Dictionary<string, object> { { "field", "value" } };

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() =>
            approval.UpdateApprovalData(newData, "Update reason"));
        exception.Code.ShouldBe("AnomalyDetection:CannotUpdateCompletedApproval");
    }

    [Fact]
    public void IsOverdue_Should_Return_True_When_Past_DueDate()
    {
        // Arrange
        var approval = new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval",
            null,
            DateTime.UtcNow.AddDays(-1), // Past due date
            2
        );

        // Act
        var isOverdue = approval.IsOverdue();

        // Assert
        isOverdue.ShouldBeTrue();
    }

    [Fact]
    public void IsOverdue_Should_Return_False_When_Before_DueDate()
    {
        // Arrange
        var approval = new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval",
            null,
            DateTime.UtcNow.AddDays(7), // Future due date
            2
        );

        // Act
        var isOverdue = approval.IsOverdue();

        // Assert
        isOverdue.ShouldBeFalse();
    }

    [Fact]
    public void IsOverdue_Should_Return_False_When_No_DueDate()
    {
        // Arrange
        var approval = CreateTestApproval();

        // Act
        var isOverdue = approval.IsOverdue();

        // Assert
        isOverdue.ShouldBeFalse();
    }

    [Fact]
    public void IsOverdue_Should_Return_False_When_Not_Pending()
    {
        // Arrange
        var approval = new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval",
            null,
            DateTime.UtcNow.AddDays(-1),
            2
        );
        approval.Approve(Guid.NewGuid());

        // Act
        var isOverdue = approval.IsOverdue();

        // Assert
        isOverdue.ShouldBeFalse();
    }

    [Fact]
    public void IsUrgent_Should_Return_True_When_Priority_Is_4()
    {
        // Arrange
        var approval = new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval",
            null,
            null,
            4 // Urgent priority
        );

        // Act
        var isUrgent = approval.IsUrgent();

        // Assert
        isUrgent.ShouldBeTrue();
    }

    [Fact]
    public void IsUrgent_Should_Return_True_When_DueDate_Within_One_Day()
    {
        // Arrange
        var approval = new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval",
            null,
            DateTime.UtcNow.AddHours(12), // Within 1 day
            2
        );

        // Act
        var isUrgent = approval.IsUrgent();

        // Assert
        isUrgent.ShouldBeTrue();
    }

    [Fact]
    public void IsUrgent_Should_Return_False_When_Not_Urgent()
    {
        // Arrange
        var approval = new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval",
            null,
            DateTime.UtcNow.AddDays(7),
            2
        );

        // Act
        var isUrgent = approval.IsUrgent();

        // Assert
        isUrgent.ShouldBeFalse();
    }

    private static OemApproval CreateTestApproval()
    {
        return new OemApproval(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CanSignal",
            new OemCode("TOYOTA", "Toyota Motor Corporation"),
            ApprovalType.Customization,
            Guid.NewGuid(),
            "Test approval reason"
        );
    }
}
