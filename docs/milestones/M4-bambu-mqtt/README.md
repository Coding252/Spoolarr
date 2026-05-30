# Milestone 4 — Bambu Lab MQTT Integration

> Connect to the Bambu Lab printer via MQTT (LAN or Cloud), listen for print-finish events, extract grams used, automatically deduct from the active spool, and stream live printer status to the browser via SignalR.

---

## Goal

By the end of this milestone finishing a print on your Bambu printer automatically deducts the correct grams from the active spool and logs the print job to the database — no manual input required. The user configures the printer connection (LAN or Cloud) through the Web UI. Live printer status (state, progress, temperatures) is pushed to the browser in real time via SignalR.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model
- Milestone 2 — Spool API

---

## Context from M1 / M2

The following are already in place:

- `Printer` entity with `IpAddress`, `AccessCode`, `Protocol`, `SerialNumber`, `HasAms`, `AmsSlotCount` fields
- `PrintJob` entity with `SpoolId`, `PrinterId`, `GramsUsed`, `Status`, `Source`, `StartedAt`, `FinishedAt`
- `IPrintJobRepository` with `CreateAsync`, `UpdateAsync`
- `ISpoolRepository` with `GetActiveAsync`, `UpdateAsync`
- `ISpoolService` with `UpdateWeightAsync` — updates `CurrentWeightG` and persists via repository
- All repositories registered as scoped services in `Program.cs`
- `Infrastructure` project already references `Application` — scoped services must be resolved via `IServiceScopeFactory` inside a hosted service

---

## Connection Modes

The user configures the printer in the Web UI and picks one of two modes:

### LAN Mode (`Protocol = "bambu_lan"`)
Connects directly to the printer on the local network. Requires LAN Mode enabled on the printer screen.

| Setting | Source |
|---|---|
| Host | `Printer.IpAddress` |
| Port | `Printer.Port` (default `8883`) |
| Username | `bblp` (fixed for all Bambu printers) |
| Password | `Printer.AccessCode` |
| Topic | `device/{Printer.SerialNumber}/report` |

### Cloud Mode (`Protocol = "bambu_cloud"`)
Connects to Bambu's cloud MQTT broker. Works from anywhere — no LAN Mode required.

| Setting | Source |
|---|---|
| Host | `us.mqtt.bambulab.com` (fixed) |
| Port | `Printer.Port` (default `8883`) |
| Username | `Printer.CloudEmail` |
| Password | JWT token fetched from Bambu login API using `Printer.CloudEmail` + `Printer.CloudPassword` |
| Topic | `device/{Printer.SerialNumber}/report` |

`Printer.CloudPassword` is stored encrypted using ASP.NET Core Data Protection (`IDataProtector`).

---

## Tasks

### M4-S1 — NuGet
- [ ] Install `MQTTnet` version 5.x in `src/backend/Infrastructure`
  - Note: `MQTTnet.Extensions.ManagedClient` was removed in v5 — do not install it

### M4-S2 — Printer entity migration
- [ ] Add `Port` field to `Printer` entity — int, default `8883`
- [ ] Add `CloudEmail` field to `Printer` entity — string, nullable
- [ ] Add `CloudPassword` field to `Printer` entity — string, nullable (stored encrypted)
- [ ] Add EF Core migration for the 3 new fields

### M4-S3 — BambuMqttService
- [ ] Create `BambuMqttService` class inside `src/backend/Infrastructure/Services/`
- [ ] Implement `IHostedService` interface
- [ ] Inject `IServiceScopeFactory`, `IDataProtectionProvider`, `ILogger<BambuMqttService>`
- [ ] Add `StartAsync` — load printer from DB via `IPrinterRepository`, branch on `Protocol`
- [ ] **LAN connect** — build `IMqttClient`, connect to `Printer.IpAddress:Printer.Port` with TLS, username `bblp`, password = `Printer.AccessCode`
- [ ] **Cloud connect** — decrypt `Printer.CloudPassword`, POST to Bambu login API to get JWT token, connect to `us.mqtt.bambulab.com:Printer.Port` with TLS, username = `Printer.CloudEmail`, password = JWT token
- [ ] Configure TLS with `ValidateServerCertificate = false` for LAN use
- [ ] Subscribe to `device/{serial}/report` on successful connect
- [ ] Add `OnMessageReceivedAsync` — fires on every MQTT message
- [ ] Parse incoming JSON payload with `System.Text.Json`
- [ ] **Print finish** — check `print.gcode_state == "FINISH"`, extract `print.filament_weight`, call `DeductFromActiveSpoolAsync`
- [ ] Skip message if grams is 0 or missing
- [ ] **Deduction** — use `IServiceScopeFactory` to resolve `ISpoolRepository`, `IPrintJobRepository`
- [ ] Get active spool via `ISpoolRepository.GetActiveAsync`, log warning and return if none
- [ ] Subtract grams from `CurrentWeightG`, floor at 0, save via `ISpoolRepository.UpdateAsync`
- [ ] Create `PrintJob` record — `SpoolId`, `GramsUsed`, `Status = "finished"`, `Source = "mqtt"`, `StartedAt = FinishedAt = UtcNow`
- [ ] Save print job via `IPrintJobRepository.CreateAsync`
- [ ] Add `StopAsync` — disconnect MQTT client cleanly
- [ ] Register `BambuMqttService` as hosted service in `Program.cs`

