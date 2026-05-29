# Milestone 2 — Spool API

> Build the REST API endpoints for managing spools — list, get, register, activate, and update weight.

---

## Goal

By the end of this milestone you have a fully working REST API for spool management that can be tested with Postman, curl, or Swagger UI. No UI yet — just the backend endpoints the frontend will call later.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model

---

## Context from M1

The following are already in place from Milestone 1:

- `Spool` entity with fields: `Id`, `Brand`, `Material`, `ColorName`, `ColorHex`, `InitialWeightG`, `CurrentWeightG`, `SpoolWeightG`, `DiameterMm`, `LowStockThresholdG`, `IsActive`, `IsArchived`, `CreatedAt`, `LastScannedAt`, `Notes`
- `NfcTag` is a **separate entity** linked to `Spool` via `SpoolId` foreign key — there is no `NfcTagUid` field on `Spool`
- `ISpoolRepository` with `GetAllAsync`, `GetByIdAsync`, `GetActiveAsync`, `CreateAsync`, `UpdateAsync`, `ArchiveAsync`, `DeleteAsync`
- All repositories registered as scoped services in `Program.cs`
- SQLite database with migrations applied and seed data on first run

---

## Tasks

### DTOs
- [ ] Create `SpoolResponse` record inside `src/backend/Application/DTOs/` — maps all `Spool` entity fields returned to the client
- [ ] Create `RegisterSpoolRequest` record inside `src/backend/Application/DTOs/` — fields required to register a new spool
- [ ] Create `UpdateWeightRequest` record inside `src/backend/Application/DTOs/` — single `NewWeightG` field

#### SpoolResponse fields
- [ ] `Id` — Guid
- [ ] `Brand` — string
- [ ] `Material` — string
- [ ] `ColorName` — string
- [ ] `ColorHex` — string
- [ ] `InitialWeightG` — float
- [ ] `CurrentWeightG` — float
- [ ] `SpoolWeightG` — float
- [ ] `DiameterMm` — float
- [ ] `LowStockThresholdG` — float
- [ ] `IsActive` — bool
- [ ] `IsArchived` — bool
- [ ] `CreatedAt` — DateTime
- [ ] `LastScannedAt` — DateTime, nullable
- [ ] `Notes` — string, nullable

#### RegisterSpoolRequest fields
- [ ] `Brand` — string, required
- [ ] `Material` — string, required
- [ ] `ColorName` — string, required
- [ ] `ColorHex` — string, required
- [ ] `InitialWeightG` — float, required, must be greater than 0
- [ ] `SpoolWeightG` — float, optional, default 200
- [ ] `DiameterMm` — float, optional, default 1.75
- [ ] `LowStockThresholdG` — float, optional, default 100
- [ ] `Notes` — string, optional

#### UpdateWeightRequest fields
- [ ] `NewWeightG` — float, required, must be 0 or greater

### SpoolService
- [ ] Create `ISpoolService` interface inside `src/backend/Application/Interfaces/`
- [ ] Create `SpoolService` class inside `src/backend/Application/Services/` implementing `ISpoolService`
- [ ] Add `GetAllAsync` — call `ISpoolRepository.GetAllAsync`, map to `SpoolResponse` list
- [ ] Add `GetByIdAsync` — call `ISpoolRepository.GetByIdAsync`, map to `SpoolResponse` or return null
- [ ] Add `RegisterAsync` — build `Spool` from `RegisterSpoolRequest`, set `CreatedAt = DateTime.UtcNow`, call `ISpoolRepository.CreateAsync`
- [ ] Add `ActivateAsync` — deactivate the current active spool via `ISpoolRepository.GetActiveAsync` + `UpdateAsync`, then activate the target spool and set `LastScannedAt = DateTime.UtcNow`
- [ ] Add `UpdateWeightAsync` — load spool by ID, set `CurrentWeightG = NewWeightG`, call `ISpoolRepository.UpdateAsync`
- [ ] Add private `ToResponse` helper — map `Spool` entity to `SpoolResponse`
- [ ] Register `ISpoolService` → `SpoolService` as scoped in `Program.cs`

### SpoolController
- [ ] Create `SpoolController` inside `src/backend/API/Controllers/`
- [ ] Add `GET /api/spools` — returns `200` with list of all spools
- [ ] Add `GET /api/spools/{id}` — returns `200` with spool or `404 Not Found`
- [ ] Add `POST /api/spools` — registers new spool, returns `201 Created` with `Location` header
- [ ] Add `PATCH /api/spools/{id}/activate` — activates spool, returns `200` updated spool or `404`
- [ ] Add `PATCH /api/spools/{id}/weight` — updates weight, returns `200` updated spool or `404`

### Validation
- [ ] `Brand` is required on register
- [ ] `Material` is required on register
- [ ] `ColorName` is required on register
- [ ] `InitialWeightG` must be greater than 0
- [ ] `NewWeightG` must be 0 or greater
- [ ] Return `400 Bad Request` with descriptive message if validation fails

### Swagger / Scalar
- [ ] Install `Scalar.AspNetCore` NuGet package in `src/backend/API`
- [ ] Register Scalar in `Program.cs`
- [ ] Enable only in `Development` environment
- [ ] Confirm all 5 endpoints appear and are testable in the browser UI at `/scalar`

### CORS
- [ ] Add CORS policy in `Program.cs`
- [ ] Allow requests from `http://localhost:3000` in development
- [ ] Allow requests from `https://spoolarr.local` in production
- [ ] Apply CORS middleware before controllers

### Error handling
- [ ] Return `404 Not Found` if spool ID does not exist
- [ ] Return `400 Bad Request` with descriptive message for invalid request body

---

## API Reference

| Method | Endpoint | Returns |
|---|---|---|
| `GET` | `/api/spools` | `200` list of spools |
| `GET` | `/api/spools/{id}` | `200` spool or `404` |
| `POST` | `/api/spools` | `201` created spool |
| `PATCH` | `/api/spools/{id}/activate` | `200` updated spool or `404` |
| `PATCH` | `/api/spools/{id}/weight` | `200` updated spool or `404` |

---

## Definition of Done

- [ ] All 5 endpoints return correct HTTP status codes
- [ ] `POST /api/spools` returns `201 Created` with `Location` header
- [ ] Activating a spool deactivates the previously active one and sets `LastScannedAt`
- [ ] All endpoints tested manually with Postman, curl, or Swagger UI
- [ ] CORS does not block frontend requests in development
