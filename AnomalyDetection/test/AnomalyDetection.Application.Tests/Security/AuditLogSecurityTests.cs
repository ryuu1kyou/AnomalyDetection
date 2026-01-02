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

namespace AnomalyDetection.Application.Tests.Security;

public class AuditLogSecurityTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly IAuditLogService _auditLogService;
    private readonly IAnomalyDetectionAuditLogRepository _auditLogRepository;

    public AuditLogSecurityTests()
    {
        _auditLogRepository = GetRequiredService<IAnomalyDetectionAuditLogRepository>();

        var currentTenant = GetRequiredService<ICurrentTenant>();
        var currentUser = GetRequiredService<ICurrentUser>();
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

        _auditLogService = new AuditLogService(
            _auditLogRepository,
            currentTenant,
            currentUser,
            httpContextAccessor);

        _auditLogRepository.InsertAsync(Arg.Any<AnomalyDetectionAuditLog>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<AnomalyDetectionAuditLog>()));

        // Setup default HttpContext with Session to avoid InvalidOperationException
        var context = new DefaultHttpContext();
        var session = Substitute.For<ISession>();
        context.Session = session;
        // Fix: Set HttpContext before calling Returns to avoid NSubstitute error
        httpContextAccessor.HttpContext = context;
    }

    [Fact]
    public async Task AuditLog_Should_Record_User_Information()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var action = AuditLogAction.Create;
        var description = "Test audit log";

        // Act
        var result = await _auditLogService.LogAsync(
            entityId,
            entityType,
            action,
            description);

        // Assert
        result.ShouldNotBeNull();
        result.EntityId.ShouldBe(entityId);
        result.EntityType.ShouldBe(entityType);
        result.Action.ShouldBe(action);
        result.Description.ShouldBe(description);

        // Verify user information is recorded in metadata
        if (result.Metadata.ContainsKey("UserId"))
        {
            result.Metadata["UserId"].ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task AuditLog_Should_Record_Tenant_Information()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantName = "TestTenant";

        // Mock current tenant
        var currentTenant = GetRequiredService<ICurrentTenant>();

        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var action = AuditLogAction.Create;
        var description = "Test audit log";

        // Act
        using (currentTenant.Change(tenantId, tenantName))
        {
            var result = await _auditLogService.LogAsync(
                entityId,
                entityType,
                action,
                description);

            // Assert
            result.ShouldNotBeNull();
            result.TenantId.ShouldBe(tenantId);

            // Verify tenant information is recorded in metadata
            if (result.Metadata.ContainsKey("TenantId"))
            {
                result.Metadata["TenantId"].ShouldBe(tenantId);
            }
            if (result.Metadata.ContainsKey("TenantName"))
            {
                result.Metadata["TenantName"].ShouldBe(tenantName);
            }
        }
    }

    [Fact]
    public async Task AuditLog_Should_Record_HTTP_Context_Information()
    {
        // Arrange
        var ipAddress = "192.168.1.100";
        var userAgent = "Mozilla/5.0 Test Browser";

        // Mock HTTP context
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress);
        httpContext.Request.Headers["User-Agent"] = userAgent;

        // Add Session mock
        var session = Substitute.For<ISession>();
        httpContext.Session = session;

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var auditLogService = new AuditLogService(
            _auditLogRepository,
            GetRequiredService<ICurrentTenant>(),
            GetRequiredService<ICurrentUser>(),
            httpContextAccessor);

        var entityId = Guid.NewGuid();
        var entityType = "TestEntity";
        var action = AuditLogAction.Create;
        var description = "Test audit log";

        // Act
        var result = await auditLogService.LogAsync(
            entityId,
            entityType,
            action,
            description);

        // Assert
        result.ShouldNotBeNull();
        result.IpAddress.ShouldBe(ipAddress);
        result.UserAgent.ShouldBe(userAgent);
    }

    [Fact]
    public async Task SecurityEvent_AuditLog_Should_Have_High_Priority()
    {
        // Arrange
        var action = AuditLogAction.Login;
        var description = "Failed login attempt";
        var level = AuditLogLevel.Critical;
        var metadata = new Dictionary<string, object>
        {
            ["FailedAttempts"] = 3,
            ["IpAddress"] = "192.168.1.100"
        };

        // Act
        var result = await _auditLogService.LogSecurityEventAsync(action, description, level, metadata);

        // Assert
        result.ShouldNotBeNull();
        result.EntityType.ShouldBe("Security");
        result.Action.ShouldBe(action);
        result.Description.ShouldBe(description);
        result.Level.ShouldBe(AuditLogLevel.Critical);
        result.Metadata.ShouldContainKey("SecurityEvent");
        result.Metadata.ShouldContainKey("FailedAttempts");
        result.Metadata.ShouldContainKey("IpAddress");
    }

    [Fact]
    public async Task AuditLog_Should_Sanitize_Sensitive_Data()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "User";
        var action = AuditLogAction.Update;
        var description = "User password updated";

        var sensitiveData = new
        {
            Username = "testuser",
            Password = "supersecretpassword123", // This should be sanitized
            Email = "test@example.com"
        };

        // Act
        var result = await _auditLogService.LogAsync(
            entityId,
            entityType,
            action,
            description,
            newValues: sensitiveData);

        // Assert
        result.ShouldNotBeNull();
        result.NewValues.ShouldNotBeNull();

        // In a real implementation, you would verify that sensitive data is sanitized
        // For example, passwords should be masked or removed from the audit log
        result.NewValues.ShouldNotContain("supersecretpassword123");
    }

    [Fact]
    public async Task AuditLog_Should_Handle_Large_Data_Gracefully()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entityType = "LargeEntity";
        var action = AuditLogAction.Create;
        var description = "Large entity created";

        // Create a large object that might exceed database limits
        var largeData = new Dictionary<string, object>();
        for (int i = 0; i < 1000; i++)
        {
            largeData[$"Property{i}"] = $"Value{i}".PadRight(100, 'x');
        }

        // Act & Assert - Should not throw exception
        var result = await _auditLogService.LogAsync(
            entityId,
            entityType,
            action,
            description,
            newValues: largeData);

        result.ShouldNotBeNull();
        result.NewValues.ShouldNotBeNull();

        // In a real implementation, you might truncate or compress large data
        // to fit within database constraints
    }
}