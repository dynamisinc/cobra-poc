# Detailed debug script for position filtering

$baseUrl = "http://localhost:5000"

Write-Host "`n=== Detailed Position Filtering Debug ===" -ForegroundColor Cyan

# Step 1: Get a template ID (assuming templates exist)
Write-Host "`n1. Getting templates..." -ForegroundColor Yellow
$templates = Invoke-RestMethod -Uri "$baseUrl/api/templates" -Headers @{
    'X-User-Email' = 'admin@example.com'
    'X-User-FullName' = 'Admin User'
    'X-User-Position' = 'Incident Commander'
    'X-User-IsAdmin' = 'true'
}

if ($templates.Count -eq 0) {
    Write-Host "  No templates found. Cannot create test checklist." -ForegroundColor Red
    exit
}

$templateId = $templates[0].id
Write-Host "  Using template: $($templates[0].name) ($templateId)" -ForegroundColor Green

# Step 2: Create a checklist with specific position assignment
Write-Host "`n2. Creating checklist assigned to 'Safety Officer,Incident Commander'..." -ForegroundColor Yellow
$createBody = @{
    templateId = $templateId
    name = "DEBUG Test Checklist - Safety Officers Only"
    eventId = "DEBUG-EVENT-001"
    eventName = "Debug Test Event"
    operationalPeriodId = "DEBUG-OP-001"
    operationalPeriodName = "Debug Op Period"
    assignedPositions = "Safety Officer,Incident Commander"
} | ConvertTo-Json

$newChecklist = Invoke-RestMethod -Uri "$baseUrl/api/checklists" -Method Post -Body $createBody -ContentType "application/json" -Headers @{
    'X-User-Email' = 'safety@example.com'
    'X-User-FullName' = 'Safety User'
    'X-User-Position' = 'Safety Officer'
    'X-User-IsAdmin' = 'false'
}

Write-Host "  Created checklist: $($newChecklist.id)" -ForegroundColor Green
Write-Host "  Assigned to: $($newChecklist.assignedPositions)" -ForegroundColor Gray

# Step 3: Test as Safety Officer (should see it)
Write-Host "`n3. Testing as Safety Officer (should see the checklist)..." -ForegroundColor Yellow
$safetyChecklists = Invoke-RestMethod -Uri "$baseUrl/api/checklists/my-checklists" -Headers @{
    'X-User-Email' = 'safety@example.com'
    'X-User-FullName' = 'Safety User'
    'X-User-Position' = 'Safety Officer'
    'X-User-IsAdmin' = 'false'
}

$found = $safetyChecklists | Where-Object { $_.id -eq $newChecklist.id }
if ($found) {
    Write-Host "  ✅ PASS: Safety Officer CAN see the checklist" -ForegroundColor Green
} else {
    Write-Host "  ❌ FAIL: Safety Officer CANNOT see the checklist" -ForegroundColor Red
}

# Step 4: Test as Operations Section Chief (should NOT see it)
Write-Host "`n4. Testing as Operations Section Chief (should NOT see the checklist)..." -ForegroundColor Yellow
$opsChecklists = Invoke-RestMethod -Uri "$baseUrl/api/checklists/my-checklists" -Headers @{
    'X-User-Email' = 'ops@example.com'
    'X-User-FullName' = 'Ops Chief'
    'X-User-Position' = 'Operations Section Chief'
    'X-User-IsAdmin' = 'false'
}

$found = $opsChecklists | Where-Object { $_.id -eq $newChecklist.id }
if ($found) {
    Write-Host "  ❌ FAIL: Operations Section Chief CAN see the checklist!" -ForegroundColor Red
    Write-Host "  This is the bug - position filtering is broken" -ForegroundColor Red
} else {
    Write-Host "  ✅ PASS: Operations Section Chief CANNOT see the checklist" -ForegroundColor Green
}

# Step 5: Show all checklists Operations Section Chief sees
Write-Host "`n5. All checklists visible to Operations Section Chief:" -ForegroundColor Yellow
foreach ($checklist in $opsChecklists) {
    Write-Host "  - $($checklist.name)" -ForegroundColor White
    Write-Host "    ID: $($checklist.id)" -ForegroundColor Gray
    Write-Host "    Assigned to: '$($checklist.assignedPositions)'" -ForegroundColor Gray
}

Write-Host "`n=== Debug Complete ===" -ForegroundColor Cyan
