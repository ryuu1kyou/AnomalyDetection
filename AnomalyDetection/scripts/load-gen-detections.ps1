Param(
  [string]$BaseUrl = "http://localhost:5000",
  [int]$Count = 50,
  [int]$Concurrency = 10,
  [guid]$DetectionLogicId = [guid]::Empty,
  [guid]$CanSignalId = [guid]::Empty
)

# Simple load generation script for Req9 SLA validation
# Requires an auth token in $env:API_TOKEN if the API is protected.

if (-not $DetectionLogicId -or $DetectionLogicId -eq [guid]::Empty) { Write-Host "DetectionLogicId required"; exit 1 }
if (-not $CanSignalId -or $CanSignalId -eq [guid]::Empty) { Write-Host "CanSignalId required"; exit 1 }

$uri = "$BaseUrl/api/app/anomaly-detection-result"  # Adjust if route differs
$token = $env:API_TOKEN

$semaphore = [System.Threading.SemaphoreSlim]::new($Concurrency, $Concurrency)
$tasks = @()

for ($i=0; $i -lt $Count; $i++) {
  $semaphore.Wait()
  $tasks += [System.Threading.Tasks.Task]::Factory.StartNew({
    try {
      $body = @{
        detectionLogicId = $DetectionLogicId
        canSignalId      = $CanSignalId
        anomalyLevel     = 2
        confidenceScore  = 0.85
        description      = "LoadGen result $i"
        inputData = @{ signalValue = 1.23; timestamp = [DateTime]::UtcNow }
        details = @{ detectionType = 0; triggerCondition = "load-test"; parameters = @{ sample = 1 }}
      } | ConvertTo-Json -Depth 5
      $headers = @{ 'Content-Type'='application/json' }
      if ($token) { $headers['Authorization'] = "Bearer $token" }
      Invoke-RestMethod -Method POST -Uri $uri -Headers $headers -Body $body | Out-Null
    } catch {
      Write-Warning "Request failed: $_"
    } finally {
      $semaphore.Release() | Out-Null
    }
  }) | Out-Null
}

[System.Threading.Tasks.Task]::WaitAll($tasks)
Write-Host "Completed $Count detection result create requests." -ForegroundColor Green
