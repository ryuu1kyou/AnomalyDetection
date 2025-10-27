# CAN異常検出管理システム - 設計書

## Overview

本設計書は、CAN異常検出管理システムのドメインモデル、データベーススキーマ、およびエンティティ関係を定義します。ABP vNextフレームワークのDDD（Domain-Driven Design）原則に従い、Aggregate Root、Entity、Value Objectの明確な区別を行います。

## Architecture

### レイヤー構造

```
Presentation Layer (Angular)
    ↓
Application Layer (Application Services, DTOs)
    ↓
Domain Layer (Entities, Value Objects, Domain Services)
    ↓
Infrastructure Layer (EF Core, Repositories)
    ↓
Database (SQL Server)
```

### 主要な設計原則

1. **Aggregate Root**: 関連するエンティティ群の整合性を保証する親エンティティ
2. **Entity**: 一意の識別子を持ち、`Entity<TKey>`を継承
3. **Value Object**: 識別子を持たず、`ValueObject`を継承
4. **Repository Pattern**: Aggregate Rootに対してのみリポジトリを提供

## Components and Interfaces

### 1. マルチテナント管理

#### 1.1 OemMaster (Aggregate Root)

**目的**: OEM組織のマスターデータを管理

**継承**: `FullAuditedAggregateRoot<Guid>`

**Value Objects**:
- `OemCode`: OEMコードと名称（Code, Name）

**Properties**:
- `CompanyName`: 会社名
- `Country`: 国
- `ContactEmail`: 連絡先メール
- `ContactPhone`: 連絡先電話
- `Description`: 説明
- `IsActive`: アクティブフラグ

**Owned Collections**:
- `Features`: OEM固有の機能設定（`OemFeature`のコレクション）
  - `FeatureName`: 機能名
  - `FeatureValue`: 機能値

**Table**: `App_OemMasters`

**Child Tables**:
- `App_OemFeatures`: OEM機能設定（Owned Collection）

#### 1.2 ExtendedTenant (Aggregate Root)

**目的**: テナント（OEM組織）の拡張情報を管理

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `OemCode`: OEMコードと名称

**Properties**:
- `Name`: テナント名
- `OemMasterId`: OEMマスターID（外部キー、nullable）
- `DatabaseConnectionString`: データベース接続文字列
- `IsActive`: アクティブフラグ
- `ExpirationDate`: 有効期限
- `Description`: 説明

**Owned Collections**:
- `Features`: テナント固有の機能設定（`TenantFeature`のコレクション）
  - `FeatureName`: 機能名
  - `FeatureValue`: 機能値
  - `CreatedBy`: 作成者
  - `UpdatedBy`: 更新者
  - `CreatedAt`: 作成日時
  - `UpdatedAt`: 更新日時

**Table**: `App_ExtendedTenants`

**Child Tables**:
- `App_TenantFeatures`: テナント機能設定（Owned Collection）

**Relationships**:
- `OemMaster` (Many-to-One, nullable, OnDelete: SetNull)

### 2. CAN信号管理

#### 2.1 CanSignal (Aggregate Root)

**目的**: CAN信号の定義と仕様を管理

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `SignalIdentifier`: 信号識別子（SignalName, CanId）
- `SignalSpecification`: 信号仕様（StartBit, Length, DataType, ByteOrder, ValueRange）
  - `ValueRange`: 値範囲（MinValue, MaxValue）
- `PhysicalValueConversion`: 物理値変換（Factor, Offset, Unit）
- `SignalTiming`: タイミング情報（CycleTimeMs, TimeoutMs, SendType）
- `OemCode`: OEMコード
- `SignalVersion`: バージョン（Major, Minor）

**Properties**:
- `SystemType`: システムタイプ（enum）
- `Description`: 説明
- `Status`: ステータス（enum）
- `SourceDocument`: ソースドキュメント
- `Notes`: 備考

**Table**: `App_CanSignals`

**Indexes**:
- `SystemType`
- `Status`

#### 2.2 CanSystemCategory (Aggregate Root)

**目的**: CANシステムカテゴリの定義と設定を管理

**継承**: `FullAuditedAggregateRoot<Guid>`

**Value Objects**:
- `SystemCategoryConfiguration`: カテゴリ設定（Priority, IsSafetyRelevant, RequiresRealTimeMonitoring, DefaultTimeoutMs, MaxSignalsPerCategory, CustomSettings）

