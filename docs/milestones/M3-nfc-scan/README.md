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

## Context from M2

The following are already in place from Milestone 2:

- `ISpoolService` with `ActivateAsync(Guid id)` — activates a spool by ID, deactivates the previous one, sets `LastScannedAt`
- `INfcTagRepository` with `GetByTagUidAsync(string tagUid)` — looks up an NFC tag by UID, includes the linked `Spool`
- `SpoolResponse` DTO in `src/backend/Application/DTOs/` — used as the spool payload in scan results
- CORS policy registered in `Program.cs` — needs `AllowCredentials()` added for SignalR WebSocket connections
- SignalR is part of `Microsoft.AspNetCore.App` — no extra NuGet package needed

---

## Tasks

### DTOs
- [ ] Create `ScanRequest` record inside `src/backend/Application/DTOs/`
- [ ] Add `TagUid` field — string, required
- [ ] Create `NfcScanResult` record inside `src/backend/Application/DTOs/`
- [ ] Add `Status` field — string, values: `"activated"` or `"unknown"`
- [ ] Add `TagUid` field — string
- [ ] Add `Spool` field — `SpoolResponse?`, nullable
- [ ] Add `Message` field — string, nullable

### NfcScanService
- [ ] Create `INfcScanService` interface inside `src/backend/Application/Interfaces/`
- [ ] Create `NfcScanService` class inside `src/backend/Application/Services/` implementing `INfcScanService`
- [ ] Add `ProcessScanAsync` — takes tag UID string, returns `NfcScanResult`
- [ ] If tag UID not found in DB → return `NfcScanResult` with `Status = "unknown"`, null spool
- [ ] If tag UID found → call `ISpoolService.ActivateAsync`, return `Status = "activated"` with spool data
- [ ] Register `INfcScanService` → `NfcScanService` as scoped in `Program.cs`

### SignalR hub
- [ ] Create `NfcScanHub` class inside `src/backend/API/Hubs/`
- [ ] Register SignalR in `Program.cs` via `builder.Services.AddSignalR()`
- [ ] Map hub endpoint to `/hubs/nfc` in `Program.cs`

### ScanController
- [ ] Create `ScanController` inside `src/backend/API/Controllers/`
- [ ] Add `POST /api/spools/scan` endpoint accepting `ScanRequest` body
- [ ] Validate `TagUid` is not empty — return `400` if missing
- [ ] Call `INfcScanService.ProcessScanAsync` with the tag UID
- [ ] Push `NfcScanResult` to all SignalR clients via `IHubContext<NfcScanHub>`
- [ ] Return `200 OK` with the `NfcScanResult`

### CORS for SignalR
- [ ] Add `AllowCredentials()` to the CORS policies in `Program.cs` — required for SignalR WebSocket handshake
- [ ] Replace `AllowAnyOrigin()` / `WithOrigins(...)` with explicit origins (required when using `AllowCredentials()`)

### Error handling
- [ ] Return `400 Bad Request` if `TagUid` is null or empty
- [ ] Log a warning if scan is received but no tag UID provided
- [ ] Handle SignalR send failure gracefully — log error but still return HTTP response

---

## API Reference

| Method | Endpoint | Returns |
|---|---|---|
| `POST` | `/api/spools/scan` | `200` NfcScanResult |

### NfcScanResult shape

```json
{
  "status": "activated",
  "tagUid": "04:A1:B2:C3:D4:E5:F6",
  "spool": { "id": "...", "brand": "Bambu", ... },
  "message": null
}
```

### Status values

| Status | Meaning |
|---|---|
| `activated` | Tag found in DB, spool is now active |
| `unknown` | Tag not in DB, user needs to register |

---

## Definition of Done

- [ ] `POST /api/spools/scan` with known UID activates spool and returns `"activated"`
- [ ] `POST /api/spools/scan` with unknown UID returns `"unknown"` with null spool
- [ ] `POST /api/spools/scan` with empty `TagUid` returns `400`
- [ ] SignalR hub is accessible at `/hubs/nfc`
- [ ] `ScanResult` SignalR event fires on every scan
- [ ] Both known and unknown tag scenarios tested with Postman or curl
- [ ] SignalR `ScanResult` event received in browser console during test
- [ ] CORS does not block SignalR connection from frontend
