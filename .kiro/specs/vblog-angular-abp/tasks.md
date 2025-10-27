# Implementation Plan

このタスクリストは、CAN 異常検出管理システム（Angular + ABP vNext Web API + マルチテナント）を段階的に実装するための計画です。各タスクは、前のタスクの成果物を基に構築されます。

## Task List

- [x] 1. プロジェクトのセットアップと初期化

  - ABP CLI を使用してマルチテナント対応プロジェクトを作成し、基本的な構成を行う
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 16.1, 16.2, 16.3_

- [x] 1.1 ABP vNext Web API プロジェクトの作成

  - ABP CLI で新規プロジェクト（VBlog_Angular）を作成
  - .NET 9、Angular UI、Entity Framework Core、SQL Server を指定
  - マルチテナンシーを有効化（--tiered false --separate-identity-server false）
  - _Requirements: 1.1, 1.2, 16.1_

- [x] 1.2 Angular プロジェクトの作成

  - Angular CLI で新規プロジェクト（frontend）を作成
  - Angular 17+、Angular Material、NgRx を設定
  - ABP Angular パッケージを統合
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [x] 1.3 データベース接続文字列の設定

  - appsettings.json で SQL Server 接続文字列を設定
  - マルチテナント用の接続文字列設定を追加
  - _Requirements: 1.3, 16.1_

- [x] 1.4 初期データベースマイグレーションの実行

  - DbMigrator プロジェクトを実行
  - ABP 標準テーブル + マルチテナント テーブルが作成されることを確認
  - _Requirements: 1.3, 1.4, 16.1_

- [x] 1.5 デフォルトユーザーでログイン確認

  - Web API プロジェクトを起動
  - Swagger UI で API 動作確認
  - Angular アプリでログイン成功を確認
  - _Requirements: 2.1, 7.1, 7.2, 7.3_

- [x] 1.6 日英ローカライゼーションの設定

  - バックエンド: VBlogResource クラスを作成
  - フロントエンド: Angular i18n 設定、ja.json と en.json ファイルを作成
  - 言語切り替えが動作することを確認
  - _Requirements: 11.1, 11.2, 11.3_

- [x] 2. マルチテナント機能の実装

  - OEM ごとのテナント管理機能を実装する
  - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6_

- [x] 2.1 テナント管理エンティティの実装

  - Tenant エンティティの拡張（OemCode、Features 等）
  - OemMaster エンティティの作成
  - TenantFeature エンティティの作成
  - _Requirements: 16.1, 16.2_

- [x] 2.2 テナント管理 Application Service の実装

  - TenantAppService の実装（CRUD 操作、OEM 情報管理）
  - OemMasterAppService の実装
  - テナント切り替え機能の実装
  - _Requirements: 16.2, 16.4_

- [x] 2.3 Angular テナント管理機能の実装

  - TenantSelectorComponent の作成
  - テナント切り替えサービスの実装
  - テナント情報の状態管理（NgRx）
  - _Requirements: 16.4, 16.5_

- [x] 2.4 テナント別データ分離の確認

  - マルチテナント フィルターの動作確認
  - テナント切り替え時のデータ表示確認
  - _Requirements: 16.5, 16.6_

- [x] 3. CAN 異常検出ドメイン層の実装

  - CAN 信号、異常検出ロジック、検出結果のエンティティを実装する
  - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6, 20.1, 20.2_

- [x] 3.1 CAN Signal エンティティの実装

  - CanSignal.cs クラスを作成
  - SignalName、CanId、StartBit、Length 等のプロパティを定義
  - CanSystemType 列挙型を定義
  - _Requirements: 20.1, 20.2_

- [x] 3.2 CAN 異常検出ロジック エンティティの実装

  - CanAnomalyDetectionLogic.cs クラスを作成（IMultiTenant 継承）
  - Name、LogicContent、DetectionType 等のプロパティを定義
  - DetectionType、SharingLevel 列挙型を定義
  - _Requirements: 17.1, 17.2, 17.3_

