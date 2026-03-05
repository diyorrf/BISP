# LegalGuard (BISP)

Full-stack app: ASP.NET 9 backend, Angular 20 frontend, PostgreSQL database.

**Repository:** [https://github.com/diyorrf/BISP](https://github.com/diyorrf/BISP)

## Quick start with Docker

Prerequisites: [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/).

```bash
git clone https://github.com/diyorrf/BISP.git
cd BISP
docker compose up --build
```

- **Frontend:** http://localhost:4200  
- **Backend API:** http://localhost:5041  
- **Swagger:** http://localhost:5041 (when enabled)  
- **PostgreSQL:** localhost:5432 (user `postgres`, password `postgres`, database `LegalGuard`)

Apply database migrations once before the first run (see "Applying migrations when using Docker" below).

## Running without Docker

### 1. Database (PostgreSQL)

Run PostgreSQL 15/16 locally or in a container:

```bash
docker run -d --name pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=LegalGuard -p 5432:5432 postgres:16-alpine
```

Create the database if needed: `CREATE DATABASE "LegalGuard";`

### 2. Backend

See **[back/README.md](back/README.md)** for:

- .NET 9 SDK, restore, appsettings connection string
- Create and apply migrations: `dotnet ef migrations add ...` then `dotnet ef database update`
- Run: `dotnet watch run` (listens on http://localhost:5041)

### 3. Frontend

See **[front/README.md](front/README.md)** for:

- Node 20+, `npm install`, `npm start`
- App runs at http://localhost:4200 and uses API at http://localhost:5041/api

## Project layout

| Directory | Stack        | Description                |
|-----------|--------------|----------------------------|
| `back/`   | ASP.NET 9   | Web API, EF Core, JWT, OpenAI |
| `front/`  | Angular 20 | SPA, Tailwind, Lucide icons   |
| (root)   | Docker      | `docker-compose.yml` for db, back, front |

## Applying migrations when using Docker

If the backend container does not run migrations automatically, run them once after the first `docker compose up`:

```bash
docker compose run --rm backend dotnet ef database update
```

(Ensure the `backend` service has the same `ConnectionStrings__DefaultConnection` so it can reach the `db` container.)
