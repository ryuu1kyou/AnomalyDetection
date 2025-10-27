# CAN異常検出管理システム - Docker セットアップスクリプト (PowerShell)

param(
    [string]$Environment = "dev"
)

Write-Host "🚀 CAN異常検出管理システム Docker セットアップを開始します..." -ForegroundColor Green

# 環境変数の確認
if ($Environment -eq "prod") {
    Write-Host "📦 本番環境用セットアップを実行します" -ForegroundColor Yellow
    $ComposeFile = "docker-compose.prod.yml"
    $EnvFile = ".env"
    
    if (!(Test-Path $EnvFile)) {
        Write-Host "❌ .env ファイルが見つかりません。.env.example をコピーして設定してください。" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "🔧 開発環境用セットアップを実行します" -ForegroundColor Cyan
    $ComposeFile = "docker-compose.yml"
    $EnvFile = ".env.development"
}

# Docker と Docker Compose の確認
try {
    docker --version | Out-Null
    docker-compose --version | Out-Null
} catch {
    Write-Host "❌ Docker または Docker Compose がインストールされていません" -ForegroundColor Red
    exit 1
}

# 既存のコンテナを停止・削除
Write-Host "🛑 既存のコンテナを停止・削除します..." -ForegroundColor Yellow
docker-compose -f $ComposeFile --env-file $EnvFile down -v

# イメージをビルド
Write-Host "🔨 Docker イメージをビルドします..." -ForegroundColor Cyan
docker-compose -f $ComposeFile --env-file $EnvFile build --no-cache

# データベースとRedisを先に起動
Write-Host "🗄️ データベースとRedisを起動します..." -ForegroundColor Cyan
docker-compose -f $ComposeFile --env-file $EnvFile up -d sqlserver redis

# データベースの起動を待機
Write-Host "⏳ データベースの起動を待機します..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# データベースマイグレーションを実行
Write-Host "🔄 データベースマイグレーションを実行します..." -ForegroundColor Cyan
docker-compose -f $ComposeFile --env-file $EnvFile --profile migration up dbmigrator

# バックエンドとフロントエンドを起動
Write-Host "🌐 バックエンドとフロントエンドを起動します..." -ForegroundColor Cyan
docker-compose -f $ComposeFile --env-file $EnvFile up -d backend frontend

# 本番環境の場合はNginxも起動
if ($Environment -eq "prod") {
    Write-Host "🔒 Nginxリバースプロキシを起動します..." -ForegroundColor Cyan
    docker-compose -f $ComposeFile --env-file $EnvFile up -d nginx
}

# ヘルスチェック
Write-Host "🏥 ヘルスチェックを実行します..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

if ($Environment -eq "prod") {
    $BackendUrl = "http://localhost"
    $FrontendUrl = "http://localhost"
} else {
    $BackendUrl = "http://localhost:44318"
    $FrontendUrl = "http://localhost:4200"
}

# バックエンドのヘルスチェック
try {
    $response = Invoke-WebRequest -Uri "$BackendUrl/health-status" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ バックエンドが正常に起動しました: $BackendUrl" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ バックエンドのヘルスチェックに失敗しました" -ForegroundColor Yellow
}

# フロントエンドのヘルスチェック
try {
    $response = Invoke-WebRequest -Uri $FrontendUrl -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ フロントエンドが正常に起動しました: $FrontendUrl" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ フロントエンドのヘルスチェックに失敗しました" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎉 セットアップが完了しました！" -ForegroundColor Green
Write-Host ""
Write-Host "📋 アクセス情報:" -ForegroundColor Cyan
Write-Host "   フロントエンド: $FrontendUrl"
Write-Host "   バックエンドAPI: $BackendUrl"
Write-Host "   Swagger UI: $BackendUrl/swagger"
Write-Host ""
Write-Host "📊 コンテナ状況を確認:" -ForegroundColor Cyan
Write-Host "   docker-compose -f $ComposeFile ps"
Write-Host ""
Write-Host "📝 ログを確認:" -ForegroundColor Cyan
Write-Host "   docker-compose -f $ComposeFile logs -f [service-name]"
Write-Host ""
Write-Host "🛑 停止する場合:" -ForegroundColor Cyan
Write-Host "   docker-compose -f $ComposeFile down"