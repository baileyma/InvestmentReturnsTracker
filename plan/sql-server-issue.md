# SQL Server Provisioning Issue — May 2026

## What happened

Tried to provision a database via Terraform as part of Step 6. Hit `ProvisioningDisabled` errors across every region tried:
- `northeurope` — failed
- `westeurope` — failed
- `uksouth` — failed
- `eastus` — failed

This is a **subscription-level restriction** on new Azure free trial accounts, not a regional capacity issue. No amount of region-switching will fix it without Azure lifting the restriction.

## Support request

A support request was submitted via **portal.azure.com → Help + Support**. Request was for quota type **SQL Database**, asking Azure to lift the provisioning restriction on the free trial subscription. Should be approved within a few hours to a day.

## Backend code changes

During this session the backend was temporarily switched from MySQL to SQL Server (Pomelo uninstalled, SqlServer package installed, `Program.cs` and connection strings updated). **These changes have since been reverted** — the backend is back to its original MySQL/Pomelo state.

If Azure SQL provisioning is unblocked next session, the switch will need to be made again. See git history for the exact changes made.

## Infra changes made

`Infra/mysql.tf` was rewritten from MySQL Flexible Server to Azure SQL. `Infra/outputs.tf` was updated — `mysql_host` output renamed to `sql_server_host`.

## Things to try next session

1. **Check if the support request was approved** — if yes, run `terraform apply` and it should work.

2. **Try creating the SQL server manually in the Azure Portal first** — there's a possibility Terraform is hitting a restriction that the Portal bypasses. If you can create it manually, you can then import it into Terraform state with:
   ```bash
   terraform import azurerm_mssql_server.main /subscriptions/1de499b9-db43-4155-9517-48768cc6a461/resourceGroups/rg-inv-tracker-app/servers/inv-tracker-sqlserver
   ```

3. **If going back to MySQL** — revert the backend code changes listed above, and restore `Infra/mysql.tf` to use `azurerm_mysql_flexible_server`. The same provisioning issue will likely apply unless the support request covers MySQL too.

## Current state of Azure resources (as of this session)

| Resource | Status |
|----------|--------|
| Resource group (`rg-inv-tracker-app`) | ✅ Created — westeurope |
| App Service Plan (`asp-inv-tracker`) | ✅ Created — westeurope |
| App Service / Web App (`inv-tracker-api`) | ✅ Created — westeurope |
| Static Web App (`inv-tracker-frontend`) | ✅ Created — eastus2 |
| SQL Server | ❌ Not created — provisioning blocked |
| SQL Database | ❌ Not created |
