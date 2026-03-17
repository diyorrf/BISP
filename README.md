# LegalGuard (BISP)

Full-stack legal document analysis platform: ASP.NET 9 backend, Angular 20 frontend, PostgreSQL database, OpenAI integration.

**Repository:** [https://github.com/diyorrf/BISP](https://github.com/diyorrf/BISP)

## Quick start with Docker

Prerequisites: [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/).

### 1. Clone the repository

```bash
git clone https://github.com/diyorrf/BISP.git
cd BISP
```

### 2. Create the `.env` file

Copy the example and fill in your secrets:

```bash
cp .env.example .env
```

Open `.env` and set the required values:

```env
# PostgreSQL (used by both the db container and the backend connection string)
POSTGRES_PASSWORD=postgres

# JWT authentication
JWT_SECRET_KEY=your-secret-key-here

# OpenAI (REQUIRED — the app will not work without a valid key)
OPENAI_API_KEY=sk-your-openai-api-key-here
OPENAI_CHAT_MODEL=gpt-4.1-mini
OPENAI_EMBEDDING_MODEL=text-embedding-3-large

# Email (SMTP for sending verification codes)
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_SENDER=your-email@gmail.com
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-gmail-app-password
```

> **Important:** `OPENAI_API_KEY` and `EMAIL_PASSWORD` have no defaults — the app **will not start** properly without them.
>
> For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833) (not your regular password).

### 3. Build and run

```bash
docker compose up --build
```

### 4. Apply database migrations

On the first run, the database tables need to be created. Run this once:

```bash
docker compose exec backend dotnet ef database update
```

> If `dotnet-ef` is not available inside the container, apply migrations from your host machine (requires .NET 9 SDK):
>
> ```bash
> cd back
> dotnet ef database update -- --connection "Host=localhost;Port=5432;Database=LegalGuard;Username=postgres;Password=postgres"
> ```

### 5. Open the app

- **Frontend:** http://localhost:4200
- **Backend API:** http://localhost:5041 (also accessible through Nginx at http://localhost:4200/api)
- **PostgreSQL:** localhost:5432 (user `postgres`, password from `.env`)

## How configuration works

The backend reads settings from two sources (in order of priority):

1. **Environment variables** (highest priority) — set via `.env` → `docker-compose.yml`
2. **`appsettings.json`** (fallback) — used for local development without Docker

Environment variables use `__` (double underscore) to represent nested config sections:

| `.env` variable | Maps to `appsettings.json` path |
|---|---|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings.DefaultConnection` |
| `OpenAI__ApiKey` | `OpenAI.ApiKey` |
| `JwtSettings__SecretKey` | `JwtSettings.SecretKey` |
| `EmailSettings__Password` | `EmailSettings.Password` |

When running with Docker, **you only need the `.env` file** — `appsettings.json` is not copied into the container.

When running locally without Docker, the backend reads from `appsettings.json` directly.

## Running without Docker (local development)

### 1. Database (PostgreSQL)

Run PostgreSQL 16 locally or in a standalone container:

```bash
docker run -d --name pg -e POSTGRES_PASSWORD=0549 -e POSTGRES_DB=LegalGuard -p 5432:5432 postgres:16-alpine
```

### 2. Backend

```bash
cd back
cp appsettings.json.example appsettings.json   # fill in your secrets
dotnet restore
dotnet ef database update
dotnet watch run --urls "http://localhost:5041"
```

### 3. Frontend

```bash
cd front
npm install
npm start
```

App runs at http://localhost:4200 and proxies API calls to http://localhost:5041.

## Project layout

| Directory | Stack | Description |
|---|---|---|
| `back/` | ASP.NET 9 | Web API, EF Core, JWT, OpenAI |
| `front/` | Angular 20 | SPA, Tailwind CSS, Lucide icons |
| (root) | Docker | `docker-compose.yml` for db, backend, frontend |

## Architecture (Docker)

```
Browser → :4200 → Nginx (frontend container)
                    ├── static files (Angular SPA)
                    ├── /api/* → proxy → backend:5000
                    └── /ws/*  → proxy → backend:5000 (WebSocket)
                                           └── db:5432 (PostgreSQL)
```

All three services (database, backend, frontend) run in separate containers. Nginx serves the Angular app and reverse-proxies API and WebSocket requests to the backend.
