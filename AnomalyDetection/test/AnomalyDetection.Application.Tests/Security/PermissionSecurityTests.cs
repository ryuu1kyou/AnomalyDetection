using System.Threading.Tasks;
using Xunit;

namespace AnomalyDetection.Application.Tests.Security;

public class PermissionSecurityTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task UnauthorizedUser_CannotAccessDetectionLogic()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task OemCustomization_OnlyAccessibleByOemUsers()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task ProjectAccess_RestrictedByRole()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task DetectionLogicApproval_RequiresApproverRole()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task OemUserApproval_RequiresOemApproverRole()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires TestDataBuilder implementation")]
    public async Task SystemAdmin_HasFullAccess()
    {
        await Task.CompletedTask;
    }
}
