# CAN異常検出管理システム (CAN Anomaly Detection Management System)

## プロジェクト概要 (Project Overview)

CAN異常検出管理システムは、自動車業界におけるCAN信号の異常検出ロジック開発・管理・共有を支援するマルチテナント対応のWebアプリケーションです。複数のOEM（自動車メーカー）が独自のデータ空間を持ちながら、業界共通の知見を共有できる仕組みを提供します。

This is a multi-tenant web application for managing CAN signal anomaly detection logic development, management, and sharing in the automotive industry. It enables multiple OEMs (Original Equipment Manufacturers) to maintain their own data spaces while sharing industry-common knowledge.

### 主要機能 (Key Features)

- **CAN信号管理**: CAN信号仕様の定義・管理・バージョン管理
- **異常検出ロジック開発**: 標準テンプレートとカスタムロジックによる効率的な開発
- **マルチテナント対応**: OEM別データ分離と業界共通情報の共有制御
- **機能安全対応**: ISO 26262準拠のトレーサビリティ管理
- **車両フェーズ管理**: 開発フェーズ間での知見継承・流用
- **統計・レポート**: 異常検出結果の分析とレポート生成
- **多言語対応**: 日本語・英語の切り替え対応

### 技術スタック (Technology Stack)

#### バックエンド (Backend)
- **.NET 9.0**: 最新の.NETフレームワーク
- **ABP vNext 9.3.5**: マルチテナント・DDD対応フレームワーク
- **Entity Framework Core**: ORM・データベースアクセス
- **SQL Server**: データベース
- **OpenIddict**: 認証・認可
- **AutoMapper**: オブジェクトマッピング
- **Serilog**: ログ管理

#### フロントエンド (Frontend)
- **Angular 20.0**: モダンなSPAフレームワーク
- **Angular Material 20.2**: UIコンポーネントライブラリ
- **NgRx 20.1**: 状態管理
- **TypeScript 5.8**: 型安全な開発
- **RxJS 7.8**: リアクティブプログラミング

#### 開発・テスト (Development & Testing)
- **Cypress**: E2Eテスト
- **Jasmine/Karma**: ユニットテスト
- **ESLint**: コード品質管理
- **Docker**: コンテナ化対応

## 前提条件 (Prerequisites)

開発環境をセットアップする前に、以下のソフトウェアがインストールされている必要があります：

