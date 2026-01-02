using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnomalyDetection.AuditLogging;
using AnomalyDetection.Application.AuditLogging;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace AnomalyDetection.Application.Tests.AuditLogging;

public class AuditLogServiceTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly IAuditLogService _auditLogService;
    private readonly IAnomalyDetectionAuditLogRepository _auditLogRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogServiceTests()
    {
        _auditLogRepository = GetRequiredService<IAnomalyDetectionAuditLogRepository>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

        _auditLogService = new AuditLogService(
            _auditLogRepository,
            _currentTenant,
            _currentUser,
            _httpContextAccessor);

        _auditLogRepository.InsertAsync(Arg.Any<AnomalyDetectionAuditLog>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<AnomalyDetectionAuditLog>()));
    }

    [Fact]
    public async Task LogAsync_Should_Create_AuditLog()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var action = AuditLogAction.Create;
        var description = "Test audit log";
        var level = AuditLogLevel.Information;
        var metadata = new Dictionary<string, object> { ["TestKey"] = "TestValue" };

        // Act
        var result = await _auditLogService.LogAsync(
            entityId,
            entityType,
            action,
            description,
            level,
            null,
            new { Name = "Test" },
            metadata);

        // Assert
        result.ShouldNotBeNull();
        result.EntityId.ShouldBe(entityId);
        result.EntityType.ShouldBe(entityType);
        result.Action.ShouldBe(action);
        result.Description.ShouldBe(description);
        result.Level.ShouldBe(level);
        result.Metadata.ShouldContainKey("TestKey");
        result.NewValues.ShouldNotBeNull();
    }

    [Fact]
    public async Task LogCreateAsync_Should_Create_Create_AuditLog()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var entity = new { Name = "Test Entity", Value = 123 };

        // Act
        var result = await _auditLogService.LogCreateAsync(entityId, entityType, entity);

        // Assert
        result.ShouldNotBeNull();
        result.EntityId.ShouldBe(entityId);
        result.EntityType.ShouldBe(entityType);
        result.Action.ShouldBe(AuditLogAction.Create);
        result.Level.ShouldBe(AuditLogLevel.Information);
        result.NewValues.ShouldNotBeNull();
        result.OldValues.ShouldBeNull();
    }

    [Fact]
    public async Task LogUpdateAsync_Should_Create_Update_AuditLog()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var oldEntity = new { Name = "Old Name", Value = 100 };
        var newEntity = new { Name = "New Name", Value = 200 };

        // Act
        var result = await _auditLogService.LogUpdateAsync(entityId, entityType, oldEntity, newEntity);

        // Assert
        result.ShouldNotBeNull();
        result.EntityId.ShouldBe(entityId);
        result.EntityType.ShouldBe(entityType);
        result.Action.ShouldBe(AuditLogAction.Update);
        result.Level.ShouldBe(AuditLogLevel.Information);
        result.OldValues.ShouldNotBeNull();
        result.NewValues.ShouldNotBeNull();
    }

    [Fact]
    public async Task LogDeleteAsync_Should_Create_Delete_AuditLog()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var entity = new { Name = "Deleted Entity", Value = 123 };

        // Act
        var result = await _auditLogService.LogDeleteAsync(entityId, entityType, entity);

        // Assert
        result.ShouldNotBeNull();
        result.EntityId.ShouldBe(entityId);
        result.EntityType.ShouldBe(entityType);
        result.Action.ShouldBe(AuditLogAction.Delete);
        result.Level.ShouldBe(AuditLogLevel.Warning);
        result.OldValues.ShouldNotBeNull();
        result.NewValues.ShouldBeNull();
    }

    [Fact]
    public async Task LogApprovalAsync_Should_Create_Approval_AuditLog()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var approvedBy = Guid.NewGuid();
        var notes = "Approved for testing";

        // Act
        var result = await _auditLogService.LogApprovalAsync(entityId, entityType, approvedBy, notes);

        // Assert
        result.ShouldNotBeNull();
        result.EntityId.ShouldBe(entityId);
        result.EntityType.ShouldBe(entityType);
        result.Action.ShouldBe(AuditLogAction.Approve);
        result.Level.ShouldBe(AuditLogLevel.Information);
        result.Metadata.ShouldContainKey("ApprovedBy");
        result.Metadata.ShouldContainKey("ApprovalNotes");
    }

    [Fact]
    public async Task LogSecurityEventAsync_Should_Create_Security_AuditLog()
    {
        // Arrange
        var action = AuditLogAction.Login;
        var description = "User login attempt";
        var level = AuditLogLevel.Warning;

        // Act
        var result = await _auditLogService.LogSecurityEventAsync(action, description, level);

        // Assert
        result.ShouldNotBeNull();
        result.EntityType.ShouldBe("Security");
        result.Action.ShouldBe(action);
        result.Description.ShouldBe(description);
        result.Level.ShouldBe(level);
        result.Metadata.ShouldContainKey("SecurityEvent");
    }
}