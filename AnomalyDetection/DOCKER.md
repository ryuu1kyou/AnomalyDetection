# Docker セットアップガイド - CAN異常検出管理システム

## 概要

このドキュメントでは、CAN異常検出管理システムをDockerを使用してセットアップする方法を説明します。

## 前提条件

- Docker Desktop (Windows/Mac) または Docker Engine (Linux)
- Docker Compose v2.0以上
- 最低8GB RAM、20GB以上の空きディスク容量

## アーキテクチャ

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │    Backend      │    │   Database      │
│   (Angular)     │◄──►│  (ABP vNext)    │◄──►│  (SQL Server)   │
│   Port: 4200    │    │   Port: 44318   │    │   Port: 1433    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │     Redis       │
                    │  (Cache/Session)│
                    │   Port: 6379    │
                    └─────────────────┘
```

## クイックスタート

### 開発環境

```bash
# リポジトリをクローン
git clone <repository-url>
cd AnomalyDetection

# 開発環境用セットアップスクリプトを実行
./docker-setup.sh

# または PowerShell (Windows)
.\docker-setup.ps1
```

### 本番環境

```bash
# 環境変数ファイルを作成
cp .env.example .env
# .env ファイルを編集して本番環境用の値を設定

# 本番環境用セットアップスクリプトを実行
./docker-setup.sh prod

# または PowerShell (Windows)
.\docker-setup.ps1 -Environment prod
```

## 手動セットアップ

### 1. 開発環境

```bash
# コンテナを起動
docker-compose up -d

# データベースマイグレーションを実行
docker-compose --profile migration up dbmigrator

# ログを確認
docker-compose logs -f
```

### 2. 本番環境

```bash
# 環境変数を設定
export $(cat .env | xargs)

# 本番環境用コンテナを起動
docker-compose -f docker-compose.prod.yml up -d

# データベースマイグレーションを実行
docker-compose -f docker-compose.prod.yml --profile migration up dbmigrator
```

## サービス詳細

### SQL Server
- **イメージ**: mcr.microsoft.com/mssql/server:2022-latest
- **ポート**: 1433
- **データベース**: AnomalyDetection
- **認証**: SA認証 (開発環境: MyPass@word123)

### Redis
- **イメージ**: redis:7-alpine
- **ポート**: 6379
- **用途**: セッション管理、キャッシュ

### Backend (ABP vNext)
- **ポート**: 44318 (開発), 80 (本番)
- **API**: RESTful API + Swagger UI
- **認証**: JWT + OpenIddict

### Frontend (Angular)
- **ポート**: 4200 (開発), 80 (本番)
- **フレームワーク**: Angular 17+
- **UI**: Angular Material + カスタムテーマ

## 環境変数

### 開発環境 (.env.development)
```bash
DB_NAME=AnomalyDetection_Dev
DB_SA_PASSWORD=MyPass@word123
BACKEND_URL=http://localhost:44318
FRONTEND_URL=http://localhost:4200
```

### 本番環境 (.env)
```bash
DB_NAME=AnomalyDetection
DB_SA_PASSWORD=YourStrongPassword123!
REDIS_PASSWORD=YourRedisPassword123!
BACKEND_URL=https://api.yourdomain.com
FRONTEND_URL=https://yourdomain.com
CORS_ORIGINS=https://yourdomain.com
CERT_PASSPHRASE=YourCertificatePassphrase123!
ENCRYPTION_PASSPHRASE=YourEncryptionPassphrase123!
```

## 運用コマンド

### コンテナ管理
```bash
# 全サービス起動
docker-compose up -d

# 特定サービス起動
docker-compose up -d backend frontend

# サービス停止
docker-compose down

# ボリューム含めて完全削除
docker-compose down -v

# サービス再起動
docker-compose restart backend

# ログ確認
docker-compose logs -f backend
```

### データベース管理
```bash
# マイグレーション実行
docker-compose --profile migration up dbmigrator

# データベースバックアップ
docker exec anomaly-detection-sqlserver-prod /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P $DB_SA_PASSWORD \
  -Q "BACKUP DATABASE AnomalyDetection TO DISK = '/var/backups/anomaly_detection_$(date +%Y%m%d_%H%M%S).bak'"

