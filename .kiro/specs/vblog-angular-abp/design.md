# DDDアーキテクチャ設計 - CAN異常検出管理システム

## DDDレイヤー構造

### アーキテクチャ概要

```
┌─────────────────────────────────────────────────────────┐
│                Presentation Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │  Angular SPA    │  │ ABP Web API     │              │
│  │  (Frontend)     │  │ (Controllers)   │              │
│  └─────────────────┘  └─────────────────┘              │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │ Application     │  │ Domain Event    │              │
│  │ Services        │  │ Handlers        │              │
│  └─────────────────┘  └─────────────────┘              │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                   Domain Layer                          │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │   Aggregates    │  │ Domain Services │              │
│  │   Entities      │  │ Domain Events   │              │
│  │ Value Objects   │  │ Repositories    │              │
│  └─────────────────┘  └─────────────────┘              │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│               Infrastructure Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │ EF Core         │  │ External APIs   │              │
│  │ Repositories    │  │ File Storage    │              │
│  └─────────────────┘  └─────────────────┘              │
└─────────────────────────────────────────────────────────┘
```

### レイヤー責務

#### Presentation Layer（プレゼンテーション層）
- **責務**: ユーザーインターフェース、HTTP API エンドポイント
- **技術**: Angular SPA、ABP vNext Web API Controllers
- **制約**: ドメインロジックを含まない、アプリケーション層のみ呼び出し

#### Application Layer（アプリケーション層）
- **責務**: ユースケースの調整、トランザクション境界、ドメインイベント処理
- **技術**: ABP Application Services、DTO変換、権限チェック
- **制約**: ビジネスロジックを含まない、ドメイン層とインフラ層を調整

#### Domain Layer（ドメイン層）
- **責務**: ビジネスロジック、ドメインルール、不変条件の維持
- **技術**: エンティティ、値オブジェクト、集約、ドメインサービス
- **制約**: 外部依存を持たない、純粋なビジネスロジックのみ

#### Infrastructure Layer（インフラ層）
- **責務**: 永続化、外部システム連携、技術的関心事
- **技術**: Entity Framework Core、ファイルシステム、外部API
- **制約**: ドメイン層のインターフェースを実装、技術詳細を隠蔽

### プロジェクト構造

```
VBlog_Angular/backend/src/
├── CanAnomalyDetection.Domain/              # ドメイン層
│   ├── CanSignals/                          # CAN信号集約
│   │   ├── CanSignal.cs                     # 集約ルート
│   │   ├── SignalSpecification.cs           # 値オブジェクト
│   │   ├── ICanSignalRepository.cs          # リポジトリ契約
│   │   └── Events/                          # ドメインイベント
│   ├── AnomalyDetection/                    # 異常検出集約
│   │   ├── AnomalyDetectionLogic.cs         # 集約ルート
│   │   ├── DetectionParameter.cs            # エンティティ
│   │   ├── SafetyClassification.cs          # 値オブジェクト
│   │   └── Services/                        # ドメインサービス
│   ├── VehiclePhases/                       # 車両フェーズ集約
│   ├── Traceability/                        # トレーサビリティ集約
│   ├── KnowledgeBase/                       # ナレッジベース集約
│   ├── MultiTenant/                         # マルチテナント集約
│   └── Shared/                              # 共有要素
│       ├── ValueObjects/                    # 共通値オブジェクト
│       ├── Enums/                          # 列挙型
│       └── Exceptions/                      # ドメイン例外
├── CanAnomalyDetection.Domain.Shared/       # 共有ドメイン
├── CanAnomalyDetection.Application/         # アプリケーション層
│   ├── CanSignals/                          # CAN信号アプリケーションサービス
│   ├── AnomalyDetection/                    # 異常検出アプリケーションサービス
│   ├── VehiclePhases/                       # 車両フェーズアプリケーションサービス
│   └── EventHandlers/                       # ドメインイベントハンドラー
├── CanAnomalyDetection.Application.Contracts/ # DTO・契約
├── CanAnomalyDetection.EntityFrameworkCore/ # インフラ層
│   ├── EntityConfigurations/                # EF設定
│   ├── Repositories/                        # リポジトリ実装
│   └── Migrations/                          # マイグレーション
└── CanAnomalyDetection.HttpApi.Host/        # プレゼンテーション層
    ├── Controllers/                         # Web API コントローラー
    └── Configuration/                       # 設定
```

## ドメインモデル設計

### 集約設計原則

1. **集約境界**: ビジネス不変条件を維持する最小単位
2. **集約ルート**: 外部からのアクセスポイント、整合性の責任者
3. **参照方式**: 集約間は ID による参照、直接オブジェクト参照は禁止
4. **トランザクション境界**: 1つの集約 = 1つのトランザクション
5. **サイズ制限**: 集約は小さく保つ、大きすぎる場合は分割検討

### 1.1 ドメインの境界とコンテキスト

#### Bounded Context Map

```
┌─────────────────────────────────────────────────────────┐
│                CAN異常検出管理ドメイン                    │
│                                                         │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │   CAN Signal    │  │ Anomaly Detection│              │
│  │   Management    │◄─┤    Context      │              │
│  │   Context       │  │                 │              │
│  └─────────────────┘  └─────────────────┘              │
│           │                     │                       │
│           ▼                     ▼                       │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │ Vehicle Phase   │  │  Traceability   │              │
│  │   Management    │  │   Management    │              │
│  │   Context       │  │   Context       │              │
│  └─────────────────┘  └─────────────────┘              │
│           │                     │                       │
│           ▼                     ▼                       │
│  ┌─────────────────┐  ┌─────────────────┐              │
│  │ Knowledge Base  │  │ Multi-Tenant    │              │
│  │   Context       │  │   Context       │              │
│  └─────────────────┘  └─────────────────┘              │
└─────────────────────────────────────────────────────────┘
```

#### コンテキスト間の関係

1. **CAN Signal Management Context**: CAN信号仕様の管理
2. **Anomaly Detection Context**: 異常検出ロジックの開発・実行
3. **Vehicle Phase Management Context**: 車両開発フェーズの管理・継承
4. **Traceability Management Context**: 機能安全トレーサビリティ
5. **Knowledge Base Context**: 異常検出ナレッジの蓄積・共有
6. **Multi-Tenant Context**: OEM別データ分離・共有制御

### 1.2 ドメインの核となる概念

#### ユビキタス言語（Ubiquitous Language）

**CAN信号関連:**
- **CAN信号（CAN Signal）**: 車両内ネットワークで送受信される制御信号
- **信号仕様（Signal Specification）**: CAN信号の技術的定義（ID、データ長、周期等）
- **CAN系統（CAN System）**: エンジン、ブレーキ等の車両システム分類
- **物理値変換（Physical Value Conversion）**: 生データから物理値への変換定義

**異常検出関連:**
- **異常検出ロジック（Anomaly Detection Logic）**: CAN信号の異常を判定するアルゴリズム
- **検出パターン（Detection Pattern）**: 範囲外、変化率、通信断等の異常パターン
- **検出パラメータ（Detection Parameter）**: 閾値、条件等の検出設定値
- **異常レベル（Anomaly Level）**: Info、Warning、Error、Critical、Fatalの重要度

**車両フェーズ関連:**
- **車両フェーズ（Vehicle Phase）**: 車両開発の段階（設計、試作、量産等）
- **フェーズ継承（Phase Inheritance）**: 過去フェーズからの情報流用
- **互換性分析（Compatibility Analysis）**: 継承時の適合性判定

**機能安全関連:**
- **ASIL（Automotive Safety Integrity Level）**: 自動車機能安全の完全性レベル
- **トレーサビリティ（Traceability）**: 要求から実装までの追跡可能性
- **変更管理（Change Management）**: 変更の承認・影響分析プロセス

**テナント関連:**
- **OEMテナント（OEM Tenant）**: 自動車メーカー専用のデータ空間
- **共有レベル（Sharing Level）**: プライベート、OEM内、業界共通、パブリック
- **情報流用（Information Reuse）**: 他フェーズ・OEMからの知見活用## 2. ド
メインモデル設計

### 2.1 CAN Signal Management Context

#### CAN Signal 集約

