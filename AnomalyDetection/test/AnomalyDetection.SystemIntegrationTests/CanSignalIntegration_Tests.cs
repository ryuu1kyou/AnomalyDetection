using System.Net;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AnomalyDetection;

public class CanSignalIntegration_Tests : SystemIntegrationTestBase
{
    public CanSignalIntegration_Tests(AnomalyDetectionWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetListAsync_Should_Return_Success()
    {
        // Act
        var response = await Client.GetAsync("/api/app/can-signal");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrWhiteSpace();
    }
}
