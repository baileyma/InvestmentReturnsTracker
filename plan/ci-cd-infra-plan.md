# CI/CD & Infrastructure / Deployment Plan

> **Status:** 🟢 Decisions finalised — ready for implementation
> **Last updated:** 2026-04-18
> **Project type:** Personal / portfolio project
> **Cost target:** £0/month (Azure free tier for 12 months)

---

## 1. Project Context

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + Vite (SPA) |
| Backend | ASP.NET Core Web API (.NET 10) |
| Database | MySQL 8.0+ via EF Core (Pomelo) |
| Auth | JWT Bearer tokens |
| Repo | GitHub (already using GitHub Actions) |

**Existing CI (GitHub Actions — PRs to main only):**
- `backend-ci.yml` — `dotnet build` + `dotnet test` when `Backend/**` changes
- `frontend-ci.yml` — `npm ci` + `npm run build` when `Frontend/**` changes

---

## 2. Decisions Made

| Topic | Decision | Notes |
|-------|----------|-------|
| Cloud provider | **Azure** | Free account (12 months) |
| Backend hosting | **Azure App Service F1 (Free tier)** | Code deploy — no Docker needed |
| Frontend hosting | **Azure Static Web Apps (Free tier)** | Always free, global CDN, auto TLS |
| Database | **Azure Database for MySQL Flexible Server B1ms** | Free for 12 months (750 hrs/mo included) |
| Container registry | **None** | Not needed without Docker |
| Docker | **None** | Removed — App Service supports .NET code deploy directly |
| IaC | **Terraform** | Provisions all Azure resources as code |
| Deploy trigger | **Push to `main` → auto-deploy** | Single environment |
| DB migrations | **On app startup** | `context.Database.Migrate()` in `Program.cs` |
| Secrets | **GitHub Actions Secrets → App Service env vars** | Injected at deploy time |
| CI quality gates | **Unit tests must pass + Frontend build must succeed** | Block deploy on failure |
| Monitoring | **None for now** | Can be added later |

---

## 3. Target Architecture

```
┌─────────────────────────────────────────────────────────┐
│                        Azure (Free)                     │
│                                                         │
│  ┌─────────────────────┐   ┌───────────────────────┐   │
│  │  Azure Static Web   │   │   Azure App Service   │   │
│  │  Apps (Frontend)    │   │   F1 Free tier        │   │
│  │                     │   │                       │   │
│  │  React 18 + Vite    │──▶│  .NET 10 code deploy  │   │
│  │  Builds from source │   │  (zip deploy, no      │   │
│  │  Free tier + CDN    │   │   Docker)             │   │
│  └─────────────────────┘   └───────────┬───────────┘   │
│                                         │               │
│                             ┌───────────▼───────────┐   │
│                             │  Azure Database for   │   │
│                             │  MySQL Flexible Svr   │   │
│                             │  B1ms — free 12 mo    │   │
│                             │  Migrations on boot   │   │
│                             └───────────────────────┘   │
│                                                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │  Azure Blob Storage — Terraform state only      │   │
│  │  (free 12 months)                               │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## 4. Cost Breakdown

| Resource | Tier | Monthly cost |
|----------|------|-------------|
| Azure App Service (backend) | F1 Free | **£0** |
| Azure Static Web Apps (frontend) | Free | **£0** |
| Azure Database for MySQL Flexible Server | B1ms — free 12 months | **£0** |
| Azure Blob Storage (Terraform state) | Free 12 months | **£0** |
| Azure Application Insights | Not included | **£0** |
| Container registry | None needed | **£0** |
| **Total** | | **£0/mo** |

### ⚠️ One known trade-off: cold starts
App Service F1 has no "Always On" option. If the API is idle for ~20 minutes, it sleeps. The first request after that takes 5–15 seconds to wake up. For a portfolio project with light traffic this is acceptable — just worth knowing before your first demo.

---

## 5. CI/CD Pipeline Design

### 5.1 Trigger
- Any push to `main` runs the full pipeline.
- PRs to `main` run CI (test + build) only — no deploy.

### 5.2 Pipeline Stages

```
Push to main
     │
     ├─── [Backend CI + Deploy]           ├─── [Frontend Deploy]
     │    dotnet restore                  │    Azure Static Web Apps
     │    dotnet build                    │    GitHub Actions integration
     │    dotnet test  ← gate             │    npm ci + npm run build
     │    dotnet publish -c Release       │    deploy dist/ to SWA
     │    zip publish output              │
     │    az webapp deploy (zip deploy)   │
     │    App starts → migrations run     │
     │                                    │
     └────────────────────────────────────┘
