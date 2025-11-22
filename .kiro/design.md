# 設計仕様書: CAN 異常検出管理システム

## 1. システムアーキテクチャ

### 1.1 アーキテクチャパターン
本システムは、ドメイン駆動設計（DDD）とレイヤードアーキテクチャに基づいて設計されています。

**レイヤー構成:**
- **プレゼンテーション層**: Angular UIとRESTful API
- **アプリケーション層**: Application Services、DTOs
- **ドメイン層**: Entities、Value Objects、Domain Services、Repositories
- **インフラストラクチャ層**: EF Core、データベース、外部サービス統合

### 1.2 技術スタック
- **フレームワーク**: ABP Framework (ASP.NET Core ベース)
- **言語**: C# (.NET 10予定)
- **ORM**: Entity Framework Core
- **データベース**: SQL Server / PostgreSQL
- **フロントエンド**: Angular
- **認証**: OAuth 2.0 / OpenID Connect

## 2. ドメインモデル設計

### 2.1 Value Objects

#### 2.1.1 OemCode
OEM組織を識別するコードと名称。

**プロパティ:**
- `Code`: string (一意のOEMコード)
- `Name`: string (OEM名称)

#### 2.1.2 SignalIdentifier
CAN信号の識別子。

**プロパティ:**
- `SignalName`: string
- `CanId`: string (一意のCAN ID)

#### 2.1.3 SignalSpecification
信号の技術仕様。

**プロパティ:**
- `StartBit`: int (開始ビット位置)
- `Length`: int (ビット長)
- `DataType`: SignalDataType (Signed/Unsigned等)
- `ValueRange`: SignalValueRange (最小/最大値)
- `ByteOrder`: SignalByteOrder (BigEndian/LittleEndian)

#### 2.1.4 PhysicalValueConversion
生の信号値から物理値への変換パラメータ。

**プロパティ:**
- `Factor`: double (スケーリング係数)
- `Offset`: double (オフセット)
- `Unit`: string (単位)

#### 2.1.5 SignalTiming
信号のタイミング情報。

**プロパティ:**
- `CycleTime`: int (サイクル時間[ms])
- `TimeoutTime`: int (タイムアウト時間[ms])
- `SendType`: SignalSendType (Cyclic/OnChange等)

### 2.2 Entities と Aggregate Roots

#### 2.2.1 CanSignal (Aggregate Root)
CAN信号の完全な定義。

**主要プロパティ:**
- `Id`: Guid
- `TenantId`: Guid?
- `Identifier`: SignalIdentifier
- `Specification`: SignalSpecification
- `Conversion`: PhysicalValueConversion
- `Timing`: SignalTiming
- `SystemType`: CanSystemType
- `OemCode`: OemCode
- `Version`: SignalVersion
- `Status`: SignalStatus
- `IsStandard`: bool

**主要メソッド:**
- `UpdateSpecification()`
- `UpdateConversion()`
- `SetAsStandard()`
- `Activate()` / `Deactivate()`

#### 2.2.2 CanAnomalyDetectionLogic (Aggregate Root)
異常検出ロジックの定義。

**主要プロパティ:**
- `Id`: Guid
- `TenantId`: Guid?
- `Identity`: DetectionLogicIdentity
- `Specification`: DetectionLogicSpecification
- `Implementation`: LogicImplementation
- `SafetyClassification`: SafetyClassification?
- `Parameters`: List<DetectionParameter> (子エンティティ)
- `SignalMappings`: List<CanSignalMapping> (子エンティティ)
- `Status`: DetectionLogicStatus
- `SharingLevel`: SharingLevel

**主要メソッド:**
- `AddParameter()`
- `MapSignal()`
- `SubmitForApproval()`
- `Approve()` / `Reject()`

#### 2.2.3 AnomalyDetectionResult (Aggregate Root)
検出実行の結果。

**主要プロパティ:**
- `Id`: Guid
- `TenantId`: Guid?
- `DetectionLogicId`: Guid
- `CanSignalId`: Guid
- `InputData`: DetectionInputData
- `DetectionDetails`: DetectionDetails
- `AnomalyLevel`: AnomalyLevel
- `ConfidenceScore`: double
- `ResolutionStatus`: ResolutionStatus

#### 2.2.4 OemCustomization (Aggregate Root)
OEMによるカスタマイズ。

**主要プロパティ:**
- `Id`: Guid
- `EntityId`: Guid
- `EntityType`: string
- `OemCode`: OemCode
- `CustomParameters`: Dictionary<string, object>
- `OriginalParameters`: Dictionary<string, object>
- `Reason`: string
- `Status`: CustomizationStatus

**主要メソッド:**
- `SubmitForApproval()`
- `Approve()`
- `Reject()`

## 3. データベース設計

