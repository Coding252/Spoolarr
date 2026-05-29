# Spoolarr

> Self-hosted filament spool tracker for 3D printing — NFC-powered, Docker-deployed, no cloud required.

---

## What is Spoolarr?

Spoolarr is a self-hosted web app that tracks your 3D printing filament spools. Tap your phone to a spool's NFC tag and the system instantly knows which filament is loaded, how many grams remain, and logs every gram used after each print — automatically pulled from your Bambu Lab printer via local MQTT.

No subscriptions. No cloud. No vendor lock-in. Runs entirely on your local network inside Docker.

---

## Features

- **NFC spool identity** — each spool gets an NFC tag (open standard or your own NTAG215 sticker). Tap to activate.
- **Two-route scan flow** — if the spool already has a tag, read it. If not, attach a sticker and register manually.
- **Bambu Lab integration** — listens to local MQTT for print-finish events and deducts grams automatically.
- **Gram tracking** — tracks remaining filament per spool from first load to empty.
- **Web UI** — mobile-friendly dashboard accessible on your local network. Scan page uses Web NFC API on Android; QR code fallback for iOS.
- **Self-hosted Docker stack** — single `docker-compose up` to run everything.

---

## Tech Stack

| Layer | Project | Technology |
|---|---|---|
| Entry point | `API` | C# · ASP.NET Core · .NET 10 |
| Business logic | `Application` | C# · interfaces · DTOs |
| Domain models | `Domain` | Pure C# — no dependencies |
| Data & external | `Infrastructure` | EF Core · MQTTnet · SQLite |
| Real-time | `API` | SignalR |
| Frontend | — | React or plain HTML/JS |
| Reverse proxy | — | Caddy (HTTPS for Web NFC) |
| Container | — | Docker · docker-compose |

---

## NFC Tag Support

| Tag type | Read | Write | Notes |
|---|---|---|---|
| OpenPrintTag (Prusament) | Yes | Yes | Auto-fills all fields including remaining grams |
| OpenTag3D | Yes | Yes | Open standard, growing brand support |
| Own NTAG215 sticker | Yes | Yes | Blank sticker attached by user, full manual registration |

---

## Scan Flow

![Spoolarr NFC Scan Flow](docs/spoolarr-nfc-scan-flow.png)

> Full interactive version — open [`docs/spoolarr-nfc-scan-flow.html`](docs/spoolarr-nfc-scan-flow.html) in any browser.

---

## Project Structure

Spoolarr follows a **Clean Architecture** pattern split across 5 projects.

```
Spoolarr/
├── src/
│   └── backend/
│       ├── API/                          # Entry point — controllers, hubs, Program.cs
│       │   ├── Controllers/
│       │   │   ├── HealthController.cs
│       │   │   ├── SpoolController.cs
│       │   │   ├── ScanController.cs
│       │   │   └── AmsController.cs
│       │   ├── Hubs/
│       │   │   └── NfcScanHub.cs
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   └── Program.cs
│       │
│       ├── Application/                  # Business logic — services, interfaces, DTOs
│       │   ├── DTOs/
│       │   │   └── SpoolDto.cs
│       │   ├── Interfaces/
│       │   │   ├── ISpoolService.cs
│       │   │   └── INfcScanService.cs
│       │   └── Services/
│       │       ├── SpoolService.cs
│       │       ├── NfcScanService.cs
│       │       └── AlertService.cs
│       │
│       ├── Domain/                       # Pure C# models — no dependencies
│       │   └── Models/
│       │       ├── Spool.cs
│       │       ├── PrintJob.cs
│       │       ├── AmsSlot.cs
│       │       └── NfcScanResult.cs
│       │
│       ├── Infrastructure/               # EF Core, repositories, MQTT, settings
│       │   ├── Data/
│       │   │   ├── FilamentDbContext.cs
│       │   │   └── SeedData.cs
│       │   ├── Repositories/
│       │   │   ├── ISpoolRepository.cs
│       │   │   ├── SpoolRepository.cs
│       │   │   ├── IPrintJobRepository.cs
│       │   │   ├── PrintJobRepository.cs
│       │   │   ├── IAmsSlotRepository.cs
│       │   │   └── AmsSlotRepository.cs
│       │   ├── Services/
│       │   │   └── MqttListenerService.cs
│       │   └── Settings/
│       │       ├── BambuMqttSettings.cs
│       │       └── AlertSettings.cs
│       │
│       ├── Test/                         # Unit tests
│       │   └── Services/
│       │
│       └── backend.sln
│
├── docker/
│   ├── docker-compose.yml
│   ├── Dockerfile.api
│   └── Caddyfile
├── docs/
│   ├── ROADMAP.md
│   ├── spoolarr-nfc-scan-flow.html
│   └── milestones/
│       ├── M0-bootstrap/
│       ├── M1-data-model/
│       ├── M2-spool-api/
│       ├── M3-nfc-scan/
│       ├── M4-bambu-mqtt/
│       ├── M5-web-ui/
│       ├── M6-alerts/
│       └── M7-ams/
├── .gitignore
└── README.md
```

