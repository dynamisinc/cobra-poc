# ============================================================================
# Checklist POC - Checklist Items API Integration Test Script
# ============================================================================
# Tests all 4 endpoints of the ChecklistItemsController
# Requires: Backend API running on https://localhost:5001
# ============================================================================

param(
    [string]$BaseUrl = "https://localhost:5001",
    [switch]$Verbose
)

# Colors for output
$script:PassColor = "Green"
$script:FailColor = "Red"
$script:InfoColor = "Cyan"
$script:WarnColor = "Yellow"

# Test results tracking
$script:TestsPassed = 0
$script:TestsFailed = 0
$script:TestResults = @()

# Test data storage
$script:ChecklistId = $null
$script:CheckboxItemId = $null
$script:StatusItemId = $null

# ============================================================================
# Helper Functions
# ============================================================================

function Write-TestHeader {
    param([string]$Message)
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor $InfoColor
    Write-Host $Message -ForegroundColor $InfoColor
    Write-Host "============================================================================" -ForegroundColor $InfoColor
}

function Write-TestStep {
    param([string]$Message)
    Write-Host ""
    Write-Host "[TEST] $Message" -ForegroundColor $InfoColor
}

function Write-TestPass {
    param([string]$Message)
    Write-Host "[PASS] $Message" -ForegroundColor $PassColor
    $script:TestsPassed++
    $script:TestResults += @{
        Status = "PASS"
        Message = $Message
        Timestamp = Get-Date
    }
}

function Write-TestFail {
    param([string]$Message, [string]$Details = "")
    Write-Host "[FAIL] $Message" -ForegroundColor $FailColor
    if ($Details) {
        Write-Host "       $Details" -ForegroundColor $FailColor
    }
    $script:TestsFailed++
    $script:TestResults += @{
        Status = "FAIL"
        Message = $Message
        Details = $Details
        Timestamp = Get-Date
    }
}

function Write-TestInfo {
    param([string]$Message)
    if ($Verbose) {
        Write-Host "       $Message" -ForegroundColor Gray
    }
}

function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [int]$ExpectedStatusCode = 200,
        [hashtable]$Headers = @{}
    )

    $uri = "$BaseUrl$Endpoint"
    Write-TestInfo "$Method $uri"

    try {
        $defaultHeaders = @{
            "X-User-Email" = "test-user@cobra.mil"
            "X-User-Position" = "Safety Officer"
        }

        # Merge custom headers with defaults
        foreach ($key in $Headers.Keys) {
            $defaultHeaders[$key] = $Headers[$key]
        }

        $params = @{
            Uri = $uri
            Method = $Method
            ContentType = "application/json"
            Headers = $defaultHeaders
            SkipCertificateCheck = $true
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            Write-TestInfo "Request Body: $($params.Body)"
        }

        $response = Invoke-RestMethod @params -StatusCodeVariable statusCode

        Write-TestInfo "Status Code: $statusCode"

        if ($statusCode -eq $ExpectedStatusCode) {
            return @{
                Success = $true
                StatusCode = $statusCode
                Data = $response
            }
        } else {
            return @{
                Success = $false
                StatusCode = $statusCode
                Error = "Expected $ExpectedStatusCode, got $statusCode"
            }
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-TestInfo "Exception Status Code: $statusCode"
        Write-TestInfo "Exception Message: $($_.Exception.Message)"

        if ($statusCode -eq $ExpectedStatusCode) {
            return @{
                Success = $true
                StatusCode = $statusCode
                Data = $null
            }
        }

        return @{
            Success = $false
            StatusCode = $statusCode
            Error = $_.Exception.Message
        }
    }
}

# ============================================================================
# Setup: Create Test Checklist
# ============================================================================

Write-TestHeader "SETUP: Creating Test Checklist"

Write-TestStep "Creating checklist from template for item testing"
$setupResult = Invoke-ApiRequest -Method "GET" -Endpoint "/api/checklists/my-checklists"

