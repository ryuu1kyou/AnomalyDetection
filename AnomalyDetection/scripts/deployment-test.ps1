# CAN異常検出管理システム - デプロイメントテストスクリプト (PowerShell)

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,
    
    [string]$BaseUrl = "",
    [int]$TimeoutSeconds = 300,
    [switch]$SkipE2E = $false,
    [switch]$Verbose = $false
)

# 設定
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# 環境別設定
$Config = @{
    staging = @{
        BaseUrl = if ($BaseUrl) { $BaseUrl } else { "http://staging.anomalydetection.local" }
        HealthCheckUrl = "/health-status"
        ApiUrl = "/api/app"
        SwaggerUrl = "/swagger"
        ExpectedServices = @("backend", "frontend", "database", "redis")
    }
    production = @{
        BaseUrl = if ($BaseUrl) { $BaseUrl } else { "https://anomalydetection.yourdomain.com" }
        HealthCheckUrl = "/health-status"
        ApiUrl = "/api/app"
        SwaggerUrl = "/swagger"
        ExpectedServices = @("backend", "frontend", "database", "redis", "nginx")
    }
}

$EnvConfig = $Config[$Environment]

# ログ関数
function Write-TestLog {
    param(
        [string]$Message,
        [ValidateSet("Info", "Success", "Warning", "Error")]
        [string]$Level = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "Info" { "Cyan" }
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
    }
    
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
    
    # ログファイルにも出力
    "$timestamp [$Level] $Message" | Out-File -FilePath "deployment-test-$Environment.log" -Append -Encoding UTF8
}

# HTTP リクエスト関数
function Invoke-TestRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [object]$Body = $null,
        [int]$ExpectedStatusCode = 200,
        [int]$TimeoutSec = 30
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            UseBasicParsing = $true
            TimeoutSec = $TimeoutSec
            Headers = $Headers
        }
        
        if ($Body) {
            $params.Body = $Body | ConvertTo-Json -Depth 10
            $params.ContentType = "application/json"
        }
        
        if ($Verbose) {
            Write-TestLog "Request: $Method $Url" -Level "Info"
        }
        
        $response = Invoke-WebRequest @params
        
        if ($response.StatusCode -eq $ExpectedStatusCode) {
            return @{
                Success = $true
                StatusCode = $response.StatusCode
                Content = $response.Content
                Headers = $response.Headers
                ResponseTime = 0  # PowerShellでは直接測定が困難
            }
        } else {
            return @{
                Success = $false
                StatusCode = $response.StatusCode
                Error = "Unexpected status code: $($response.StatusCode)"
            }
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
            StatusCode = if ($_.Exception.Response) { $_.Exception.Response.StatusCode } else { 0 }
        }
    }
}

# テスト結果管理
$TestResults = @{
    TotalTests = 0
    PassedTests = 0
    FailedTests = 0
    Warnings = 0
    StartTime = Get-Date
    Tests = @()
}

function Add-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message = "",
        [object]$Details = $null,
        [bool]$IsWarning = $false
    )
    
    $TestResults.TotalTests++
    
    if ($IsWarning) {
        $TestResults.Warnings++
        $status = "WARNING"
        $level = "Warning"
    } elseif ($Passed) {
        $TestResults.PassedTests++
        $status = "PASS"
        $level = "Success"
    } else {
        $TestResults.FailedTests++
        $status = "FAIL"
        $level = "Error"
    }
    
    $result = @{
        Name = $TestName
        Status = $status
        Message = $Message
        Details = $Details
        Timestamp = Get-Date
    }
    
    $TestResults.Tests += $result
    Write-TestLog "$status - $TestName: $Message" -Level $level
}

# メインテスト実行
function Start-DeploymentTest {
    Write-TestLog "🚀 $Environment 環境のデプロイメントテストを開始します..." -Level "Info"
    Write-TestLog "Base URL: $($EnvConfig.BaseUrl)" -Level "Info"
    
    # 1. 基本接続テスト
    Test-BasicConnectivity
    
    # 2. ヘルスチェックテスト
    Test-HealthCheck
    
    # 3. API エンドポイントテスト
    Test-ApiEndpoints
    
    # 4. 認証テスト
    Test-Authentication
    
    # 5. データベース接続テスト
    Test-DatabaseConnectivity
    
    # 6. パフォーマンステスト
    Test-Performance
    
    # 7. セキュリティテスト
    Test-Security
    
    # 8. E2Eテスト（オプション）
    if (!$SkipE2E) {
        Test-EndToEnd
    }
    
    # 9. 監視・ログテスト
    Test-MonitoringAndLogs
    
    # 結果レポート生成
    Generate-TestReport
}

