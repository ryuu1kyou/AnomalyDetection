using System;
using System.Linq;
using System.Threading.Tasks;
using AnomalyDetection.AnomalyDetection;
using AnomalyDetection.CanSignals;
using AnomalyDetection.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace AnomalyDetection.Data;

public class AnomalyDetectionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ILogger<AnomalyDetectionDataSeedContributor> _logger;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRepository<OemMaster, Guid> _oemMasterRepository;
    private readonly IRepository<CanSystemCategory, Guid> _canSystemCategoryRepository;
    private readonly IRepository<CanSignal, Guid> _canSignalRepository;
    private readonly IRepository<CanAnomalyDetectionLogic, Guid> _detectionLogicRepository;

    public AnomalyDetectionDataSeedContributor(
        ILogger<AnomalyDetectionDataSeedContributor> logger,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        ITenantRepository tenantRepository,
        IRepository<OemMaster, Guid> oemMasterRepository,
        IRepository<CanSystemCategory, Guid> canSystemCategoryRepository,
        IRepository<CanSignal, Guid> canSignalRepository,
        IRepository<CanAnomalyDetectionLogic, Guid> detectionLogicRepository)
    {
        _logger = logger;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _tenantRepository = tenantRepository;
        _oemMasterRepository = oemMasterRepository;
        _canSystemCategoryRepository = canSystemCategoryRepository;
        _canSignalRepository = canSignalRepository;
        _detectionLogicRepository = detectionLogicRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        _logger.LogInformation("Starting AnomalyDetection data seeding...");

        // Host tenant data (業界共通データ)
        if (_currentTenant.Id == null)
        {
            await SeedHostDataAsync();
        }
        else
        {
            // Tenant specific data
            await SeedTenantDataAsync();
        }

        _logger.LogInformation("AnomalyDetection data seeding completed.");
    }

    private async Task SeedHostDataAsync()
    {
        _logger.LogInformation("Seeding host data...");

        // Create default OEM tenants
        await CreateDefaultOemTenantsAsync();

        // Create default CAN system categories
        await CreateDefaultCanSystemCategoriesAsync();

        // Create sample standard CAN signals (業界共通)
        await CreateStandardCanSignalsAsync();

        // Create sample standard detection logics (業界共通)
        await CreateStandardDetectionLogicsAsync();
    }

    private async Task SeedTenantDataAsync()
    {
        _logger.LogInformation("Seeding tenant data for tenant: {TenantId}", _currentTenant.Id);

        // Create tenant-specific CAN signals
        await CreateTenantCanSignalsAsync();

        // Create tenant-specific detection logics
        await CreateTenantDetectionLogicsAsync();
    }

    private async Task CreateDefaultOemTenantsAsync()
    {
        var oemMasters = new[]
        {
            new { Code = "TOYOTA", Name = "Toyota Motor Corporation", Country = "Japan", Description = "世界最大級の自動車メーカー" },
            new { Code = "HONDA", Name = "Honda Motor Co., Ltd.", Country = "Japan", Description = "二輪車・四輪車・汎用製品メーカー" },
            new { Code = "NISSAN", Name = "Nissan Motor Co., Ltd.", Country = "Japan", Description = "日本の大手自動車メーカー" }
        };

        foreach (var oemData in oemMasters)
        {
            var existingOem = await _oemMasterRepository.FirstOrDefaultAsync(x => x.OemCode.Code == oemData.Code);
            if (existingOem == null)
            {
                var oemMaster = new OemMaster(
                    _guidGenerator.Create(),
                    new OemCode(oemData.Code, oemData.Name),
                    oemData.Name,
                    oemData.Country,
                    oemData.Description
                );

                await _oemMasterRepository.InsertAsync(oemMaster);
                _logger.LogInformation("Created OEM Master: {Name}", oemData.Name);

                // Create corresponding tenant
                var existingTenant = await _tenantRepository.FindByNameAsync(oemData.Code.ToLower());
                if (existingTenant == null)
                {
                    // Note: Tenant creation should be handled by TenantManager in a real implementation
                    _logger.LogInformation("Tenant {TenantName} should be created manually or via TenantManager", oemData.Code.ToLower());
                }
            }
        }
    }

    private async Task CreateDefaultCanSystemCategoriesAsync()
    {
        var categories = new[]
        {
            new { Type = CanSystemType.Engine, Name = "エンジン系統", Icon = "engine", Color = "#FF6B6B", Description = "エンジン制御関連信号" },
            new { Type = CanSystemType.Brake, Name = "ブレーキ系統", Icon = "brake", Color = "#4ECDC4", Description = "ブレーキ制御関連信号" },
            new { Type = CanSystemType.Steering, Name = "ステアリング系統", Icon = "steering", Color = "#45B7D1", Description = "ステアリング制御関連信号" },
            new { Type = CanSystemType.Transmission, Name = "トランスミッション系統", Icon = "transmission", Color = "#96CEB4", Description = "変速機制御関連信号" },
            new { Type = CanSystemType.Body, Name = "ボディ系統", Icon = "body", Color = "#FFEAA7", Description = "ボディ制御関連信号" },
            new { Type = CanSystemType.Chassis, Name = "シャシー系統", Icon = "chassis", Color = "#DDA0DD", Description = "シャシー制御関連信号" },
            new { Type = CanSystemType.HVAC, Name = "空調系統", Icon = "hvac", Color = "#98D8C8", Description = "空調制御関連信号" },
            new { Type = CanSystemType.Lighting, Name = "照明系統", Icon = "lighting", Color = "#F7DC6F", Description = "照明制御関連信号" },
            new { Type = CanSystemType.Infotainment, Name = "インフォテインメント系統", Icon = "infotainment", Color = "#BB8FCE", Description = "情報娯楽関連信号" },
            new { Type = CanSystemType.Safety, Name = "安全系統", Icon = "safety", Color = "#F1948A", Description = "安全制御関連信号" },
            new { Type = CanSystemType.Powertrain, Name = "パワートレイン系統", Icon = "powertrain", Color = "#85C1E9", Description = "動力伝達関連信号" },
            new { Type = CanSystemType.Gateway, Name = "ゲートウェイ系統", Icon = "gateway", Color = "#82E0AA", Description = "ネットワーク中継関連信号" }
        };

        foreach (var categoryData in categories)
        {
            var existingCategory = await _canSystemCategoryRepository.FirstOrDefaultAsync(x => x.SystemType == categoryData.Type);
            if (existingCategory == null)
            {
                var category = new CanSystemCategory(
                    _guidGenerator.Create(),
                    null, // Host tenant
                    categoryData.Type,
                    categoryData.Name,
                    categoryData.Description,
                    categoryData.Icon,
                    categoryData.Color
                );

                await _canSystemCategoryRepository.InsertAsync(category);
                _logger.LogInformation("Created CAN System Category: {Name}", categoryData.Name);
            }
        }
    }

    private async Task CreateStandardCanSignalsAsync()
    {
        var standardSignals = new[]
        {
            // Engine signals
            new { Name = "EngineRPM", CanId = "0C4", System = CanSystemType.Engine, StartBit = 24, Length = 16, Factor = 0.25, Offset = 0.0, Unit = "rpm", Description = "エンジン回転数" },
            new { Name = "VehicleSpeed", CanId = "0D0", System = CanSystemType.Engine, StartBit = 8, Length = 16, Factor = 0.01, Offset = 0.0, Unit = "km/h", Description = "車速" },
            new { Name = "ThrottlePosition", CanId = "0C4", System = CanSystemType.Engine, StartBit = 40, Length = 8, Factor = 0.4, Offset = 0.0, Unit = "%", Description = "スロットル開度" },
            new { Name = "CoolantTemp", CanId = "0C4", System = CanSystemType.Engine, StartBit = 0, Length = 8, Factor = 1.0, Offset = -40.0, Unit = "°C", Description = "冷却水温度" },
            
            // Brake signals
            new { Name = "BrakePedalPosition", CanId = "1A0", System = CanSystemType.Brake, StartBit = 16, Length = 8, Factor = 0.4, Offset = 0.0, Unit = "%", Description = "ブレーキペダル踏み込み量" },
            new { Name = "ABSActive", CanId = "1A0", System = CanSystemType.Brake, StartBit = 0, Length = 1, Factor = 1.0, Offset = 0.0, Unit = "bool", Description = "ABS作動状態" },
            new { Name = "BrakePressure", CanId = "1A1", System = CanSystemType.Brake, StartBit = 0, Length = 16, Factor = 0.1, Offset = 0.0, Unit = "bar", Description = "ブレーキ圧力" },
            
            // Steering signals
            new { Name = "SteeringAngle", CanId = "025", System = CanSystemType.Steering, StartBit = 8, Length = 16, Factor = 0.1, Offset = -3276.8, Unit = "deg", Description = "ステアリング角度" },
            new { Name = "SteeringTorque", CanId = "025", System = CanSystemType.Steering, StartBit = 24, Length = 16, Factor = 0.01, Offset = -327.68, Unit = "Nm", Description = "ステアリングトルク" },
            
            // Safety signals
            new { Name = "AirbagWarning", CanId = "050", System = CanSystemType.Safety, StartBit = 0, Length = 1, Factor = 1.0, Offset = 0.0, Unit = "bool", Description = "エアバッグ警告" },
            new { Name = "SeatbeltStatus", CanId = "050", System = CanSystemType.Safety, StartBit = 8, Length = 8, Factor = 1.0, Offset = 0.0, Unit = "bitmap", Description = "シートベルト状態" }
        };

        foreach (var signalData in standardSignals)
        {
            var existingSignal = await _canSignalRepository.FirstOrDefaultAsync(x =>
                x.Identifier.SignalName == signalData.Name && x.Identifier.CanId == signalData.CanId);

            if (existingSignal == null)
            {
                var signal = new CanSignal(
                    _guidGenerator.Create(),
                    null, // Host tenant (standard signal)
                    new SignalIdentifier(signalData.Name, signalData.CanId),
                    new SignalSpecification(
                        signalData.StartBit,
                        signalData.Length,
                        SignalDataType.Unsigned,
                        new SignalValueRange(0, Math.Pow(2, signalData.Length) - 1)
                    ),
                    signalData.System,
                    new OemCode("STANDARD", "Standard"),
                    signalData.Description
                );

                // Note: Signal properties should be set via constructor or property setters

                await _canSignalRepository.InsertAsync(signal);
                _logger.LogInformation("Created standard CAN signal: {Name}", signalData.Name);
            }
        }
    }

    private async Task CreateStandardDetectionLogicsAsync()
    {
        var standardLogics = new[]
        {
            new {
                Name = "Engine_RPM_OutOfRange",
                Description = "エンジン回転数が正常範囲を超えた場合の異常検出",
                DetectionType = AnomalyType.OutOfRange,
                SystemType = CanSystemType.Engine,
                SignalName = "EngineRPM",
                MinThreshold = 0.0,
                MaxThreshold = 8000.0
            },
            new {
                Name = "Vehicle_Speed_OutOfRange",
                Description = "車速信号の異常値検出",
                DetectionType = AnomalyType.OutOfRange,
                SystemType = CanSystemType.Engine,
                SignalName = "VehicleSpeed",
                MinThreshold = 0.0,
                MaxThreshold = 300.0
            },
            new {
                Name = "Brake_Pressure_OutOfRange",
                Description = "ブレーキ圧力の異常値検出",
                DetectionType = AnomalyType.OutOfRange,
                SystemType = CanSystemType.Brake,
                SignalName = "BrakePressure",
                MinThreshold = 0.0,
                MaxThreshold = 200.0
            },
            new {
                Name = "Steering_Angle_Timeout",
                Description = "ステアリング角度信号の通信断検出",
                DetectionType = AnomalyType.Timeout,
                SystemType = CanSystemType.Steering,
                SignalName = "SteeringAngle",
                MinThreshold = 0.0,
                MaxThreshold = 500.0 // 500ms timeout
            }
        };

        foreach (var logicData in standardLogics)
        {
            var existingLogic = await _detectionLogicRepository.FirstOrDefaultAsync(x =>
                x.Identity.Name == logicData.Name);

            if (existingLogic == null)
            {
                var logic = new CanAnomalyDetectionLogic(
                    _guidGenerator.Create(),
                    null, // Host tenant (standard logic)
                    new DetectionLogicIdentity(
                        logicData.Name,
                        new LogicVersion(1, 0, 0),
                        new OemCode("STANDARD", "Standard")
                    ),
                    new DetectionLogicSpecification(
                        logicData.DetectionType,
                        logicData.Description,
                        logicData.SystemType
                    ),
                    new SafetyClassification(AsilLevel.QM)
                );

                // Insert the logic without parameters for simplicity
                // Parameters can be added later through the application UI
                await _detectionLogicRepository.InsertAsync(logic, autoSave: true);
                _logger.LogInformation("Created standard detection logic: {Name}", logicData.Name);
            }
        }
    }

    private async Task CreateTenantCanSignalsAsync()
    {
        // Create tenant-specific CAN signals based on current tenant
        var tenantId = _currentTenant.Id;
        if (tenantId == null) return;

        var tenant = await _tenantRepository.GetAsync(tenantId.Value);
        var oemCode = tenant.Name.ToUpper();

        var tenantSignals = new[]
        {
            new { Name = $"{oemCode}_CustomSignal1", CanId = "700", System = CanSystemType.Engine, Description = $"{oemCode}独自エンジン信号" },
            new { Name = $"{oemCode}_CustomSignal2", CanId = "701", System = CanSystemType.Brake, Description = $"{oemCode}独自ブレーキ信号" }
        };

        foreach (var signalData in tenantSignals)
        {
            var existingSignal = await _canSignalRepository.FirstOrDefaultAsync(x =>
                x.Identifier.SignalName == signalData.Name && x.TenantId == tenantId);

            if (existingSignal == null)
            {
                var signal = new CanSignal(
                    _guidGenerator.Create(),
                    tenantId,
                    new SignalIdentifier(signalData.Name, signalData.CanId),
                    new SignalSpecification(0, 32, SignalDataType.Unsigned, new SignalValueRange(0, 4294967295)),
                    signalData.System,
                    new OemCode(oemCode, oemCode),
                    signalData.Description
                );

                // Note: Signal properties should be set via constructor or property setters

                await _canSignalRepository.InsertAsync(signal);
                _logger.LogInformation("Created tenant CAN signal: {Name}", signalData.Name);
            }
        }
    }

    private async Task CreateTenantDetectionLogicsAsync()
    {
        // Create tenant-specific detection logics
        var tenantId = _currentTenant.Id;
        if (tenantId == null) return;

        var tenant = await _tenantRepository.GetAsync(tenantId.Value);
        var oemCode = tenant.Name.ToUpper();

        var tenantLogics = new[]
        {
            new {
                Name = $"{oemCode}独自異常検出ロジック1",
                Description = $"{oemCode}独自の異常検出パターン",
                DetectionType = AnomalyType.OutOfRange,
                SystemType = CanSystemType.Engine
            }
        };

        foreach (var logicData in tenantLogics)
        {
            var existingLogic = await _detectionLogicRepository.FirstOrDefaultAsync(x =>
                x.Identity.Name == logicData.Name && x.TenantId == tenantId);

            if (existingLogic == null)
            {
                var logic = new CanAnomalyDetectionLogic(
                    _guidGenerator.Create(),
                    tenantId,
                    new DetectionLogicIdentity(
                        logicData.Name,
                        new LogicVersion(1, 0, 0),
                        new OemCode(oemCode, oemCode)
                    ),
                    new DetectionLogicSpecification(
                        logicData.DetectionType,
                        logicData.Description,
                        logicData.SystemType
                    ),
                    new SafetyClassification(AsilLevel.QM)
                );

                // Note: Sharing level should be set via appropriate methods

                await _detectionLogicRepository.InsertAsync(logic);
                _logger.LogInformation("Created tenant detection logic: {Name}", logicData.Name);
            }
        }
    }

    private string GenerateDetectionScript(DetectionType detectionType)
    {
        return detectionType switch
        {
            DetectionType.OutOfRange => @"
function detectAnomaly(value, parameters) {
    const min = parseFloat(parameters.MinThreshold);
    const max = parseFloat(parameters.MaxThreshold);
    
    if (value < min || value > max) {
        return {
            isAnomalous: true,
            level: 'Warning',
            message: `値 ${value} が範囲外です (${min} - ${max})`
        };
    }
    
    return {
        isAnomalous: false,
        level: 'Info',
        message: '正常'
    };
}",
            DetectionType.Timeout => @"
function detectAnomaly(value, parameters, lastUpdateTime) {
    const timeout = parseFloat(parameters.MaxThreshold);
    const currentTime = Date.now();
    
    if (currentTime - lastUpdateTime > timeout) {
        return {
            isAnomalous: true,
            level: 'Error',
            message: `通信断検出: ${currentTime - lastUpdateTime}ms > ${timeout}ms`
        };
    }
    
    return {
        isAnomalous: false,
        level: 'Info',
        message: '通信正常'
    };
}",
            _ => @"
function detectAnomaly(value, parameters) {
    return {
        isAnomalous: false,
        level: 'Info',
        message: '検出ロジック未実装'
    };
}"
        };
    }
}