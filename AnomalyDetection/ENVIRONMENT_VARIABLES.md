# 環境変数リファレンス - CAN異常検出管理システム

このドキュメントでは、CAN異常検出管理システムで使用するすべての環境変数について説明します。

## 目次

1. [必須環境変数](#必須環境変数)
2. [オプション環境変数](#オプション環境変数)
3. [環境別設定](#環境別設定)
4. [セキュリティのベストプラクティス](#セキュリティのベストプラクティス)
5. [環境変数の設定方法](#環境変数の設定方法)

---

## 必須環境変数

これらの環境変数は、本番環境で必ず設定する必要があります。

### データベース接続

| 変数名 | 説明 | 例 | 必須 |
|--------|------|-----|------|
| `DB_SERVER` | SQL Serverのホスト名またはIPアドレス | `your-sql-server.database.windows.net` | ✅ |
| `DB_NAME` | データベース名 | `AnomalyDetection_Production` | ✅ |
| `DB_USER` | データベースユーザー名 | `anomalydetection_admin` | ✅ |
| `DB_PASSWORD` | データベースパスワード | `YourStrongPassword123!` | ✅ |

**設定例:**
```bash
export DB_SERVER="your-sql-server.database.windows.net"
export DB_NAME="AnomalyDetection_Production"
export DB_USER="anomalydetection_admin"
export DB_PASSWORD="YourStrongPassword123!"
```

**Docker Compose:**
```yaml
environment:
  - DB_SERVER=your-sql-server.database.windows.net
  - DB_NAME=AnomalyDetection_Production
  - DB_USER=anomalydetection_admin
  - DB_PASSWORD=${DB_PASSWORD}
```

### 認証・暗号化

| 変数名 | 説明 | 例 | 必須 |
|--------|------|-----|------|
| `CERT_PASSPHRASE` | 認証証明書のパスフレーズ | `YourCertPassphrase123!` | ✅ |
| `ENCRYPTION_PASSPHRASE` | データ暗号化のパスフレーズ | `YourEncryptionKey123!` | ✅ |

**重要:** これらのパスフレーズは以下の要件を満たす必要があります：
- 最低16文字以上
- 大文字、小文字、数字、特殊文字を含む
- 推測されにくいランダムな文字列

**生成方法:**
```bash
# Linux/Mac
openssl rand -base64 32

# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

### SSL/TLS証明書

| 変数名 | 説明 | 例 | 必須 |
|--------|------|-----|------|
| `SSL_CERT_PATH` | SSL証明書ファイルのパス | `/app/certs/certificate.pfx` | ✅ |
| `SSL_CERT_PASSWORD` | SSL証明書のパスワード | `YourCertPassword123!` | ✅ |

**証明書の準備:**
```bash
# PFX形式の証明書を作成
openssl pkcs12 -export \
  -out certificate.pfx \
  -inkey private.key \
  -in certificate.crt \
  -certfile ca-bundle.crt \
  -password pass:YourCertPassword123!
```

---

## オプション環境変数

これらの環境変数は、追加機能を有効にする場合に設定します。

### Redis (キャッシュ・分散ロック)

| 変数名 | 説明 | デフォルト | 例 |
|--------|------|-----------|-----|
| `REDIS_CONNECTION_STRING` | Redis接続文字列 | なし | `your-redis:6379,password=xxx,ssl=true` |

**設定例:**
```bash
export REDIS_CONNECTION_STRING="your-redis-server:6379,password=YourRedisPassword123!,ssl=true,abortConnect=false"
```

**接続文字列オプション:**
- `ssl=true`: SSL/TLS接続を有効化
- `abortConnect=false`: 接続失敗時にアプリケーションを停止しない
- `connectTimeout=5000`: 接続タイムアウト（ミリ秒）
- `syncTimeout=5000`: 同期操作タイムアウト（ミリ秒）

### RabbitMQ (メッセージング)

| 変数名 | 説明 | デフォルト | 例 |
|--------|------|-----------|-----|
| `RABBITMQ_HOST` | RabbitMQホスト名 | なし | `your-rabbitmq-server` |
| `RABBITMQ_PORT` | RabbitMQポート | `5672` | `5672` |
| `RABBITMQ_USER` | RabbitMQユーザー名 | なし | `anomalydetection` |
| `RABBITMQ_PASSWORD` | RabbitMQパスワード | なし | `YourRabbitMQPassword123!` |

**設定例:**
```bash
export RABBITMQ_HOST="your-rabbitmq-server"
export RABBITMQ_PORT="5672"
export RABBITMQ_USER="anomalydetection"
export RABBITMQ_PASSWORD="YourRabbitMQPassword123!"
```

### Elasticsearch (ログ集約)

| 変数名 | 説明 | デフォルト | 例 |
|--------|------|-----------|-----|
| `ELASTICSEARCH_URL` | ElasticsearchエンドポイントURL | なし | `https://your-elasticsearch:9200` |
| `ELASTICSEARCH_USER` | Elasticsearchユーザー名 | なし | `elastic` |
| `ELASTICSEARCH_PASSWORD` | Elasticsearchパスワード | なし | `YourElasticPassword123!` |

**設定例:**
```bash
export ELASTICSEARCH_URL="https://your-elasticsearch-server:9200"
export ELASTICSEARCH_USER="elastic"
export ELASTICSEARCH_PASSWORD="YourElasticPassword123!"
```

### Application Insights (監視)

| 変数名 | 説明 | デフォルト | 例 |
|--------|------|-----------|-----|
| `APPINSIGHTS_INSTRUMENTATIONKEY` | Application Insights計測キー | なし | `12345678-1234-1234-1234-123456789abc` |

**設定例:**
```bash
export APPINSIGHTS_INSTRUMENTATIONKEY="12345678-1234-1234-1234-123456789abc"
```

### ASP.NET Core 設定

| 変数名 | 説明 | デフォルト | 例 |
|--------|------|-----------|-----|
| `ASPNETCORE_ENVIRONMENT` | 実行環境 | `Production` | `Production` |
| `ASPNETCORE_URLS` | リスニングURL | `http://+:80` | `http://+:80;https://+:443` |

**設定例:**
```bash
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://+:80;https://+:443"
```

---

## 環境別設定

### 開発環境 (Development)

```bash
# データベース
export DB_SERVER="(LocalDb)\\MSSQLLocalDB"
export DB_NAME="AnomalyDetection"
export DB_USER=""
export DB_PASSWORD=""

# 認証・暗号化（開発用）
export CERT_PASSPHRASE="dev-cert-passphrase"
export ENCRYPTION_PASSPHRASE="dev-encryption-key"

# ASP.NET Core
export ASPNETCORE_ENVIRONMENT="Development"
export ASPNETCORE_URLS="https://localhost:44318"
```

### ステージング環境 (Staging)

```bash
# データベース
export DB_SERVER="staging-sql-server.database.windows.net"
export DB_NAME="AnomalyDetection_Staging"
export DB_USER="anomalydetection_staging"
export DB_PASSWORD="StagingPassword123!"

# 認証・暗号化
export CERT_PASSPHRASE="staging-cert-passphrase"
export ENCRYPTION_PASSPHRASE="staging-encryption-key"

# SSL証明書
export SSL_CERT_PATH="/app/certs/staging-certificate.pfx"
export SSL_CERT_PASSWORD="StagingCertPassword123!"

# Redis
export REDIS_CONNECTION_STRING="staging-redis:6379,password=StagingRedisPassword123!,ssl=true"

# ASP.NET Core
export ASPNETCORE_ENVIRONMENT="Staging"
export ASPNETCORE_URLS="http://+:80;https://+:443"
```

### 本番環境 (Production)

```bash
# データベース
export DB_SERVER="production-sql-server.database.windows.net"
export DB_NAME="AnomalyDetection_Production"
export DB_USER="anomalydetection_prod"
export DB_PASSWORD="ProductionPassword123!"

# 認証・暗号化
export CERT_PASSPHRASE="production-cert-passphrase"
export ENCRYPTION_PASSPHRASE="production-encryption-key"

# SSL証明書
export SSL_CERT_PATH="/app/certs/production-certificate.pfx"
export SSL_CERT_PASSWORD="ProductionCertPassword123!"

# Redis
export REDIS_CONNECTION_STRING="production-redis:6379,password=ProductionRedisPassword123!,ssl=true"

# RabbitMQ
export RABBITMQ_HOST="production-rabbitmq-server"
export RABBITMQ_PORT="5672"
export RABBITMQ_USER="anomalydetection"
export RABBITMQ_PASSWORD="ProductionRabbitMQPassword123!"

# Elasticsearch
export ELASTICSEARCH_URL="https://production-elasticsearch:9200"
export ELASTICSEARCH_USER="elastic"
export ELASTICSEARCH_PASSWORD="ProductionElasticPassword123!"

# Application Insights
export APPINSIGHTS_INSTRUMENTATIONKEY="12345678-1234-1234-1234-123456789abc"

# ASP.NET Core
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://+:80;https://+:443"
```

---

## セキュリティのベストプラクティス

### 1. 環境変数の保護

**❌ 避けるべき方法:**
```bash
# コードに直接記述
export DB_PASSWORD="MyPassword123!"

# バージョン管理にコミット
git add .env
git commit -m "Add environment variables"
```

**✅ 推奨される方法:**
```bash
# シークレット管理サービスを使用
# Azure Key Vault
az keyvault secret set --vault-name my-vault --name DB-PASSWORD --value "MyPassword123!"

# AWS Secrets Manager
aws secretsmanager create-secret --name DB_PASSWORD --secret-string "MyPassword123!"

# Kubernetes Secrets
kubectl create secret generic db-credentials \
  --from-literal=password='MyPassword123!'
```

### 2. パスワードの要件

すべてのパスワードは以下の要件を満たす必要があります：

- **最低文字数**: 16文字以上
- **複雑性**: 大文字、小文字、数字、特殊文字を含む
- **ランダム性**: 辞書にない、推測されにくい文字列
- **一意性**: 各環境・サービスで異なるパスワードを使用

**パスワード生成例:**
```bash
# Linux/Mac
openssl rand -base64 24

# PowerShell
Add-Type -AssemblyName System.Web
[System.Web.Security.Membership]::GeneratePassword(24, 8)
```

### 3. 環境変数のローテーション

定期的に環境変数（特にパスワード）をローテーションします：

- **データベースパスワード**: 90日ごと
- **証明書**: 有効期限の30日前
- **APIキー**: 180日ごと

### 4. アクセス制御

環境変数へのアクセスを制限します：

```bash
# ファイルパーミッション
chmod 600 .env

# 所有者のみ読み書き可能
chown root:root .env
```

---

## 環境変数の設定方法

### Linux/Mac

#### 1. シェル環境変数

```bash
# 一時的な設定（現在のセッションのみ）
export DB_SERVER="your-sql-server.database.windows.net"
export DB_NAME="AnomalyDetection_Production"

# 永続的な設定（~/.bashrc または ~/.zshrc に追加）
echo 'export DB_SERVER="your-sql-server.database.windows.net"' >> ~/.bashrc
source ~/.bashrc
```

#### 2. .env ファイル

```bash
# .env ファイルを作成
cat > .env << EOF
DB_SERVER=your-sql-server.database.windows.net
DB_NAME=AnomalyDetection_Production
DB_USER=anomalydetection_admin
DB_PASSWORD=YourStrongPassword123!
EOF

# .env ファイルを読み込み
export $(cat .env | xargs)
```

#### 3. systemd サービス

```ini
# /etc/systemd/system/anomalydetection.service
[Unit]
Description=CAN Anomaly Detection API
After=network.target

[Service]
Type=notify
WorkingDirectory=/app
ExecStart=/usr/bin/dotnet AnomalyDetection.HttpApi.Host.dll
Environment="DB_SERVER=your-sql-server.database.windows.net"
Environment="DB_NAME=AnomalyDetection_Production"
Environment="DB_USER=anomalydetection_admin"
Environment="DB_PASSWORD=YourStrongPassword123!"
Environment="ASPNETCORE_ENVIRONMENT=Production"

[Install]
WantedBy=multi-user.target
```

### Windows

#### 1. コマンドプロンプト

```cmd
REM 一時的な設定
set DB_SERVER=your-sql-server.database.windows.net
set DB_NAME=AnomalyDetection_Production

REM 永続的な設定
setx DB_SERVER "your-sql-server.database.windows.net"
setx DB_NAME "AnomalyDetection_Production"
```

#### 2. PowerShell

```powershell
# 一時的な設定
$env:DB_SERVER = "your-sql-server.database.windows.net"
$env:DB_NAME = "AnomalyDetection_Production"

# 永続的な設定（ユーザー環境変数）
[Environment]::SetEnvironmentVariable("DB_SERVER", "your-sql-server.database.windows.net", "User")
[Environment]::SetEnvironmentVariable("DB_NAME", "AnomalyDetection_Production", "User")

# 永続的な設定（システム環境変数）
[Environment]::SetEnvironmentVariable("DB_SERVER", "your-sql-server.database.windows.net", "Machine")
```

#### 3. Windows サービス

```powershell
# サービス作成時に環境変数を設定
sc.exe create AnomalyDetectionAPI binPath= "C:\App\AnomalyDetection.HttpApi.Host.exe"
sc.exe config AnomalyDetectionAPI obj= "NT AUTHORITY\NetworkService"

# レジストリで環境変数を設定
$serviceName = "AnomalyDetectionAPI"
$regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$serviceName"
New-ItemProperty -Path $regPath -Name Environment -PropertyType MultiString -Value @(
    "DB_SERVER=your-sql-server.database.windows.net",
    "DB_NAME=AnomalyDetection_Production"
)
```

### Docker

#### 1. docker run コマンド

```bash
docker run -d \
  --name anomalydetection-api \
  -e DB_SERVER="your-sql-server.database.windows.net" \
  -e DB_NAME="AnomalyDetection_Production" \
  -e DB_USER="anomalydetection_admin" \
  -e DB_PASSWORD="YourStrongPassword123!" \
  -e ASPNETCORE_ENVIRONMENT="Production" \
  -p 80:80 \
  -p 443:443 \
  anomalydetection-api:latest
```

#### 2. docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    image: anomalydetection-api:latest
    environment:
      - DB_SERVER=your-sql-server.database.windows.net
      - DB_NAME=AnomalyDetection_Production
      - DB_USER=anomalydetection_admin
      - DB_PASSWORD=${DB_PASSWORD}
      - ASPNETCORE_ENVIRONMENT=Production
    env_file:
      - .env
    ports:
      - "80:80"
      - "443:443"
```

#### 3. .env ファイル（Docker Compose用）

```bash
# .env
DB_SERVER=your-sql-server.database.windows.net
DB_NAME=AnomalyDetection_Production
DB_USER=anomalydetection_admin
DB_PASSWORD=YourStrongPassword123!
CERT_PASSPHRASE=YourCertPassphrase123!
ENCRYPTION_PASSPHRASE=YourEncryptionKey123!
```

### Kubernetes

#### 1. ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: anomalydetection-config
data:
  DB_SERVER: "your-sql-server.database.windows.net"
  DB_NAME: "AnomalyDetection_Production"
  ASPNETCORE_ENVIRONMENT: "Production"
```

#### 2. Secret

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: anomalydetection-secrets
type: Opaque
stringData:
  DB_USER: "anomalydetection_admin"
  DB_PASSWORD: "YourStrongPassword123!"
  CERT_PASSPHRASE: "YourCertPassphrase123!"
  ENCRYPTION_PASSPHRASE: "YourEncryptionKey123!"
```

#### 3. Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: anomalydetection-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: anomalydetection-api
  template:
    metadata:
      labels:
        app: anomalydetection-api
    spec:
      containers:
      - name: api
        image: anomalydetection-api:latest
        envFrom:
        - configMapRef:
            name: anomalydetection-config
        - secretRef:
            name: anomalydetection-secrets
        ports:
        - containerPort: 80
        - containerPort: 443
```

### Azure App Service

#### 1. Azure Portal

1. Azure Portal にログイン
2. App Service を選択
3. 「構成」→「アプリケーション設定」
4. 「新しいアプリケーション設定」をクリック
5. 環境変数を追加

#### 2. Azure CLI

```bash
# アプリケーション設定を追加
az webapp config appsettings set \
  --name anomalydetection-api \
  --resource-group anomalydetection-rg \
  --settings \
    DB_SERVER="your-sql-server.database.windows.net" \
    DB_NAME="AnomalyDetection_Production" \
    DB_USER="anomalydetection_admin" \
    DB_PASSWORD="YourStrongPassword123!" \
    ASPNETCORE_ENVIRONMENT="Production"

# Key Vault参照を使用
az webapp config appsettings set \
  --name anomalydetection-api \
  --resource-group anomalydetection-rg \
  --settings \
    DB_PASSWORD="@Microsoft.KeyVault(SecretUri=https://my-vault.vault.azure.net/secrets/DB-PASSWORD/)"
```

### AWS Elastic Beanstalk

#### 1. .ebextensions/environment.config

```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    DB_SERVER: "your-rds-instance.region.rds.amazonaws.com"
    DB_NAME: "AnomalyDetection_Production"
    DB_USER: "anomalydetection_admin"
    ASPNETCORE_ENVIRONMENT: "Production"
```

#### 2. AWS CLI

```bash
# 環境変数を設定
aws elasticbeanstalk update-environment \
  --environment-name anomalydetection-prod \
  --option-settings \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=DB_SERVER,Value=your-rds-instance.region.rds.amazonaws.com \
    Namespace=aws:elasticbeanstalk:application:environment,OptionName=DB_NAME,Value=AnomalyDetection_Production
```

---

## 環境変数の検証

### 起動時チェックスクリプト

```bash
#!/bin/bash
# check-env.sh - 環境変数の検証スクリプト

echo "環境変数チェック開始..."

# 必須環境変数のリスト
REQUIRED_VARS=(
  "DB_SERVER"
  "DB_NAME"
  "DB_USER"
  "DB_PASSWORD"
  "CERT_PASSPHRASE"
  "ENCRYPTION_PASSPHRASE"
)

# チェック実行
MISSING_VARS=()
for var in "${REQUIRED_VARS[@]}"; do
  if [ -z "${!var}" ]; then
    MISSING_VARS+=("$var")
  fi
done

# 結果表示
if [ ${#MISSING_VARS[@]} -eq 0 ]; then
  echo "✅ すべての必須環境変数が設定されています"
  exit 0
else
  echo "❌ 以下の環境変数が設定されていません:"
  for var in "${MISSING_VARS[@]}"; do
    echo "  - $var"
  done
  exit 1
fi
```

### PowerShell検証スクリプト

```powershell
# check-env.ps1 - 環境変数の検証スクリプト

Write-Host "環境変数チェック開始..." -ForegroundColor Cyan

# 必須環境変数のリスト
$requiredVars = @(
    "DB_SERVER",
    "DB_NAME",
    "DB_USER",
    "DB_PASSWORD",
    "CERT_PASSPHRASE",
    "ENCRYPTION_PASSPHRASE"
)

# チェック実行
$missingVars = @()
foreach ($var in $requiredVars) {
    if (-not (Test-Path "env:$var")) {
        $missingVars += $var
    }
}

# 結果表示
if ($missingVars.Count -eq 0) {
    Write-Host "✅ すべての必須環境変数が設定されています" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ 以下の環境変数が設定されていません:" -ForegroundColor Red
    foreach ($var in $missingVars) {
        Write-Host "  - $var" -ForegroundColor Yellow
    }
    exit 1
}
```

---

## トラブルシューティング

### 環境変数が認識されない

**症状**: アプリケーションが環境変数を読み取れない

**解決方法**:
1. 環境変数が正しく設定されているか確認
   ```bash
   echo $DB_SERVER
   printenv | grep DB_
   ```

2. アプリケーションを再起動
   ```bash
   systemctl restart anomalydetection
   ```

3. Docker の場合、コンテナを再作成
   ```bash
   docker-compose down
   docker-compose up -d
   ```

### 接続文字列のエラー

**症状**: データベース接続エラー

**解決方法**:
1. 接続文字列の構文を確認
2. 環境変数の展開を確認
   ```bash
   # appsettings.Production.json の ${DB_SERVER} が正しく展開されているか
   ```

3. 手動で接続テスト
   ```bash
   sqlcmd -S $DB_SERVER -U $DB_USER -P $DB_PASSWORD -Q "SELECT 1"
   ```

---

## 参考資料

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Docker Environment Variables](https://docs.docker.com/compose/environment-variables/)
- [Kubernetes ConfigMaps and Secrets](https://kubernetes.io/docs/concepts/configuration/)
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)
- [AWS Secrets Manager](https://docs.aws.amazon.com/secretsmanager/)

---

## サポート

環境変数の設定に関する質問や問題がある場合は、以下に連絡してください：

**サポート連絡先**: support@anomalydetection.example.com
**ドキュメント**: https://docs.anomalydetection.example.com