### Dependency direction

```
API → Application → Domain
Infrastructure → Application → Domain
```

| Project | Responsibility |
|---|---|
| `API` | HTTP entry point — controllers, SignalR hubs, middleware, `Program.cs` |
| `Application` | Business logic — services, interfaces, DTOs |
| `Domain` | Pure C# models and entities — no dependencies on anything |
| `Infrastructure` | EF Core, repositories, MQTT listener, settings |
| `Test` | Unit tests for the Application layer |

---

## Getting Started

### Prerequisites

- Docker + Docker Compose
- A Bambu Lab printer on your local network
- Android phone with Chrome (for Web NFC) or any phone for QR fallback
- NFC stickers — NTAG215 recommended (or Prusament OpenPrintTag spools)

### Run

```bash
git clone https://github.com/yourname/spoolarr
cd spoolarr
cp docker/.env.example docker/.env
# edit .env — set your printer IP and MQTT credentials
docker compose -f docker/docker-compose.yml up -d
```

Then open `https://spoolarr.local` (or your Docker host IP) in your browser.

### Environment variables

| Variable | Description | Example |
|---|---|---|
| `BAMBU_PRINTER_IP` | Local IP of your Bambu printer | `192.168.1.50` |
| `BAMBU_MQTT_PORT` | MQTT port | `8883` |
| `BAMBU_SERIAL` | Printer serial number | `01S00C123456789` |
| `BAMBU_ACCESS_CODE` | LAN access code from printer screen | `12345678` |
| `DB_PATH` | SQLite file path | `/data/spoolarr.db` |

---

## Roadmap

Each milestone has its own detailed README with tasks and definition of done.

| Milestone | Description | Status |
|---|---|---|
| [M0 — Project Bootstrap](docs/milestones/M0%20-%20Project%20Bootstrap/README.md) | Solution setup, EF Core, health endpoint | ✅ Done |
| [M1 — Data Model](docs/milestones/M1-data-model/README.md) | `Spool`, `Printer`, `PrintJob`, `NfcTag` entities, migrations, repositories, seed data | 🔄 In progress |
| [M2 — Spool API](docs/milestones/M2-spool-api/README.md) | REST endpoints for spool management | ⬜ Not started |
| [M3 — NFC Scan Flow](docs/milestones/M3-nfc-scan/README.md) | Scan endpoint, `NfcScanService`, SignalR real-time push | ⬜ Not started |
| [M4 — Bambu MQTT](docs/milestones/M4-bambu-mqtt/README.md) | MQTT listener, print-finish event, auto gram deduction | ⬜ Not started |
| [M5 — Web UI](docs/milestones/M5-web-ui/README.md) | Dashboard, scan page, Web NFC, QR fallback, register form | ⬜ Not started |
| [M6 — Alerts](docs/milestones/M6-alerts/README.md) | Low stock threshold check, ntfy / webhook notifications | ⬜ Not started |
| [M7 — AMS Support](docs/milestones/M7-ams/README.md) | Multi-slot mapping, AMS MQTT parsing, slot UI | ⬜ Not started |
| [M8 — Docker & Deployment](docs/milestones/M8-docker/README.md) | Dockerfile, Docker Compose, Caddy HTTPS proxy, container verification | ⬜ Not started |

