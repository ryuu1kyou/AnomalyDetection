# CAN異常検出管理システム - 本番環境セットアップスクリプト (PowerShell)
# このスクリプトは本番環境の環境変数を設定します

param(
    [Parameter(Mandatory=$false)]
    [string]$EnvFile = ".env",
    
    [Parameter(Mandatory=$false)]
    [switch]$Validate,
    
    [Parameter(Mandatory=$false)]
    [switch]$Export,
    
    [Parameter(Mandatory=$false)]
    [switch]$Help
)

# ヘルプ表示
if ($Help) {
    Write-Host @"
CAN異常検出管理システム - 本番環境セットアップスクリプト

使用方法:
  .\setup-production-env.ps1 [-EnvFile <path>] [-Validate] [-Export] [-Help]

オプション:
  -EnvFile <path>   環境変数ファイルのパス (デフォルト: .env)
  -Validate         環境変数の検証のみ実行
  -Export           環境変数をエクスポート
  -Help             このヘルプを表示

例:
  # 環境変数を検証
  .\setup-production-env.ps1 -Validate

  # .env ファイルから環境変数を読み込み
  .\setup-production-env.ps1 -EnvFile .env -Export

  # カスタムファイルから読み込み
  .\setup-production-env.ps1 -EnvFile .env.production -Export
"@
    exit 0
}

# カラー出力関数
function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

# 必須環境変数のリスト
$requiredVars = @(
    "DB_SERVER",
    "DB_NAME",
    "DB_USER",
    "DB_PASSWORD",
    "CERT_PASSPHRASE",
    "ENCRYPTION_PASSPHRASE",
    "SSL_CERT_PATH",
    "SSL_CERT_PASSWORD"
)

# オプション環境変数のリスト
$optionalVars = @(
    "REDIS_CONNECTION_STRING",
    "RABBITMQ_HOST",
    "RABBITMQ_PORT",
    "RABBITMQ_USER",
    "RABBITMQ_PASSWORD",
    "ELASTICSEARCH_URL",
    "ELASTICSEARCH_USER",
    "ELASTICSEARCH_PASSWORD",
    "APPINSIGHTS_INSTRUMENTATIONKEY",
    "ASPNETCORE_ENVIRONMENT",
    "ASPNETCORE_URLS"
)

# 環境変数の検証
function Test-EnvironmentVariables {
    Write-Info "環境変数の検証を開始します..."
    
    $missingVars = @()
    $weakPasswords = @()
    
    # 必須環境変数のチェック
    foreach ($var in $requiredVars) {
        if (-not (Test-Path "env:$var")) {
            $missingVars += $var
        } else {
            # パスワードの強度チェック
            if ($var -like "*PASSWORD*" -or $var -like "*PASSPHRASE*") {
                $value = (Get-Item "env:$var").Value
                if ($value.Length -lt 16) {
                    $weakPasswords += "$var (長さが16文字未満)"
                }
                if ($value -notmatch "[A-Z]" -or $value -notmatch "[a-z]" -or $value -notmatch "[0-9]" -or $value -notmatch "[^a-zA-Z0-9]") {
                    $weakPasswords += "$var (複雑性が不足)"
                }
            }
        }
    }
    
    # 結果表示
    $hasErrors = $false
    
    if ($missingVars.Count -gt 0) {
        Write-Error-Custom "以下の必須環境変数が設定されていません:"
        foreach ($var in $missingVars) {
            Write-Host "  - $var" -ForegroundColor Yellow
        }
        $hasErrors = $true
    } else {
        Write-Success "すべての必須環境変数が設定されています"
    }
    
    if ($weakPasswords.Count -gt 0) {
        Write-Warning-Custom "以下のパスワードが弱い可能性があります:"
        foreach ($var in $weakPasswords) {
            Write-Host "  - $var" -ForegroundColor Yellow
        }
        $hasErrors = $true
    }
    
    # オプション環境変数のチェック
    $missingOptionalVars = @()
    foreach ($var in $optionalVars) {
        if (-not (Test-Path "env:$var")) {
            $missingOptionalVars += $var
        }
    }
    
    if ($missingOptionalVars.Count -gt 0) {
        Write-Info "以下のオプション環境変数が設定されていません:"
        foreach ($var in $missingOptionalVars) {
            Write-Host "  - $var" -ForegroundColor Gray
        }
    }
    
    return -not $hasErrors
}

