# PowerShell script to start backend, teamsbot, and frontend
# - Kills backend and teamsbot if already running
# - Only starts frontend if not already running
# - Streams live logs to console
# - Saves logs to files for review/sharing

$ErrorActionPreference = 'Stop'

# Configuration
$backendPort = 5000
$botPort = 3978
$frontendPort = 5188
$logsDir = "logs"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

# Create logs directory
if (-not (Test-Path $logsDir)) {
    New-Item -ItemType Directory -Path $logsDir | Out-Null
}

# Log file paths
$backendLog = "$logsDir/backend_$timestamp.log"
$botLog = "$logsDir/teamsbot_$timestamp.log"
$frontendLog = "$logsDir/frontend_$timestamp.log"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  COBRA Development Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kill all CobraAPI.exe processes for backend
Get-Process | Where-Object { $_.ProcessName -eq 'CobraAPI' } | ForEach-Object {
    Write-Host "Killing CobraAPI.exe process $($_.Id)" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force
}

# Kill all CobraAPI.TeamsBot.exe processes for teamsbot
Get-Process | Where-Object { $_.ProcessName -eq 'CobraAPI.TeamsBot' } | ForEach-Object {
    Write-Host "Killing CobraAPI.TeamsBot.exe process $($_.Id)" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force
}

# Clean up any previous jobs
Get-Job -Name "backend", "teamsbot", "frontend" -ErrorAction SilentlyContinue | Remove-Job -Force

# Backend
$backendPath = "src/backend/CobraAPI"
Write-Host "--- Starting Backend on port $backendPort ---" -ForegroundColor Green
Start-Job -ScriptBlock {
    Set-Location $using:backendPath
    dotnet run --urls "http://localhost:$using:backendPort" 2>&1
} -Name "backend" | Out-Null

# TeamsBot
$botPath = "src/backend/CobraAPI.TeamsBot"
Write-Host "--- Starting TeamsBot on port $botPort ---" -ForegroundColor Green
Start-Job -ScriptBlock {
    Set-Location $using:botPath
    dotnet run --urls "http://localhost:$using:botPort" 2>&1
} -Name "teamsbot" | Out-Null

# Frontend
$frontendPath = "src/frontend"
$frontendUrl = "http://localhost:$frontendPort/"
$frontendRunning = Get-CimInstance Win32_Process | Where-Object {
    $_.CommandLine -and $_.CommandLine -match "vite.*dev"
}
if (-not $frontendRunning) {
    Write-Host "--- Starting Frontend on port $frontendPort ---" -ForegroundColor Green
    Start-Job -ScriptBlock {
        Set-Location $using:frontendPath
        npm run dev 2>&1
    } -Name "frontend" | Out-Null
} else {
    Write-Host "Frontend already running." -ForegroundColor Yellow
}

# Wait a moment for services to start
Write-Host ""
Write-Host "Waiting for services to start..." -ForegroundColor Gray
Start-Sleep -Seconds 3

# Display URLs and log locations
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Services:" -ForegroundColor Cyan
Write-Host "  - Backend API:     http://localhost:$backendPort" -ForegroundColor White
Write-Host "  - Backend Swagger: http://localhost:$backendPort/swagger" -ForegroundColor White
Write-Host "  - TeamsBot:        http://localhost:$botPort/api/messages" -ForegroundColor White
Write-Host "  - Frontend:        $frontendUrl" -ForegroundColor White
Write-Host ""
Write-Host "  Log files:" -ForegroundColor Cyan
Write-Host "  - Backend:  $backendLog" -ForegroundColor Gray
Write-Host "  - TeamsBot: $botLog" -ForegroundColor Gray
Write-Host "  - Frontend: $frontendLog" -ForegroundColor Gray
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop streaming (services will keep running)" -ForegroundColor Yellow
Write-Host "To stop services: Get-Job | Stop-Job; Get-Job | Remove-Job" -ForegroundColor Gray
Write-Host ""

# Color mapping for services
$colors = @{
    "backend" = "Green"
    "teamsbot" = "Cyan"
    "frontend" = "Magenta"
}

$logFiles = @{
    "backend" = $backendLog
    "teamsbot" = $botLog
    "frontend" = $frontendLog
}

# Stream logs continuously
try {
    while ($true) {
        $hasOutput = $false
        Get-Job | ForEach-Object {
            $jobName = $_.Name
            $output = Receive-Job -Id $_.Id 2>&1
            if ($output) {
                $hasOutput = $true
                $color = $colors[$jobName]
                if (-not $color) { $color = "White" }

                $output | ForEach-Object {
                    $line = $_
                    $timestamp = Get-Date -Format "HH:mm:ss"
                    $logLine = "[$timestamp][$jobName] $line"

                    # Write to console with color
                    Write-Host "[$timestamp]" -NoNewline -ForegroundColor DarkGray
                    Write-Host "[$jobName]" -NoNewline -ForegroundColor $color
                    Write-Host " $line"

                    # Append to log file
                    $logFile = $logFiles[$jobName]
                    if ($logFile) {
                        Add-Content -Path $logFile -Value $logLine
                    }
                }
            }
        }

        # Check if any jobs have failed
        $failedJobs = Get-Job | Where-Object { $_.State -eq 'Failed' }
        if ($failedJobs) {
            Write-Host ""
            Write-Host "WARNING: Some jobs have failed!" -ForegroundColor Red
            $failedJobs | ForEach-Object {
                Write-Host "  - $($_.Name): $($_.State)" -ForegroundColor Red
            }
        }

        if (-not $hasOutput) {
            Start-Sleep -Milliseconds 500
        }
    }
}
finally {
    Write-Host ""
    Write-Host "Log streaming stopped. Services are still running in background." -ForegroundColor Yellow
    Write-Host "Log files saved to: $logsDir/" -ForegroundColor Gray
}
