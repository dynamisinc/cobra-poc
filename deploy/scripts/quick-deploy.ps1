<#
.SYNOPSIS
    Quick deployment script using git push (the method that actually works).

.DESCRIPTION
    Builds frontend, copies to wwwroot, commits, and pushes to Azure via git.
    This uses "git push azure HEAD:master" which is the reliable deployment method.

.PARAMETER Message
    Optional commit message. Defaults to "deploy: <timestamp>".

.PARAMETER SkipBuild
    Skip frontend build (use existing dist folder).

.PARAMETER SkipTests
    Skip running backend tests.

.EXAMPLE
    .\quick-deploy.ps1

.EXAMPLE
    .\quick-deploy.ps1 -Message "fix: item completion bug"

.EXAMPLE
    .\quick-deploy.ps1 -SkipBuild -SkipTests
#>

param(
    [string]$Message = "",
    [switch]$SkipBuild,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

# Paths
$RepoRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$FrontendDir = Join-Path $RepoRoot "src\frontend"
$WwwrootDir = Join-Path $RepoRoot "src\backend\ChecklistAPI\wwwroot"
$DistDir = Join-Path $FrontendDir "dist"
$AppUrl = "https://checklist-poc-app.azurewebsites.net"

function Write-Step($step, $total, $text) {
    Write-Host "[$step/$total] $text" -ForegroundColor Yellow
}

function Write-Success($text) {
    Write-Host "  $text" -ForegroundColor Green
}

function Write-Info($text) {
    Write-Host "  $text" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Quick Deploy to Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Set-Location $RepoRoot

# 1. Verify Azure remote
Write-Step 1 6 "Checking git remote..."
$remotes = git remote -v 2>&1
if ($remotes -notmatch "azure") {
    Write-Host "ERROR: Azure remote not configured." -ForegroundColor Red
    Write-Host "Add it with: git remote add azure <your-azure-git-url>" -ForegroundColor Gray
    exit 1
}
Write-Success "Azure remote configured"

# 2. Run tests (optional)
if (-not $SkipTests) {
    Write-Step 2 6 "Running tests..."
    Push-Location (Join-Path $RepoRoot "src\backend")
    $testOutput = dotnet test --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Tests failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Pop-Location
    Write-Success "All tests passed"
} else {
    Write-Step 2 6 "Skipping tests"
}

# 3. Build frontend
if (-not $SkipBuild) {
    Write-Step 3 6 "Building frontend..."
    Push-Location $FrontendDir

    if (-not (Test-Path "node_modules")) {
        Write-Info "Installing dependencies..."
        npm install 2>&1 | Out-Null
    }

    npm run build 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    Pop-Location
    Write-Success "Frontend built"
} else {
    Write-Step 3 6 "Skipping build"
}

# 4. Copy to wwwroot (IMPORTANT: clean first!)
Write-Step 4 6 "Copying to wwwroot..."
if (Test-Path $WwwrootDir) {
    Remove-Item "$WwwrootDir\*" -Recurse -Force -ErrorAction SilentlyContinue
}
Copy-Item "$DistDir\*" $WwwrootDir -Recurse -Force

$jsFile = (Get-ChildItem "$WwwrootDir\assets\*.js" | Select-Object -First 1).Name
Write-Success "Copied (bundle: $jsFile)"

# 5. Commit
Write-Step 5 6 "Committing changes..."
git add -A 2>&1 | Out-Null

$hasChanges = git status --porcelain 2>&1
if ([string]::IsNullOrWhiteSpace($hasChanges)) {
    Write-Info "No changes to commit"
} else {
    if ([string]::IsNullOrWhiteSpace($Message)) {
        $Message = "deploy: $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
    }
    git commit -m $Message 2>&1 | Out-Null
    Write-Success "Committed: $Message"
}

# 6. Push to Azure (MUST be master, not main!)
Write-Step 6 6 "Pushing to Azure (master branch)..."
$pushOutput = git push azure HEAD:master 2>&1
if ($pushOutput -match "Everything up-to-date") {
    Write-Info "Already up to date"
} elseif ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Push failed!" -ForegroundColor Red
    Write-Host $pushOutput -ForegroundColor Red
    exit 1
} else {
    Write-Success "Pushed to Azure"
}

# Done
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "App: $AppUrl" -ForegroundColor White
Write-Host "API: $AppUrl/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Verify with: curl $AppUrl/api/templates" -ForegroundColor Gray
Write-Host ""
