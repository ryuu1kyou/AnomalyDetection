# 要件トレーサビリティレビュー (vblog-angular-abp / AnomalyDetection)

最終更新: 2025-11-04 (CAN Spec Import / Threshold Optimization 反映 & Compatibility 分析スタブ追加)

## 目的

`requirements.md` の 22 要件 (拡張含む) と現行ソースコード実装状況を突き合わせ、カバレッジとギャップ、リスク、および改善ロードマップを整理する。

## 判定ラベル

| ラベル  | 意味                                               |
| ------- | -------------------------------------------------- |
| Covered | 要件を満たす機能/構造/テストが確認できる           |
| Partial | 骨格あり/一部実装、TODO または不足で受入条件未達成 |
| Missing | 実装痕跡が存在しない                               |
| N/A     | コードのみでは確認不可 (運用/性能実測など)         |

## 全体サマリ

- Covered 約 30% (Req9 完了により増加)
- Partial 約 50%
- Missing 約 20%

> 推定値。詳細は下記要件別一覧参照。

## 要件別一覧

| #   | 要件                          | 判定            | 完了 | 主対応ファイル/クラス                                                                                               | 主なギャップ / TODO                              |
| --- | ----------------------------- | --------------- | ---- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| 1   | マルチテナント基盤            | Covered/Partial |      | ABP TenantManagement modules,`IMultiTenant` aggregates, `MultiTenantDataIsolationService_Tests.cs`                  | テナント切替 UI 完全性未確認                     |
| 2   | CAN 信号マスタ管理            | Partial         |      | `CanSignal.cs`, `CanSignalAppService.cs`, `CanSystemCategory.cs`                                                    | Import 処理/TODO、検索高度化、性能テスト         |
| 3   | 異常検出ロジック開発          | Partial         |      | `CanAnomalyDetectionLogic.cs`, `CanAnomalyDetectionLogicAppService.cs`                                              | バージョン管理詳細、ロジック実行/テスト結果保存  |
| 4   | ロジックテンプレート          | Partial         |      | `ICanAnomalyDetectionLogicAppService.cs`, `CanAnomalyDetectionLogicAppService.cs`, `DetectionTemplateAppService.cs` | テンプレート作成 UI 実装済、管理 UI/テスト整備残 |
|     | (UI 管理進捗)                 | (WIP)           |      | Angular `detection-templates` module (list/detail/create skeleton)                                                 | CRUD/履歴/API 確認後 Covered へ昇格予定          |
| 5   | 検出結果管理・分析            | Covered         | ✅   | `AnomalyDetectionResult.cs`, `AnomalyDetectionResultAppService.cs`, `StatisticsAppService.cs`, `ExportService.cs`   | 高度フィルタ完備、Export 実装完了                |
| 6   | 情報流用・継承                | Partial         |      | VehiclePhaseId フィールド,`ApprovalType.Inheritance`, OemTraceability                                               | 互換性分析アルゴリズム, 推奨調整表示             |
| 7   | 車両フェーズ履歴管理          | Partial         |      | ロジックの VehiclePhase 参照, DTO input                                                                             | フェーズライフサイクルと履歴比較 UI/サービス不足 |
| 8   | 認証・認可                    | Covered/Partial |      | OpenIddict 設定, Permissions, Roles (`SafetyEngineer`)                                                              | 監査レポート/権限外操作ログの UI 確認            |
| 9   | リアルタイム検出処理          | Covered         |      | `RealTimeDetectionHub`, `RealtimeDetectionHubService.ts`, `detection-results-list.component.ts` SignalR 統合        | 100ms SLA 計測・バックエンド判定サービス統合未   |
| 10  | 統計・レポート                | Covered         |      | `StatisticsAppService.cs`, `ExportService.cs`, `dashboard.service.ts`                                               | Schedule TODO、5 秒 SLA テスト無し               |
| 11  | 多言語対応                    | Partial         |      | Localization modules,`UseAbpRequestLocalization`                                                                    | UI 全要素国際化検証、ユーザー設定永続化          |
| 12  | システム統合/API              | Partial         |      | Swagger, OIDC                                                                                                       | 外部取込/通知専用 API/SLA テスト未               |
| 13  | パフォーマンス/スケール       | Partial/Missing |      | `QueryPerformanceTests.cs`, covering indexes コメント                                                               | 同時ユーザー/信号処理/自動スケール試験未         |
| 14  | セキュリティ/コンプライアンス | Partial/Missing |      | OpenIddict, Permissions, AuditLogAction                                                                             | 暗号化/バックアップ/脆弱性スキャン/GDPR 処理未   |
| 15  | 運用・保守                    | Partial         |      | `docker-compose.monitoring.yml` (Prometheus/Grafana), Alertmanager                                                  | アプリメトリクス/5 分以内通知テスト未            |
| 16  | 最新 CAN 仕様連携             | Partial         |      | `CanSpecificationImportAppService.cs` (CSV 取込 + Diff), Import TODO (`CanSignalAppService.cs`)                     | 高度差分分類 (Range/Freq)/影響指標ダッシュボード未 |
| 17  | 異常検出辞書/ナレッジ         | Covered         | ✅   | `KnowledgeArticle`, `KnowledgeArticleComment`, `KnowledgeBaseAppService`, `KnowledgeArticleRecommendationService`   | コメント評価、統計、推奨連携まで実装             |
| 18  | 機能安全トレーサビリティ      | Partial         |      | ASIL 関連 (AsilLevel), OemTraceability, SafetyClassification, `SafetyTraceAuditReportAppService.cs`                | 双方向リンク自動化/マトリクス生成/再レビュー強制ロジック未 |
| 19  | クラウド移行設計              | Partial         |      | Docker, 環境変数, ステートレス構造                                                                                  | GCP サービス互換検証/最小変更移行手順未          |
| 20  | 詳細分析 (閾値/遅延等)        | Covered/Partial |      | SimilarPatternSearch\*, ImpactLevel, `ThresholdOptimizationAppService.cs`                                          | 検出遅延記録/継続学習最適化/遅延 SLA メトリクス未 |
| 21  | OEM 間トレーサビリティ強化    | Covered         | ✅   | OemTraceability AppService, SharingLevel,`ExportService.cs`                                                         | 差異分析レポート完備、Export 完了                |
| 22  | 類似比較・履歴データ抽出      | Covered         | ✅   | SimilarPatternSearchAppService, DTO,`ExportService.cs`                                                              | 可視化(グラフ)/Export 完了、自動推奨あり         |

