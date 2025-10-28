# CAN異常検出管理システム - スモークテストスクリプト (PowerShell)

param(
    [Parameter(Mandatory=$true)]
    [string]$BaseUrl,
    [int]$TimeoutSeconds = 60,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "🔥 スモークテストを開始します..." -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# テスト結果
$TestResults = @{
    Passed = 0
    Failed = 0
    Tests = @()
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus = 200,
        [int]$TimeoutSec = 10
    )
    
    try {
        if ($Verbose) {
            Write-Host "Testing: $Name ($Url)" -ForegroundColor Gray
        }
        
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec $TimeoutSec
        
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-Host "✅ $Name" -ForegroundColor Green
            $TestResults.Passed++
            $TestResults.Tests += @{Name=$Name; Status="PASS"; Url=$Url}
            return $true
        } else {
            Write-Host "❌ $Name - Status: $($response.StatusCode)" -ForegroundColor Red
            $TestResults.Failed++
            $TestResults.Tests += @{Name=$Name; Status="FAIL"; Url=$Url; Error="Status: $($response.StatusCode)"}
            return $false
        }
    }
    catch {
        Write-Host "❌ $Name - Error: $($_.Exception.Message)" -ForegroundColor Red
        $TestResults.Failed++
        $TestResults.Tests += @{Name=$Name; Status="FAIL"; Url=$Url; Error=$_.Exception.Message}
        return $false
    }
}

# 基本的なエンドポイントテスト
Write-Host "`n🔍 基本エンドポイントテスト" -ForegroundColor Yellow

Test-Endpoint -Name "Frontend" -Url $BaseUrl
Test-Endpoint -Name "Health Check" -Url "$BaseUrl/health-status"
Test-Endpoint -Name "API Root" -Url "$BaseUrl/api/app"
Test-Endpoint -Name "Swagger UI" -Url "$BaseUrl/swagger"

# API エンドポイントテスト
Write-Host "`n🔗 API エンドポイントテスト" -ForegroundColor Yellow

Test-Endpoint -Name "CAN Signals API" -Url "$BaseUrl/api/app/can-signal"
Test-Endpoint -Name "Detection Logic API" -Url "$BaseUrl/api/app/can-anomaly-detection-logic"
Test-Endpoint -Name "Projects API" -Url "$BaseUrl/api/app/anomaly-detection-project"

# 認証エンドポイントテスト（401が期待される）
Write-Host "`n🔐 認証エンドポイントテスト" -ForegroundColor Yellow

Test-Endpoint -Name "Auth Token Endpoint" -Url "$BaseUrl/connect/token" -ExpectedStatus 400  # Bad Request (no params)

# 結果サマリー
Write-Host "`n📊 テスト結果" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "総テスト数: $($TestResults.Passed + $TestResults.Failed)" -ForegroundColor White
Write-Host "成功: $($TestResults.Passed)" -ForegroundColor Green
Write-Host "失敗: $($TestResults.Failed)" -ForegroundColor Red

if ($TestResults.Failed -gt 0) {
    Write-Host "`n❌ 失敗したテスト:" -ForegroundColor Red
    $TestResults.Tests | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  • $($_.Name): $($_.Error)" -ForegroundColor Red
    }
}

# 終了コード
if ($TestResults.Failed -eq 0) {
    Write-Host "`n🎉 すべてのスモークテストが成功しました！" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n💥 スモークテストに失敗しました。デプロイメントを確認してください。" -ForegroundColor Red
    exit 1
}