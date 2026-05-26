# Milestone 1 — Data Model

> Define the database entities, run EF Core migrations, set up repositories, and seed test data.

---

## Goal

By the end of this milestone the database has the correct tables for spools and prints, repositories are in place to read and write data, and seed data exists so you can test without manually inserting rows.

---

## Depends On

- Milestone 0 — Project Bootstrap

---

## Tasks

### Models
- [ ] Create `Spool` entity class inside `Domain/Models/`
- [ ] Create `Print` entity class inside `Domain/Models/`
- [ ] Create `PrintSource` enum inside `Domain/Models/` with values: `Mqtt`, `Nfc`, `Manual`

### Spool fields
- [ ] `Id` — Guid, primary key
- [ ] `NfcTagUid` — string, unique
- [ ] `Brand` — string
- [ ] `Material` — string
- [ ] `ColorName` — string
- [ ] `ColorHex` — string, format `#RRGGBB`
- [ ] `InitialWeightG` — double
- [ ] `CurrentWeightG` — double
- [ ] `LowStockThresholdG` — double, default 100
- [ ] `IsActive` — bool, default false
- [ ] `CreatedAt` — DateTime
- [ ] `UpdatedAt` — DateTime
- [ ] `LastScannedAt` — DateTime, nullable
- [ ] `Notes` — string, nullable
- [ ] `Prints` — navigation property to `Print`

### Print fields
- [ ] `Id` — Guid, primary key (internal)
- [ ] `SpoolId` — Guid, foreign key to `Spool`
- [ ] `BambuTaskId` — string, nullable — Bambu MQTT `task_id` (unique job ID assigned by Bambu)
- [ ] `BambuSerialNumber` — string, nullable — Bambu MQTT `subtask_id` (unique serial per print run)
- [ ] `Name` — string, nullable — Bambu MQTT `subtask_name` (print file name e.g. `model.3mf`)
- [ ] `GcodeFile` — string, nullable — Bambu MQTT `gcode_file` (gcode path on the printer)
- [ ] `GramsUsed` — double
- [ ] `PrintedAt` — DateTime
- [ ] `Source` — `PrintSource` enum, default `PrintSource.Mqtt`
- [ ] `Spool` — navigation property to `Spool`

### Database context
- [ ] Add `DbSet<Spool>` to `FilamentDbContext`
- [ ] Add `DbSet<Print>` to `FilamentDbContext`
- [ ] Configure unique index on `Spool.NfcTagUid`
- [ ] Configure index on `Print.BambuTaskId` — for fast MQTT job lookup
- [ ] Configure index on `Print.BambuSerialNumber` — for fast serial lookup
- [ ] Configure cascade delete — deleting a spool deletes its prints
- [ ] Configure foreign key relationship between `Print` and `Spool`
- [ ] Configure `PrintSource` to be stored as a string in the database (use `HasConversion<string>()`)

### Migrations
- [ ] Run migrations add from `src/backend/` — `dotnet ef migrations add InitialCreate --project Infrastructure --startup-project API`
- [ ] Run database update from `src/backend/` — `dotnet ef database update --project Infrastructure --startup-project API`
- [ ] Confirm `Spools` and `Prints` tables are created in the SQLite file

### Repositories
- [ ] Create `ISpoolRepository` interface inside `Application/Interfaces/`
- [ ] Create `SpoolRepository` class inside `Infrastructure/Repositories/` implementing `ISpoolRepository`
- [ ] Add `GetAllAsync` — return all spools ordered by `LastScannedAt` descending
- [ ] Add `GetByIdAsync` — return spool by ID including prints
- [ ] Add `GetByNfcTagUidAsync` — return spool by NFC tag UID
- [ ] Add `GetActiveAsync` — return the currently active spool
- [ ] Add `CreateAsync` — insert new spool
- [ ] Add `UpdateAsync` — update existing spool
- [ ] Add `DeleteAsync` — delete spool by ID
- [ ] Create `IPrintRepository` interface inside `Application/Interfaces/`
- [ ] Create `PrintRepository` class inside `Infrastructure/Repositories/` implementing `IPrintRepository`
- [ ] Add `GetBySpoolIdAsync` — return all prints for a spool ordered by `PrintedAt` descending
- [ ] Add `GetByIdAsync` — return a single print by ID
- [ ] Add `CreateAsync` — insert new print

### Dependency injection
- [ ] Register `ISpoolRepository` → `SpoolRepository` in `Program.cs`
- [ ] Register `IPrintRepository` → `PrintRepository` in `Program.cs`

### Seed data
- [ ] Create `SeedData` class inside `Infrastructure/Data/`
- [ ] Add 2 test spools with realistic data (e.g. Bambu PLA Basic Black, Bambu PETG Basic White)
- [ ] Call `SeedData` on app startup only if the `Spools` table is empty
- [ ] Run migrations automatically on app startup before seeding
- [ ] Do not re-seed if data already exists

### Error handling
- [ ] Wrap migration and seed on startup in try/catch — log the error and continue if it fails
- [ ] Log a clear message if the database file cannot be created or accessed

---

## Definition of Done

- [ ] `Spools` and `Prints` tables exist in the SQLite database
- [ ] `Spool.NfcTagUid` has a unique index
- [ ] `Print.BambuTaskId` and `Print.BambuSerialNumber` have indexes
- [ ] Cascade delete works — deleting a spool removes its prints
- [ ] Both repositories registered in DI and resolve without errors
- [ ] Seed data inserts 2 test spools on first run only
- [ ] `dotnet ef migrations list` shows `InitialCreate` as applied
- [ ] Inserting a duplicate `NfcTagUid` throws a database constraint error
- [ ] `PrintSource` is stored as a string in the database (not an integer)
