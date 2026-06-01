# Implementation Steps

> Do these in order — each step depends on the one before it.

---

## Step 1 — Sign up for Azure free account

Go to [azure.microsoft.com/free](https://azure.microsoft.com/free) and create an account. You'll need a Microsoft account and a credit card (for identity verification only — you won't be charged within the free tier). You get $200 credit for the first 30 days plus 12 months of free services.

After signup, note down your **Subscription ID** — you'll find it in the Azure Portal under "Subscriptions". You'll need it later.

---

## Step 2 — Install tools on your machine

You need two tools installed locally:

**Azure CLI** — a command-line tool for talking to Azure. Install from [aka.ms/installazurecli](https://aka.ms/installazurecli), then run:
```bash
az login
```
This opens a browser, you log in, and your terminal is now authenticated with Azure.

**Terraform** — the tool that reads your `infra/` files and creates Azure resources from them. Install from [developer.hashicorp.com/terraform/install](https://developer.hashicorp.com/terraform/install).

Verify both are working:
```bash
az --version
terraform --version
```

---

## Step 3 — Create a Service Principal

A **service principal** is like a "robot user" account in Azure — it's what GitHub Actions will use to deploy your app automatically, so it doesn't need your personal login credentials.

Run this in your terminal (replace `your-subscription-id` with the one from Step 1):
```bash
az ad sp create-for-rbac \
  --name "inv-tracker-deployer" \
  --role Contributor \
  --scopes /subscriptions/your-subscription-id
```

This prints out a JSON block. **Save it somewhere safe** — you only see the secret once:
```json
{
  "appId":       "...",   ← this is AZURE_CLIENT_ID
  "password":    "...",   ← this is AZURE_CLIENT_SECRET
  "tenant":      "..."    ← this is AZURE_TENANT_ID
}
```

You now have all four Azure credentials you'll need:
- `AZURE_CLIENT_ID` — from `appId` above
- `AZURE_CLIENT_SECRET` — from `password` above
- `AZURE_TENANT_ID` — from `tenant` above
- `AZURE_SUBSCRIPTION_ID` — from Step 1

---

## Step 4 — Create Terraform state storage (manual, one-time)

Terraform needs somewhere to save a record of what it has created (called **state** — essentially a log file that tracks your infrastructure). This storage has to exist *before* Terraform runs, so it's the one bit of Azure you create by hand.

Run these commands:
```bash
# Create a resource group (a folder in Azure that holds related resources)
az group create --name rg-inv-tracker --location uksouth

# Create a storage account (give it a globally unique name, lowercase letters/numbers only)
az storage account create \
  --name invtrackertfstate \
  --resource-group rg-inv-tracker \
  --sku Standard_LRS

# Create a container inside the storage account (like a folder inside it)
az storage container create \
  --name tfstate \
  --account-name invtrackertfstate
```

---

## Step 5 — Write the Terraform files

Create the `infra/` folder in the root of the repo and write the following files. Each file defines one piece of Azure infrastructure.

**`infra/main.tf`** — tells Terraform to use the Azure provider and where to store state:
```hcl
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "rg-inv-tracker"
    storage_account_name = "invtrackertfstate"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}
```

**`infra/variables.tf`** — configurable values so you're not hardcoding things:
```hcl
variable "location" {
  default = "uksouth"
}
variable "app_name" {
  default = "inv-tracker"
}
```

**`infra/resource_group.tf`** — the Azure "folder" that holds everything:
```hcl
resource "azurerm_resource_group" "main" {
  name     = "rg-${var.app_name}"
  location = var.location
}
```

**`infra/app_service.tf`** — the server that runs your .NET backend:
```hcl
resource "azurerm_service_plan" "main" {
  name                = "asp-${var.app_name}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "F1"  # Free tier
}

resource "azurerm_linux_web_app" "main" {
  name                = "${var.app_name}-api"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    always_on = false  # must be false on Free tier
    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = {
    ASPNETCORE_ENVIRONMENT = "Production"
  }
}
```

**`infra/mysql.tf`** — the database:
```hcl
resource "azurerm_mysql_flexible_server" "main" {
  name                   = "${var.app_name}-mysql"
  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  administrator_login    = "adminuser"
  administrator_password = var.db_password  # add this to variables.tf
  sku_name               = "B_Standard_B1ms"
  version                = "8.0.21"
  zone                   = "1"

  storage {
    size_gb = 20
    auto_grow_enabled = false
  }
}

resource "azurerm_mysql_flexible_server_firewall_rule" "azure" {
  name                = "allow-azure-services"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.main.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}
```

**`infra/static_web_app.tf`** — the hosting for your React frontend:
```hcl
resource "azurerm_static_web_app" "main" {
  name                = "${var.app_name}-frontend"
  resource_group_name = azurerm_resource_group.main.name
  location            = "eastus2"  # Static Web Apps has limited regions
  sku_tier            = "Free"
  sku_size            = "Free"
}
```

**`infra/outputs.tf`** — values Terraform prints after it finishes, which you'll need for config:
```hcl
output "app_service_url" {
  value = "https://${azurerm_linux_web_app.main.default_hostname}"
}
output "mysql_host" {
  value = azurerm_mysql_flexible_server.main.fqdn
}
output "static_web_app_api_token" {
  value     = azurerm_static_web_app.main.api_key
  sensitive = true
}
```

---

## Step 6 — Run Terraform

From inside the `infra/` folder:

```bash
# Initialise Terraform — downloads the Azure plugin and connects to state storage
terraform init

# Preview what Terraform is about to create (nothing actually happens yet)
terraform plan

# Create everything in Azure
terraform apply
```

Terraform will ask "Do you want to perform these actions?" — type `yes`.

When it finishes, run:
```bash
terraform output
terraform output -raw static_web_app_api_token
```

**Write down all the output values** — you need them in the next step.

---

## Step 7 — Add GitHub Actions Secrets

This is where you connect the service principal to GitHub Actions. There's no single "link" command — you store the service principal's credentials as encrypted secrets in GitHub, and the pipeline YAML (written in Steps 9 and 10) reads them at runtime to log in to Azure on each deploy.

Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**.

Add all of these:

| Secret name | Value |
|-------------|-------|
| `AZURE_CLIENT_ID` | from Step 3 |
| `AZURE_CLIENT_SECRET` | from Step 3 |
| `AZURE_TENANT_ID` | from Step 3 |
| `AZURE_SUBSCRIPTION_ID` | from Step 1 |
| `AZURE_WEBAPP_NAME` | `inv-tracker-api` (the App Service name from Terraform) |
| `STATIC_WEB_APP_API_TOKEN` | from `terraform output` in Step 6 |
| `JWT_SECRET` | make up a long random string (32+ characters) |
| `DB_PASSWORD` | the password you used in `mysql.tf` |

---

## Step 8 — Update Backend code

Two small changes to the backend before writing the pipeline.

**`Backend/InvTracker/Program.cs`** — add automatic database migrations on startup. Find where the app is built and add before `app.Run()`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InvTrackerContext>();
    db.Database.Migrate();
}
```

**`Backend/InvTracker/appsettings.Production.json`** — create this new file as a template (no real secrets, those come from environment variables at runtime):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

The JWT secret and database connection string will be injected automatically by the deploy pipeline as environment variables — you don't put them in this file.

---

## Step 9 — Write the backend deploy pipeline

Create `.github/workflows/cd-backend.yml`:

```yaml
name: Deploy Backend

on:
  push:
    branches: [main]
    paths: [Backend/**]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore Backend/InvTracker/InvTracker.csproj

      - name: Run tests
        run: dotnet test Backend/InvTracker.UnitTests/InvTracker.UnitTests.csproj

      - name: Publish
        # Compiles the app and outputs it ready to run (like a build folder)
        run: dotnet publish Backend/InvTracker/InvTracker.csproj -c Release -o ./publish

      - name: Login to Azure
        uses: azure/login@v2
        with:
          creds: |
            {
              "clientId": "${{ secrets.AZURE_CLIENT_ID }}",
              "clientSecret": "${{ secrets.AZURE_CLIENT_SECRET }}",
              "tenantId": "${{ secrets.AZURE_TENANT_ID }}",
              "subscriptionId": "${{ secrets.AZURE_SUBSCRIPTION_ID }}"
            }

      - name: Set App Service environment variables
        # Injects secrets into the running app as environment variables
        run: |
          az webapp config appsettings set \
            --name ${{ secrets.AZURE_WEBAPP_NAME }} \
            --resource-group rg-inv-tracker \
            --settings \
              JWT__Secret="${{ secrets.JWT_SECRET }}" \
              "ConnectionStrings__InvestmentTrackerDB=Server=${{ secrets.MYSQL_HOST }};Database=investment_tracker_db;User Id=adminuser;Password=${{ secrets.DB_PASSWORD }};SslMode=Required;"

      - name: Deploy to App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_WEBAPP_NAME }}
          package: ./publish
```

> Add `MYSQL_HOST` to your GitHub secrets — it's the `mysql_host` value from `terraform output`.

---

## Step 10 — Write the frontend deploy pipeline

Create `.github/workflows/cd-frontend.yml`:

```yaml
name: Deploy Frontend

on:
  push:
    branches: [main]
    paths: [Frontend/**]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: Frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: Frontend

      - name: Build
        run: npm run build
        working-directory: Frontend
        env:
          VITE_API_URL: https://${{ secrets.AZURE_WEBAPP_NAME }}.azurewebsites.net/api

      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.STATIC_WEB_APP_API_TOKEN }}
          action: upload
          app_location: Frontend
          output_location: dist
```

---

## Step 11 — Update the existing CI workflows

The existing `backend-ci.yml` and `frontend-ci.yml` only run on PRs. Leave them as-is — they act as your quality gate on pull requests. The new `cd-*.yml` files handle deploys on push to `main` and include their own build/test steps.

No changes needed here unless you want to tighten them up later.

---

## Step 12 — First deploy

**Before pushing — update CORS in the backend**

Your backend currently only allows requests from `http://localhost:5173` (your local dev address). In production the frontend will be coming from the Static Web App URL instead, so you need to update `Program.cs` to allow that.

Find the CORS configuration in `Program.cs` — it will look something like:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
    });
});
```

Update it to include both URLs (keep localhost so local dev still works):
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://<your-static-web-app-url>.azurestaticapps.net"
        )
    });
});
```

You'll get the Static Web App URL from `terraform output` after Step 6.

---

Then push everything to `main`:
```bash
git add .
git commit -m "add ci/cd and infrastructure"
git push origin main
```

Go to your GitHub repo → **Actions** tab. You should see the `Deploy Backend` and `Deploy Frontend` workflows running. Watch the logs — if anything fails, the error message will tell you what's wrong.

---

## Step 13 — Verify it's working

Once both pipelines go green:

1. **Backend** — visit `https://inv-tracker-api.azurewebsites.net/swagger` in your browser. You should see the Swagger API docs. (First load may take 10–15 seconds due to cold start.)
2. **Frontend** — find your Static Web App URL in the Azure Portal (or `terraform output`). Open it and try logging in.
3. **Database** — if the app loads and you can register/login, the migrations ran successfully on first boot.

---

## Sequence summary

```
1. Azure account signup
2. Install tools (Azure CLI, Terraform)
3. Create service principal → save 4 credentials
4. Create Terraform state storage → 3 az commands
5. Write infra/ Terraform files
6. terraform init + apply → Azure resources created
7. Add GitHub secrets (credentials + Terraform outputs)
8. Update Program.cs + add appsettings.Production.json
9. Write cd-backend.yml
10. Write cd-frontend.yml
11. (Leave existing CI workflows alone)
12. Push to main → first deploy
13. Verify in browser
```