```csharp
// CAN Signal 集約ルート
public class CanSignal : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト
    public SignalIdentifier Identifier { get; private set; }
    public SignalSpecification Specification { get; private set; }
    public PhysicalValueConversion Conversion { get; private set; }
    public SignalTiming Timing { get; private set; }
    
    // エンティティ属性
    public CanSystemType SystemType { get; private set; }
    public string Description { get; private set; }
    public OemCode OemCode { get; private set; }
    public bool IsStandard { get; private set; }
    public SignalVersion Version { get; private set; }
    public DateTime? EffectiveDate { get; private set; }
    
    // ドメインイベント
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected CanSignal() { } // EF Core

    public CanSignal(
        Guid? tenantId,
        SignalIdentifier identifier,
        SignalSpecification specification,
        CanSystemType systemType,
        OemCode oemCode)
    {
        TenantId = tenantId;
        Identifier = Check.NotNull(identifier, nameof(identifier));
        Specification = Check.NotNull(specification, nameof(specification));
        SystemType = systemType;
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
        IsStandard = false;
        Version = SignalVersion.Initial();
        
        AddDomainEvent(new CanSignalCreatedDomainEvent(this));
    }

    // ビジネスメソッド
    public void UpdateSpecification(SignalSpecification newSpecification, string changeReason)
    {
        if (Specification.Equals(newSpecification))
            return;

        var oldSpecification = Specification;
        Specification = newSpecification;
        Version = Version.Increment();
        
        AddDomainEvent(new CanSignalSpecificationUpdatedDomainEvent(
            this, oldSpecification, newSpecification, changeReason));
    }

    public void SetAsStandard()
    {
        if (IsStandard)
            return;
            
        IsStandard = true;
        AddDomainEvent(new CanSignalMarkedAsStandardDomainEvent(this));
    }

    public bool IsCompatibleWith(CanSignal otherSignal)
    {
        return Identifier.CanId == otherSignal.Identifier.CanId &&
               Specification.IsCompatibleWith(otherSignal.Specification);
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// 値オブジェクト
public class SignalIdentifier : ValueObject
{
    public string SignalName { get; private set; }
    public string CanId { get; private set; }

    protected SignalIdentifier() { }

    public SignalIdentifier(string signalName, string canId)
    {
        SignalName = Check.NotNullOrWhiteSpace(signalName, nameof(signalName));
        CanId = ValidateCanId(canId);
    }

    private static string ValidateCanId(string canId)
    {
        Check.NotNullOrWhiteSpace(canId, nameof(canId));
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(canId, @"^[0-9A-Fa-f]{1,8}$"))
            throw new ArgumentException("CAN ID must be a valid hexadecimal value", nameof(canId));
            
        return canId.ToUpperInvariant();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return SignalName;
        yield return CanId;
    }
}

public class SignalSpecification : ValueObject
{
    public int StartBit { get; private set; }
    public int Length { get; private set; }
    public SignalDataType DataType { get; private set; }
    public SignalValueRange ValueRange { get; private set; }

    protected SignalSpecification() { }

    public SignalSpecification(int startBit, int length, SignalDataType dataType, SignalValueRange valueRange)
    {
        StartBit = ValidateStartBit(startBit);
        Length = ValidateLength(length);
        DataType = dataType;
        ValueRange = Check.NotNull(valueRange, nameof(valueRange));
    }

    public bool IsCompatibleWith(SignalSpecification other)
    {
        return StartBit == other.StartBit &&
               Length == other.Length &&
               DataType == other.DataType;
    }

    private static int ValidateStartBit(int startBit)
    {
        if (startBit < 0 || startBit > 63)
            throw new ArgumentOutOfRangeException(nameof(startBit), "Start bit must be between 0 and 63");
        return startBit;
    }

    private static int ValidateLength(int length)
    {
        if (length < 1 || length > 64)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 64");
        return length;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartBit;
        yield return Length;
        yield return DataType;
        yield return ValueRange;
    }
}

public class PhysicalValueConversion : ValueObject
{
    public double Factor { get; private set; }
    public double Offset { get; private set; }
    public string Unit { get; private set; }

    protected PhysicalValueConversion() { }

    public PhysicalValueConversion(double factor, double offset, string unit)
    {
        Factor = factor;
        Offset = offset;
        Unit = unit ?? string.Empty;
    }

    public double ConvertToPhysical(double rawValue)
    {
        return rawValue * Factor + Offset;
    }

    public double ConvertToRaw(double physicalValue)
    {
        return (physicalValue - Offset) / Factor;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Factor;
        yield return Offset;
        yield return Unit;
    }
}

// 列挙型
public enum CanSystemType
{
    Engine = 1,
    Brake = 2,
    Steering = 3,
    Transmission = 4,
    Body = 5,
    Chassis = 6,
    HVAC = 7,
    Lighting = 8,
    Infotainment = 9,
    Safety = 10,
    Powertrain = 11,
    Gateway = 12
}

public enum SignalDataType
{
    Unsigned = 1,
    Signed = 2,
    Float = 3,
    Double = 4
}
```

### 2.2 Anomaly Detection Context

#### Anomaly Detection Logic 集約

```csharp
// 異常検出ロジック集約ルート
public class AnomalyDetectionLogic : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト
    public DetectionLogicIdentity Identity { get; private set; }
    public DetectionLogicSpecification Specification { get; private set; }
    public LogicImplementation Implementation { get; private set; }
    public SafetyClassification Safety { get; private set; }
    
    // エンティティ
    private readonly List<DetectionParameter> _parameters = new();
    private readonly List<CanSignalMapping> _signalMappings = new();
    
    public IReadOnlyList<DetectionParameter> Parameters => _parameters.AsReadOnly();
    public IReadOnlyList<CanSignalMapping> SignalMappings => _signalMappings.AsReadOnly();
    
    // 属性
    public DetectionLogicStatus Status { get; private set; }
    public SharingLevel SharingLevel { get; private set; }
    public Guid? SourceLogicId { get; private set; }
    public Guid? VehiclePhaseId { get; private set; }
    
    // ドメインイベント
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AnomalyDetectionLogic() { }

    public AnomalyDetectionLogic(
        Guid? tenantId,
        DetectionLogicIdentity identity,
        DetectionLogicSpecification specification,
        SafetyClassification safety)
    {
        TenantId = tenantId;
        Identity = Check.NotNull(identity, nameof(identity));
        Specification = Check.NotNull(specification, nameof(specification));
        Safety = Check.NotNull(safety, nameof(safety));
        Status = DetectionLogicStatus.Draft;
        SharingLevel = SharingLevel.Private;
        
        AddDomainEvent(new DetectionLogicCreatedDomainEvent(this));
    }

    // ビジネスメソッド
    public void UpdateImplementation(LogicImplementation newImplementation)
    {
        Check.NotNull(newImplementation, nameof(newImplementation));
        
        if (Status == DetectionLogicStatus.Approved)
            throw new BusinessException("Cannot update implementation of approved logic");
            
        Implementation = newImplementation;
        AddDomainEvent(new DetectionLogicImplementationUpdatedDomainEvent(this));
    }

    public void AddParameter(DetectionParameter parameter)
    {
        Check.NotNull(parameter, nameof(parameter));
        
        if (_parameters.Any(p => p.Name == parameter.Name))
            throw new BusinessException($"Parameter '{parameter.Name}' already exists");
            
        _parameters.Add(parameter);
    }

    public void AddSignalMapping(CanSignalMapping mapping)
    {
        Check.NotNull(mapping, nameof(mapping));
        
        if (_signalMappings.Any(m => m.CanSignalId == mapping.CanSignalId))
            throw new BusinessException("Signal is already mapped");
            
        _signalMappings.Add(mapping);
    }

    public void SubmitForApproval()
    {
        if (Status != DetectionLogicStatus.Draft)
            throw new BusinessException("Only draft logic can be submitted for approval");
            
        if (Implementation == null)
            throw new BusinessException("Implementation is required for approval");
            
        if (!_signalMappings.Any())
            throw new BusinessException("At least one signal mapping is required");
            
        Status = DetectionLogicStatus.PendingApproval;
        AddDomainEvent(new DetectionLogicSubmittedForApprovalDomainEvent(this));
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != DetectionLogicStatus.PendingApproval)
            throw new BusinessException("Only pending logic can be approved");
            
        if (Safety.AsilLevel >= AsilLevel.C)
        {
            // ASIL C/D requires additional validation
            ValidateHighAsilRequirements();
        }
        
        Status = DetectionLogicStatus.Approved;
        AddDomainEvent(new DetectionLogicApprovedDomainEvent(this, approvedBy));
    }

    public DetectionResult ExecuteDetection(Dictionary<string, object> inputData)
    {
        if (Status != DetectionLogicStatus.Approved)
            throw new BusinessException("Only approved logic can be executed");
            
        if (Implementation == null)
            throw new BusinessException("Implementation is required for execution");
            
        // ドメインサービスに委譲
        return new DetectionExecutionService().Execute(this, inputData);
    }

    private void ValidateHighAsilRequirements()
    {
        // ASIL C/D specific validation logic
        if (string.IsNullOrEmpty(Safety.SafetyRequirementId))
            throw new BusinessException("Safety requirement ID is required for ASIL C/D");
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// 値オブジェクト
public class DetectionLogicIdentity : ValueObject
{
    public string Name { get; private set; }
    public LogicVersion Version { get; private set; }
    public OemCode OemCode { get; private set; }

    protected DetectionLogicIdentity() { }

    public DetectionLogicIdentity(string name, LogicVersion version, OemCode oemCode)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Version = Check.NotNull(version, nameof(version));
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Name;
        yield return Version;
        yield return OemCode;
    }
}

public class SafetyClassification : ValueObject
{
    public AsilLevel AsilLevel { get; private set; }
    public string SafetyRequirementId { get; private set; }
    public string SafetyGoalId { get; private set; }

    protected SafetyClassification() { }

    public SafetyClassification(AsilLevel asilLevel, string safetyRequirementId = null, string safetyGoalId = null)
    {
        AsilLevel = asilLevel;
        SafetyRequirementId = safetyRequirementId;
        SafetyGoalId = safetyGoalId;
    }

    public bool RequiresApproval()
    {
        return AsilLevel >= AsilLevel.B;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return AsilLevel;
        yield return SafetyRequirementId ?? string.Empty;
        yield return SafetyGoalId ?? string.Empty;
    }
}

// エンティティ
public class DetectionParameter : Entity<Guid>
{
    public string Name { get; private set; }
    public ParameterDataType DataType { get; private set; }
    public string DefaultValue { get; private set; }
    public ParameterConstraints Constraints { get; private set; }
    public string Description { get; private set; }
    public bool IsRequired { get; private set; }

    protected DetectionParameter() { }

    public DetectionParameter(
        string name,
        ParameterDataType dataType,
        string defaultValue,
        ParameterConstraints constraints,
        string description,
        bool isRequired)
    {
        Id = Guid.NewGuid();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        DataType = dataType;
        DefaultValue = defaultValue;
        Constraints = constraints;
        Description = description;
        IsRequired = isRequired;
    }
}

// 列挙型
public enum DetectionLogicStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Deprecated = 4
}

public enum AsilLevel
{
    QM = 0,
    A = 1,
    B = 2,
    C = 3,
    D = 4
}

public enum SharingLevel
{
    Private = 0,
    OemPartner = 1,
    Industry = 2,
    Public = 3
}
```### 2.3 
Vehicle Phase Management Context

#### Vehicle Phase 集約

