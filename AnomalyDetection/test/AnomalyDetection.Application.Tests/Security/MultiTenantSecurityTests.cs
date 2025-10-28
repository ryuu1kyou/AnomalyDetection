using System;
using System.Threading.Tasks;
using AnomalyDetection.Application.Contracts.OemTraceability;
using AnomalyDetection.Application.Contracts.OemTraceability.Dtos;
using AnomalyDetection.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace AnomalyDetection.Application.Tests.Security;

public class MultiTenantSecurityTests : AnomalyDetectionApplicationTestBase
{
    private readonly IOemTraceabilityAppService _oemTraceabilityAppService;
    private readonly ICurrentTenant _currentTenant;

    public MultiTenantSecurityTests()
    {
        _oemTraceabilityAppService = GetRequiredService<IOemTraceabilityAppService>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task CreateOemCustomization_Should_Be_Isolated_By_Tenant()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        
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

        // Act - Create customization for tenant 1
        using (_currentTenant.Change(tenant1Id))
        {
            LoginAsAdmin();
            var customizationId1 = await _oemTraceabilityAppService.CreateOemCustomizationAsync(input);
            customizationId1.ShouldNotBe(Guid.Empty);
        }

        // Act - Try to access customization from tenant 2
        using (_currentTenant.Change(tenant2Id))
        {
            LoginAsAdmin();
            var customizations = await _oemTraceabilityAppService.GetOemCustomizationsAsync();
            
            // Assert - Tenant 2 should not see tenant 1's customizations
            customizations.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task GetOemCustomizations_Should_Filter_By_Current_Tenant()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        
        var input1 = new CreateOemCustomizationDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "TestEntity1",
            OemCode = "TEST1",
            Type = CustomizationType.ParameterAdjustment,
            CustomParameters = new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "value1" },
            OriginalParameters = new System.Collections.Generic.Dictionary<string, object> { ["param1"] = "original" },
            CustomizationReason = "Test customization 1"
        };

        var input2 = new CreateOemCustomizationDto
        {
            EntityId = Guid.NewGuid(),
            EntityType = "TestEntity2",
            OemCode = "TEST2",
            Type = CustomizationType.ParameterAdjustment,
            CustomParameters = new System.Collections.Generic.Dictionary<string, object> { ["param2"] = "value2" },
            OriginalParameters = new System.Collections.Generic.Dictionary<string, object> { ["param2"] = "original" },
            CustomizationReason = "Test customization 2"
        };

        // Act - Create customizations for different tenants
        Guid customizationId1, customizationId2;
        
        using (_currentTenant.Change(tenant1Id))
        {
            LoginAsAdmin();
            customizationId1 = await _oemTraceabilityAppService.CreateOemCustomizationAsync(input1);
        }

        using (_currentTenant.Change(tenant2Id))
        {
            LoginAsAdmin();
            customizationId2 = await _oemTraceabilityAppService.CreateOemCustomizationAsync(input2);
        }

        // Assert - Each tenant should only see their own customizations
        using (_currentTenant.Change(tenant1Id))
        {
            LoginAsAdmin();
            var tenant1Customizations = await _oemTraceabilityAppService.GetOemCustomizationsAsync();
            tenant1Customizations.Count.ShouldBe(1);
            tenant1Customizations[0].EntityType.ShouldBe("TestEntity1");
        }

        using (_currentTenant.Change(tenant2Id))
        {
            LoginAsAdmin();
            var tenant2Customizations = await _oemTraceabilityAppService.GetOemCustomizationsAsync();
            tenant2Customizations.Count.ShouldBe(1);
            tenant2Customizations[0].EntityType.ShouldBe("TestEntity2");
        }
    }

    [Fact]
    public async Task Cross_Tenant_Access_Should_Be_Prevented()
    {
        // Arrange
        var tenant1Id = Guid.NewGuid();
        var tenant2Id = Guid.NewGuid();
        
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

        Guid customizationId;
        
        // Create customization in tenant 1
        using (_currentTenant.Change(tenant1Id))
        {
            LoginAsAdmin();
            customizationId = await _oemTraceabilityAppService.CreateOemCustomizationAsync(input);
        }

        // Try to access the customization from tenant 2
        using (_currentTenant.Change(tenant2Id))
        {
            LoginAsAdmin();
            
            // Act & Assert - Should not be able to access customization from different tenant
            await Should.ThrowAsync<Exception>(
                async () => await _oemTraceabilityAppService.GetOemCustomizationAsync(customizationId));
        }
    }

    private void LoginAsAdmin()
    {
        // Note: In a real test, you would need to properly set up the current user
        // This is a simplified example
    }
}