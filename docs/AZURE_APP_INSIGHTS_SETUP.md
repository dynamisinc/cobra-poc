# Azure Application Insights Setup Guide

## Overview
This guide walks through setting up Azure Application Insights for the Checklist POC to enable monitoring, logging, and diagnostics.

## Prerequisites
- Azure CLI installed ([Download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- Azure subscription
- Appropriate permissions to create resources

## Step 1: Login to Azure
```bash
az login
```

## Step 2: Set Default Subscription (if you have multiple)
```bash
# List subscriptions
az account list --output table

# Set default subscription
az account set --subscription "Your-Subscription-Name-Or-Id"
```

## Step 3: Create Resource Group (if not exists)
```bash
az group create \
  --name rg-checklist-poc \
  --location eastus
```

## Step 4: Create Application Insights Resource
```bash
az monitor app-insights component create \
  --app checklist-poc-appinsights \
  --location eastus \
  --resource-group rg-checklist-poc \
  --application-type web \
  --kind web
```

## Step 5: Get Connection String
```bash
az monitor app-insights component show \
  --app checklist-poc-appinsights \
  --resource-group rg-checklist-poc \
  --query connectionString \
  --output tsv
```

**Copy the output** - this is your Application Insights connection string.

## Step 6: Get Instrumentation Key (Legacy - for reference)
```bash
az monitor app-insights component show \
  --app checklist-poc-appinsights \
  --resource-group rg-checklist-poc \
  --query instrumentationKey \
  --output tsv
```

## Step 7: Update appsettings.json
Add the connection string to your `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=YOUR-KEY-HERE;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/"
  }
}
```

## Step 8: Update appsettings.Development.json
For local development, you can use the same connection string or disable:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "",
    "EnableAdaptiveSampling": false,
    "EnableDependencyTracking": true
  }
}
```

## Viewing Telemetry in Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Search for "checklist-poc-appinsights"
3. View dashboards:
   - **Overview**: High-level metrics
   - **Live Metrics**: Real-time performance
   - **Application Map**: Dependency visualization
   - **Performance**: Request performance
   - **Failures**: Error tracking
   - **Logs**: Query telemetry data

## Useful Kusto Queries

### View All Requests
```kusto
requests
| where timestamp > ago(1h)
| order by timestamp desc
| take 50
```

### View Exceptions
```kusto
exceptions
| where timestamp > ago(1h)
| order by timestamp desc
```

### View Custom Events
```kusto
customEvents
| where timestamp > ago(1h)
| order by timestamp desc
```

### View API Response Times
```kusto
requests
| where timestamp > ago(1h)
| summarize avg(duration), percentile(duration, 95) by name
| order by avg_duration desc
```

## Local Development Without Azure

If you don't want to create an Azure resource for local development:

1. Leave the connection string empty in `appsettings.Development.json`
2. Application Insights will be disabled locally
3. Logs will still appear in console output

## Cleanup (When Done with POC)

```bash
# Delete the Application Insights resource
az monitor app-insights component delete \
  --app checklist-poc-appinsights \
  --resource-group rg-checklist-poc

# Optional: Delete entire resource group (if no other resources)
az group delete --name rg-checklist-poc --yes
```

## Additional Resources
- [Application Insights Overview](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [ASP.NET Core Integration](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)
- [Kusto Query Language](https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/)