**Properties**:
- `SystemType`: システムタイプ（enum, unique）
- `Name`: カテゴリ名
- `Description`: 説明
- `Icon`: アイコン
- `Color`: 色
- `DisplayOrder`: 表示順
- `IsActive`: アクティブフラグ

**Table**: `App_CanSystemCategories`

**Indexes**:
- `SystemType` (unique)
- `DisplayOrder`
- `IsActive`

### 3. 異常検出ロジック管理

#### 3.1 CanAnomalyDetectionLogic (Aggregate Root)

**目的**: 異常検出ロジックの定義と実装を管理

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `DetectionLogicIdentity`: ロジック識別子（Name, Version, OemCode）
  - `DetectionLogicVersion`: バージョン（Major, Minor, Patch）
  - `OemCode`: OEMコード
- `DetectionLogicSpecification`: ロジック仕様（DetectionType, Description, TargetSystemType, Complexity, Requirements）
- `LogicImplementation`: 実装情報（Type, Content, Language, EntryPoint, CreatedAt, CreatedBy）
- `SafetyClassification`: 安全分類（AsilLevel, SafetyRequirementId, SafetyGoalId, HazardAnalysisId）

**Properties**:
- `Status`: ステータス（enum）
- `SharingLevel`: 共有レベル（enum）
- `SourceLogicId`: ソースロジックID（nullable）
- `ApprovalNotes`: 承認メモ

**Child Entities** (1対多の関係):
- `Parameters`: 検出パラメータ（`DetectionParameter`エンティティのコレクション）
- `SignalMappings`: 信号マッピング（`CanSignalMapping`エンティティのコレクション）

**Table**: `App_CanAnomalyDetectionLogics`

**Child Tables**:
- `App_DetectionParameters`: 検出パラメータ（Entity）
- `App_CanSignalMappings`: CAN信号マッピング（Entity）

**Indexes**:
- `Identity_Name`, `TenantId`
- `Status`
- `SharingLevel`
- `Safety_AsilLevel`

**Relationships**:
- `DetectionParameter` (One-to-Many, OnDelete: Cascade)
- `CanSignalMapping` (One-to-Many, OnDelete: Cascade)

#### 3.2 DetectionParameter (Entity)

**目的**: 検出ロジックのパラメータを管理

**継承**: `Entity<Guid>`

**Value Objects**:
- `ParameterConstraints`: パラメータ制約（MinValue, MaxValue, MinLength, MaxLength, Pattern, AllowedValues）

**Properties**:
- `Name`: パラメータ名
- `DataType`: データ型（enum）
- `Value`: 値
- `DefaultValue`: デフォルト値
- `Description`: 説明
- `IsRequired`: 必須フラグ
- `Unit`: 単位
- `CreatedAt`: 作成日時
- `UpdatedAt`: 更新日時

**Foreign Keys**:
- `DetectionLogicId`: 検出ロジックID（親）

**Table**: `App_DetectionParameters`

**Storage Strategy**:
- `Constraints`: JSON列として保存

**Indexes**:
- `DetectionLogicId`

#### 3.3 CanSignalMapping (Entity)

**目的**: 検出ロジックとCAN信号のマッピングを管理

**継承**: `Entity` (複合キー: DetectionLogicId + CanSignalId)

**Value Objects**:
- `SignalMappingConfiguration`: マッピング設定（ScalingFactor, Offset, FilterExpression, CustomProperties）

**Properties**:
- `CanSignalId`: CAN信号ID
- `SignalRole`: 信号の役割
- `IsRequired`: 必須フラグ
- `Description`: 説明
- `CreatedAt`: 作成日時
- `UpdatedAt`: 更新日時

**Foreign Keys**:
- `DetectionLogicId`: 検出ロジックID（親）

**Table**: `App_CanSignalMappings`

**Storage Strategy**:
- `Configuration`: JSON列として保存

**Indexes**:
- `DetectionLogicId`
- `CanSignalId`

### 4. 異常検出結果管理

#### 4.1 AnomalyDetectionResult (Aggregate Root)

**目的**: 異常検出の実行結果を記録

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `DetectionInputData`: 入力データ（SignalValue, Timestamp, AdditionalData）
- `DetectionDetails`: 検出詳細（DetectionType, TriggerCondition, ExecutionTimeMs, Parameters）

