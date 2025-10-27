#!/bin/bash

# CAN異常検出管理システム - Docker セットアップスクリプト

set -e

echo "🚀 CAN異常検出管理システム Docker セットアップを開始します..."

# 環境変数の確認
if [ "$1" = "prod" ]; then
    echo "📦 本番環境用セットアップを実行します"
    COMPOSE_FILE="docker-compose.prod.yml"
    ENV_FILE=".env"
    
    if [ ! -f "$ENV_FILE" ]; then
        echo "❌ .env ファイルが見つかりません。.env.example をコピーして設定してください。"
        exit 1
    fi
else
    echo "🔧 開発環境用セットアップを実行します"
    COMPOSE_FILE="docker-compose.yml"
    ENV_FILE=".env.development"
fi

# Docker と Docker Compose の確認
if ! command -v docker &> /dev/null; then
    echo "❌ Docker がインストールされていません"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose がインストールされていません"
    exit 1
fi

# 既存のコンテナを停止・削除
echo "🛑 既存のコンテナを停止・削除します..."
docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE down -v

# イメージをビルド
echo "🔨 Docker イメージをビルドします..."
docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE build --no-cache

# データベースとRedisを先に起動
echo "🗄️ データベースとRedisを起動します..."
docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE up -d sqlserver redis

# データベースの起動を待機
echo "⏳ データベースの起動を待機します..."
sleep 30

# データベースマイグレーションを実行
echo "🔄 データベースマイグレーションを実行します..."
docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE --profile migration up dbmigrator

# バックエンドとフロントエンドを起動
echo "🌐 バックエンドとフロントエンドを起動します..."
docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE up -d backend frontend

# 本番環境の場合はNginxも起動
if [ "$1" = "prod" ]; then
    echo "🔒 Nginxリバースプロキシを起動します..."
    docker-compose -f $COMPOSE_FILE --env-file $ENV_FILE up -d nginx
fi

# ヘルスチェック
echo "🏥 ヘルスチェックを実行します..."
sleep 10

if [ "$1" = "prod" ]; then
    BACKEND_URL="http://localhost"
    FRONTEND_URL="http://localhost"
else
    BACKEND_URL="http://localhost:44318"
    FRONTEND_URL="http://localhost:4200"
fi

# バックエンドのヘルスチェック
if curl -f "$BACKEND_URL/health-status" > /dev/null 2>&1; then
    echo "✅ バックエンドが正常に起動しました: $BACKEND_URL"
else
    echo "⚠️ バックエンドのヘルスチェックに失敗しました"
fi

# フロントエンドのヘルスチェック
if curl -f "$FRONTEND_URL" > /dev/null 2>&1; then
    echo "✅ フロントエンドが正常に起動しました: $FRONTEND_URL"
else
    echo "⚠️ フロントエンドのヘルスチェックに失敗しました"
fi

echo ""
echo "🎉 セットアップが完了しました！"
echo ""
echo "📋 アクセス情報:"
echo "   フロントエンド: $FRONTEND_URL"
echo "   バックエンドAPI: $BACKEND_URL"
echo "   Swagger UI: $BACKEND_URL/swagger"
echo ""
echo "📊 コンテナ状況を確認:"
echo "   docker-compose -f $COMPOSE_FILE ps"
echo ""
echo "📝 ログを確認:"
echo "   docker-compose -f $COMPOSE_FILE logs -f [service-name]"
echo ""
echo "🛑 停止する場合:"
echo "   docker-compose -f $COMPOSE_FILE down"