```csharp
// 車両フェーズ集約ルート
public class VehiclePhase : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト
    public VehicleIdentity Identity { get; private set; }
    public PhaseSpecification Specification { get; private set; }
    public CanSpecificationInfo CanSpecification { get; private set; }
    
    // エンティティ
    private readonly List<PhaseCanSignalAssociation> _canSignals = new();
    private readonly List<PhaseDetectionLogicAssociation> _detectionLogics = new();
    
    public IReadOnlyList<PhaseCanSignalAssociation> CanSignals => _canSignals.AsReadOnly();
    public IReadOnlyList<PhaseDetectionLogicAssociation> DetectionLogics => _detectionLogics.AsReadOnly();
    
    // 継承関係
    public Guid? BaseVehiclePhaseId { get; private set; }
    private readonly List<PhaseInheritanceRecord> _inheritanceRecords = new();
    public IReadOnlyList<PhaseInheritanceRecord> InheritanceRecords => _inheritanceRecords.AsReadOnly();
    
    // 属性
    public VehiclePhaseStatus Status { get; private set; }
    public PhasePeriod Period { get; private set; }
    
    // ドメインイベント
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected VehiclePhase() { }

    public VehiclePhase(
        Guid? tenantId,
        VehicleIdentity identity,
        PhaseSpecification specification,
        PhasePeriod period)
    {
        TenantId = tenantId;
        Identity = Check.NotNull(identity, nameof(identity));
        Specification = Check.NotNull(specification, nameof(specification));
        Period = Check.NotNull(period, nameof(period));
        Status = VehiclePhaseStatus.Planning;
        
        AddDomainEvent(new VehiclePhaseCreatedDomainEvent(this));
    }

    // ビジネスメソッド
    public void StartPhase()
    {
        if (Status != VehiclePhaseStatus.Planning)
            throw new BusinessException("Only planning phase can be started");
            
        Status = VehiclePhaseStatus.Active;
        AddDomainEvent(new VehiclePhaseStartedDomainEvent(this));
    }

    public void CompletePhase()
    {
        if (Status != VehiclePhaseStatus.Active)
            throw new BusinessException("Only active phase can be completed");
            
        Status = VehiclePhaseStatus.Completed;
        AddDomainEvent(new VehiclePhaseCompletedDomainEvent(this));
    }

    public void UpdateCanSpecification(CanSpecificationInfo newCanSpec)
    {
        Check.NotNull(newCanSpec, nameof(newCanSpec));
        
        var oldCanSpec = CanSpecification;
        CanSpecification = newCanSpec;
        
        AddDomainEvent(new CanSpecificationUpdatedDomainEvent(this, oldCanSpec, newCanSpec));
    }

    public void AssociateCanSignal(Guid canSignalId, bool isActive, string notes = null)
    {
        if (_canSignals.Any(cs => cs.CanSignalId == canSignalId))
            throw new BusinessException("CAN signal is already associated with this phase");
            
        var association = new PhaseCanSignalAssociation(Id, canSignalId, isActive, notes);
        _canSignals.Add(association);
    }

    public void AssociateDetectionLogic(Guid detectionLogicId, string purpose)
    {
        if (_detectionLogics.Any(dl => dl.DetectionLogicId == detectionLogicId))
            throw new BusinessException("Detection logic is already associated with this phase");
            
        var association = new PhaseDetectionLogicAssociation(Id, detectionLogicId, purpose);
        _detectionLogics.Add(association);
    }

    public PhaseInheritanceResult InheritFrom(VehiclePhase sourcePhase, InheritanceStrategy strategy)
    {
        Check.NotNull(sourcePhase, nameof(sourcePhase));
        Check.NotNull(strategy, nameof(strategy));
        
        if (sourcePhase.Status != VehiclePhaseStatus.Completed)
            throw new BusinessException("Can only inherit from completed phases");
            
        var inheritanceService = new PhaseInheritanceService();
        var result = inheritanceService.ExecuteInheritance(this, sourcePhase, strategy);
        
        var record = new PhaseInheritanceRecord(
            TenantId,
            sourcePhase.Id,
            Id,
            strategy.Type,
            result);
            
        _inheritanceRecords.Add(record);
        
        AddDomainEvent(new PhaseInheritanceExecutedDomainEvent(this, sourcePhase, result));
        
        return result;
    }

    public CompatibilityAnalysisResult AnalyzeCompatibilityWith(VehiclePhase otherPhase)
    {
        Check.NotNull(otherPhase, nameof(otherPhase));
        
        var analysisService = new CompatibilityAnalysisService();
        return analysisService.Analyze(this, otherPhase);
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// 値オブジェクト
public class VehicleIdentity : ValueObject
{
    public string VehicleName { get; private set; }
    public string ModelCode { get; private set; }
    public string ModelYear { get; private set; }
    public OemCode OemCode { get; private set; }

    protected VehicleIdentity() { }

    public VehicleIdentity(string vehicleName, string modelCode, string modelYear, OemCode oemCode)
    {
        VehicleName = Check.NotNullOrWhiteSpace(vehicleName, nameof(vehicleName));
        ModelCode = Check.NotNullOrWhiteSpace(modelCode, nameof(modelCode));
        ModelYear = ValidateModelYear(modelYear);
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
    }

    private static string ValidateModelYear(string modelYear)
    {
        Check.NotNullOrWhiteSpace(modelYear, nameof(modelYear));
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(modelYear, @"^\d{4}$"))
            throw new ArgumentException("Model year must be a 4-digit year", nameof(modelYear));
            
        var year = int.Parse(modelYear);
        var currentYear = DateTime.Now.Year;
        
        if (year < currentYear - 10 || year > currentYear + 10)
            throw new ArgumentException("Model year must be within reasonable range", nameof(modelYear));
            
        return modelYear;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return VehicleName;
        yield return ModelCode;
        yield return ModelYear;
        yield return OemCode;
    }
}

public class PhaseSpecification : ValueObject
{
    public string DevelopmentPhase { get; private set; }
    public string Platform { get; private set; }
    public VehicleSegment Segment { get; private set; }

    protected PhaseSpecification() { }

    public PhaseSpecification(string developmentPhase, string platform, VehicleSegment segment)
    {
        DevelopmentPhase = Check.NotNullOrWhiteSpace(developmentPhase, nameof(developmentPhase));
        Platform = platform;
        Segment = segment;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DevelopmentPhase;
        yield return Platform ?? string.Empty;
        yield return Segment;
    }
}

public class CanSpecificationInfo : ValueObject
{
    public string Version { get; private set; }
    public string FilePath { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public string Checksum { get; private set; }

    protected CanSpecificationInfo() { }

    public CanSpecificationInfo(string version, string filePath, DateTime lastUpdated, string checksum)
    {
        Version = Check.NotNullOrWhiteSpace(version, nameof(version));
        FilePath = filePath;
        LastUpdated = lastUpdated;
        Checksum = checksum;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Version;
        yield return FilePath ?? string.Empty;
        yield return LastUpdated;
        yield return Checksum ?? string.Empty;
    }
}

// エンティティ
public class PhaseInheritanceRecord : Entity<Guid>
{
    public Guid? TenantId { get; private set; }
    public Guid SourcePhaseId { get; private set; }
    public Guid TargetPhaseId { get; private set; }
    public InheritanceType Type { get; private set; }
    public InheritanceResult Result { get; private set; }
    public DateTime ExecutionTime { get; private set; }

    protected PhaseInheritanceRecord() { }

    public PhaseInheritanceRecord(
        Guid? tenantId,
        Guid sourcePhaseId,
        Guid targetPhaseId,
        InheritanceType type,
        InheritanceResult result)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        SourcePhaseId = sourcePhaseId;
        TargetPhaseId = targetPhaseId;
        Type = type;
        Result = Check.NotNull(result, nameof(result));
        ExecutionTime = DateTime.UtcNow;
    }
}

// 列挙型
public enum VehiclePhaseStatus
{
    Planning = 0,
    Active = 1,
    Testing = 2,
    Completed = 3,
    Cancelled = 4
}

public enum VehicleSegment
{
    A = 1,  // Mini cars
    B = 2,  // Small cars
    C = 3,  // Medium cars
    D = 4,  // Large cars
    E = 5,  // Executive cars
    F = 6,  // Luxury cars
    SUV = 7,
    Truck = 8,
    Bus = 9
}

public enum InheritanceType
{
    FullInheritance = 1,
    PartialInheritance = 2,
    ModifiedInheritance = 3,
    CustomInheritance = 4
}
```

### 2.4 Traceability Management Context

#### Traceability Link 集約

```csharp
// トレーサビリティリンク集約ルート
public class TraceabilityLink : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト
    public TraceabilityRelation Relation { get; private set; }
    public TraceabilityEvidence Evidence { get; private set; }
    public SafetyContext SafetyContext { get; private set; }
    
    // 変更管理
    public ChangeManagementInfo ChangeInfo { get; private set; }
    public TraceabilityStatus Status { get; private set; }
    
    // ドメインイベント
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected TraceabilityLink() { }

    public TraceabilityLink(
        Guid? tenantId,
        TraceabilityRelation relation,
        TraceabilityEvidence evidence,
        SafetyContext safetyContext)
    {
        TenantId = tenantId;
        Relation = Check.NotNull(relation, nameof(relation));
        Evidence = Check.NotNull(evidence, nameof(evidence));
        SafetyContext = Check.NotNull(safetyContext, nameof(safetyContext));
        Status = TraceabilityStatus.Draft;
        
        AddDomainEvent(new TraceabilityLinkCreatedDomainEvent(this));
    }

    // ビジネスメソッド
    public void UpdateEvidence(TraceabilityEvidence newEvidence, string changeReason)
    {
        Check.NotNull(newEvidence, nameof(newEvidence));
        Check.NotNullOrWhiteSpace(changeReason, nameof(changeReason));
        
        var oldEvidence = Evidence;
        Evidence = newEvidence;
        
        var changeInfo = new ChangeManagementInfo(
            Guid.NewGuid().ToString(),
            changeReason,
            DateTime.UtcNow);
        ChangeInfo = changeInfo;
        
        AddDomainEvent(new TraceabilityEvidenceUpdatedDomainEvent(this, oldEvidence, newEvidence, changeReason));
    }

    public void Validate()
    {
        if (Status == TraceabilityStatus.Validated)
            return;
            
        var validationService = new TraceabilityValidationService();
        var validationResult = validationService.Validate(this);
        
        if (validationResult.IsValid)
        {
            Status = TraceabilityStatus.Validated;
            AddDomainEvent(new TraceabilityLinkValidatedDomainEvent(this));
        }
        else
        {
            Status = TraceabilityStatus.Invalid;
            AddDomainEvent(new TraceabilityLinkInvalidatedDomainEvent(this, validationResult.Errors));
        }
    }

    public ImpactAnalysisResult AnalyzeChangeImpact()
    {
        var impactService = new ChangeImpactAnalysisService();
        return impactService.Analyze(this);
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// 値オブジェクト
public class TraceabilityRelation : ValueObject
{
    public TraceabilityArtifact Source { get; private set; }
    public TraceabilityArtifact Target { get; private set; }
    public TraceabilityType Type { get; private set; }

    protected TraceabilityRelation() { }

    public TraceabilityRelation(TraceabilityArtifact source, TraceabilityArtifact target, TraceabilityType type)
    {
        Source = Check.NotNull(source, nameof(source));
        Target = Check.NotNull(target, nameof(target));
        Type = type;
        
        ValidateRelation();
    }

    private void ValidateRelation()
    {
        if (Source.Equals(Target))
            throw new BusinessException("Source and target cannot be the same artifact");
            
        if (!IsValidRelationType(Source.Type, Target.Type, Type))
            throw new BusinessException($"Invalid traceability relation: {Source.Type} -> {Target.Type} ({Type})");
    }

    private static bool IsValidRelationType(string sourceType, string targetType, TraceabilityType relationType)
    {
        // ビジネスルールに基づく関係の妥当性チェック
        return relationType switch
        {
            TraceabilityType.Derives => sourceType == "SafetyRequirement" && targetType == "SystemRequirement",
            TraceabilityType.Implements => targetType == "DetectionLogic",
            TraceabilityType.Verifies => targetType == "TestCase",
            TraceabilityType.Validates => sourceType == "TestCase" && targetType == "Requirement",
            _ => true
        };
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Source;
        yield return Target;
        yield return Type;
    }
}

public class TraceabilityArtifact : ValueObject
{
    public string Type { get; private set; }
    public string Id { get; private set; }
    public string Name { get; private set; }

    protected TraceabilityArtifact() { }

    public TraceabilityArtifact(string type, string id, string name)
    {
        Type = Check.NotNullOrWhiteSpace(type, nameof(type));
        Id = Check.NotNullOrWhiteSpace(id, nameof(id));
        Name = name ?? string.Empty;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return Id;
        yield return Name;
    }
}

// 列挙型
public enum TraceabilityType
{
    Derives = 1,
    Implements = 2,
    Verifies = 3,
    Validates = 4,
    Refines = 5,
    Satisfies = 6
}

public enum TraceabilityStatus
{
    Draft = 0,
    Validated = 1,
    Invalid = 2,
    Obsolete = 3
}
```### エ
ンティティ (Entities)