**Properties**:
- `DetectionLogicId`: 検出ロジックID（外部キー）
- `CanSignalId`: CAN信号ID（外部キー）
- `DetectedAt`: 検出日時
- `AnomalyLevel`: 異常レベル（enum）
- `ConfidenceScore`: 信頼度スコア
- `Description`: 説明
- `ResolutionStatus`: 解決ステータス（enum）
- `ResolutionNotes`: 解決メモ
- `ResolvedAt`: 解決日時
- `ResolvedBy`: 解決者
- `SharingLevel`: 共有レベル（enum）

**Table**: `App_AnomalyDetectionResults`

**Indexes**:
- `DetectedAt`
- `AnomalyLevel`
- `ResolutionStatus`
- `DetectionLogicId`
- `CanSignalId`

**Relationships**:
- `CanAnomalyDetectionLogic` (Many-to-One, OnDelete: Restrict)
- `CanSignal` (Many-to-One, OnDelete: Restrict)

### 5. プロジェクト管理

#### 5.1 AnomalyDetectionProject (Aggregate Root)

**目的**: 異常検出プロジェクトを管理

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `OemCode`: OEMコード
- `ProjectConfiguration`: プロジェクト設定（Priority, IsConfidential, Tags, CustomSettings, Notes, NotificationSettings）
  - `NotificationSettings`: 通知設定（EnableMilestoneNotifications, EnableProgressNotifications, EnableOverdueNotifications, NotificationFrequencyHours, NotificationChannels）

**Properties**:
- `ProjectCode`: プロジェクトコード（unique）
- `Name`: プロジェクト名
- `Description`: 説明
- `Status`: ステータス（enum）
- `VehicleModel`: 車両モデル
- `ModelYear`: モデル年
- `PrimarySystem`: 主要システム（enum）
- `StartDate`: 開始日
- `EndDate`: 終了日
- `ProjectManagerId`: プロジェクトマネージャーID

**Child Entities** (1対多の関係):
- `Milestones`: マイルストーン（`ProjectMilestone`エンティティのコレクション）
- `Members`: メンバー（`ProjectMember`エンティティのコレクション）

**Table**: `App_AnomalyDetectionProjects`

**Child Tables**:
- `App_ProjectMilestones`: プロジェクトマイルストーン（Entity）
- `App_ProjectMembers`: プロジェクトメンバー（Entity）

**Indexes**:
- `ProjectCode` (unique)
- `Status`
- `StartDate`
- `EndDate`
- `OemCode_Code`

**Relationships**:
- `ProjectMilestone` (One-to-Many, OnDelete: Cascade)
- `ProjectMember` (One-to-Many, OnDelete: Cascade)

#### 5.2 ProjectMilestone (Entity)

**目的**: プロジェクトのマイルストーンを管理

**継承**: `Entity` (複合キー: ProjectId + Name)

**Value Objects**:
- `MilestoneConfiguration`: マイルストーン設定（IsCritical, RequiresApproval, Dependencies, CustomProperties）

**Properties**:
- `Name`: マイルストーン名
- `Description`: 説明
- `DueDate`: 期限
- `Status`: ステータス（enum）
- `CompletedDate`: 完了日
- `CompletedBy`: 完了者
- `DisplayOrder`: 表示順
- `CreatedAt`: 作成日時
- `UpdatedAt`: 更新日時

**Foreign Keys**:
- `ProjectId`: プロジェクトID（親）

**Table**: `App_ProjectMilestones`

**Storage Strategy**:
- `Configuration`: JSON列として保存

**Indexes**:
- `ProjectId`

#### 5.3 ProjectMember (Entity)

**目的**: プロジェクトのメンバーを管理

**継承**: `Entity` (複合キー: ProjectId + UserId)

**Value Objects**:
- `MemberConfiguration`: メンバー設定（Permissions, Settings, CanReceiveNotifications, CanAccessReports）

**Properties**:
- `UserId`: ユーザーID
- `Role`: 役割（enum）
- `JoinedDate`: 参加日
- `LeftDate`: 退出日
- `IsActive`: アクティブフラグ
- `Notes`: 備考

**Foreign Keys**:
- `ProjectId`: プロジェクトID（親）