```

No Docker. No container registry. No image builds.

### 5.3 GitHub Actions Workflows

| File | Trigger | Purpose |
|------|---------|---------|
| `.github/workflows/backend-ci.yml` | PR to `main`, `Backend/**` | Build + test (update existing) |
| `.github/workflows/frontend-ci.yml` | PR to `main`, `Frontend/**` | Build check (update existing) |
| `.github/workflows/cd-backend.yml` | Push to `main`, `Backend/**` | Publish + zip deploy to App Service |
| `.github/workflows/cd-frontend.yml` | Push to `main`, `Frontend/**` | Deploy to Azure Static Web Apps |

---

## 6. Infrastructure as Code (Terraform)

### 6.1 Resources to Define

```
infra/
├── main.tf              # Provider config, remote state backend
├── variables.tf         # Input variables (region, app name, etc.)
├── outputs.tf           # App Service URL, MySQL host, etc.
├── resource_group.tf    # Azure Resource Group
├── app_service.tf       # App Service Plan (F1) + Web App (Linux, .NET 10)
├── mysql.tf             # Azure Database for MySQL Flexible Server
├── static_web_app.tf    # Azure Static Web Apps (Free tier)
└── app_insights.tf      # Application Insights — skipped for now
```

No `acr.tf` — container registry is not needed.

### 6.2 Terraform State
- Remote state stored in **Azure Blob Storage** (free tier, created as a bootstrap step).

### 6.3 Bootstrap Steps (one-time, manual)
1. Create Azure account (free account signup)
2. Install Azure CLI + Terraform locally
3. `az login` and set subscription
4. Create a service principal for Terraform: `az ad sp create-for-rbac`
5. Create storage account + container for Terraform state (free tier)
6. Add service principal credentials to GitHub Actions Secrets
7. `terraform init` + `terraform apply` to provision all resources
8. Copy outputs (App Service URL, MySQL host, App Insights key) into remaining GitHub Actions Secrets

---

## 7. Secrets & Environment Configuration

### 7.1 GitHub Actions Secrets Required

| Secret name | Used for |
|-------------|----------|
| `AZURE_CLIENT_ID` | Terraform / Azure CLI auth |
| `AZURE_CLIENT_SECRET` | Terraform / Azure CLI auth |
| `AZURE_SUBSCRIPTION_ID` | Terraform / Azure CLI auth |
| `AZURE_TENANT_ID` | Terraform / Azure CLI auth |
| `AZURE_WEBAPP_NAME` | Target App Service name for zip deploy |
| `JWT_SECRET` | Injected as App Service env var |
| `DB_PASSWORD` | Injected as App Service env var |
| `STATIC_WEB_APP_API_TOKEN` | Azure Static Web Apps deploy token |

### 7.2 App Service Application Settings (set at deploy time)
```
JWT__ValidIssuer=https://<app-service-url>
JWT__ValidAudience=https://<app-service-url>
JWT__Secret=${{ secrets.JWT_SECRET }}
ConnectionStrings__InvestmentTrackerDB=Server=<mysql-host>;Database=investment_tracker_db;User Id=<user>;Password=${{ secrets.DB_PASSWORD }};SslMode=Required;
ASPNETCORE_ENVIRONMENT=Production
```

### 7.3 Frontend Environment (.env at build time in CI)
```
VITE_API_URL=https://<app-service-url>/api
```

---

## 8. Database Plan

### 8.1 Azure Database for MySQL Flexible Server
- SKU: `Standard_B1ms` — covered by Azure free account for 12 months
- Storage: 20 GB (do not enable auto-grow — no need to allow unexpected cost)
- Backup retention: 7 days (default, included free)
- TLS/SSL required for all connections
- Public access with firewall rule allowing Azure services

### 8.2 EF Core Migrations
- `Program.cs` calls `context.Database.Migrate()` on startup
- Migrations applied automatically on first boot after each deploy

---

## 9. Files to Create / Modify

### New files
- [ ] `infra/main.tf`
- [ ] `infra/variables.tf`
- [ ] `infra/outputs.tf`
- [ ] `infra/resource_group.tf`
- [ ] `infra/app_service.tf`
- [ ] `infra/mysql.tf`
- [ ] `infra/static_web_app.tf`
- [ ] `.github/workflows/cd-backend.yml`
- [ ] `.github/workflows/cd-frontend.yml`
- [ ] `Backend/InvTracker/appsettings.Production.json` (template, no secrets)
- [ ] `Frontend/.env.example`
- [ ] `plan/bootstrap.md` (step-by-step Azure setup guide)

### Modify
- [ ] `.github/workflows/backend-ci.yml` — ensure it gates the deploy
- [ ] `.github/workflows/frontend-ci.yml` — ensure it gates the deploy
- [ ] `Backend/InvTracker/Program.cs` — add `context.Database.Migrate()`

### Remove / ignore
- [ ] `Frontend/Dockerfile` — delete (not needed)
- [ ] `Backend/InvTracker/Dockerfile` — delete (not needed)

---

## 11. Implementation Order

1. **Bootstrap** — Azure free account signup, service principal, Terraform state storage
2. **Terraform** — write and apply `infra/` to provision App Service F1, MySQL, Static Web Apps, App Insights
3. **App code changes** — App Insights registration, `Database.Migrate()`, production appsettings
4. **GitHub Actions cd-backend.yml** — dotnet publish + zip deploy pipeline
5. **GitHub Actions cd-frontend.yml** — Static Web Apps deploy pipeline
6. **Update existing CI workflows** — wire quality gates into deploy pipeline
7. **Secrets** — populate all GitHub Actions secrets from Terraform outputs
8. **First deploy** — push to `main`, verify end-to-end
