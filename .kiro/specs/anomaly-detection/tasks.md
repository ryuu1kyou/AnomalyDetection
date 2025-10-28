# Implementation Plan - CAN 異常検出管理システム

このタスクリストは、requirements.md と design.md に基づいて、CAN 異常検出管理システムを段階的に実装するための計画です。

## 実装の原則

- 各タスクは独立して実装可能
- 各タスクは要求仕様の特定の要件を満たす
- テストは実装後に追加（オプション）
- 段階的に機能を追加し、各段階で動作確認

## タスクリスト

- [x] 1. ドメインモデルの基盤整備

  - Value Objects の実装と既存コードの整合性確認
  - _Requirements: 1.1-1.5, 2.1-2.7, 3.1-3.8_

- [x] 1.1 OemCode バリューオブジェクトの検証と修正

  - OemCode.cs が正しく ValueObject を継承しているか確認
  - Equals()と GetHashCode()の実装を確認
  - _Requirements: 1.2_

- [x] 1.2 SignalIdentifier バリューオブジェクトの検証

  - SignalName、CanId プロパティの実装確認
  - 等価性比較の実装確認
  - _Requirements: 2.2_

- [x] 1.3 SignalSpecification バリューオブジェクトの検証

  - ネストされた ValueRange Value Object の実装確認
  - StartBit、Length、DataType、ByteOrder プロパティの確認
  - _Requirements: 2.3_

- [x] 1.4 DetectionLogicIdentity バリューオブジェクトの検証

  - ネストされた DetectionLogicVersion の実装確認
  - Name、Version、OemCode プロパティの確認
  - _Requirements: 3.1_

- [x] 1.5 その他の Value Objects の検証

  - PhysicalValueConversion、SignalTiming、SignalVersion
  - DetectionLogicSpecification、LogicImplementation、SafetyClassification
  - ProjectConfiguration、NotificationSettings
  - _Requirements: 2.4, 2.5, 2.6, 3.2, 3.3, 3.4, 6.2_

- [x] 2. Entity Framework Core 設定の完全実装

  - DbContext と ModelCreatingExtensions の設計書準拠実装
  - _Requirements: All_

- [x] 2.1 AnomalyDetectionDbContext の更新

  - 全 Aggregate Root の DbSet 定義
  - 全子 Entity の DbSet 定義
  - OemTraceability エンティティの追加
  - _Requirements: All_

- [x] 2.2 マルチテナント管理の EF Core 設定

  - OemMaster の設定（Value Objects、Owned Collections）
  - ExtendedTenant の設定（Value Objects、Owned Collections、Relationships）
  - _Requirements: 1.1-1.5_

- [x] 2.3 CAN 信号管理の EF Core 設定

  - CanSignal の設定（複数の Value Objects、Indexes）
  - CanSystemCategory の設定（Value Object、Indexes）
  - _Requirements: 2.1-2.7, 10.1-10.5_

- [x] 2.4 異常検出ロジック管理の EF Core 設定

  - CanAnomalyDetectionLogic の設定（複数の Value Objects、子 Entity 関係）
  - DetectionParameter の設定（JSON 列）
  - CanSignalMapping の設定（JSON 列）
  - AnomalyDetectionResult の設定（Value Objects、Relationships）
  - _Requirements: 3.1-3.8, 4.1-4.5, 5.1-5.4, 9.1-9.6_

- [x] 2.5 プロジェクト管理の EF Core 設定

  - AnomalyDetectionProject の設定（Value Objects、子 Entity 関係）
  - ProjectMilestone の設定（JSON 列）
  - ProjectMember の設定（JSON 列）
  - _Requirements: 6.1-6.5, 7.1-7.5, 8.1-8.5_

- [x] 2.6 OEM トレーサビリティの EF Core 設定

  - OemCustomization の設定（Value Object、Indexes）
  - OemApproval の設定（Value Object、Indexes）
  - _Requirements: 11.1-11.8, 12.1-12.8_

- [x] 3. データベースマイグレーションの実行

  - 既存マイグレーションの削除と新規作成
  - _Requirements: All_

- [x] 3.1 既存マイグレーションの削除

  - Migrations フォルダ内の全ファイル削除
  - データベースのドロップ
  - _Requirements: All_

- [x] 3.2 新規マイグレーションの作成

  - dotnet ef migrations add Initial コマンド実行
  - 生成されたマイグレーションファイルの確認
  - _Requirements: All_

- [x] 3.3 データベースの作成とシード

  - DbMigrator の実行
  - サンプルデータの投入確認
  - _Requirements: All_

