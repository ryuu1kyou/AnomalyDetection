# CI/CD セットアップガイド - CAN異常検出管理システム

## 概要

このドキュメントでは、CAN異常検出管理システムのCI/CDパイプライン設定について説明します。
GitHub ActionsとAzure DevOpsの両方に対応しており、自動ビルド、テスト、デプロイメントを提供します。

## アーキテクチャ

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Source Code   │    │   CI Pipeline   │    │  CD Pipeline    │
│   (GitHub)      │───►│  (Build/Test)   │───►│   (Deploy)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       ▼                       ▼
         │              ┌─────────────────┐    ┌─────────────────┐
         │              │  Artifacts      │    │  Environments   │
         │              │  (Images/Pkg)   │    │  (Stg/Prod)     │
         │              └─────────────────┘    └─────────────────┘
         │
         ▼
┌─────────────────┐
│   Quality Gates │
│  (Tests/Scans)  │
└─────────────────┘
```

## パイプライン構成

### 1. GitHub Actions ワークフロー

#### 1.1 メインCI/CDパイプライン (`.github/workflows/ci-cd.yml`)

**トリガー条件**:
- `main`, `develop` ブランチへのプッシュ
- `main` ブランチへのプルリクエスト

**ジョブ構成**:
1. **backend-build-test**: バックエンドのビルドとテスト
2. **frontend-build-test**: フロントエンドのビルドとテスト
3. **security-scan**: セキュリティスキャン
4. **e2e-test**: E2Eテスト
5. **build-images**: Dockerイメージビルド
6. **deploy-staging**: Stagingデプロイ
7. **deploy-production**: Productionデプロイ

#### 1.2 セキュリティスキャン (`.github/workflows/security-scan.yml`)

**トリガー条件**:
- 毎日午前2時（スケジュール実行）
- 手動実行
- 依存関係ファイルの変更

**スキャン内容**:
- .NET依存関係の脆弱性チェック
- Node.js依存関係の脆弱性チェック
- Trivyによるファイルシステムスキャン
- CodeQL静的解析
- Dockerイメージスキャン

#### 1.3 パフォーマンステスト (`.github/workflows/performance-test.yml`)

**トリガー条件**:
- 毎週日曜日午前3時（スケジュール実行）
- 手動実行（パラメータ指定可能）

**テスト内容**:
- k6を使用した負荷テスト
- レスポンスタイム測定
- エラー率測定
- パフォーマンス閾値チェック

### 2. Azure DevOps パイプライン (`azure-pipelines.yml`)

**ステージ構成**:
1. **Build**: ビルドとテスト
2. **E2ETest**: E2Eテスト
3. **BuildImages**: Dockerイメージビルド
4. **DeployStaging**: Stagingデプロイ
5. **DeployProduction**: Productionデプロイ

## セットアップ手順

### 1. GitHub Actions セットアップ

#### 1.1 必要なシークレット設定

GitHub リポジトリの Settings > Secrets and variables > Actions で以下を設定:

```bash
# Container Registry
GITHUB_TOKEN  # 自動設定済み

# Slack通知
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK

# Azure (オプション)
AZURE_CREDENTIALS='{
  "clientId": "your-client-id",
  "clientSecret": "your-client-secret",
  "subscriptionId": "your-subscription-id",
  "tenantId": "your-tenant-id"
}'
```

#### 1.2 環境設定

GitHub リポジトリの Settings > Environments で以下の環境を作成:

- **staging**: Staging環境用（自動デプロイ）
- **production**: Production環境用（承認必須）

#### 1.3 ブランチ保護ルール

Settings > Branches で `main` ブランチに以下のルールを設定:

- Require a pull request before merging
- Require status checks to pass before merging
  - `backend-build-test`
  - `frontend-build-test`
  - `security-scan`
- Require branches to be up to date before merging
- Restrict pushes that create files larger than 100MB

### 2. Azure DevOps セットアップ

#### 2.1 サービス接続設定

Azure DevOps プロジェクトで以下のサービス接続を作成:

```yaml
# Container Registry
anomaly-detection-acr:
  type: Azure Container Registry
  registry: anomalydetection.azurecr.io

# Azure Subscription
anomaly-detection-subscription:
  type: Azure Resource Manager
  subscription: your-subscription-id

# Slack
slack-webhook:
  type: Incoming Webhook
  url: https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK
