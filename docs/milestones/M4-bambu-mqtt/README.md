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
- [x] Install `MQTTnet` version 5.x in `src/backend/Infrastructure`
  - Note: `MQTTnet.Extensions.ManagedClient` was removed in v5 — do not install it

### M4-S2 — Printer entity migration
- [x] Add `Port` field to `Printer` entity — int, default `8883`
- [x] Add `CloudEmail` field to `Printer` entity — string, nullable
- [x] Add `CloudPassword` field to `Printer` entity — string, nullable (stored encrypted)
- [x] Add EF Core migration for the 3 new fields

### M4-S3 — MqttPrinterService
> Originally named `BambuMqttService` — renamed to `MqttPrinterService` for brand-agnostic naming.
> Message processing logic extracted to `IMqttMessageProcessor` / `MqttMessageProcessor` in `Application/Services/` for testability.

- [x] Create `MqttPrinterService` class inside `src/backend/Infrastructure/Services/`
- [x] Implement `IHostedService` interface
- [x] Inject `IServiceScopeFactory`, `IDataProtectionProvider`, `ILogger<MqttPrinterService>`
- [x] Add `StartAsync` — load printer from DB via `IPrinterRepository`, branch on `Protocol`
- [x] **LAN connect** — build `IMqttClient`, connect to `Printer.IpAddress:Printer.Port` with TLS, username `bblp`, password = `Printer.AccessCode`
- [x] **Cloud connect** — decrypt `Printer.CloudPassword`, POST to Bambu login API to get JWT token, connect to `us.mqtt.bambulab.com:Printer.Port` with TLS, username = `Printer.CloudEmail`, password = JWT token
- [x] Configure TLS with `ValidateServerCertificate = false` for LAN use
- [x] Subscribe to `device/{serial}/report` on successful connect
- [x] Add `OnMessageReceivedAsync` — delegates to `IMqttMessageProcessor.ProcessAsync`
- [x] `MqttMessageProcessor.ProcessAsync` — parses JSON payload with `System.Text.Json`
- [x] **Print finish** — check `print.gcode_state == "FINISH"`, extract `print.filament_weight`
- [x] Skip message if grams is 0 or missing
- [x] **Deduction** — use `IServiceScopeFactory` to resolve `ISpoolRepository`, `IPrintJobRepository`
- [x] Get active spool via `ISpoolRepository.GetActiveAsync`, log warning and return if none
- [x] Subtract grams from `CurrentWeightG`, floor at 0, save via `ISpoolRepository.UpdateAsync`
- [x] Create `PrintJob` record — `SpoolId`, `GramsUsed`, `Status = "finished"`, `Source = "mqtt"`, `StartedAt = FinishedAt = UtcNow`
- [x] Save print job via `IPrintJobRepository.CreateAsync`
- [x] Add `StopAsync` — disconnect MQTT client cleanly
- [x] Register `MqttPrinterService` as hosted service in `Program.cs`

### M4-S3 — Retry and resilience
- [x] If no printer in DB at startup — log warning, retry every 30 seconds
- [x] If printer offline at startup — log warning, retry every 30 seconds
- [x] Add reconnect loop — attempt to reconnect every 30 seconds if connection drops
- [x] Use `CancellationToken` from `StopAsync` to stop retry loop cleanly

### M4-S3 — Logging
- [x] Log on successful connection (LAN or Cloud)
- [x] Log on connection failure with error details
- [x] Log each reconnect attempt with timestamp
- [x] Log each print-finish event with grams extracted
- [x] Log warning when no active spool is set
- [x] Log successful gram deduction with spool ID and remaining weight

### M4-S4 — Live printer status
- [x] Create `PrinterStatus` record inside `src/backend/Application/DTOs/`
  - `GcodeState` — string (`IDLE`, `RUNNING`, `PAUSE`, `FINISH`, `FAILED`)
  - `ProgressPercent` — int (0–100)
  - `RemainingMinutes` — int
  - `SubtaskName` — string, nullable
  - `LayerNum` — int
  - `TotalLayerNum` — int
  - `NozzleTempC` — float
  - `BedTempC` — float
  - `UpdatedAt` — DateTime (UTC)
- [x] Create `IPrinterStatusService` interface inside `src/backend/Application/Interfaces/`
  - `GetStatus()` — returns `PrinterStatus?`
  - `UpdateStatus(PrinterStatus status)`
- [x] Create `PrinterStatusService` inside `src/backend/Application/Services/` — in-memory, no DB
- [x] Register `PrinterStatusService` as singleton in `Program.cs`
- [x] Create `PrinterHub` class inside `src/backend/API/Hubs/`
- [x] Register and map `PrinterHub` to `/hubs/printer` in `Program.cs`
- [x] In `MqttPrinterService.OnMessageReceivedAsync` — parse status fields from every message, call `IPrinterStatusService.UpdateStatus`, push via `PrinterHub` (event name `PrinterStatus`)
- [x] Add `GET /api/printers/status` in `PrinterController` — returns `PrinterStatus` or `204` if not connected

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

- [x] Service starts and connects to printer when API boots (LAN or Cloud)
- [x] Logs confirm successful MQTT connection
- [x] Print-finish event is detected and processed
- [x] Grams are correctly deducted from the active spool
- [x] `PrintJob` row is created in DB after each print
- [x] Warning is logged if no active spool is set
- [x] `CurrentWeightG` never goes below 0
- [x] Service disconnects cleanly when API shuts down
- [x] Service retries when printer is offline at startup or no printer in DB
- [x] Service reconnects automatically if connection drops mid-session
- [x] `GET /api/printers/status` returns current printer state
- [x] `PrinterStatus` SignalR event fires on every MQTT message
- [x] Browser receives live state, progress, and temperature updates
- [x] Cloud password is stored encrypted, never exposed in API responses
