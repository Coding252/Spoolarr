# Milestone 7 — AMS Multi-Slot Support

> Extend Spoolarr to support the Bambu Lab AMS — track up to 4 filament slots simultaneously and map each MQTT print event to the correct spool.

---

## Goal

By the end of this milestone each AMS slot is mapped to a registered spool. When a print finishes the gram deduction goes to the correct spool based on which slot was used — not just the single active spool.

---

## Depends On

- Milestone 1 — Data Model
- Milestone 2 — Spool API
- Milestone 4 — Bambu MQTT

---

## Tasks

### Models
- [ ] Create `AmsSlot` entity class inside `Domain/Models/`
- [ ] Add `Id` field — Guid, primary key
- [ ] Add `SlotIndex` field — int, values 0–3
- [ ] Add `AmsUnit` field — int, default 0 (for multi-AMS setups)
- [ ] Add `SpoolId` field — Guid, nullable, foreign key to `Spool`
- [ ] Add `Spool` — navigation property, nullable
- [ ] Add `LoadedAt` field — DateTime, nullable

### Database
- [ ] Add `DbSet<AmsSlot>` to `FilamentDbContext`
- [ ] Configure foreign key from `AmsSlot.SpoolId` to `Spool.Id`
- [ ] Set foreign key as nullable — slot can be empty
- [ ] Run `dotnet ef migrations add AddAmsSlots` — run from the `Infrastructure` project
- [ ] Run `dotnet ef database update` — run from the `Infrastructure` project
- [ ] Confirm `AmsSlots` table is created

### Repository
- [ ] Create `IAmsSlotRepository` interface inside `Infrastructure/Repositories/`
- [ ] Create `AmsSlotRepository` class inside `Infrastructure/Repositories/` implementing `IAmsSlotRepository`
- [ ] Add `GetAllAsync` — return all slots including their assigned spool
- [ ] Add `GetBySlotAsync` — return slot by unit and slot index
- [ ] Add `UpsertAsync` — create or update a slot assignment by unit and index
- [ ] Register `IAmsSlotRepository` → `AmsSlotRepository` in `Program.cs`

### AMS API
- [ ] Create `AmsController` inside `API/Controllers/`
- [ ] Add `GET /api/ams` — return all slots with assigned spool data
- [ ] Add `PUT /api/ams/{unit}/{slot}` — assign or unassign a spool to a slot
- [ ] Return `404` if spool ID does not exist when assigning
- [ ] Allow `SpoolId` to be null to unassign a slot

### Update MQTT service
- [ ] Update `MqttListenerService` to parse AMS slot data from print-finish payload
- [ ] Extract which slot was used from `print.ams` in the payload
- [ ] Look up the assigned spool for that slot via `IAmsSlotRepository`
- [ ] Log warning and skip if the slot has no spool assigned
- [ ] Deduct grams from the slot's assigned spool instead of the globally active spool
- [ ] Keep the single active spool fallback for non-AMS prints

### Duplicate slot assignment guard
- [ ] Check if a spool is already assigned to another slot before assigning
- [ ] Return `409 Conflict` if the same spool is assigned to two different slots
- [ ] Show a clear error in the UI if the user tries to assign a duplicate

### Web UI updates
- [ ] Add AMS panel to the dashboard
- [ ] Show 4 slot cards representing AMS unit 0
- [ ] Each card shows assigned spool color, material, and remaining grams
- [ ] Show `Empty` state in grey for unassigned slots
- [ ] Allow user to assign a spool to a slot by selecting from the spool list
- [ ] Allow user to unassign a slot
- [ ] Prevent assigning the same spool to two slots — show error message

---

## AMS MQTT Payload Reference

| Field | Description |
|---|---|
| `print.ams.ams[n].id` | AMS unit index |
| `print.ams.ams[n].tray[n].id` | Slot index 0–3 |
| `print.filament_weight` | Total grams used in the print |

> Spoolarr ignores Bambu's own `remain` percentage and uses its own gram tracking for accuracy.

---

## API Reference

| Method | Endpoint | Returns |
|---|---|---|
| `GET` | `/api/ams` | `200` list of all slots |
| `PUT` | `/api/ams/{unit}/{slot}` | `200` updated slot or `404` |

---

## Definition of Done

- [ ] `AmsSlots` table created by migration
- [ ] Spools can be assigned and unassigned from slots via the API
- [ ] Print-finish event deducts from the correct slot's spool
- [ ] Warning is logged when a slot has no spool assigned
- [ ] Non-AMS prints still fall back to the active spool correctly
- [ ] AMS panel shows in the dashboard with live slot data
- [ ] All new endpoints tested with Postman or curl
- [ ] Assigning the same spool to two slots returns `409 Conflict`
