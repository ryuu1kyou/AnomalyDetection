using System.Threading.Tasks;
using Xunit;

namespace AnomalyDetection.Application.Tests.Security;

public class MultiTenantSecurityTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task Tenant_CannotAccessOtherTenantData()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task OemCustomization_IsolatedByTenant()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task DetectionLogicSharing_WorksAcrossTenants()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task OemWideLogic_AccessibleByAllOemTenants()
    {
        await Task.CompletedTask;
    }
}
