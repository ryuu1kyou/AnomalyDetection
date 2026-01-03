using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.CanSpecification;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace AnomalyDetection.Application.Tests.CanSpecification;

public class CanSpecImportAppServiceTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private readonly ICanSpecImportAppService _appService;
    private readonly IRepository<CanSpecImport, Guid> _specRepository;
    private readonly Dictionary<Guid, CanSpecImport> _imports = new();

    public CanSpecImportAppServiceTests()
    {
        _appService = GetRequiredService<ICanSpecImportAppService>();
        _specRepository = GetRequiredService<IRepository<CanSpecImport, Guid>>();

        SetupSpecRepository();
    }

    private void SetupSpecRepository()
    {
        _specRepository.InsertAsync(Arg.Any<CanSpecImport>(), Arg.Any<bool>(), Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var import = callInfo.Arg<CanSpecImport>();
                _imports[import.Id] = import;
                return Task.FromResult(import);
            });

        _specRepository.GetAsync(Arg.Any<Guid>(), includeDetails: Arg.Any<bool>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_imports.TryGetValue(id, out var entity))
                {
                    return Task.FromResult(entity);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(CanSpecImport), id);
            });

        _specRepository.GetAsync(Arg.Any<Guid>(), cancellationToken: Arg.Any<System.Threading.CancellationToken>())
            .Returns(callInfo =>
            {
                var id = callInfo.Arg<Guid>();
                if (_imports.TryGetValue(id, out var entity))
                {
                    return Task.FromResult(entity);
                }
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(CanSpecImport), id);
            });
    }

    [Fact]
    public async Task GetDiffSummaryAsync_Should_Aggregate_Diff_Metadata()
    {
        var spec = new CanSpecImport(
            Guid.NewGuid(),
            fileName: "ADAS_baseline.dbc",
            fileFormat: "DBC",
            fileSize: 1024,
            fileHash: Guid.NewGuid().ToString("N"),
            importedBy: "test-user")
        {
            Status = ImportStatus.Completed
        };

        spec.MarkAsCompleted(messageCount: 1, signalCount: 2);

        var messageDiff = new CanSpecDiff(DiffType.Added, "Message", "VehicleStatus")
        {
            CanSpecImportId = spec.Id,
            PreviousSpecId = Guid.NewGuid(),
            MessageId = 0x100,
            ChangeCategory = "MessageAdded",
            Severity = ChangeSeverity.High,
            ImpactedSubsystem = "ADAS",
            ChangeSummary = "VehicleStatus message added",
            Details = "Test high severity message addition"
        };

        var signalDiff = new CanSpecDiff(DiffType.Removed, "Signal", "WheelSpeed")
        {
            CanSpecImportId = spec.Id,
            PreviousSpecId = Guid.NewGuid(),
            MessageId = 0x101,
            ChangeCategory = "SignalRemoved",
            Severity = ChangeSeverity.Low,
            ImpactedSubsystem = "Powertrain",
            ChangeSummary = "WheelSpeed signal removed",
            Details = "Test low severity signal removal"
        };

        spec.AddDiff(messageDiff);
        spec.AddDiff(signalDiff);

        await WithUnitOfWorkAsync(async () =>
        {
            await _specRepository.InsertAsync(spec);
        });

        var summary = await _appService.GetDiffSummaryAsync(spec.Id);

        summary.ShouldNotBeNull();
        summary.MessageAddedCount.ShouldBe(1);
        summary.SignalRemovedCount.ShouldBe(1);
        summary.SeverityHighCount.ShouldBe(1);
        summary.SeverityLowCount.ShouldBe(1);
        summary.ImpactedSubsystems.ShouldContain("ADAS");
        summary.ImpactedSubsystems.ShouldContain("Powertrain");
        summary.SummaryText.ShouldNotBeNullOrWhiteSpace();
    }
}