### Task checklist

<details>
<summary>Milestone 0 — Project Bootstrap</summary>

#### Solution setup
- [ ] Create the solution file `Spoolarr.sln`
- [ ] Create the `API` ASP.NET Core Web API project inside `src/`
- [ ] Create the `Application` class library project inside `src/`
- [ ] Create the `Domain` class library project inside `src/`
- [ ] Create the `Infrastructure` class library project inside `src/`
- [ ] Create the `Test` project inside `src/`
- [ ] Add all 5 projects to `Spoolarr.sln`

#### NuGet packages
- [ ] Install `Microsoft.EntityFrameworkCore` in `Infrastructure`
- [ ] Install `Microsoft.EntityFrameworkCore.Sqlite` in `Infrastructure`
- [ ] Install `Microsoft.EntityFrameworkCore.Design` in `Infrastructure`

#### Project structure
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

#### Database
- [ ] Create `FilamentDbContext` class inside `Infrastructure/Data/`
- [ ] Add SQLite connection string to `appsettings.json`
- [ ] Register `FilamentDbContext` in `Program.cs`

#### Health check
- [ ] Create `HealthController` inside `API/Controllers/`
- [ ] Add `GET /health` endpoint that returns `{ status: "ok", app: "Spoolarr" }`

#### Docker
- [ ] Create `docker/` folder at the root of the project
- [ ] Write `Dockerfile.api` for the API project
- [ ] Write `docker-compose.yml` with the API service and a persistent volume for SQLite
- [ ] Write `Caddyfile` for HTTPS reverse proxy
- [ ] Add Caddy service to `docker-compose.yml`

#### Environment config
- [ ] Create `appsettings.Development.json` for local dev
- [ ] Add `ASPNETCORE_ENVIRONMENT` to docker-compose
- [ ] Make sure SQLite file is stored in the persistent volume `/data/spoolarr.db`

#### Git
- [ ] Create `.gitignore` at the root of the project

</details>

<details>
<summary>Milestone 1 — Data Model</summary>

#### Models
- [x] Create `Spool` entity class inside `Domain/Models/`
- [x] Create `Printer` entity class inside `Domain/Models/`
- [x] Create `PrintJob` entity class inside `Domain/Models/`
- [x] Create `NfcTag` entity class inside `Domain/Models/`

#### Spool fields
- [x] `Id` — Guid, primary key
- [x] `Brand` — string
- [x] `Material` — string
- [x] `ColorName` — string
- [x] `ColorHex` — string
- [x] `InitialWeightG` — float
- [x] `CurrentWeightG` — float
- [x] `SpoolWeightG` — float, default 200
- [x] `DiameterMm` — float, default 1.75
- [x] `LowStockThresholdG` — float, default 100
- [x] `IsActive` — bool, default false
- [x] `IsArchived` — bool, default false
- [x] `CreatedAt` — DateTime
- [x] `LastScannedAt` — DateTime, nullable
- [x] `Notes` — string, nullable
- [x] `NfcTags` — navigation property to `NfcTag`
- [x] `PrintJobs` — navigation property to `PrintJob`

#### Printer fields
- [x] `Id` — Guid, primary key
- [x] `Name` — string
- [x] `Brand` — string
- [x] `Model` — string
- [x] `SerialNumber` — string, nullable
- [x] `IpAddress` — string
- [x] `Protocol` — string
- [x] `AccessCode` — string, nullable
- [x] `Port` — int, nullable
- [x] `HasAms` — bool, default false
- [x] `AmsSlotCount` — int, default 0
- [x] `IsActive` — bool, default true
- [x] `LastSeenAt` — DateTime, nullable
- [x] `CreatedAt` — DateTime
- [x] `Notes` — string, nullable
- [x] `PrintJobs` — navigation property to `PrintJob`

