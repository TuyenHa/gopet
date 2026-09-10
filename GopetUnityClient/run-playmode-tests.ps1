<#
    Chay PlayMode test bang dong lenh.

        powershell -ExecutionPolicy Bypass -File run-playmode-tests.ps1

    PHAI DONG Unity Editor truoc: batchmode can khoa project, mo Editor thi no
    tu choi ngay.

    KHONG tin exit code cua Unity. Tren may nay batchmode crash luc THOAT vi
    khong resolve duoc cdp.cloud.unity3d.com (endpoint analytics), sau khi da
    ghi xong ket qua. Vi vay script doc file XML ket qua de ket luan.

    File nay giu ASCII thuan: Windows PowerShell 5.1 doc .ps1 khong BOM theo
    codepage ANSI, nen ky tu co dau se lam hong parser.
#>

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$unity = $env:UNITY_EXE
if (-not $unity) { $unity = "D:\Unity Editor\6000.5.4f1\Editor\Unity.exe" }

$results = Join-Path $root "Logs\playmode-results.xml"
$log = Join-Path $root "Logs\playmode-run.log"

if (-not (Test-Path $unity)) {
    Write-Host "Khong thay Unity tai: $unity" -ForegroundColor Red
    Write-Host "Dat bien moi truong UNITY_EXE de tro toi cho khac." -ForegroundColor Yellow
    exit 2
}

if (Get-Process Unity -ErrorAction SilentlyContinue) {
    Write-Host "Unity Editor dang mo - batchmode khong gianh duoc khoa project." -ForegroundColor Red
    Write-Host "Dong Editor roi chay lai." -ForegroundColor Yellow
    exit 2
}

Remove-Item $results -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force (Split-Path $results) | Out-Null

Write-Host "Chay PlayMode test (co the mat vai phut lan dau)..." -ForegroundColor Cyan

& $unity -batchmode -nographics -projectPath $root `
    -runTests -testPlatform PlayMode `
    -testResults $results -logFile $log | Out-Null

if (-not (Test-Path $results)) {
    Write-Host "Khong sinh ra file ket qua. 30 dong cuoi cua log:" -ForegroundColor Red
    if (Test-Path $log) { Get-Content $log -Tail 30 }
    exit 1
}

[xml]$xml = Get-Content $results
$run = $xml.'test-run'

$total  = [int]$run.total
$passed = [int]$run.passed
$failed = [int]$run.failed

Write-Host ""
if ($failed -gt 0) {
    # In ten tung test hong, khong bat nguoi doc mo file XML.
    $xml.SelectNodes("//test-case[@result='Failed']") | ForEach-Object {
        Write-Host "  [FAIL] $($_.fullname)" -ForegroundColor Red
        if ($_.failure.message) { Write-Host "         $($_.failure.'#cdata-section')" -ForegroundColor DarkRed }
    }
    Write-Host ""
    Write-Host "PLAYMODE THAT BAI: $failed/$total" -ForegroundColor Red
    exit 1
}

Write-Host "PLAYMODE OK: $passed/$total" -ForegroundColor Green
