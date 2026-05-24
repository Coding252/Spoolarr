# Milestone 2 — Spool API

> Build the REST API endpoints for managing spools — list, get, register, activate, and update weight.

---

## Goal

By the end of this milestone you have a fully working REST API for spool management that can be tested with Postman or curl. No UI yet — just the backend endpoints the frontend will call later.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model

---

## Tasks

### DTOs
- [ ] Create `SpoolResponse` record inside `Application/DTOs/` — all spool fields returned to the client
- [ ] Create `RegisterSpoolRequest` record inside `Application/DTOs/` — fields required to register a new spool
- [ ] Create `UpdateWeightRequest` record inside `Application/DTOs/` — single `NewWeightG` field

### SpoolService
- [ ] Create `ISpoolService` interface inside `Application/Interfaces/`
- [ ] Create `SpoolService` class inside `Application/Services/` implementing `ISpoolService`
- [ ] Add `GetAllAsync` — return all spools as `SpoolResponse` list
- [ ] Add `GetByIdAsync` — return single spool as `SpoolResponse` or null
- [ ] Add `RegisterAsync` — create new spool from `RegisterSpoolRequest`
- [ ] Add `ActivateAsync` — deactivate current active spool, activate the new one, update `LastScannedAt`
- [ ] Add `UpdateWeightAsync` — update `CurrentWeightG` on a spool
- [ ] Add private `ToResponse` helper — map `Spool` entity to `SpoolResponse`
- [ ] Register `ISpoolService` → `SpoolService` in `Program.cs`

### SpoolController
- [ ] Create `SpoolController` inside `API/Controllers/`
- [ ] Add `GET /api/spools` — returns list of all spools
- [ ] Add `GET /api/spools/{id}` — returns single spool or `404`
- [ ] Add `POST /api/spools` — registers new spool, returns `201 Created` with location header
- [ ] Add `PATCH /api/spools/{id}/activate` — activates spool, returns updated spool or `404`
- [ ] Add `PATCH /api/spools/{id}/weight` — updates weight, returns updated spool or `404`

### Validation
- [ ] `NfcTagUid` is required on register
- [ ] `Material` is required on register
- [ ] `InitialWeightG` must be greater than 0
- [ ] `NewWeightG` must be 0 or greater
- [ ] Return `400 Bad Request` with message if validation fails

### Swagger / Scalar
- [ ] Install `Scalar.AspNetCore` or `Swashbuckle.AspNetCore` NuGet package
- [ ] Register Swagger / Scalar in `Program.cs`
- [ ] Enable only in `Development` environment
- [ ] Confirm all 5 endpoints appear and are testable in the browser UI

### CORS
- [ ] Add CORS policy in `Program.cs`
- [ ] Allow requests from the frontend origin in development (e.g. `http://localhost:3000`)
- [ ] Allow requests from `https://spoolarr.local` in production
- [ ] Apply CORS middleware before controllers

### Error handling
- [ ] Return `404 Not Found` if spool ID does not exist
- [ ] Return `409 Conflict` if `NfcTagUid` is already registered
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
- [ ] `POST /api/spools` returns `201 Created` with location header
- [ ] Activating a spool deactivates the previously active one
- [ ] Duplicate `NfcTagUid` returns `409 Conflict`
- [ ] All endpoints tested manually with Postman, curl, or Swagger UI
- [ ] CORS does not block frontend requests in development
