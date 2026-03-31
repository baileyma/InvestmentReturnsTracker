# Investment Returns Tracker

A full-stack web application for tracking investment account performance over time. Calculates XIRR (annualised return) for individual accounts and across an aggregate portfolio, accounting for deposits and withdrawals.

## Tech Stack

**Backend** - ASP.NET Core Web API (.NET 8, C#)
- MySQL database via Entity Framework Core
- ASP.NET Identity for user authentication (JWT bearer tokens)
- In-memory caching for computed returns
- XIRR calculation via `Excel.FinancialFunctions` library

**Frontend** - React 18 (Vite)
- React Router for client-side routing
- Axios for API calls with JWT interceptor
- SCSS styling
- reactjs-popup for modals

## Project Structure

```
Backend/
  InvTracker/           # Main API project
    Controllers/        # Auth, Accounts, Returns, Update endpoints
    Services/           # Business logic (account details, aggregate calculations)
    Repositories/       # Data access layer (EF Core)
    Models/             # Domain models and DTOs
    Utils/              # XIRR calculator
    Authentication/     # Identity user model and auth DTOs
    DbContexts/         # EF Core DbContext
    Migrations/         # EF Core migrations
  InvTracker.UnitTests/ # Unit tests (xUnit, NSubstitute, AutoFixture)
  Benchmarks/           # BenchmarkDotNet project (WIP)

Frontend/
  src/
    Components/
      Home/             # Dashboard - all accounts, yearly returns grid
      AccountDetails/   # Single account view - balances, payments, XIRR
      Login/            # Login form
      Register/         # Registration form
      Header/           # Navigation header
    context/            # Auth context (JWT token management)
    utils/              # Axios instance with auth interceptor
```

## Features

- **User authentication** - Register/login with JWT tokens
- **Home dashboard** - Grid view of all accounts showing yearly balances, XIRR returns, cumulative performance, aggregate totals, deposit/withdrawal breakdowns
- **Account detail view** - Per-account, per-year breakdown with balance history, payment list, and XIRR calculation
- **Data entry** - Add/delete payments (deposits and withdrawals), update balances via popup forms
- **Caching** - Server-side in-memory cache for computed returns

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js
- MySQL 8.0+

### Backend
1. Update `appsettings.Development.json` with your MySQL connection string and JWT secret
2. Run EF Core migrations: `dotnet ef database update`
3. Start the API: `dotnet run`

### Frontend
1. Create a `.env` file in `Frontend/`:
   ```
   VITE_API_URL=https://localhost:7019/api
   ```
2. Install dependencies: `npm install`
3. Start the dev server: `npm run dev`

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/Auth/Register` | Register a new user |
| POST | `/api/Auth/Login` | Login, returns JWT |
| GET | `/api/Accounts` | Get user's accounts |
| GET | `/api/Returns/accountreturns` | Home page aggregate data |
| GET | `/api/Returns/individualaccountdata/{id}/year/{year}` | Account detail by year |
| POST | `/api/Update/add-payment` | Add a payment |
| PUT | `/api/Update/update-balance` | Update a balance |
| DELETE | `/api/Update/delete-payment/{id}` | Delete a payment |
