# CAN異常検出管理システム - Docker ヘルスチェックスクリプト (PowerShell)

param(
    [string]$ComposeFile = "docker-compose.yml",
    [string]$EnvFile = ".env.development",
    [int]$Timeout = 30,
    [int]$RetryCount = 3
)

# 色付きログ関数
function Write-InfoLog {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-WarnLog {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-ErrorLog {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# ヘルスチェック関数
function Test-ServiceHealth {
    param(
        [string]$ServiceName,
        [string]$HealthUrl,
        [int]$ExpectedStatus = 200
    )
    
    Write-InfoLog "Checking $ServiceName health..."
    
    for ($i = 1; $i -le $RetryCount; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $HealthUrl -UseBasicParsing -TimeoutSec $Timeout
            if ($response.StatusCode -eq $ExpectedStatus) {
                Write-InfoLog "✅ $ServiceName is healthy"
                return $true
            }
        }
        catch {
            Write-WarnLog "⚠️ $ServiceName health check failed (attempt $i/$RetryCount): $($_.Exception.Message)"
            if ($i -lt $RetryCount) {
                Start-Sleep -Seconds 5
            }
        }
    }
    
    Write-ErrorLog "❌ $ServiceName health check failed after $RetryCount attempts"
    return $false
}

# コンテナ状態チェック
function Test-ContainerStatus {
    param([string]$ServiceName)
    
    Write-InfoLog "Checking $ServiceName container status..."
    
    try {
        $containerId = docker-compose -f $ComposeFile ps -q $ServiceName
        if ([string]::IsNullOrEmpty($containerId)) {
            Write-ErrorLog "❌ $ServiceName container not found"
            return $false
        }
        
        $status = docker inspect --format='{{.State.Status}}' $containerId
        
        switch ($status) {
            "running" {
                Write-InfoLog "✅ $ServiceName container is running"
                return $true
            }
            "exited" {
                Write-ErrorLog "❌ $ServiceName container has exited"
                return $false
            }
            default {
                Write-WarnLog "⚠️ $ServiceName container status: $status"
                return $false
            }
        }
    }
    catch {
        Write-ErrorLog "❌ Error checking $ServiceName container: $($_.Exception.Message)"
        return $false
    }
}

# データベース接続チェック
function Test-DatabaseConnection {
    Write-InfoLog "Checking database connection..."
    
    try {
        $dbContainer = docker-compose -f $ComposeFile ps -q sqlserver
        if ([string]::IsNullOrEmpty($dbContainer)) {
            Write-ErrorLog "❌ Database container not found"
            return $false
        }
        
        $dbPassword = if ($env:DB_SA_PASSWORD) { $env:DB_SA_PASSWORD } else { "MyPass@word123" }
        
        for ($i = 1; $i -le $RetryCount; $i++) {
            try {
                $result = docker exec $dbContainer /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $dbPassword -Q "SELECT 1" 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-InfoLog "✅ Database connection is healthy"
                    return $true
                }
            }
            catch {
                Write-WarnLog "⚠️ Database connection failed (attempt $i/$RetryCount)"
                if ($i -lt $RetryCount) {
                    Start-Sleep -Seconds 5
                }
            }
        }
        
        Write-ErrorLog "❌ Database connection failed after $RetryCount attempts"
        return $false
    }
    catch {
        Write-ErrorLog "❌ Error checking database connection: $($_.Exception.Message)"
        return $false
    }
}

# Redis接続チェック
function Test-RedisConnection {
    Write-InfoLog "Checking Redis connection..."
    
    try {
        $redisContainer = docker-compose -f $ComposeFile ps -q redis
        if ([string]::IsNullOrEmpty($redisContainer)) {
            Write-ErrorLog "❌ Redis container not found"
            return $false
        }
        
        for ($i = 1; $i -le $RetryCount; $i++) {
            try {
                $result = docker exec $redisContainer redis-cli ping
                if ($result -eq "PONG") {
                    Write-InfoLog "✅ Redis connection is healthy"
                    return $true
                }
            }
            catch {
                Write-WarnLog "⚠️ Redis connection failed (attempt $i/$RetryCount)"
                if ($i -lt $RetryCount) {
                    Start-Sleep -Seconds 5
                }
            }
        }
        
        Write-ErrorLog "❌ Redis connection failed after $RetryCount attempts"
        return $false
    }
    catch {
        Write-ErrorLog "❌ Error checking Redis connection: $($_.Exception.Message)"
        return $false
    }
}

# メイン実行
function Main {
    Write-Host "🏥 Docker ヘルスチェックを開始します..." -ForegroundColor Cyan
    
    $exitCode = 0
    
    # 環境変数読み込み
    if (Test-Path $EnvFile) {
        Get-Content $EnvFile | Where-Object { $_ -notmatch '^#' -and $_ -match '=' } | ForEach-Object {
            $key, $value = $_ -split '=', 2
            [Environment]::SetEnvironmentVariable($key, $value, "Process")
        }
    }
    
    # URL設定
    if ($ComposeFile -eq "docker-compose.prod.yml") {
        $BackendUrl = "http://localhost"
        $FrontendUrl = "http://localhost"
    } else {
        $BackendUrl = "http://localhost:44318"
        $FrontendUrl = "http://localhost:4200"
    }
    
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Docker Health Check Report" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Compose File: $ComposeFile"
    Write-Host "Environment: $EnvFile"
    Write-Host "Backend URL: $BackendUrl"
    Write-Host "Frontend URL: $FrontendUrl"
    Write-Host "==========================================" -ForegroundColor Cyan
    
    # コンテナ状態チェック
    if (-not (Test-ContainerStatus "sqlserver")) { $exitCode = 1 }
    if (-not (Test-ContainerStatus "redis")) { $exitCode = 1 }
    if (-not (Test-ContainerStatus "backend")) { $exitCode = 1 }
    if (-not (Test-ContainerStatus "frontend")) { $exitCode = 1 }
    
    # 接続チェック
    if (-not (Test-DatabaseConnection)) { $exitCode = 1 }
    if (-not (Test-RedisConnection)) { $exitCode = 1 }
    
    # サービスヘルスチェック
    if (-not (Test-ServiceHealth "Backend API" "$BackendUrl/health-status")) { $exitCode = 1 }
    if (-not (Test-ServiceHealth "Frontend" $FrontendUrl)) { $exitCode = 1 }
    
    # 詳細情報表示
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Container Details" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    docker-compose -f $ComposeFile ps
    
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "Resource Usage" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.NetIO}}\t{{.BlockIO}}"
    
    if ($exitCode -eq 0) {
        Write-InfoLog "🎉 All health checks passed!"
    } else {
        Write-ErrorLog "💥 Some health checks failed!"
    }
    
    return $exitCode
}

# スクリプト実行
try {
    $result = Main
    exit $result
}
catch {
    Write-ErrorLog "Script execution failed: $($_.Exception.Message)"
    exit 1
}