### 3.1 主要テーブル
- `CanSignals`: CAN信号マスタ
- `CanAnomalyDetectionLogics`: 検出ロジック
- `DetectionParameters`: 検出パラメータ（親: CanAnomalyDetectionLogics）
- `CanSignalMappings`: 信号マッピング（親: CanAnomalyDetectionLogics）
- `AnomalyDetectionResults`: 検出結果
- `AnomalyDetectionProjects`: プロジェクト
- `ProjectMilestones`: マイルストーン（親: AnomalyDetectionProjects）
- `ProjectMembers`: プロジェクトメンバー（親: AnomalyDetectionProjects）
- `OemCustomizations`: OEMカスタマイズ
- `OemApprovals`: 承認ワークフロー
- `OemMasters`: OEMマスタ
- `ExtendedTenants`: テナント拡張情報

### 3.2 インデックス戦略
- `CanSignals`: `TenantId + CanId` (一意)、`SystemType`、`Status`
- `CanAnomalyDetectionLogics`: `TenantId + Name + Version`、`Status`、`SharingLevel`
- `AnomalyDetectionResults`: `TenantId + Timestamp`、`DetectionLogicId`、`CanSignalId`
- `OemCustomizations`: `TenantId + EntityId + EntityType`、`Status`

### 3.3 Value Object のマッピング
Value ObjectsはEF Coreの`OwnsOne()`を使用して親エンティティのテーブルに埋め込まれます。JSON列も使用可能。

## 4. Application Services 設計

### 4.1 CanSignalAppService
**責務**: CAN信号の管理

**主要メソッド:**
- `GetListAsync()`: ページング、フィルタリング、ソート
- `GetAsync()`: 単一信号の取得
- `CreateAsync()`: 新規信号作成（CAN ID重複チェック含む）
- `UpdateAsync()`: 信号更新
- `DeleteAsync()`: 信号削除
- `ImportFromFileAsync()`: ファイルからの一括インポート **(Req 16)**
- `ExportToFileAsync()`: ファイルへのエクスポート

### 4.2 CanAnomalyDetectionLogicAppService
**責務**: 検出ロジックの管理

**主要メソッド:**
- `GetListAsync()`
- `CreateAsync()`
- `UpdateAsync()`
- `AddParameterAsync()`
- `MapSignalAsync()`
- `SubmitForApprovalAsync()`

### 4.3 SimilarPatternSearchAppService
**責務**: 類似パターン検索 **(Req 13)**

**主要メソッド:**
- `SearchSimilarSignalsAsync()`: 類似信号の検索
- `CompareTestDataAsync()`: テストデータの比較

### 4.4 AnomalyAnalysisAppService
**責務**: 異常分析 **(Req 14)**

**主要メソッド:**
- `AnalyzeAnomalyPatternAsync()`: パターン分析
- `CalculateDetectionAccuracyAsync()`: 精度計算
- `GenerateThresholdRecommendationsAsync()`: 閾値推奨

## 5. Domain Services 設計

### 5.1 CanSpecificationParser
**責務**: CAN仕様ファイルの解析

**メソッド:**
- `Parse(byte[] content, string format)`: ファイル解析してParseResult返却

**サポート形式:**
- CSV
- DBC (今後)

### 5.2 SimilarPatternSearchService
**責務**: 類似度計算ロジック

**メソッド:**
- `CalculateSimilarity()`: 2つの信号の類似度計算
- `CalculateSimilarityBreakdown()`: 詳細な類似度内訳

### 5.3 AnomalyAnalysisService
**責務**: 異常分析ロジック

**メソッド:**
- `AnalyzePattern()`: パターン分析
- `CalculateAccuracy()`: 精度計算

## 6. セキュリティ設計

### 6.1 認証・認可
- **認証**: OAuth 2.0 / OpenID Connect
- **認可**: ABP Framework のRBAC
- **権限定義**: `AnomalyDetectionPermissions` クラス

### 6.2 マルチテナント分離
- EF Coreのグローバルフィルタで`TenantId`による自動フィルタリング
- 共有リソース（`SharingLevel`）の制御

### 6.3 監査ログ
- ABP の `FullAuditedAggregateRoot` による自動記録
- 作成者、作成日時、更新者、更新日時、削除者、削除日時

## 7. パフォーマンス設計

### 7.1 キャッシング
- Redis を使用したマスターデータのキャッシング
- `CacheInvalidationService` による自動キャッシュ無効化

### 7.2 クエリ最適化
- Eager Loading (`Include()`) によるN+1問題の解消
- ページネーションの徹底
- 適切なインデックスの設定

## 8. UI設計 (Angular)

### 8.1 主要コンポーネント
- **OEM Traceability Dashboard**: カスタマイズと承認の管理
- **Similar Signal Search**: 類似信号の検索と比較
- **Pattern Analysis**: 異常パターンの分析
- **Threshold Recommendations**: 閾値の推奨
- **Accuracy Metrics**: 検出精度の表示

### 8.2 共通コンポーネント
- Data Visualization (Chart.js等)
- Data Table (ページング、ソート、フィルタ)
- Form Validation

## 9. デプロイメント設計

### 9.1 Docker化
- Dockerfile と docker-compose.yml による環境構築
- コンテナイメージの作成

### 9.2 CI/CD
- GitHub Actions / Azure Pipelines
- 自動ビルド、テスト、デプロイ

### 9.3 モニタリング
- Application Insights
- 構造化ロギング (Serilog)
- アラート設定
