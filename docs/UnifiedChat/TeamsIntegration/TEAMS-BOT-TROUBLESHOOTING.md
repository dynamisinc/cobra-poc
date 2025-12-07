# Teams Bot Troubleshooting Guide

> **Last Updated:** 2025-12-07

This guide covers common issues when setting up and operating the COBRA Teams Bot.

## Table of Contents
1. [Quick Diagnosis Checklist](#quick-diagnosis-checklist)
2. [No Messages Received from Teams](#no-messages-received-from-teams)
3. [Messages Not Sending to Teams (401 Errors)](#messages-not-sending-to-teams-401-errors)
4. [Authentication Configuration Errors](#authentication-configuration-errors)
5. [Tenant and Multi-Tenant Issues](#tenant-and-multi-tenant-issues)
6. [Dev Tunnel Issues](#dev-tunnel-issues)
7. [Database Issues](#database-issues)

---

## Quick Diagnosis Checklist

Before diving into specific issues, verify these basics:

### Azure Configuration
- [ ] App Registration exists and you have the App ID
- [ ] Client secret is created and not expired
- [ ] App Registration is set to **Multi-tenant** (Authentication page)
- [ ] API permissions include `User.Read` with admin consent granted
- [ ] Azure Bot resource exists
- [ ] Azure Bot messaging endpoint is correct
- [ ] Teams channel is enabled on Azure Bot

### Local Configuration
- [ ] `appsettings.Development.json` exists (not just the template)
- [ ] App ID matches in all places
- [ ] Client secret matches in all places
- [ ] Tenant ID is your **bot's tenant** (not the target Teams tenant)
- [ ] `ConnectionsMap` section exists (critical for proactive messaging!)
- [ ] Dev tunnel is running and URL matches Azure Bot endpoint

### Services Running
- [ ] CobraAPI is running on port 5000
- [ ] TeamsBot is running on port 3978
- [ ] Dev tunnel is forwarding to port 3978

---

## No Messages Received from Teams

### Symptom
You send a message to the bot in Teams, but nothing appears in the TeamsBot console.

### Cause 1: Dev Tunnel Not Running or Wrong URL

**Check:**
```bash
# Verify tunnel is running
devtunnel host -p 3978 --allow-anonymous
```

**Fix:** Start the tunnel and update Azure Bot -> Configuration -> Messaging endpoint with the new URL.

### Cause 2: Bot Not Running on Port 3978

**Check:** Look for this in TeamsBot console output:
```
Now listening on: http://localhost:3978
```

**Fix:** Start the TeamsBot service:
```bash
cd src/backend/CobraAPI.TeamsBot
dotnet run
```

### Cause 3: ngrok Interstitial Page (Free Tier)

**Symptom:** Using ngrok free tier, Azure Bot gets HTML instead of your bot.

**Fix:** Switch to Microsoft Dev Tunnels (free, no interstitial):
```bash
devtunnel host -p 3978 --allow-anonymous
```

### Cause 4: Tenant Mismatch (Single-Tenant Bot)

**Symptom:** Bot was configured for a different tenant than where Teams is installed.

**Check:** Compare:
- Azure Bot -> Configuration -> App Tenant ID
- The tenant where your Teams is installed

**Fix:** Either:
1. Use the bot in the correct tenant, OR
2. Set App Registration to Multi-tenant (see Multi-Tenant section)

### Cause 5: Teams Channel Not Enabled

**Check:** Azure Bot -> Channels -> Microsoft Teams should show "Running"

**Fix:**
1. Go to Azure Bot -> Channels
2. Click Microsoft Teams
3. Accept terms and Apply

---

## Messages Not Sending to Teams (401 Errors)

### Symptom
Messages from COBRA don't appear in Teams. Logs show 401 Unauthorized errors.

### Error: "AADSTS7000229: Missing service principal in tenant"

**Full Error:**
```
AADSTS7000229: The client application {app-id} is missing service principal
in the tenant {tenant-id}
```

**Cause:** The app is trying to authenticate against a tenant where it's not registered.

**Fix:**
1. Make sure App Registration is set to **Multi-tenant**
2. Use your **bot's tenant ID** in config (not the target tenant)
3. Grant admin consent in your tenant

### Error: "Authentication configuration section 'Settings' not Found"

**Full Error:**
```
System.ArgumentException: Authentication configuration section 'Settings' not Found
An instance of IAccessTokenProvider not found for 00000000-0000-0000-0000-000000000000
```

**Cause:** Missing `ConnectionsMap` in configuration.

**Fix:** Add `ConnectionsMap` to `appsettings.Development.json`:
```json
{
  "Connections": {
    "ServiceConnection": {
      "Settings": {
        "AuthType": "ClientSecret",
        "ClientId": "your-app-id",
        "ClientSecret": "your-secret",
        "AuthorityEndpoint": "https://login.microsoftonline.com/your-tenant-id",
        "Scopes": ["https://api.botframework.com/.default"]
      }
    }
  },
  "ConnectionsMap": [
    { "ServiceUrl": "*", "Connection": "ServiceConnection" }
  ]
}
```

### Error: "Access Token was rejected by the remote endpoint"

**Full Error:**
```
The Access Token created to respond to a request from the agent was rejected
by the remote endpoint. This can be caused by incorrect connection configuration.
```

**Cause:** Mismatch between Azure Bot Type and App Registration type.

**Check:**
- Azure Bot -> Configuration -> Bot Type
- App Registration -> Authentication -> Supported account types

**Fix:** They must align:

| Azure Bot Type | App Registration | AuthorityEndpoint |
|---------------|------------------|-------------------|
| Single Tenant | Multi-tenant OK | `https://login.microsoftonline.com/{your-tenant-id}` |
| Multi Tenant | Multi-tenant | `https://login.microsoftonline.com/botframework.com` |

**If Azure Bot is Single-Tenant (common, can't be changed):**
1. Keep App Registration as Multi-tenant (for cross-org support)
2. Use your bot's tenant ID in AuthorityEndpoint
3. Use `MicrosoftAppType: "SingleTenant"` in appsettings

---

## Authentication Configuration Errors

### Error: "invalid_client"

**Cause:** Client secret is wrong or expired.

**Fix:**
1. Go to App Registration -> Certificates & secrets
2. Check if secret is expired
3. Create a new secret if needed
4. Update `appsettings.Development.json` with new secret (in BOTH places)

### Error: "AADSTS1003031: Misconfigured required resource access"

**Cause:** App Registration has no API permissions configured.

**Fix:**
1. Go to App Registration -> API permissions
2. Add permission: Microsoft Graph -> Delegated -> `User.Read`
3. Click "Grant admin consent for {your tenant}"

### Warning: "No connections map found in configuration"

**Cause:** `ConnectionsMap` is missing or malformed.

**Fix:** Ensure this exists in your appsettings:
```json
"ConnectionsMap": [
  { "ServiceUrl": "*", "Connection": "ServiceConnection" }
]
```

---

## Tenant and Multi-Tenant Issues

### Understanding the Tenants

| Tenant | Description | Example |
|--------|-------------|---------|
| Bot Tenant | Where Azure Bot and App Registration live | Your company's Azure AD |
| Target Tenant | Where Teams users are | Customer's Microsoft 365 |

### Single-Tenant Bot + Multi-Tenant App Registration

This is the **recommended configuration** for bots that need to work across organizations:

1. **Azure Bot Type**: Single Tenant (often can't be changed after creation)
2. **App Registration**: Multi-tenant
3. **appsettings**: Use your **bot tenant ID**, not target tenant

```json
{
  "MicrosoftAppType": "SingleTenant",
  "MicrosoftAppTenantId": "YOUR_BOT_TENANT_ID",
  "Connections": {
    "ServiceConnection": {
      "Settings": {
        "AuthorityEndpoint": "https://login.microsoftonline.com/YOUR_BOT_TENANT_ID"
      }
    }
  }
}
```

### "Bot Type Cannot Be Changed"

**Problem:** Azure Bot Type dropdown is greyed out.

**Solution:** You cannot change Bot Type after creation. Options:
1. Delete and recreate the Azure Bot resource with correct type
2. Use Single-Tenant with Multi-Tenant App Registration (recommended)

### Multi-Tenant Bot Deprecation

**Warning:** Multi-tenant Bot Type is deprecated after July 31, 2025.

**Recommendation:** Use Single-Tenant Bot + Multi-Tenant App Registration for new bots.

---

## Dev Tunnel Issues

### Error: "Connection refused" in tunnel

**Symptom:**
```
ClientSSH: PortForwardingService connection to localhost:3978 failed:
No connection could be made because the target machine actively refused it.
```

**Cause:** TeamsBot is not running on port 3978.

**Fix:** Start the bot first, then the tunnel:
```bash
# Terminal 1
cd src/backend/CobraAPI.TeamsBot
dotnet run

# Terminal 2 (after bot starts)
devtunnel host -p 3978 --allow-anonymous
```

### Tunnel URL Changed

**Problem:** Dev tunnel URL changes between sessions.

**Fix Options:**
1. Update Azure Bot endpoint each time (tedious)
2. Create a persistent named tunnel:
```bash
# One-time setup
devtunnel create cobra-bot --allow-anonymous
devtunnel port create cobra-bot -p 3978

# Use each time
devtunnel host cobra-bot
```

### Monitoring Tunnel Traffic

Visit https://devtunnels.ms/ to see active tunnels and traffic.

---

## Database Issues

### Error: "String or binary data would be truncated"

**Cause:** Teams conversation IDs are longer than the database column allows.

**Fix:** Increase column size:
```bash
cd src/backend/CobraAPI
dotnet ef migrations add IncreaseExternalGroupIdLength
dotnet ef database update
```

Migration should change `ExternalGroupId` from 100 to 500 characters.

### ConversationReference Not Stored

**Symptom:** Inbound messages work, but outbound fails with "No ConversationReference stored"

**Cause:** The conversation reference wasn't saved when the bot was first added to Teams.

**Fix:**
1. Remove the bot from the Teams channel/chat
2. Re-add the bot
3. Send a message from Teams to trigger reference storage

---

## Diagnostic Commands

### Check Bot Health
```bash
curl http://localhost:3978/api/internal/health
```

### Check Active Conversations
```bash
curl http://localhost:3978/api/internal/conversations
```

### Test Bot Endpoint
```bash
curl -X POST http://localhost:3978/api/messages \
  -H "Content-Type: application/json" \
  -d '{"type":"message","text":"test"}'
```

### View Bot Logs
The TeamsBot logs to console with detailed MSAL authentication info when `Microsoft.Agents` log level is set to `Debug`.

---

## Getting Help

If issues persist:
1. Check Azure Bot -> Activity log for errors
2. Enable verbose logging: Set `Microsoft.Agents` to `Debug` in appsettings
3. Verify all configuration values match across Azure and appsettings
4. Check that client secret hasn't expired

---

## See Also

- [TEAMS-BOT-SETUP.md](./TEAMS-BOT-SETUP.md) - Complete setup guide
- [TEAMS-CUSTOMER-INSTALLATION-GUIDE.md](./TEAMS-CUSTOMER-INSTALLATION-GUIDE.md) - Customer installation guide