# データベース復元
docker exec anomaly-detection-sqlserver-prod /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P $DB_SA_PASSWORD \
  -Q "RESTORE DATABASE AnomalyDetection FROM DISK = '/var/backups/backup_file.bak'"
```

### 監視・ヘルスチェック
```bash
# コンテナ状態確認
docker-compose ps

# リソース使用量確認
docker stats

# ヘルスチェック
curl http://localhost:44318/health-status

# システム情報
docker system df
docker system prune  # 不要なリソース削除
```

## トラブルシューティング

### よくある問題

#### 1. データベース接続エラー
```bash
# SQL Serverコンテナのログを確認
docker-compose logs sqlserver

# 接続テスト
docker exec -it anomaly-detection-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P MyPass@word123 -Q "SELECT @@VERSION"
```

#### 2. ポート競合エラー
```bash
# ポート使用状況確認
netstat -tulpn | grep :1433
netstat -tulpn | grep :4200

# 競合するプロセスを停止
sudo kill -9 <PID>
```

#### 3. メモリ不足
```bash
# Docker Desktop のメモリ設定を8GB以上に増加
# または不要なコンテナを停止
docker container prune
docker image prune
```

#### 4. SSL証明書エラー (本番環境)
```bash
# 証明書ファイルの配置確認
ls -la ./ssl/
chmod 644 ./ssl/cert.pem
chmod 600 ./ssl/private.key
```

### ログ分析
```bash
# エラーログのみ表示
docker-compose logs backend | grep ERROR

# 特定時間範囲のログ
docker-compose logs --since="2024-01-01T00:00:00" --until="2024-01-01T23:59:59" backend

# ログファイルの場所
# Backend: ./logs/
# Frontend: docker-compose logs frontend
# Database: docker-compose logs sqlserver
```

## パフォーマンス最適化

### リソース制限設定
```yaml
# docker-compose.prod.yml での設定例
deploy:
  resources:
    limits:
      memory: 2G
      cpus: '1.0'
    reservations:
      memory: 1G
      cpus: '0.5'
```

### データベース最適化
```sql
-- インデックス再構築
ALTER INDEX ALL ON [dbo].[CanSignals] REBUILD;

-- 統計情報更新
UPDATE STATISTICS [dbo].[CanSignals];

-- データベース最適化
DBCC CHECKDB('AnomalyDetection');
```

## セキュリティ

### 本番環境セキュリティチェックリスト
- [ ] 強力なパスワードの設定
- [ ] SSL証明書の適切な配置
- [ ] ファイアウォール設定
- [ ] 定期的なセキュリティアップデート
- [ ] ログ監視の設定
- [ ] バックアップの自動化

### ネットワークセキュリティ
```bash
# Docker ネットワーク確認
docker network ls
docker network inspect anomaly-detection-network

# ポートスキャン
nmap -p 1433,4200,44318 localhost
```

## バックアップ・復旧

### 自動バックアップ設定
```bash
# crontab に追加
0 2 * * * /path/to/backup-script.sh

# バックアップスクリプト例
#!/bin/bash
BACKUP_DIR="/var/backups/anomaly-detection"
DATE=$(date +%Y%m%d_%H%M%S)

# データベースバックアップ
docker exec anomaly-detection-sqlserver-prod /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P $DB_SA_PASSWORD \
  -Q "BACKUP DATABASE AnomalyDetection TO DISK = '/var/backups/db_$DATE.bak'"

# ファイルバックアップ
tar -czf $BACKUP_DIR/files_$DATE.tar.gz ./logs ./ssl

# 古いバックアップ削除 (30日以上)
find $BACKUP_DIR -name "*.bak" -mtime +30 -delete
find $BACKUP_DIR -name "*.tar.gz" -mtime +30 -delete
```

## 参考リンク

- [Docker公式ドキュメント](https://docs.docker.com/)
- [Docker Compose公式ドキュメント](https://docs.docker.com/compose/)
- [ABP Framework](https://abp.io/)
- [Angular](https://angular.io/)
- [SQL Server on Docker](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)