# Milestone 0 — Project Bootstrap

> Get the C# backend solution created, structured, and running inside Docker with a working database connection and health check.

---

## Goal

By the end of this milestone the ASP.NET Core API is running inside Docker, connected to a SQLite database, and responding to a health check endpoint. No features yet — just a solid foundation to build on.

---

## Depends On

Nothing. This is the first milestone.

---

## Tasks

### Folder preparation
- [x] Create `src/` folder at the root of the project
- [x] Create `src/backend/` inside `src/`
- [x] Create `src/backend/API/` inside `src/backend/`
- [x] Create `src/backend/API/Controllers/` inside `src/backend/API/`
- [x] Create `src/backend/API/Hubs/` inside `src/backend/API/`
- [x] Create `src/backend/Application/` inside `src/backend/`
- [x] Create `src/backend/Application/DTOs/` inside `src/backend/Application/`
- [x] Create `src/backend/Application/Interfaces/` inside `src/backend/Application/`
- [x] Create `src/backend/Application/Services/` inside `src/backend/Application/`
- [x] Create `src/backend/Domain/` inside `src/backend/`
- [x] Create `src/backend/Domain/Models/` inside `src/backend/Domain/`
- [x] Create `src/backend/Infrastructure/` inside `src/backend/`
- [x] Create `src/backend/Infrastructure/Data/` inside `src/backend/Infrastructure/`
- [x] Create `src/backend/Infrastructure/Repositories/` inside `src/backend/Infrastructure/`
- [x] Create `src/backend/Infrastructure/Services/` inside `src/backend/Infrastructure/`
- [x] Create `src/backend/Infrastructure/Settings/` inside `src/backend/Infrastructure/`
- [x] Create `src/backend/Test/` inside `src/backend/`
- [x] Create `src/backend/Test/Services/` inside `src/backend/Test/`

### Solution setup
- [x] Create the solution file `backend.sln` inside `src/backend/`
- [x] Create the `API` ASP.NET Core Web API project inside `src/backend/`
- [x] Create the `Application` class library project inside `src/backend/`
- [x] Create the `Domain` class library project inside `src/backend/`
- [x] Create the `Infrastructure` class library project inside `src/backend/`
- [x] Create the `Test` project inside `src/backend/`
- [x] Add all 5 projects to `backend.sln`

### Project references
- [x] Add `Domain` reference to `Application`
- [x] Add `Application` reference to `API`
- [x] Add `Application` reference to `Infrastructure`
- [x] Add `Domain` reference to `Infrastructure`
- [x] Add `API` and `Infrastructure` reference to `Test`

### NuGet packages
- [x] Install `Microsoft.EntityFrameworkCore` in `src/backend/Infrastructure`
- [x] Install `Microsoft.EntityFrameworkCore.Sqlite` in `src/backend/Infrastructure`
- [x] Install `Microsoft.EntityFrameworkCore.Design` in `src/backend/Infrastructure`

### Database
- [x] Create `FilamentDbContext` class inside `src/backend/Infrastructure/Data/`
- [x] Add SQLite connection string to `appsettings.json` in `src/backend/API`
- [x] Register `FilamentDbContext` in `Program.cs`

### Health check
- [x] Create `HealthController` inside `src/backend/API/Controllers/`
- [x] Add `GET /health` endpoint that returns `{ status: "ok", app: "Spoolarr" }`

### Docker
- [ ] Write `Dockerfile.api` for the ASP.NET Core API
- [ ] Write `docker-compose.yml` with the API service and a persistent volume for SQLite
- [ ] Write `Caddyfile` for HTTPS reverse proxy
- [ ] Add Caddy service to `docker-compose.yml`
- [ ] Test `docker compose up --build` runs without errors

### Environment config
- [ ] Add `ASPNETCORE_ENVIRONMENT` to docker-compose
- [ ] Make sure SQLite database file is stored in the persistent volume `/data/spoolarr.db`
- [ ] Create `appsettings.Development.json` for local dev outside Docker
- [ ] Set different connection string in `appsettings.Development.json` pointing to a local file path

### Git
- [ ] Create `.gitignore` file at the root of the project
- [ ] Ignore `bin/`, `obj/`, `*.db`, `*.db-shm`, `*.db-wal`, `.env`, `appsettings.*.json` (except default)

### Manual verification
- [ ] Run the API locally with `dotnet run` outside Docker and confirm health check responds
- [ ] Run the API inside Docker with `docker compose up --build` and confirm health check responds
- [ ] Confirm the SQLite file is created at the correct path in both environments

---

## Definition of Done

- [ ] `dotnet build` passes with no errors
- [ ] `docker compose up --build` starts without errors
- [ ] `GET /health` returns `200 OK`
- [ ] SQLite file is created at `/data/spoolarr.db` on container start
- [ ] Caddy proxies HTTPS traffic to the API on the local network
- [ ] Health endpoint responds correctly in both local dev and Docker environments
