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

## Scan Flow

![Spoolarr NFC Scan Flow](docs/spoolarr-nfc-scan-flow.png)

> Full interactive version — open [`docs/spoolarr-nfc-scan-flow.html`](docs/spoolarr-nfc-scan-flow.html) in any browser.

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

- [ ] Phase 1 — NFC spool identity
- [ ] Phase 2 — Bambu Lab MQTT integration (no AMS)
- [ ] Phase 3 — Gram tracking per spool
- [ ] Phase 4 — Docker stack
- [ ] Phase 5 — AMS multi-slot support
- [ ] Phase 6 — Low stock alerts (ntfy / webhook)
- [ ] Phase 7 — Print history & stats dashboard

---

## Contributing

Pull requests welcome. Open an issue first for anything major.

---

## License

MIT