#### 設計原則
- **アイデンティティ**: 一意のIDを持ち、ライフサイクル全体で同一性を保つ
- **可変性**: 状態変更可能だが、ビジネスルールに従って制御
- **責務**: 自身の状態とビジネスルールの維持

#### 主要エンティティ

```csharp
// CAN Signal エンティティ（集約ルート）
public class CanSignal : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // アイデンティティ
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト（不変な概念）
    public SignalIdentifier Identifier { get; private set; }
    public SignalSpecification Specification { get; private set; }
    public PhysicalValueConversion Conversion { get; private set; }
    
    // エンティティ属性
    public CanSystemType SystemType { get; private set; }
    public bool IsStandard { get; private set; }
    public SignalVersion Version { get; private set; }
    
    // ビジネスメソッド
    public void UpdateSpecification(SignalSpecification newSpec, string reason)
    {
        // ビジネスルール: 仕様変更時はバージョン更新必須
        if (Specification.Equals(newSpec)) return;
        
        Specification = newSpec;
        Version = Version.Increment();
        
        // ドメインイベント発行
        AddDomainEvent(new CanSignalSpecificationUpdatedDomainEvent(this, reason));
    }
    
    public bool IsCompatibleWith(CanSignal other)
    {
        // ビジネスルール: CAN ID とデータ構造の互換性チェック
        return Identifier.CanId == other.Identifier.CanId &&
               Specification.IsCompatibleWith(other.Specification);
    }
}

// Detection Logic エンティティ（集約ルート）
public class AnomalyDetectionLogic : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 値オブジェクト
    public DetectionLogicIdentity Identity { get; private set; }
    public SafetyClassification Safety { get; private set; }
    
    // 子エンティティ（集約内）
    private readonly List<DetectionParameter> _parameters = new();
    public IReadOnlyList<DetectionParameter> Parameters => _parameters.AsReadOnly();
    
    // 状態
    public DetectionLogicStatus Status { get; private set; }
    
    // ビジネスメソッド
    public void SubmitForApproval()
    {
        // ビジネスルール: 下書き状態のみ承認申請可能
        if (Status != DetectionLogicStatus.Draft)
            throw new BusinessException("Only draft logic can be submitted");
            
        // ビジネスルール: 実装とパラメータが必要
        ValidateForApproval();
        
        Status = DetectionLogicStatus.PendingApproval;
        AddDomainEvent(new DetectionLogicSubmittedDomainEvent(this));
    }
    
    public void Approve(Guid approvedBy)
    {
        // ビジネスルール: ASIL-C以上は追加検証必要
        if (Safety.AsilLevel >= AsilLevel.C)
            ValidateHighAsilRequirements();
            
        Status = DetectionLogicStatus.Approved;
        AddDomainEvent(new DetectionLogicApprovedDomainEvent(this, approvedBy));
    }
}
```

### 値オブジェクト (Value Objects)

#### 設計原則
- **不変性**: 作成後は変更不可、変更時は新しいインスタンス作成
- **等価性**: 値による等価性、構造的等価性
- **副作用なし**: メソッド実行による状態変更なし

#### 主要値オブジェクト

```csharp
// Signal Identifier 値オブジェクト
public class SignalIdentifier : ValueObject
{
    public string SignalName { get; private set; }
    public string CanId { get; private set; }

    public SignalIdentifier(string signalName, string canId)
    {
        SignalName = Check.NotNullOrWhiteSpace(signalName, nameof(signalName));
        CanId = ValidateCanId(canId);
    }

    private static string ValidateCanId(string canId)
    {
        // ビジネスルール: CAN IDは16進数形式
        if (!Regex.IsMatch(canId, @"^[0-9A-Fa-f]{1,8}$"))
            throw new ArgumentException("Invalid CAN ID format");
        return canId.ToUpperInvariant();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return SignalName;
        yield return CanId;
    }
}

// Safety Classification 値オブジェクト
public class SafetyClassification : ValueObject
{
    public AsilLevel AsilLevel { get; private set; }
    public string SafetyRequirementId { get; private set; }

    public SafetyClassification(AsilLevel asilLevel, string safetyRequirementId = null)
    {
        AsilLevel = asilLevel;
        SafetyRequirementId = safetyRequirementId;
        
        // ビジネスルール: ASIL-B以上は安全要求ID必須
        if (asilLevel >= AsilLevel.B && string.IsNullOrEmpty(safetyRequirementId))
            throw new BusinessException("Safety requirement ID required for ASIL-B and above");
    }

    public bool RequiresApproval() => AsilLevel >= AsilLevel.B;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return AsilLevel;
        yield return SafetyRequirementId ?? string.Empty;
    }
}

// Physical Value Conversion 値オブジェクト
public class PhysicalValueConversion : ValueObject
{
    public double Factor { get; private set; }
    public double Offset { get; private set; }
    public string Unit { get; private set; }

    public PhysicalValueConversion(double factor, double offset, string unit)
    {
        Factor = ValidateFactor(factor);
        Offset = offset;
        Unit = unit ?? string.Empty;
    }

    public double ConvertToPhysical(double rawValue) => rawValue * Factor + Offset;
    public double ConvertToRaw(double physicalValue) => (physicalValue - Offset) / Factor;

    private static double ValidateFactor(double factor)
    {
        if (Math.Abs(factor) < double.Epsilon)
            throw new ArgumentException("Factor cannot be zero");
        return factor;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Factor;
        yield return Offset;
        yield return Unit;
    }
}
```

### 集約 (Aggregates)

#### 設計原則
- **整合性境界**: 集約内の不変条件を保証
- **トランザクション境界**: 集約単位でのデータ整合性
- **参照制限**: 集約外への直接参照禁止、IDによる参照のみ

#### 主要集約

```csharp
// CAN Signal 集約
public class CanSignal : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // 集約ルートとしての責務
    // 1. CAN信号仕様の整合性維持
    // 2. バージョン管理
    // 3. 標準信号の管理
    
    // 集約境界内のエンティティ
    private readonly List<SignalHistory> _history = new();
    public IReadOnlyList<SignalHistory> History => _history.AsReadOnly();
    
    // 不変条件の維持
    public void UpdateSpecification(SignalSpecification newSpec, string reason)
    {
        // 不変条件: 仕様変更時は履歴記録必須
        var historyEntry = new SignalHistory(Specification, reason, DateTime.UtcNow);
        _history.Add(historyEntry);
        
        Specification = newSpec;
        Version = Version.Increment();
    }
}

// Vehicle Phase 集約
public class VehiclePhase : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // 集約ルートとしての責務
    // 1. フェーズライフサイクル管理
    // 2. 継承関係の整合性
    // 3. CAN仕様との関連管理
    
    // 集約境界内のエンティティ
    private readonly List<PhaseCanSignalAssociation> _canSignals = new();
    private readonly List<PhaseInheritanceRecord> _inheritanceRecords = new();
    
    // 不変条件: フェーズ状態遷移ルール
    public void StartPhase()
    {
        if (Status != VehiclePhaseStatus.Planning)
            throw new BusinessException("Only planning phase can be started");
        Status = VehiclePhaseStatus.Active;
    }
    
    // 不変条件: 完了フェーズからのみ継承可能
    public void InheritFrom(Guid sourcePhaseId, InheritanceStrategy strategy)
    {
        // ドメインサービスに委譲
        var inheritanceService = new PhaseInheritanceService();
        var result = inheritanceService.ExecuteInheritance(this, sourcePhaseId, strategy);
        
        var record = new PhaseInheritanceRecord(sourcePhaseId, Id, strategy.Type, result);
        _inheritanceRecords.Add(record);
    }
}
```### 
ドメインサービス (Domain Services)

#### 設計原則
- **複数集約にまたがるロジック**: 単一エンティティに属さないビジネスロジック
- **ステートレス**: 状態を持たない、純粋な振る舞い
- **ドメイン知識**: ビジネスルールとドメイン知識のカプセル化

#### 主要ドメインサービス