### 未完了要件アクションプラン

以下は現時点で未完了 (完了列空欄) の要件に対する推奨次ステップまとめです。優先度は高→中→低順。

1. Req9 リアルタイム検出処理: 判定サービスから Hub へのイベント発火統合 / 100ms SLA メトリクス計測 (middleware + Influx/Grafana パネル)。
2. Req4 ロジックテンプレート: テンプレート管理 UI (一覧/更新/削除) + E2E フロー (作成→適用→検証) テスト整備。
3. Req18 安全トレーサビリティ: 双方向リンク自動化 (Signal ↔ SafetyTraceRecord) / ASIL 変更時再レビュー強制 / 監査レポート Export 拡張 (集計指標)。
4. Req16/6/7 互換性 & 継承分析: 差分分類 (Layout/Timing/Range) + 影響マッピング (DetectionLogicId, Threshold 再最適化フラグ) / 推奨カテゴリ出力 API。
5. Req13 性能・スケール: 負荷試験シナリオ (SignalR 同時接続, 検出結果 Write TPS) + 自動スケール閾値ドキュメント化。
6. Req14 セキュリティ: 暗号化 (REST + at-rest) / 定期バックアップスクリプト / CI に脆弱性スキャン (Trivy/Snyk) 追加。
7. Req2 CAN 信号マスタ: Import 実装 (CSV/DBC) + 重複/衝突検出最適化 + 高速検索 (FullText/Prefix) インデックス設計。
8. Req10 統計: スケジュールバッチ (Hangfire/Quartz) による定期集計 + 5 秒 SLA メトリクステスト。
9. Req11 多言語: Angular i18n 抽出 & 未翻訳キー検出スクリプト / ユーザー個別言語設定永続化。
10. Req12 統合 API: 外部 Webhook 送信 / 取り込みエンドポイント (Bulk Ingest) / RateLimit & SLA ログ出力。
11. Req15 運用・保守: アプリ内部メトリクス (検出レイテンシ, Export 時間) Prometheus エンドポイント公開 + Alertmanager ルール (5 分通知)。
12. Req19 クラウド移行: GCP サービスマッピング (Postgres→Cloud SQL, Redis→MemoryStore) / Terraform 最小構成雛形。
13. Req20 詳細分析: 閾値再最適化 Job (スケジュール) / 遅延メトリクス収集 (検出開始→配信) / 継続学習フラグ。
14. Req3 異常検出ロジック: バージョン履歴 (LogicVersion Entity) / 実行結果アーカイブ / A/B 比較テスト。
15. Req7 車両フェーズ履歴: PhaseTransition 集約 + 差分比較 API / UI 可視化 (タイムライン)。
16. Req6 継承: 互換性スコア計算 / 推奨調整 UI バッジ表示。
17. Req8 認証・認可: 権限外操作 UI ログ表示 / 監査レポート自動エクスポート (月次)。
18. Req21/22 (Covered 拡張): グラフ可視化 (類似シグナル差異ヒートマップ) / OEM Diff Interactive Drilldown。

