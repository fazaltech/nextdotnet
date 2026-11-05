# Restaurant Management System

Next.js 16 frontend and .NET 10 API with SQL Server, Dapper, ASP.NET Identity, and SignalR.

The application supports dining sessions and shared seats, manager approval, item-level kitchen and delivery states, recipe inventory, purchasing and partial receipts, split bills, payments/refunds, reporting, and permission-controlled administration.

See [the implementation and operations guide](docs/restaurant-service.md) for the inspected schema, additive database changes, connected UI/API routes, accounting rules, verification coverage, and remaining legacy/branch limitations.

## Run locally

Use .NET 10, SQL Server, and Node 22.13+.

Configure `ConnectionStrings__DefaultConnection` and `Jwt__Key` securely through the environment or local development settings. On a new database, set `Bootstrap__AdminPassword` to create the initial `admin` account; existing account passwords are preserved.

```powershell
dotnet restore backend/Backend.sln
dotnet run --project backend/src/Backend.Api
```

Startup applies the existing guarded schema initializer and the new additive service script. No EF migration command is needed. Development API: `http://localhost:5179`, Swagger: `/swagger`.

```powershell
cd frontend
npm ci
npm run dev
```

The frontend uses `NEXT_PUBLIC_API_BASE_URL`, defaulting to `http://localhost:5179/api`. Sign in with your actual restaurant account. Owners manage users and passwords at `/dashboard/settings`.

## Verify

```powershell
dotnet build backend/Backend.sln --no-restore
powershell -NoProfile -ExecutionPolicy Bypass -File backend/tests/verify-service.ps1
cd frontend
npx tsc --noEmit
npm run lint
npm run build
```

The integration harness creates and retains a uniquely named isolated verification database. It does not write fixtures into the restaurant database. It exercises transactional stock/billing, permissions, password resets, reports, and a SignalR WebSocket notification. See the operations guide for details and limits.
