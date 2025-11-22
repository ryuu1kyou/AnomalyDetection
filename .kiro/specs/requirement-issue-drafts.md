# Requirement Issue Drafts (Generated 2025-11-04)

目的: 未完了/部分対応の要件を GitHub Issue 化するためのドラフト一覧。各項目は `.github/ISSUE_TEMPLATE/requirement-task.md` を用いて正式 Issue に転記。

## Legend

- Status: Partial / Missing / Covered-Enhance
- Sprint: Proposed target sprint (Current=Sprint Focus, Next=Next Sprint, Future=Later)

| Req | Title (Short) | Status | Sprint | Primary Gaps | Draft Issue Key Points |
|-----|---------------|--------|--------|--------------|------------------------|
| 1 | Multi-Tenant UI Completion | Partial | Future | テナント切替 UI 不完全 | UI 切替コンポーネント + E2E + 権限境界表示 |
| 2 | CAN Signal Import & Advanced Search | Partial | Next | CSV/DBC Import 未/検索高度化 | Importパーサ + 重複検出 + Prefix/全文検索 |
| 3 | Detection Logic Versioning & Execution Archive | Partial | Future | バージョン管理/結果保存未 | LogicVersion Entity + 実行結果履歴 + 比較API |
| 4 | Logic Template Management & History | Partial | Current | 管理UI/テスト不足 | CRUD UI + 適用履歴 + Cypress E2E |
| 6 | Inheritance & Compatibility Scoring | Partial | Next | 互換性分析アルゴリズム未 | 差分分類 + スコア算出 + 推奨提示 |
| 7 | Vehicle Phase Lifecycle & Comparison | Partial | Future | 履歴比較 UI/サービス未 | PhaseTransition 集約 + タイムライン表示 |
| 8 | Auth Audit & Unauthorized Action Log UI | Covered/Partial | Next | 監査 UI 未 | 操作履歴一覧 + フィルタ + Export |
| 9 | Real-time Backend Broadcast & SLA Metrics | Covered (Frontend)/Partial | Current | 判定サービス統合/SLA計測未 | DomainEvent→Hub + Prometheus + SLAテスト |
| 10 | Scheduled Statistics Generation | Covered | Next | スケジュール/5秒SLAテスト未 | Quartz/Hangfire Job + SLA計測 + Export確認 |
| 11 | Complete I18n Coverage & User Pref Persistence | Partial | Next | 未翻訳/ユーザー別保存未 | i18n抽出スクリプト + 言語設定保存 API |
| 12 | Integration APIs & Webhook Delivery | Partial | Next | 専用API/SLAテスト未 | Webhook送信 + Bulk Import + RateLimit + Retry |
| 13 | Performance & Scale Testing Suite | Partial/Missing | Future | 負荷/スケール試験未 | k6/Locust Scripts + 指標閾値 + レポート生成 |
| 14 | Security Hardening & Compliance Ops | Partial/Missing | Future | 暗号化/バックアップ/スキャン/GDPR 未 | Encryption層 + Backup Job + CIスキャン + GDPR処理 |
| 15 | Ops Metrics & 5-min Alerting | Partial | Next | 内部メトリクス/通知テスト未 | Custom Prometheus Exporter + Alert Rules |
| 16 | Latest CAN Spec Diff Impact Dashboard | Partial | Next | 高度差分分類/影響指標未 | Range/Freq分類 + 影響マッピング + Dashboard |
| 18 | Safety Trace Re-Review & Audit Export | Partial | Current | 再レビュー強制/監査レポート未 | UpdateAsilLevel Logic + Export + 双方向リンク |
| 19 | Cloud Migration Minimal Path | Partial | Future | サービス互換/手順未 | GCP Mapping Doc + Terraform Base + Readiness Checklist |
| 20 | Threshold Optimization Continuous Job | Covered/Partial | Future | 遅延SLA/継続学習未 | Scheduled再最適化 + Latency収集 + 指標比較 |

## Detailed Drafts

### Req9: Real-time Backend Broadcast & SLA Metrics

Use template. Key acceptance:

- Domain event triggers within detection service (<5ms overhead).
- Broadcast latency p95 < 100ms measured via Prometheus histogram.
- Failure counter increments on exception; retry policy (max 3).
- Grafana dashboard panels: connections, latency histogram, results per minute.

### Req4: Logic Template Management & History

Acceptance:

- CRUD endpoints secured with `DetectionTemplates.Manage` permission.
- Version auto-increment on update; LastUsedAt updates on create-from-template.
- Angular list filter by tag and text (debounced 300ms).
- Cypress: create -> generate logic -> edit -> delete passes.

### Req18: Safety Trace Re-Review & Audit Export

Acceptance:

- ASIL change Approved -> UnderReview transition enforced with event log.
- Export includes aggregate row and timestamp; PDF & CSV match column spec.
- Bidirectional link sync removes stale references within 1s of change.
- 8 transition tests (valid + invalid) all green.

### Req2: CAN Signal Import & Advanced Search

Acceptance:

- Import handles 1000-signal CSV in <5s.
- Duplicate detection returns conflict list with line numbers.
- Search: prefix (case-insensitive) & full-text over description using index.

### Req6: Inheritance & Compatibility Scoring

Acceptance:

- Diff classification (Layout/Timing/Range) achieves >90% test coverage.
- Score algorithm outputs 0-100 with thresholds for actions.

(Additional requirement drafts continue similarly...)

> After confirmation, each draft can be converted into a GitHub Issue using the template.
