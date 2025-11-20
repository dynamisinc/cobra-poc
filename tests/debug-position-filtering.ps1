# Debug script to test position filtering

$baseUrl = "http://localhost:5000"

Write-Host "`n=== Testing Position Filtering ===" -ForegroundColor Cyan

# Test 1: Check what "Safety Officer" sees
Write-Host "`n1. Fetching checklists for Safety Officer..." -ForegroundColor Yellow
$safetyOfficerChecklists = Invoke-RestMethod -Uri "$baseUrl/api/checklists/my-checklists" -Headers @{
    'X-User-Email' = 'test@example.com'
    'X-User-FullName' = 'Test User'
    'X-User-Position' = 'Safety Officer'
    'X-User-IsAdmin' = 'false'
}

Write-Host "Safety Officer sees $($safetyOfficerChecklists.Count) checklists:" -ForegroundColor Green
foreach ($checklist in $safetyOfficerChecklists) {
    Write-Host "  - $($checklist.name)" -ForegroundColor White
    Write-Host "    Assigned to: $($checklist.assignedPositions)" -ForegroundColor Gray
}

# Test 2: Check what "Operations Section Chief" sees
Write-Host "`n2. Fetching checklists for Operations Section Chief..." -ForegroundColor Yellow
$opsChiefChecklists = Invoke-RestMethod -Uri "$baseUrl/api/checklists/my-checklists" -Headers @{
    'X-User-Email' = 'ops@example.com'
    'X-User-FullName' = 'Ops Chief'
    'X-User-Position' = 'Operations Section Chief'
    'X-User-IsAdmin' = 'false'
}

Write-Host "Operations Section Chief sees $($opsChiefChecklists.Count) checklists:" -ForegroundColor Green
foreach ($checklist in $opsChiefChecklists) {
    Write-Host "  - $($checklist.name)" -ForegroundColor White
    Write-Host "    Assigned to: $($checklist.assignedPositions)" -ForegroundColor Gray
}

# Test 3: Check what checklists with specific assignment the Ops Chief sees
Write-Host "`n3. Checking if Ops Chief sees 'Safety Officer,Incident Commander' checklists..." -ForegroundColor Yellow
$safetyAssignedChecklists = $opsChiefChecklists | Where-Object { $_.assignedPositions -eq "Safety Officer,Incident Commander" }
if ($safetyAssignedChecklists) {
    Write-Host "  ❌ BUG: Ops Chief CAN see Safety Officer checklists!" -ForegroundColor Red
    foreach ($checklist in $safetyAssignedChecklists) {
        Write-Host "    - $($checklist.name)" -ForegroundColor Red
    }
} else {
    Write-Host "  ✅ CORRECT: Ops Chief cannot see Safety Officer checklists" -ForegroundColor Green
}

# Test 4: Check what null-assigned checklists the Ops Chief sees
Write-Host "`n4. Checking null-assigned checklists (should be visible to everyone)..." -ForegroundColor Yellow
$nullAssignedChecklists = $opsChiefChecklists | Where-Object { $null -eq $_.assignedPositions -or $_.assignedPositions -eq "" }
Write-Host "  Ops Chief sees $($nullAssignedChecklists.Count) null-assigned checklists" -ForegroundColor Green
foreach ($checklist in $nullAssignedChecklists) {
    Write-Host "    - $($checklist.name)" -ForegroundColor White
}

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