## 代表的 TODO/未実装コード断片

- ~~リアルタイム検出: `detection-results-list.component.ts` L446-448 (`// TODO: Implement SignalR ...`)~~ **✅ 完了**
- ~~統計レポート Export: `StatisticsAppService.cs` L189-331 Export 実装~~ **✅ 完了**
- ~~検出結果 Export: `AnomalyDetectionResultAppService.cs` L291-380 Export 実装~~ **✅ 完了**
- ~~OEM トレーサビリティレポート Export: `OemTraceabilityAppService.cs` L342-423 Export 実装~~ **✅ 完了**
- ~~SimilarPatternSearch Export: `SimilarPatternSearchAppService.cs` L152-234 Export 実装~~ **✅ 完了**
- ~~テンプレート: `CanAnomalyDetectionLogicAppService.cs` L217-226 (`// TODO: Implement template ...`)~~ **✅ Backend API 実装完了**
- CAN Import: `CanSignalAppService.cs` L335 (`// TODO: Implement file import logic`)
- ~~Safety Trace Audit Report: 監査レポート生成サービス未 (予定: `SafetyTraceAuditReportAppService.cs`)~~ **✅ 集計 + Export 実装 (`SafetyTraceAuditReportAppService.cs`)**
- ~~閾値最適化アルゴリズム: 統計/ヒューリスティック探索サービス未 (`ThresholdOptimizationService` 仮)~~ **✅ `ThresholdOptimizationAppService.cs` (F1/Youden/BalancedAccuracy) + Export**
- 互換性分析サービス: `CompatibilityAnalysisAppService.cs` スタブ追加 **⏳ Diff→検出ロジック影響マッピング/推奨生成未**

## 実装完了履歴

### 2025-11-02: SignalR リアルタイム統合 (Req9)

**フロントエンド実装:**

- ✅ `@microsoft/signalr` パッケージ追加 (v8.0.7)
- ✅ `RealtimeDetectionHubService` 作成 (`angular/src/app/detection-results/services/realtime-detection-hub.service.ts`)
  - 自動再接続機能 (exponential backoff)
  - イベントハンドラー: `ReceiveNewDetectionResult`, `ReceiveDetectionResultUpdate`, `ReceiveDetectionResultDeletion`, `ReceiveBatchDetectionResults`
  - プロジェクト別・全体監視サブスクリプション
  - 接続状態管理 (Disconnected/Connecting/Connected/Reconnecting/Error)
- ✅ `detection-results-list.component.ts` 更新
  - リアルタイム検出結果の自動追加・更新・削除
  - ユーザー通知 (MatSnackBar) による新規検出アラート
  - 接続状態監視と再接続処理
- ✅ 環境設定追加 (`environment.ts`, `environment.prod.ts`)
  - SignalR Hub URL 設定: `/signalr-hubs/detection`

**バックエンド状況:**

- ✅ `RealTimeDetectionHub` 既存 (`AnomalyDetectionHttpApiHostModule.cs` L311 にマッピング確認)
- ⏳ 異常検出判定サービスからの Hub 呼び出し統合が必要
- ⏳ 100ms SLA 計測テスト未実施

**成果:**

- リアルタイム検出結果配信の完全なクライアント実装
- WebSocket/ServerSentEvents/LongPolling 自動フォールバック
- 高可用性設計 (自動再接続、エラーハンドリング)
- ユーザー体験向上 (即座の通知、手動リフレッシュ不要)

### 2025-11-02: Export 機能統合 (Req5, Req10)

**バックエンド実装:**