**Table**: `App_ProjectMembers`

**Storage Strategy**:
- `Configuration`: JSON列として保存

**Indexes**:
- `ProjectId`
- `UserId`

## Data Models

### Entity Framework Core設定の原則

1. **Aggregate Root**: `DbSet<T>`として公開し、独自のテーブルを持つ
2. **Entity（子エンティティ）**: `DbSet<T>`として公開し、独自のテーブルを持つ。親エンティティとの関係は`HasMany().WithOne().HasForeignKey()`で設定
3. **Value Object**: `OwnsOne()`または`OwnsMany()`で設定。独自のテーブルを持たず、親エンティティのテーブルに列として保存されるか、別テーブルに保存される
4. **Owned Collection（Value Object）**: `OwnsMany()`で設定。別テーブルに保存されるが、親エンティティの一部として扱われる

### テーブル一覧

| テーブル名 | エンティティタイプ | 説明 |
|-----------|------------------|------|
| App_OemMasters | Aggregate Root | OEMマスター |
| App_OemFeatures | Owned Collection | OEM機能設定 |
| App_ExtendedTenants | Aggregate Root | 拡張テナント |
| App_TenantFeatures | Owned Collection | テナント機能設定 |
| App_CanSignals | Aggregate Root | CAN信号 |
| App_CanSystemCategories | Aggregate Root | CANシステムカテゴリ |
| App_CanAnomalyDetectionLogics | Aggregate Root | 異常検出ロジック |
| App_DetectionParameters | Entity | 検出パラメータ |
| App_CanSignalMappings | Entity | CAN信号マッピング |
| App_AnomalyDetectionResults | Aggregate Root | 異常検出結果 |
| App_AnomalyDetectionProjects | Aggregate Root | 異常検出プロジェクト |
| App_ProjectMilestones | Entity | プロジェクトマイルストーン |
| App_ProjectMembers | Entity | プロジェクトメンバー |

### Value Objectの保存戦略

| Value Object | 保存方法 | 説明 |
|-------------|---------|------|
| OemCode | OwnsOne（列） | 親テーブルに列として保存 |
| SignalIdentifier | OwnsOne（列） | 親テーブルに列として保存 |
| SignalSpecification | OwnsOne（列） | 親テーブルに列として保存 |
| ValueRange | OwnsOne（ネスト列） | SignalSpecification内にネストして保存 |
| PhysicalValueConversion | OwnsOne（列） | 親テーブルに列として保存 |
| SignalTiming | OwnsOne（列） | 親テーブルに列として保存 |
| SignalVersion | OwnsOne（列） | 親テーブルに列として保存 |
| SystemCategoryConfiguration | OwnsOne（列） | 親テーブルに列として保存 |
| DetectionLogicIdentity | OwnsOne（列） | 親テーブルに列として保存 |
| DetectionLogicVersion | OwnsOne（ネスト列） | DetectionLogicIdentity内にネストして保存 |
| DetectionLogicSpecification | OwnsOne（列） | 親テーブルに列として保存 |
| LogicImplementation | OwnsOne（列） | 親テーブルに列として保存 |
| SafetyClassification | OwnsOne（列） | 親テーブルに列として保存 |
| ParameterConstraints | JSON列 | JSON形式で保存 |
| SignalMappingConfiguration | JSON列 | JSON形式で保存 |
| DetectionInputData | OwnsOne（列） | 親テーブルに列として保存 |
| DetectionDetails | OwnsOne（列） | 親テーブルに列として保存 |
| ProjectConfiguration | OwnsOne（列） | 親テーブルに列として保存 |
| NotificationSettings | OwnsOne（ネスト列） | ProjectConfiguration内にネストして保存 |
| MilestoneConfiguration | JSON列 | JSON形式で保存 |
| MemberConfiguration | JSON列 | JSON形式で保存 |
| OemFeature | OwnsMany（別テーブル） | App_OemFeaturesテーブル |
| TenantFeature | OwnsMany（別テーブル） | App_TenantFeaturesテーブル |

## Error Handling

### バリデーションエラー

- ドメインエンティティのコンストラクタおよびメソッドで入力値を検証
- 不正な値の場合は`ArgumentException`または`ArgumentNullException`をスロー
- Application Layerでビジネスルール違反を検証し、`BusinessException`をスローする