- [x] 3.3 検出パラメータ・結果 エンティティの実装

  - DetectionParameter.cs クラスを作成
  - AnomalyDetectionResult.cs クラスを作成（IMultiTenant 継承）
  - CanSignalMapping.cs クラスを作成（多対多中間テーブル）
  - _Requirements: 17.4, 18.1, 18.2_

- [x] 3.4 CAN System Category エンティティの実装

  - CanSystemCategory.cs クラスを作成（IMultiTenant 継承）
  - SystemType、Icon、Color 等のプロパティを定義
  - _Requirements: 17.1_

- [x] 3.5 異常検出プロジェクト エンティティの実装

  - AnomalyDetectionProject.cs クラスを作成（IMultiTenant 継承）
  - ProjectCode、VehicleModel、PrimarySystem 等のプロパティを定義
  - _Requirements: 19.1, 19.2_

- [x] 4. Entity Framework Core 設定とマイグレーション

  - DbContext の設定とデータベーススキーマを作成する
  - _Requirements: 1.3, 1.4, 16.1, 17.1, 17.2, 20.1_

- [x] 4.1 VBlogDbContext の設定

  - VBlogDbContext.cs に CAN 関連 DbSet を追加
  - OnModelCreating メソッドで ConfigureVBlog を呼び出す
  - マルチテナント設定を追加
  - _Requirements: 1.3, 16.1_

- [x] 4.2 Entity Framework Core 設定の実装

  - VBlogDbContextModelCreatingExtensions.cs を更新
  - CAN 関連エンティティのテーブル名、プロパティ制約、インデックスを設定
  - マルチテナント フィルターを設定
  - _Requirements: 17.1, 17.2, 20.1, 16.1_

- [x] 4.3 データベースマイグレーションの作成

  - Add-Migration CreateCanAnomalyDetectionTables コマンドを実行
  - 生成されたマイグレーションファイルを確認
  - _Requirements: 1.4_

- [x] 4.4 データベースマイグレーションの適用

  - DbMigrator を実行してマイグレーションを適用
  - SQL Server Management Studio でテーブルが作成されたことを確認
  - _Requirements: 1.4_

- [x] 5. アプリケーション層の実装（DTO とインターフェース）

  - DTO クラスと Application Service インターフェースを定義する
  - _Requirements: 17.3, 17.4, 17.5, 17.6, 18.3, 18.4, 19.3, 19.4, 20.3, 20.4_

- [x] 5.1 CAN Signal DTOs の作成

  - CanSignalDto、CreateCanSignalDto、UpdateCanSignalDto を作成
  - AutoMapper プロファイルを設定
  - _Requirements: 20.1, 20.2_

- [x] 5.2 異常検出ロジック DTOs の作成

  - CanAnomalyDetectionLogicDto、CreateDetectionLogicDto、UpdateDetectionLogicDto を作成
  - DetectionParameterDto、CanSignalMappingDto を作成
  - AutoMapper プロファイルを設定
  - _Requirements: 17.3, 17.4, 17.5_

- [x] 5.3 異常検出結果 DTOs の作成

  - AnomalyDetectionResultDto、CreateDetectionResultDto を作成
  - AutoMapper プロファイルを設定
  - _Requirements: 18.1, 18.2, 18.3_

- [x] 5.4 プロジェクト管理 DTOs の作成

  - AnomalyDetectionProjectDto、CreateProjectDto、UpdateProjectDto を作成
  - ProjectMilestoneDto、ProjectMemberDto を作成
  - _Requirements: 19.1, 19.2, 19.3_

- [x] 5.5 統計・レポート DTOs の作成

  - DetectionStatisticsDto、SystemAnomalyReportDto を作成
  - _Requirements: 6.1, 6.2, 6.3, 20.5_

- [x] 5.6 Application Service インターフェースの定義

  - ICanSignalAppService、ICanAnomalyDetectionLogicAppService を作成
  - IAnomalyDetectionResultAppService、IAnomalyDetectionProjectAppService を作成
  - 各メソッドのシグネチャを定義
  - _Requirements: 17.6, 18.4, 19.4, 20.3_

- [x] 6. 権限定義の実装

  - ABP Permission System で CAN 異常検出関連の権限を定義する
  - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 16.5_