```csharp
// Phase Inheritance Service（車両フェーズ継承サービス）
public class PhaseInheritanceService : DomainService
{
    private readonly ICanSignalRepository _canSignalRepository;
    private readonly IAnomalyDetectionLogicRepository _detectionLogicRepository;

    public PhaseInheritanceResult ExecuteInheritance(
        VehiclePhase targetPhase,
        VehiclePhase sourcePhase,
        InheritanceStrategy strategy)
    {
        // ビジネスルール: 完了フェーズからのみ継承可能
        if (sourcePhase.Status != VehiclePhaseStatus.Completed)
            throw new BusinessException("Can only inherit from completed phases");

        // 互換性分析
        var compatibilityResult = AnalyzeCompatibility(targetPhase, sourcePhase);
        
        // 継承実行
        var inheritedSignals = InheritCanSignals(targetPhase, sourcePhase, strategy);
        var inheritedLogics = InheritDetectionLogics(targetPhase, sourcePhase, strategy);
        
        // 利用履歴・設計背景の継承
        var inheritedContext = InheritDesignContext(sourcePhase, targetPhase);
        
        return new PhaseInheritanceResult(
            inheritedSignals,
            inheritedLogics,
            inheritedContext,
            compatibilityResult);
    }

    private CompatibilityAnalysisResult AnalyzeCompatibility(
        VehiclePhase targetPhase,
        VehiclePhase sourcePhase)
    {
        // CAN仕様バージョンの互換性チェック
        var canSpecCompatibility = CompareCanSpecifications(
            targetPhase.CanSpecification,
            sourcePhase.CanSpecification);
        
        // プラットフォーム互換性チェック
        var platformCompatibility = ComparePlatforms(
            targetPhase.Specification.Platform,
            sourcePhase.Specification.Platform);
        
        return new CompatibilityAnalysisResult(canSpecCompatibility, platformCompatibility);
    }

    private List<DesignContext> InheritDesignContext(
        VehiclePhase sourcePhase,
        VehiclePhase targetPhase)
    {
        // 設計背景・検査データの継承
        var contexts = new List<DesignContext>();
        
        foreach (var logic in sourcePhase.DetectionLogics)
        {
            var context = new DesignContext
            {
                SourceVehicle = sourcePhase.Identity.VehicleName,
                SourcePhase = sourcePhase.Specification.DevelopmentPhase,
                SourceOem = sourcePhase.Identity.OemCode.Value,
                DesignRationale = logic.DesignRationale,
                TestEvidence = logic.TestEvidence,
                InheritedAt = DateTime.UtcNow,
                TargetVehicle = targetPhase.Identity.VehicleName
            };
            contexts.Add(context);
        }
        
        return contexts;
    }
}

// Compatibility Analysis Service（互換性分析サービス）
public class CompatibilityAnalysisService : DomainService
{
    public CompatibilityAnalysisResult Analyze(VehiclePhase phase1, VehiclePhase phase2)
    {
        var canSpecDiff = CompareCanSpecifications(
            phase1.CanSpecification,
            phase2.CanSpecification);
        
        var signalDiff = CompareSignals(phase1, phase2);
        
        var recommendations = GenerateRecommendations(canSpecDiff, signalDiff);
        
        return new CompatibilityAnalysisResult
        {
            CanSpecificationDifferences = canSpecDiff,
            SignalDifferences = signalDiff,
            CompatibilityStatus = DetermineCompatibilityStatus(canSpecDiff, signalDiff),
            Recommendations = recommendations
        };
    }
}

// Detection Execution Service（検出実行サービス）
public class DetectionExecutionService : DomainService
{
    public DetectionResult Execute(
        AnomalyDetectionLogic logic,
        Dictionary<string, object> inputData)
    {
        // 入力データの妥当性検証
        ValidateInputData(logic, inputData);
        
        // 検出開始時刻記録
        var startTime = DateTime.UtcNow;
        
        // ロジック実行
        var executionResult = ExecuteLogic(logic.Implementation, inputData);
        
        // 検出時間計測
        var detectionTime = DateTime.UtcNow - startTime;
        
        // 結果の構造化
        return new DetectionResult
        {
            LogicId = logic.Id,
            ExecutionTime = DateTime.UtcNow,
            DetectionDuration = detectionTime,
            InputData = inputData,
            IsAnomalyDetected = executionResult.IsAnomaly,
            AnomalyLevel = executionResult.Level,
            AnomalyType = executionResult.AnomalyType, // 異常種類
            DetectionCondition = executionResult.Condition, // 検出条件
            ConfidenceScore = executionResult.Confidence,
            Details = executionResult.Details
        };
    }
}

// Anomaly Analysis Service（異常分析サービス）
public class AnomalyAnalysisService : DomainService
{
    private readonly IDetectionResultRepository _resultRepository;
    
    public AnomalyPatternAnalysisResult AnalyzePattern(
        Guid canSignalId,
        DateTime startDate,
        DateTime endDate)
    {
        // 特定CAN信号の異常検出履歴を取得
        var results = _resultRepository.GetByCanSignalAndPeriod(canSignalId, startDate, endDate);
        
        // 異常種類別の統計
        var anomalyTypeStats = results
            .GroupBy(r => r.AnomalyType)
            .Select(g => new AnomalyTypeStatistics
            {
                AnomalyType = g.Key,
                Count = g.Count(),
                AverageDetectionTime = g.Average(r => r.DetectionDuration.TotalMilliseconds),
                Frequency = g.Count() / (double)results.Count
            }).ToList();
        
        // 検出時間の統計
        var detectionTimeStats = new DetectionTimeStatistics
        {
            AverageTime = results.Average(r => r.DetectionDuration.TotalMilliseconds),
            MinTime = results.Min(r => r.DetectionDuration.TotalMilliseconds),
            MaxTime = results.Max(r => r.DetectionDuration.TotalMilliseconds),
            MedianTime = CalculateMedian(results.Select(r => r.DetectionDuration.TotalMilliseconds))
        };
        
        // 閾値最適化推奨
        var thresholdRecommendations = GenerateThresholdRecommendations(results);
        
        return new AnomalyPatternAnalysisResult
        {
            CanSignalId = canSignalId,
            AnalysisPeriod = new Period(startDate, endDate),
            TotalAnomalies = results.Count,
            AnomalyTypeStatistics = anomalyTypeStats,
            DetectionTimeStatistics = detectionTimeStats,
            ThresholdRecommendations = thresholdRecommendations,
            DetectionAccuracy = CalculateDetectionAccuracy(results)
        };
    }
    
    private List<ThresholdRecommendation> GenerateThresholdRecommendations(
        List<DetectionResult> results)
    {
        // 誤検出率・検出漏れ率から最適閾値を推奨
        var recommendations = new List<ThresholdRecommendation>();
        
        // 統計分析に基づく推奨ロジック
        // （実装詳細は省略）
        
        return recommendations;
    }
    
    private DetectionAccuracyMetrics CalculateDetectionAccuracy(
        List<DetectionResult> results)
    {
        // 検出精度指標の算出
        return new DetectionAccuracyMetrics
        {
            DetectionRate = CalculateDetectionRate(results),
            FalsePositiveRate = CalculateFalsePositiveRate(results),
            FalseNegativeRate = CalculateFalseNegativeRate(results),
            AverageResponseTime = results.Average(r => r.DetectionDuration.TotalMilliseconds)
        };
    }
}

// Traceability Query Service（トレーサビリティ照会サービス）
public class TraceabilityQueryService : DomainService
{
    private readonly IUsageHistoryRepository _usageHistoryRepository;
    private readonly IDesignRationaleRepository _designRationaleRepository;
    private readonly ITestEvidenceRepository _testEvidenceRepository;

    public UsageTraceabilityResult TraceUsageHistory(Guid entityId, string entityType)
    {
        // 「このCAN信号/ロジックはどの車両で使われたか」を追跡
        var usageHistory = _usageHistoryRepository.GetByEntityId(entityId, entityType);
        
        return new UsageTraceabilityResult
        {
            EntityId = entityId,
            EntityType = entityType,
            UsedInVehicles = usageHistory.Select(h => new VehicleUsageInfo
            {
                VehicleName = h.VehicleName,
                ModelCode = h.ModelCode,
                Phase = h.Phase,
                OemCode = h.OemCode,
                UsagePeriod = new Period(h.StartDate, h.EndDate),
                DesignRationale = h.DesignRationale,
                TestEvidence = h.TestEvidence
            }).ToList()
        };
    }

    public DesignBackgroundResult TraceDesignBackground(Guid entityId, string entityType)
    {
        // 「なぜこの仕様になったか」を追跡
        var rationale = _designRationaleRepository.GetByEntityId(entityId, entityType);
        var testEvidence = _testEvidenceRepository.GetByEntityId(entityId, entityType);
        
        return new DesignBackgroundResult
        {
            EntityId = entityId,
            DesignRationale = rationale.Reason,
            Constraints = rationale.Constraints,
            Assumptions = rationale.Assumptions,
            TestEvidence = testEvidence.Select(e => new TestEvidenceInfo
            {
                TestDate = e.TestDate,
                TestScenario = e.Scenario,
                TestData = e.Data,
                TestResult = e.Result,
                Conclusion = e.Conclusion
            }).ToList()
        };
    }
    
    public OemTraceabilityResult TraceAcrossOems(Guid entityId, string entityType)
    {
        // OEM間のトレーサビリティ追跡
        var usageHistory = _usageHistoryRepository.GetByEntityId(entityId, entityType);
        
        var oemUsages = usageHistory
            .GroupBy(h => h.OemCode)
            .Select(g => new OemUsageInfo
            {
                OemCode = g.Key,
                UsageCount = g.Count(),
                Vehicles = g.Select(h => h.VehicleName).Distinct().ToList(),
                CustomizationHistory = GetOemCustomizations(entityId, g.Key),
                ApprovalRecords = GetOemApprovals(entityId, g.Key)
            }).ToList();
        
        return new OemTraceabilityResult
        {
            EntityId = entityId,
            EntityType = entityType,
            OemUsages = oemUsages,
            CrossOemDifferences = AnalyzeCrossOemDifferences(oemUsages)
        };
    }
}

// Similar Pattern Search Service（類似パターン検索サービス）
public class SimilarPatternSearchService : DomainService
{
    private readonly ICanSignalRepository _canSignalRepository;
    private readonly ITestEvidenceRepository _testEvidenceRepository;
    private readonly IDetectionResultRepository _detectionResultRepository;
    
    public SimilarSignalSearchResult SearchSimilarSignals(
        CanSignal targetSignal,
        SimilaritySearchCriteria criteria)
    {
        // 類似CAN信号の検索
        var allSignals = _canSignalRepository.GetAllAsync().Result;
        
        var similarSignals = allSignals
            .Where(s => s.Id != targetSignal.Id)
            .Select(s => new
            {
                Signal = s,
                Similarity = CalculateSimilarity(targetSignal, s, criteria)
            })
            .Where(x => x.Similarity >= criteria.MinimumSimilarity)
            .OrderByDescending(x => x.Similarity)
            .Take(criteria.MaxResults)
            .ToList();
        
        // 類似信号の過去検査データを取得
        var similarSignalResults = new List<SimilarSignalInfo>();
        
        foreach (var item in similarSignals)
        {
            var testEvidence = _testEvidenceRepository
                .GetByEntityAsync(item.Signal.Id, "CanSignal")
                .Result;
            
            var detectionResults = _detectionResultRepository
                .GetByCanSignalAsync(item.Signal.Id)
                .Result;
            
            similarSignalResults.Add(new SimilarSignalInfo
            {
                Signal = item.Signal,
                SimilarityScore = item.Similarity,
                TestEvidence = testEvidence,
                DetectionHistory = detectionResults,
                UsageHistory = _usageHistoryRepository
                    .GetByEntityAsync(item.Signal.Id, "CanSignal")
                    .Result
            });
        }
        
        return new SimilarSignalSearchResult
        {
            TargetSignal = targetSignal,
            SimilarSignals = similarSignalResults,
            SearchCriteria = criteria
        };
    }
    
    public ComparisonAnalysisResult CompareTestData(
        Guid targetSignalId,
        List<Guid> compareSignalIds)
    {
        // 検査データの比較分析
        var targetEvidence = _testEvidenceRepository
            .GetByEntityAsync(targetSignalId, "CanSignal")
            .Result;
        
        var comparisons = new List<TestDataComparison>();
        
        foreach (var compareId in compareSignalIds)
        {
            var compareEvidence = _testEvidenceRepository
                .GetByEntityAsync(compareId, "CanSignal")
                .Result;
            
            var comparison = new TestDataComparison
            {
                CompareSignalId = compareId,
                ThresholdDifferences = CompareThresholds(targetEvidence, compareEvidence),
                DetectionConditionDifferences = CompareDetectionConditions(targetEvidence, compareEvidence),
                ResultDifferences = CompareResults(targetEvidence, compareEvidence),
                Recommendations = GenerateRecommendations(targetEvidence, compareEvidence)
            };
            
            comparisons.Add(comparison);
        }
        
        return new ComparisonAnalysisResult
        {
            TargetSignalId = targetSignalId,
            Comparisons = comparisons,
            OverallRecommendations = SynthesizeRecommendations(comparisons)
        };
    }
    
    private double CalculateSimilarity(
        CanSignal signal1,
        CanSignal signal2,
        SimilaritySearchCriteria criteria)
    {
        double similarity = 0.0;
        
        // CAN ID類似度
        if (criteria.CompareCanId && signal1.Identifier.CanId == signal2.Identifier.CanId)
            similarity += 0.3;
        
        // 系統類似度
        if (criteria.CompareSystemType && signal1.SystemType == signal2.SystemType)
            similarity += 0.2;
        
        // 信号名類似度（部分一致）
        if (criteria.CompareSignalName)
        {
            var nameSimilarity = CalculateStringSimilarity(
                signal1.Identifier.SignalName,
                signal2.Identifier.SignalName);
            similarity += nameSimilarity * 0.2;
        }
        
        // 物理値範囲類似度
        if (criteria.CompareValueRange)
        {
            var rangeSimilarity = CalculateRangeSimilarity(
                signal1.Specification.ValueRange,
                signal2.Specification.ValueRange);
            similarity += rangeSimilarity * 0.3;
        }
        
        return similarity;
    }
}
```

