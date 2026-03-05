# LegalGuard Frontend (Angular 20)

Web UI for LegalGuard. Built with Angular 20, Tailwind CSS, and Lucide icons.

## Prerequisites

- [Node.js](https://nodejs.org/) 20+ (LTS)
- [npm](https://www.npmjs.com/) (comes with Node)

## 1. Clone the repository

From the repo root (parent of `front/`):

```bash
git clone https://github.com/diyorrf/BISP.git
cd BISP
```

Frontend code lives in the `front/` directory.

## 2. Install dependencies

From the project root (parent of `front/`):

```bash
cd front
npm install
```

## 3. Configure the API URL (optional)

The app talks to the backend API. By default it uses:

- **API:** `http://localhost:5041/api`
- **WebSocket:** `ws://localhost:5041/ws`

To change them, edit `src/environments/environment.ts` (and `environment.prod.ts` for production builds).

Ensure the backend is running on the same host/port (see `back/README.md`).

## 4. Run the development server

```bash
npm start
```

Or:

```bash
ng serve
```

Open **http://localhost:4200/** in your browser. The app will reload when you change source files.

## Build for production

```bash
ng build
```

Output is in `dist/`. Serve that folder with any static host or use the backend to proxy.

## Running with Docker

From the repo root:

```bash
docker compose up
```

Frontend is served on **http://localhost:4200**, backend on **http://localhost:5041**, and PostgreSQL on port **5432**. See the root `README.md` or `docker-compose.yml` for details.

## Project structure

- `src/app/` — Angular app (components, services, guards, models)
- `src/environments/` — API URL and env config
- `src/styles.css` — global and Tailwind styles
- `angular.json` — Angular CLI config