- [x] 6.1 VBlogPermissions クラスの更新

  - CAN 異常検出関連の権限定数を定義
  - CanSignals.Create、DetectionLogics.Edit、Results.View 等を定義
  - テナント管理権限も定義
  - _Requirements: 9.1, 16.5_

- [x] 6.2 VBlogPermissionDefinitionProvider の実装

  - Define メソッドで CAN 異常検出権限グループと権限を登録
  - 日英ローカライゼーションキーを設定
  - _Requirements: 9.1, 9.2_

- [x] 7. アプリケーション層の実装（Application Services）

  - ビジネスロジックと CRUD 操作を実装する
  - _Requirements: 17.6, 18.4, 18.5, 19.4, 19.5, 20.3, 20.4, 20.5_

- [x] 7.1 CanSignalAppService の実装

  - GetListAsync、GetAsync、CreateAsync、UpdateAsync、DeleteAsync メソッドを実装
  - 権限チェック（Authorize 属性）を追加
  - CAN 信号の重複チェック機能
  - _Requirements: 20.3, 20.4_

- [x] 7.2 CanAnomalyDetectionLogicAppService の実装

  - GetListAsync（フィルタリング、ページネーション）を実装
  - CreateAsync、UpdateAsync、DeleteAsync メソッドを実装
  - ロジック実行・テスト機能を実装
  - 権限チェックとテナント分離を追加
  - _Requirements: 17.6, 18.1_

- [x] 7.3 AnomalyDetectionResultAppService の実装

  - GetListAsync、CreateAsync、UpdateAsync メソッドを実装
  - 異常検出結果の統計機能を実装
  - 共有レベル制御機能を実装
  - _Requirements: 18.4, 18.5_

- [x] 7.4 AnomalyDetectionProjectAppService の実装

  - プロジェクト管理の CRUD 操作を実装
  - マイルストーン管理機能を実装
  - プロジェクト進捗レポート機能を実装
  - _Requirements: 19.4, 19.5_

- [x] 7.5 統計・レポート機能の実装

  - 異常検出統計の集計機能を実装
  - システム別異常レポート生成機能を実装
  - PDF/Excel エクスポート機能を実装
  - _Requirements: 6.1, 6.2, 6.3, 20.5_

- [x] 8. Angular フロントエンド基盤の構築

  - Angular プロジェクト構造とベースコンポーネントを実装する
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 8.1, 8.2, 9.1, 9.2_

- [x] 8.1 Angular プロジェクト構造の構築

  - フィーチャーモジュール（can-signals、detection-logics、results 等）を作成
  - 共通コンポーネント（layout、ui-components）を作成
  - NgRx ストア構造を設定
  - _Requirements: 2.1, 8.1_

- [x] 8.2 Element Plus スタイル UI コンポーネントの作成

  - Button、Card、Table、Modal コンポーネントを作成
  - CAN 異常検出テーマ CSS を作成
  - Angular Material + カスタム CSS で Element Plus スタイルを再現
  - _Requirements: 2.5, 9.1_

- [x] 8.3 レイアウトコンポーネントの実装

  - Header（テナント切り替え、言語切り替え、ユーザーメニュー）を実装
  - Sidebar（CAN 系統別メニュー、権限制御）を実装
  - Main Layout を実装
  - _Requirements: 2.2, 2.3, 16.4_

- [x] 8.4 認証・認可機能の実装

  - AuthService（JWT 認証、トークン管理）を実装
  - AuthGuard、PermissionDirective を実装
  - JWT Interceptor を実装
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [x] 8.5 API クライアントサービスの実装

  - BaseApiService を作成
  - CanSignalApiService、DetectionLogicApiService を作成
  - エラーハンドリング、ローディング状態管理を実装
  - _Requirements: 14.1, 14.2, 14.3, 14.5_

- [x] 9. CAN 信号管理画面の実装

  - CAN 信号の一覧、作成、編集、削除機能を実装する
  - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.6_

