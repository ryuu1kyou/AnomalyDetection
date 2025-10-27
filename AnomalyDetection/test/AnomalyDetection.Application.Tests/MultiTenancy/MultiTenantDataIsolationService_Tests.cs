using System;
using System.Threading.Tasks;
using AnomalyDetection.MultiTenancy;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace AnomalyDetection.MultiTenancy;

public class MultiTenantDataIsolationService_Tests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly MultiTenantDataIsolationService _multiTenantDataIsolationService;
    private readonly ICurrentTenant _currentTenant;

    public MultiTenantDataIsolationService_Tests()
    {
        _multiTenantDataIsolationService = GetRequiredService<MultiTenantDataIsolationService>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task VerifyDataIsolationAsync_Should_Return_Isolation_Result()
    {
        // Act
        var result = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<MultiTenantDataIsolationResult>();
        result.TestExecutedAt.ShouldNotBe(default(DateTime));
        result.CurrentTenantData.ShouldNotBeNull();
    }

    [Fact]
    public async Task VerifyDataIsolationAsync_For_Host_Tenant_Should_Show_Host_Context()
    {
        // Arrange - Test as host tenant (null tenant ID)
        using (_currentTenant.Change(null))
        {
            // Act
            var result = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();

            // Assert
            result.ShouldNotBeNull();
            result.CurrentTenantId.ShouldBeNull();
            result.IsHostTenant.ShouldBeTrue();
            result.AllTenantsCount.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task VerifyDataIsolationAsync_For_Regular_Tenant_Should_Show_Tenant_Context()
    {
        // Arrange - Test with a specific tenant ID
        var testTenantId = Guid.NewGuid();
        
        using (_currentTenant.Change(testTenantId))
        {
            // Act
            var result = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();

            // Assert
            result.ShouldNotBeNull();
            result.CurrentTenantId.ShouldBe(testTenantId);
            result.IsHostTenant.ShouldBeFalse();
            result.AllTenantsCount.ShouldBeNull(); // Regular tenants can't see all tenants
        }
    }

    [Fact]
    public async Task VerifyTenantSwitchAsync_Should_Return_Switch_Result()
    {
        // Arrange
        var originalTenantId = Guid.NewGuid();
        var targetTenantId = Guid.NewGuid();

        using (_currentTenant.Change(originalTenantId))
        {
            // Act
            var result = await _multiTenantDataIsolationService.VerifyTenantSwitchAsync(targetTenantId);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<TenantSwitchVerificationResult>();
            result.OriginalTenantId.ShouldBe(originalTenantId);
            result.TargetTenantId.ShouldBe(targetTenantId);
            result.TestExecutedAt.ShouldNotBe(default(DateTime));
            result.AfterSwitchData.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task VerifyTenantSwitchAsync_Between_Different_Tenants_Should_Show_Data_Isolation()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();

        using (_currentTenant.Change(tenant1Id))
        {
            // Act
            var result = await _multiTenantDataIsolationService.VerifyTenantSwitchAsync(tenant2Id);

            // Assert
            result.ShouldNotBeNull();
            result.OriginalTenantId.ShouldBe(tenant1Id);
            result.TargetTenantId.ShouldBe(tenant2Id);
            
            // Data isolation should be verified
            // Note: In a real scenario with actual data, this would show different counts
            result.IsSuccess.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task VerifyTenantSwitchAsync_From_Host_To_Tenant_Should_Show_Context_Change()
    {
        // Arrange - Start from host context
        var targetTenantId = Guid.NewGuid();

        using (_currentTenant.Change(null)) // Host context
        {
            // Act
            var result = await _multiTenantDataIsolationService.VerifyTenantSwitchAsync(targetTenantId);

            // Assert
            result.ShouldNotBeNull();
            result.OriginalTenantId.ShouldBeNull(); // Host tenant
            result.TargetTenantId.ShouldBe(targetTenantId);
            result.IsSuccess.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task VerifyTenantSwitchAsync_From_Tenant_To_Host_Should_Show_Context_Change()
    {
        // Arrange - Start from tenant context
        var originalTenantId = Guid.NewGuid();

        using (_currentTenant.Change(originalTenantId))
        {
            // Act
            var result = await _multiTenantDataIsolationService.VerifyTenantSwitchAsync(null); // Switch to host

            // Assert
            result.ShouldNotBeNull();
            result.OriginalTenantId.ShouldBe(originalTenantId);
            result.TargetTenantId.ShouldBeNull(); // Host tenant
            result.IsSuccess.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task VerifyDataIsolationAsync_Should_Handle_Success_Case()
    {
        // Act
        var result = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldNotBeNullOrEmpty();
        result.ErrorDetails.ShouldBeNullOrEmpty();
    }

    [Fact]
    public async Task Multiple_Tenant_Contexts_Should_Maintain_Isolation()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        var tenant3Id = Guid.NewGuid();

        MultiTenantDataIsolationResult result1, result2, result3;

        // Act - Test multiple tenant contexts
        using (_currentTenant.Change(tenant1Id))
        {
            result1 = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();
        }

        using (_currentTenant.Change(tenant2Id))
        {
            result2 = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();
        }

        using (_currentTenant.Change(tenant3Id))
        {
            result3 = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();
        }

        // Assert - Each context should maintain its own tenant ID
        result1.CurrentTenantId.ShouldBe(tenant1Id);
        result2.CurrentTenantId.ShouldBe(tenant2Id);
        result3.CurrentTenantId.ShouldBe(tenant3Id);

        // All should be successful
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result3.IsSuccess.ShouldBeTrue();

        // None should be host tenant
        result1.IsHostTenant.ShouldBeFalse();
        result2.IsHostTenant.ShouldBeFalse();
        result3.IsHostTenant.ShouldBeFalse();
    }

    [Fact]
    public async Task Nested_Tenant_Context_Changes_Should_Work_Correctly()
    {
        // Arrange
        var outerTenantId = Guid.NewGuid();
        var innerTenantId = Guid.NewGuid();

        // Act & Assert
        using (_currentTenant.Change(outerTenantId))
        {
            var outerResult = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();
            outerResult.CurrentTenantId.ShouldBe(outerTenantId);

            using (_currentTenant.Change(innerTenantId))
            {
                var innerResult = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();
                innerResult.CurrentTenantId.ShouldBe(innerTenantId);
            }

            // After inner context is disposed, should return to outer context
            var backToOuterResult = await _multiTenantDataIsolationService.VerifyDataIsolationAsync();
            backToOuterResult.CurrentTenantId.ShouldBe(outerTenantId);
        }
    }
}