using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSpecification;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace AnomalyDetection.Application.Tests.CanSpecification;

public class CompatibilityAnalysisAppServiceTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly ICompatibilityAnalysisAppService _compatibilityAppService;
    private readonly IRepository<CanSpecImport, Guid> _specRepository;

    public CompatibilityAnalysisAppServiceTests()
    {
        _compatibilityAppService = GetRequiredService<ICompatibilityAnalysisAppService>();
        _specRepository = GetRequiredService<IRepository<CanSpecImport, Guid>>();
    }

    [Fact]
    public async Task AssessCompatibilityAsync_Should_Return_Status_With_Cached_Result()
    {
        var oldSpec = BuildSpec("ADAS_v1.dbc", transmitter: "ADAS_CTRL");
        var newSpec = BuildSpec("ADAS_v2.dbc", transmitter: "ADAS_CONTROLLER");

        await WithUnitOfWorkAsync(async () =>
        {
            await _specRepository.InsertAsync(oldSpec);
            await _specRepository.InsertAsync(newSpec);
        });

        var request = new CompatibilityAssessmentRequestDto
        {
            OldSpecId = oldSpec.Id,
            NewSpecId = newSpec.Id,
            Context = "General"
        };

        var status = await _compatibilityAppService.AssessCompatibilityAsync(request);

        status.ShouldNotBeNull();
        status.IsCompatible.ShouldBeTrue();
        status.HighestSeverity.ShouldBe((int)ChangeSeverity.High);
        status.BreakingChangeCount.ShouldBeGreaterThanOrEqualTo(1);
        status.WarningCount.ShouldBeGreaterThanOrEqualTo(0);
        status.KeyFindings.ShouldNotBeEmpty();
        status.ImpactedSubsystems.ShouldContain("ADAS");
        status.Summary.ShouldNotBeNullOrWhiteSpace();

        var cachedStatus = await _compatibilityAppService.AssessCompatibilityAsync(request);
        cachedStatus.GeneratedAt.ShouldBe(status.GeneratedAt);
        cachedStatus.CompatibilityScore.ShouldBe(status.CompatibilityScore);
    }

    private static CanSpecImport BuildSpec(string fileName, string transmitter)
    {
        var spec = new CanSpecImport(
            Guid.NewGuid(),
            fileName,
            fileFormat: "DBC",
            fileSize: 2048,
            fileHash: Guid.NewGuid().ToString("N"),
            importedBy: "test-user");

        var message = new CanSpecMessage(0x120, "ADAS_Status", 8)
        {
            CanSpecImportId = spec.Id,
            Transmitter = transmitter,
            CycleTime = 20
        };

        spec.AddMessage(message);
        spec.MarkAsCompleted(messageCount: 1, signalCount: 0);

        return spec;
    }
}