### リポジトリインターフェース (Repository Interfaces)

#### 設計原則
- **永続化の抽象化**: ドメイン層は永続化の詳細を知らない
- **集約単位**: 集約ルートごとにリポジトリを定義
- **ドメイン指向**: ドメインの言葉でクエリメソッドを定義

#### 主要リポジトリ

```csharp
// CAN Signal Repository
public interface ICanSignalRepository : IRepository<CanSignal, Guid>
{
    // ドメイン指向のクエリメソッド
    Task<List<CanSignal>> GetBySystemTypeAsync(CanSystemType systemType, CancellationToken cancellationToken = default);
    
    Task<List<CanSignal>> GetStandardSignalsAsync(CancellationToken cancellationToken = default);
    
    Task<CanSignal> FindByIdentifierAsync(string canId, string signalName, CancellationToken cancellationToken = default);
    
    Task<List<CanSignal>> GetByVehiclePhaseAsync(Guid vehiclePhaseId, CancellationToken cancellationToken = default);
    
    Task<bool> IsCanIdInUseAsync(string canId, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

// Anomaly Detection Logic Repository
public interface IAnomalyDetectionLogicRepository : IRepository<AnomalyDetectionLogic, Guid>
{
    Task<List<AnomalyDetectionLogic>> GetByDetectionTypeAsync(DetectionType detectionType, CancellationToken cancellationToken = default);
    
    Task<List<AnomalyDetectionLogic>> GetApprovedLogicsAsync(CancellationToken cancellationToken = default);
    
    Task<List<AnomalyDetectionLogic>> GetByAsilLevelAsync(AsilLevel asilLevel, CancellationToken cancellationToken = default);
    
    Task<List<AnomalyDetectionLogic>> GetByVehiclePhaseAsync(Guid vehiclePhaseId, CancellationToken cancellationToken = default);
    
    Task<List<AnomalyDetectionLogic>> GetInheritedFromAsync(Guid sourceLogicId, CancellationToken cancellationToken = default);
}

// Vehicle Phase Repository
public interface IVehiclePhaseRepository : IRepository<VehiclePhase, Guid>
{
    Task<List<VehiclePhase>> GetCompletedPhasesAsync(CancellationToken cancellationToken = default);
    
    Task<List<VehiclePhase>> GetByOemCodeAsync(string oemCode, CancellationToken cancellationToken = default);
    
    Task<List<VehiclePhase>> GetByPlatformAsync(string platform, CancellationToken cancellationToken = default);
    
    Task<VehiclePhase> FindByModelCodeAsync(string modelCode, string modelYear, CancellationToken cancellationToken = default);
    
    Task<List<VehiclePhase>> GetSimilarPhasesAsync(VehiclePhase phase, CancellationToken cancellationToken = default);
}

// Usage History Repository（利用履歴リポジトリ）
public interface IUsageHistoryRepository : IRepository<UsageHistory, Guid>
{
    Task<List<UsageHistory>> GetByEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    
    Task<List<UsageHistory>> GetByVehicleAsync(string vehicleName, CancellationToken cancellationToken = default);
    
    Task<List<UsageHistory>> GetByOemAsync(string oemCode, CancellationToken cancellationToken = default);
}

// Test Evidence Repository（検査データリポジトリ）
public interface ITestEvidenceRepository : IRepository<TestEvidence, Guid>
{
    Task<List<TestEvidence>> GetByEntityAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
    
    Task<List<TestEvidence>> GetByVehiclePhaseAsync(Guid vehiclePhaseId, CancellationToken cancellationToken = default);
    
    Task<TestEvidence> GetLatestEvidenceAsync(Guid entityId, string entityType, CancellationToken cancellationToken = default);
}

// Detection Result Repository（検出結果リポジトリ）
public interface IDetectionResultRepository : IRepository<DetectionResult, Guid>
{
    Task<List<DetectionResult>> GetByCanSignalAsync(Guid canSignalId, CancellationToken cancellationToken = default);
    
    Task<List<DetectionResult>> GetByCanSignalAndPeriodAsync(Guid canSignalId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<List<DetectionResult>> GetByAnomalyTypeAsync(AnomalyType anomalyType, CancellationToken cancellationToken = default);
    
    Task<List<DetectionResult>> GetByDetectionLogicAsync(Guid logicId, CancellationToken cancellationToken = default);
    
    Task<DetectionStatistics> GetStatisticsAsync(Guid canSignalId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

// OEM Customization Repository（OEMカスタマイズリポジトリ）
public interface IOemCustomizationRepository : IRepository<OemCustomization, Guid>
{
    Task<List<OemCustomization>> GetByEntityAndOemAsync(Guid entityId, string entityType, string oemCode, CancellationToken cancellationToken = default);
    
    Task<List<OemCustomization>> GetByOemAsync(string oemCode, CancellationToken cancellationToken = default);
    
    Task<OemCustomization> GetLatestCustomizationAsync(Guid entityId, string entityType, string oemCode, CancellationToken cancellationToken = default);
}

// OEM Approval Repository（OEM承認リポジトリ）
public interface IOemApprovalRepository : IRepository<OemApproval, Guid>
{
    Task<List<OemApproval>> GetByEntityAndOemAsync(Guid entityId, string entityType, string oemCode, CancellationToken cancellationToken = default);
    
    Task<List<OemApproval>> GetPendingApprovalsAsync(string oemCode, CancellationToken cancellationToken = default);
    
    Task<OemApproval> GetLatestApprovalAsync(Guid entityId, string entityType, string oemCode, CancellationToken cancellationToken = default);
}
```

### ドメインイベント (Domain Events)

#### 設計原則
- **不変性**: イベントは発生した事実、変更不可
- **過去形**: イベント名は過去形で命名
- **疎結合**: イベント発行者と購読者は疎結合

#### 主要ドメインイベント

```csharp
// CAN Signal Events
public class CanSignalCreatedDomainEvent : DomainEvent
{
    public Guid SignalId { get; }
    public string SignalName { get; }
    public string CanId { get; }
    public CanSystemType SystemType { get; }
    public string OemCode { get; }

    public CanSignalCreatedDomainEvent(CanSignal signal)
    {
        SignalId = signal.Id;
        SignalName = signal.Identifier.SignalName;
        CanId = signal.Identifier.CanId;
        SystemType = signal.SystemType;
        OemCode = signal.OemCode.Value;
    }
}

public class CanSignalSpecificationUpdatedDomainEvent : DomainEvent
{
    public Guid SignalId { get; }
    public SignalSpecification OldSpecification { get; }
    public SignalSpecification NewSpecification { get; }
    public string ChangeReason { get; }

    public CanSignalSpecificationUpdatedDomainEvent(
        CanSignal signal,
        SignalSpecification oldSpec,
        SignalSpecification newSpec,
        string changeReason)
    {
        SignalId = signal.Id;
        OldSpecification = oldSpec;
        NewSpecification = newSpec;
        ChangeReason = changeReason;
    }
}

// Detection Logic Events
public class DetectionLogicApprovedDomainEvent : DomainEvent
{
    public Guid LogicId { get; }
    public string LogicName { get; }
    public AsilLevel AsilLevel { get; }
    public Guid ApprovedBy { get; }
    public DateTime ApprovalDate { get; }

    public DetectionLogicApprovedDomainEvent(AnomalyDetectionLogic logic, Guid approvedBy)
    {
        LogicId = logic.Id;
        LogicName = logic.Identity.Name;
        AsilLevel = logic.Safety.AsilLevel;
        ApprovedBy = approvedBy;
        ApprovalDate = DateTime.UtcNow;
    }
}

// Vehicle Phase Events
public class VehiclePhaseCompletedDomainEvent : DomainEvent
{
    public Guid PhaseId { get; }
    public string VehicleName { get; }
    public string ModelCode { get; }
    public DateTime CompletionDate { get; }
    public int DetectionLogicCount { get; }
    public int CanSignalCount { get; }

    public VehiclePhaseCompletedDomainEvent(VehiclePhase phase)
    {
        PhaseId = phase.Id;
        VehicleName = phase.Identity.VehicleName;
        ModelCode = phase.Identity.ModelCode;
        CompletionDate = DateTime.UtcNow;
        DetectionLogicCount = phase.DetectionLogics.Count;
        CanSignalCount = phase.CanSignals.Count;
    }
}

// Inheritance Events
public class PhaseInheritanceExecutedDomainEvent : DomainEvent
{
    public Guid SourcePhaseId { get; }
    public Guid TargetPhaseId { get; }
    public InheritanceType Type { get; }
    public int InheritedItemCount { get; }
    public CompatibilityStatus CompatibilityStatus { get; }

    public PhaseInheritanceExecutedDomainEvent(
        VehiclePhase targetPhase,
        VehiclePhase sourcePhase,
        PhaseInheritanceResult result)
    {
        SourcePhaseId = sourcePhase.Id;
        TargetPhaseId = targetPhase.Id;
        Type = result.Type;
        InheritedItemCount = result.InheritedItems.Count;
        CompatibilityStatus = result.CompatibilityStatus;
    }
}
```

