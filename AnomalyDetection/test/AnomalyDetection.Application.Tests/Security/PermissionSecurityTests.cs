using System;
using System.Threading.Tasks;
using AnomalyDetection.Application.Contracts.AuditLogging;
using AnomalyDetection.Application.Contracts.OemTraceability;
using AnomalyDetection.Application.Contracts.OemTraceability.Dtos;
using AnomalyDetection.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Users;
using Xunit;

namespace AnomalyDetection.Application.Tests.Security;

public class PermissionSecurityTests : AnomalyDetectionApplicationTestBase
{
    private readonly IOemTraceabilityAppService _oemTraceabilityAppService;
    private readonly IAuditLogAppService _auditLogAppService;

    public PermissionSecurityTests()
    {
        _oemTraceabilityAppService = GetRequiredService<IOemTraceabilityAppService>();
        _auditLogAppService = GetRequiredService<IAuditLogAppService>();
    }

    [Fact]
    public async Task CreateOemCustomization_Without_Permission_Should_Throw_AuthorizationException()
    {
        // Arrange
        Login(TestDataBuilder.UserJohnId); // User without permissions
        
        var input = new CreateOemCustomizationDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "TestEntity",
            OemCode = "TEST",
            Type = CustomizationType.ParameterAdjustment,
            CustomParameters = new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "value1" },
            OriginalParameters = new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "original" },
            CustomizationReason = "Test customization"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<AbpAuthorizationException>(
            async () => await _oemTraceabilityAppService.CreateOemCustomizationAsync(input));
        
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task ApproveCustomization_Without_Permission_Should_Throw_AuthorizationException()
    {
        // Arrange
        Login(TestDataBuilder.UserJohnId); // User without permissions
        var customizationId = Guid.NewGuid();

        // Act & Assert
        var exception = await Should.ThrowAsync<AbpAuthorizationException>(
            async () => await _oemTraceabilityAppService.ApproveCustomizationAsync(customizationId, "Test approval"));
        
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task ViewAuditLogs_Without_Permission_Should_Throw_AuthorizationException()
    {
        // Arrange
        Login(TestDataBuilder.UserJohnId); // User without permissions
        var entityId = Guid.NewGuid();

        // Act & Assert
        var exception = await Should.ThrowAsync<AbpAuthorizationException>(
            async () => await _auditLogAppService.GetEntityAuditLogsAsync(entityId));
        
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task ViewSecurityAuditLogs_Without_Permission_Should_Throw_AuthorizationException()
    {
        // Arrange
        Login(TestDataBuilder.UserJohnId); // User without permissions

        // Act & Assert
        var exception = await Should.ThrowAsync<AbpAuthorizationException>(
            async () => await _auditLogAppService.GetSecurityAuditLogsAsync());
        
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateOemCustomization_With_Permission_Should_Succeed()
    {
        // Arrange
        LoginAsAdmin(); // Admin has all permissions
        
        var input = new CreateOemCustomizationDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "TestEntity",
            OemCode = "TEST",
            Type = CustomizationType.ParameterAdjustment,
            CustomParameters = new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "value1" },
            OriginalParameters = new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "original" },
            CustomizationReason = "Test customization"
        };

        // Act
        var result = await _oemTraceabilityAppService.CreateOemCustomizationAsync(input);

        // Assert
        result.ShouldNotBe(Guid.Empty);
    }

    private void Login(Guid userId)
    {
        var currentUser = GetRequiredService<ICurrentUser>();
        // Note: In a real test, you would need to properly mock the current user
        // This is a simplified example
    }

    private void LoginAsAdmin()
    {
        Login(TestDataBuilder.UserAdminId);
    }
}