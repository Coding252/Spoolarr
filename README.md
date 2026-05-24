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

| Layer | Technology |
|---|---|
| Backend API | C# · ASP.NET Core · .NET 8 |
| Real-time | SignalR (NFC scan push to browser) |
| MQTT listener | MQTTnet · `IHostedService` |
| Database | SQLite (default) |
| ORM | Entity Framework Core |
| Frontend | React or plain HTML/JS |
| Reverse proxy | Caddy (HTTPS for Web NFC API) |
| Container | Docker · docker-compose |

---

## NFC Tag Support

| Tag type | Read | Write | Notes |
|---|---|---|---|
| OpenPrintTag (Prusament) | Yes | Yes | Auto-fills all fields including remaining grams |
| OpenTag3D | Yes | Yes | Open standard, growing brand support |
| Own NTAG215 sticker | Yes | Yes | Blank sticker attached by user, full manual registration |

---

## Project Structure

```
spoolarr/
├── src/
│   ├── Spoolarr.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── SpoolController.cs
│   │   │   └── PrintJobController.cs
│   │   ├── Hubs/
│   │   │   └── NfcScanHub.cs       # SignalR hub
│   │   ├── Services/
│   │   │   ├── SpoolService.cs
│   │   │   ├── NfcScanService.cs
│   │   │   └── MqttListenerService.cs
│   │   ├── Repositories/
│   │   │   ├── SpoolRepository.cs
│   │   │   └── PrintJobRepository.cs
│   │   ├── Models/
│   │   │   ├── Spool.cs
│   │   │   └── PrintJob.cs
│   │   ├── Data/
│   │   │   └── FilamentDbContext.cs
│   │   └── Program.cs
│   └── Spoolarr.Web/              # Frontend UI
├── docker/
│   ├── docker-compose.yml
│   ├── Dockerfile.api
│   └── Caddyfile
├── docs/
│   └── spoolarr-nfc-scan-flow.html
└── README.md
```

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

Each milestone has its own detailed README with tasks, code snippets, and definition of done.

| Milestone | Description | Status |
|---|---|---|
| [M0 — Project Bootstrap](docs/milestones/M0-bootstrap/README.md) | Solution setup, Docker, Caddy, EF Core, health endpoint | ⬜ Not started |
| [M1 — Data Model](docs/milestones/M1-data-model/README.md) | `Spool` + `PrintJob` entities, migrations, repositories, seed data | ⬜ Not started |
| [M2 — Spool API](docs/milestones/M2-spool-api/README.md) | REST endpoints for spool management | ⬜ Not started |
| [M3 — NFC Scan Flow](docs/milestones/M3-nfc-scan/README.md) | Scan endpoint, `NfcScanService`, SignalR real-time push | ⬜ Not started |
| [M4 — Bambu MQTT](docs/milestones/M4-bambu-mqtt/README.md) | MQTT listener, print-finish event, auto gram deduction | ⬜ Not started |
| [M5 — Web UI](docs/milestones/M5-web-ui/README.md) | Dashboard, scan page, Web NFC, QR fallback, register form | ⬜ Not started |
| [M6 — Alerts](docs/milestones/M6-alerts/README.md) | Low stock threshold check, ntfy / webhook notifications | ⬜ Not started |
| [M7 — AMS Support](docs/milestones/M7-ams/README.md) | Multi-slot mapping, AMS MQTT parsing, slot UI | ⬜ Not started |

### Task checklist

<details>
<summary>Milestone 0 — Project Bootstrap</summary>

- [ ] Create solution `Spoolarr.sln` with `Spoolarr.Api` and `Spoolarr.Web` projects
- [ ] Set up Docker + docker-compose skeleton
- [ ] Configure Caddy for HTTPS
- [ ] Set up EF Core + SQLite + `FilamentDbContext`
- [ ] Basic health check endpoint `GET /health`

</details>

<details>
<summary>Milestone 1 — Data Model</summary>

- [ ] `Spool` entity + migration
- [ ] `PrintJob` entity + migration
- [ ] `SpoolRepository` + `PrintJobRepository`
- [ ] Seed data for testing

</details>

<details>
<summary>Milestone 2 — Spool API</summary>

- [ ] `GET /api/spools` — list all spools
- [ ] `GET /api/spools/{id}` — get single spool
- [ ] `POST /api/spools` — register new spool
- [ ] `PATCH /api/spools/{id}/activate` — set active spool
- [ ] `PATCH /api/spools/{id}/weight` — update weight manually

</details>

<details>
<summary>Milestone 3 — NFC Scan Flow</summary>

- [ ] `POST /api/spools/scan` — receives tag UID, routes to register or activate
- [ ] `NfcScanService` — lookup by UID, return spool or "unknown tag"
- [ ] `NfcScanHub` SignalR — push scan result to browser in real time

</details>

<details>
<summary>Milestone 4 — Bambu MQTT</summary>

- [ ] `MqttListenerService` as `IHostedService`
- [ ] Connect to printer on LAN using MQTTnet
- [ ] Parse print-finish event, extract grams used
- [ ] Deduct grams from active spool via `SpoolService`
- [ ] Log `PrintJob` to DB

</details>

<details>
<summary>Milestone 5 — Web UI</summary>

- [ ] Spool list dashboard
- [ ] Spool detail page
- [ ] NFC scan page (Web NFC + QR fallback for iOS)
- [ ] Register spool form
- [ ] Active spool indicator

</details>

<details>
<summary>Milestone 6 — Alerts</summary>

- [ ] Threshold check after every gram deduction
- [ ] Webhook / ntfy push notification

</details>

<details>
<summary>Milestone 7 — AMS Support</summary>

- [ ] Slot-to-spool mapping model
- [ ] Multi-slot MQTT parsing
- [ ] UI slot management

</details>

---

## Contributing

Pull requests welcome. Open an issue first for anything major.

---

## License

MIT
