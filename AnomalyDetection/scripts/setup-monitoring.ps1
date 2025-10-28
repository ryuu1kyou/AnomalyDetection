# CAN異常検出管理システム - 監視セットアップスクリプト (PowerShell)

param(
    [string]$Environment = "development",
    [switch]$SkipPrompts = $false
)

Write-Host "🔍 CAN異常検出管理システム 監視セットアップを開始します..." -ForegroundColor Green

# 設定
$MonitoringComposeFile = "docker-compose.monitoring.yml"
$MainComposeFile = if ($Environment -eq "production") { "docker-compose.prod.yml" } else { "docker-compose.yml" }

# 必要なディレクトリを作成
$Directories = @(
    "monitoring/prometheus",
    "monitoring/grafana/dashboards",
    "monitoring/grafana/datasources",
    "monitoring/alertmanager",
    "monitoring/logstash/pipeline",
    "monitoring/logstash/config",
    "monitoring/filebeat",
    "logs"
)

Write-Host "📁 必要なディレクトリを作成します..." -ForegroundColor Cyan
foreach ($dir in $Directories) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "  ✅ 作成: $dir" -ForegroundColor Green
    } else {
        Write-Host "  ℹ️ 存在: $dir" -ForegroundColor Yellow
    }
}

# Grafana データソース設定を作成
Write-Host "📊 Grafana データソース設定を作成します..." -ForegroundColor Cyan
$GrafanaDatasource = @"
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    editable: true
    
  - name: Elasticsearch
    type: elasticsearch
    access: proxy
    url: http://elasticsearch:9200
    database: "logstash-*"
    interval: Daily
    timeField: "@timestamp"
    
  - name: Jaeger
    type: jaeger
    access: proxy
    url: http://jaeger:16686
"@

$GrafanaDatasource | Out-File -FilePath "monitoring/grafana/datasources/datasources.yml" -Encoding UTF8

# Grafana ダッシュボード設定を作成
$GrafanaDashboardConfig = @"
apiVersion: 1

providers:
  - name: 'default'
    orgId: 1
    folder: ''
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /etc/grafana/provisioning/dashboards
"@

$GrafanaDashboardConfig | Out-File -FilePath "monitoring/grafana/dashboards/dashboards.yml" -Encoding UTF8

# Filebeat設定を作成
Write-Host "📋 Filebeat設定を作成します..." -ForegroundColor Cyan
$FilebeatConfig = @"
filebeat.inputs:
- type: container
  paths:
    - '/var/lib/docker/containers/*/*.log'
  processors:
    - add_docker_metadata:
        host: "unix:///var/run/docker.sock"

