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
- [x] Create `ScanRequest` record inside `src/backend/Application/DTOs/`
- [x] Add `TagUid` field — string, `[Required]`
- [x] Create `NfcScanResult` record inside `src/backend/Application/DTOs/`
- [x] Add `Status` field — string, values: `"activated"` or `"unknown"`
- [x] Add `TagUid` field — string
- [x] Add `Spool` field — `SpoolResponse?`, nullable
- [x] Add `Message` field — string, nullable

### NfcScanService
- [x] Create `INfcScanService` interface inside `src/backend/Application/Interfaces/`
- [x] Create `NfcScanService` class inside `src/backend/Application/Services/` implementing `INfcScanService`
- [x] Add `ProcessScanAsync` — takes tag UID string, returns `NfcScanResult`
- [x] If tag UID not found in DB → return `NfcScanResult` with `Status = "unknown"`, null spool
- [x] If tag UID found → call `ISpoolService.ActivateAsync`, return `Status = "activated"` with spool data
- [x] Register `INfcScanService` → `NfcScanService` as scoped in `Program.cs`

### SignalR hub
- [x] Create `NfcScanHub` class inside `src/backend/API/Hubs/`
- [x] Register SignalR in `Program.cs` via `builder.Services.AddSignalR()`
- [x] Map hub endpoint to `/hubs/nfc` in `Program.cs`

### NfcTagController
- [x] Create `NfcTagController` inside `src/backend/API/Controllers/` (route: `api/nfc-tags`)
- [x] Add `GET /api/nfc-tags` — returns all NFC tags
- [x] Add `GET /api/nfc-tags/{id}` — returns tag by ID or `404`
- [x] Add `POST /api/nfc-tags` — registers new tag, returns `201 Created`
- [x] Add `DELETE /api/nfc-tags/{id}` — deletes tag, returns `204` or `404`
- [x] Add `POST /api/nfc-tags/scan` endpoint accepting `ScanRequest` body
- [x] Validate `TagUid` is not empty — return `400` if missing
- [x] Call `INfcScanService.ProcessScanAsync` with the tag UID
- [x] Push `NfcScanResult` to all SignalR clients via `IHubContext<NfcScanHub>`
- [x] Return `200 OK` with the `NfcScanResult`

### NfcTagService
- [x] Create `INfcTagService` interface inside `src/backend/Application/Interfaces/`
- [x] Create `NfcTagService` class inside `src/backend/Application/Services/` implementing `INfcTagService`
- [x] Add `GetAllAsync`, `GetByIdAsync`, `RegisterAsync`, `DeleteAsync`
- [x] Register `INfcTagService` → `NfcTagService` as scoped in `Program.cs`

### CORS for SignalR
- [x] Add `AllowCredentials()` to CORS policies in `Program.cs` — required for SignalR WebSocket handshake
- [x] Explicit origins already set from M2 (`WithOrigins(...)`) — required when using `AllowCredentials()`

### Error handling
- [x] Return `400 Bad Request` if `TagUid` is null or empty
- [x] Log a warning if scan is received but no tag UID provided
- [x] Handle SignalR send failure gracefully — log error but still return HTTP response

---

## API Reference

| Method | Endpoint | Returns |
|---|---|---|
| `GET` | `/api/nfc-tags` | `200` list of NFC tags |
| `GET` | `/api/nfc-tags/{id}` | `200` tag or `404` |
| `POST` | `/api/nfc-tags` | `201` created tag |
| `DELETE` | `/api/nfc-tags/{id}` | `204` or `404` |
| `POST` | `/api/nfc-tags/scan` | `200` NfcScanResult |

### NfcScanResult shape

```json
{
  "status": "activated",
  "tagUid": "04:A1:B2:C3:D4:E5:F6",
  "spool": { "id": "...", "brand": "Bambu", "material": "PLA" },
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

- [x] `POST /api/nfc-tags/scan` with known UID activates spool and returns `"activated"`
- [x] `POST /api/nfc-tags/scan` with unknown UID returns `"unknown"` with null spool
- [x] `POST /api/nfc-tags/scan` with empty `TagUid` returns `400`
- [x] `GET /api/nfc-tags` returns all registered tags
- [x] `POST /api/nfc-tags` registers a new tag and returns `201`
- [x] `DELETE /api/nfc-tags/{id}` removes a tag and returns `204`
- [x] SignalR hub is accessible at `/hubs/nfc`
- [x] `ScanResult` SignalR event fires on every scan
- [x] Both known and unknown tag scenarios tested with Postman or curl
- [x] SignalR `ScanResult` event received in browser console during test
- [x] CORS does not block SignalR connection from frontend
