# Azure Deployment Guide for Checklist POC

> **Last Updated:** 2025-11-28
> **Target Environment:** Azure App Service + Azure SQL Database

This guide documents the complete deployment process for the Checklist POC application, including common pitfalls and their solutions learned during initial deployment.

---

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Prerequisites](#prerequisites)
3. [Database Configuration](#database-configuration)
4. [Frontend Build Configuration](#frontend-build-configuration)
5. [Deployment Process](#deployment-process)
6. [Common Issues and Solutions](#common-issues-and-solutions)
7. [Verification Steps](#verification-steps)

---

## Architecture Overview

```
Azure App Service (checklist-poc-app)
├── ASP.NET Core Web API (.NET 10)
│   ├── /api/* endpoints
│   └── wwwroot/ (static frontend files)
│
└── Azure SQL Database (ChecklistPOC)
    └── SQL Authentication (not Managed Identity for POC)
```

**Key Points:**
- Single App Service hosts both API and frontend
- Frontend is built and copied to `wwwroot/` before deployment
- API routes are prefixed with `/api/`
- Frontend uses relative `/api` base URL in production
- SPA fallback routes non-API requests to index.html

---

## Prerequisites

1. **Azure CLI** installed and logged in (`az login`)
2. **Git** configured with Azure deployment credentials
3. **.NET 10 SDK** installed locally
4. **Node.js 20+** installed locally
5. **SQL Server tools** (sqlcmd or Azure Data Studio) for database setup

---

## Database Configuration

### Password Requirements

**CRITICAL:** Use SQL Authentication with a password that does **NOT** contain special characters:

| Avoid These | Why |
|-------------|-----|
| `#` | Breaks URL parsing |
| `@` | Conflicts with connection string format |
| `%` | URL encoding issues |
| `&` | Query string delimiter |
| `=`, `+`, `/`, `?`, `;` | Connection string parsing issues |

**Good Password Example:** `YourSimplePassword2025x`
**Bad Password Example:** `P@ss#word!2025`

### Create Application User

```sql
-- Run on master database
CREATE LOGIN checklistapp WITH PASSWORD = 'YourSimplePassword2025x';

-- Run on ChecklistPOC database
USE ChecklistPOC;
CREATE USER checklistapp FOR LOGIN checklistapp;
ALTER ROLE db_datareader ADD MEMBER checklistapp;
ALTER ROLE db_datawriter ADD MEMBER checklistapp;
ALTER ROLE db_ddladmin ADD MEMBER checklistapp;
```

---

## Frontend Build Configuration

### Critical: API URL Configuration

The frontend uses Axios with a `baseURL` configuration. The production environment file sets this:

**File:** `src/frontend/.env.production`
```env
VITE_API_URL=/api
VITE_HUB_URL=/hubs/checklist
VITE_ENABLE_MOCK_AUTH=true
```

### Service Files: No `/api` Prefix

**CRITICAL:** All service files must use paths **WITHOUT** the `/api` prefix because `baseURL` already includes it.

```typescript
// CORRECT - baseURL adds /api automatically
const response = await apiClient.get('/templates');
const response = await apiClient.get('/checklists/my-checklists');
const response = await apiClient.patch(`/checklists/${id}/items/${itemId}/completion`);

// WRONG - results in /api/api/templates (404 error)
const response = await apiClient.get('/api/templates');
```

**Service files to verify:**
- `src/frontend/src/services/templateService.ts`
- `src/frontend/src/services/checklistService.ts`
- `src/frontend/src/services/itemService.ts`
- `src/frontend/src/services/analyticsService.ts`
- `src/frontend/src/services/itemLibraryService.ts`

### StatusConfiguration Parsing

The API returns `statusConfiguration` as a JSON string in two possible formats:

1. **Simple string array** (from seed data):
   ```json
   ["Not Started", "In Progress", "Completed"]
   ```

2. **Full StatusOption objects** (from UI-created items):
   ```json
   [{"label": "Not Started", "isCompletion": false, "order": 0}, ...]
   ```

Frontend components must handle both formats using the `parseStatusConfiguration` helper:

```typescript
const parseStatusConfiguration = (statusConfiguration?: string | null): StatusOption[] => {
  if (!statusConfiguration) return [];
  try {
    const parsed = JSON.parse(statusConfiguration);
    if (Array.isArray(parsed)) {
      if (parsed.length === 0) return [];
      if (typeof parsed[0] === 'string') {
        // Convert string array to StatusOption array
        return parsed.map((label: string, index: number) => ({
          label,
          isCompletion: label.toLowerCase().includes('complete') ||
                        label.toLowerCase().includes('done'),
          order: index,
        }));
      } else {
        return (parsed as StatusOption[]).sort((a, b) => a.order - b.order);
      }
    }
    return [];
  } catch (error) {
    console.error('Failed to parse status configuration:', error);
    return [];
  }
};
```

**Files requiring this helper:**
- `src/frontend/src/pages/TemplatePreviewPage.tsx`
- `src/frontend/src/pages/TemplateEditorPage.tsx`
- `src/frontend/src/pages/ChecklistDetailPage.tsx`
- `src/frontend/src/components/ItemStatusDialog.tsx`

---

## Deployment Process

### Quick Deploy (Recommended)

Use the deployment script for a one-command deployment:

```powershell
cd deploy/scripts
.\quick-deploy.ps1
```

Options:
- `.\quick-deploy.ps1 -Message "fix: bug description"` - Custom commit message
- `.\quick-deploy.ps1 -SkipBuild` - Skip frontend build (use existing dist)
- `.\quick-deploy.ps1 -SkipTests` - Skip running tests

### Manual Steps

If you prefer to deploy manually, follow these steps:

### Step 1: Build Frontend

```bash
cd src/frontend
npm install
npm run build
```

### Step 2: Copy Build to wwwroot

```bash
# Remove old static files
rm -rf src/backend/ChecklistAPI/wwwroot/*

# Copy new build
cp -r src/frontend/dist/* src/backend/ChecklistAPI/wwwroot/
```

### Step 3: Commit and Deploy

**IMPORTANT:** Azure App Service deploys from the `master` branch, not `main`. Always push to `master`:

```bash
git add -A
git commit -m "feat: deploy application to Azure"
git push azure HEAD:master
```

The deployment typically takes 1-2 minutes. Watch for "Deployment successful" in the output.

> **Note:** If you accidentally push to `main`, the files will be in the Azure repository but won't deploy. The deployment only triggers from `master`.

### Step 4: Restart if Needed

If you see 500 errors immediately after deployment:

```bash
az webapp restart --name checklist-poc-app --resource-group c5-poc-eastus2-rg

# Wait 30 seconds for app to restart
sleep 30
```

---

## Common Issues and Solutions

### Issue 1: "Login failed for user" (SQL Error 18456)

**Symptoms:**
- API returns 500 errors
- Azure logs show "Login failed for user 'checklistapp'"

**Causes:**
1. Password contains special characters (`#`, `@`, etc.) breaking connection string
2. User doesn't exist in the database
3. Password mismatch between Azure config and database

**Solution:**
```sql
-- Recreate user with simple password
DROP USER IF EXISTS checklistapp;
DROP LOGIN checklistapp;

CREATE LOGIN checklistapp WITH PASSWORD = 'SimplePassword2025x';
USE ChecklistPOC;
CREATE USER checklistapp FOR LOGIN checklistapp;
ALTER ROLE db_datareader ADD MEMBER checklistapp;
ALTER ROLE db_datawriter ADD MEMBER checklistapp;
ALTER ROLE db_ddladmin ADD MEMBER checklistapp;
```

Then restart the app:
```bash
az webapp restart --name checklist-poc-app --resource-group c5-poc-eastus2-rg
```

---

### Issue 2: Double `/api/api/` in URLs (404 or HTML responses)

**Symptoms:**
- Browser network tab shows requests to `/api/api/templates`
- API returns 404 or HTML (the SPA fallback page)
- Console shows "Unexpected token '<'" JSON parse errors

**Cause:** Service files include `/api` prefix, but `baseURL` already has `/api`.

**Solution:** Remove `/api` prefix from all service file endpoints:

```typescript
// BEFORE (wrong)
await apiClient.get('/api/templates');

// AFTER (correct)
await apiClient.get('/templates');
```

---

### Issue 3: "Something went wrong" on Template Preview/Edit

**Symptoms:**
- Template list works correctly
- Clicking on a template shows generic error
- Console may show "Cannot read property 'map' of undefined"

**Cause:** `statusConfiguration` parsing expects `StatusOption[]` objects but receives string array from API seed data.

**Solution:** Use the `parseStatusConfiguration` helper that handles both formats (see [StatusConfiguration Parsing](#statusconfiguration-parsing) section).

---

### Issue 4: 405 Method Not Allowed on PATCH/PUT

**Symptoms:**
- Item completion fails with 405 status
- URL shows double `/api/api/`

**Cause:** Same as Issue 2 - duplicate `/api` prefix in `itemService.ts`.

**Solution:** Fix URLs in `itemService.ts`:
```typescript
// BEFORE
await apiClient.patch(`/api/checklists/${checklistId}/items/${itemId}/completion`);

// AFTER
await apiClient.patch(`/checklists/${checklistId}/items/${itemId}/completion`);
```

---

### Issue 5: 500 Errors Immediately After Deployment

**Symptoms:**
- All API calls return 500
- App was working before deployment

**Cause:** App Service may need restart to pick up new configuration.

**Solution:**
```bash
az webapp restart --name checklist-poc-app --resource-group c5-poc-eastus2-rg
```

---

### Issue 6: Deployment Shows Success but Changes Not Applied

**Symptoms:**
- `git push azure HEAD:main` shows "Deployment successful"
- Browser still shows old JavaScript bundle
- Changes are not visible in the deployed app

**Cause:** Azure App Service deploys from `master` branch, not `main`. The Azure repository has two branches and only `master` triggers deployment.

**Diagnosis:**
```bash
# Check what branch Azure is using
curl -s -u '$checklist-poc-app:<password>' \
  "https://checklist-poc-app.scm.azurewebsites.net/api/command" \
  -H "Content-Type: application/json" \
  -d '{"command":"git branch -v","dir":"site/repository"}'

# Output will show which branch is checked out (marked with *)
```

**Solution:**
```bash
# Push to master instead of main
git push azure HEAD:master
```

**Prevention:** Always use `git push azure HEAD:master` for deployments.

---

## Verification Steps

After deployment, verify these endpoints work:

```bash
# 1. Templates list (should return JSON array)
curl -s "https://checklist-poc-app.azurewebsites.net/api/templates" | head -100

# 2. Single template (should return JSON object)
curl -s "https://checklist-poc-app.azurewebsites.net/api/templates/{template-id}"

# 3. Checklists (should return JSON array)
curl -s "https://checklist-poc-app.azurewebsites.net/api/checklists/my-checklists?includeArchived=false"

# 4. Analytics (should return JSON object)
curl -s "https://checklist-poc-app.azurewebsites.net/api/analytics/dashboard"

# 5. Frontend (should return HTML)
curl -s "https://checklist-poc-app.azurewebsites.net/" | head -20
```

---

## Quick Reference: Service URL Patterns

| Service | Correct URL | Incorrect URL |
|---------|-------------|---------------|
| Templates | `/templates` | `/api/templates` |
| Single Template | `/templates/{id}` | `/api/templates/{id}` |
| Checklists | `/checklists/my-checklists` | `/api/checklists/my-checklists` |
| Create Checklist | `/checklists` | `/api/checklists` |
| Item Completion | `/checklists/{id}/items/{id}/completion` | `/api/checklists/{id}/items/{id}/completion` |
| Analytics | `/analytics/dashboard` | `/api/analytics/dashboard` |
| Item Library | `/item-library` | `/api/item-library` |

**Remember:** `baseURL` in `api.ts` adds `/api`, so service files should NOT include it.

---

## Deployment Checklist

### Before Deploying

- [ ] All service files use URLs **without** `/api` prefix
- [ ] `parseStatusConfiguration` handles both string arrays and objects
- [ ] Frontend is built (`npm run build` in `src/frontend`)
- [ ] Build output copied to `src/backend/ChecklistAPI/wwwroot/`
- [ ] Database user exists with correct password (no special characters)
- [ ] Connection string configured in Azure App Service

### After Deploying

- [ ] `/api/templates` returns JSON (not HTML or 500)
- [ ] `/api/checklists/my-checklists` returns JSON
- [ ] Frontend loads at root URL
- [ ] Template list displays correctly
- [ ] Template preview works (click on template)
- [ ] Template edit works (edit button)
- [ ] Checklist creation works
- [ ] Item completion works (checkbox/status toggle)

---

## Current Azure Resources

| Resource | Name | Resource Group |
|----------|------|----------------|
| App Service | `checklist-poc-app` | `c5-poc-eastus2-rg` |
| SQL Server | `checklist-poc-sql` | `c5-poc-eastus2-rg` |
| Database | `ChecklistPOC` | - |
| App URL | https://checklist-poc-app.azurewebsites.net | - |