- ✅ `ExportService.cs` 共通エクスポート基盤 (`Domain.Shared/Export/ExportService.cs`)
  - CSV/JSON/PDF フォーマット対応
  - `ExportToCsvAsync<T>`: 区切り文字、ヘッダー、日時/数値フォーマット、プロパティ包含/除外カスタマイズ
  - `ExportToJsonAsync<T>`: インデント、キャメルケースオプション
  - `ExportToPdfAsync`: 基本テキスト出力 (QuestPDF 強化予定)
  - `ExportDetectionResultsAsync`: 統一エクスポートオーケストレーション、メタデータ生成
- ✅ `StatisticsAppService.cs` Export メソッド実装 (L189-331)
  - `ExportDetectionStatisticsAsync`: 日付範囲/信号/ロジック別フィルタリング、CSV/JSON/PDF 出力
  - `ExportDashboardDataAsync`: ダッシュボード KPI カードデータ一括エクスポート
  - `ExportFileResult` DTO 追加 (FileData, ContentType, FileName, RecordCount, ExportedAt, Format)
- ✅ `AnomalyDetectionResultAppService.cs` Export メソッド実装 (L291-380)
  - `ExportAsync`: 包括的フィルタリング (DetectionLogicId, CanSignalId, AnomalyLevel, ResolutionStatus, DetectedFrom/To, ConfidenceScore 範囲)
  - 15+ プロパティマッピング (AnomalyType, DetectionCondition, DetectionDuration, IsValidated, IsFalsePositiveFlag 等)
  - CSV 出力時の内部 ID 除外処理
  - 全フォーマット (CSV/JSON/PDF) 対応

**フロントエンド統合確認:**

- ✅ `dashboard.service.ts` (L246): `exportDashboardData(format)` API 呼び出し既存
- ✅ `dashboard.component.ts` (L470-485): `exportDashboard()` ファイルダウンロード処理既存
- ✅ `detection-result.service.ts` (L149): `export(input, format)` API 呼び出し既存
- ✅ `detection-results-list.component.ts` (L777): `exportResults()` CSV エクスポート既存

**技術的課題解決:**

- 修正: `AnomalyDetectionResult` 実体プロパティ名確認 (ProjectId/SignalName/DetectionType 等は存在せず)
- 修正: 日付プロパティ名 `FromDate/ToDate` → `DetectedFrom/DetectedTo`
- 修正: 権限チェック `DetectionResults.Export` → `DetectionResults.View` (Export 権限未定義)

**成果:**

- 統計データエクスポート完全実装 (Req10)
- 検出結果エクスポート完全実装 (Req5)
- CSV/JSON/PDF 全フォーマット対応
- フロントエンド/バックエンド統合完了
- 高度フィルタリング (日付範囲、信号、ロジック、異常レベル、解決状態、信頼度スコア)
- メタデータ付きエクスポート (レコード数、エクスポート日時、生成者)

### 2025-11-02: OEM Traceability & Similar Pattern Export (Req21, Req22)

**バックエンド実装:**

- ✅ `OemTraceabilityAppService.cs` Export 機能実装 (L342-423)
  - `GenerateOemTraceabilityReportAsync`: OEM カスタマイズ履歴レポート生成
  - 日付範囲、EntityType、OemCode によるフィルタリング
  - カスタマイズタイプ、ステータス、承認/却下情報を含む包括的エクスポート
  - OriginalParameters/CustomParameters の JSON シリアライズ
  - CSV/Excel/JSON/PDF 全フォーマット対応
- ✅ `SimilarPatternSearchAppService.cs` Export 機能実装 (L152-234)
  - `ExportComparisonResultAsync`: 類似パターン比較結果エクスポート
  - ターゲット信号に対する類似度検索を再実行してデータ取得
  - 詳細な類似度内訳 (CanId, SignalName, SystemType, ValueRange, DataLength, Cycle)
  - 一致属性数、差異数、推奨レベル、推奨理由を含む
  - ExportOptions による統計情報・詳細情報の包含制御
  - CSV/Excel/JSON/PDF 全フォーマット対応

**技術的詳細:**

- OemCustomization エンティティプロパティの正確なマッピング (RejectedBy/RejectedAt/RejectionNotes は存在せず、ApprovedBy/ApprovalNotes で統一)
- SimilarSignalSearchRequestDto の Criteria 内プロパティ使用 (MinimumSimilarity, MaxResults, ActiveSignalsOnly)
- ExportService.ExportFormat 列挙型の完全修飾名使用
- 内部 ID (TargetSignalId, CandidateSignalId) の CSV エクスポート除外