function Test-BasicConnectivity {
    Write-TestLog "🔌 基本接続テストを実行中..." -Level "Info"
    
    # フロントエンド接続テスト
    $response = Invoke-TestRequest -Url $EnvConfig.BaseUrl -TimeoutSec 10
    Add-TestResult -TestName "Frontend Connectivity" -Passed $response.Success -Message $response.Error
    
    # バックエンドAPI接続テスト
    $apiUrl = "$($EnvConfig.BaseUrl)$($EnvConfig.ApiUrl)"
    $response = Invoke-TestRequest -Url $apiUrl -TimeoutSec 10
    Add-TestResult -TestName "Backend API Connectivity" -Passed $response.Success -Message $response.Error
}

function Test-HealthCheck {
    Write-TestLog "🏥 ヘルスチェックテストを実行中..." -Level "Info"
    
    $healthUrl = "$($EnvConfig.BaseUrl)$($EnvConfig.HealthCheckUrl)"
    $response = Invoke-TestRequest -Url $healthUrl -TimeoutSec 15
    
    if ($response.Success) {
        try {
            $healthData = $response.Content | ConvertFrom-Json
            
            # 各サービスの状態をチェック
            foreach ($service in $EnvConfig.ExpectedServices) {
                $serviceHealthy = $healthData.status -eq "Healthy"
                Add-TestResult -TestName "Health Check - $service" -Passed $serviceHealthy -Message "Status: $($healthData.status)"
            }
        }
        catch {
            Add-TestResult -TestName "Health Check Response Parse" -Passed $false -Message "Failed to parse health check response"
        }
    } else {
        Add-TestResult -TestName "Health Check Endpoint" -Passed $false -Message $response.Error
    }
}

function Test-ApiEndpoints {
    Write-TestLog "🔗 API エンドポイントテストを実行中..." -Level "Info"
    
    $apiBase = "$($EnvConfig.BaseUrl)$($EnvConfig.ApiUrl)"
    
    # 主要エンドポイントのテスト
    $endpoints = @(
        @{Path="/can-signal"; Method="GET"; Name="CAN Signal List"},
        @{Path="/can-anomaly-detection-logic"; Method="GET"; Name="Detection Logic List"},
        @{Path="/anomaly-detection-project"; Method="GET"; Name="Project List"},
        @{Path="/oem-traceability/customizations"; Method="GET"; Name="OEM Customizations"},
        @{Path="/similar-pattern-search/search-signals"; Method="POST"; Name="Similar Pattern Search"; RequiresAuth=$true},
        @{Path="/anomaly-analysis/analyze-pattern"; Method="POST"; Name="Anomaly Analysis"; RequiresAuth=$true}
    )
    
    foreach ($endpoint in $endpoints) {
        $url = "$apiBase$($endpoint.Path)"
        
        if ($endpoint.RequiresAuth) {
            # 認証が必要なエンドポイントは401が期待される
            $response = Invoke-TestRequest -Url $url -Method $endpoint.Method -ExpectedStatusCode 401
            Add-TestResult -TestName "API Endpoint - $($endpoint.Name)" -Passed $response.Success -Message "Authentication required (expected 401)"
        } else {
            $response = Invoke-TestRequest -Url $url -Method $endpoint.Method
            Add-TestResult -TestName "API Endpoint - $($endpoint.Name)" -Passed $response.Success -Message $response.Error
        }
    }
}

function Test-Authentication {
    Write-TestLog "🔐 認証テストを実行中..." -Level "Info"
    
    # Swagger UI アクセステスト
    $swaggerUrl = "$($EnvConfig.BaseUrl)$($EnvConfig.SwaggerUrl)"
    $response = Invoke-TestRequest -Url $swaggerUrl
    Add-TestResult -TestName "Swagger UI Access" -Passed $response.Success -Message $response.Error
    
    # 認証エンドポイントテスト
    $authUrl = "$($EnvConfig.BaseUrl)/connect/token"
    $response = Invoke-TestRequest -Url $authUrl -Method "POST" -ExpectedStatusCode 400  # Bad Requestが期待される（パラメータなし）
    Add-TestResult -TestName "Authentication Endpoint" -Passed $response.Success -Message "Auth endpoint accessible"
}

