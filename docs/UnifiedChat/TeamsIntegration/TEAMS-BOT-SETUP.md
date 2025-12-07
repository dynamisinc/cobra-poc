# Teams Bot Setup Guide

> **Last Updated:** 2025-12-07
> **Status:** Production Ready
> **SDK:** Microsoft 365 Agents SDK (successor to Bot Framework SDK)

This guide covers the complete setup of the COBRA Teams Bot, including Azure configuration, local development, and multi-tenant deployment.

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Azure Resource Setup](#azure-resource-setup)
3. [App Registration Configuration](#app-registration-configuration)
4. [Local Development Setup](#local-development-setup)
5. [Multi-Tenant Configuration](#multi-tenant-configuration)
6. [Testing the Bot](#testing-the-bot)
7. [Deploying to Production](#deploying-to-production)

---

## Architecture Overview

The COBRA Teams Bot uses the **Microsoft 365 Agents SDK** (successor to Bot Framework SDK) and consists of:

| Component | Purpose |
|-----------|---------|
| **Azure Bot Resource** | Routes messages between Teams and your bot endpoint |
| **App Registration** | Provides OAuth credentials for authentication |
| **TeamsBot Service** | Your .NET application that handles messages |
| **CobraAPI** | Main API that stores messages and conversation references |

### Message Flow

```
Teams User -> Azure Bot Service -> Your Bot Endpoint -> CobraAPI
                    ^                                    |
                    <- <- <- <- (proactive messages) <- <-
```

### Key Insight: Single-Tenant Bot + Multi-Tenant App Registration

**IMPORTANT:** As of 2025, Microsoft is deprecating Multi-Tenant Bot Type (after July 31, 2025). The recommended approach is:

- **Azure Bot Resource**: Single-Tenant
- **App Registration**: Multi-Tenant (Accounts in any organizational directory)

This combination allows your bot to work across multiple organizations while using the supported Single-Tenant bot type.

---

## Azure Resource Setup

### Step 1: Create App Registration

1. Go to **Azure Portal** -> **Microsoft Entra ID** -> **App registrations**
2. Click **+ New registration**
3. Configure:
   - **Name**: `cobra-teams-bot`
   - **Supported account types**: **"Accounts in any organizational directory (Any Microsoft Entra ID tenant - Multitenant)"**
   - **Redirect URI**: Leave blank (not needed for bots)
4. Click **Register**
5. Note the **Application (client) ID** - you'll need this

### Step 2: Create Client Secret

1. In your app registration, go to **Certificates & secrets**
2. Click **+ New client secret**
3. Add a description (e.g., "COBRA Bot Secret")
4. Select expiration (recommend 24 months)
5. Click **Add**
6. **IMMEDIATELY copy the secret value** - it won't be shown again

### Step 3: Configure API Permissions

1. Go to **API permissions**
2. Click **+ Add a permission**
3. Select **Microsoft Graph** -> **Delegated permissions**
4. Add: **`User.Read`**
5. Click **Add permissions**
6. Click **"Grant admin consent for [Your Tenant]"**

### Step 4: Create Azure Bot Resource

1. Go to **Azure Portal** -> **Create a resource**
2. Search for **"Azure Bot"**
3. Click **Create**
4. Configure:
   - **Bot handle**: `cobra-teams-bot` (globally unique)
   - **Subscription**: Your subscription
   - **Resource group**: Your resource group
   - **Pricing tier**: Standard (or Free for dev)
   - **Type of App**: **Single Tenant** (IMPORTANT!)
   - **Creation type**: **Use existing app registration**
   - **App ID**: Paste your Application (client) ID from Step 1
   - **App tenant ID**: Your tenant ID (from App Registration -> Overview)
5. Click **Review + create** -> **Create**

### Step 5: Configure Messaging Endpoint

1. Go to your Azure Bot resource -> **Configuration**
2. Set **Messaging endpoint** to your bot URL:
   - **Development**: `https://your-tunnel-url/api/messages`
   - **Production**: `https://your-domain/api/messages`
3. Click **Apply**

### Step 6: Enable Teams Channel

1. Go to **Channels** in your Azure Bot
2. Click **Microsoft Teams**
3. Accept the terms
4. Click **Apply**
5. Verify **Health status** shows "Healthy"

---

## App Registration Configuration

### Required Settings Summary

| Setting | Value |
|---------|-------|
| **Supported account types** | Accounts in any organizational directory (Multitenant) |
| **API Permissions** | Microsoft Graph -> User.Read (Delegated) |
| **Admin Consent** | Granted for your tenant |

### Verifying Configuration

1. **App Registration -> Authentication**
   - Supported account types: Multi-tenant check

2. **App Registration -> API permissions**
   - User.Read: Granted check

3. **Azure Bot -> Configuration**
   - Bot Type: Single Tenant check
   - Microsoft App ID: Matches App Registration check

---

## Local Development Setup

### Prerequisites

- .NET 10 SDK
- Microsoft Dev Tunnels CLI (`devtunnel`)
- Node.js (for frontend)

### Step 1: Create appsettings.Development.json

Copy the template and fill in your values:

```bash
cd src/backend/CobraAPI.TeamsBot
cp appsettings.Development.json.template appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "MicrosoftAppType": "SingleTenant",
  "MicrosoftAppId": "YOUR_APP_ID",
  "MicrosoftAppPassword": "YOUR_CLIENT_SECRET",
  "MicrosoftAppTenantId": "YOUR_TENANT_ID",
  "TokenValidation": {
    "Enabled": false
  },
  "Connections": {
    "ServiceConnection": {
      "Settings": {
        "AuthType": "ClientSecret",
        "ClientId": "YOUR_APP_ID",
        "ClientSecret": "YOUR_CLIENT_SECRET",
        "AuthorityEndpoint": "https://login.microsoftonline.com/YOUR_TENANT_ID",
        "Scopes": ["https://api.botframework.com/.default"]
      }
    }
  },
  "ConnectionsMap": [
    { "ServiceUrl": "*", "Connection": "ServiceConnection" }
  ],
  "Bot": {
    "DisplayName": "COBRA Teams Bot",
    "Description": "COBRA Event Management Bot"
  },
  "CobraApi": {
    "BaseUrl": "http://localhost:5000",
    "ApiKey": "dev-teams-bot-key-12345"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Agents": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

**CRITICAL**: The `ConnectionsMap` section is **required** for proactive messaging (COBRA -> Teams). Without it, outbound messages will fail with authentication errors.

### Step 2: Start Dev Tunnel

```bash
# First time: Login to dev tunnels
devtunnel user login

# Start tunnel (run in separate terminal)
devtunnel host -p 3978 --allow-anonymous
```

Note the tunnel URL (e.g., `https://abc123-3978.use.devtunnels.ms`)

### Step 3: Update Azure Bot Messaging Endpoint

1. Go to Azure Bot -> Configuration
2. Update **Messaging endpoint** to: `https://YOUR_TUNNEL_URL/api/messages`
3. Click **Apply**

### Step 4: Start Services

```bash
# Terminal 1: Start CobraAPI
cd src/backend/CobraAPI
dotnet run

# Terminal 2: Start TeamsBot
cd src/backend/CobraAPI.TeamsBot
dotnet run

# Terminal 3: Start Frontend
cd src/frontend
npm run dev
```

---

## Multi-Tenant Configuration

### How Multi-Tenant Works

With Single-Tenant Bot + Multi-Tenant App Registration:

1. **Your tenant** hosts the Azure Bot and App Registration
2. **Other tenants** can install the Teams app
3. When users in other tenants message the bot, Azure Bot Service handles the cross-tenant authentication

### Installing in Another Tenant

1. Create a Teams app package (`.zip`) containing `manifest.json`
2. The other tenant's admin uploads the package to their Teams Admin Center
3. Or publish to the Teams App Store for wider distribution

### Key Configuration Points

| Setting | Your Tenant (Bot Owner) | Customer Tenant |
|---------|------------------------|-----------------|
| App Registration | Multi-tenant | N/A (uses yours) |
| Azure Bot | Single-tenant | N/A |
| Admin Consent | Required | May be required |
| Teams App | Source | Installed |

---

## Testing the Bot

### Test 1: Receive Messages (Teams -> COBRA)

1. Open Teams in your test tenant
2. Find the COBRA Bot (search or in a channel)
3. Send a message: "Hello"
4. Check TeamsBot console for incoming message log
5. Verify message appears in COBRA UI

### Test 2: Send Messages (COBRA -> Teams)

1. Open COBRA UI
2. Navigate to an event with a linked Teams channel
3. Send a message from COBRA
4. Verify message appears in Teams

### Common Issues

See [TEAMS-BOT-TROUBLESHOOTING.md](./TEAMS-BOT-TROUBLESHOOTING.md) for detailed troubleshooting.

---

## Deploying to Production

### Environment Variables for Production

Set these in your App Service configuration:

```
MicrosoftAppType=SingleTenant
MicrosoftAppId=your-app-id
MicrosoftAppPassword=your-client-secret
MicrosoftAppTenantId=your-tenant-id
```

### Connections Configuration

For production, set the Connections via environment variables or Azure Key Vault:

```
Connections__ServiceConnection__Settings__AuthType=ClientSecret
Connections__ServiceConnection__Settings__ClientId=your-app-id
Connections__ServiceConnection__Settings__ClientSecret=your-client-secret
Connections__ServiceConnection__Settings__AuthorityEndpoint=https://login.microsoftonline.com/your-tenant-id
Connections__ServiceConnection__Settings__Scopes__0=https://api.botframework.com/.default
ConnectionsMap__0__ServiceUrl=*
ConnectionsMap__0__Connection=ServiceConnection
```

### Update Messaging Endpoint

After deployment, update Azure Bot -> Configuration -> Messaging endpoint to your production URL.

---

## Quick Reference

### Configuration Values You Need

| Value | Where to Find |
|-------|---------------|
| App ID | App Registration -> Overview -> Application (client) ID |
| Tenant ID | App Registration -> Overview -> Directory (tenant) ID |
| Client Secret | App Registration -> Certificates & secrets |
| Bot Handle | Azure Bot -> Overview |

### File Locations

| File | Purpose |
|------|---------|
| `appsettings.Development.json` | Local dev configuration (git-ignored) |
| `appsettings.Development.json.template` | Template for local config |
| `appsettings.json` | Base configuration |

### Required appsettings Sections

1. **MicrosoftAppId/Password/TenantId** - Bot identity
2. **Connections.ServiceConnection** - OAuth for proactive messaging
3. **ConnectionsMap** - Maps service URLs to connections (CRITICAL!)
4. **CobraApi** - Connection to main API

---

## See Also

- [TEAMS-BOT-TROUBLESHOOTING.md](./TEAMS-BOT-TROUBLESHOOTING.md) - Common issues and solutions
- [TEAMS-CUSTOMER-INSTALLATION-GUIDE.md](./TEAMS-CUSTOMER-INSTALLATION-GUIDE.md) - Customer installation guide
- [TEAMS-DEVELOPER-DOCUMENTATION.md](./TEAMS-DEVELOPER-DOCUMENTATION.md) - Full developer reference
