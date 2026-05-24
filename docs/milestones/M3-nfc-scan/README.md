# Milestone 3 — NFC Scan Flow

> Handle the NFC tag scan event — look up the tag UID, route to register or activate, and push the result to the browser in real time via SignalR.

---

## Goal

By the end of this milestone tapping a phone to a spool triggers a scan event that the API processes and instantly pushes back to the browser — no page refresh needed. Unknown tags prompt registration. Known tags activate the spool.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model
- Milestone 2 — Spool API

---

## Tasks

### NuGet packages
- [ ] Install `Microsoft.AspNetCore.SignalR`

### Models
- [ ] Create `NfcScanResult` record inside `Models/`
- [ ] Add `Status` field — string, values: `"activated"` or `"unknown"`
- [ ] Add `TagUid` field — string
- [ ] Add `Spool` field — `SpoolResponse`, nullable
- [ ] Add `Message` field — string, nullable
- [ ] Create `ScanRequest` record inside `Models/`
- [ ] Add `TagUid` field — string

### NfcScanService
- [ ] Create `INfcScanService` interface inside `Services/`
- [ ] Create `NfcScanService` class implementing `INfcScanService`
- [ ] Add `ProcessScanAsync` — takes tag UID, returns `NfcScanResult`
- [ ] If tag UID not found in DB → return status `"unknown"` with no spool
- [ ] If tag UID found in DB → activate spool, return status `"activated"` with spool data
- [ ] Register `INfcScanService` → `NfcScanService` in `Program.cs`

### SignalR hub
- [ ] Create `NfcScanHub` class inside `Hubs/`
- [ ] Register SignalR in `Program.cs` via `builder.Services.AddSignalR()`
- [ ] Map hub endpoint to `/hubs/nfc` in `Program.cs`

### ScanController
- [ ] Create `ScanController` inside `Controllers/`
- [ ] Add `POST /api/spools/scan` endpoint
- [ ] Validate `TagUid` is not empty — return `400` if missing
- [ ] Call `NfcScanService.ProcessScanAsync` with the tag UID
- [ ] Push `NfcScanResult` to all SignalR clients via `NfcScanHub`
- [ ] Return `200 OK` with the `NfcScanResult`

### CORS for SignalR
- [ ] Ensure CORS policy allows SignalR WebSocket connections from the frontend origin
- [ ] Test SignalR connection is not blocked in the browser console

### Error handling
- [ ] Return `400 Bad Request` if `TagUid` is null or empty
- [ ] Log a warning if scan is received but no tag UID provided
- [ ] Handle SignalR send failure gracefully — log error but still return HTTP response

---

## API Reference

| Method | Endpoint | Returns |
|---|---|---|
| `POST` | `/api/spools/scan` | `200` NfcScanResult |

### Scan result values

| Status | Meaning |
|---|---|
| `activated` | Tag found in DB, spool is now active |
| `unknown` | Tag not in DB, user needs to register |

---

## Definition of Done

- [ ] `POST /api/spools/scan` with known UID activates spool and returns `"activated"`
- [ ] `POST /api/spools/scan` with unknown UID returns `"unknown"` with null spool
- [ ] SignalR hub is accessible at `/hubs/nfc`
- [ ] `ScanResult` SignalR event fires on every scan
- [ ] Both known and unknown tag scenarios tested with Postman or curl
- [ ] SignalR `ScanResult` event received in browser console during test
- [ ] CORS does not block SignalR connection from frontend