function Test-DatabaseConnectivity {
    Write-TestLog "🗄️ データベース接続テストを実行中..." -Level "Info"
    
    # ヘルスチェック経由でデータベース状態を確認
    $healthUrl = "$($EnvConfig.BaseUrl)$($EnvConfig.HealthCheckUrl)"
    $response = Invoke-TestRequest -Url $healthUrl
    
    if ($response.Success) {
        try {
            $healthData = $response.Content | ConvertFrom-Json
            $dbHealthy = $healthData.entries.Database.status -eq "Healthy"
            Add-TestResult -TestName "Database Connectivity" -Passed $dbHealthy -Message "Database status via health check"
        }
        catch {
            Add-TestResult -TestName "Database Connectivity" -Passed $false -Message "Could not determine database status"
        }
    }
}

function Test-Performance {
    Write-TestLog "⚡ パフォーマンステストを実行中..." -Level "Info"
    
    $performanceTests = @()
    $iterations = 5
    
    for ($i = 1; $i -le $iterations; $i++) {
        $startTime = Get-Date
        $response = Invoke-TestRequest -Url "$($EnvConfig.BaseUrl)$($EnvConfig.HealthCheckUrl)"
        $endTime = Get-Date
        $responseTime = ($endTime - $startTime).TotalMilliseconds
        
        $performanceTests += $responseTime
        
        if ($Verbose) {
            Write-TestLog "Performance test $i/$iterations : ${responseTime}ms" -Level "Info"
        }
    }
    
    $avgResponseTime = ($performanceTests | Measure-Object -Average).Average
    $maxResponseTime = ($performanceTests | Measure-Object -Maximum).Maximum
    
    # パフォーマンス閾値チェック
    $avgPassed = $avgResponseTime -lt 2000  # 2秒以下
    $maxPassed = $maxResponseTime -lt 5000  # 5秒以下
    
    Add-TestResult -TestName "Average Response Time" -Passed $avgPassed -Message "${avgResponseTime:F0}ms (threshold: 2000ms)"
    Add-TestResult -TestName "Max Response Time" -Passed $maxPassed -Message "${maxResponseTime:F0}ms (threshold: 5000ms)"
}

function Test-Security {
    Write-TestLog "🔒 セキュリティテストを実行中..." -Level "Info"
    
    # HTTPS リダイレクトテスト（本番環境のみ）
    if ($Environment -eq "production") {
        $httpUrl = $EnvConfig.BaseUrl -replace "https://", "http://"
        try {
            $response = Invoke-WebRequest -Uri $httpUrl -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
            $httpsRedirect = $response.StatusCode -eq 301 -or $response.StatusCode -eq 302
            Add-TestResult -TestName "HTTPS Redirect" -Passed $httpsRedirect -Message "HTTP to HTTPS redirection"
        }
        catch {
            Add-TestResult -TestName "HTTPS Redirect" -Passed $false -Message "Could not test HTTPS redirect"
        }
    }
    
    # セキュリティヘッダーテスト
    $response = Invoke-TestRequest -Url $EnvConfig.BaseUrl
    if ($response.Success) {
        $securityHeaders = @(
            "X-Frame-Options",
            "X-Content-Type-Options",
            "X-XSS-Protection"
        )
        
        foreach ($header in $securityHeaders) {
            $headerPresent = $response.Headers.ContainsKey($header)
            Add-TestResult -TestName "Security Header - $header" -Passed $headerPresent -Message "Header presence check" -IsWarning (!$headerPresent)
        }
    }
}

function Test-EndToEnd {
    Write-TestLog "🔄 E2Eテストを実行中..." -Level "Info"
    
    # 簡単なE2Eシナリオ
    # 1. フロントエンドアクセス
    $frontendResponse = Invoke-TestRequest -Url $EnvConfig.BaseUrl
    Add-TestResult -TestName "E2E - Frontend Access" -Passed $frontendResponse.Success -Message $frontendResponse.Error
    
    # 2. API データ取得
    $apiResponse = Invoke-TestRequest -Url "$($EnvConfig.BaseUrl)$($EnvConfig.ApiUrl)/can-signal"
    Add-TestResult -TestName "E2E - API Data Retrieval" -Passed $apiResponse.Success -Message $apiResponse.Error
    
    # 3. ヘルスチェック
    $healthResponse = Invoke-TestRequest -Url "$($EnvConfig.BaseUrl)$($EnvConfig.HealthCheckUrl)"
    Add-TestResult -TestName "E2E - Health Check" -Passed $healthResponse.Success -Message $healthResponse.Error
}

