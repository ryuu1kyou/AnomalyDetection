# アプリケーション起動手順

## 前提条件
- .NET 10 SDK がインストール済み
- Node.js と npm がインストール済み
- SQL Server または PostgreSQL が動作中

## 1. Backend API の起動

### 1.1 データベースのマイグレーション
```powershell
cd c:\\work\\tool\\net\\AnomalyDetection\\AnomalyDetection\\src\\AnomalyDetection.DbMigrator
dotnet run
```

### 1.2 API サーバーの起動
```powershell
cd c:\\work\\tool\\net\\AnomalyDetection\\AnomalyDetection\\src\\AnomalyDetection.HttpApi.Host
dotnet run
```

APIは `https://localhost:44362` で起動します。

## 2. Frontend (Angular) の起動

### 2.1 依存パッケージのインストール（初回のみ）
```powershell
cd c:\\work\\tool\\net\\AnomalyDetection\\AnomalyDetection\\angular
npm install
```

### 2.2 開発サーバーの起動
```powershell
npm start
```

または

```powershell
ng serve
```

Angularアプリは `http://localhost:4200` で起動します。

## 3. アプリケーションへのアクセス

### デフォルトログイン情報
- **ユーザー名**: `admin`
- **パスワード**: `1q2w3E*`

### アクセス可能なURL
- **Home**: http://localhost:4200
- **OEM Traceability**: http://localhost:4200/oem-traceability
- **Similar Pattern Search**: http://localhost:4200/similar-comparison
- **Anomaly Analysis**: http://localhost:4200/anomaly-analysis
- **CAN Signals**: http://localhost:4200/can-signals
- **Detection Logics**: http://localhost:4200/detection-logics
- **Projects**: http://localhost:4200/projects
- **Dashboard**: http://localhost:4200/dashboard

## 4. API動作確認

Swagger UIでAPIを確認：
- https://localhost:44362/swagger

## トラブルシューティング

### API起動エラー
- データベース接続文字列を確認: `appsettings.json`
- ポート44362が使用中でないか確認

### Angular起動エラー
- `node_modules`を削除して再インストール:
  ```powershell
  Remove-Item -Recurse -Force node_modules
  npm install
  ```

### CORS エラー
- `HttpApi.Host`の`appsettings.json`で CORS設定を確認
- デフォルトで`http://localhost:4200`は許可されているはず

## 開発時のヒント

### ホットリロード
- Backend: ファイル保存時に自動再起動（`dotnet watch run`）
- Frontend: ファイル保存時に自動リロード（ng serveのデフォルト動作）

### ログ確認
- Backend: コンソール出力
- Frontend: ブラウザの開発者ツール（F12）

### ビルド
```powershell
# Backend
dotnet build AnomalyDetection.sln

# Frontend
npm run build
```
