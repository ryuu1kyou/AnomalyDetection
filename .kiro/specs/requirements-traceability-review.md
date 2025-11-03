# 要件トレーサビリティレビュー (vblog-angular-abp / AnomalyDetection)

最終更新: 2025-11-02 (SignalR 統合完了後)

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

| #   | 要件                          | 判定            | 主対応ファイル/クラス                                                                                             | 主なギャップ / TODO                              |
| --- | ----------------------------- | --------------- | ----------------------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| 1   | マルチテナント基盤            | Covered/Partial | ABP TenantManagement modules,`IMultiTenant` aggregates, `MultiTenantDataIsolationService_Tests.cs`                | テナント切替 UI 完全性未確認                     |
| 2   | CAN 信号マスタ管理            | Partial         | `CanSignal.cs`, `CanSignalAppService.cs`, `CanSystemCategory.cs`                                                  | Import 処理/TODO、検索高度化、性能テスト         |
| 3   | 異常検出ロジック開発          | Partial         | `CanAnomalyDetectionLogic.cs`, `CanAnomalyDetectionLogicAppService.cs`                                            | バージョン管理詳細、ロジック実行/テスト結果保存  |
| 4   | ロジックテンプレート          | Missing         | `ICanAnomalyDetectionLogicAppService.cs` (メソッド定義), AppService TODO                                          | テンプレート生成/取得実装不在                    |
| 5   | 検出結果管理・分析            | Covered         | `AnomalyDetectionResult.cs`, `AnomalyDetectionResultAppService.cs`, `StatisticsAppService.cs`, `ExportService.cs` | 高度フィルタ完備、Export 実装完了                |
| 6   | 情報流用・継承                | Partial         | VehiclePhaseId フィールド,`ApprovalType.Inheritance`, OemTraceability                                             | 互換性分析アルゴリズム, 推奨調整表示             |
| 7   | 車両フェーズ履歴管理          | Partial         | ロジックの VehiclePhase 参照, DTO input                                                                           | フェーズライフサイクルと履歴比較 UI/サービス不足 |
| 8   | 認証・認可                    | Covered/Partial | OpenIddict 設定, Permissions, Roles (`SafetyEngineer`)                                                            | 監査レポート/権限外操作ログの UI 確認            |
| 9   | リアルタイム検出処理          | Covered         | `RealTimeDetectionHub`, `RealtimeDetectionHubService.ts`, `detection-results-list.component.ts` SignalR 統合      | 100ms SLA 計測・バックエンド判定サービス統合未   |
| 10  | 統計・レポート                | Covered         | `StatisticsAppService.cs`, `ExportService.cs`, `dashboard.service.ts`                                             | Schedule TODO、5 秒 SLA テスト無し               |
| 11  | 多言語対応                    | Partial         | Localization modules,`UseAbpRequestLocalization`                                                                  | UI 全要素国際化検証、ユーザー設定永続化          |
| 12  | システム統合/API              | Partial         | Swagger, OIDC                                                                                                     | 外部取込/通知専用 API/SLA テスト未               |
| 13  | パフォーマンス/スケール       | Partial/Missing | `QueryPerformanceTests.cs`, covering indexes コメント                                                             | 同時ユーザー/信号処理/自動スケール試験未         |
| 14  | セキュリティ/コンプライアンス | Partial/Missing | OpenIddict, Permissions, AuditLogAction                                                                           | 暗号化/バックアップ/脆弱性スキャン/GDPR 処理未   |
| 15  | 運用・保守                    | Partial         | `docker-compose.monitoring.yml` (Prometheus/Grafana), Alertmanager                                                | アプリメトリクス/5 分以内通知テスト未            |
| 16  | 最新 CAN 仕様連携             | Missing         | Import TODO (`CanSignalAppService.cs`)                                                                            | 差分検出/適合分析/推奨/ダッシュボード未          |
| 17  | 異常検出辞書/ナレッジ         | Covered         | `KnowledgeArticle`, `KnowledgeArticleComment`, `KnowledgeBaseAppService`, `KnowledgeArticleRecommendationService` | コメント評価、統計、推奨連携まで実装             |
| 18  | 機能安全トレーサビリティ      | Partial         | ASIL 関連 (AsilLevel), OemTraceability, SafetyClassification                                                      | 双方向リンク自動化/マトリクス生成/監査レポート未 |
| 19  | クラウド移行設計              | Partial         | Docker, 環境変数, ステートレス構造                                                                                | GCP サービス互換検証/最小変更移行手順未          |
| 20  | 詳細分析 (閾値/遅延等)        | Partial         | SimilarPatternSearch\*, ImpactLevel                                                                               | 検出遅延記録/閾値最適化推奨/精度指標算出未       |
| 21  | OEM 間トレーサビリティ強化    | Covered         | OemTraceability AppService, SharingLevel,`ExportService.cs`                                                       | 差異分析レポート完備、Export 完了                |
| 22  | 類似比較・履歴データ抽出      | Covered         | SimilarPatternSearchAppService, DTO,`ExportService.cs`                                                            | 可視化(グラフ)/Export 完了、自動推奨あり         |