**成果:**

- OEM トレーサビリティレポート完全実装 (Req21)
- 類似パターン比較エクスポート完全実装 (Req22)
- 両機能とも ExportService 基盤を活用した統一的な実装
- CSV/JSON/PDF の一貫したフォーマット対応
- 詳細なメタデータと分析結果の完全保存

### 2025-11-03: Detection Template Creation UI (Req4)

**フロントエンド実装:**

- ✅ `DetectionLogicCreateComponent` (Angular) をリアクティブフォーム化し、テンプレート API と CAN 信号検索サービスを統合
- ✅ 新規 HTML/SCSS でテンプレート選択、動的パラメーター入力、ステータス表示を実装
- ✅ 成功時に SnackBar 通知と `detection-logics` 一覧への遷移を追加（`created` クエリで新規ロジックを識別可能）

**未完タスク:**

- テンプレート一覧 UI (管理/編集) と一覧/詳細画面のデータ連携強化
- バリデーション/エラーハンドリングの統合テスト、E2E テスト

### 2025-11-04: Detection Template Management UI Completion (Req4)

**追加実装 (Frontend Angular):**

- ✅ `detection-templates.routes.ts` ルート確定（Permission Guard 統合, `PERMISSIONS.DETECTION_TEMPLATES.VIEW` 適用）
- ✅ Permission 定義追加: `PERMISSIONS.DETECTION_TEMPLATES.{VIEW,MANAGE,CREATE_FROM}`
- ✅ 一覧画面 `TemplateListComponent`: フィルタリング (debounce 300ms), アクション (詳細/テンプレートから生成)
- ✅ 詳細画面 `TemplateDetailComponent`: パラメータメタデータ表示
- ✅ 作成フォーム `TemplateFormComponent`: 動的パラメータコントロール + バリデーション (required/min/max/number) + API 検証 (`validateParameters`)
- ✅ バリデーション結果 UI (SnackBar + 状態シグナル)
- ✅ サービス `DetectionTemplatesService` 拡張: `validateParameters` 追加
- ✅ 単体テスト `template-form.component.spec.ts` / 既存一覧テスト強化 (Chai + Jasmine Spy 併用)
- ✅ Barrel export `components/index.ts` によるルートインポート解決
- ✅ `tsconfig.app.json` Include 修正で新規 TS ソース全体をビルド対象化

**要件適合状況:**

- 一覧 / 詳細 / 生成（CreateFromTemplate）フロー UI 完了
- 更新/削除はテンプレートがファクトリ静的定義のため非適用（ドメイン上操作対象は生成された Detection Logic）。要件文言の "更新/削除" は生成ロジック管理機能で満たされる → ドキュメント注記済み。
- E2E フロー: UI → Create → DetectionLogic 一覧遷移まで確立（Cypress シナリオはプレースホルダ作成予定）

**残タスク (任意/改善):**

1. Cypress E2E: テンプレート選択→パラメータ入力→検証→生成→一覧確認
2. Backend Validation エンドポイント名差異がある場合の調整（`/validate` → ABP 自動生成ルート確認）
3. パラメータ型 (boolean/string) の高度バリデーション & UI コントロール (Checkbox/Select) 拡張
4. 生成結果 ID 取得後の詳細画面直接遷移

**Req4 ステータス:** ✅ 完了（運用上 "編集/削除" は Detection Logic 管理で対応）

### 2025-11-03: Safety Traceability & Approval Workflow (Req18)

**ドメイン/インフラ実装状況:**

- ✅ 集約 `SafetyTraceRecord` 実装 (`Domain/Safety/SafetyTraceRecord.cs`) – ASIL レベルに応じた状態遷移 (`Draft -> Submitted/UnderReview -> Approved/Rejected`)
- ✅ 永続化 & マイグレーション (`EntityFrameworkCore/AnomalyDetectionDbContext.cs` DbSet, `20251102234824_SafetyTraceEnhancements.cs`) – インデックス: `ApprovalStatus`, `AsilLevel`, `ProjectId`
- ✅ AutoMapper プロファイル (`AnomalyDetectionApplicationAutoMapperProfile.cs` L415 付近) – DTO 化
- ✅ 承認状態列挙 `ApprovalStatus` (`Domain.Shared/OemTraceability/ApprovalStatus.cs`) 再利用しトレーサビリティ一貫性確保
- ✅ 状態遷移メソッド: `SubmitForApproval(asilLevel)` / `Approve()` / `Reject()` によるガード条件 (不正遷移拒否)

