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

### Solution setup
- [ ] Create the solution file `Spoolarr.sln`
- [ ] Create the `API` ASP.NET Core Web API project inside `src/`
- [ ] Create the `Application` class library project inside `src/`
- [ ] Create the `Domain` class library project inside `src/`
- [ ] Create the `Infrastructure` class library project inside `src/`
- [ ] Create the `Test` project inside `src/`
- [ ] Add all 5 projects to `Spoolarr.sln`

### Project references
- [ ] Add `Domain` reference to `Application`
- [ ] Add `Application` reference to `API`
- [ ] Add `Application` reference to `Infrastructure`
- [ ] Add `Domain` reference to `Infrastructure`
- [ ] Add `API` and `Infrastructure` reference to `Test`

### NuGet packages
- [ ] Install `Microsoft.EntityFrameworkCore` in `Infrastructure`
- [ ] Install `Microsoft.EntityFrameworkCore.Sqlite` in `Infrastructure`
- [ ] Install `Microsoft.EntityFrameworkCore.Design` in `Infrastructure`

### Project structure
- [ ] Create `Controllers/` folder in `API`
- [ ] Create `Hubs/` folder in `API`
- [ ] Create `DTOs/` folder in `Application`
- [ ] Create `Interfaces/` folder in `Application`
- [ ] Create `Services/` folder in `Application`
- [ ] Create `Models/` folder in `Domain`
- [ ] Create `Data/` folder in `Infrastructure`
- [ ] Create `Repositories/` folder in `Infrastructure`
- [ ] Create `Services/` folder in `Infrastructure`
- [ ] Create `Settings/` folder in `Infrastructure`

### Database
- [ ] Create `FilamentDbContext` class inside `Infrastructure/Data/`
- [ ] Add SQLite connection string to `appsettings.json` in `API`
- [ ] Register `FilamentDbContext` in `Program.cs`

### Health check
- [ ] Create `HealthController` inside `API/Controllers/`
- [ ] Add `GET /health` endpoint that returns `{ status: "ok", app: "Spoolarr" }`

### Docker
- [ ] Create `docker/` folder at the root of the project
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
