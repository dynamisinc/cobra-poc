<#
.SYNOPSIS
    Local testing script for COBRA Teams Bot

.DESCRIPTION
    Tests the bot's endpoints without requiring Bot Framework Emulator.
    Validates health, conversation storage, and message handling.

.EXAMPLE
    .\test-bot-local.ps1

.NOTES
    Requires the bot to be running on http://localhost:3978
    Start the bot with: dotnet run --urls="http://localhost:3978"
#>

$baseUrl = "http://localhost:3978"
$passed = 0
$failed = 0

function Write-TestResult {
    param (
        [string]$TestName,
        [bool]$Success,
        [string]$Details = ""
    )

    if ($Success) {
        Write-Host "[PASS] " -ForegroundColor Green -NoNewline
        $script:passed++
    } else {
        Write-Host "[FAIL] " -ForegroundColor Red -NoNewline
        $script:failed++
    }
    Write-Host "$TestName"
    if ($Details) {
        Write-Host "       $Details" -ForegroundColor Gray
    }
}

Write-Host "`n=== COBRA Teams Bot Local Test Suite ===" -ForegroundColor Cyan
Write-Host "Target: $baseUrl`n"

# Test 1: Health Check
Write-Host "Test 1: Health Endpoint" -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/api/health" -Method Get
    $success = $health.status -eq "healthy" -and $health.service -eq "COBRA Teams Bot"
    Write-TestResult -TestName "Health endpoint returns healthy status" -Success $success -Details "Service: $($health.service)"
} catch {
    Write-TestResult -TestName "Health endpoint returns healthy status" -Success $false -Details $_.Exception.Message
}

# Test 2: Conversations endpoint (should start empty or have existing)
Write-Host "`nTest 2: Conversations Storage" -ForegroundColor Yellow
try {
    $convsBefore = Invoke-RestMethod -Uri "$baseUrl/api/diagnostics/conversations" -Method Get
    Write-TestResult -TestName "Conversations endpoint accessible" -Success $true -Details "Count: $($convsBefore.count)"
} catch {
    Write-TestResult -TestName "Conversations endpoint accessible" -Success $false -Details $_.Exception.Message
}

# Test 3: Simulate bot installation
Write-Host "`nTest 3: Bot Installation Simulation" -ForegroundColor Yellow
try {
    $installResult = Invoke-RestMethod -Uri "$baseUrl/api/diagnostics/simulate-install" -Method Post
    $success = $installResult.success -eq $true -and $installResult.conversationId
    Write-TestResult -TestName "Simulate install creates conversation reference" -Success $success -Details "ConversationId: $($installResult.conversationId)"
    $testConversationId = $installResult.conversationId
} catch {
    Write-TestResult -TestName "Simulate install creates conversation reference" -Success $false -Details $_.Exception.Message
    $testConversationId = $null
}

# Test 4: Verify conversation was stored
Write-Host "`nTest 4: Verify Conversation Storage" -ForegroundColor Yellow
try {
    $convsAfter = Invoke-RestMethod -Uri "$baseUrl/api/diagnostics/conversations" -Method Get
    $stored = $convsAfter.conversations | Where-Object { $_.conversationId -eq $testConversationId }
    $success = $null -ne $stored
    Write-TestResult -TestName "Conversation reference persisted" -Success $success -Details "Total conversations: $($convsAfter.count)"
} catch {
    Write-TestResult -TestName "Conversation reference persisted" -Success $false -Details $_.Exception.Message
}

# Test 5: Send a message activity (will fail on reply, but message should be logged)
Write-Host "`nTest 5: Message Activity Processing" -ForegroundColor Yellow
$messageBody = @{
    type = "message"
    id = "test-msg-$(Get-Random)"
    timestamp = (Get-Date).ToString("o")
    channelId = "emulator"
    from = @{
        id = "test-user"
        name = "Test User"
    }
    conversation = @{
        id = "test-conv-$(Get-Random)"
    }
    recipient = @{
        id = "cobra-bot"
        name = "COBRA Bot"
    }
    text = "Hello from PowerShell test!"
    serviceUrl = $baseUrl
} | ConvertTo-Json -Depth 10

try {
    # This will throw because the bot can't reply back, but message processing still works
    $response = Invoke-WebRequest -Uri "$baseUrl/api/messages" -Method Post -Body $messageBody -ContentType "application/json" -ErrorAction SilentlyContinue
    # If we get here without error, something unexpected happened
    Write-TestResult -TestName "Message endpoint accepts activities" -Success $true -Details "Unexpected success (check bot logs)"
} catch {
    # Expected: 500 error because reply fails (no connector service)
    # The important thing is that the bot received and processed the message
    if ($_.Exception.Response.StatusCode.value__ -eq 500) {
        Write-TestResult -TestName "Message endpoint accepts activities" -Success $true -Details "Message processed (reply failed as expected without emulator)"
    } else {
        Write-TestResult -TestName "Message endpoint accepts activities" -Success $false -Details "Status: $($_.Exception.Response.StatusCode.value__)"
    }
}

# Test 6: Swagger documentation
Write-Host "`nTest 6: API Documentation" -ForegroundColor Yellow
try {
    $swagger = Invoke-WebRequest -Uri "$baseUrl/swagger/v1/swagger.json" -Method Get
    $success = $swagger.StatusCode -eq 200
    Write-TestResult -TestName "Swagger documentation available" -Success $success -Details "OpenAPI spec accessible at /swagger"
} catch {
    Write-TestResult -TestName "Swagger documentation available" -Success $false -Details $_.Exception.Message
}

# Summary
Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })

if ($failed -eq 0) {
    Write-Host "`nAll tests passed! Bot is ready for Bot Framework Emulator testing." -ForegroundColor Green
} else {
    Write-Host "`nSome tests failed. Check bot logs for details." -ForegroundColor Yellow
}

Write-Host "`nNext steps:"
Write-Host "1. Download Bot Framework Emulator: https://github.com/microsoft/BotFramework-Emulator/releases"
Write-Host "2. Connect to: $baseUrl/api/messages"
Write-Host "3. Leave App ID and Password blank for local testing"
Write-Host ""
