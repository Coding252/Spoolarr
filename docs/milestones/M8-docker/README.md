# Milestone 8 — Docker & Deployment

> Containerize the API with Docker, add a Caddy HTTPS reverse proxy, and verify the full stack runs correctly inside Docker.

---

## Goal

By the end of this milestone the API and Caddy proxy are running inside Docker Compose. The API is reachable over HTTPS at `spoolarr.local` and the SQLite database persists across container restarts via a named volume.

---

## Depends On

- Milestone 0 — Project Bootstrap

---

## Tasks

### Docker
- [x] Write `Dockerfile.api` for the ASP.NET Core API
- [ ] Write `docker-compose.yml` with the API service and a persistent volume for SQLite
- [ ] Write `Caddyfile` for HTTPS reverse proxy
- [ ] Add Caddy service to `docker-compose.yml`

### Environment config
- [ ] Add `ASPNETCORE_ENVIRONMENT` to `docker-compose.yml`
- [ ] Make sure SQLite database file is stored in the persistent volume `/data/spoolarr.db`

### Manual verification
- [ ] Run `docker compose up --build` and confirm it starts without errors
- [ ] Confirm the SQLite file is created at `/data/spoolarr.db` on container start
- [ ] Confirm Caddy proxies HTTPS traffic to the API on the local network

---

## Definition of Done

- [ ] `docker compose up --build` starts without errors
- [ ] SQLite file is created at `/data/spoolarr.db` on container start
- [ ] Caddy proxies HTTPS traffic to the API on the local network
- [ ] API responds correctly in the Docker environment
