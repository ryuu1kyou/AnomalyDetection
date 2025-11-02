# 要件トレーサビリティレビュー (vblog-angular-abp / AnomalyDetection)

最終更新: 2025-11-02

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

- Covered 約 25%
- Partial 約 55%
- Missing 約 20%

> 推定値。詳細は下記要件別一覧参照。

## 要件別一覧

| #   | 要件                          | 判定            | 主対応ファイル/クラス                                                                               | 主なギャップ / TODO                              |
| --- | ----------------------------- | --------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| 1   | マルチテナント基盤            | Covered/Partial | ABP TenantManagement modules, `IMultiTenant` aggregates, `MultiTenantDataIsolationService_Tests.cs` | テナント切替 UI 完全性未確認                     |
| 2   | CAN 信号マスタ管理            | Partial         | `CanSignal.cs`, `CanSignalAppService.cs`, `CanSystemCategory.cs`                                    | Import 処理/TODO、検索高度化、性能テスト         |
| 3   | 異常検出ロジック開発          | Partial         | `CanAnomalyDetectionLogic.cs`, `CanAnomalyDetectionLogicAppService.cs`                              | バージョン管理詳細、ロジック実行/テスト結果保存  |
| 4   | ロジックテンプレート          | Missing         | `ICanAnomalyDetectionLogicAppService.cs` (メソッド定義), AppService TODO                            | テンプレート生成/取得実装不在                    |
| 5   | 検出結果管理・分析            | Partial         | `AnomalyDetectionResult.cs`, `AnomalyDetectionResultAppService.cs`, `StatisticsAppService.cs`       | リアルタイム通知, Export, 高度フィルタ           |
| 6   | 情報流用・継承                | Partial         | VehiclePhaseId フィールド, `ApprovalType.Inheritance`, OemTraceability                              | 互換性分析アルゴリズム, 推奨調整表示             |
| 7   | 車両フェーズ履歴管理          | Partial         | ロジックの VehiclePhase 参照, DTO input                                                             | フェーズライフサイクルと履歴比較 UI/サービス不足 |
| 8   | 認証・認可                    | Covered/Partial | OpenIddict 設定, Permissions, Roles (`SafetyEngineer`)                                              | 監査レポート/権限外操作ログの UI 確認            |
| 9   | リアルタイム検出処理          | Missing         | SignalR 依存 DLL のみ                                                                               | Hub/判定サービス、100ms SLA 計測未               |
| 10  | 統計・レポート                | Partial         | `StatisticsAppService.cs`                                                                           | Export/Schedule TODO、5 秒 SLA テスト無し        |
| 11  | 多言語対応                    | Partial         | Localization modules, `UseAbpRequestLocalization`                                                   | UI 全要素国際化検証、ユーザー設定永続化          |
| 12  | システム統合/API              | Partial         | Swagger, OIDC                                                                                       | 外部取込/通知専用 API/SLA テスト未               |
| 13  | パフォーマンス/スケール       | Partial/Missing | `QueryPerformanceTests.cs`, covering indexes コメント                                               | 同時ユーザー/信号処理/自動スケール試験未         |
| 14  | セキュリティ/コンプライアンス | Partial/Missing | OpenIddict, Permissions, AuditLogAction                                                             | 暗号化/バックアップ/脆弱性スキャン/GDPR 処理未   |
| 15  | 運用・保守                    | Partial         | `docker-compose.monitoring.yml` (Prometheus/Grafana), Alertmanager                                  | アプリメトリクス/5 分以内通知テスト未            |
| 16  | 最新 CAN 仕様連携             | Missing         | Import TODO (`CanSignalAppService.cs`)                                                              | 差分検出/適合分析/推奨/ダッシュボード未          |
| 17  | 異常検出辞書/ナレッジ         | Missing         | (該当モデル無し)                                                                                    | スキーマ/検索/評価/推奨未                        |
| 18  | 機能安全トレーサビリティ      | Partial         | ASIL 関連 (AsilLevel), OemTraceability, SafetyClassification                                        | 双方向リンク自動化/マトリクス生成/監査レポート未 |
| 19  | クラウド移行設計              | Partial         | Docker, 環境変数, ステートレス構造                                                                  | GCP サービス互換検証/最小変更移行手順未          |
| 20  | 詳細分析 (閾値/遅延等)        | Partial         | SimilarPatternSearch\*, ImpactLevel                                                                 | 検出遅延記録/閾値最適化推奨/精度指標算出未       |
| 21  | OEM 間トレーサビリティ強化    | Partial         | OemTraceability AppService, SharingLevel                                                            | 差異分析レポート/Export 実装未                   |
| 22  | 類似比較・履歴データ抽出      | Partial         | SimilarPatternSearchAppService, DTO                                                                 | 可視化(グラフ)/Export/自動推奨完成度不足         |

## 代表的 TODO/未実装コード断片

- テンプレート: `CanAnomalyDetectionLogicAppService.cs` L217-226 (`// TODO: Implement template ...`)
- 統計レポート: `StatisticsAppService.cs` Export/Schedule 各種 TODO
- CAN Import: `CanSignalAppService.cs` L335 (`// TODO: Implement file import logic`)
- SimilarPatternSearch Export: `SimilarPatternSearchAppService.cs` L152 (CSV/Excel/PDF/JSON 予定)
- OEM トレーサビリティレポート: 実レポート生成コメントのみ

## リスク分類

### 高

- Req4 テンプレート未 → 開発効率低下
- Req9 リアルタイム処理未 → コア価値毀損
- Req17 ナレッジベース不在 → 知見活用不可
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

### Sprint 1-2 (基盤強化)

1. RealTimeDetectionHub + 判定サービス + SLA 計測ミドルウェア (Req9)
2. DetectionTemplateFactory + CreateFromTemplate 実装 (Req4)
3. 機能安全最小トレーサビリティ: `SafetyTraceRecord` 集約 + 変更承認ワークフロー (Req18)
4. ナレッジベース初期: `KnowledgeEntry` 集約, CRUD, Tag/全文検索 (Req17)

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

| タスク                           | 目的                 | 成功条件                                     | 関連要件   |
| -------------------------------- | -------------------- | -------------------------------------------- | ---------- |
| RealTimeDetectionHub             | リアルタイム結果配信 | 異常検出後 <100ms Hub broadcast / テスト通過 | 9,5        |
| DetectionTemplateFactory         | 標準パターン迅速作成 | 4 種類テンプレート + パラメータ検証          | 4          |
| SafetyTraceRecord + ApprovalFlow | 監査対応             | ASIL B+ 変更時承認必須 & 監査レポ生成        | 18,14      |
| KnowledgeBase MVP                | 事例検索             | CRUD + Tag 検索 + 類似検索(名称/タグ)        | 17         |
| ExportService                    | 共通化               | PDF/CSV 2 形式サポート & 3 ドメイン適用      | 5,10,21,22 |

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