- [x] 4. OEM トレーサビリティ機能の実装

  - カスタマイズと承認ワークフローの実装
  - _Requirements: 11.1-11.8, 12.1-12.8_

- [x] 4.1 OemCustomization エンティティのビジネスロジック検証

  - SubmitForApproval()メソッドの動作確認
  - Approve()、Reject()メソッドの動作確認
  - UpdateCustomParameters()メソッドの動作確認
  - _Requirements: 11.3, 11.4, 11.5, 11.6, 11.7_

- [x] 4.2 OemApproval エンティティのビジネスロジック検証

  - Approve()、Reject()、Cancel()メソッドの動作確認
  - UpdateDueDate()、UpdatePriority()メソッドの動作確認
  - IsOverdue()、IsUrgent()メソッドの動作確認
  - _Requirements: 12.5, 12.6, 12.7, 12.8_

- [x] 4.3 OemTraceabilityAppService の実装

  - カスタマイズ CRUD 操作の実装
  - 承認ワークフロー API の実装
  - トレーサビリティ照会 API の実装
  - _Requirements: 11.1-11.8, 12.1-12.8_

- [x] 4.4 OemTraceability DTOs の実装

  - CreateOemCustomizationDto、UpdateOemCustomizationDto
  - OemCustomizationDto、OemApprovalDto
  - TraceabilityResultDto
  - _Requirements: 11.1-11.8, 12.1-12.8_

- [x] 4.5 OemTraceability の統合テスト

  - カスタマイズ作成から承認までのフロー
  - 承認ワークフローのテスト
  - _Requirements: 11.1-11.8, 12.1-12.8_

- [x] 5. 類似パターン検索機能の実装

  - 類似信号検索とテストデータ比較の実装
  - _Requirements: 13.1-13.8_

- [x] 5.1 SimilarPatternSearchService の検証と拡張

  - SearchSimilarSignalsAsync()の動作確認
  - CompareTestDataAsync()の動作確認
  - CalculateSimilarity()の精度確認
  - _Requirements: 13.2, 13.3, 13.7_

- [x] 5.2 類似度計算アルゴリズムの最適化

  - CalculateSimilarityBreakdown()の重み付け調整
  - DetermineRecommendationLevel()の閾値調整
  - _Requirements: 13.3, 13.5_

- [x] 5.3 SimilarPatternSearchAppService の実装

  - 類似信号検索 API の実装
  - テストデータ比較 API の実装
  - 類似度計算 API の実装
  - _Requirements: 13.1-13.8_

- [x] 5.4 SimilarPatternSearch DTOs の実装

  - SimilaritySearchCriteriaDto
  - SimilarSignalResultDto、SimilarityBreakdownDto
  - TestDataComparisonDto
  - _Requirements: 13.1-13.8_

- [x] 5.5 類似パターン検索の統合テスト

  - 類似信号検索の精度テスト
  - テストデータ比較の正確性テスト
  - _Requirements: 13.1-13.8_

- [x] 6. 異常分析サービスの実装

  - パターン分析、閾値推奨、精度計算の実装
  - _Requirements: 14.1-14.6_

- [x] 6.1 AnomalyAnalysisService の検証と拡張

  - AnalyzePatternAsync()の動作確認
  - GenerateThresholdRecommendationsAsync()の動作確認
  - CalculateDetectionAccuracyAsync()の動作確認

  - _Requirements: 14.1, 14.4, 14.3_

- [x] 6.2 分析アルゴリズムの実装

  - 頻度パターン分析の実装
  - 相関分析の実装
  - 精度メトリクス計算の実装
  - _Requirements: 14.1, 14.2, 14.3_

- [x] 6.3 AnomalyAnalysisAppService の実装

  - パターン分析 API の実装
  - 閾値推奨 API の実装
  - 検出精度 API の実装
  - _Requirements: 14.1-14.6_

- [x] 6.4 AnomalyAnalysis DTOs の実装

  - AnomalyPatternAnalysisResultDto
  - ThresholdRecommendationResultDto
  - DetectionAccuracyMetricsDto
  - _Requirements: 14.1-14.6_

- [x] 6.5 異常分析サービスの統合テスト

  - パターン分析の正確性テスト
  - 閾値推奨の妥当性テスト
  - 精度計算の正確性テスト
  - _Requirements: 14.1-14.6_

- [ ] 7. Angular UI の実装

  - OEM トレーサビリティ、類似パターン検索、異常分析の UI 実装
  - _Requirements: 11.1-11.8, 12.1-12.8, 13.1-13.8, 14.1-14.6_