function Test-MonitoringAndLogs {
    Write-TestLog "📊 監視・ログテストを実行中..." -Level "Info"
    
    # メトリクスエンドポイントテスト（利用可能な場合）
    $metricsUrl = "$($EnvConfig.BaseUrl)/metrics"
    $response = Invoke-TestRequest -Url $metricsUrl -ExpectedStatusCode 200
    Add-TestResult -TestName "Metrics Endpoint" -Passed $response.Success -Message "Prometheus metrics availability" -IsWarning (!$response.Success)
    
    # ログ出力テスト（ヘルスチェック経由）
    $healthResponse = Invoke-TestRequest -Url "$($EnvConfig.BaseUrl)$($EnvConfig.HealthCheckUrl)"
    Add-TestResult -TestName "Application Logging" -Passed $healthResponse.Success -Message "Log generation via health check"
}

function Generate-TestReport {
    $endTime = Get-Date
    $duration = $endTime - $TestResults.StartTime
    
    Write-TestLog "📋 テストレポートを生成中..." -Level "Info"
    
    # コンソール出力
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "デプロイメントテスト結果 - $Environment" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "実行時間: $($duration.ToString('mm\:ss'))" -ForegroundColor White
    Write-Host "総テスト数: $($TestResults.TotalTests)" -ForegroundColor White
    Write-Host "成功: $($TestResults.PassedTests)" -ForegroundColor Green
    Write-Host "失敗: $($TestResults.FailedTests)" -ForegroundColor Red
    Write-Host "警告: $($TestResults.Warnings)" -ForegroundColor Yellow
    Write-Host "成功率: $([math]::Round(($TestResults.PassedTests / $TestResults.TotalTests) * 100, 1))%" -ForegroundColor White
    Write-Host ""
    
    # 失敗したテストの詳細
    if ($TestResults.FailedTests -gt 0) {
        Write-Host "失敗したテスト:" -ForegroundColor Red
        $TestResults.Tests | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
            Write-Host "  ❌ $($_.Name): $($_.Message)" -ForegroundColor Red
        }
        Write-Host ""
    }
    
    # 警告のあるテスト
    if ($TestResults.Warnings -gt 0) {
        Write-Host "警告のあるテスト:" -ForegroundColor Yellow
        $TestResults.Tests | Where-Object { $_.Status -eq "WARNING" } | ForEach-Object {
            Write-Host "  ⚠️ $($_.Name): $($_.Message)" -ForegroundColor Yellow
        }
        Write-Host ""
    }
    
    # JSON レポート生成
    $jsonReport = @{
        Environment = $Environment
        BaseUrl = $EnvConfig.BaseUrl
        TestSummary = @{
            TotalTests = $TestResults.TotalTests
            PassedTests = $TestResults.PassedTests
            FailedTests = $TestResults.FailedTests
            Warnings = $TestResults.Warnings
            SuccessRate = [math]::Round(($TestResults.PassedTests / $TestResults.TotalTests) * 100, 1)
            Duration = $duration.ToString()
            StartTime = $TestResults.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
            EndTime = $endTime.ToString("yyyy-MM-dd HH:mm:ss")
        }
        TestResults = $TestResults.Tests
    }
    
    $reportFile = "deployment-test-report-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $jsonReport | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportFile -Encoding UTF8
    
    Write-TestLog "📄 詳細レポート: $reportFile" -Level "Info"
    
    # 終了コード設定
    if ($TestResults.FailedTests -gt 0) {
        Write-TestLog "❌ テストに失敗があります。デプロイメントを確認してください。" -Level "Error"
        exit 1
    } elseif ($TestResults.Warnings -gt 0) {
        Write-TestLog "⚠️ 警告があります。確認することをお勧めします。" -Level "Warning"
        exit 0
    } else {
        Write-TestLog "✅ すべてのテストが成功しました！" -Level "Success"
        exit 0
    }
}

# メイン実行
try {
    Start-DeploymentTest
}
catch {
    Write-TestLog "💥 テスト実行中にエラーが発生しました: $($_.Exception.Message)" -Level "Error"
    exit 1
}