# .env ファイルから環境変数を読み込み
function Import-EnvFile {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        Write-Error-Custom "環境変数ファイルが見つかりません: $FilePath"
        Write-Info ".env.example をコピーして .env ファイルを作成してください"
        return $false
    }
    
    Write-Info "環境変数ファイルを読み込んでいます: $FilePath"
    
    $envVars = @{}
    $lineNumber = 0
    
    Get-Content $FilePath | ForEach-Object {
        $lineNumber++
        $line = $_.Trim()
        
        # コメント行と空行をスキップ
        if ($line -match "^#" -or $line -eq "") {
            return
        }
        
        # KEY=VALUE 形式をパース
        if ($line -match "^([^=]+)=(.*)$") {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            
            # クォートを削除
            $value = $value -replace '^["'']|["'']$', ''
            
            $envVars[$key] = $value
        } else {
            Write-Warning-Custom "行 $lineNumber の形式が不正です: $line"
        }
    }
    
    Write-Success "$($envVars.Count) 個の環境変数を読み込みました"
    return $envVars
}

# 環境変数をエクスポート
function Export-EnvironmentVariables {
    param([hashtable]$EnvVars)
    
    Write-Info "環境変数をエクスポートしています..."
    
    $exportedCount = 0
    foreach ($key in $EnvVars.Keys) {
        [Environment]::SetEnvironmentVariable($key, $EnvVars[$key], "Process")
        $exportedCount++
    }
    
    Write-Success "$exportedCount 個の環境変数をエクスポートしました"
}

# パスワード生成
function New-SecurePassword {
    param(
        [int]$Length = 24,
        [int]$SpecialCharCount = 8
    )
    
    Add-Type -AssemblyName System.Web
    return [System.Web.Security.Membership]::GeneratePassword($Length, $SpecialCharCount)
}