#### PrintJob fields
- [x] `Id` — Guid, primary key
- [x] `PrinterId` — Guid, foreign key to `Printer`
- [x] `SpoolId` — Guid, foreign key to `Spool`
- [x] `PrintFileName` — string, nullable
- [x] `Status` — string
- [x] `GramsUsed` — float, default 0
- [x] `LastReportedGramsUsed` — float, default 0
- [x] `StartedAt` — DateTime
- [x] `FinishedAt` — DateTime, nullable
- [x] `LastUpdatedAt` — DateTime
- [x] `Source` — string, default `"mqtt"`
- [x] `Notes` — string, nullable
- [x] `Printer` — navigation property to `Printer`
- [x] `Spool` — navigation property to `Spool`

#### NfcTag fields
- [x] `Id` — Guid, primary key
- [x] `TagUid` — string, unique
- [x] `Type` — string
- [x] `SpoolId` — Guid, foreign key to `Spool`
- [x] `CreatedAt` — DateTime
- [x] `Spool` — navigation property to `Spool`

#### Database context
- [x] Add `DbSet<Spool>` to `FilamentDbContext`
- [x] Add `DbSet<Printer>` to `FilamentDbContext`
- [x] Add `DbSet<PrintJob>` to `FilamentDbContext`
- [x] Add `DbSet<NfcTag>` to `FilamentDbContext`
- [x] Configure unique index on `NfcTag.TagUid`
- [x] Configure cascade delete — spool deletes NFC tags and print jobs
- [x] Configure cascade delete — printer deletes print jobs
- [x] Configure foreign key relationships

#### Migrations
- [x] Run `dotnet ef migrations add InitialCreate`
- [x] Run `dotnet ef database update`
- [x] Confirm `Spools`, `Printers`, `PrintJobs`, and `NfcTags` tables are created

#### Spool repository
- [ ] Create `ISpoolRepository` and `SpoolRepository` in `Infrastructure/Repositories/`
- [ ] Add `GetAllAsync`, `GetByIdAsync`, `GetActiveAsync`, `CreateAsync`, `UpdateAsync`, `ArchiveAsync`, `DeleteAsync`

#### Printer repository
- [ ] Create `IPrinterRepository` and `PrinterRepository` in `Infrastructure/Repositories/`
- [ ] Add `GetAllAsync`, `GetByIdAsync`, `GetActiveAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`

#### PrintJob repository
- [ ] Create `IPrintJobRepository` and `PrintJobRepository` in `Infrastructure/Repositories/`
- [ ] Add `GetBySpoolIdAsync`, `GetByPrinterIdAsync`, `GetRunningAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`

#### NfcTag repository
- [ ] Create `INfcTagRepository` and `NfcTagRepository` in `Infrastructure/Repositories/`
- [ ] Add `GetByTagUidAsync`, `GetBySpoolIdAsync`, `CreateAsync`, `DeleteAsync`

#### Dependency injection
- [ ] Register all 4 repositories in `Program.cs`

#### Seed data
- [ ] Create `SeedData` class inside `Infrastructure/Data/`
- [ ] Add 2 test spools, 1 NFC tag, 1 printer
- [ ] Call on startup only if database is empty
- [ ] Run migrations automatically on startup

#### Error handling
- [ ] Wrap migration in try/catch on startup
- [ ] Log error if database file cannot be created or accessed

</details>

<details>
<summary>Milestone 2 — Spool API</summary>

#### DTOs
- [ ] Create `SpoolResponse` record in `Application/DTOs/`
- [ ] Create `RegisterSpoolRequest` record in `Application/DTOs/`
- [ ] Create `UpdateWeightRequest` record in `Application/DTOs/`

#### SpoolService
- [ ] Create `ISpoolService` interface in `Application/Interfaces/`
- [ ] Create `SpoolService` in `Application/Services/`
- [ ] Add `GetAllAsync`, `GetByIdAsync`, `RegisterAsync`, `ActivateAsync`, `UpdateWeightAsync`
- [ ] Register `ISpoolService` → `SpoolService` in `Program.cs`

#### SpoolController
- [ ] Create `SpoolController` in `API/Controllers/`
- [ ] Add `GET /api/spools`
- [ ] Add `GET /api/spools/{id}`
- [ ] Add `POST /api/spools` — returns `201 Created`
- [ ] Add `PATCH /api/spools/{id}/activate`
- [ ] Add `PATCH /api/spools/{id}/weight`

#### Swagger / Scalar
- [ ] Install Swagger or Scalar NuGet package in `API`
- [ ] Register and enable in `Development` environment only