### ファクトリ (Factories)

#### 設計原則
- **複雑な生成ロジック**: エンティティ・集約の複雑な生成をカプセル化
- **不変条件の保証**: 生成時点で有効な状態を保証
- **ドメイン知識**: 生成ルールとビジネス制約の適用

```csharp
// Detection Logic Factory
public class DetectionLogicFactory : IDomainService
{
    public AnomalyDetectionLogic CreateFromTemplate(
        Guid? tenantId,
        DetectionLogicTemplate template,
        VehiclePhase vehiclePhase,
        List<CanSignal> targetSignals)
    {
        // テンプレートから検出ロジックを生成
        var identity = new DetectionLogicIdentity(
            template.Name,
            LogicVersion.Initial(),
            vehiclePhase.Identity.OemCode);

        var specification = new DetectionLogicSpecification(
            template.DetectionType,
            template.SystemType,
            template.Description);

        var safety = new SafetyClassification(
            template.DefaultAsilLevel,
            null);

        var logic = new AnomalyDetectionLogic(tenantId, identity, specification, safety);
        
        // テンプレートパラメータの適用
        foreach (var paramTemplate in template.Parameters)
        {
            var parameter = new DetectionParameter(
                paramTemplate.Name,
                paramTemplate.DataType,
                paramTemplate.DefaultValue,
                paramTemplate.Constraints,
                paramTemplate.Description,
                paramTemplate.IsRequired);
            logic.AddParameter(parameter);
        }
        
        // CAN信号のマッピング
        foreach (var signal in targetSignals)
        {
            var mapping = new CanSignalMapping(logic.Id, signal.Id, signal.Identifier.SignalName, true);
            logic.AddSignalMapping(mapping);
        }
        
        // 設計背景の記録
        logic.SetDesignRationale(new DesignRationale(
            $"Created from template: {template.Name}",
            $"Vehicle: {vehiclePhase.Identity.VehicleName}, Phase: {vehiclePhase.Specification.DevelopmentPhase}",
            DateTime.UtcNow));
        
        return logic;
    }

    public AnomalyDetectionLogic CreateByInheritance(
        Guid? tenantId,
        AnomalyDetectionLogic sourceLogic,
        VehiclePhase targetPhase,
        InheritanceModifications modifications)
    {
        // 継承元ロジックから新規ロジックを生成
        var identity = new DetectionLogicIdentity(
            sourceLogic.Identity.Name,
            LogicVersion.Initial(),
            targetPhase.Identity.OemCode);

        var logic = new AnomalyDetectionLogic(
            tenantId,
            identity,
            sourceLogic.Specification,
            sourceLogic.Safety);

        logic.SetSourceLogic(sourceLogic.Id);
        
        // 継承元の設計背景・検査データを引き継ぐ
        logic.InheritDesignContext(
            sourceLogic.DesignRationale,
            sourceLogic.TestEvidence,
            targetPhase);
        
        // 修正内容の適用
        if (modifications != null)
        {
            ApplyModifications(logic, modifications);
        }
        
        return logic;
    }
}
```


## 3. 新規要件対応のドメインモデル拡張

### 3.1 異常検出詳細分析のドメインモデル

#### Detection Result エンティティ（拡張）

```csharp
// 異常検出結果エンティティ（詳細分析対応）
public class DetectionResult : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 基本情報
    public Guid LogicId { get; private set; }
    public Guid CanSignalId { get; private set; }
    public DateTime ExecutionTime { get; private set; }
    
    // 異常検出詳細情報
    public TimeSpan DetectionDuration { get; private set; } // 検出時間
    public AnomalyType AnomalyType { get; private set; } // 異常種類
    public DetectionCondition Condition { get; private set; } // 検出条件
    public bool IsAnomalyDetected { get; private set; }
    public AnomalyLevel AnomalyLevel { get; private set; }
    public double ConfidenceScore { get; private set; }
    
    // 入力データ
    public Dictionary<string, object> InputData { get; private set; }
    
    // 詳細情報
    public string Details { get; private set; }
    public string DetectionReason { get; private set; }
    
    // 検証情報
    public bool IsValidated { get; private set; }
    public bool IsFalsePositive { get; private set; }
    public string ValidationNotes { get; private set; }
    
    protected DetectionResult() { }
    
    public DetectionResult(
        Guid? tenantId,
        Guid logicId,
        Guid canSignalId,
        DateTime executionTime,
        TimeSpan detectionDuration,
        AnomalyType anomalyType,
        DetectionCondition condition,
        bool isAnomalyDetected,
        AnomalyLevel anomalyLevel,
        double confidenceScore,
        Dictionary<string, object> inputData,
        string details)
    {
        TenantId = tenantId;
        LogicId = logicId;
        CanSignalId = canSignalId;
        ExecutionTime = executionTime;
        DetectionDuration = detectionDuration;
        AnomalyType = anomalyType;
        Condition = condition;
        IsAnomalyDetected = isAnomalyDetected;
        AnomalyLevel = anomalyLevel;
        ConfidenceScore = confidenceScore;
        InputData = inputData;
        Details = details;
        IsValidated = false;
        IsFalsePositive = false;
    }
    
    public void MarkAsFalsePositive(string notes)
    {
        IsFalsePositive = true;
        IsValidated = true;
        ValidationNotes = notes;
    }
    
    public void MarkAsValidAnomaly(string notes)
    {
        IsFalsePositive = false;
        IsValidated = true;
        ValidationNotes = notes;
    }
}

// 異常種類列挙型
public enum AnomalyType
{
    Timeout = 1,           // 通信断・タイムアウト
    OutOfRange = 2,        // 値範囲外
    RateOfChange = 3,      // 変化率異常
    Stuck = 4,             // 固着
    PeriodicAnomaly = 5,   // 周期異常
    SequenceError = 6,     // シーケンスエラー
    Correlation = 7,       // 相関異常
    Other = 99             // その他
}

// 検出条件値オブジェクト
public class DetectionCondition : ValueObject
{
    public string ConditionType { get; private set; }
    public Dictionary<string, object> Parameters { get; private set; }
    public string Description { get; private set; }
    
    protected DetectionCondition() { }
    
    public DetectionCondition(string conditionType, Dictionary<string, object> parameters, string description)
    {
        ConditionType = Check.NotNullOrWhiteSpace(conditionType, nameof(conditionType));
        Parameters = parameters ?? new Dictionary<string, object>();
        Description = description ?? string.Empty;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ConditionType;
        yield return Description;
        foreach (var param in Parameters.OrderBy(p => p.Key))
        {
            yield return param.Key;
            yield return param.Value;
        }
    }
}
```

### 3.2 OEM間トレーサビリティのドメインモデル

#### OEM Customization エンティティ

```csharp
// OEMカスタマイズエンティティ
public class OemCustomization : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 対象エンティティ
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; } // "CanSignal", "DetectionLogic"
    
    // OEM情報
    public OemCode OemCode { get; private set; }
    
    // カスタマイズ内容
    public CustomizationType Type { get; private set; }
    public Dictionary<string, object> CustomParameters { get; private set; }
    public string CustomizationReason { get; private set; }
    
    // 元の値
    public Dictionary<string, object> OriginalParameters { get; private set; }
    
    // 承認情報
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public CustomizationStatus Status { get; private set; }
    
    protected OemCustomization() { }
    
    public OemCustomization(
        Guid? tenantId,
        Guid entityId,
        string entityType,
        OemCode oemCode,
        CustomizationType type,
        Dictionary<string, object> customParameters,
        Dictionary<string, object> originalParameters,
        string customizationReason)
    {
        TenantId = tenantId;
        EntityId = entityId;
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
        Type = type;
        CustomParameters = customParameters ?? new Dictionary<string, object>();
        OriginalParameters = originalParameters ?? new Dictionary<string, object>();
        CustomizationReason = customizationReason;
        Status = CustomizationStatus.Draft;
    }
    
    public void Approve(Guid approvedBy)
    {
        if (Status != CustomizationStatus.PendingApproval)
            throw new BusinessException("Only pending customizations can be approved");
            
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        Status = CustomizationStatus.Approved;
    }
    
    public void SubmitForApproval()
    {
        if (Status != CustomizationStatus.Draft)
            throw new BusinessException("Only draft customizations can be submitted");
            
        Status = CustomizationStatus.PendingApproval;
    }
}

// カスタマイズ種類列挙型
public enum CustomizationType
{
    ParameterAdjustment = 1,  // パラメータ調整
    ThresholdChange = 2,      // 閾値変更
    LogicModification = 3,    // ロジック修正
    SpecificationChange = 4,  // 仕様変更
    Other = 99
}

// カスタマイズ状態列挙型
public enum CustomizationStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Obsolete = 4
}
```

#### OEM Approval エンティティ

