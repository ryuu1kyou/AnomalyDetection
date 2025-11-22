Param(
  [string]$SourceRoot = "../AnomalyDetection",
  [string]$LocalizationRoot = "../AnomalyDetection/src/AnomalyDetection.Domain.Shared/Localization",
  [string]$Output = "missing-localization-keys.csv",
  [switch]$GenerateStubJson,
  [string]$StubJsonOutput = "missing-localization-stubs.json",
  [string]$StubCulture = "en"
)

Write-Host "Scanning source for localization key usages..."
$patternCs = 'L\("([^"]+)"\)'
$patternTs = 'translate\.instant\("([^"]+)"'  # Angular instant translate pattern

$csFiles = Get-ChildItem -Path $SourceRoot -Recurse -Include *.cs -ErrorAction SilentlyContinue
$tsFiles = Get-ChildItem -Path $SourceRoot -Recurse -Include *.ts -ErrorAction SilentlyContinue

$keyUsages = @{}
foreach ($f in $csFiles) {
  $content = Get-Content $f.FullName -Raw
  foreach ($m in [regex]::Matches($content, $patternCs)) { $keyUsages[$m.Groups[1].Value] = $true }
}
foreach ($f in $tsFiles) {
  $content = Get-Content $f.FullName -Raw
  foreach ($m in [regex]::Matches($content, $patternTs)) { $keyUsages[$m.Groups[1].Value] = $true }
}

Write-Host "Collected" $keyUsages.Count "unique keys referenced."

# Collect defined keys from JSON resource files
$defined = @{}
if (Test-Path $LocalizationRoot) {
  $jsonFiles = Get-ChildItem -Path $LocalizationRoot -Recurse -Include *.json -ErrorAction SilentlyContinue
  foreach ($jf in $jsonFiles) {
    try { $json = Get-Content $jf.FullName -Raw | ConvertFrom-Json } catch { continue }
    # ABP JSON resources typically have: { "culture": "en", "texts": { "Key":"Value" } }
    if ($json.texts) {
      $json.texts.PSObject.Properties | ForEach-Object { $defined[$_.Name] = $true }
    } else {
      # Fallback: treat top-level properties as keys
      $json.PSObject.Properties | ForEach-Object { if ($_.Name -ne 'culture') { $defined[$_.Name] = $true } }
    }
  }
}
Write-Host "Collected" $defined.Count "defined localization keys."

$missing = $keyUsages.Keys | Where-Object { -not $defined.ContainsKey($_) } | Sort-Object
Write-Host "Missing keys:" $missing.Count

"Key" | Out-File $Output -Encoding UTF8
$missing | ForEach-Object { $_ | Out-File $Output -Append -Encoding UTF8 }
Write-Host "Missing keys exported to $Output"

if ($GenerateStubJson) {
  $stub = [ordered]@{
    culture = $StubCulture
    texts = [ordered]@{}
  }
  foreach ($k in $missing) { $stub.texts[$k] = $k }
  $json = $stub | ConvertTo-Json -Depth 5
  $json | Out-File $StubJsonOutput -Encoding UTF8
  Write-Host "Stub localization JSON written to $StubJsonOutput (culture=$StubCulture)"
}
