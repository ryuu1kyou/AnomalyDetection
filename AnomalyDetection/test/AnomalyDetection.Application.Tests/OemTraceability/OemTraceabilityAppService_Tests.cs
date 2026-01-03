using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AnomalyDetection.Application.Contracts.OemTraceability;
using AnomalyDetection.Application.Contracts.OemTraceability.Dtos;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace AnomalyDetection.OemTraceability;

public class OemTraceabilityAppService_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly IOemTraceabilityAppService _oemTraceabilityAppService;
    private readonly IOemCustomizationRepository _customizationRepository;
    private readonly IOemApprovalRepository _approvalRepository;

    private readonly Dictionary<Guid, OemCustomization> _customizations = new();
    private readonly Dictionary<Guid, OemApproval> _approvals = new();

    public OemTraceabilityAppService_Tests()
    {
        _oemTraceabilityAppService = GetRequiredService<IOemTraceabilityAppService>();
        _customizationRepository = GetRequiredService<IOemCustomizationRepository>();
        _approvalRepository = GetRequiredService<IOemApprovalRepository>();

        SetupCustomizationRepository();
        SetupApprovalRepository();
    }

    private void SetupCustomizationRepository()
    {
        _customizationRepository.InsertAsync(Arg.Any<OemCustomization>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<OemCustomization>();
                _customizations[entity.Id] = entity;
                return Task.FromResult(entity);
            });

        _customizationRepository.UpdateAsync(Arg.Any<OemCustomization>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<OemCustomization>();
                _customizations[entity.Id] = entity;
                return Task.FromResult(entity);
            });

        _customizationRepository.GetAsync(Arg.Any<Guid>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_customizations.TryGetValue(id, out var entity))
                {
                    return Task.FromResult(entity);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(OemCustomization), id);
            });

        _customizationRepository.GetAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_customizations.TryGetValue(id, out var entity))
                {
                    return Task.FromResult(entity);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(OemCustomization), id);
            });

        _customizationRepository.GetListAsync(includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(_customizations.Values.ToList()));

        _customizationRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<OemCustomization, bool>>>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<System.Linq.Expressions.Expression<Func<OemCustomization, bool>>>().Compile();
                return Task.FromResult(_customizations.Values.AsQueryable().Where(predicate).ToList());
            });

        _customizationRepository.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<OemCustomization, bool>>>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<System.Linq.Expressions.Expression<Func<OemCustomization, bool>>>().Compile();
                return Task.FromResult(_customizations.Values.AsQueryable().FirstOrDefault(predicate));
            });

        _customizationRepository.GetListAsync()
            .Returns(callInfo => Task.FromResult(_customizations.Values.ToList()));

        _customizationRepository.GetQueryableAsync()
            .Returns(callInfo => Task.FromResult(_customizations.Values.AsQueryable()));

        _customizationRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                _customizations.Remove(id);
                return Task.CompletedTask;
            });

        _customizationRepository.GetCustomizationStatisticsAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var stats = _customizations.Values
                    .GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => g.Count());
                return Task.FromResult(stats);
            });
    }

    private void SetupApprovalRepository()
    {
        _approvalRepository.InsertAsync(Arg.Any<OemApproval>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<OemApproval>();
                _approvals[entity.Id] = entity;
                return Task.FromResult(entity);
            });

        _approvalRepository.UpdateAsync(Arg.Any<OemApproval>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<OemApproval>();
                _approvals[entity.Id] = entity;
                return Task.FromResult(entity);
            });

        _approvalRepository.GetAsync(Arg.Any<Guid>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_approvals.TryGetValue(id, out var entity))
                {
                    return Task.FromResult(entity);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(OemApproval), id);
            });

        _approvalRepository.GetAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_approvals.TryGetValue(id, out var entity))
                {
                    return Task.FromResult(entity);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(OemApproval), id);
            });

        _approvalRepository.GetListAsync(includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(_approvals.Values.ToList()));

        _approvalRepository.GetListAsync(Arg.Any<System.Linq.Expressions.Expression<Func<OemApproval, bool>>>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<System.Linq.Expressions.Expression<Func<OemApproval, bool>>>().Compile();
                return Task.FromResult(_approvals.Values.AsQueryable().Where(predicate).ToList());
            });

        _approvalRepository.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<OemApproval, bool>>>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var predicate = callInfo.Arg<System.Linq.Expressions.Expression<Func<OemApproval, bool>>>().Compile();
                return Task.FromResult(_approvals.Values.AsQueryable().FirstOrDefault(predicate));
            });

        _approvalRepository.GetListAsync()
            .Returns(callInfo => Task.FromResult(_approvals.Values.ToList()));

        _approvalRepository.GetQueryableAsync()
            .Returns(callInfo => Task.FromResult(_approvals.Values.AsQueryable()));

        _approvalRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                _approvals.Remove(id);
                return Task.CompletedTask;
            });

        _approvalRepository.GetUrgentApprovalsAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(_approvals.Values.Where(a => a.IsUrgent()).ToList()));

        _approvalRepository.GetOverdueApprovalsAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(_approvals.Values.Where(a => a.IsOverdue()).ToList()));

        _approvalRepository.GetApprovalStatisticsAsync(Arg.Any<string>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var stats = _approvals.Values
                    .GroupBy(a => a.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                return Task.FromResult(stats);
            });
    }

    [Fact]
    public async Task Should_Create_OemCustomization()
    {
        // Arrange
        var input = new CreateOemCustomizationDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "CanSignal",
            OemCode = "TOYOTA",
            Type = CustomizationType.ParameterAdjustment,
            CustomParameters = new Dictionary<string, object> { { "threshold", 100 } },
            OriginalParameters = new Dictionary<string, object> { { "threshold", 80 } },
            CustomizationReason = "Adjust threshold for specific vehicle model"
        };

        // Act
        var customizationId = await _oemTraceabilityAppService.CreateOemCustomizationAsync(input);

        // Assert
        customizationId.ShouldNotBe(Guid.Empty);

        var customization = await _customizationRepository.GetAsync(customizationId);
        customization.ShouldNotBeNull();
        customization.EntityId.ShouldBe(input.EntityId);
        customization.EntityType.ShouldBe(input.EntityType);
        customization.Type.ShouldBe(input.Type);
        customization.Status.ShouldBe(CustomizationStatus.Draft);
    }

    [Fact]
    public async Task Should_Update_OemCustomization()
    {
        // Arrange
        var customizationId = await CreateTestCustomizationAsync();
        var updateInput = new UpdateOemCustomizationDto
        {
            CustomParameters = new Dictionary<string, object> { { "threshold", 120 } },
            CustomizationReason = "Updated threshold based on test results"
        };

        // Act
        var result = await _oemTraceabilityAppService.UpdateOemCustomizationAsync(customizationId, updateInput);

        // Assert
        result.ShouldNotBeNull();
        result.CustomParameters["threshold"].ShouldBe(120);
        result.CustomizationReason.ShouldBe(updateInput.CustomizationReason);
        result.Status.ShouldBe(CustomizationStatus.Draft);
    }

    [Fact]
    public async Task Should_Submit_Customization_For_Approval()
    {
        // Arrange
        var customizationId = await CreateTestCustomizationAsync();

        // Act
        var result = await _oemTraceabilityAppService.SubmitForApprovalAsync(customizationId);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(CustomizationStatus.PendingApproval);
    }

    [Fact]
    public async Task Should_Approve_Customization()
    {
        // Arrange
        var customizationId = await CreateTestCustomizationAsync();
        await _oemTraceabilityAppService.SubmitForApprovalAsync(customizationId);

        // Act
        var result = await _oemTraceabilityAppService.ApproveCustomizationAsync(customizationId, "Approved for production");

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(CustomizationStatus.Approved);
        result.ApprovedBy.ShouldNotBeNull();
        result.ApprovedAt.ShouldNotBeNull();
        result.ApprovalNotes.ShouldBe("Approved for production");
    }

    [Fact]
    public async Task Should_Reject_Customization()
    {
        // Arrange
        var customizationId = await CreateTestCustomizationAsync();
        await _oemTraceabilityAppService.SubmitForApprovalAsync(customizationId);

        // Act
        var result = await _oemTraceabilityAppService.RejectCustomizationAsync(customizationId, "Does not meet requirements");

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(CustomizationStatus.Rejected);
        result.ApprovedBy.ShouldNotBeNull();
        result.ApprovedAt.ShouldNotBeNull();
        result.ApprovalNotes.ShouldBe("Does not meet requirements");
    }

    [Fact]
    public async Task Should_Get_OemCustomizations_By_OemCode()
    {
        // Arrange
        await CreateTestCustomizationAsync("TOYOTA");
        await CreateTestCustomizationAsync("TOYOTA");
        await CreateTestCustomizationAsync("HONDA");

        // Act
        var toyotaCustomizations = await _oemTraceabilityAppService.GetOemCustomizationsAsync(oemCode: "TOYOTA");

        // Assert
        toyotaCustomizations.ShouldNotBeNull();
        toyotaCustomizations.Count.ShouldBeGreaterThanOrEqualTo(2);
        toyotaCustomizations.All(c => c.OemCode == "TOYOTA").ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Get_OemCustomizations_By_Status()
    {
        // Arrange
        var customizationId1 = await CreateTestCustomizationAsync();
        var customizationId2 = await CreateTestCustomizationAsync();
        await _oemTraceabilityAppService.SubmitForApprovalAsync(customizationId1);

        // Act
        var draftCustomizations = await _oemTraceabilityAppService.GetOemCustomizationsAsync(status: CustomizationStatus.Draft);
        var pendingCustomizations = await _oemTraceabilityAppService.GetOemCustomizationsAsync(status: CustomizationStatus.PendingApproval);

        // Assert
        draftCustomizations.ShouldNotBeNull();
        draftCustomizations.Any(c => c.Id == customizationId2).ShouldBeTrue();

        pendingCustomizations.ShouldNotBeNull();
        pendingCustomizations.Any(c => c.Id == customizationId1).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Create_OemApproval()
    {
        // Arrange
        var input = new CreateOemApprovalDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "CanSignal",
            OemCode = "TOYOTA",
            Type = ApprovalType.Customization,
            ApprovalReason = "Need approval for customization",
            ApprovalData = new Dictionary<string, object> { { "customField", "value" } },
            DueDate = DateTime.UtcNow.AddDays(7),
            Priority = 3
        };

        // Act
        var approvalId = await _oemTraceabilityAppService.CreateOemApprovalAsync(input);

        // Assert
        approvalId.ShouldNotBe(Guid.Empty);

        var approval = await _approvalRepository.GetAsync(approvalId);
        approval.ShouldNotBeNull();
        approval.EntityId.ShouldBe(input.EntityId);
        approval.EntityType.ShouldBe(input.EntityType);
        approval.Type.ShouldBe(input.Type);
        approval.Status.ShouldBe(ApprovalStatus.Pending);
        approval.Priority.ShouldBe(input.Priority);
    }

    [Fact]
    public async Task Should_Approve_Approval_Request()
    {
        // Arrange
        var approvalId = await CreateTestApprovalAsync();

        // Act
        var result = await _oemTraceabilityAppService.ApproveAsync(approvalId, "Approved");

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApprovalStatus.Approved);
        result.ApprovedBy.ShouldNotBeNull();
        result.ApprovedAt.ShouldNotBeNull();
        result.ApprovalNotes.ShouldBe("Approved");
    }

    [Fact]
    public async Task Should_Reject_Approval_Request()
    {
        // Arrange
        var approvalId = await CreateTestApprovalAsync();

        // Act
        var result = await _oemTraceabilityAppService.RejectApprovalAsync(approvalId, "Rejected due to safety concerns");

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApprovalStatus.Rejected);
        result.ApprovedBy.ShouldNotBeNull();
        result.ApprovedAt.ShouldNotBeNull();
        result.ApprovalNotes.ShouldBe("Rejected due to safety concerns");
    }

    [Fact]
    public async Task Should_Get_Pending_Approvals()
    {
        // Arrange
        await CreateTestApprovalAsync("TOYOTA");
        await CreateTestApprovalAsync("TOYOTA");
        var approvedId = await CreateTestApprovalAsync("TOYOTA");
        await _oemTraceabilityAppService.ApproveAsync(approvedId);

        // Act
        var pendingApprovals = await _oemTraceabilityAppService.GetPendingApprovalsAsync("TOYOTA");

        // Assert
        pendingApprovals.ShouldNotBeNull();
        pendingApprovals.Count.ShouldBeGreaterThanOrEqualTo(2);
        pendingApprovals.All(a => a.Status == ApprovalStatus.Pending).ShouldBeTrue();
        pendingApprovals.All(a => a.OemCode == "TOYOTA").ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Get_Urgent_Approvals()
    {
        // Arrange
        var urgentApprovalId = await CreateTestApprovalAsync("TOYOTA", priority: 4);
        await CreateTestApprovalAsync("TOYOTA", priority: 2);

        // Act
        var urgentApprovals = await _oemTraceabilityAppService.GetUrgentApprovalsAsync("TOYOTA");

        // Assert
        urgentApprovals.ShouldNotBeNull();
        urgentApprovals.Any(a => a.Id == urgentApprovalId).ShouldBeTrue();
        urgentApprovals.All(a => a.IsUrgent).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Get_Overdue_Approvals()
    {
        // Arrange
        var overdueApprovalId = await CreateTestApprovalAsync("TOYOTA", dueDate: DateTime.UtcNow.AddDays(-1));
        await CreateTestApprovalAsync("TOYOTA", dueDate: DateTime.UtcNow.AddDays(7));

        // Act
        var overdueApprovals = await _oemTraceabilityAppService.GetOverdueApprovalsAsync("TOYOTA");

        // Assert
        overdueApprovals.ShouldNotBeNull();
        overdueApprovals.Any(a => a.Id == overdueApprovalId).ShouldBeTrue();
        overdueApprovals.All(a => a.IsOverdue).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Complete_Customization_Workflow()
    {
        // Arrange - Create customization
        var customizationId = await CreateTestCustomizationAsync();

        // Act & Assert - Submit for approval
        var submitted = await _oemTraceabilityAppService.SubmitForApprovalAsync(customizationId);
        submitted.Status.ShouldBe(CustomizationStatus.PendingApproval);

        // Act & Assert - Approve customization
        var approved = await _oemTraceabilityAppService.ApproveCustomizationAsync(customizationId, "Approved after review");
        approved.Status.ShouldBe(CustomizationStatus.Approved);
        approved.ApprovedBy.ShouldNotBeNull();
        approved.ApprovedAt.ShouldNotBeNull();

        // Verify cannot update after approval
        var updateInput = new UpdateOemCustomizationDto
        {
            CustomParameters = new Dictionary<string, object> { { "threshold", 150 } },
            CustomizationReason = "Try to update after approval"
        };

        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _oemTraceabilityAppService.UpdateOemCustomizationAsync(customizationId, updateInput);
        });
    }

    [Fact]
    public async Task Should_Complete_Approval_Workflow()
    {
        // Arrange - Create approval request
        var approvalId = await CreateTestApprovalAsync();

        // Verify initial state
        var initial = await _oemTraceabilityAppService.GetOemApprovalAsync(approvalId);
        initial.Status.ShouldBe(ApprovalStatus.Pending);

        // Act & Assert - Approve request
        var approved = await _oemTraceabilityAppService.ApproveAsync(approvalId, "Approved after review");
        approved.Status.ShouldBe(ApprovalStatus.Approved);
        approved.ApprovedBy.ShouldNotBeNull();
        approved.ApprovedAt.ShouldNotBeNull();

        // Verify cannot approve again
        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _oemTraceabilityAppService.ApproveAsync(approvalId);
        });
    }

    [Fact]
    public async Task Should_Get_Customization_Statistics()
    {
        // Arrange
        await CreateTestCustomizationAsync("TOYOTA", CustomizationType.ParameterAdjustment);
        await CreateTestCustomizationAsync("TOYOTA", CustomizationType.ParameterAdjustment);
        await CreateTestCustomizationAsync("TOYOTA", CustomizationType.ThresholdChange);

        // Act
        var statistics = await _oemTraceabilityAppService.GetCustomizationStatisticsAsync("TOYOTA");

        // Assert
        statistics.ShouldNotBeNull();
        statistics.ShouldContainKey(CustomizationType.ParameterAdjustment);
        statistics[CustomizationType.ParameterAdjustment].ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_Get_Approval_Statistics()
    {
        // Arrange
        var approvalId1 = await CreateTestApprovalAsync("TOYOTA");
        var approvalId2 = await CreateTestApprovalAsync("TOYOTA");
        await _oemTraceabilityAppService.ApproveAsync(approvalId1);

        // Act
        var statistics = await _oemTraceabilityAppService.GetApprovalStatisticsAsync("TOYOTA");

        // Assert
        statistics.ShouldNotBeNull();
        statistics.ShouldContainKey(ApprovalStatus.Pending);
        statistics.ShouldContainKey(ApprovalStatus.Approved);
        statistics[ApprovalStatus.Approved].ShouldBeGreaterThanOrEqualTo(1);
    }

    // Helper methods
    private async Task<Guid> CreateTestCustomizationAsync(string oemCode = "TOYOTA", CustomizationType type = CustomizationType.ParameterAdjustment)
    {
        var input = new CreateOemCustomizationDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "CanSignal",
            OemCode = oemCode,
            Type = type,
            CustomParameters = new Dictionary<string, object> { { "threshold", 100 } },
            OriginalParameters = new Dictionary<string, object> { { "threshold", 80 } },
            CustomizationReason = "Test customization"
        };

        return await _oemTraceabilityAppService.CreateOemCustomizationAsync(input);
    }

    private async Task<Guid> CreateTestApprovalAsync(string oemCode = "TOYOTA", int priority = 2, DateTime? dueDate = null)
    {
        var input = new CreateOemApprovalDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "CanSignal",
            OemCode = oemCode,
            Type = ApprovalType.Customization,
            ApprovalReason = "Test approval",
            ApprovalData = [],
            DueDate = dueDate,
            Priority = priority
        };

        return await _oemTraceabilityAppService.CreateOemApprovalAsync(input);
    }
}
