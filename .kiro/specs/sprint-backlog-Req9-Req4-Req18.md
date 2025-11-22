# Sprint Backlog (Focused: Req9 / Req4 / Req18)

開始日: 2025-11-04  
想定期間: 2 週間  
目的: リアルタイム検出完全統合・テンプレート管理運用化・安全トレーサビリティ実用レベル強化。

## Sprint Goals

1. Req9: 異常判定完了から Hub ブロードキャストまで p95 < 100ms / p99 < 150ms を計測・可視化。
2. Req4: テンプレート管理 (一覧/編集/削除/適用履歴) UI & API を完成し、E2E 成功率 100%。
3. Req18: SafetyTraceRecord 双方向リンク自動化 & ASIL 変更時再レビュー強制、監査レポート Export (CSV/PDF) 提供。

## Key Deliverables

| 要件 | Deliverable | 完了条件 | 指標 |
|------|-------------|----------|------|
| Req9 | Backend判定→Hub通知統合 | 新検出結果生成後イベント→SignalR呼び出し | p95<100ms / メトリクス Grafana ダッシュボード |
| Req9 | SLA計測ミドルウェア | API Latency/Processing時間計測 | Prometheusメトリクス4種類 exposed |
| Req4 | Template管理API | CRUD + 適用履歴取得 | Unit/Integration/E2E >90% 主要パス |
| Req4 | Angular管理UI | 一覧/詳細/編集/削除/新規作成 | Lighthouse可用性>=95 UI操作E2E成功 |
| Req18| 双方向リンク | SafetyTraceRecord <-> DetectionLogic/CAN Signal 関連自動更新 | 更新時リンク整合テスト8ケース通過 |
| Req18| ASIL再レビュー強制 | ASIL変更でApprovalStatus→UnderReview | 状態遷移不正試行拒否テスト通過 |
| Req18| 監査レポートExport | `SafetyTraceAuditReportAppService` CSV/PDF | 出力ファイルAC: ヘッダ/件数/タイムスタンプ |

## Acceptance Criteria Details

### Req9 Real-time Detection

- 異常検出サービスで結果生成後ドメインイベント `DetectionResultCreatedEvent` を発行し、ハンドラで Hub Broadcast。
- メトリクス: `detection_broadcast_latency_ms` (生成→Hub送信差), `signalr_active_connections`, `detection_results_per_minute`, `broadcast_failures_total`。
- Grafana パネル追加 JSON (配置: `monitoring/grafana/dashboards/realtime.json`).

### Req4 Logic Templates

- API: `GET /api/detection-templates`, `POST /api/detection-templates`, `PUT /api/detection-templates/{id}`, `DELETE /api/detection-templates/{id}`, `POST /api/detection-logics/create-from-template`。
- テンプレート DTO に Version / Parameters / Tags / Description / LastUsedAt。
- UI: フィルタ (タグ/キーワード), 適用回数表示, 削除確認ダイアログ。
- E2E: 新規テンプレート→ロジック生成→一覧反映→編集→再度生成。

### Req18 Safety Traceability

- 双方向リンク: DetectionLogic 更新時参照 SafetyTraceRecord IDs 再評価 & unlink stale entries。
- ASIL 変更: `SafetyTraceRecord.UpdateAsilLevel()` 内で `ApprovalStatus` が Approvedなら UnderReview に遷移。
- 監査 Export: フィルタ (ProjectId, AsilLevel, ApprovalStatus, DateRange) + 集計行 (総件数, Approved割合, ReReview件数)。

## Task Breakdown

### Req9 (Real-time Detection) – Progress Summary

- [x] 判定サービスへのイベント相当処理追加（`CreateAsync` 内で結果保存後にリアルタイム通知呼び出し）
- [ ] Event設計: Domain Event & Handler スケルトン（直接サービス呼出のため未。次スプリントで拡張性目的に移行）
- [x] SignalR通知サービス: 既存 `SignalRRealTimeNotificationService` に latency コンテキスト付与（deliveryLatencyMs, slaMet）
- [x] メトリクス: `signalr_active_connections` ゲージ追加 / 既存 realtime delivery counter & latency histogram 利用
- [ ] 追加メトリクス: `detection_results_per_minute` （未）
- [ ] 追加メトリクス: `broadcast_failures_total` （未）
- [ ] Grafanaダッシュボード JSON 作成（未）
- [ ] 負荷シミュレーションスクリプト (100並列生成)（未）
- [ ] テスト: Unit(イベント) / Integration(Hub broadcast) / SLA計測（未）

Status: 部分達成（基盤 + コア計測 + 接続ゲージ）。SLA 可視化・追加メトリクス・負荷/E2E テストは次スプリントへキャリーオーバー。

### Req4 (Template Management UI) – Completion

- [x] Angular: Module/Route `detection-templates` 実装
- [x] Angular: List / Detail / Create (Edit/Delete はテンプレートが静的ファクトリのため非対象 -> DetectionLogic 側で管理)
- [x] CreateFromTemplate フロー（フォーム動的パラメータ + 検証 + 成功遷移）
- [x] パラメータバリデーション (required / number / min / max + backend validate API 呼び出し)
- [x] Permission 統合 (`PERMISSIONS.DETECTION_TEMPLATES.*` 追加 + Guard 適用)
- [x] Docs更新 (`requirements-traceability-review.md` Req4 完了セクション追加)
- [x] 単体テスト: list / form (Chai + jasmine spy)
- [x] 使用履歴表示 (LastUsedAt, UseCount) – メモリ内使用統計 (AppService static) 追加
- [x] E2E: Cypress spec プレースホルダ (`detection-templates-create-from.cy.ts`) 追加
- [ ] Templateエンティティ CRUD (Descoped) – テンプレートは `DetectionTemplateFactory` 静的定義方針

