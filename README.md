# Restaurant Management System

## Project Summary

This repository contains a full-stack restaurant management system built with a Next.js frontend and an ASP.NET Core backend. The current implementation covers JWT-based authentication, a dashboard shell, customer data retrieval from secured API endpoints, and backend modules for common restaurant operations such as menu items, tables, orders, invoices, payments, inventory, expenses, reports, roles, users, and settings.

The frontend focuses on an admin-facing dashboard experience. The login screen authenticates against the backend, stores the bearer token locally, and routes authenticated users into the dashboard. The customers page is already connected to the backend and loads live data from `/api/customers`, while analytics and messages are scaffolded as extendable dashboard sections.

On the backend, the solution is organized into layered projects for API, application contracts, domain entities, repositories, and services. It uses SQL Server, ASP.NET Core Identity, JWT authentication, Entity Framework Core, and Dapper. On startup, the API initializes the database and seeds a default admin user, roles, and permissions.

## Highlights

- Next.js 16 frontend with App Router and TypeScript
- ASP.NET Core Web API targeting .NET 10
- JWT authentication with seeded admin credentials
- SQL Server persistence with EF Core and Dapper
- Layered backend structure for domain, application, repositories, and services
- Swagger enabled in development
- Dashboard UI with authenticated customer data loading

## Tech Stack

- Frontend: Next.js, React, TypeScript, Tailwind CSS
- Backend: ASP.NET Core, ASP.NET Identity, JWT Bearer Auth
- Data: SQL Server, Entity Framework Core, Dapper

## Current Feature Scope

- Authentication and token-based session handling
- Dashboard shell with protected routes
- Live customer listing from the backend API
- Backend CRUD and reporting modules for:
  - Food categories
  - Menu items
  - Tables
  - Customers
  - Orders
  - Invoices
  - Payments
  - Inventory
  - Expenses
  - Reports
  - Roles and permissions
  - Users
  - Settings

## Repository Structure

```text
backend/
  src/
    Backend.Api/
    Backend.Application/
    Backend.Domain/
    Backend.Repositories/
    Backend.Services/

frontend/
  src/
    app/
    components/
    lib/
```

## Local Setup

### Prerequisites

- Node.js 20+
- npm
- .NET 10 SDK
- SQL Server instance

### Backend

1. Update the SQL Server connection string in `backend/src/Backend.Api/appsettings.json`.
2. Start the API:

```bash
dotnet run --project backend/src/Backend.Api
```

The development profile runs on `http://localhost:5179`, with Swagger available at `http://localhost:5179/swagger`.

### Frontend

1. Install dependencies:

```bash
cd frontend
npm install
```

2. Create the local environment file from the example:

```bash
copy .env.example .env.local
```

3. Start the frontend:

```bash
npm run dev
```

The frontend expects the backend API at `http://localhost:5179/api` by default.

## Default Login

- Username: `admin`
- Password: `Admin@12345`

These credentials are seeded automatically by the backend during startup.

## API Notes

- Root endpoint: `GET /`
- Login endpoint: `POST /api/auth/login`
- Example secured endpoint used by the frontend: `GET /api/customers`

## Status

This project already has the backend domain and API surface for a restaurant management platform, plus an operational frontend authentication flow and dashboard shell. Parts of the frontend are still scaffolded and ready for deeper integration with backend summary, analytics, messaging, and operational modules.
