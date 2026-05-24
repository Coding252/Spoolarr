# Milestone 4 — Bambu Lab MQTT Integration

> Connect to the Bambu Lab printer on the local network via MQTT, listen for print-finish events, extract grams used, and automatically deduct from the active spool.

---

## Goal

By the end of this milestone finishing a print on your Bambu printer automatically deducts the correct grams from the active spool and logs the print job to the database — no manual input required.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model
- Milestone 2 — Spool API

---

## Tasks

### NuGet packages
- [ ] Install `MQTTnet` in `Infrastructure`
- [ ] Install `MQTTnet.Extensions.ManagedClient` in `Infrastructure`

### Settings
- [ ] Create `BambuMqttSettings` class inside `Infrastructure/Settings/`
- [ ] Add `PrinterIp` field — string
- [ ] Add `Port` field — int, default `8883`
- [ ] Add `Serial` field — string
- [ ] Add `AccessCode` field — string
- [ ] Add `BambuMqtt` section to `appsettings.json` in `API` with placeholder values
- [ ] Register `BambuMqttSettings` in `Program.cs` via `Configure<BambuMqttSettings>`

### MqttListenerService
- [ ] Create `MqttListenerService` class inside `Infrastructure/Services/`
- [ ] Implement `IHostedService` interface
- [ ] Add `StartAsync` — create MQTT client and connect to printer using settings
- [ ] Configure TLS with certificate validation disabled for LAN use
- [ ] Subscribe to topic `device/{serial}/report` on connect
- [ ] Add `OnMessageReceivedAsync` — fires on every MQTT message
- [ ] Parse incoming JSON payload
- [ ] Check `print.gcode_state` equals `"FINISH"` before processing
- [ ] Extract `filament_weight` grams from payload
- [ ] Skip message if grams is 0 or missing
- [ ] Call `DeductFromActiveSpoolAsync` with extracted grams
- [ ] Add `DeductFromActiveSpoolAsync` — deduct grams from active spool
- [ ] Use `IServiceScopeFactory` to resolve scoped services inside hosted service
- [ ] Get active spool from `ISpoolRepository`
- [ ] Log warning and skip if no active spool is set
- [ ] Subtract grams from `CurrentWeightG`, floor at 0
- [ ] Save updated spool via `ISpoolRepository`
- [ ] Create new `PrintJob` record with grams used, spool ID, source `"mqtt"`
- [ ] Save print job via `IPrintJobRepository`
- [ ] Log successful deduction with grams and remaining weight
- [ ] Add `StopAsync` — disconnect MQTT client cleanly
- [ ] Register `MqttListenerService` as hosted service in `Program.cs`

### Retry and resilience
- [ ] If printer is offline at startup do not crash — log warning and keep retrying
- [ ] Add reconnect logic — attempt to reconnect every 30 seconds if connection drops
- [ ] Log each reconnect attempt with timestamp
- [ ] Cap retry attempts or use exponential backoff to avoid hammering the network

### Logging
- [ ] Log on successful connection to printer
- [ ] Log on connection failure with error details
- [ ] Log each reconnect attempt
- [ ] Log each print-finish event received
- [ ] Log warning when no active spool is set
- [ ] Log successful gram deduction with remaining weight

---

## Bambu MQTT Reference

| Setting | Value |
|---|---|
| Host | Printer LAN IP |
| Port | `8883` (TLS) |
| Username | `bblp` |
| Password | LAN access code from printer screen |
| Topic | `device/{serial}/report` |

### Print-finish payload fields

| Field | Description |
|---|---|
| `print.gcode_state` | Must equal `"FINISH"` to process |
| `print.filament_weight` | Grams used in the print |
| `print.subtask_name` | Print file name, optional |

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
