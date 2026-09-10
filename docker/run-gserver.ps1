<#
    Chay GServer voi credential lay tu docker/.env.

        powershell -ExecutionPolicy Bypass -File docker\run-gserver.ps1

    Tranh phai go tay bien moi truong moi lan, va tranh viec mat khau
    lot vao lich su shell.

    File nay giu ASCII thuan: Windows PowerShell 5.1 doc .ps1 khong BOM
    theo codepage ANSI, nen ky tu co dau se lam hong parser.
#>

$ErrorActionPreference = 'Stop'

$dockerDir = $PSScriptRoot
$serverDir = Join-Path (Split-Path $dockerDir -Parent) "SRCGOPETGOC\GServer"
$exe = Join-Path $serverDir "bin\Debug\net8.0\Gopet.exe"
$envFile = Join-Path $dockerDir ".env"

if (-not (Test-Path $envFile)) {
    Write-Host "Thieu $envFile - sao chep .env.example thanh .env" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $exe)) {
    Write-Host "Chua build GServer. Chay truoc:" -ForegroundColor Red
    Write-Host "  cd `"$serverDir`"; dotnet build" -ForegroundColor Yellow
    exit 1
}

# Kiem tra container co chay khong - loi o day de hieu hon nhieu so voi
# mot exception ket noi giua luc nap template.
$running = docker ps --filter "name=gopet-mariadb" --format "{{.Names}}" 2>$null
if ($running -ne "gopet-mariadb") {
    Write-Host "Container gopet-mariadb chua chay. Chay truoc:" -ForegroundColor Red
    Write-Host "  cd `"$dockerDir`"; docker compose up -d" -ForegroundColor Yellow
    exit 1
}

$password = (Get-Content $envFile |
    Where-Object { $_ -match '^\s*MARIADB_ROOT_PASSWORD\s*=' } |
    Select-Object -First 1) -replace '^\s*MARIADB_ROOT_PASSWORD\s*=\s*', ''

if ([string]::IsNullOrWhiteSpace($password)) {
    Write-Host "Khong doc duoc MARIADB_ROOT_PASSWORD tu .env" -ForegroundColor Red
    exit 1
}

$env:GOPET_DB_HOST     = "127.0.0.1"
$env:GOPET_DB_PORT     = "3306"
$env:GOPET_DB_USER     = "root"
$env:GOPET_DB_PASSWORD = $password

Write-Host "Khoi dong GServer (game: 19180, HTTP API: 8082)..." -ForegroundColor Cyan
Write-Host ""
Write-Host "Luu y: dong 'Application started' la cua ASP.NET (cong 8082)." -ForegroundColor DarkGray
Write-Host "Cong game 19180 bind SAU do vai tram ms - doi thay no LISTENING" -ForegroundColor DarkGray
Write-Host "roi hay chay smoke test, dung tin vao dong log kia." -ForegroundColor DarkGray
Write-Host ""

Set-Location $serverDir
& $exe