```csharp
// OEM承認エンティティ
public class OemApproval : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    
    // 対象エンティティ
    public Guid EntityId { get; private set; }
    public string EntityType { get; private set; }
    
    // OEM情報
    public OemCode OemCode { get; private set; }
    
    // 承認情報
    public ApprovalType Type { get; private set; }
    public Guid RequestedBy { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public ApprovalStatus Status { get; private set; }
    
    // 承認内容
    public string ApprovalReason { get; private set; }
    public string ApprovalNotes { get; private set; }
    public Dictionary<string, object> ApprovalData { get; private set; }
    
    protected OemApproval() { }
    
    public OemApproval(
        Guid? tenantId,
        Guid entityId,
        string entityType,
        OemCode oemCode,
        ApprovalType type,
        Guid requestedBy,
        string approvalReason)
    {
        TenantId = tenantId;
        EntityId = entityId;
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        OemCode = Check.NotNull(oemCode, nameof(oemCode));
        Type = type;
        RequestedBy = requestedBy;
        RequestedAt = DateTime.UtcNow;
        ApprovalReason = approvalReason;
        Status = ApprovalStatus.Pending;
    }
    
    public void Approve(Guid approvedBy, string notes)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("Only pending approvals can be approved");
            
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        Status = ApprovalStatus.Approved;
    }
    
    public void Reject(Guid rejectedBy, string notes)
    {
        if (Status != ApprovalStatus.Pending)
            throw new BusinessException("Only pending approvals can be rejected");
            
        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        Status = ApprovalStatus.Rejected;
    }
}

// 承認種類列挙型
public enum ApprovalType
{
    NewEntity = 1,           // 新規作成
    Modification = 2,        // 修正
    Customization = 3,       // カスタマイズ
    Inheritance = 4,         // 継承
    Sharing = 5,             // 共有
    Deletion = 6             // 削除
}

// 承認状態列挙型
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}
```

### 3.3 類似比較・履歴データ抽出のドメインモデル

#### Similarity Search Criteria 値オブジェクト

```csharp
// 類似度検索条件値オブジェクト
public class SimilaritySearchCriteria : ValueObject
{
    public bool CompareCanId { get; private set; }
    public bool CompareSignalName { get; private set; }
    public bool CompareSystemType { get; private set; }
    public bool CompareValueRange { get; private set; }
    public bool ComparePhysicalUnit { get; private set; }
    public double MinimumSimilarity { get; private set; }
    public int MaxResults { get; private set; }
    
    protected SimilaritySearchCriteria() { }
    
    public SimilaritySearchCriteria(
        bool compareCanId = true,
        bool compareSignalName = true,
        bool compareSystemType = true,
        bool compareValueRange = true,
        bool comparePhysicalUnit = false,
        double minimumSimilarity = 0.5,
        int maxResults = 10)
    {
        CompareCanId = compareCanId;
        CompareSignalName = compareSignalName;
        CompareSystemType = compareSystemType;
        CompareValueRange = compareValueRange;
        ComparePhysicalUnit = comparePhysicalUnit;
        MinimumSimilarity = ValidateMinimumSimilarity(minimumSimilarity);
        MaxResults = ValidateMaxResults(maxResults);
    }
    
    private static double ValidateMinimumSimilarity(double value)
    {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), "Minimum similarity must be between 0.0 and 1.0");
        return value;
    }
    
    private static int ValidateMaxResults(int value)
    {
        if (value < 1 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "Max results must be between 1 and 100");
        return value;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CompareCanId;
        yield return CompareSignalName;
        yield return CompareSystemType;
        yield return CompareValueRange;
        yield return ComparePhysicalUnit;
        yield return MinimumSimilarity;
        yield return MaxResults;
    }
}
```

#### Test Data Comparison 値オブジェクト

```csharp
// 検査データ比較値オブジェクト
public class TestDataComparison : ValueObject
{
    public Guid CompareSignalId { get; private set; }
    public Dictionary<string, ThresholdDifference> ThresholdDifferences { get; private set; }
    public List<string> DetectionConditionDifferences { get; private set; }
    public Dictionary<string, object> ResultDifferences { get; private set; }
    public List<string> Recommendations { get; private set; }
    
    protected TestDataComparison() { }
    
    public TestDataComparison(
        Guid compareSignalId,
        Dictionary<string, ThresholdDifference> thresholdDifferences,
        List<string> detectionConditionDifferences,
        Dictionary<string, object> resultDifferences,
        List<string> recommendations)
    {
        CompareSignalId = compareSignalId;
        ThresholdDifferences = thresholdDifferences ?? new Dictionary<string, ThresholdDifference>();
        DetectionConditionDifferences = detectionConditionDifferences ?? new List<string>();
        ResultDifferences = resultDifferences ?? new Dictionary<string, object>();
        Recommendations = recommendations ?? new List<string>();
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CompareSignalId;
        foreach (var diff in ThresholdDifferences.OrderBy(d => d.Key))
        {
            yield return diff.Key;
            yield return diff.Value;
        }
    }
}

// 閾値差異値オブジェクト
public class ThresholdDifference : ValueObject
{
    public string ParameterName { get; private set; }
    public object TargetValue { get; private set; }
    public object CompareValue { get; private set; }
    public double DifferencePercentage { get; private set; }
    public string DifferenceDescription { get; private set; }
    
    protected ThresholdDifference() { }
    
    public ThresholdDifference(
        string parameterName,
        object targetValue,
        object compareValue,
        double differencePercentage,
        string differenceDescription)
    {
        ParameterName = Check.NotNullOrWhiteSpace(parameterName, nameof(parameterName));
        TargetValue = targetValue;
        CompareValue = compareValue;
        DifferencePercentage = differencePercentage;
        DifferenceDescription = differenceDescription ?? string.Empty;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ParameterName;
        yield return TargetValue?.ToString() ?? string.Empty;
        yield return CompareValue?.ToString() ?? string.Empty;
        yield return DifferencePercentage;
    }
}
```

## 4. アプリケーション層の設計

### 4.1 異常検出分析アプリケーションサービス

```csharp
public class AnomalyAnalysisAppService : ApplicationService
{
    private readonly AnomalyAnalysisService _analysisService;
    private readonly IDetectionResultRepository _resultRepository;
    
    public async Task<AnomalyPatternAnalysisDto> AnalyzeAnomalyPatternAsync(
        Guid canSignalId,
        DateTime startDate,
        DateTime endDate)
    {
        // ドメインサービスで分析実行
        var result = _analysisService.AnalyzePattern(canSignalId, startDate, endDate);
        
        // DTOに変換
        return ObjectMapper.Map<AnomalyPatternAnalysisResult, AnomalyPatternAnalysisDto>(result);
    }
    
    public async Task<List<ThresholdRecommendationDto>> GetThresholdRecommendationsAsync(
        Guid canSignalId)
    {
        var results = await _resultRepository.GetByCanSignalAsync(canSignalId);
        var recommendations = _analysisService.GenerateThresholdRecommendations(results);
        
        return ObjectMapper.Map<List<ThresholdRecommendation>, List<ThresholdRecommendationDto>>(recommendations);
    }
}
```

### 4.2 OEMトレーサビリティアプリケーションサービス

```csharp
public class OemTraceabilityAppService : ApplicationService
{
    private readonly TraceabilityQueryService _traceabilityService;
    private readonly IOemCustomizationRepository _customizationRepository;
    private readonly IOemApprovalRepository _approvalRepository;
    
    public async Task<OemTraceabilityDto> GetOemTraceabilityAsync(
        Guid entityId,
        string entityType)
    {
        // OEM間トレーサビリティ取得
        var result = _traceabilityService.TraceAcrossOems(entityId, entityType);
        
        return ObjectMapper.Map<OemTraceabilityResult, OemTraceabilityDto>(result);
    }
    
    public async Task<Guid> CreateOemCustomizationAsync(
        CreateOemCustomizationDto input)
    {
        // OEMカスタマイズ作成
        var customization = new OemCustomization(
            CurrentTenant.Id,
            input.EntityId,
            input.EntityType,
            new OemCode(input.OemCode),
            input.Type,
            input.CustomParameters,
            input.OriginalParameters,
            input.CustomizationReason);
        
        await _customizationRepository.InsertAsync(customization);
        
        return customization.Id;
    }
}
```

### 4.3 類似パターン検索アプリケーションサービス

```csharp
public class SimilarPatternSearchAppService : ApplicationService
{
    private readonly SimilarPatternSearchService _searchService;
    private readonly ICanSignalRepository _canSignalRepository;
    
    public async Task<SimilarSignalSearchResultDto> SearchSimilarSignalsAsync(
        Guid targetSignalId,
        SimilaritySearchCriteriaDto criteriaDto)
    {
        // 対象信号取得
        var targetSignal = await _canSignalRepository.GetAsync(targetSignalId);
        
        // 検索条件変換
        var criteria = ObjectMapper.Map<SimilaritySearchCriteriaDto, SimilaritySearchCriteria>(criteriaDto);
        
        // 類似信号検索
        var result = _searchService.SearchSimilarSignals(targetSignal, criteria);
        
        return ObjectMapper.Map<SimilarSignalSearchResult, SimilarSignalSearchResultDto>(result);
    }
    
    public async Task<ComparisonAnalysisResultDto> CompareTestDataAsync(
        Guid targetSignalId,
        List<Guid> compareSignalIds)
    {
        // 検査データ比較
        var result = _searchService.CompareTestData(targetSignalId, compareSignalIds);
        
        return ObjectMapper.Map<ComparisonAnalysisResult, ComparisonAnalysisResultDto>(result);
    }
}
```

## 5. まとめ

新規要件に対応するため、以下のドメインモデルとサービスを追加しました：

### 5.1 異常検出詳細分析
- **DetectionResult エンティティ拡張**: 検出時間、異常種類、検出条件の詳細記録
- **AnomalyAnalysisService**: 異常パターン分析、閾値最適化推奨
- **IDetectionResultRepository**: 検出結果の統計・分析クエリ

### 5.2 OEM間トレーサビリティ
- **OemCustomization エンティティ**: OEM固有のカスタマイズ管理
- **OemApproval エンティティ**: OEM固有の承認ワークフロー
- **TraceabilityQueryService拡張**: OEM間トレーサビリティ追跡

### 5.3 類似比較・履歴データ抽出
- **SimilarPatternSearchService**: 類似CAN信号検索、検査データ比較
- **SimilaritySearchCriteria**: 類似度検索条件
- **TestDataComparison**: 検査データ比較結果

これらの拡張により、以下が実現されます：
1. 特定CAN信号の異常検出詳細分析（検出時間、異常種類、統計）
2. OEM間でのトレーサビリティ管理（カスタマイズ、承認、差異分析）
3. 類似CAN信号の過去検査データ検索・比較分析