- [x] 9.1 CAN 信号一覧ページの実装

  - テーブルで CAN 信号一覧を表示（信号名、CAN ID、システム種別等）
  - ページネーション、検索・フィルター機能を実装
  - システム種別、OEM 別フィルターを実装
  - _Requirements: 20.1, 20.3_

- [x] 9.2 CAN 信号作成・編集ページの実装

  - 信号名、CAN ID、データ長、周期等の入力フォームを実装
  - システム種別選択、OEM 情報設定を実装
  - バリデーション（CAN ID 重複チェック等）を実装
  - _Requirements: 20.2, 20.4_

- [x] 9.3 CAN 信号詳細・削除機能の実装

  - 信号詳細表示ページを実装
  - 関連する異常検出ロジック一覧を表示
  - 削除時の整合性チェック（使用中ロジック確認）を実装
  - _Requirements: 20.3, 20.6_

- [x] 10. 異常検出ロジック管理画面の実装

  - 異常検出ロジックの一覧、作成、編集、削除機能を実装する
  - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6_

- [x] 10.1 異常検出ロジック一覧ページの実装

  - テーブルでロジック一覧を表示（名前、検出タイプ、システム種別、共有レベル等）
  - ページネーション、検索・フィルター機能を実装
  - システム種別、検出タイプ、共有レベル別フィルターを実装
  - _Requirements: 17.1, 17.6_

- [x] 10.2 異常検出ロジック作成・編集ページの実装

  - ロジック名、説明、検出タイプ選択フォームを実装
  - CAN 信号選択・マッピング機能を実装
  - 検出パラメータ設定機能を実装
  - ロジック内容エディタ（JSON/Script）を実装
  - _Requirements: 17.2, 17.3, 17.4_

- [x] 10.3 ロジックテンプレート機能の実装

  - 検出タイプ別テンプレート（範囲外、変化率、通信断等）を作成
  - テンプレート選択・適用機能を実装
  - カスタムテンプレート保存機能を実装
  - _Requirements: 17.4, 17.5_

- [x] 10.4 ロジック実行・テスト機能の実装

  - テストデータ入力機能を実装
  - ロジック実行・結果表示機能を実装
  - デバッグ情報表示機能を実装
  - _Requirements: 17.6_

- [x] 11. 異常検出結果管理画面の実装

  - 異常検出結果の一覧、詳細、共有機能を実装する
  - _Requirements: 18.1, 18.2, 18.3, 18.4, 18.5, 18.6_

- [x] 11.1 異常検出結果一覧ページの実装

  - テーブルで検出結果一覧を表示（検出時間、異常レベル、信頼度等）
  - 日付範囲、異常レベル、解決状況別フィルターを実装
  - リアルタイム更新機能（SignalR）を実装

  - _Requirements: 18.1, 18.3_

- [x] 11.2 異常検出結果詳細ページの実装

  - 検出結果詳細情報表示を実装
  - 入力データ（CAN 信号値）の可視化を実装
  - 解決状況更新・コメント機能を実装
  - _Requirements: 18.2, 18.4_

- [x] 11.3 結果共有機能の実装

  - 共有レベル設定機能を実装
  - 共有承認ワークフロー機能を実装
  - 他 OEM からの共有結果閲覧機能を実装
  - _Requirements: 18.5, 18.6_

- [x] 12. プロジェクト管理画面の実装

  - 異常検出プロジェクトの管理機能を実装する
  - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6_

- [x] 12.1 プロジェクト一覧・作成ページの実装

  - プロジェクト一覧表示（名前、ステータス、進捗率等）を実装
  - プロジェクト作成フォーム（基本情報、OEM 情報、システム種別）を実装
  - プロジェクトステータス管理機能を実装
  - _Requirements: 19.1, 19.4_

- [x] 12.2 プロジェクト詳細・進捗管理ページの実装

  - プロジェクト詳細情報表示を実装
  - 関連検出ロジック一覧表示を実装
  - 進捗ダッシュボード（ガントチャート、進捗率）を実装
  - _Requirements: 19.2, 19.3_

- [x] 12.3 マイルストーン・メンバー管理機能の実装

  - マイルストーン設定・管理機能を実装
  - プロジェクトメンバー管理機能を実装
  - 通知・アラート機能を実装
  - _Requirements: 19.4, 19.5, 19.6_

