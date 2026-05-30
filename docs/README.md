# Spoolarr — Roadmap

Full development roadmap for the Spoolarr project. Each milestone links to its own detailed README with code, setup instructions, and definition of done.

---

## Progress Overview

| Milestone | Description | Status |
|---|---|---|
| [M0 — Project Bootstrap](#milestone-0--project-bootstrap) | Solution, EF Core, database connection | ✅ Done |
| [M1 — Data Model](#milestone-1--data-model) | Entities, migrations, repositories, seed data | ✅ Done |
| [M2 — Spool API](#milestone-2--spool-api) | REST endpoints for spool management | ✅ Done |
| [M3 — NFC Scan Flow](#milestone-3--nfc-scan-flow) | Scan endpoint, NfcScanService, SignalR | ✅ Done |
| [M4 — Bambu MQTT](#milestone-4--bambu-mqtt) | MQTT listener, print-finish, gram deduction | ✅ Done |
| [M5 — Web UI](#milestone-5--web-ui) | Dashboard, scan page, Web NFC, QR fallback | ⬜ Not started |
| [M6 — Alerts](#milestone-6--alerts) | Low stock threshold, ntfy / webhook | ⬜ Not started |
| [M7 — AMS Support](#milestone-7--ams-support) | Multi-slot mapping, AMS MQTT, slot UI | ⬜ Not started |
| [M8 — Docker & Deployment](#milestone-8--docker--deployment) | Dockerfile, Docker Compose, Caddy HTTPS, container verification | ⬜ Not started |

---

## Milestone 0 — Project Bootstrap

> Set up the solution structure, Docker skeleton, HTTPS proxy, and database connection.

📄 [Full milestone README](milestones/M0%20-%20Project%20Bootstrap/README.md)

**Depends on:** Nothing — start here.

### Tasks

- [x] Create solution `backend.sln` with `API`, `Application`, `Domain`, `Infrastructure`, and `Test` projects
- [x] Set up EF Core + SQLite + `FilamentDbContext`
- [x] Configure environment and connection string in `appsettings.json`
- [x] Auto-migration and error handling on startup

---

## Milestone 1 — Data Model

> Define the database entities, run EF Core migrations, set up repositories, and seed test data.

📄 [Full milestone README](milestones/M1-data-model/README.md)

**Depends on:** M0

### Tasks

- [x] `Spool`, `Printer`, `PrintJob`, `NfcTag` entity classes
- [x] EF Core migrations — `InitialCreate` applied, all 4 tables created
- [x] `ISpoolRepository`, `IPrinterRepository`, `IPrintJobRepository`, `INfcTagRepository` + implementations
- [x] All 4 repositories registered in DI
- [x] `SeedData` — 2 spools, 1 NFC tag, 1 printer on first run
- [x] Auto-migration and error handling on startup

---

## Milestone 2 — Spool API

> Build the REST API endpoints for managing spools — list, get, register, activate, and update weight.

📄 [Full milestone README](milestones/M2-spool-api/README.md)

**Depends on:** M0, M1

### Tasks

- [x] `GET /api/spools` — list all spools
- [x] `GET /api/spools/{id}` — get single spool
- [x] `POST /api/spools` — register new spool
- [x] `PATCH /api/spools/{id}/activate` — set active spool
- [x] `PUT /api/spools/{id}` — update spool fields
- [x] `DELETE /api/spools/{id}` — delete spool

---

## Milestone 3 — NFC Scan Flow

> Handle the NFC tag scan event — look up the tag UID, route to register or activate, and push the result to the browser in real time via SignalR.

📄 [Full milestone README](milestones/M3-nfc-scan/README.md)

**Depends on:** M0, M1, M2

### Tasks

- [x] `POST /api/nfc-tags/scan` — receives tag UID, activates spool or returns "unknown"
- [x] `NfcScanService` — lookup by UID, return spool or "unknown tag"
- [x] `NfcScanHub` SignalR — push scan result to browser in real time
- [x] `GET /api/nfc-tags` — list all NFC tags
- [x] `GET /api/nfc-tags/{id}` — get NFC tag by ID
- [x] `POST /api/nfc-tags` — register new NFC tag (link UID to spool)
- [x] `DELETE /api/nfc-tags/{id}` — delete NFC tag

---

## Milestone 4 — Bambu MQTT

> Connect to the Bambu Lab printer on the local network via MQTT, listen for print-finish events, extract grams used, and automatically deduct from the active spool.

📄 [Full milestone README](milestones/M4-bambu-mqtt/README.md)

**Depends on:** M0, M1, M2

### Tasks

- [x] Install `MQTTnet` v5 in `Infrastructure`
- [x] Add `CloudEmail` + `CloudPassword` + `Port` to `Printer` entity + migration
- [x] Create `BambuMqttService` in `Infrastructure/Services/` as `IHostedService`
- [x] Connect via LAN (`bambu_lan`) or Cloud (`bambu_cloud`) — protocol driven by `Printer.Protocol`
- [x] Cloud password stored encrypted via ASP.NET Core Data Protection
- [x] Subscribe to `device/{serial}/report` topic
- [x] Parse `print.gcode_state == "FINISH"` and extract `print.filament_weight`
- [x] Deduct grams from active spool, floor at 0, save via `ISpoolRepository`
- [x] Log `PrintJob` to DB via `IPrintJobRepository`
- [x] Retry connection every 30 seconds if offline, no printer configured, or connection drops
- [x] Register `BambuMqttService` as hosted service in `Program.cs`
- [x] `PrinterStatus` DTO — state, progress, temps, layers
- [x] `IPrinterStatusService` + `PrinterStatusService` — in-memory live status
- [x] `PrinterHub` SignalR hub mapped to `/hubs/printer`
- [x] Push `PrinterStatus` on every MQTT message via SignalR
- [x] `GET /api/printers/status` — returns current status or `204`

---

## Milestone 5 — Web UI

> Build the browser-based dashboard — spool list, spool detail, NFC scan page, register form, and active spool indicator.

📄 [Full milestone README](milestones/M5-web-ui/README.md)

**Depends on:** M2, M3

### Tasks

- [ ] Spool list dashboard
- [ ] Spool detail page
- [ ] NFC scan page (Web NFC + QR fallback for iOS)
- [ ] Register spool form
- [ ] Active spool indicator

---

## Milestone 6 — Alerts

> Trigger a push notification when a spool's remaining weight drops below its threshold after a print.

📄 [Full milestone README](milestones/M6-alerts/README.md)

**Depends on:** M4

### Tasks

- [ ] Threshold check after every gram deduction
- [ ] Webhook / ntfy push notification

---

## Milestone 7 — AMS Support

> Extend Spoolarr to support the Bambu Lab AMS — track up to 4 filament slots simultaneously and map each MQTT print event to the correct spool.

📄 [Full milestone README](milestones/M7-ams/README.md)

**Depends on:** M1, M2, M4

### Tasks

- [ ] Slot-to-spool mapping model
- [ ] Multi-slot MQTT parsing
- [ ] UI slot management

---

## Milestone 8 — Docker & Deployment

> Containerize the API with Docker, add a Caddy HTTPS reverse proxy, and verify the full stack runs correctly inside Docker.

📄 [Full milestone README](milestones/M8-docker/README.md)

**Depends on:** M0

### Tasks

- [ ] Write `docker-compose.yml` with API service and SQLite volume
- [ ] Write `Caddyfile` for HTTPS reverse proxy
- [ ] Add Caddy service to `docker-compose.yml`
- [ ] Configure environment variables in `docker-compose.yml`
- [ ] Verify `docker compose up --build` runs without errors

---

## Dependency Graph

```
M0 — Bootstrap
 ├── M1 — Data Model
 │    └── M2 — Spool API
 │         ├── M3 — NFC Scan Flow
 │         │    └── M5 — Web UI
 │         └── M4 — Bambu MQTT
 │              ├── M6 — Alerts
 │              └── M7 — AMS Support
 └── M8 — Docker & Deployment
```

---

*Update the status column above as milestones are completed. Change `⬜ Not started` → `🔄 In progress` → `✅ Done`.*