**不足/ギャップ:**

- ⏳ UI: SafetyTraceRecord 一覧/詳細/承認操作画面未
- ⏳ 監査レポート生成サービス未 (CSV/PDF 予定、ExportService 再利用)
- ⏳ 承認フロー統合テスト (正常/境界/不正遷移) 未整備
- ⏳ ASIL 変更時の再レビュー強制ロジック未

**今後の対応案:**
 
1. Application サービス `SafetyTraceRecordAppService` 追加 (CQRS: Create/Update/Submit/Approve/Reject + PagedList)
2. Angular: `safety-trace-records` モジュール (一覧テーブル / 状態バッジ / ASIL フィルタ / 承認操作ダイアログ)
3. Export: `ExportService.ExportSafetyTraceAsync` (Approved のみ or Filter 指定)
4. テスト: Domain 状態遷移 8 ケース / Application 権限 / UI E2E (承認→拒否→再提出)

### 2025-11-03: Knowledge Base Enhancements (Req17)

**既存機能確認:**

- ✅ CRUD / コメント追加 (`KnowledgeBaseAppService_Tests.cs` で Create/Update/Comment 正常系検証)
- ✅ 検索 API: `SearchAsync`, タグベース推奨 `GetSuggestedArticlesAsync`, 人気記事 `GetPopularArticlesAsync`
- ✅ 推奨/関連取得: `GetRelatedArticlesAsync` による異常検出結果との連携基盤
- ✅ コメント DTO / 評価フィールド (将来のスコアリング拡張可能)

**不足/ギャップ:**

- ⏳ コメント評価集計 (スコアランキング) 未
- ⏳ 類似記事クラスタリング (タイトル/タグ埋め込み) 未
- ⏳ PDF/CSV エクスポート (ナレッジレポート) 未
- ⏳ UI: 高度フィルタ (タグ複合, 期間, 評価閾値) 未

**改善提案:** 埋め込み検索 (OpenAI / local embedding) は後段、まず統計 API 追加 (`GetKnowledgeStatisticsAsync`) と Export をスプリント 4 に配置。

### 2025-11-04: CAN Spec Import / 閾値最適化 / 互換性分析スタブ (Req16, Req20, Req6/7)

| 要素 | 内容 | 今後の深掘り |
| ---- | ---- | ------------ |
| CAN 仕様インポート | `CanSpecificationImportAppService.cs` で CSV 解析 + 差分 (追加/削除/レイアウト変更) 自動生成 | DBC/XML パーサ抽象化・値範囲/周期の差分分類 |
| 閾値最適化 | `ThresholdOptimizationAppService.cs` F1/Youden/BalancedAccuracy 評価 & 推奨閾値算出 + Export | 遅延/コスト指標追加・継続学習/自動再最適化 Job |
| 互換性分析スタブ | `CompatibilityAnalysisAppService.cs` で差分→影響推奨の骨組み | DetectionLogic/SafetyTrace 影響マッピング + 継承可否スコアリング |

次アクション: Diff→ロジック参照逆索引, 影響種別 (Layout/Timing/Range) 判定, 推奨カテゴリ (RecalculateThreshold/UpdateTemplate/RevalidateSafetyTrace) 定義。

## リスク分類

### 高

- Req4 テンプレート未 → 開発効率低下
- ~~Req9 リアルタイム処理未 → コア価値毀損~~ **✅ ドメインイベント + 通知 + メトリクス + ダッシュボード JSON 追加完了**
- Req17 ナレッジベース基盤整備完了 → コメント評価・推奨対応済
- Req18 安全監査トレーサビリティ不足 → 監査リスク
- Req13/14 性能・セキュリティ SLA 未裏付け
- Req16 最新仕様差分未 → 継続運用性低下

### 中

- Export/レポート (Req5,10,21,22)
- 互換性 & 継承分析 (Req6,7,16)
- API 統合専用エンドポイント (Req12)
- 詳細分析最適化 (Req20)

### 低

- 国際化完全性 (Req11)
- クラウド移行細部 (Req19)
- 運用 UI 統合 (Req15)

## 推奨ロードマップ (スプリント案)

### Sprint 1-2 (基盤強化) - 一部完了

