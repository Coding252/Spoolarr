# Milestone 4 — Bambu Lab MQTT Integration

> Connect to the Bambu Lab printer on the local network via MQTT, listen for print-finish events, extract grams used, automatically deduct from the active spool, and stream live printer status to the browser via SignalR.

---

## Goal

By the end of this milestone finishing a print on your Bambu printer automatically deducts the correct grams from the active spool and logs the print job to the database — no manual input required. Live printer status (state, progress, temperatures) is also pushed to the browser in real time via SignalR.

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

## Tasks

### NuGet packages
- [ ] Install `MQTTnet` version 5.x in `src/backend/Infrastructure`
  - Note: `MQTTnet.Extensions.ManagedClient` was removed in v5 — do not install it

### Settings
- [ ] Create `BambuMqttSettings` class inside `src/backend/Infrastructure/Settings/`
- [ ] Add `PrinterIp` field — string
- [ ] Add `Port` field — int, default `8883`
- [ ] Add `Serial` field — string
- [ ] Add `AccessCode` field — string
- [ ] Add `BambuMqtt` section to `src/backend/API/appsettings.json` with placeholder values
- [ ] Add `BambuMqtt` section to `src/backend/API/appsettings.Development.json` with real local values for testing
- [ ] Register `BambuMqttSettings` in `Program.cs` via `builder.Services.Configure<BambuMqttSettings>(...)`

### MqttListenerService
- [ ] Create `MqttListenerService` class inside `src/backend/Infrastructure/Services/`
- [ ] Implement `IHostedService` interface
- [ ] Inject `IServiceScopeFactory`, `IOptions<BambuMqttSettings>`, `ILogger<MqttListenerService>`
- [ ] Add `StartAsync` — create `MqttFactory`, build `IMqttClient`, connect to printer using settings
- [ ] Configure TLS with `ValidateServerCertificate = false` for LAN use
- [ ] Set MQTT credentials — username `bblp`, password = `AccessCode`
- [ ] Subscribe to topic `device/{serial}/report` on successful connect
- [ ] Add `OnMessageReceivedAsync` — fires on every MQTT message
- [ ] Parse incoming JSON payload with `System.Text.Json`
- [ ] Check `print.gcode_state` equals `"FINISH"` before processing — skip all other states
- [ ] Extract `print.filament_weight` (grams, float) from payload
- [ ] Skip message if grams is 0 or missing
- [ ] Call `DeductFromActiveSpoolAsync` with extracted grams and print file name
- [ ] Add `DeductFromActiveSpoolAsync` — deduct grams and log print job
- [ ] Use `IServiceScopeFactory` to create a scope and resolve `ISpoolRepository`, `IPrintJobRepository`
- [ ] Get active spool via `ISpoolRepository.GetActiveAsync`
- [ ] Log warning and return if no active spool is set
- [ ] Subtract grams from `CurrentWeightG`, floor at 0, save via `ISpoolRepository.UpdateAsync`
- [ ] Create `PrintJob` record — set `SpoolId`, `GramsUsed`, `Status = "finished"`, `Source = "mqtt"`, `StartedAt = FinishedAt = UtcNow`
- [ ] Save print job via `IPrintJobRepository.CreateAsync`
- [ ] Log successful deduction with grams used and remaining weight
- [ ] Add `StopAsync` — disconnect MQTT client cleanly
- [ ] Register `MqttListenerService` as hosted service in `Program.cs`

### Live printer status (M4-S4)
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
  - `GetStatus()` — returns current `PrinterStatus?` (in-memory, no DB)
  - `UpdateStatus(PrinterStatus status)` — called by MQTT listener on every message
- [ ] Create `PrinterStatusService` inside `src/backend/Application/Services/` implementing `IPrinterStatusService`
  - Store status as a private field — no DB write, in-memory only
  - Register as singleton in `Program.cs`
- [ ] Create `PrinterHub` class inside `src/backend/API/Hubs/`
- [ ] Register and map `PrinterHub` to `/hubs/printer` in `Program.cs`
- [ ] Inject `IPrinterStatusService` and `IHubContext<PrinterHub>` into `MqttListenerService`
- [ ] In `OnMessageReceivedAsync` — parse status fields from every message regardless of `gcode_state`
- [ ] Call `IPrinterStatusService.UpdateStatus(...)` with parsed values
- [ ] Push `PrinterStatus` to all SignalR clients via `PrinterHub` — event name `PrinterStatus`
- [ ] Add `GET /api/printers/status` endpoint in a new `PrinterController`
  - Return current `PrinterStatus` from `IPrinterStatusService.GetStatus()`
  - Return `204 No Content` if no status received yet

### Retry and resilience
- [ ] If printer is offline at startup — log warning and keep retrying, do not crash
- [ ] Add reconnect loop — attempt to reconnect every 30 seconds if connection drops
- [ ] Log each reconnect attempt with timestamp
- [ ] Use `CancellationToken` from `StopAsync` to stop the retry loop cleanly

### Logging
- [ ] Log on successful connection to printer
- [ ] Log on connection failure with error details
- [ ] Log each reconnect attempt
- [ ] Log each print-finish event received with grams extracted
- [ ] Log warning when no active spool is set
- [ ] Log successful gram deduction with spool ID and remaining weight

---

## Bambu MQTT Reference

| Setting | Value |
|---|---|
| Host | Printer LAN IP |
| Port | `8883` (TLS) |
| Username | `bblp` |
| Password | LAN access code from printer screen |
| Topic | `device/{serial}/report` |

### Print-finish payload (relevant fields)

```json
{
  "print": {
    "gcode_state": "FINISH",
    "filament_weight": 12.5,
    "subtask_name": "benchy.gcode"
  }
}
```

| Field | Description |
|---|---|
| `print.gcode_state` | Must equal `"FINISH"` to process — other values: `"RUNNING"`, `"PAUSE"`, `"FAILED"`, `"IDLE"` |
| `print.filament_weight` | Grams used in the print (float) |
| `print.subtask_name` | Print file name, used as `PrintJob.PrintFileName` |

### Live status payload (all messages)

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

---

## Definition of Done

- [ ] Service starts and connects to printer when API boots
- [ ] Logs confirm successful MQTT connection
- [ ] Print-finish event is detected and processed
- [ ] Grams are correctly deducted from the active spool
- [ ] `PrintJob` row is created in DB after each print
- [ ] Warning is logged if no active spool is set
- [ ] `CurrentWeightG` never goes below 0
- [ ] Service disconnects cleanly when API shuts down
- [ ] Service retries connection when printer is offline at startup
- [ ] Service reconnects automatically if connection drops mid-session
- [ ] `GET /api/printers/status` returns current printer state
- [ ] `PrinterStatus` SignalR event fires on every MQTT message
- [ ] Browser receives live state, progress, and temperature updates
