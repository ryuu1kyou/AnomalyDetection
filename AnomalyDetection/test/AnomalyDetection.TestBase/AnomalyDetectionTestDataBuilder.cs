using System;
using System.Threading.Tasks;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace AnomalyDetection;

public class AnomalyDetectionTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;

    public AnomalyDetectionTestDataSeedContributor(
        ICurrentTenant currentTenant,
        IRepository<CanSignal, Guid> canSignalRepository)
    {
        _currentTenant = currentTenant;
        _canSignalRepository = canSignalRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await SeedTestCanSignalsAsync();
        }
    }

    private async Task SeedTestCanSignalsAsync()
    {
        // テスト用のCAN信号データを作成
        if (await _canSignalRepository.GetCountAsync() == 0)
        {
            var testSignals = new[]
            {
                CreateTestCanSignal("ENGINE_RPM", "0x100", CanSystemType.Engine, "Engine RPM signal"),
                CreateTestCanSignal("ENGINE_TEMP", "0x101", CanSystemType.Engine, "Engine temperature signal"),
                CreateTestCanSignal("BRAKE_PRESSURE", "0x200", CanSystemType.Brake, "Brake pressure signal"),
                CreateTestCanSignal("VEHICLE_SPEED", "0x300", CanSystemType.Chassis, "Vehicle speed signal")
            };

            await _canSignalRepository.InsertManyAsync(testSignals);
        }
    }

    private static CanSignal CreateTestCanSignal(string signalName, string canId, CanSystemType systemType, string description)
    {
        var identifier = new SignalIdentifier(signalName, canId);
        var valueRange = new SignalValueRange(0, 1000);
        var specification = new SignalSpecification(0, 16, SignalDataType.Unsigned, valueRange);
        var oemCode = new OemCode("TEST", "Test OEM");

        return new CanSignal(
            Guid.NewGuid(),
            null, // tenantId
            identifier,
            specification,
            systemType,
            oemCode,
            description);
    }
}