### データ整合性エラー

- 外部キー制約違反
- 一意制約違反
- マルチテナント分離違反

## Testing Strategy

### ユニットテスト

**対象**:
- ドメインエンティティのビジネスロジック
- Value Objectの等価性と不変性
- ドメインサービスのロジック
- Application Serviceのビジネスロジック

**ツール**:
- xUnit
- Shouldly (Assertion Library)
- NSubstitute (Mocking Framework)

**カバレッジ目標**: 80%以上

### 統合テスト

**対象**:
- リポジトリの CRUD 操作
- データベーストランザクション
- マルチテナントフィルタリング
- Application Service全体のフロー

**ツール**:
- xUnit
- ABP Test Infrastructure
- In-Memory Database (SQLite)

### E2Eテスト

**対象**:
- ユーザーシナリオベースのテスト
- API エンドポイントのテスト
- UI フローのテスト

**ツール**:
- Playwright (UI Testing)
- Postman/Newman (API Testing)



### 6. OEMトレーサビリティ管理

#### 6.1 OemCustomization (Aggregate Root)

**目的**: OEM固有のカスタマイズ情報を管理

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `OemCode`: OEMコード

**Properties**:
- `EntityId`: 対象エンティティID
- `EntityType`: 対象エンティティ種類
- `Type`: カスタマイズ種類（enum）
- `CustomParameters`: カスタマイズパラメータ（Dictionary）
- `OriginalParameters`: 元のパラメータ（Dictionary）
- `CustomizationReason`: カスタマイズ理由
- `ApprovedBy`: 承認者ID
- `ApprovedAt`: 承認日時
- `Status`: カスタマイズステータス（enum）
- `ApprovalNotes`: 承認メモ

**Table**: `App_OemCustomizations`

**Indexes**:
- `EntityType`
- `EntityId`
- `OemCode_Code`
- `Status`

**Business Methods**:
- `SubmitForApproval()`: 承認申請
- `Approve()`: 承認
- `Reject()`: 却下
- `UpdateCustomParameters()`: パラメータ更新
- `MarkAsObsolete()`: 廃止

#### 6.2 OemApproval (Aggregate Root)

**目的**: OEM承認ワークフローを管理

**継承**: `FullAuditedAggregateRoot<Guid>`, `IMultiTenant`

**Value Objects**:
- `OemCode`: OEMコード

**Properties**:
- `EntityId`: 対象エンティティID
- `EntityType`: 対象エンティティ種類
- `Type`: 承認種類（enum）
- `RequestedBy`: 申請者ID
- `RequestedAt`: 申請日時
- `ApprovedBy`: 承認者ID
- `ApprovedAt`: 承認日時
- `Status`: 承認ステータス（enum）
- `ApprovalReason`: 承認理由
- `ApprovalNotes`: 承認メモ
- `ApprovalData`: 承認データ（Dictionary）
- `DueDate`: 承認期限
- `Priority`: 優先度（1-4）

**Table**: `App_OemApprovals`

**Indexes**:
- `EntityType`
- `EntityId`
- `OemCode_Code`
- `Status`
- `RequestedAt`

**Business Methods**:
- `Approve()`: 承認
- `Reject()`: 却下
- `Cancel()`: キャンセル
- `UpdateDueDate()`: 期限更新
- `UpdatePriority()`: 優先度更新
- `IsOverdue()`: 期限切れ判定
- `IsUrgent()`: 緊急判定

### 7. 類似パターン検索

#### 7.1 SimilarPatternSearchService (Domain Service)

**目的**: 類似CAN信号と検出パターンを検索

**Interface**: `ISimilarPatternSearchService`

**Methods**:
- `SearchSimilarSignalsAsync()`: 類似信号検索
- `CompareTestDataAsync()`: テストデータ比較
- `CalculateSimilarity()`: 類似度計算
- `CalculateSimilarityBreakdown()`: 類似度詳細計算
- `DetermineRecommendationLevel()`: 推奨レベル決定

**Value Objects**:
- `SimilaritySearchCriteria`: 検索条件
- `SimilarSignalResult`: 類似信号結果
- `SimilarityBreakdown`: 類似度内訳
- `TestDataComparison`: テストデータ比較結果
- `AttributeDifference`: 属性差異
- `ThresholdDifference`: 閾値差異
- `DetectionConditionDifference`: 検出条件差異
- `ResultDifference`: 結果差異
- `ComparisonRecommendation`: 比較推奨

