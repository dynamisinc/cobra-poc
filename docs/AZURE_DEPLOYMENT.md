# Azure Deployment Guide - Low-Cost SME Feedback Environment

> **Target:** Emergency Management SMEs providing feedback
> **Priority:** Minimal cost over performance
> **Estimated Monthly Cost:** ~$55-65 USD

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Cost Breakdown](#cost-breakdown)
3. [Prerequisites](#prerequisites)
4. [Deployment Steps](#deployment-steps)
5. [Configuration](#configuration)
6. [Monitoring & Cost Management](#monitoring--cost-management)
7. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

### Deployment Model: Single App Service + Azure SQL Free Tier

```
┌─────────────────────────────────────────────────────┐
│  Azure App Service (B1 Basic - Linux)               │
│  ┌───────────────────┐  ┌──────────────────────┐  │
│  │ React Frontend    │  │ ASP.NET Core API     │  │
│  │ (Static Files)    │  │ (.NET 10)            │  │
│  └───────────────────┘  └──────────────────────┘  │
│                                                     │
│  Cost: ~$13/month (Linux) or ~$55/month (Windows)  │
└─────────────────────────────────────────────────────┘
                          │
                          │ SQL Connection
                          ▼
┌─────────────────────────────────────────────────────┐
│  Azure SQL Database (Serverless Free Tier)         │
│  - 100,000 vCore seconds/month (FREE)              │
│  - 32 GB storage (FREE)                             │
│  - Auto-pause after 1 hour                          │
│                                                     │
│  Cost: $0 (within free limits) to ~$5/month        │
└─────────────────────────────────────────────────────┘
```

### Why This Architecture?

✅ **Single App Service** - Host both React frontend and .NET backend together
✅ **Linux App Service** - 60% cheaper than Windows ($13 vs $55/month)
✅ **Azure SQL Serverless Free Tier** - 100,000 vCore seconds FREE forever
✅ **Auto-pause** - Database stops when not in use (reduces costs)
✅ **No Container Registry Needed** - Direct deployment from code

---

## Cost Breakdown

### Estimated Monthly Costs (US East Region)

| Service | Tier | Monthly Cost | Notes |
|---------|------|--------------|-------|
| **App Service Plan** | B1 Basic (Linux) | **$13.14** | Runs 24/7, hosts frontend + API |
| **Azure SQL Database** | Serverless (Free Tier) | **$0-5** | FREE up to 100K vCore seconds |
| **Application Insights** | Basic | **$0-2** | First 5GB/month FREE |
| **Bandwidth** | Outbound | **$0-1** | First 100GB FREE |
| **TOTAL** | | **~$15-20/month** | Assumes low SME usage |

### If Using Windows App Service
| Service | Tier | Monthly Cost |
|---------|------|--------------|
| App Service Plan | B1 Basic (Windows) | **$54.75** |
| Azure SQL Database | Serverless (Free Tier) | **$0-5** |
| **TOTAL** | | **~$55-65/month** |

**Recommendation:** Use **Linux** to save $40/month. .NET 10 runs perfectly on Linux.

### Azure SQL Free Tier Details
- **100,000 vCore seconds/month** = ~28 hours of active compute (FREE)
- **Auto-pause after 60 minutes** of inactivity (configurable)
- **First cold start:** 30-60 seconds (acceptable for SME feedback)
- **Overage cost:** ~$0.55/vCore hour (only if exceeds free tier)

**For SME Feedback:** Free tier is more than sufficient (infrequent access, low concurrency).

---

## Prerequisites

### Required Tools
- **Azure CLI** ([Install](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli))
- **.NET 10 SDK** (already installed)
- **Node.js 20+** (already installed)
- **Azure Subscription** (free trial works)

### Azure Subscription Setup
```bash
# Login to Azure
az login

# Set your subscription (if you have multiple)
az account list --output table
az account set --subscription "Your Subscription Name"

# Verify
az account show
```

---

## Deployment Steps

### Step 1: Prepare Application for Deployment

#### 1.1 Configure Frontend for Production

Create production environment file:

```bash
# src/frontend/.env.production
VITE_API_URL=/api
VITE_HUB_URL=/hubs/checklist
VITE_ENABLE_MOCK_AUTH=true
```

**Why `/api` instead of full URL?** Since frontend and backend are hosted together, we use relative paths.

#### 1.2 Build Frontend

```bash
cd src/frontend
npm install
npm run build

# Output will be in: src/frontend/dist/
```

#### 1.3 Configure Backend to Serve Frontend

Update `Program.cs` to serve React app:

```csharp
// Add before app.Run() in Program.cs

// Serve React frontend (production)
app.UseStaticFiles(); // Enable static file serving
app.UseDefaultFiles(); // Serve index.html by default

// Fallback to index.html for client-side routing
app.MapFallbackToFile("index.html");
```

#### 1.4 Copy Frontend Build to Backend

```bash
# Windows PowerShell
cd c:\Code\checklist-poc
xcopy /E /I /Y src\frontend\dist src\backend\ChecklistAPI\wwwroot

# Linux/Mac (when needed)
# cp -r src/frontend/dist/* src/backend/ChecklistAPI/wwwroot/
```

### Step 2: Create Azure Resources

#### 2.1 Set Variables

```bash
# Configuration variables
RESOURCE_GROUP="rg-checklist-poc"
LOCATION="eastus"
APP_NAME="checklist-poc-app"          # Must be globally unique
SQL_SERVER="checklist-poc-sql"        # Must be globally unique
SQL_DB="ChecklistPOC"
SQL_ADMIN="checklistadmin"
SQL_PASSWORD="YourStrongPassword123!"  # Change this!

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION
```

#### 2.2 Create Azure SQL Server and Database (Free Tier)

```bash
# Create SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

# Allow Azure services to access SQL Server
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create FREE serverless database (General Purpose)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 \
  --capacity 1 \
  --auto-pause-delay 60 \
  --min-capacity 0.5 \
  --backup-storage-redundancy Local

# Get connection string
az sql db show-connection-string \
  --client ado.net \
  --name $SQL_DB \
  --server $SQL_SERVER
```

**Connection String Format:**
```
Server=tcp:checklist-poc-sql.database.windows.net,1433;Initial Catalog=ChecklistPOC;Persist Security Info=False;User ID=checklistadmin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

#### 2.3 Create App Service Plan (Linux B1 - Low Cost)

```bash
# Create Linux App Service Plan (B1 Basic)
az appservice plan create \
  --name "${APP_NAME}-plan" \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --is-linux \
  --sku B1

# Alternative: Windows (costs more)
# az appservice plan create \
#   --name "${APP_NAME}-plan" \
#   --resource-group $RESOURCE_GROUP \
#   --location $LOCATION \
#   --sku B1
```

#### 2.4 Create Web App

```bash
# Create Web App (Linux with .NET 10)
az webapp create \
  --resource-group $RESOURCE_GROUP \
  --plan "${APP_NAME}-plan" \
  --name $APP_NAME \
  --runtime "DOTNET|10.0"

# Enable HTTPS only
az webapp update \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --https-only true
```

#### 2.5 Configure Application Settings

```bash
# Set connection string
az webapp config connection-string set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DB};User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};Encrypt=True;TrustServerCertificate=False;"

# Set app settings
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    WEBSITE_RUN_FROM_PACKAGE=0
```

### Step 3: Deploy Application

#### 3.1 Prepare Deployment Package

```bash
cd src/backend/ChecklistAPI

# Publish .NET app
dotnet publish -c Release -o ./publish

# Ensure wwwroot (React build) is included in publish output
# (Should already be included by default)
```

#### 3.2 Deploy to Azure

**Option A: Deploy via ZIP (Recommended for POC)**

```bash
cd src/backend/ChecklistAPI/publish

# Create ZIP file
# Windows (PowerShell):
Compress-Archive -Path * -DestinationPath ../deploy.zip -Force

# Linux/Mac:
# zip -r ../deploy.zip .

# Deploy ZIP to Azure
cd ..
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --src deploy.zip
```

**Option B: Deploy via GitHub Actions (For Continuous Deployment)**

See Section 6 for GitHub Actions setup.

#### 3.3 Apply Database Migrations

```bash
# Get your Azure SQL connection string
AZURE_SQL_CONN="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DB};User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};Encrypt=True;TrustServerCertificate=False;"

# Apply migrations (from local machine)
cd src/backend/ChecklistAPI
dotnet ef database update --connection "$AZURE_SQL_CONN"
```

**Alternative: Apply migrations via Kudu console**
1. Navigate to: `https://{APP_NAME}.scm.azurewebsites.net/DebugConsole`
2. Go to: `site/wwwroot`
3. Run: `dotnet ChecklistAPI.dll` (migrations run on startup if configured)

### Step 4: Verify Deployment

```bash
# Get app URL
az webapp show \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --query defaultHostName \
  --output tsv

# Your app is live at: https://{APP_NAME}.azurewebsites.net
```

**Test Endpoints:**
- Frontend: `https://{APP_NAME}.azurewebsites.net`
- API Health: `https://{APP_NAME}.azurewebsites.net/api/health` (if you add one)
- Swagger: `https://{APP_NAME}.azurewebsites.net/swagger`

---

## Configuration

### Environment Variables in Azure

Set via Azure Portal or CLI:

```bash
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    Logging__LogLevel__Default=Information \
    Logging__LogLevel__Microsoft.AspNetCore=Warning \
    APPLICATIONINSIGHTS_CONNECTION_STRING="your-app-insights-connection-string"
```

### Application Insights (Optional but Recommended)

```bash
# Create Application Insights
az monitor app-insights component create \
  --app "${APP_NAME}-insights" \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web

# Get connection string
INSIGHTS_CONN=$(az monitor app-insights component show \
  --app "${APP_NAME}-insights" \
  --resource-group $RESOURCE_GROUP \
  --query connectionString \
  --output tsv)

# Configure app to use Application Insights
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="$INSIGHTS_CONN"
```

**Update `Program.cs`:**
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Custom Domain (Optional)

```bash
# Map custom domain
az webapp config hostname add \
  --webapp-name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --hostname "checklist.yourdomain.com"

# Enable managed SSL certificate (FREE)
az webapp config ssl create \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --hostname "checklist.yourdomain.com"
```

---

## Monitoring & Cost Management

### View Real-Time Costs

```bash
# View current month costs for resource group
az consumption usage list \
  --start-date "2025-01-01" \
  --end-date "2025-01-31" \
  --query "[?resourceGroup=='$RESOURCE_GROUP'].{Service:instanceName,Cost:pretaxCost}" \
  --output table
```

**Azure Portal:**
- Navigate to: **Cost Management + Billing** → **Cost Analysis**
- Filter by Resource Group: `rg-checklist-poc`

### Set Budget Alerts

```bash
# Create budget (e.g., $30/month)
az consumption budget create \
  --budget-name "checklist-poc-budget" \
  --amount 30 \
  --category Cost \
  --time-grain Monthly \
  --start-date "2025-01-01" \
  --end-date "2025-12-31" \
  --resource-group $RESOURCE_GROUP
```

**Email Alert Setup (Azure Portal):**
1. Go to **Cost Management + Billing** → **Budgets**
2. Edit budget → **Alert conditions**
3. Add email notification at 80% and 100% thresholds

### Monitor Database Usage (Free Tier Limits)

```bash
# Check database size
az sql db show \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --query "maxSizeBytes" \
  --output tsv

# Monitor vCore usage (via Application Insights or SQL DMVs)
# Query in Azure SQL:
SELECT
  avg_cpu_percent,
  avg_memory_percent,
  avg_data_io_percent
FROM sys.dm_db_resource_stats
ORDER BY end_time DESC;
```

**Important:** Azure SQL Serverless free tier gives 100,000 vCore seconds/month:
- **~28 hours** of 1 vCore compute (FREE)
- Auto-pauses after 60 minutes of inactivity
- First access after pause: ~30-60 second cold start

### Stop Services When Not Needed

```bash
# Stop App Service (stops billing for compute)
az webapp stop --name $APP_NAME --resource-group $RESOURCE_GROUP

# Start App Service
az webapp start --name $APP_NAME --resource-group $RESOURCE_GROUP

# Note: SQL Database auto-pauses, no manual stop needed
```

---

## Troubleshooting

### Issue: "Cannot connect to database"

**Check Firewall Rules:**
```bash
# Add your IP to SQL Server firewall
MY_IP=$(curl -s ifconfig.me)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name MyClientIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

**Check Connection String:**
- Verify in Azure Portal: **App Service** → **Configuration** → **Connection Strings**
- Ensure `Encrypt=True` and `TrustServerCertificate=False`

### Issue: "Frontend shows 404 errors"

**Verify wwwroot files:**
```bash
# Check via Kudu console
# Navigate to: https://{APP_NAME}.scm.azurewebsites.net/DebugConsole
# Check: site/wwwroot/ contains index.html and static assets
```

**Check Program.cs:**
```csharp
// Ensure these are present:
app.UseStaticFiles();
app.UseDefaultFiles();
app.MapFallbackToFile("index.html"); // AFTER all other endpoints
```

### Issue: "Database cold start is slow"

**Expected Behavior:** First access after auto-pause takes 30-60 seconds.

**Solution Options:**
1. **Accept it** - For SME feedback, this is fine
2. **Increase auto-pause delay:**
   ```bash
   az sql db update \
     --resource-group $RESOURCE_GROUP \
     --server $SQL_SERVER \
     --name $SQL_DB \
     --auto-pause-delay 120  # 2 hours instead of 1
   ```
3. **Disable auto-pause** (costs more):
   ```bash
   az sql db update \
     --resource-group $RESOURCE_GROUP \
     --server $SQL_SERVER \
     --name $SQL_DB \
     --auto-pause-delay -1  # Never pause (billable)
   ```

### Issue: "App crashes or throws 500 errors"

**View Logs:**
```bash
# Enable logging
az webapp log config \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --application-logging filesystem \
  --level information

# Stream logs
az webapp log tail \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP
```

**Check logs in Azure Portal:**
- **App Service** → **Log stream**
- **App Service** → **Diagnose and solve problems**

### Issue: "Exceeding free tier limits"

**Check SQL Database Usage:**
```bash
# View database metrics
az monitor metrics list \
  --resource "/subscriptions/{subscription-id}/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Sql/servers/$SQL_SERVER/databases/$SQL_DB" \
  --metric cpu_percent \
  --start-time "2025-01-01T00:00:00Z" \
  --end-time "2025-01-31T23:59:59Z"
```

**Reduce Costs:**
1. Lower min/max vCore capacity
2. Increase auto-pause delay
3. Optimize database queries (add indexes, reduce N+1 queries)

---

## Deployment Checklist

### Pre-Deployment
- [ ] Frontend built: `npm run build` (in `src/frontend`)
- [ ] Frontend copied to `src/backend/ChecklistAPI/wwwroot`
- [ ] `Program.cs` configured to serve static files
- [ ] Production `.env.production` configured (relative paths)
- [ ] Azure CLI installed and logged in
- [ ] Resource names chosen (globally unique)

### Azure Setup
- [ ] Resource group created
- [ ] SQL Server created
- [ ] SQL Database created (Serverless, Free Tier)
- [ ] SQL firewall rules configured (Azure services + your IP)
- [ ] App Service Plan created (B1 Linux)
- [ ] Web App created
- [ ] Connection string configured in App Service
- [ ] Application settings configured

### Deployment
- [ ] App published: `dotnet publish -c Release`
- [ ] ZIP created and deployed to Azure
- [ ] Database migrations applied (`dotnet ef database update`)
- [ ] App accessible at `https://{APP_NAME}.azurewebsites.net`
- [ ] Swagger accessible (if enabled)
- [ ] Frontend loads correctly
- [ ] API endpoints respond correctly

### Post-Deployment
- [ ] Application Insights configured (optional)
- [ ] Budget alerts set up
- [ ] Monitoring dashboard created
- [ ] SME test accounts created (mock auth)
- [ ] Feedback collection process established

---

## Cost Optimization Tips

1. **Use Linux App Service** - Saves $40/month vs Windows
2. **Leverage SQL Serverless Free Tier** - 100K vCore seconds/month FREE
3. **Enable auto-pause** - Database pauses when idle (saves compute)
4. **Use Free Application Insights** - First 5GB/month FREE
5. **Stop App Service when not demoing** - No charges while stopped
6. **Monitor usage weekly** - Check Cost Management dashboard
7. **Delete resources after feedback phase** - Clean up to avoid ongoing charges

---

## Alternative: Azure Container Instances (Ultra Low Cost)

If you want even lower costs (~$5-10/month), consider Azure Container Instances:

```bash
# Deploy frontend + backend as containers
# SQL Database still uses free tier
# App runs only when container is active
```

See `docs/CONTAINER_DEPLOYMENT.md` (to be created) for details.

---

## Quick Reference Commands

```bash
# View app logs
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP

# Restart app
az webapp restart --name $APP_NAME --resource-group $RESOURCE_GROUP

# Stop app (stop billing)
az webapp stop --name $APP_NAME --resource-group $RESOURCE_GROUP

# Start app
az webapp start --name $APP_NAME --resource-group $RESOURCE_GROUP

# View current costs
az consumption usage list --output table

# Delete all resources (when done)
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

---

## Next Steps

1. **Deploy using steps above**
2. **Test with SMEs** - Gather feedback
3. **Monitor costs** - Ensure staying within budget
4. **Iterate** - Make changes based on feedback
5. **Clean up** - Delete resources when feedback phase is complete

---

## Sources & References

- [Azure SQL Free Tier Documentation](https://learn.microsoft.com/en-us/azure/azure-sql/database/free-offer)
- [Azure App Service Pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/windows/)
- [Deploy ASP.NET Core to Azure](https://learn.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore)
- [Azure SQL Serverless](https://learn.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-overview)

---

**Questions or Issues?** Check:
- Azure Portal: **Support + Troubleshooting** → **New Support Request**
- Azure CLI Docs: https://learn.microsoft.com/en-us/cli/azure/
- This project's `README.md` and `CLAUDE.md`