#### CORS
- [ ] Add CORS policy in `Program.cs`
- [ ] Allow frontend origin in development
- [ ] Allow `https://spoolarr.local` in production

</details>

<details>
<summary>Milestone 3 — NFC Scan Flow</summary>

#### Models
- [ ] Create `NfcScanResult` record in `Domain/Models/`
- [ ] Create `ScanRequest` record in `Domain/Models/`

#### NfcScanService
- [ ] Create `INfcScanService` in `Application/Interfaces/`
- [ ] Create `NfcScanService` in `Application/Services/`
- [ ] Return `"activated"` for known tag, `"unknown"` for unregistered tag
- [ ] Register in `Program.cs`

#### SignalR
- [ ] Install `Microsoft.AspNetCore.SignalR`
- [ ] Create `NfcScanHub` in `API/Hubs/`
- [ ] Register SignalR and map hub to `/hubs/nfc` in `Program.cs`

#### ScanController
- [ ] Create `ScanController` in `API/Controllers/`
- [ ] Add `POST /api/spools/scan`
- [ ] Push `ScanResult` to all SignalR clients on every scan

</details>

<details>
<summary>Milestone 4 — Bambu MQTT</summary>

#### NuGet packages
- [ ] Install `MQTTnet` in `Infrastructure`
- [ ] Install `MQTTnet.Extensions.ManagedClient` in `Infrastructure`

#### Settings
- [ ] Create `BambuMqttSettings` in `Infrastructure/Settings/`
- [ ] Add `BambuMqtt` section to `appsettings.json`
- [ ] Register settings in `Program.cs`

#### MqttListenerService
- [ ] Create `MqttListenerService` in `Infrastructure/Services/`
- [ ] Implement `IHostedService`
- [ ] Connect to printer, subscribe to `device/{serial}/report`
- [ ] Parse print-finish event, extract grams used
- [ ] Deduct grams from active spool, log `PrintJob`
- [ ] Add retry and reconnect logic
- [ ] Register as hosted service in `Program.cs`

</details>

<details>
<summary>Milestone 5 — Web UI</summary>

#### Setup
- [ ] Create frontend project in `src/`
- [ ] Create `Dockerfile.web` in `docker/`
- [ ] Add `web` service to `docker-compose.yml`
- [ ] Set up environment variables for API base URL

#### Pages
- [ ] Dashboard `/` — spool list with color, weight, active badge
- [ ] Scan page `/scan` — Web NFC on Android, QR fallback on iOS
- [ ] Spool detail `/spools/:id` — full info and print history
- [ ] Register form `/spools/register` — with NFC UID pre-filled

#### SignalR
- [ ] Connect to `/hubs/nfc`
- [ ] Handle `ScanResult` event — show banner or open register form

</details>

<details>
<summary>Milestone 6 — Alerts</summary>

- [ ] Create `AlertSettings` in `Infrastructure/Settings/`
- [ ] Create `IAlertService` in `Application/Interfaces/`
- [ ] Create `AlertService` in `Application/Services/`
- [ ] Send ntfy notification when weight drops below threshold
- [ ] Send webhook when provider set to `"webhook"`
- [ ] Add `POST /api/spools/{id}/test-alert` endpoint (dev only)
- [ ] Wire alert check into `MqttListenerService` after every deduction

</details>

<details>
<summary>Milestone 7 — AMS Support</summary>

- [ ] Create `AmsSlot` entity in `Domain/Models/`
- [ ] Add `DbSet<AmsSlot>` to `FilamentDbContext`
- [ ] Create `IAmsSlotRepository` and `AmsSlotRepository` in `Infrastructure/Repositories/`
- [ ] Create `AmsController` in `API/Controllers/`
- [ ] Add `GET /api/ams` and `PUT /api/ams/{unit}/{slot}`
- [ ] Guard against assigning same spool to two slots — return `409`
- [ ] Update `MqttListenerService` to parse AMS slot and deduct from correct spool
- [ ] Add AMS slot panel to dashboard UI

</details>

---

## Contributing

Pull requests welcome. Open an issue first for anything major.

---

## License

MIT