# 対話的セットアップ
function Start-InteractiveSetup {
    Write-Info "対話的セットアップを開始します..."
    Write-Host ""
    
    $envVars = @{}
    
    # データベース設定
    Write-Host "=== データベース設定 ===" -ForegroundColor Cyan
    $envVars["DB_SERVER"] = Read-Host "データベースサーバー (例: your-sql-server.database.windows.net)"
    $envVars["DB_NAME"] = Read-Host "データベース名 (デフォルト: AnomalyDetection_Production)"
    if ([string]::IsNullOrWhiteSpace($envVars["DB_NAME"])) {
        $envVars["DB_NAME"] = "AnomalyDetection_Production"
    }
    $envVars["DB_USER"] = Read-Host "データベースユーザー名"
    $envVars["DB_PASSWORD"] = Read-Host "データベースパスワード" -AsSecureString
    $envVars["DB_PASSWORD"] = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($envVars["DB_PASSWORD"])
    )
    
    Write-Host ""
    
    # セキュリティ設定
    Write-Host "=== セキュリティ設定 ===" -ForegroundColor Cyan
    $generatePassphrases = Read-Host "パスフレーズを自動生成しますか? (Y/n)"
    if ($generatePassphrases -ne "n") {
        $envVars["CERT_PASSPHRASE"] = New-SecurePassword
        $envVars["ENCRYPTION_PASSPHRASE"] = New-SecurePassword
        Write-Success "パスフレーズを自動生成しました"
    } else {
        $envVars["CERT_PASSPHRASE"] = Read-Host "証明書パスフレーズ"
        $envVars["ENCRYPTION_PASSPHRASE"] = Read-Host "暗号化パスフレーズ"
    }
    
    Write-Host ""
    
    # SSL証明書
    Write-Host "=== SSL証明書設定 ===" -ForegroundColor Cyan
    $envVars["SSL_CERT_PATH"] = Read-Host "SSL証明書パス (デフォルト: /app/certs/certificate.pfx)"
    if ([string]::IsNullOrWhiteSpace($envVars["SSL_CERT_PATH"])) {
        $envVars["SSL_CERT_PATH"] = "/app/certs/certificate.pfx"
    }
    $envVars["SSL_CERT_PASSWORD"] = Read-Host "SSL証明書パスワード" -AsSecureString
    $envVars["SSL_CERT_PASSWORD"] = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($envVars["SSL_CERT_PASSWORD"])
    )
    
    Write-Host ""
    
    # ASP.NET Core設定
    Write-Host "=== ASP.NET Core設定 ===" -ForegroundColor Cyan
    $envVars["ASPNETCORE_ENVIRONMENT"] = "Production"
    $envVars["ASPNETCORE_URLS"] = "http://+:80;https://+:443"
    
    # .env ファイルに保存
    $saveToFile = Read-Host ".env ファイルに保存しますか? (Y/n)"
    if ($saveToFile -ne "n") {
        $envContent = @"
# CAN異常検出管理システム - 本番環境設定
# 生成日時: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

# データベース設定
DB_SERVER=$($envVars["DB_SERVER"])
DB_NAME=$($envVars["DB_NAME"])
DB_USER=$($envVars["DB_USER"])
DB_PASSWORD=$($envVars["DB_PASSWORD"])

# セキュリティ設定
CERT_PASSPHRASE=$($envVars["CERT_PASSPHRASE"])
ENCRYPTION_PASSPHRASE=$($envVars["ENCRYPTION_PASSPHRASE"])

# SSL証明書
SSL_CERT_PATH=$($envVars["SSL_CERT_PATH"])
SSL_CERT_PASSWORD=$($envVars["SSL_CERT_PASSWORD"])

# ASP.NET Core設定
ASPNETCORE_ENVIRONMENT=$($envVars["ASPNETCORE_ENVIRONMENT"])
ASPNETCORE_URLS=$($envVars["ASPNETCORE_URLS"])
"@
        
        $envContent | Out-File -FilePath ".env" -Encoding UTF8
        Write-Success ".env ファイルを作成しました"
        
        # パーミッション設定（Windowsでは制限）
        $acl = Get-Acl ".env"
        $acl.SetAccessRuleProtection($true, $false)
        $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            [System.Security.Principal.WindowsIdentity]::GetCurrent().Name,
            "FullControl",
            "Allow"
        )
        $acl.SetAccessRule($rule)
        Set-Acl ".env" $acl
        Write-Success ".env ファイルのアクセス権限を設定しました"
    }
    
    return $envVars
}

# メイン処理
Write-Host @"
╔═══════════════════════════════════════════════════════════╗
║  CAN異常検出管理システム - 本番環境セットアップ          ║
╚═══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

Write-Host ""

# 検証のみの場合
if ($Validate) {
    $isValid = Test-EnvironmentVariables
    if ($isValid) {
        Write-Success "環境変数の検証が完了しました"
        exit 0
    } else {
        Write-Error-Custom "環境変数の検証に失敗しました"
        exit 1
    }
}

# .env ファイルから読み込み
if (Test-Path $EnvFile) {
    $envVars = Import-EnvFile -FilePath $EnvFile
    
    if ($envVars -and $Export) {
        Export-EnvironmentVariables -EnvVars $envVars
        
        # 検証
        $isValid = Test-EnvironmentVariables
        if (-not $isValid) {
            Write-Warning-Custom "環境変数の検証で警告が発生しました"
        }
    }
} else {
    # 対話的セットアップ
    Write-Warning-Custom "環境変数ファイルが見つかりません: $EnvFile"
    $runInteractive = Read-Host "対話的セットアップを実行しますか? (Y/n)"
    
    if ($runInteractive -ne "n") {
        $envVars = Start-InteractiveSetup
        
        if ($Export) {
            Export-EnvironmentVariables -EnvVars $envVars
        }
    } else {
        Write-Info ".env.example をコピーして .env ファイルを作成してください:"
        Write-Host "  Copy-Item .env.example .env" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host ""
Write-Success "セットアップが完了しました"
Write-Info "詳細は ENVIRONMENT_VARIABLES.md を参照してください"
