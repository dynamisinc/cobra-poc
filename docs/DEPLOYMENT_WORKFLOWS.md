# Deployment Workflows Guide

This guide covers different deployment strategies for the Checklist POC, from manual PowerShell scripts to fully automated CI/CD.

## Table of Contents
1. [Deployment Options Overview](#deployment-options-overview)
2. [Option 1: Config-Based Deployment (Recommended for POC)](#option-1-config-based-deployment-recommended-for-poc)
3. [Option 2: GitHub Actions CI/CD (Recommended for Production)](#option-2-github-actions-cicd-recommended-for-production)
4. [Option 3: Azure DevOps Pipelines](#option-3-azure-devops-pipelines)
5. [Comparison Matrix](#comparison-matrix)
6. [Secrets Management](#secrets-management)

---

## Deployment Options Overview

| Option | Setup Time | Deployment Speed | Best For | Automation Level |
|--------|-----------|-----------------|----------|------------------|
| **Config-Based Script** | 5 min | ~3 min | POC, manual control | Semi-automated |
| **GitHub Actions** | 15 min | ~5 min | Regular updates, CI/CD | Fully automated |
| **Azure DevOps** | 30 min | ~5 min | Enterprise, complex workflows | Fully automated |

---

## Option 1: Config-Based Deployment (Recommended for POC)

**Best for:** SME feedback phase, infrequent deployments, manual control

### How It Works

1. **One-time setup:** Edit `azure-config.json` with your Azure details
2. **Deploy:** Run `.\deploy.ps1` (uses config file)
3. **Updates:** Run `.\deploy.ps1 -DeployOnly` (skips resource creation)

### Setup Steps

#### 1. Edit Configuration File

Edit [azure-config.json](../azure-config.json):

```json
{
  "resourceGroup": "rg-checklist-poc",
  "location": "eastus",
  "appName": "checklist-poc-jdoe",           // Change to unique name
  "sqlServer": "my-existing-sql-server",     // Your SQL Server
  "sqlDatabase": "ChecklistPOC",
  "sqlAdminUser": "sqladmin",                // Your SQL admin username
  "appServiceOS": "Linux",
  "enableApplicationInsights": false
}
```

#### 2. Set SQL Password (Choose One Method)

**Option A: Environment Variable (Recommended)**

```powershell
# Windows (PowerShell) - persists across sessions
[System.Environment]::SetEnvironmentVariable('AZURE_SQL_PASSWORD', 'YourPassword123!', 'User')

# Restart PowerShell, then password is auto-loaded
```

**Option B: Prompt Each Time**

Script will prompt you for password if environment variable not set.

#### 3. First Deployment (Setup + Deploy)

```powershell
# First time: Create resources AND deploy app
.\deploy.ps1
```

**This does:**
- ✅ Creates Azure resources (App Service, SQL Database)
- ✅ Builds frontend and backend
- ✅ Deploys to Azure
- ✅ Applies database migrations

**Duration:** ~6 minutes

#### 4. Subsequent Deployments (Deploy Only)

```powershell
# After code changes: Just redeploy
.\deploy.ps1 -DeployOnly
```

**This does:**
- ✅ Builds frontend and backend
- ✅ Deploys to Azure
- ✅ Applies migrations

**Duration:** ~3 minutes

### Advanced Usage

```powershell
# Setup only (create resources, don't deploy)
.\deploy.ps1 -SetupOnly

# Deploy without rebuilding (if you already built)
.\deploy.ps1 -DeployOnly -SkipBuild

# Deploy without migrations (if DB schema unchanged)
.\deploy.ps1 -DeployOnly -SkipMigrations

# Use different config file
.\deploy.ps1 -ConfigFile azure-config-staging.json
```

### Pros & Cons

✅ **Pros:**
- Simple setup (edit one config file)
- No secrets in Git (password via env var)
- Manual control over deployments
- Works from any machine with Azure CLI
- Can use different config files for dev/staging/prod

❌ **Cons:**
- Must run manually after each update
- Requires Azure CLI and PowerShell on your machine
- No automatic deployment on git push
- Team members need their own config

---

## Option 2: GitHub Actions CI/CD (Recommended for Production)

**Best for:** Automatic deployment on every push, team collaboration, production workloads

### How It Works

1. **One-time setup:** Configure GitHub secrets and download publish profile
2. **Deploy:** Push to `main` branch → automatic deployment
3. **Manual trigger:** Use GitHub UI to deploy on-demand

### Setup Steps

#### 1. Get Azure Publish Profile

```powershell
# Download publish profile
az webapp deployment list-publishing-profiles `
  --name YOUR_APP_NAME `
  --resource-group YOUR_RESOURCE_GROUP `
  --xml

# Save output to a file: publish-profile.xml
```

Or via Azure Portal:
1. Go to your App Service
2. Click **Get publish profile** button
3. Download `publish-profile.xml`

#### 2. Configure GitHub Secrets

Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions**

Add these secrets:

| Secret Name | Value | Example |
|------------|-------|---------|
| `AZURE_APP_NAME` | Your app name | `checklist-poc-jdoe` |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Contents of publish-profile.xml | `<publishData>...</publishData>` |
| `AZURE_SQL_CONNECTION_STRING` | SQL connection string | `Server=tcp:...` |

**Get connection string:**
```powershell
az webapp config connection-string list `
  --name YOUR_APP_NAME `
  --resource-group YOUR_RESOURCE_GROUP `
  --query "[?name=='DefaultConnection'].value" `
  --output tsv
```

#### 3. Push GitHub Actions Workflow

The workflow files are already in your repo at:
- [.github/workflows/azure-deploy.yml](../.github/workflows/azure-deploy.yml) - Auto-deploy on push to main
- [.github/workflows/azure-deploy-manual.yml](../.github/workflows/azure-deploy-manual.yml) - Manual trigger

```bash
git add .github/workflows/
git commit -m "Add GitHub Actions CI/CD"
git push origin main
```

#### 4. Verify Deployment

1. Go to GitHub → **Actions** tab
2. You should see workflow running
3. Wait ~5 minutes for deployment to complete

### Auto-Deploy on Push

```bash
# Make code changes
git add .
git commit -m "feat: add new feature"
git push origin main

# GitHub Actions automatically:
# 1. Builds frontend
# 2. Builds backend
# 3. Deploys to Azure
# 4. Applies migrations
```

### Manual Deploy from GitHub UI

1. Go to GitHub → **Actions** → **Manual Deploy to Azure**
2. Click **Run workflow**
3. Choose environment (dev/staging/prod)
4. Click **Run workflow** button

### Pros & Cons

✅ **Pros:**
- Fully automated deployment on push
- No local setup needed (runs in cloud)
- Works for entire team
- Deployment history visible in GitHub
- Can deploy to multiple environments
- Manual trigger option available

❌ **Cons:**
- Initial setup is more complex
- Requires GitHub secrets configuration
- GitHub Actions minutes usage (free tier: 2000 min/month)
- Less control over when deployments happen (on every push)

---

## Option 3: Azure DevOps Pipelines

**Best for:** Enterprise environments, complex multi-stage pipelines, Azure-native workflows

### Overview

Azure DevOps Pipelines offer more features than GitHub Actions:
- Multi-stage pipelines (build → test → deploy)
- Release gates and approvals
- Integration with Azure Boards and Test Plans
- Self-hosted agents

### Setup (High-Level)

1. Create Azure DevOps project
2. Connect to GitHub repository
3. Create build pipeline ([azure-pipelines.yml](../azure-pipelines.yml))
4. Create release pipeline with stages (Dev → Staging → Prod)
5. Configure approvals and gates

**Note:** For SME feedback POC, this is overkill. Use GitHub Actions instead.

---

## Comparison Matrix

| Feature | Config Script | GitHub Actions | Azure DevOps |
|---------|--------------|----------------|-------------|
| **Setup Complexity** | ⭐ Simple | ⭐⭐ Moderate | ⭐⭐⭐ Complex |
| **Deployment Speed** | 3 min | 5 min | 5-10 min |
| **Automatic on Push** | ❌ | ✅ | ✅ |
| **Manual Control** | ✅ | ⚠️ Limited | ✅ |
| **Multiple Environments** | ⚠️ Manual | ✅ | ✅ |
| **Team Collaboration** | ❌ | ✅ | ✅ |
| **Cost** | Free | Free* | Free* |
| **Requires Local Tools** | ✅ Yes | ❌ No | ❌ No |
| **Approval Gates** | ❌ | ❌ | ✅ |
| **Rollback** | Manual | Manual | ✅ Automated |

*Free tier: GitHub Actions (2000 min/month), Azure DevOps (1 parallel job, 1800 min/month)

---

## Secrets Management

### Where to Store Secrets

| Secret | Local Dev | GitHub Actions | Azure DevOps |
|--------|-----------|----------------|-------------|
| SQL Password | Environment variable | GitHub Secrets | Azure DevOps Library |
| Connection String | `appsettings.Development.json` | GitHub Secrets | Azure DevOps Library |
| App Insights Key | `appsettings.json` | App Service Config | App Service Config |

### Best Practices

#### 1. Never Commit Secrets to Git

**Add to `.gitignore`:**
```
appsettings.Development.json
appsettings.Production.json
azure-config.local.json
*.publishsettings
```

#### 2. Use Environment Variables Locally

```powershell
# PowerShell
$env:AZURE_SQL_PASSWORD = "YourPassword"

# Or persist across sessions
[System.Environment]::SetEnvironmentVariable('AZURE_SQL_PASSWORD', 'YourPassword', 'User')
```

#### 3. Use Azure Key Vault for Production

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### 4. Rotate Secrets Regularly

```powershell
# Rotate SQL password
az sql server update `
  --name YOUR_SQL_SERVER `
  --resource-group YOUR_RESOURCE_GROUP `
  --admin-password "NewPassword123!"

# Update in GitHub Secrets
# Update in environment variable
```

---

## Recommendation by Phase

### Phase 1: Initial SME Feedback (Current)
**Use:** Config-Based Script (`.\deploy.ps1`)

**Why:**
- ✅ Fast setup (5 minutes)
- ✅ Manual control (deploy when ready)
- ✅ Easy to troubleshoot
- ✅ No CI/CD overhead

### Phase 2: Active Development with Team
**Use:** GitHub Actions (auto-deploy on main)

**Why:**
- ✅ Automatic deployment on every merge
- ✅ Team can see deployment status
- ✅ No local setup for team members
- ✅ Deployment history tracked

### Phase 3: Production with Multiple Environments
**Use:** GitHub Actions with Environments OR Azure DevOps

**Why:**
- ✅ Deploy to dev → staging → prod
- ✅ Approval gates before production
- ✅ Rollback capabilities
- ✅ Environment-specific secrets

---

## Migration Path

### From Config Script → GitHub Actions

1. **Continue using config script** for local development
2. **Set up GitHub Actions** for automatic deployments
3. **Test GitHub Actions** with a few pushes
4. **Switch to GitHub Actions** for regular deployments
5. **Keep config script** for emergency manual deployments

Both can coexist!

---

## Quick Commands Reference

### Config-Based Deployment

```powershell
# First time (setup + deploy)
.\deploy.ps1

# Updates (deploy only)
.\deploy.ps1 -DeployOnly

# Fast deploy (no rebuild)
.\deploy.ps1 -DeployOnly -SkipBuild

# Deploy without migrations
.\deploy.ps1 -DeployOnly -SkipMigrations

# Different environment
.\deploy.ps1 -ConfigFile azure-config-prod.json
```

### GitHub Actions

```bash
# Auto-deploy: just push to main
git push origin main

# Manual deploy from CLI
gh workflow run "Manual Deploy to Azure" \
  --field environment=prod \
  --field skip_migrations=false

# View deployment status
gh run list --workflow="Deploy to Azure"
```

### Monitoring

```powershell
# Stream logs
az webapp log tail --name YOUR_APP_NAME --resource-group YOUR_RG

# View recent deployments
az webapp deployment list --name YOUR_APP_NAME --resource-group YOUR_RG

# Check app status
az webapp show --name YOUR_APP_NAME --resource-group YOUR_RG --query "state"
```

---

## Troubleshooting Deployments

### Config Script Fails

**Check Azure CLI login:**
```powershell
az account show
az login  # If not logged in
```

**Check config file:**
```powershell
# Validate JSON
Get-Content azure-config.json | ConvertFrom-Json
```

**Check SQL password:**
```powershell
# Verify environment variable
$env:AZURE_SQL_PASSWORD
```

### GitHub Actions Fails

**Check secrets:**
1. GitHub → Settings → Secrets → Actions
2. Verify all three secrets are set

**View logs:**
1. GitHub → Actions → Click failed workflow
2. Expand steps to see error details

**Common issues:**
- Publish profile expired → Re-download from Azure Portal
- Connection string wrong → Verify in Azure Portal
- .NET version mismatch → Check workflow file

---

## Cost Considerations

### Config Script
- **CI/CD Cost:** $0 (runs locally)
- **Azure Cost:** ~$15-20/month (App Service + SQL)

### GitHub Actions
- **CI/CD Cost:** $0 (within free tier: 2000 min/month)
- **Each deployment:** ~5 minutes = ~400 deployments/month free
- **Azure Cost:** ~$15-20/month (same as above)

### Azure DevOps
- **CI/CD Cost:** $0 (within free tier: 1 parallel job, 1800 min/month)
- **Azure Cost:** ~$15-20/month (same as above)

**Verdict:** All options are cost-effective for POC. GitHub Actions offers best value for automated deployments.

---

## Next Steps

### For POC (Recommended Now)
1. ✅ Edit `azure-config.json` with your details
2. ✅ Set `AZURE_SQL_PASSWORD` environment variable
3. ✅ Run `.\deploy.ps1` for first deployment
4. ✅ Use `.\deploy.ps1 -DeployOnly` for updates

### For Production (Future)
1. Set up GitHub secrets
2. Test GitHub Actions workflow
3. Configure multiple environments (dev, staging, prod)
4. Add approval gates for production
5. Set up monitoring and alerts

---

## Related Documentation

- [DEPLOYMENT_QUICK_START.md](../DEPLOYMENT_QUICK_START.md) - Quick start guide
- [DEPLOYMENT_EXISTING_SQL.md](../DEPLOYMENT_EXISTING_SQL.md) - Using existing SQL Server
- [docs/AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md) - Comprehensive deployment guide
- [azure-config.json](../azure-config.json) - Configuration file

---

**Questions?** Check the main deployment docs or open an issue!
