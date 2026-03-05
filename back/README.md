# LegalGuard Backend (ASP.NET 9)

API and business logic for LegalGuard. Uses .NET 9, Entity Framework Core, PostgreSQL, and JWT auth.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/) (e.g. 15 or 16) — local install or Docker
- (Optional) [Docker](https://www.docker.com/) — to run DB or full stack via `docker-compose` from repo root

## 1. Clone the repository

From the repo root (parent of `back/`):

```bash
git clone https://github.com/diyorrf/BISP.git
cd BISP
```

Backend code lives in the `back/` directory.

## 2. Install dependencies

Restore NuGet packages:

```bash
cd back
dotnet restore
```

## 3. Configure the database

Ensure PostgreSQL is running and create a database (if needed):

```sql
CREATE DATABASE "LegalGuard";
```

Set the connection string:

- **Option A — appsettings**  
  Edit `appsettings.json` and set `ConnectionStrings:DefaultConnection` (e.g. `Host=localhost;Port=5432;Database=LegalGuard;Username=postgres;Password=YOUR_PASSWORD`).

- **Option B — environment**  
  Set environment variable:
  ```bash
  export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=LegalGuard;Username=postgres;Password=YOUR_PASSWORD"
  ```

- **Option C — Docker**  
  From repo root run `docker compose up -d db` and use the connection string for the `db` service (see root README or `docker-compose.yml`).

## 4. Create and apply migrations

Create a new migration (only when you change the model):

```bash
dotnet ef migrations add YourMigrationName
```

Apply migrations to the database:

```bash
dotnet ef database update
```

If the `dotnet ef` command is not found, install the EF Core global tool:

```bash
dotnet tool install --global dotnet-ef
```

## 5. Run the backend

Development (watch mode):

```bash
dotnet watch run
```

One-off run:

```bash
dotnet run
```

By default the API listens on **http://localhost:5041**. Swagger UI: **http://localhost:5041** (when Swagger is enabled in development).

## Project structure

- `Controllers/` — API endpoints
- `Services/` — business logic (Auth, Document, Question, AI, Report, Parser)
- `Data/` — DbContext, entities, repositories
- `Models/` — DTOs, settings
- `Infrastructure/` — WebSocket handler, etc.
- `Migrations/` — EF Core migrations

## Environment variables

- `ConnectionStrings__DefaultConnection` — PostgreSQL connection string
- `ASPNETCORE_ENVIRONMENT` — Development / Production
- `JwtSettings:SecretKey` — JWT signing key (use a secret in production)
- `OpenAI:ApiKey` — OpenAI API key for AI features
