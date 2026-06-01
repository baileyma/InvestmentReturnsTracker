# InvTracker — Azure SQL Migration

## Context

This document summarises the database migration work completed for InvTracker, moving from MySQL/Pomelo to Azure SQL Server. It is intended to provide a Cowork project with context on what has been done and what remains.

---

## Starting Point

- `Program.cs:47` was using MySQL with the Pomelo EF Core provider
- `Infra/mysql.tf` had already been updated to define Azure SQL Server resources, but nothing had been deployed to Azure yet

---

## What Was Completed

### 1. Azure Infrastructure (via Portal + Terraform)

- Created an Azure SQL **logical server** (`inv-tracker-sqlserver`) in **North Europe**
  - Note: East US and several other regions were unavailable on the free trial subscription
- Created a **SQL database** (`investment-tracker-db`) with the following settings:
  - SKU: `GP_S_Gen5_1` (serverless)
  - Auto-pause: 60 minutes
  - Backup storage: `Local` (required by the free tier — Geo redundancy is not allowed)
- Created a **firewall rule** (`allow-azure-services`, IP range `0.0.0.0–0.0.0.0`) to allow App Service to reach the database

### 2. Terraform State Reconciliation

Resources were created manually in the portal first, then imported into Terraform state:

```bash
terraform import azurerm_mssql_server.main <resource-id>
terraform import azurerm_mssql_database.main <resource-id>
```

The firewall rule did not exist yet, so it was created directly via `terraform apply`.

### 3. Terraform Config Changes (`Infra/mysql.tf`)

| Field                  | Old Value     | New Value                                   |
| ---------------------- | ------------- | ------------------------------------------- |
| `location`             | `"eastus"`    | `"northeurope"`                             |
| `administrator_login`  | `"adminuser"` | `"CloudSA2df448e6"` (portal auto-generated) |
| `storage_account_type` | _(not set)_   | `"Local"`                                   |

### 4. Issues Resolved Along the Way

- **Git Bash path mangling** — resource IDs starting with `/subscriptions/...` were being prepended with `C:/Program Files/Git/`. Fixed by switching to PowerShell.
- **Admin login mismatch** — the portal's free offer auto-generated the login as `CloudSA2df448e6` rather than the intended `adminuser`. Terraform config updated to match.
- **Entra-only auth conflict** — the free offer enabled Microsoft Entra authentication only. Disabled via the portal (Networking → Microsoft Entra ID → uncheck "Support only Microsoft Entra authentication").
- **Free tier storage restriction** — Terraform attempted to change `storage_account_type` to `Geo`, which is not permitted on free tier databases. Fixed by explicitly setting `"Local"` in the config.

---

## Current State

All infrastructure is deployed and Terraform state is clean. Running `terraform plan` returns no changes.

| Resource                            | Status      |
| ----------------------------------- | ----------- |
| `azurerm_resource_group.main`       | ✅ Deployed |
| `azurerm_service_plan.main`         | ✅ Deployed |
| `azurerm_linux_web_app.main`        | ✅ Deployed |
| `azurerm_static_web_app.main`       | ✅ Deployed |
| `azurerm_mssql_server.main`         | ✅ Deployed |
| `azurerm_mssql_database.main`       | ✅ Deployed |
| `azurerm_mssql_firewall_rule.azure` | ✅ Deployed |

**SQL Server host:** `inv-tracker-sqlserver.database.windows.net`

---

## What's Next (Step 5)

The backend code still uses MySQL/Pomelo and needs to be switched to SQL Server:

1. **Remove** the Pomelo NuGet package (`Pomelo.EntityFrameworkCore.MySql`)
2. **Add** the SQL Server EF Core package (`Microsoft.EntityFrameworkCore.SqlServer`)
3. **Update `Program.cs:47`** — change `UseMySql(...)` to `UseSqlServer(...)`
4. **Update the connection string** to SQL Server format, pointing at `inv-tracker-sqlserver.database.windows.net`
5. **Run EF Core migrations** against the new database