```

#### 2.2 変数グループ設定

Library > Variable groups で以下のグループを作成:

**anomaly-detection-common**:
```yaml
buildConfiguration: Release
dotnetVersion: 9.0.x
nodeVersion: 18.x
containerRegistry: anomalydetection.azurecr.io
imageRepository: anomaly-detection
```

**anomaly-detection-staging**:
```yaml
STAGING_CONNECTION_STRING: Server=staging-sql;Database=AnomalyDetection;...
STAGING_REDIS_CONNECTION: staging-redis:6379
```

**anomaly-detection-production**:
```yaml
PRODUCTION_CONNECTION_STRING: Server=prod-sql;Database=AnomalyDetection;...
PRODUCTION_REDIS_CONNECTION: prod-redis:6379
```

### 3. 品質ゲート設定

#### 3.1 テストカバレッジ要件

- バックエンド: 80%以上
- フロントエンド: 70%以上

#### 3.2 セキュリティ要件

- 高リスク脆弱性: 0件
- 中リスク脆弱性: 5件以下

#### 3.3 パフォーマンス要件

- 平均レスポンスタイム: 2秒以下
- エラー率: 10%以下

## デプロイメント戦略

### 1. ブランチ戦略

```
main (本番)
├── develop (開発)
├── feature/* (機能開発)
└── hotfix/* (緊急修正)
```

### 2. デプロイメントフロー

#### 2.1 開発環境
- `develop` ブランチへのプッシュで自動デプロイ
- 開発者による機能テスト

#### 2.2 Staging環境
- `main` ブランチへのマージで自動デプロイ
- E2Eテスト実行
- ステークホルダーによる受け入れテスト

#### 2.3 Production環境
- Staging環境でのテスト完了後
- 手動承認が必要
- Blue-Greenデプロイメント

### 3. ロールバック戦略

#### 3.1 自動ロールバック条件
- ヘルスチェック失敗
- エラー率が閾値を超過
- レスポンスタイムが閾値を超過

#### 3.2 手動ロールバック
```bash
# GitHub Container Registry から前のバージョンをデプロイ
kubectl set image deployment/backend-deployment \
  backend=ghcr.io/your-org/anomaly-detection-backend:previous-tag
```

## 監視とアラート

### 1. パイプライン監視

#### 1.1 成功率監視
- ビルド成功率: 95%以上
- デプロイ成功率: 98%以上

#### 1.2 実行時間監視
- CI実行時間: 15分以内
- CD実行時間: 30分以内

### 2. 通知設定

#### 2.1 Slack通知
- ビルド失敗
- デプロイ完了
- セキュリティアラート
- パフォーマンス劣化

#### 2.2 メール通知
- 本番デプロイ完了
- 緊急セキュリティアラート

## トラブルシューティング

### 1. よくある問題

#### 1.1 ビルド失敗
```bash
# ローカルでの確認
dotnet build AnomalyDetection.sln --configuration Release
cd angular && yarn build:prod

# 依存関係の問題
dotnet restore --force
yarn install --frozen-lockfile
```

#### 1.2 テスト失敗
```bash
# ローカルでのテスト実行
dotnet test --configuration Release --logger trx
cd angular && yarn test:ci

# データベース接続問題
docker-compose up -d sqlserver
```

#### 1.3 デプロイ失敗
```bash
# コンテナログ確認
kubectl logs deployment/backend-deployment
kubectl logs deployment/frontend-deployment

# ヘルスチェック確認
curl http://your-app/health-status
```

### 2. パフォーマンス最適化

#### 2.1 ビルド時間短縮
- Docker layer キャッシュ活用
- 並列ビルド実行
- 不要なファイル除外

#### 2.2 テスト時間短縮
- テストの並列実行
- 統合テストの最適化
- キャッシュ活用

## セキュリティベストプラクティス

### 1. シークレット管理
- GitHub Secrets / Azure Key Vault使用
- 最小権限の原則
- 定期的なローテーション

### 2. イメージセキュリティ
- 最小限のベースイメージ使用
- 定期的な脆弱性スキャン
- イメージ署名

### 3. アクセス制御
- ブランチ保護ルール
- 承認プロセス
- 監査ログ

## 参考リンク

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure DevOps Documentation](https://docs.microsoft.com/en-us/azure/devops/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Kubernetes Deployment Strategies](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)