- type: log
  enabled: true
  paths:
    - /var/log/app/*.log
  fields:
    service: anomaly-detection
    environment: $Environment
  fields_under_root: true

output.elasticsearch:
  hosts: ["elasticsearch:9200"]
  index: "anomaly-detection-logs-%{+yyyy.MM.dd}"

setup.template.name: "anomaly-detection"
setup.template.pattern: "anomaly-detection-*"

logging.level: info
logging.to_files: true
logging.files:
  path: /var/log/filebeat
  name: filebeat
  keepfiles: 7
  permissions: 0644
"@

$FilebeatConfig | Out-File -FilePath "monitoring/filebeat/filebeat.yml" -Encoding UTF8

# Logstash パイプライン設定を作成
Write-Host "🔄 Logstash設定を作成します..." -ForegroundColor Cyan
$LogstashPipeline = @"
input {
  beats {
    port => 5044
  }
}

filter {
  if [container][name] {
    mutate {
      add_field => { "service_name" => "%{[container][name]}" }
    }
  }
  
  # JSON ログの解析
  if [message] =~ /^\{.*\}$/ {
    json {
      source => "message"
    }
  }
  
  # .NET ログレベルの正規化
  if [Level] {
    mutate {
      add_field => { "log_level" => "%{Level}" }
    }
  }
  
  # タイムスタンプの解析
  if [Timestamp] {
    date {
      match => [ "Timestamp", "ISO8601" ]
    }
  }
  
  # エラーログの特別処理
  if [log_level] == "Error" or [log_level] == "Fatal" {
    mutate {
      add_tag => [ "error" ]
    }
  }
}

output {
  elasticsearch {
    hosts => ["elasticsearch:9200"]
    index => "anomaly-detection-logs-%{+YYYY.MM.dd}"
  }
  
  # デバッグ用（開発環境のみ）
  if "development" in [environment] {
    stdout { codec => rubydebug }
  }
}
"@

$LogstashPipeline | Out-File -FilePath "monitoring/logstash/pipeline/logstash.conf" -Encoding UTF8

# 環境変数ファイルを作成
Write-Host "🔧 監視用環境変数ファイルを作成します..." -ForegroundColor Cyan
$MonitoringEnv = @"
# Grafana設定
GRAFANA_ADMIN_PASSWORD=admin123

# アラート設定
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
ALERT_EMAIL_FROM=alerts@yourdomain.com
CRITICAL_ALERT_EMAIL=critical@yourdomain.com
DBA_EMAIL=dba@yourdomain.com
SECURITY_TEAM_EMAIL=security@yourdomain.com

# Slack設定
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK

# SMS設定（オプション）
SMS_WEBHOOK_URL=https://your-sms-service.com/webhook

# Jaeger設定
JAEGER_ENDPOINT=http://jaeger:14250
"@

$MonitoringEnv | Out-File -FilePath ".env.monitoring" -Encoding UTF8

# Docker Composeファイルの確認
Write-Host "🐳 Docker Compose設定を確認します..." -ForegroundColor Cyan
if (!(Test-Path $MonitoringComposeFile)) {
    Write-Host "❌ $MonitoringComposeFile が見つかりません" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $MainComposeFile)) {
    Write-Host "❌ $MainComposeFile が見つかりません" -ForegroundColor Red
    exit 1
}

# 監視スタックを起動
if (!$SkipPrompts) {
    $response = Read-Host "監視スタックを起動しますか？ (y/N)"
    if ($response -ne "y" -and $response -ne "Y") {
        Write-Host "セットアップのみ完了しました。" -ForegroundColor Yellow
        exit 0
    }
}

Write-Host "🚀 監視スタックを起動します..." -ForegroundColor Green

# メインアプリケーションが起動していることを確認
Write-Host "📋 メインアプリケーションの状態を確認します..." -ForegroundColor Cyan
$mainContainers = docker-compose -f $MainComposeFile ps -q
if ($mainContainers.Count -eq 0) {
    Write-Host "⚠️ メインアプリケーションが起動していません。先に起動してください。" -ForegroundColor Yellow
    Write-Host "   docker-compose -f $MainComposeFile up -d" -ForegroundColor Gray
}

# 監視スタックを起動
try {
    Write-Host "🔍 監視コンテナを起動します..." -ForegroundColor Cyan
    docker-compose -f $MonitoringComposeFile --env-file .env.monitoring up -d
    
    Write-Host "⏳ サービスの起動を待機します..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # ヘルスチェック
    Write-Host "🏥 サービスのヘルスチェックを実行します..." -ForegroundColor Cyan
    
    $services = @(
        @{Name="Prometheus"; Url="http://localhost:9090/-/healthy"; Port=9090},
        @{Name="Grafana"; Url="http://localhost:3000/api/health"; Port=3000},
        @{Name="Alertmanager"; Url="http://localhost:9093/-/healthy"; Port=9093},
        @{Name="Elasticsearch"; Url="http://localhost:9200/_cluster/health"; Port=9200}
    )
    
    foreach ($service in $services) {
        try {
            $response = Invoke-WebRequest -Uri $service.Url -UseBasicParsing -TimeoutSec 10
            if ($response.StatusCode -eq 200) {
                Write-Host "  ✅ $($service.Name) is healthy" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "  ⚠️ $($service.Name) health check failed: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "🎉 監視セットアップが完了しました！" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 アクセス情報:" -ForegroundColor Cyan
    Write-Host "   Grafana:      http://localhost:3000 (admin/admin123)"
    Write-Host "   Prometheus:   http://localhost:9090"
    Write-Host "   Alertmanager: http://localhost:9093"
    Write-Host "   Jaeger:       http://localhost:16686"
    Write-Host "   Kibana:       http://localhost:5601"
    Write-Host ""
    Write-Host "🔧 管理コマンド:" -ForegroundColor Cyan
    Write-Host "   監視停止:     docker-compose -f $MonitoringComposeFile down"
    Write-Host "   ログ確認:     docker-compose -f $MonitoringComposeFile logs -f [service-name]"
    Write-Host "   再起動:       docker-compose -f $MonitoringComposeFile restart [service-name]"
    
}
catch {
    Write-Host "❌ 監視スタックの起動に失敗しました: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "📚 次のステップ:" -ForegroundColor Yellow
Write-Host "1. Grafanaにログインしてダッシュボードを確認"
Write-Host "2. Alertmanagerでアラート設定を確認"
Write-Host "3. アプリケーションログがKibanaに表示されることを確認"
Write-Host "4. Prometheusでメトリクスが収集されていることを確認"