if ($setupResult.Success -and $setupResult.Data.Count -gt 0) {
    $script:ChecklistId = $setupResult.Data[0].id
    Write-TestPass "Using existing checklist: $ChecklistId"

    # Find checkbox and status items
    $checklist = Invoke-ApiRequest -Method "GET" -Endpoint "/api/checklists/$ChecklistId"

    if ($checklist.Success) {
        $checkboxItem = $checklist.Data.items | Where-Object { $_.itemType -eq "checkbox" } | Select-Object -First 1
        $statusItem = $checklist.Data.items | Where-Object { $_.itemType -eq "status" } | Select-Object -First 1

        if ($checkboxItem) {
            $script:CheckboxItemId = $checkboxItem.id
            Write-TestPass "Found checkbox item: $CheckboxItemId"
        } else {
            Write-TestFail "No checkbox items found in checklist"
            exit 1
        }

        if ($statusItem) {
            $script:StatusItemId = $statusItem.id
            Write-TestPass "Found status item: $StatusItemId"
        } else {
            Write-TestInfo "No status items found (optional)"
        }
    } else {
        Write-TestFail "Failed to retrieve checklist details"
        exit 1
    }
} else {
    Write-TestFail "No checklists available for testing. Please create a checklist first."
    exit 1
}

# ============================================================================
# Test 1: GET Item by ID
# ============================================================================

Write-TestHeader "Test 1: GET Item by ID"

Write-TestStep "1.1 - Get existing checkbox item"
$result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId"

if ($result.Success -and $result.Data) {
    Write-TestPass "Successfully retrieved item"
    Write-TestInfo "Item Text: $($result.Data.itemText)"
    Write-TestInfo "Item Type: $($result.Data.itemType)"
    Write-TestInfo "Is Completed: $($result.Data.isCompleted)"
} else {
    Write-TestFail "Failed to retrieve item" -Details $result.Error
}

Write-TestStep "1.2 - Get non-existent item (expect 404)"
$nonExistentId = [Guid]::NewGuid()
$result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/checklists/$ChecklistId/items/$nonExistentId" -ExpectedStatusCode 404

if ($result.Success -and $result.StatusCode -eq 404) {
    Write-TestPass "Correctly returned 404 for non-existent item"
} else {
    Write-TestFail "Should have returned 404 for non-existent item"
}

# ============================================================================
# Test 2: PATCH Item Completion
# ============================================================================

Write-TestHeader "Test 2: PATCH Item Completion"