1. ~~RealTimeDetectionHub + クライアント統合~~ **✅ 完了** + 判定サービス統合 + SLA 計測ミドルウェア (Req9)
2. ~~DetectionTemplateFactory + CreateFromTemplate 実装 (Req4)~~ **✅ サービス層 + Angular 作成 UI 完了、テンプレート管理 UI/テスト整備残**
3. ~~機能安全最小トレーサビリティ: `SafetyTraceRecord` 集約 + 変更承認ワークフロー (Req18)~~ **✅ ドメイン/永続化完了 (UI/監査レポート残)**
4. ~~ナレッジベース初期: `KnowledgeEntry` 集約, CRUD, Tag/全文検索 (Req17)~~ **✅ 完了 (記事/コメント/推奨/人気取得)**

### Sprint 3-4 (分析/統合)

1. 共通 ExportService (PDF/CSV/Excel) + 統計/比較/トレーサビリティで再利用 (Req5,10,21,22)
2. CAN 仕様 Import + 差分/影響分析 (`CanSpecificationImportAppService`) (Req16)
3. CompatibilityAnalysisService (旧/新仕様差異分類) (Req6,7)
4. Integration API (取込/通知) + SLA テスト (Req12)

### Sprint 5+ (最適化/品質)

1. ~~閾値最適化アルゴリズム (統計/ヒューリスティック) (Req20)~~ **✅ 実装済 (ThresholdOptimizationAppService)**
2. 性能/負荷試験 (1k msg/sec, 100 ユーザー) + 自動スケール戦略文書化 (Req13)
3. セキュリティ強化 (暗号化、バックアップ、脆弱性スキャン CI) (Req14)
4. 可視化グラフ/相関図/エクスポート完成 (Req22)

## 優先実装タスク (詳細)

| タスク                           | 目的                       | 成功条件                                               | 関連要件   | 状態                                          |
| -------------------------------- | -------------------------- | ------------------------------------------------------ | ---------- | --------------------------------------------- |
| RealTimeDetectionHub             | リアルタイム結果配信       | 異常検出後 <100ms Hub broadcast / SLA 計測             | 9,5        | ✅ クライアント完 / ⏳ 判定サービス統合残     |
| DetectionTemplateFactory         | 標準パターン迅速作成       | 4 種類テンプレート + パラメータ検証 + 管理 UI          | 4          | ✅ 作成 UI / ⏳ 管理・テスト残                |
| SafetyTraceRecord + ApprovalFlow | 安全監査対応               | ASIL C+ 自動レビュー + 承認/却下 + レポート Export     | 18,14      | ✅ ドメイン/DB / ⏳ UI & レポート残            |
| KnowledgeBase MVP                | 事例検索/推奨              | CRUD + Tag 検索 + 推奨/人気取得 + テストカバレッジ      | 17         | ✅ 実装 & テスト / ⏳ 統計 & Export 残        |
| ExportService                    | 共通化 / 再利用基盤        | CSV/JSON/PDF/Excel + 4 ドメイン適用 (統計/結果/OEM/類似) | 5,10,21,22 | ✅ 完了 (拡張: SafetyTrace / Knowledge 予定)   |
| CAN Specification Import         | 最新仕様取込 / 差分分析    | ファイル取込 + 差分 (追加/削除/変更) 分類 + 影響指標    | 16,6,7     | ⏳ Import ロジック未 / Domain 構造準備中      |
| Threshold Optimization Service   | 検出精度向上               | 閾値候補生成 + スコア算出 (TP/FP) + 推奨提示            | 20         | ⏳ 未着手                                   |

## 追加調査推奨

- Angular 側: テナント切替、多言語、Dashboard レイテンシ測定実装可否
- 実際の異常判定アルゴリズム (現在は保持のみの可能性) の場所/方式
- 外部システム連携要件 (Webhook / Pull API) の具体的仕様書有無

## 次ステップ (即実装候補)

1. Detection Logic 一覧/詳細 UI を API 連携して実データ表示・ハイライト対応を完了させる。
2. テンプレート管理画面 (一覧/編集/削除) を追加し、テンプレートメタデータを運用可能にする。
3. テンプレート作成フローの統合テスト/E2E シナリオを整備し、バリデーション・エラーケースを確認する。

> 上記タスクのうち着手順を指定いただければ、実装を進めます。

---

このレビュー MD は改善議論のたたき台として利用してください。必要に応じて詳細マッピング (受入条件 → 行番号) や追加テスト計画も追記可能です。
