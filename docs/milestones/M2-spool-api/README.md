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
- [x] Create `SpoolResponse` record inside `src/backend/Application/DTOs/` — maps all `Spool` entity fields returned to the client
- [x] Create `RegisterSpoolRequest` record inside `src/backend/Application/DTOs/` — fields required to register a new spool
- [x] Create `UpdateWeightRequest` record inside `src/backend/Application/DTOs/` — single `NewWeightG` field

#### SpoolResponse fields
- [x] `Id` — Guid
- [x] `Brand` — string
- [x] `Material` — string
- [x] `ColorName` — string
- [x] `ColorHex` — string
- [x] `InitialWeightG` — float
- [x] `CurrentWeightG` — float
- [x] `SpoolWeightG` — float
- [x] `DiameterMm` — float
- [x] `LowStockThresholdG` — float
- [x] `IsActive` — bool
- [x] `IsArchived` — bool
- [x] `CreatedAt` — DateTime
- [x] `LastScannedAt` — DateTime, nullable
- [x] `Notes` — string, nullable

#### RegisterSpoolRequest fields
- [x] `Brand` — string, required
- [x] `Material` — string, required
- [x] `ColorName` — string, required
- [x] `ColorHex` — string, required
- [x] `InitialWeightG` — float, required, must be greater than 0
- [x] `SpoolWeightG` — float, optional, default 200
- [x] `DiameterMm` — float, optional, default 1.75
- [x] `LowStockThresholdG` — float, optional, default 100
- [x] `Notes` — string, optional

#### UpdateWeightRequest fields
- [x] `NewWeightG` — float, required, must be 0 or greater

### SpoolService
- [x] Create `ISpoolService` interface inside `src/backend/Application/Interfaces/`
- [x] Create `SpoolService` class inside `src/backend/Application/Services/` implementing `ISpoolService`
- [x] Add `GetAllAsync` — call `ISpoolRepository.GetAllAsync`, map to `SpoolResponse` list
- [x] Add `GetByIdAsync` — call `ISpoolRepository.GetByIdAsync`, map to `SpoolResponse` or return null
- [x] Add `RegisterAsync` — build `Spool` from `RegisterSpoolRequest`, set `CreatedAt = DateTime.UtcNow`, call `ISpoolRepository.CreateAsync`
- [x] Add `ActivateAsync` — deactivate the current active spool via `ISpoolRepository.GetActiveAsync` + `UpdateAsync`, then activate the target spool and set `LastScannedAt = DateTime.UtcNow`
- [x] Add `UpdateWeightAsync` — load spool by ID, set `CurrentWeightG = NewWeightG`, call `ISpoolRepository.UpdateAsync`
- [x] Add private `ToResponse` helper — map `Spool` entity to `SpoolResponse`
- [x] Register `ISpoolService` → `SpoolService` as scoped in `Program.cs`

### SpoolController
- [x] Create `SpoolController` inside `src/backend/API/Controllers/`
- [x] Add `GET /api/spools` — returns `200` with list of all spools
- [x] Add `GET /api/spools/{id}` — returns `200` with spool or `404 Not Found`
- [x] Add `POST /api/spools` — registers new spool, returns `201 Created` with `Location` header
- [x] Add `PATCH /api/spools/{id}/activate` — activates spool, returns `200` updated spool or `404`
- [x] Add `PUT /api/spools/{id}` — patch-style update of any spool fields, returns `200` or `404`
- [x] Add `DELETE /api/spools/{id}` — deletes spool, returns `204` or `404`

### Validation
- [x] `Brand` is required on register
- [x] `Material` is required on register
- [x] `ColorName` is required on register
- [x] `InitialWeightG` must be greater than 0
- [x] `NewWeightG` must be 0 or greater
- [x] Return `400 Bad Request` with descriptive message if validation fails

### Swagger / Scalar
- [x] Install `Scalar.AspNetCore` NuGet package in `src/backend/API`
- [x] Register Scalar in `Program.cs`
- [x] Enable only in `Development` environment
- [x] Confirm all 5 endpoints appear and are testable in the browser UI at `/scalar`

### CORS
- [x] Add CORS policy in `Program.cs`
- [x] Allow requests from `http://localhost:3000` in development
- [x] Allow requests from `https://spoolarr.local` in production
- [x] Apply CORS middleware before controllers

### Error handling
- [x] Return `404 Not Found` if spool ID does not exist
- [x] Return `400 Bad Request` with descriptive message for invalid request body

---

## API Reference

| Method | Endpoint | Returns |
|---|---|---|
| `GET` | `/api/spools` | `200` list of spools |
| `GET` | `/api/spools/{id}` | `200` spool or `404` |
| `POST` | `/api/spools` | `201` created spool |
| `PATCH` | `/api/spools/{id}/activate` | `200` updated spool or `404` |
| `PUT` | `/api/spools/{id}` | `200` updated spool or `404` |
| `DELETE` | `/api/spools/{id}` | `204` or `404` |

---

## Definition of Done

- [x] All 5 endpoints return correct HTTP status codes
- [x] `POST /api/spools` returns `201 Created` with `Location` header
- [x] Activating a spool deactivates the previously active one and sets `LastScannedAt`
- [x] All endpoints tested manually with Postman, curl, or Swagger UI
- [x] CORS does not block frontend requests in development