### 必須要件 (Required)
- **[.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet)**: バックエンド開発用
- **[Node.js v18 or v20](https://nodejs.org/en)**: フロントエンド開発用
- **[SQL Server](https://www.microsoft.com/sql-server)**: データベース（LocalDB可）
- **[Git](https://git-scm.com/)**: バージョン管理

### 推奨 (Recommended)
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** または **[Visual Studio Code](https://code.visualstudio.com/)**: IDE
- **[SQL Server Management Studio](https://docs.microsoft.com/sql/ssms/)**: データベース管理
- **[Postman](https://www.postman.com/)**: API テスト

## セットアップ手順 (Setup Instructions)

### 1. リポジトリのクローン (Clone Repository)

```bash
git clone <repository-url>
cd AnomalyDetection
```

### 2. バックエンドセットアップ (Backend Setup)

#### 2.1 依存関係の復元 (Restore Dependencies)

```bash
# プロジェクトルートで実行
dotnet restore
```

#### 2.2 データベース設定 (Database Configuration)

`src/AnomalyDetection.HttpApi.Host/appsettings.json` と `src/AnomalyDetection.DbMigrator/appsettings.json` の接続文字列を環境に合わせて変更：

```json
{
  "ConnectionStrings": {
    "Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=AnomalyDetection;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

#### 2.3 データベース初期化 (Database Initialization)

```bash
# DbMigratorプロジェクトを実行してデータベースを作成
cd src/AnomalyDetection.DbMigrator
dotnet run
```

#### 2.4 HTTPS証明書の生成 (Generate HTTPS Certificate)

```bash
# 開発用証明書の生成
dotnet dev-certs https -v -ep openiddict.pfx -p 759b5d73-38f7-495a-a237-650313e3a83f
```

#### 2.5 バックエンドAPI起動 (Start Backend API)

```bash
# HttpApi.Hostプロジェクトを起動
cd src/AnomalyDetection.HttpApi.Host
dotnet run
```

バックエンドAPIは `https://localhost:44397` で起動します。
Swagger UIは `https://localhost:44397/swagger` でアクセス可能です。

### 3. フロントエンドセットアップ (Frontend Setup)

#### 3.1 依存関係のインストール (Install Dependencies)

```bash
cd angular
npm install
# または
yarn install
```

#### 3.2 ABPライブラリのインストール (Install ABP Libraries)

```bash
# プロジェクトルートで実行
abp install-libs
```

#### 3.3 フロントエンド起動 (Start Frontend)

```bash
cd angular
npm start
# または
yarn start
```

フロントエンドアプリケーションは `http://localhost:4200` で起動します。

### 4. 初期ログイン (Initial Login)

デフォルトの管理者アカウントでログイン：

- **ユーザー名**: admin
- **パスワード**: 1q2w3E*

## 開発ガイド (Development Guide)

### プロジェクト構造 (Project Structure)

```
AnomalyDetection/
├── src/                                    # バックエンドソースコード
│   ├── AnomalyDetection.Domain/           # ドメイン層
│   ├── AnomalyDetection.Application/      # アプリケーション層
│   ├── AnomalyDetection.HttpApi/          # Web API層
│   ├── AnomalyDetection.HttpApi.Host/     # ホストプロジェクト
│   ├── AnomalyDetection.EntityFrameworkCore/ # データアクセス層
│   └── AnomalyDetection.DbMigrator/       # データベース移行
├── angular/                               # フロントエンドソースコード
│   ├── src/app/                          # Angularアプリケーション
│   ├── src/environments/                 # 環境設定
│   └── cypress/                          # E2Eテスト
├── test/                                 # テストプロジェクト
└── etc/                                  # 設定ファイル
```

### 主要なドメインエンティティ (Key Domain Entities)

- **CanSignal**: CAN信号の定義と仕様
- **AnomalyDetectionLogic**: 異常検出ロジック
- **DetectionResult**: 異常検出結果
- **VehiclePhase**: 車両開発フェーズ
- **Tenant**: OEMテナント情報

### API エンドポイント (API Endpoints)

- **CAN信号管理**: `/api/app/can-signal`
- **異常検出ロジック**: `/api/app/anomaly-detection-logic`
- **検出結果**: `/api/app/detection-result`
- **統計情報**: `/api/app/statistics`

## テスト実行 (Running Tests)

### バックエンドテスト (Backend Tests)

```bash
# ユニットテスト実行
dotnet test

# 特定のプロジェクトのテスト実行
dotnet test test/AnomalyDetection.Application.Tests
```

### フロントエンドテスト (Frontend Tests)

```bash
cd angular

# ユニットテスト実行
npm test

# E2Eテスト実行
npm run e2e
```

## ビルド・デプロイ (Build & Deployment)

### 開発ビルド (Development Build)

```bash
# バックエンド
dotnet build

# フロントエンド
cd angular
npm run build
```

### 本番ビルド (Production Build)

```bash
# バックエンド
dotnet publish -c Release

# フロントエンド
cd angular
npm run build:prod
```

## 設定 (Configuration)

### 環境変数 (Environment Variables)

主要な設定項目：

- `ConnectionStrings__Default`: データベース接続文字列
- `App__SelfUrl`: アプリケーションURL
- `App__CorsOrigins`: CORS許可オリジン
- `AuthServer__Authority`: 認証サーバーURL

### マルチテナント設定 (Multi-Tenant Configuration)

テナント設定は `appsettings.json` で管理：

```json
{
  "AbpMultiTenancy": {
    "IsEnabled": true
  }
}
```

## トラブルシューティング (Troubleshooting)

### よくある問題 (Common Issues)

1. **データベース接続エラー**
   - 接続文字列を確認
   - SQL Serverサービスが起動していることを確認

2. **HTTPS証明書エラー**
   - `dotnet dev-certs https --trust` を実行

3. **フロントエンドビルドエラー**
   - `node_modules` を削除して `npm install` を再実行

4. **ABPライブラリエラー**
   - `abp install-libs` を再実行

## ライセンス (License)

このプロジェクトは [MIT License](LICENSE) の下で公開されています。

## サポート (Support)

技術的な質問やサポートが必要な場合は、以下のリソースを参照してください：

- **ABP Framework Documentation**: https://abp.io/docs
- **Angular Documentation**: https://angular.io/docs
- **プロジェクトIssues**: [GitHub Issues](https://github.com/your-repo/issues)

## 貢献 (Contributing)

プロジェクトへの貢献を歓迎します。貢献する前に [CONTRIBUTING.md](CONTRIBUTING.md) をお読みください。