**Enums**:
- `RecommendationLevel`: 推奨レベル（Highly, High, Medium, Low, NotRecommended）
- `RecommendationType`: 推奨種類（ThresholdAdjustment, ConditionChange, ParameterTuning）
- `RecommendationPriority`: 推奨優先度（Low, Medium, High, Critical）
- `DifferenceType`: 差異種類（ValueDifference, MissingSetting, AdditionalSetting）
- `ImpactLevel`: 影響レベル（Low, Medium, High, Critical）

### 8. 異常分析サービス

#### 8.1 AnomalyAnalysisService (Domain Service)

**目的**: 異常検出パターンと精度を分析

**Interface**: `IAnomalyAnalysisService`

**Methods**:
- `AnalyzePatternAsync()`: パターン分析
- `GenerateThresholdRecommendationsAsync()`: 閾値推奨生成
- `CalculateDetectionAccuracyAsync()`: 検出精度計算

**Value Objects**:
- `AnomalyPatternAnalysisResult`: パターン分析結果
- `ThresholdRecommendationResult`: 閾値推奨結果
- `DetectionAccuracyMetrics`: 検出精度メトリクス
- `OptimizationMetrics`: 最適化メトリクス
- `ThresholdRecommendation`: 閾値推奨
- `AnomalyFrequencyPattern`: 異常頻度パターン
- `AnomalyCorrelation`: 異常相関
- `AccuracyByAnomalyType`: 異常タイプ別精度
- `AccuracyByTimeRange`: 時間範囲別精度

## API Design

### RESTful API エンドポイント

#### CAN Signal Management
- `GET /api/app/can-signal` - CAN信号一覧取得
- `GET /api/app/can-signal/{id}` - CAN信号詳細取得
- `POST /api/app/can-signal` - CAN信号作成
- `PUT /api/app/can-signal/{id}` - CAN信号更新
- `DELETE /api/app/can-signal/{id}` - CAN信号削除

#### Detection Logic Management
- `GET /api/app/can-anomaly-detection-logic` - 検出ロジック一覧取得
- `GET /api/app/can-anomaly-detection-logic/{id}` - 検出ロジック詳細取得
- `POST /api/app/can-anomaly-detection-logic` - 検出ロジック作成
- `PUT /api/app/can-anomaly-detection-logic/{id}` - 検出ロジック更新
- `DELETE /api/app/can-anomaly-detection-logic/{id}` - 検出ロジック削除
- `POST /api/app/can-anomaly-detection-logic/{id}/execute` - 検出ロジック実行

#### Project Management
- `GET /api/app/anomaly-detection-project` - プロジェクト一覧取得
- `GET /api/app/anomaly-detection-project/{id}` - プロジェクト詳細取得
- `POST /api/app/anomaly-detection-project` - プロジェクト作成
- `PUT /api/app/anomaly-detection-project/{id}` - プロジェクト更新
- `DELETE /api/app/anomaly-detection-project/{id}` - プロジェクト削除

#### OEM Traceability
- `GET /api/app/oem-traceability/customizations` - カスタマイズ一覧取得
- `POST /api/app/oem-traceability/customizations` - カスタマイズ作成
- `PUT /api/app/oem-traceability/customizations/{id}/submit` - 承認申請
- `PUT /api/app/oem-traceability/customizations/{id}/approve` - カスタマイズ承認
- `PUT /api/app/oem-traceability/customizations/{id}/reject` - カスタマイズ却下
- `GET /api/app/oem-traceability/approvals` - 承認一覧取得
- `PUT /api/app/oem-traceability/approvals/{id}/approve` - 承認
- `PUT /api/app/oem-traceability/approvals/{id}/reject` - 却下
- `GET /api/app/oem-traceability/trace/{entityType}/{entityId}` - トレーサビリティ取得

#### Similar Pattern Search
- `POST /api/app/similar-pattern-search/search-signals` - 類似信号検索
- `POST /api/app/similar-pattern-search/compare-test-data` - テストデータ比較
- `POST /api/app/similar-pattern-search/calculate-similarity` - 類似度計算