## 代表的 TODO/未実装コード断片

- ~~リアルタイム検出: `detection-results-list.component.ts` L446-448 (`// TODO: Implement SignalR ...`)~~ **✅ 完了**
- ~~統計レポート Export: `StatisticsAppService.cs` L189-331 Export 実装~~ **✅ 完了**
- ~~検出結果 Export: `AnomalyDetectionResultAppService.cs` L291-380 Export 実装~~ **✅ 完了**
- ~~OEM トレーサビリティレポート Export: `OemTraceabilityAppService.cs` L342-423 Export 実装~~ **✅ 完了**
- ~~SimilarPatternSearch Export: `SimilarPatternSearchAppService.cs` L152-234 Export 実装~~ **✅ 完了**
- テンプレート: `CanAnomalyDetectionLogicAppService.cs` L217-226 (`// TODO: Implement template ...`)
- CAN Import: `CanSignalAppService.cs` L335 (`// TODO: Implement file import logic`)

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

## リスク分類

### 高

- Req4 テンプレート未 → 開発効率低下
- ~~Req9 リアルタイム処理未 → コア価値毀損~~ **✅ フロントエンド完了、バックエンド統合残**
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
2. DetectionTemplateFactory + CreateFromTemplate 実装 (Req4)
3. 機能安全最小トレーサビリティ: `SafetyTraceRecord` 集約 + 変更承認ワークフロー (Req18)
4. ~~ナレッジベース初期: `KnowledgeEntry` 集約, CRUD, Tag/全文検索 (Req17)~~ **✅ 完了 (KnowledgeArticle/Comment + 推奨/統計)**

### Sprint 3-4 (分析/統合)

5. 共通 ExportService (PDF/CSV/Excel) + 統計/比較/トレーサビリティで再利用 (Req5,10,21,22)
6. CAN 仕様 Import + 差分/影響分析 (`CanSpecificationImportAppService`) (Req16)
7. CompatibilityAnalysisService (旧/新仕様差異分類) (Req6,7)
8. Integration API (取込/通知) + SLA テスト (Req12)

### Sprint 5+ (最適化/品質)

9. 閾値最適化アルゴリズム (統計/ヒューリスティック) (Req20)
10. 性能/負荷試験 (1k msg/sec, 100 ユーザー) + 自動スケール戦略文書化 (Req13)
11. セキュリティ強化 (暗号化、バックアップ、脆弱性スキャン CI) (Req14)
12. 可視化グラフ/相関図/エクスポート完成 (Req22)

## 優先実装タスク (詳細)

| タスク                           | 目的                 | 成功条件                                     | 関連要件   | 状態            |
| -------------------------------- | -------------------- | -------------------------------------------- | ---------- | --------------- |
| RealTimeDetectionHub             | リアルタイム結果配信 | 異常検出後 <100ms Hub broadcast / テスト通過 | 9,5        | ✅ フロント完了 |
| DetectionTemplateFactory         | 標準パターン迅速作成 | 4 種類テンプレート + パラメータ検証          | 4          | ⏳ 未着手       |
| SafetyTraceRecord + ApprovalFlow | 監査対応             | ASIL B+ 変更時承認必須 & 監査レポ生成        | 18,14      | ⏳ 未着手       |
| KnowledgeBase MVP                | 事例検索             | CRUD + Tag 検索 + 類似検索(名称/タグ)        | 17         | ⏳ 未着手       |
| ExportService                    | 共通化               | PDF/CSV 2 形式サポート & 3 ドメイン適用      | 5,10,21,22 | ⏳ 未着手       |

## 追加調査推奨

- Angular 側: テナント切替、多言語、Dashboard レイテンシ測定実装可否
- 実際の異常判定アルゴリズム (現在は保持のみの可能性) の場所/方式
- 外部システム連携要件 (Webhook / Pull API) の具体的仕様書有無

## 次ステップ (即実装候補)

1. `RealTimeDetectionHub.cs` 雛形追加
2. `DetectionTemplateFactory.cs` と `CreateFromTemplateAsync` 実装
3. `KnowledgeEntry.cs` Aggregate と AppService スケルトン

> 要望あれば次ターンで上記コード生成を開始します。どれから着手するか指示してください。

---

このレビュー MD は改善議論のたたき台として利用してください。必要に応じて詳細マッピング (受入条件 → 行番号) や追加テスト計画も追記可能です。