### M4-S3 — Retry and resilience
- [ ] If no printer in DB at startup — log warning, retry every 30 seconds
- [ ] If printer offline at startup — log warning, retry every 30 seconds
- [ ] Add reconnect loop — attempt to reconnect every 30 seconds if connection drops
- [ ] Use `CancellationToken` from `StopAsync` to stop retry loop cleanly

### M4-S3 — Logging
- [ ] Log on successful connection (LAN or Cloud)
- [ ] Log on connection failure with error details
- [ ] Log each reconnect attempt with timestamp
- [ ] Log each print-finish event with grams extracted
- [ ] Log warning when no active spool is set
- [ ] Log successful gram deduction with spool ID and remaining weight

### M4-S4 — Live printer status
- [ ] Create `PrinterStatus` record inside `src/backend/Application/DTOs/`
  - `GcodeState` — string (`IDLE`, `RUNNING`, `PAUSE`, `FINISH`, `FAILED`)
  - `ProgressPercent` — int (0–100)
  - `RemainingMinutes` — int
  - `SubtaskName` — string, nullable
  - `LayerNum` — int
  - `TotalLayerNum` — int
  - `NozzleTempC` — float
  - `BedTempC` — float
  - `UpdatedAt` — DateTime (UTC)
- [ ] Create `IPrinterStatusService` interface inside `src/backend/Application/Interfaces/`
  - `GetStatus()` — returns `PrinterStatus?`
  - `UpdateStatus(PrinterStatus status)`
- [ ] Create `PrinterStatusService` inside `src/backend/Application/Services/` — in-memory, no DB
- [ ] Register `PrinterStatusService` as singleton in `Program.cs`
- [ ] Create `PrinterHub` class inside `src/backend/API/Hubs/`
- [ ] Register and map `PrinterHub` to `/hubs/printer` in `Program.cs`
- [ ] In `BambuMqttService.OnMessageReceivedAsync` — parse status fields from every message, call `IPrinterStatusService.UpdateStatus`, push via `PrinterHub` (event name `PrinterStatus`)
- [ ] Add `GET /api/printers/status` in `PrinterController` — returns `PrinterStatus` or `204` if not connected

---

## Bambu MQTT Reference

### Print-finish payload

```json
{
  "print": {
    "gcode_state": "FINISH",
    "filament_weight": 12.5,
    "subtask_name": "benchy.gcode"
  }
}
```

### Live status payload

```json
{
  "print": {
    "gcode_state": "RUNNING",
    "mc_percent": 42,
    "mc_remaining_time": 37,
    "subtask_name": "benchy.gcode",
    "layer_num": 85,
    "total_layer_num": 200,
    "nozzle_temper": 220.0,
    "bed_temper": 65.0
  }
}
```

| Field | Maps to |
|---|---|
| `print.gcode_state` | `PrinterStatus.GcodeState` |
| `print.mc_percent` | `PrinterStatus.ProgressPercent` |
| `print.mc_remaining_time` | `PrinterStatus.RemainingMinutes` |
| `print.subtask_name` | `PrinterStatus.SubtaskName` |
| `print.layer_num` | `PrinterStatus.LayerNum` |
| `print.total_layer_num` | `PrinterStatus.TotalLayerNum` |
| `print.nozzle_temper` | `PrinterStatus.NozzleTempC` |
| `print.bed_temper` | `PrinterStatus.BedTempC` |

### Cloud auth endpoint

```
POST https://bambulab.com/api/sign-in/form
Body: { "account": "email", "password": "password" }
Response: { "accessToken": "jwt..." }
```

---

## Definition of Done

- [ ] Service starts and connects to printer when API boots (LAN or Cloud)
- [ ] Logs confirm successful MQTT connection
- [ ] Print-finish event is detected and processed
- [ ] Grams are correctly deducted from the active spool
- [ ] `PrintJob` row is created in DB after each print
- [ ] Warning is logged if no active spool is set
- [ ] `CurrentWeightG` never goes below 0
- [ ] Service disconnects cleanly when API shuts down
- [ ] Service retries when printer is offline at startup or no printer in DB
- [ ] Service reconnects automatically if connection drops mid-session
- [ ] `GET /api/printers/status` returns current printer state
- [ ] `PrinterStatus` SignalR event fires on every MQTT message
- [ ] Browser receives live state, progress, and temperature updates
- [ ] Cloud password is stored encrypted, never exposed in API responses