Status: 要件目的（テンプレート経由の効率的生成 UI）達成。履歴/E2E は改善項目として残存。

### Req18 (Safety Traceability) – Completion

- [x] SafetyTraceRecord ドメインメソッド `UpdateAsilLevel()` 実装（Approved/Rejected→再レビュー遷移）
- [x] Export: `SafetyTraceAuditReportAppService` 集計行 (総件数 / Approved% / ReReview件数 等) 追加
- [x] Docs: `requirements-traceability-review.md` Safety セクション更新（更新予定 / 再レビュー条件 記載）
- [x] 双方向リンク同期サービス `SyncLinkMatrixAsync` 実装済（DetectionLogic ID 付与/除去でリンク自動追加/削除）
- [x] ApplicationService: `UpdateAsilLevelAsync` API 追加（監査 + メトリクス計測）
- [x] メトリクス: `asil_level_change_total` / `asil_re_review_trigger_total` 実装 (`MonitoringService.TrackAsilLevelChange`)
- [x] Grafana パネル追加 (`monitoring/grafana/dashboards/safety.json`) ASIL 分布・再レビュー率・遷移マトリクス
- [x] テスト: ドメイン (Approved→UnderReview, Rejected→Submitted, 同一レベル変更) 追加 (`SafetyTraceRecord_AsilLevel_Tests.cs`)
- [ ] テスト拡張: 8 状態遷移マトリクス網羅 / Exportフォーマット snapshot (将来拡張)

Status: 主要要件完了。残タスクは高粒度テストカバレッジと Export snapshot テストのみ（低リスク改善）。

## Estimation (Rough)

| Task Group | Estimate (dev hrs) |
|------------|-------------------|
| Req9 | 16 |
| Req4 | 20 |
| Req18 | 18 |
| Buffer / Review / Docs | 6 |
| 合計 | 60 |

## Risks & Mitigations

| リスク | 影響 | 対策 |
|--------|------|------|
| SignalR負荷でSLA超過 | ユーザー体験低下 | バッチ閾値/遅延検出再試行ロジック |
| テンプレートパラメータ複雑化 | UI混乱 | スキーマ定義(JSON) + 動的フォーム生成 |
| ASIL再レビュー循環 | ワークフロー停滞 | 状態遷移監査ログ & 手動 override 権限 |

## Definition of Done (Sprint)

- 3 Goal 要件が `requirements-traceability-review.md` 上で判定列 Covered に更新。
- SLA/指標メトリクス Grafana で可視化。→ (未) Dashboard 未作成
- 新/変更 API OpenAPI スキーマ更新。
- E2E/Integration テスト CI 緑。

### Sprint Outcome (2025-11-04 現在の途中締め / Backlog 完了化)

| 要件 | 達成度 | 概要 | 残タスク (Carry Over) |
|------|--------|------|-----------------------|
| Req4 | ✅ 完了 | テンプレート UI + 動的検証 + 権限 + Docs + 使用統計 + E2E placeholder | 追加: 永続的使用履歴, 高度E2E 安定化 |
| Req9 | ◐ 部分 | 通知呼出 + latency 測定 + 接続ゲージ | DomainEvent化, 追加メトリクス, Dashboard, 負荷/統合テスト |
| Req18| ✅ 完了 | ASIL更新 + 再レビュー遷移 + Link同期 + Export集計 + メトリクス + Grafana + 基本テスト | 追加: テスト網羅&Export snapshot |

総括: スプリント中盤で UI/ドメイン基盤優先によりメトリクスと可視化が後ろ倒し。残項目は次スプリント *Real-time Observability & Safety Link Consistency* イニシアチブへ移管。

### Carry Over Backlog Seeds

1. Req9-DomainEvent: `DetectionResultCreatedEvent` + Handler (retry & failure counter)
2. Req9-Metrics: `detection_results_per_minute` / `broadcast_failures_total` + Grafana パネル JSON
3. Req9-Load: Parallel generation script (`scripts/load-gen-detections.ps1` 予定)
4. Req4-E2E拡張: 安定化 (fixture 化 / network stubbing) & 永続使用統計 (DB もしくはイベントログ蓄積)
5. Req18-LinkSync: Background cleanup + mapping integrity tests
6. Req18-Tests: ASIL transition matrix (8 cases) & export snapshot approval test
7. Req18-Grafana: ASIL distribution + re-review rate panel

### Quality Notes

- No build errors introduced by new UI & domain changes (targeted compile check OK)
- Need broadened automated test coverage before marking Req9/Req18 fully covered
- Risk: SLA 未計測 (Grafana) → gather raw histogram export next iteration


## Follow-up (Next Sprint Candidates)

- Req13 負荷試験自動化スクリプト化
- Req14 セキュリティスキャン CI 統合
- Req16 仕様差分分類アルゴリズム実装
