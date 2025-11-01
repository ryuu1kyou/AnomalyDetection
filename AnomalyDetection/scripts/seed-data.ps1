# シードデータ挿入スクリプト
# データベースが既に作成されていることを前提に、IDataSeederを直接呼び出してシードデータを挿入

Write-Host "データベースへのシードデータ挿入を開始します..." -ForegroundColor Green

$projectPath = "src\AnomalyDetection.DbMigrator\AnomalyDetection.DbMigrator.csproj"

Write-Host "DbMigratorプロジェクトをビルド中..." -ForegroundColor Yellow
dotnet build $projectPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "ビルド成功" -ForegroundColor Green
    
    Write-Host "`nシードデータを挿入中..." -ForegroundColor Yellow
    Write-Host "注意: 接続文字列の問題により、DbMigratorが動作しない可能性があります。" -ForegroundColor Red
    Write-Host "別の方法でシードデータを挿入する必要があるかもしれません。" -ForegroundColor Red
    
    dotnet run --project $projectPath
}
else {
    Write-Host "ビルド失敗" -ForegroundColor Red
    exit 1
}

Write-Host "`nシードデータ:" -ForegroundColor Cyan
Write-Host "- OEMマスター: Toyota, Honda, Nissan" -ForegroundColor White
Write-Host "- CANシステムカテゴリ: 12カテゴリ" -ForegroundColor White  
Write-Host "- 標準CANシグナル: EngineRPM, VehicleSpeed等" -ForegroundColor White
Write-Host "- 標準検出ロジック: エンジン回転数範囲外検出等" -ForegroundColor White
Write-Host "- デフォルト管理者: admin@abp.io / 1q2w3E*" -ForegroundColor White