Write-TestStep "2.1 - Mark checkbox item as complete"
$body = @{
    isCompleted = $true
    notes = "Verified all PPE is properly stored and accounted for"
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/completion" -Body $body

if ($result.Success -and $result.Data.isCompleted -eq $true) {
    Write-TestPass "Successfully marked item as complete"
    Write-TestInfo "Completed By: $($result.Data.completedBy)"
    Write-TestInfo "Completed At: $($result.Data.completedAt)"
    Write-TestInfo "Notes: $($result.Data.notes)"

    if ($result.Data.completedBy -and $result.Data.completedAt) {
        Write-TestPass "User attribution fields populated correctly"
    } else {
        Write-TestFail "User attribution fields missing"
    }
} else {
    Write-TestFail "Failed to mark item as complete" -Details $result.Error
}

Write-TestStep "2.2 - Mark checkbox item as incomplete"
$body = @{
    isCompleted = $false
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/completion" -Body $body

if ($result.Success -and $result.Data.isCompleted -eq $false) {
    Write-TestPass "Successfully marked item as incomplete"

    if (-not $result.Data.completedBy -and -not $result.Data.completedAt) {
        Write-TestPass "Completion fields correctly cleared"
    } else {
        Write-TestFail "Completion fields should be cleared when marked incomplete"
    }
} else {
    Write-TestFail "Failed to mark item as incomplete" -Details $result.Error
}

if ($StatusItemId) {
    Write-TestStep "2.3 - Attempt completion on status item (expect 400)"
    $body = @{
        isCompleted = $true
    }
    $result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$StatusItemId/completion" -Body $body -ExpectedStatusCode 400

    if ($result.Success -and $result.StatusCode -eq 400) {
        Write-TestPass "Correctly rejected completion update for status item"
    } else {
        Write-TestFail "Should reject completion update for non-checkbox items"
    }
}

Write-TestStep "2.4 - Update completion with notes"
$body = @{
    isCompleted = $true
    notes = "Updated with additional context at 15:45"
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/completion" -Body $body

if ($result.Success -and $result.Data.notes -eq "Updated with additional context at 15:45") {
    Write-TestPass "Successfully updated item with notes"
} else {
    Write-TestFail "Failed to update notes during completion" -Details $result.Error
}

# ============================================================================
# Test 3: PATCH Item Status
# ============================================================================

Write-TestHeader "Test 3: PATCH Item Status"

if ($StatusItemId) {
    Write-TestStep "3.1 - Update status item to 'In Progress'"
    $body = @{
        status = "In Progress"
        notes = "Started equipment check at 16:00"
    }
    $result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$StatusItemId/status" -Body $body

    if ($result.Success -and $result.Data.currentStatus -eq "In Progress") {
        Write-TestPass "Successfully updated status to 'In Progress'"
        Write-TestInfo "Current Status: $($result.Data.currentStatus)"
        Write-TestInfo "Notes: $($result.Data.notes)"

        if ($result.Data.isCompleted -eq $false) {
            Write-TestPass "Item correctly NOT marked as complete (status not 'Complete')"
        } else {
            Write-TestFail "Item should not be marked complete when status is 'In Progress'"
        }
    } else {
        Write-TestFail "Failed to update status" -Details $result.Error
    }

    Write-TestStep "3.2 - Update status item to 'Complete'"
    $body = @{
        status = "Complete"
    }
    $result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$StatusItemId/status" -Body $body

    if ($result.Success -and $result.Data.currentStatus -eq "Complete") {
        Write-TestPass "Successfully updated status to 'Complete'"

        if ($result.Data.isCompleted -eq $true) {
            Write-TestPass "Item correctly marked as complete when status is 'Complete'"
        } else {
            Write-TestFail "Item should be marked complete when status is 'Complete'"
        }
    } else {
        Write-TestFail "Failed to update status to 'Complete'" -Details $result.Error
    }

    Write-TestStep "3.3 - Attempt to set invalid status (expect 400)"
    $body = @{
        status = "Invalid Status Value"
    }
    $result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$StatusItemId/status" -Body $body -ExpectedStatusCode 400

    if ($result.Success -and $result.StatusCode -eq 400) {
        Write-TestPass "Correctly rejected invalid status value"
    } else {
        Write-TestFail "Should reject invalid status values"
    }

    Write-TestStep "3.4 - Attempt status update on checkbox item (expect 400)"
    $body = @{
        status = "Complete"
    }
    $result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/status" -Body $body -ExpectedStatusCode 400

    if ($result.Success -and $result.StatusCode -eq 400) {
        Write-TestPass "Correctly rejected status update for checkbox item"
    } else {
        Write-TestFail "Should reject status update for non-status items"
    }
} else {
    Write-TestInfo "Skipping status tests (no status items in checklist)"
}

# ============================================================================
# Test 4: PATCH Item Notes
# ============================================================================

Write-TestHeader "Test 4: PATCH Item Notes"

Write-TestStep "4.1 - Update notes on checkbox item"
$body = @{
    notes = "Additional context added by Safety Officer at 17:00"
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/notes" -Body $body

if ($result.Success -and $result.Data.notes -eq "Additional context added by Safety Officer at 17:00") {
    Write-TestPass "Successfully updated notes on checkbox item"
    Write-TestInfo "LastModifiedBy: $($result.Data.lastModifiedBy)"
} else {
    Write-TestFail "Failed to update notes" -Details $result.Error
}

if ($StatusItemId) {
    Write-TestStep "4.2 - Update notes on status item"
    $body = @{
        notes = "Equipment verified and logged"
    }
    $result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$StatusItemId/notes" -Body $body

    if ($result.Success -and $result.Data.notes -eq "Equipment verified and logged") {
        Write-TestPass "Successfully updated notes on status item"
    } else {
        Write-TestFail "Failed to update notes on status item" -Details $result.Error
    }
}

Write-TestStep "4.3 - Clear notes (set to null)"
$body = @{
    notes = $null
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/notes" -Body $body

if ($result.Success) {
    Write-TestPass "Successfully cleared notes"
    Write-TestInfo "Notes value: $($result.Data.notes)"
} else {
    Write-TestFail "Failed to clear notes" -Details $result.Error
}

Write-TestStep "4.4 - Update notes on non-existent item (expect 404)"
$nonExistentId = [Guid]::NewGuid()
$body = @{
    notes = "Test"
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$nonExistentId/notes" -Body $body -ExpectedStatusCode 404

if ($result.Success -and $result.StatusCode -eq 404) {
    Write-TestPass "Correctly returned 404 for non-existent item"
} else {
    Write-TestFail "Should have returned 404 for non-existent item"
}

# ============================================================================
# Test 5: Progress Recalculation Verification
# ============================================================================

Write-TestHeader "Test 5: Progress Recalculation"

Write-TestStep "5.1 - Verify progress updated after item completion"
# Get checklist progress before
$before = Invoke-ApiRequest -Method "GET" -Endpoint "/api/checklists/$ChecklistId"
$progressBefore = $before.Data.progressPercentage

# Complete an item
$body = @{
    isCompleted = $true
}
$result = Invoke-ApiRequest -Method "PATCH" -Endpoint "/api/checklists/$ChecklistId/items/$CheckboxItemId/completion" -Body $body

# Get checklist progress after
$after = Invoke-ApiRequest -Method "GET" -Endpoint "/api/checklists/$ChecklistId"
$progressAfter = $after.Data.progressPercentage

Write-TestInfo "Progress Before: $progressBefore%"
Write-TestInfo "Progress After: $progressAfter%"
Write-TestInfo "Completed Items: $($after.Data.completedItems) / $($after.Data.totalItems)"

if ($progressAfter -ge $progressBefore) {
    Write-TestPass "Progress correctly recalculated after completion"
} else {
    Write-TestFail "Progress should increase or stay same after marking item complete"
}

# ============================================================================
# Test Summary
# ============================================================================

Write-TestHeader "TEST SUMMARY"

$totalTests = $script:TestsPassed + $script:TestsFailed
$passRate = if ($totalTests -gt 0) { [math]::Round(($script:TestsPassed / $totalTests) * 100, 2) } else { 0 }

Write-Host ""
Write-Host "Total Tests:  $totalTests" -ForegroundColor White
Write-Host "Passed:       $script:TestsPassed" -ForegroundColor $PassColor
Write-Host "Failed:       $script:TestsFailed" -ForegroundColor $(if ($script:TestsFailed -gt 0) { $FailColor } else { $PassColor })
Write-Host "Pass Rate:    $passRate%" -ForegroundColor $(if ($passRate -eq 100) { $PassColor } else { $WarnColor })
Write-Host ""

if ($script:TestsFailed -eq 0) {
    Write-Host "✓ ALL TESTS PASSED!" -ForegroundColor $PassColor
    exit 0
} else {
    Write-Host "✗ SOME TESTS FAILED" -ForegroundColor $FailColor
    Write-Host ""
    Write-Host "Failed Tests:" -ForegroundColor $FailColor
    $script:TestResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  - $($_.Message)" -ForegroundColor $FailColor
        if ($_.Details) {
            Write-Host "    $($_.Details)" -ForegroundColor Gray
        }
    }
    exit 1
}
