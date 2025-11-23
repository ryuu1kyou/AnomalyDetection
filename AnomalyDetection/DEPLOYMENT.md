# デプロイメントガイド - CAN異常検出管理システム

## 目次

1. [環境変数設定](#環境変数設定)
2. [本番環境設定](#本番環境設定)
3. [データベース設定](#データベース設定)
4. [セキュリティ設定](#セキュリティ設定)
5. [デプロイ手順](#デプロイ手順)
6. [ヘルスチェック](#ヘルスチェック)
7. [トラブルシューティング](#トラブルシューティング)
8. [デプロイメントチェックリスト](#デプロイメントチェックリスト)
9. [実装サマリー](#実装サマリー)

---

## 環境変数設定

### バックエンド環境変数

本番環境では、以下の環境変数を設定する必要があります。

#### 必須環境変数

```bash
# データベース接続
DB_SERVER=your-sql-server.database.windows.net
DB_NAME=AnomalyDetection_Production
DB_USER=anomalydetection_admin
DB_PASSWORD=<strong-password>

# 認証・暗号化
CERT_PASSPHRASE=<certificate-passphrase>
ENCRYPTION_PASSPHRASE=<encryption-passphrase>

# SSL証明書
SSL_CERT_PATH=/app/certs/certificate.pfx
SSL_CERT_PASSWORD=<ssl-cert-password>
```

#### オプション環境変数

```bash
# Redis (キャッシュ・分散ロック)
REDIS_CONNECTION_STRING=your-redis-server:6379,password=<redis-password>,ssl=true

# RabbitMQ (メッセージング)
RABBITMQ_HOST=your-rabbitmq-server
RABBITMQ_PORT=5672
RABBITMQ_USER=anomalydetection
RABBITMQ_PASSWORD=<rabbitmq-password>

# Elasticsearch (ログ集約)
ELASTICSEARCH_URL=https://your-elasticsearch-server:9200

# アプリケーション設定
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80;https://+:443
```

### フロントエンド環境変数

Angular アプリケーションの環境変数は、`environment.prod.ts` で設定します。

```typescript
// 本番環境URL
const baseUrl = 'https://app.anomalydetection.example.com';
const apiUrl = 'https://api.anomalydetection.example.com';
```

ビルド時に環境変数を使用する場合：

```bash
# ビルド時の環境変数
export NG_APP_BASE_URL=https://app.anomalydetection.example.com
export NG_APP_API_URL=https://api.anomalydetection.example.com

# ビルド実行
npm run build:prod
```

---

## 本番環境設定

### appsettings.Production.json

バックエンドの本番環境設定ファイル：

```json
{
  "App": {
    "SelfUrl": "https://api.anomalydetection.example.com",
    "AngularUrl": "https://app.anomalydetection.example.com",
    "CorsOrigins": "https://app.anomalydetection.example.com",
    "DisablePII": true
  },
  "ConnectionStrings": {
    "Default": "Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD}"
  }
}
```

### environment.prod.ts

フロントエンドの本番環境設定ファイル：

```typescript
export const environment = {
  production: true,
  application: {
    baseUrl: 'https://app.anomalydetection.example.com',
    name: 'CAN Anomaly Detection System',
  },
  apis: {
    default: {
      url: 'https://api.anomalydetection.example.com',
    },
  },
};
```

---

## データベース設定

### SQL Server 本番環境設定

#### 接続文字列

```
Server=your-sql-server.database.windows.net;
Database=AnomalyDetection_Production;
User Id=anomalydetection_admin;
Password=<strong-password>;
TrustServerCertificate=false;
Encrypt=true;
Connection Timeout=30;
```

#### データベース初期化

```bash
# マイグレーション実行
cd AnomalyDetection/src/AnomalyDetection.DbMigrator
dotnet run --environment Production

# または Docker で実行
docker run --rm \
  -e ConnectionStrings__Default="<connection-string>" \
  anomalydetection-dbmigrator:latest
```

#### バックアップ設定

```sql
-- 自動バックアップ設定 (Azure SQL Database)
-- Azure Portal で設定

-- オンプレミス SQL Server の場合
BACKUP DATABASE AnomalyDetection_Production
TO DISK = '/backup/AnomalyDetection_Production_Full.bak'
WITH FORMAT, INIT, COMPRESSION;
```

### マルチテナント データベース設定

各テナント（OEM）ごとに独立したデータベースを使用する場合：

```json
{
  "ConnectionStrings": {
    "Default": "Server=${DB_SERVER};Database=AnomalyDetection_Host;...",
    "Tenant_Toyota": "Server=${DB_SERVER};Database=AnomalyDetection_Toyota;...",
    "Tenant_Honda": "Server=${DB_SERVER};Database=AnomalyDetection_Honda;...",
    "Tenant_Nissan": "Server=${DB_SERVER};Database=AnomalyDetection_Nissan;..."
  }
}
```

---

## セキュリティ設定

### SSL/TLS 証明書

#### 証明書の配置

```bash
# 証明書ファイルを配置
mkdir -p /app/certs
cp certificate.pfx /app/certs/
chmod 600 /app/certs/certificate.pfx
```

#### Kestrel SSL 設定

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/app/certs/certificate.pfx",
          "Password": "${SSL_CERT_PASSWORD}"
        }
      }
    }
  }
}
```

### CORS 設定

```json
{
  "App": {
    "CorsOrigins": "https://app.anomalydetection.example.com,https://*.anomalydetection.example.com"
  }
}
```

### 認証設定

```json
{
  "AuthServer": {
    "Authority": "https://api.anomalydetection.example.com",
    "RequireHttpsMetadata": true,
    "SwaggerClientId": "AnomalyDetection_Swagger"
  }
}
```

### セキュリティヘッダー

```csharp
// Program.cs または Startup.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
    context.Response.Headers.Add("Content-Security-Policy", 
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");
    await next();
});
```

---

## デプロイ手順

### Docker デプロイ

#### 1. イメージビルド

```bash
# バックエンド
cd AnomalyDetection/src/AnomalyDetection.HttpApi.Host
docker build -t anomalydetection-api:latest -f Dockerfile .

# フロントエンド
cd AnomalyDetection/angular
docker build -t anomalydetection-web:latest -f Dockerfile .

# DbMigrator
cd AnomalyDetection/src/AnomalyDetection.DbMigrator
docker build -t anomalydetection-dbmigrator:latest -f Dockerfile .
```

#### 2. Docker Compose デプロイ

```bash
# 本番環境用 docker-compose
docker-compose -f docker-compose.prod.yml up -d

# ログ確認
docker-compose -f docker-compose.prod.yml logs -f

# ヘルスチェック
curl https://api.anomalydetection.example.com/health-status
```

### Kubernetes デプロイ

#### 1. シークレット作成

```bash
# データベース接続文字列
kubectl create secret generic db-connection \
  --from-literal=connection-string="Server=...;Database=...;User Id=...;Password=..."

# SSL証明書
kubectl create secret tls ssl-cert \
  --cert=certificate.crt \
  --key=certificate.key
```

#### 2. デプロイメント適用

```bash
# デプロイメント
kubectl apply -f k8s/deployment.yaml

# サービス
kubectl apply -f k8s/service.yaml

# Ingress
kubectl apply -f k8s/ingress.yaml

# 状態確認
kubectl get pods
kubectl get services
kubectl get ingress
```

### Azure App Service デプロイ

#### 1. リソース作成

```bash
# リソースグループ
az group create --name anomalydetection-rg --location japaneast

# App Service Plan
az appservice plan create \
  --name anomalydetection-plan \
  --resource-group anomalydetection-rg \
  --sku P1V2 \
  --is-linux

# Web App (API)
az webapp create \
  --name anomalydetection-api \
  --resource-group anomalydetection-rg \
  --plan anomalydetection-plan \
  --runtime "DOTNETCORE:10.0"

# Web App (Frontend)
az webapp create \
  --name anomalydetection-web \
  --resource-group anomalydetection-rg \
  --plan anomalydetection-plan \
  --runtime "NODE:18-lts"
```

#### 2. 環境変数設定

```bash
# API 環境変数
az webapp config appsettings set \
  --name anomalydetection-api \
  --resource-group anomalydetection-rg \
  --settings \
    DB_SERVER="your-sql-server.database.windows.net" \
    DB_NAME="AnomalyDetection_Production" \
    DB_USER="anomalydetection_admin" \
    DB_PASSWORD="<password>"
```

#### 3. デプロイ

```bash
# API デプロイ
cd AnomalyDetection/src/AnomalyDetection.HttpApi.Host
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip \
  --name anomaly detection-api \
  --resource-group anomalydetection-rg \
  --src ./publish.zip

# Frontend デプロイ
cd AnomalyDetection/angular
npm run build:prod
az webapp deployment source config-zip \
  --name anomalydetection-web \
  --resource-group anomalydetection-rg \
  --src ./dist.zip
```

---

## ヘルスチェック

### エンドポイント

```
GET https://api.anomalydetection.example.com/health-status
```

### レスポンス例

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0123456"
    },
    "redis": {
      "status": "Healthy",
      "duration": "00:00:00.0098765"
    },
    "rabbitmq": {
      "status": "Healthy",
      "duration": "00:00:00.0087654"
    }
  }
}
```

### ヘルスチェック UI

```
https://api.anomalydetection.example.com/health-ui
```

### 監視スクリプト

```bash
#!/bin/bash
# health-check.sh

API_URL="https://api.anomalydetection.example.com/health-status"
SLACK_WEBHOOK="https://hooks.slack.com/services/YOUR/WEBHOOK/URL"

response=$(curl -s -o /dev/null -w "%{http_code}" $API_URL)

if [ $response -ne 200 ]; then
  message="⚠️ API Health Check Failed! Status: $response"
  curl -X POST -H 'Content-type: application/json' \
    --data "{\"text\":\"$message\"}" \
    $SLACK_WEBHOOK
fi
```

---

## トラブルシューティング

### データベース接続エラー

**症状**: `Cannot connect to SQL Server`

**解決方法**:
1. 接続文字列を確認
2. ファイアウォール設定を確認
3. SQL Server が起動しているか確認

```bash
# 接続テスト
sqlcmd -S your-sql-server.database.windows.net -U anomalydetection_admin -P <password> -Q "SELECT 1"
```

### 認証エラー

**症状**: `401 Unauthorized`

**解決方法**:
1. OAuth 設定を確認
2. 証明書が正しく配置されているか確認
3. CORS 設定を確認

```bash
# 証明書確認
openssl x509 -in certificate.crt -text -noout

# CORS テスト
curl -H "Origin: https://app.anomalydetection.example.com" \
  -H "Access-Control-Request-Method: GET" \
  -H "Access-Control-Request-Headers: X-Requested-With" \
  -X OPTIONS --verbose \
  https://api.anomalydetection.example.com/api/app/can-signals
```

### パフォーマンス問題

**症状**: レスポンスが遅い

**解決方法**:
1. データベースインデックスを確認
2. Redis キャッシュが有効か確認
3. アプリケーションログを確認

```bash
# ログ確認
docker logs anomalydetection-api --tail 100

# パフォーマンスプロファイリング
dotnet-trace collect --process-id <pid>
```

### メモリリーク

**症状**: メモリ使用量が増加し続ける

**解決方法**:
1. メモリダンプを取得
2. ガベージコレクションログを確認
3. アプリケーションを再起動

```bash
# メモリダンプ取得
dotnet-dump collect --process-id <pid>

# メモリダンプ分析
dotnet-dump analyze <dump-file>
```

---

## デプロイメントチェックリスト

### デプロイメント前チェックリスト

#### 1. 環境準備

##### 1.1 インフラストラクチャ
- [ ] サーバーリソースの確認（CPU、メモリ、ディスク容量）
- [ ] ネットワーク設定の確認（ファイアウォール、ロードバランサー）
- [ ] SSL証明書の有効性確認
- [ ] DNS設定の確認
- [ ] バックアップシステムの動作確認

##### 1.2 データベース
- [ ] データベースサーバーの稼働確認
- [ ] データベース接続文字列の確認
- [ ] マイグレーションスクリプトの準備
- [ ] データベースバックアップの実行
- [ ] インデックス最適化の実行

##### 1.3 外部サービス
- [ ] Redis接続の確認
- [ ] SMTP設定の確認
- [ ] 外部API接続の確認
- [ ] 監視サービスの設定確認

#### 2. アプリケーション準備

##### 2.1 ビルド・テスト
- [ ] 最新コードのビルド成功確認
- [ ] 単体テストの実行・成功確認
- [ ] 統合テストの実行・成功確認
- [ ] E2Eテストの実行・成功確認
- [ ] セキュリティスキャンの実行・問題なし確認

##### 2.2 設定ファイル
- [ ] 本番環境用設定ファイルの準備
- [ ] 環境変数の設定確認
- [ ] ログレベルの設定確認
- [ ] パフォーマンス設定の確認

##### 2.3 Docker イメージ
- [ ] Dockerイメージのビルド成功確認
- [ ] イメージサイズの最適化確認
- [ ] セキュリティスキャンの実行
- [ ] コンテナレジストリへのプッシュ確認

#### 3. セキュリティ

##### 3.1 認証・認可
- [ ] 認証設定の確認
- [ ] 権限設定の確認
- [ ] API キーの設定確認
- [ ] JWT設定の確認

##### 3.2 ネットワークセキュリティ
- [ ] HTTPS設定の確認
- [ ] セキュリティヘッダーの設定確認
- [ ] CORS設定の確認
- [ ] ファイアウォール設定の確認

### デプロイメント実行チェックリスト

#### 1. デプロイメント手順

##### 1.1 事前準備
- [ ] メンテナンス通知の送信
- [ ] 現在のバージョンの記録
- [ ] ロールバック手順の確認
- [ ] 関係者への通知

##### 1.2 デプロイメント実行
- [ ] データベースマイグレーションの実行
- [ ] アプリケーションのデプロイ
- [ ] 設定ファイルの更新
- [ ] サービスの再起動

##### 1.3 即座の確認
- [ ] アプリケーションの起動確認
- [ ] ヘルスチェックの成功確認
- [ ] ログエラーの確認
- [ ] 基本機能の動作確認

### デプロイメント後チェックリスト

#### 1. 機能テスト

##### 1.1 スモークテスト
```powershell
# スモークテスト実行
.\scripts\smoke-test.ps1 -BaseUrl "https://your-domain.com"
```

- [ ] フロントエンドアクセス確認
- [ ] API エンドポイント確認
- [ ] 認証機能確認
- [ ] データベース接続確認

##### 1.2 主要機能テスト
- [ ] ユーザーログイン・ログアウト
- [ ] CAN信号管理機能
- [ ] 異常検出ロジック管理機能
- [ ] プロジェクト管理機能
- [ ] OEMトレーサビリティ機能
- [ ] 類似パターン検索機能
- [ ] 異常分析機能

#### 2. システム監視

##### 2.1 アプリケーション監視
- [ ] CPU使用率の確認
- [ ] メモリ使用率の確認
- [ ] ディスク使用率の確認
- [ ] ネットワーク使用率の確認

##### 2.2 アプリケーションメトリクス
- [ ] レスポンスタイムの確認
- [ ] エラー率 の確認
- [ ] スループットの確認
- [ ] アクティブユーザー数の確認

##### 2.3 ログ監視
- [ ] アプリケーションログの確認
- [ ] エラーログの確認
- [ ] セキュリティログの確認
- [ ] パフォーマンスログの確認

#### 3. データ整合性

##### 3.1 データベース
- [ ] データマイグレーションの成功確認
- [ ] データ整合性の確認
- [ ] インデックスの確認
- [ ] パフォーマンスの確認

##### 3.2 キャッシュ
- [ ] Redisキャッシュの動作確認
- [ ] キャッシュヒット率の確認
- [ ] キャッシュ無効化の動作確認

#### 4. セキュリティ確認

##### 4.1 アクセス制御
- [ ] 認証機能の動作確認
- [ ] 権限制御の動作確認
- [ ] セッション管理の確認
- [ ]  API セキュリティの確認

##### 4.2 ネットワークセキュリティ
- [ ] HTTPS通信の確認
- [ ] セキュリティヘッダーの確認
- [ ] 不正アクセス検知の確認

---

## 実装サマリー

### 実装完了機能

#### 1. Docker設定 ✅

##### コンテナ構成
- **Backend**: ABP vNext Web API (.NET 10.0 LTS)
- **Frontend**: Angular 17 + Nginx
- **Database**: SQL Server 2022
- **Cache**: Redis 7
- **Reverse Proxy**: Nginx (本番環境)

##### 設定ファイル
- `docker-compose.yml` - 開発環境用
- `docker-compose.prod.yml` - 本番環境用
- `docker-compose.monitoring.yml` - 監視スタック用

#### 2. CI/CD パイプライン ✅

##### GitHub Actions ワークフロー
- **メインCI/CD** (`.github/workflows/ci-cd.yml`)
- **セキュリティスキャン** (`.github/workflows/security-scan.yml`)
- **パフォーマンステスト** (`.github/workflows/performance-test.yml`)

##### Azure DevOps パイプライン
- `azure-pipelines.yml` - 企業環境向け設定

#### 3. 監視システム ✅

##### メトリクス収集
- **Prometheus** - メトリクス収集・保存
- **Grafana** - 可視化ダッシュボード
- **Application Insights** - APM監視

##### ログ管理
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Filebeat** - ログ収集
- 構造化ログ出力

### 使用方法

#### 開発環境セットアップ

```powershell
# 開発環境の起動
.\docker-setup.ps1

# ヘルスチェック実行
.\scripts\docker-health-check.ps1
```

#### 本番環境デプロイ

```powershell
# 本番環境設定
.\docker-setup.ps1 -Environment prod

# デプロイメントテスト
.\scripts\deployment-test.ps1 -Environment production -BaseUrl "https://your-domain.com"
```

#### 監視システム起動

```powershell
# 監視スタック起動
.\scripts\setup-monitoring.ps1

# アクセス先
# Grafana: http://localhost:3000
# Prometheus: http://localhost:9090
# Alertmanager: http://localhost:9093
```

---

## サポート

問題が発生した場合は、以下の情報を含めてサポートチームに連絡してください：

1. エラーメッセージ
2. アプリケーションログ
3. 環境情報 (OS, .NET バージョン、データベースバージョン)
4. 再現手順

**サポート連絡先**: support@anomalydetection.example.com

---

**実装完了**: 2024年10月28日  
**最終更新**: 2025年11月23日