- [x] 13. 統計ダッシュボード画面の実装

  - 異常検出統計とグラフを表示する
  - _Requirements: 6.1, 6.2, 6.3, 6.4, 20.5_

- [x] 13.1 統計概要ダッシュボードの実装

  - 統計概要カード（総検出数、システム別異常数等）を表示
  - 異常レベル別統計グラフを実装
  - システム別異常検出数グラフを実装

  - _Requirements: 6.1, 6.2_

- [x] 13.2 詳細統計・レポート機能の実装

  - 日付別異常検出数グラフを実装
  - OEM 別統計比較機能を実装
  - カスタムレポート生成機能を実装
  - _Requirements: 6.3, 6.4, 20.5_

- [x] 14. データシードとサンプルデータ

  - 初期データとサンプルデータを投入する
  - _Requirements: 16.2, 20.1, 17.1_

- [x] 14.1 VBlogDataSeedContributor の実装

  - デフォルト OEM テナント（Toyota、Honda、Nissan）を作成
  - デフォルト CAN システムカテゴリを作成
  - サンプル CAN 信号を作成
  - サンプル異常検出ロジックを作成
  - _Requirements: 16.2, 20.1, 17.1_

- [x] 14.2 権限・ロール設定

  - CAN 異常検出関連権限の設定
  - OEM 管理者、エンジニア、閲覧者ロールの作成
  - テナント別権限付与
  - _Requirements: 9.1, 16.5_

- [x] 15. テスト実装

  - Application Service と Angular コンポーネントのテストを作成する
  - _Requirements: 12.1, 12.2, 12.3_

- [x] 15.1 Backend Unit Tests の実装

  - CanSignalAppService_Tests を実装
  - CanAnomalyDetectionLogicAppService_Tests を実装
  - マルチテナント機能のテストを実装
  - _Requirements: 12.1, 12.3_

- [x] 15.2 Angular Unit Tests の実装

  - CAN 信号管理コンポーネントのテストを実装
  - 異常検出ロジック管理コンポーネントのテストを実装
  - テナント切り替え機能のテストを実装
  - _Requirements: 12.1, 12.3_

- [x] 15.3 E2E Tests の実装

  - Cypress で CAN 信号管理の E2E テストを実装
  - 異常検出ロジック作成・実行の E2E テストを実装
  - マルチテナント機能の E2E テストを実装
  - _Requirements: 12.2_

- [x] 16. ドキュメントとデプロイ準備

  - プロジェクトの README とデプロイ設定を作成する
  - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5_

- [x] 16.1 README とセットアップガイドの作成

  - VBlog_Angular/README.md を作成
  - プロジェクト概要（CAN 異常検出管理システム）
  - 技術スタック（Angular 17+、ABP vNext 9.x、マルチテナント）
  - セットアップ手順（バックエンド・フロントエンド）
  - _Requirements: 13.5_

- [x] 16.2 Docker 設定の作成

  - backend/Dockerfile、frontend/Dockerfile を作成
  - docker-compose.yml（開発・本番環境）を作成
  - 環境変数設定ファイルを作成
  - _Requirements: 13.1, 13.2_

- [x] 16.3 CI/CD パイプラインの設定

  - GitHub Actions または Azure DevOps パイプラインを設定
  - 自動テスト・ビルド・デプロイを設定
  - _Requirements: 13.3, 13.4, 13.5_

- [x] 17. 異常検出詳細分析機能の実装

  - CAN 信号の異常検出詳細分析機能を実装する
  - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.5, 20.6, 20.7, 20.8_

- [x] 17.1 DetectionResult エンティティの拡張

  - DetectionDuration、AnomalyType、DetectionCondition プロパティを追加
  - IsValidated、IsFalsePositive、ValidationNotes プロパティを追加
  - AnomalyType 列挙型を定義（Timeout、OutOfRange、RateOfChange 等）
  - _Requirements: 20.1, 20.2_

