# Milestone 1 — Data Model

> Define the database entities, run EF Core migrations, set up repositories, and seed test data.

---

## Goal

By the end of this milestone the database has the correct tables for spools and print jobs, repositories are in place to read and write data, and seed data exists so you can test without manually inserting rows.

---

## Depends On

- Milestone 0 — Project Bootstrap

---

## Tasks

### Models
- [ ] Create `Spool` entity class inside `Domain/Models/`
- [ ] Create `PrintJob` entity class inside `Domain/Models/`

### Spool fields
- [ ] `Id` — Guid, primary key
- [ ] `NfcTagUid` — string, unique
- [ ] `Brand` — string
- [ ] `Material` — string
- [ ] `ColorName` — string
- [ ] `ColorHex` — string
- [ ] `InitialWeightG` — float
- [ ] `CurrentWeightG` — float
- [ ] `LowStockThresholdG` — float, default 100g
- [ ] `IsActive` — bool, default false
- [ ] `CreatedAt` — DateTime
- [ ] `LastScannedAt` — DateTime, nullable
- [ ] `Notes` — string, nullable
- [ ] `PrintJobs` — navigation property to `PrintJob`

### PrintJob fields
- [ ] `Id` — Guid, primary key
- [ ] `SpoolId` — Guid, foreign key to `Spool`
- [ ] `GramsUsed` — float
- [ ] `PrintName` — string, nullable
- [ ] `PrintedAt` — DateTime
- [ ] `Source` — string, default `"mqtt"`
- [ ] `Spool` — navigation property to `Spool`

### Database context
- [ ] Add `DbSet<Spool>` to `FilamentDbContext`
- [ ] Add `DbSet<PrintJob>` to `FilamentDbContext`
- [ ] Configure unique index on `Spool.NfcTagUid`
- [ ] Configure cascade delete — deleting a spool deletes its print jobs
- [ ] Configure foreign key relationship between `PrintJob` and `Spool`

### Migrations
- [ ] Run `dotnet ef migrations add InitialCreate` — run from the `Infrastructure` project
- [ ] Run `dotnet ef database update` — run from the `Infrastructure` project
- [ ] Confirm `Spools` and `PrintJobs` tables are created in the SQLite file

### Repositories
- [ ] Create `ISpoolRepository` interface inside `Infrastructure/Repositories/`
- [ ] Create `SpoolRepository` class implementing `ISpoolRepository`
- [ ] Add `GetAllAsync` — return all spools ordered by last scanned
- [ ] Add `GetByIdAsync` — return spool by ID including print jobs
- [ ] Add `GetByNfcTagUidAsync` — return spool by NFC tag UID
- [ ] Add `GetActiveAsync` — return the currently active spool
- [ ] Add `CreateAsync` — insert new spool
- [ ] Add `UpdateAsync` — update existing spool
- [ ] Add `DeleteAsync` — delete spool by ID
- [ ] Create `IPrintJobRepository` interface inside `Infrastructure/Repositories/`
- [ ] Create `PrintJobRepository` class implementing `IPrintJobRepository`
- [ ] Add `GetBySpoolIdAsync` — return all print jobs for a spool ordered by date
- [ ] Add `CreateAsync` — insert new print job

### Dependency injection
- [ ] Register `ISpoolRepository` → `SpoolRepository` in `Program.cs`
- [ ] Register `IPrintJobRepository` → `PrintJobRepository` in `Program.cs`

### Seed data
- [ ] Create `SeedData` class inside `Infrastructure/Data/`
- [ ] Add at least 2 test spools with different materials and colors
- [ ] Call `SeedData` on app startup only if the database is empty
- [ ] Run migrations automatically on app startup
- [ ] Handle the case where the database file already exists — do not re-seed

### Error handling
- [ ] Wrap migration on startup in try/catch — log error and continue if migration fails
- [ ] Log a clear message if the database file cannot be created or accessed

---

## Definition of Done

- [ ] `Spools` and `PrintJobs` tables exist in the SQLite database
- [ ] `NfcTagUid` has a unique index
- [ ] Cascade delete works — deleting a spool removes its print jobs
- [ ] Both repositories registered in DI and resolve without errors
- [ ] Seed data inserts test spools on first run only
- [ ] `dotnet ef migrations list` shows `InitialCreate` as applied
- [ ] Inserting a duplicate `NfcTagUid` throws a database constraint error
- [ ] Deleting a spool also deletes all its associated print jobs (cascade delete verified)