- [x] 7.1 OEM トレーサビリティ UI の検証

  - oem-traceability-dashboard.component.ts の動作確認
  - oem-customization-management.component.ts の動作確認
  - oem-traceability.service.ts の動作確認
  - _Requirements: 11.1-11.8, 12.1-12.8_

- [x] 7.2 類似パターン検索 UI の検証

  - similar-signal-search.component.ts の動作確認
  - comparison-analysis.component.ts の動作確認
  - data-visualization.component.ts の動作確認
  - test-data-list.component.ts の動作確認
  - _Requirements: 13.1-13.8_

- [x] 7.3 異常分析 UI の検証

  - pattern-analysis.component.ts の動作確認
  - threshold-recommendations.component.ts の動作確認
  - accuracy-metrics.component.ts の動作確認
  - _Requirements: 14.1-14.6_

- [x] 7.4 UI コンポーネントの統合

  - ルーティング設定の確認
  - ナビゲーションメニューの追加
  - 権限制御の実装
  - _Requirements: All_

- [x] 7.5 UI E2E テスト

  - OEM トレーサビリティフローのテスト
  - 類似パターン検索フローのテスト
  - 異常分析フローのテスト
  - _Requirements: All_

- [x] 8. セキュリティと権限管理の実装

  - 認証・認可、権限定義の実装
  - _Requirements: NFR 2.1-2.6_

- [x] 8.1 権限定義の実装

  - AnomalyDetectionPermissions クラスの拡張
  - OemTraceability 権限の追加
  - Analysis 権限の追加

  - _Requirements: NFR 2.4_

- [x] 8.2 権限チェックの実装

  - Application Service への[Authorize]属性追加
  - 各 API エンドポイントの権限チェック
  - _Requirements: NFR 2.4_

- [x] 8.3 監査ログの実装

  - 重要操作の監査ログ記録
  - データ変更履歴の記録
  - _Requirements: NFR 2.5_

- [x] 8.4 セキュリティテスト

  - 権限チェックのテスト
  - マルチテナント分離のテスト
  - _Requirements: NFR 2.1-2.6_

- [x] 9. パフォーマンス最適化


  - データベース最適化、キャッシング、非同期処理の実装
  - _Requirements: NFR 1.1-1.5_

- [x] 9.1 データベースインデックスの最適化

  - 頻繁に検索される列のインデックス追加
  - 複合インデックスの追加
  - _Requirements: NFR 1.1, 1.2_

- [x] 9.2 クエリ最適化

  - N+1 問題の解消（Eager Loading）
  - ページネーションの実装
  - _Requirements: NFR 1.1_

- [x] 9.3 キャッシング実装

  - Redis キャッシュの設定
  - マスターデータのキャッシング
  - _Requirements: NFR 1.1_

- [x] 9.4 パフォーマンステスト

  - 負荷テストの実行
  - レスポンスタイムの測定
  - _Requirements: NFR 1.1-1.5_

- [x] 10. デプロイメントと CI/CD






  - Docker、CI/CD パイプライン、モニタリングの設定
  - _Requirements: NFR 3.1-3.5, NFR 4.1-4.5_

- [x] 10.1 Docker 設定の検証




  - Dockerfile の確認
  - docker-compose.yml の確認
  - _Requirements: NFR 3.1-3.5_

- [x] 10.2 CI/CD パイプラインの設定



  - GitHub Actions / Azure Pipelines の設定
  - ビルド・テスト・デプロイの自動化
  - _Requirements: NFR 4.3_

- [x] 10.3 モニタリング設定



  - Application Insights の設定
  - ログ集約の設定
  - アラート設定
  - _Requirements: NFR 4.1, 4.2_

- [x] 10.4 デプロイメントテスト


  - Staging 環境へのデプロイ
  - Production 環境へのデプロイ
  - _Requirements: NFR 4.1-4.5_

## 実装の優先順位

### Phase 1: 基盤整備（タスク 1-3）

ドメインモデルとデータベースの整備

### Phase 2: コア機能実装（タスク 4-6）

OEM トレーサビリティ、類似パターン検索、異常分析の実装

### Phase 3: UI 実装（タスク 7）

Angular UI の実装と統合

### Phase 4: 品質向上（タスク 8-9）

セキュリティとパフォーマンスの最適化

### Phase 5: 運用準備（タスク 10）

デプロイメントと CI/CD の設定

## 注意事項

- 全てのタスクは必須として実装
- 各タスク完了後は、動作確認を実施
- 問題が発生した場合は、設計書に戻って確認
- 既存のソースコードとの整合性を常に確認
- テストは実装と同時に作成し、品質を確保