- [x] 17.2 AnomalyAnalysisService ドメインサービスの実装

  - AnalyzePattern メソッド（異常パターン分析）を実装
  - GenerateThresholdRecommendations メソッド（閾値最適化推奨）を実装

  - CalculateDetectionAccuracy メソッド（検出精度評価）を実装
  - _Requirements: 20.3, 20.4, 20.5_

- [x] 17.3 IDetectionResultRepository の拡張

  - GetByCanSignalAndPeriodAsync メソッドを追加
  - GetByAnomalyTypeAsync メソッドを追加
  - GetStatisticsAsync メソッドを追加
  - _Requirements: 20.3_

- [x] 17.4 AnomalyAnalysisAppService の実装

  - AnalyzeAnomalyPatternAsync メソッドを実装
  - GetThresholdRecommendationsAsync メソッドを実装
  - GetDetectionAccuracyMetricsAsync メソッドを実装
  - _Requirements: 20.4, 20.5, 20.6_

- [x] 17.5 Angular 異常分析画面の実装

  - 異常パターン分析ページを実装（統計グラフ、異常種類別集計）
  - 閾値最適化推奨表示機能を実装
  - 検出精度評価ダッシュボードを実装
  - _Requirements: 20.7, 20.8_

- [x] 18. OEM 間トレーサビリティ強化機能の実装

  - OEM 固有のカスタマイズと承認ワークフローを実装する
  - _Requirements: 21.1, 21.2, 21.3, 21.4, 21.5, 21.6, 21.7, 21.8, 21.9_

- [x] 18.1 OemCustomization エンティティの実装

  - EntityId、EntityType、OemCode、CustomizationType プロパティを定義
  - CustomParameters、OriginalParameters プロパティを定義
  - Approve、SubmitForApproval メソッドを実装
  - _Requirements: 21.1, 21.3_

- [x] 18.2 OemApproval エンティティの実装

  - EntityId、EntityType、OemCode、ApprovalType プロパティを定義
  - RequestedBy、ApprovedBy、Status プロパティを定義
  - Approve、Reject メソッドを実装
  - _Requirements: 21.5_

- [x] 18.3 TraceabilityQueryService の拡張

  - TraceAcrossOems メソッド（OEM 間トレーサビリティ追跡）を実装
  - GetOemCustomizations メソッドを実装
  - AnalyzeCrossOemDifferences メソッド（OEM 間差異分析）を実装
  - _Requirements: 21.2, 21.4_

- [x] 18.4 IOemCustomizationRepository、IOemApprovalRepository の実装

  - GetByEntityAndOemAsync、GetByOemAsync メソッドを実装
  - GetPendingApprovalsAsync メソッドを実装
  - GetLatestCustomizationAsync、GetLatestApprovalAsync メソッドを実装
  - _Requirements: 21.3, 21.5_

- [x] 18.5 OemTraceabilityAppService の実装

  - GetOemTraceabilityAsync メソッドを実装
  - CreateOemCustomizationAsync メソッドを実装
  - SubmitForApprovalAsync、ApproveCustomizationAsync メソッドを実装
  - GenerateOemTraceabilityReportAsync メソッドを実装
  - _Requirements: 21.6, 21.7, 21.8, 21.9_

- [x] 18.6 Angular OEM トレーサビリティ画面の実装

  - OEM 間トレーサビリティ表示ページを実装
  - OEM カスタマイズ管理ページを実装
  - OEM 承認ワークフロー画面を実装
  - OEM 間差異分析ダッシュボードを実装
  - _Requirements: 21.8, 21.9_

- [x] 19. 類似比較・履歴データ抽出機能の実装

  - CAN 信号の類似検索と検査データ比較機能を実装する
  - _Requirements: 22.1, 22.2, 22.3, 22.4, 22.5, 22.6, 22.7, 22.8, 22.9_

- [x] 19.1 SimilarPatternSearchService ドメインサービスの実装

  - SearchSimilarSignals メソッド（類似 CAN 信号検索）を実装
  - CompareTestData メソッド（検査データ比較）を実装
  - CalculateSimilarity メソッド（類似度計算）を実装
  - _Requirements: 22.1, 22.4_