#### Anomaly Analysis
- `POST /api/app/anomaly-analysis/analyze-pattern` - パターン分析
- `POST /api/app/anomaly-analysis/threshold-recommendations` - 閾値推奨
- `POST /api/app/anomaly-analysis/detection-accuracy` - 検出精度計算

## Security Design

### 認証・認可

#### 認証方式
- **OAuth 2.0 / OpenID Connect**: 外部IDプロバイダー連携
- **JWT Token**: APIアクセス用トークン
- **Session Management**: Webアプリケーション用セッション

#### 権限管理
- **Role-Based Access Control (RBAC)**: ロールベースのアクセス制御
- **Permission-Based Authorization**: 細かい権限制御

#### 権限定義

**Roles**:
- `Admin`: システム管理者
- `OemAdmin`: OEM管理者
- `ProjectManager`: プロジェクトマネージャー
- `DetectionEngineer`: 検出エンジニア
- `SignalEngineer`: 信号エンジニア
- `TestEngineer`: テストエンジニア
- `Viewer`: 閲覧者

**Permissions**:
- `AnomalyDetection.CanSignals`: CAN信号管理
  - `Create`, `Edit`, `Delete`, `View`
- `AnomalyDetection.DetectionLogics`: 検出ロジック管理
  - `Create`, `Edit`, `Delete`, `View`, `Execute`
- `AnomalyDetection.Projects`: プロジェクト管理
  - `Create`, `Edit`, `Delete`, `View`, `ManageMembers`
- `AnomalyDetection.OemTraceability`: OEMトレーサビリティ
  - `CreateCustomization`, `ApproveCustomization`, `ViewTraceability`
- `AnomalyDetection.Analysis`: 分析機能
  - `AnalyzePatterns`, `GenerateRecommendations`, `ViewMetrics`

### データ保護

#### 暗号化
- **転送時**: TLS 1.2以上
- **保存時**: AES-256暗号化（機密フィールド）
- **バックアップ**: 暗号化バックアップ

#### マルチテナント分離
- **データベースレベル**: TenantIdによるフィルタリング
- **アプリケーションレベル**: ABP Frameworkのマルチテナント機能
- **監査ログ**: 全データアクセスの記録

## Performance Optimization

### データベース最適化

#### インデックス戦略
- 頻繁に検索される列にインデックス作成
- 複合インデックスの活用
- カバリングインデックスの検討

#### クエリ最適化
- N+1問題の回避（Eager Loading）
- ページネーションの実装
- 非同期クエリの活用

#### キャッシング
- **分散キャッシュ**: Redis使用
- **キャッシュ対象**: マスターデータ、頻繁にアクセスされる設定
- **キャッシュ戦略**: Cache-Aside Pattern

### アプリケーション最適化

#### 非同期処理
- バックグラウンドジョブ（Hangfire/Quartz.NET）
- メッセージキュー（RabbitMQ/Azure Service Bus）

#### リソース管理
- コネクションプーリング
- オブジェクトプーリング
- メモリ管理の最適化

## Deployment Architecture

### 環境構成

#### Development
- ローカル開発環境
- Docker Compose

#### Staging
- Azure App Service / AWS ECS
- Azure SQL Database / AWS RDS
- Redis Cache

#### Production
- Azure App Service (複数インスタンス) / AWS ECS
- Azure SQL Database (高可用性構成) / AWS RDS Multi-AZ
- Redis Cache (クラスター構成)
- Azure Application Insights / AWS CloudWatch

### CI/CD Pipeline

#### ビルドパイプライン
1. ソースコードチェックアウト
2. 依存関係の復元
3. コンパイル
4. ユニットテスト実行
5. コードカバレッジ計測
6. 静的コード分析
7. Dockerイメージビルド
8. コンテナレジストリへプッシュ

#### デプロイパイプライン
1. 環境変数設定
2. データベースマイグレーション
3. アプリケーションデプロイ
4. スモークテスト実行
5. ヘルスチェック確認

### モニタリング

#### アプリケーションモニタリング
- Application Performance Monitoring (APM)
- エラートラッキング
- ログ集約

#### インフラストラクチャモニタリング
- CPU/メモリ使用率
- ディスクI/O
- ネットワークトラフィック

#### ビジネスメトリクス
- アクティブユーザー数
- API呼び出し回数
- 検出ロジック実行回数
- 異常検出数
