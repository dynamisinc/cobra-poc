# PowerShell script to stop all COBRA development services

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Stopping COBRA Services" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Stop and remove PowerShell jobs
$jobs = Get-Job -Name "backend", "teamsbot", "frontend" -ErrorAction SilentlyContinue
if ($jobs) {
    Write-Host "Stopping PowerShell jobs..." -ForegroundColor Yellow
    $jobs | Stop-Job
    $jobs | Remove-Job -Force
    Write-Host "  Jobs stopped." -ForegroundColor Green
} else {
    Write-Host "  No PowerShell jobs found." -ForegroundColor Gray
}

# Kill backend process
$backend = Get-Process -Name "CobraAPI" -ErrorAction SilentlyContinue
if ($backend) {
    Write-Host "Killing CobraAPI (backend)..." -ForegroundColor Yellow
    $backend | ForEach-Object {
        Write-Host "  PID $($_.Id)" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force
    }
    Write-Host "  Backend stopped." -ForegroundColor Green
} else {
    Write-Host "  Backend not running." -ForegroundColor Gray
}

# Kill TeamsBot process
$bot = Get-Process -Name "CobraAPI.TeamsBot" -ErrorAction SilentlyContinue
if ($bot) {
    Write-Host "Killing CobraAPI.TeamsBot..." -ForegroundColor Yellow
    $bot | ForEach-Object {
        Write-Host "  PID $($_.Id)" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force
    }
    Write-Host "  TeamsBot stopped." -ForegroundColor Green
} else {
    Write-Host "  TeamsBot not running." -ForegroundColor Gray
}

# Kill frontend (node/vite) - optional, only if started by our script
$frontend = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -and $_.CommandLine -match "vite.*dev"
}
if ($frontend) {
    Write-Host "Killing Frontend (Vite)..." -ForegroundColor Yellow
    $frontend | ForEach-Object {
        Write-Host "  PID $($_.ProcessId)" -ForegroundColor Gray
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }
    Write-Host "  Frontend stopped." -ForegroundColor Green
} else {
    Write-Host "  Frontend not running." -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  All services stopped." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