- [x] 19.2 SimilaritySearchCriteria 値オブジェクトの実装

  - CompareCanId、CompareSignalName、CompareSystemType プロパティを定義
  - MinimumSimilarity、MaxResults プロパティを定義
  - バリデーションロジックを実装
  - _Requirements: 22.1_

- [x] 19.3 TestDataComparison 値オブジェクトの実装

  - ThresholdDifferences、DetectionConditionDifferences プロパティを定義
  - ResultDifferences、Recommendations プロパティを定義
  - ThresholdDifference 値オブジェクトを実装
  - _Requirements: 22.4_

- [x] 19.4 SimilarPatternSearchAppService の実装

  - SearchSimilarSignalsAsync メソッドを実装
  - CompareTestDataAsync メソッドを実装
  - GetSimilarSignalRecommendationsAsync メソッドを実装
  - ExportComparisonResultAsync メソッド（CSV/Excel/PDF）を実装
  - _Requirements: 22.2, 22.3, 22.5, 22.6_

- [x] 19.5 Angular 類似比較画面の実装

  - 類似 CAN 信号検索ページを実装（検索条件設定、結果一覧）
  - 過去検査データ一覧ページを実装（フィルター、ソート）
  - 検査データ比較分析ページを実装（差異表示、推奨表示）
  - 検査データ可視化機能を実装（時系列グラフ、分布図、相関図）
  - _Requirements: 22.7, 22.8, 22.9_

- [-] 20. 最終確認とデプロイ準備

  - すべての機能が正常に動作することを確認する
  - _Requirements: すべて_

- [ ] 20.1 機能テストの実施

  - マルチテナント機能（テナント切り替え、データ分離）
  - CAN 信号管理（CRUD 操作、検索・フィルター）
  - 異常検出ロジック管理（作成、編集、実行、テスト）
  - 異常検出結果管理（一覧、詳細、共有）
  - プロジェクト管理（進捗、マイルストーン）
  - 統計ダッシュボード（グラフ、レポート）
  - 異常検出詳細分析（パターン分析、閾値推奨）
  - OEM トレーサビリティ（カスタマイズ、承認、差異分析）
  - 類似比較・履歴データ抽出（検索、比較、可視化）
  - 権限制御・言語切り替えの動作確認
  - _Requirements: すべて_

- [ ] 20.2 パフォーマンステストの実施

  - 大量 CAN 信号データでの一覧表示性能
  - 異常検出ロジック実行性能
  - マルチテナント環境での同時アクセス性能
  - 類似検索・比較分析の性能
  - _Requirements: 10.1, 10.2_

- [x] 20.3 本番環境設定ファイルの作成

  - appsettings.Production.json を作成
  - Angular environment.prod.ts を作成
  - 環境変数の設定方法をドキュメント化
  - _Requirements: 13.1, 13.2_

## 注意事項

- **マルチテナント対応**: すべてのエンティティで IMultiTenant インターフェースの実装を確認
- **CAN 異常検出特化**: 既存のブログ機能ではなく、CAN 信号・異常検出に特化した機能を実装
- **OEM 情報管理**: テナント切り替えと OEM 固有情報の適切な分離を実装
- **段階的実装**: 各タスクは順番に実装し、前のタスクが完了してから次に進む
- **Angular + ABP 統合**: ABP Angular パッケージを活用した効率的な開発

## 実装順序の推奨

1. **基盤構築（タスク 1-4）**: プロジェクト作成、マルチテナント、エンティティ、データベース
2. **ビジネスロジック（タスク 5-7）**: DTOs、権限、Application Services
3. **フロントエンド基盤（タスク 8）**: Angular 構造、認証、UI コンポーネント
4. **機能実装（タスク 9-13）**: CAN 信号、検出ロジック、結果、プロジェクト、統計
5. **品質保証（タスク 14-15）**: データシード、テスト
6. **ドキュメント・デプロイ（タスク 16）**: README、Docker、CI/CD
7. **新規要件対応（タスク 17-19）**: 異常検出詳細分析、OEM トレーサビリティ、類似比較
8. **完成（タスク 20）**: 最終確認、パフォーマンステスト、本番環境設定
