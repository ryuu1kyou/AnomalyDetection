using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;
using AnomalyDetection.Safety;
using AnomalyDetection.AnomalyDetection; // AsilLevel
using Microsoft.Extensions.DependencyInjection;

namespace AnomalyDetection.SafetyTraceTests;

public class SafetyTraceLinkSyncTests : AnomalyDetectionApplicationTestBase<AnomalyDetectionApplicationTestModule>
{
    private ISafetyTraceAppService AppService => ServiceProvider.GetRequiredService<ISafetyTraceAppService>();
    private IRepository<SafetyTraceRecord, Guid> RecordRepository => ServiceProvider.GetRequiredService<IRepository<SafetyTraceRecord, Guid>>();

    [Fact(Skip="Pending test environment fix for PermissionManagement initialization")]
    public async Task SyncLinkMatrix_Should_Add_Links_For_Approved_Records_With_DetectionLogic()
    {
        // Arrange
        var logicId1 = Guid.NewGuid();
        var logicId2 = Guid.NewGuid();
        var record1 = new SafetyTraceRecord(Guid.NewGuid(), "REQ-1", "SG-1", AsilLevel.B, "Record 1")
        {
            DetectionLogicId = logicId1,
            ApprovalStatus = ApprovalStatus.Approved
        };
        var record2 = new SafetyTraceRecord(Guid.NewGuid(), "REQ-2", "SG-2", AsilLevel.B, "Record 2")
        {
            DetectionLogicId = logicId2,
            ApprovalStatus = ApprovalStatus.Approved
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await RecordRepository.InsertAsync(record1, autoSave: true);
            await RecordRepository.InsertAsync(record2, autoSave: true);
        });

        // Act
        var result = await AppService.SyncLinkMatrixAsync(new SafetyTraceLinkMatrixSyncInput());

        // Assert
        result.AddedCount.ShouldBeGreaterThanOrEqualTo(2); // At least two links added for our two records
        result.RemovedCount.ShouldBe(0);
        result.Diff.Added.Count(l => l.SourceRecordId == record1.Id && l.TargetRecordId == logicId1).ShouldBe(1);
        result.Diff.Added.Count(l => l.SourceRecordId == record2.Id && l.TargetRecordId == logicId2).ShouldBe(1);
    }

    [Fact(Skip="Pending test environment fix for PermissionManagement initialization")]
    public async Task SyncLinkMatrix_Should_Remove_Links_When_Record_NoLonger_Has_DetectionLogic()
    {
        // Arrange initial state
        var logicId = Guid.NewGuid();
        var record = new SafetyTraceRecord(Guid.NewGuid(), "REQ-REMOVE", "SG-R", AsilLevel.B, "Record Remove")
        {
            DetectionLogicId = logicId,
            ApprovalStatus = ApprovalStatus.Approved
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await RecordRepository.InsertAsync(record, autoSave: true);
        });

        // Initial sync to create link
        var initial = await AppService.SyncLinkMatrixAsync(new SafetyTraceLinkMatrixSyncInput());
        initial.AddedCount.ShouldBeGreaterThanOrEqualTo(1);

        // Modify record: remove detection logic id
        await WithUnitOfWorkAsync(async () =>
        {
            record.DetectionLogicId = null;
            await RecordRepository.UpdateAsync(record, autoSave: true);
        });

        // Act: second sync should remove the link
        var second = await AppService.SyncLinkMatrixAsync(new SafetyTraceLinkMatrixSyncInput());

        // Assert removal
        second.RemovedCount.ShouldBeGreaterThanOrEqualTo(1);
        second.Diff.Removed.Any(r => r.SourceRecordId == record.Id && r.TargetRecordId == logicId).ShouldBeTrue();
    }
}
