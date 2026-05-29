# Milestone 1 — Data Model

> Define the database entities, run EF Core migrations, set up repositories, and seed test data.

---

## Goal

By the end of this milestone the database has four tables — `Spools`, `Printers`, `PrintJobs`, and `NfcTags` — with the correct fields, relationships, and indexes. Repositories are in place to read and write data. Seed data exists so the app can be tested without manually inserting rows.

---

## Depends On

- Milestone 0 — Project Bootstrap

---

## Tasks

### Models
- [x] Create `Spool` entity class inside `src/back-end/Domain/Models/`
- [x] Create `Printer` entity class inside `src/back-end/Domain/Models/`
- [x] Create `PrintJob` entity class inside `src/back-end/Domain/Models/`
- [x] Create `NfcTag` entity class inside `src/back-end/Domain/Models/`

### Spool fields
- [x] `Id` — Guid, primary key
- [x] `Brand` — string
- [x] `Material` — string
- [x] `ColorName` — string
- [x] `ColorHex` — string
- [x] `InitialWeightG` — float
- [x] `CurrentWeightG` — float
- [x] `SpoolWeightG` — float, default 200
- [x] `DiameterMm` — float, default 1.75
- [x] `LowStockThresholdG` — float, default 100
- [x] `IsActive` — bool, default false
- [x] `IsArchived` — bool, default false
- [x] `CreatedAt` — DateTime
- [x] `LastScannedAt` — DateTime, nullable
- [x] `Notes` — string, nullable
- [x] `NfcTags` — navigation property to `NfcTag` (one spool can have one or more tags)
- [x] `PrintJobs` — navigation property to `PrintJob`

### Printer fields
- [x] `Id` — Guid, primary key
- [x] `Name` — string, user label (e.g. "Garage X1C")
- [x] `Brand` — string (e.g. "Bambu Lab", "Prusa", "Voron")
- [x] `Model` — string (e.g. "X1 Carbon", "MK4", "P1S")
- [x] `SerialNumber` — string, nullable
- [x] `IpAddress` — string
- [x] `Protocol` — string (values: `"bambu-mqtt"`, `"moonraker"`, `"prusalink"`, `"octoprint"`)
- [x] `AccessCode` — string, nullable (Bambu LAN access code or equivalent)
- [x] `Port` — int, nullable
- [x] `HasAms` — bool, default false
- [x] `AmsSlotCount` — int, default 0
- [x] `IsActive` — bool, default true
- [x] `LastSeenAt` — DateTime, nullable
- [x] `CreatedAt` — DateTime
- [x] `Notes` — string, nullable
- [x] `PrintJobs` — navigation property to `PrintJob`

### PrintJob fields
- [x] `Id` — Guid, primary key
- [x] `PrinterId` — Guid, foreign key to `Printer`
- [x] `SpoolId` — Guid, foreign key to `Spool`
- [x] `PrintFileName` — string, nullable
- [x] `Status` — string (values: `"running"`, `"completed"`, `"failed"`, `"interrupted"`)
- [x] `GramsUsed` — float, default 0
- [x] `LastReportedGramsUsed` — float, default 0
- [x] `StartedAt` — DateTime
- [x] `FinishedAt` — DateTime, nullable
- [x] `LastUpdatedAt` — DateTime
- [x] `Source` — string, default `"mqtt"` (values: `"mqtt"`, `"manual"`)
- [x] `Notes` — string, nullable
- [x] `Printer` — navigation property to `Printer`
- [x] `Spool` — navigation property to `Spool`

### NfcTag fields
- [x] `Id` — Guid, primary key
- [x] `TagUid` — string, unique (physical NFC tag UID, e.g. "04:A1:B2:C3")
- [x] `Type` — string (values: `"ntag215"`, `"bambu"`, `"openprinttag"`, `"opentag3d"`)
- [x] `SpoolId` — Guid, foreign key to `Spool`
- [x] `CreatedAt` — DateTime
- [x] `Spool` — navigation property to `Spool`

> NFC tags are read-only in Spoolarr — the app never writes to them.

### Database context
- [x] Add `DbSet<Spool>` to `FilamentDbContext`
- [x] Add `DbSet<Printer>` to `FilamentDbContext`
- [x] Add `DbSet<PrintJob>` to `FilamentDbContext`
- [x] Add `DbSet<NfcTag>` to `FilamentDbContext`
- [x] Configure unique index on `NfcTag.TagUid`
- [x] Configure foreign key relationship between `NfcTag` and `Spool`
- [x] Configure foreign key relationship between `PrintJob` and `Spool`
- [x] Configure foreign key relationship between `PrintJob` and `Printer`
- [x] Configure cascade delete — deleting a spool deletes its NFC tags
- [x] Configure cascade delete — deleting a spool deletes its print jobs
- [x] Configure cascade delete — deleting a printer deletes its print jobs

### Migrations
- [x] Run `dotnet ef migrations add InitialCreate` — run from the `Infrastructure` project
- [x] Run `dotnet ef database update` — run from the `Infrastructure` project
- [x] Confirm `Spools`, `Printers`, `PrintJobs`, and `NfcTags` tables are created in the SQLite file

### Spool repository
- [ ] Create `ISpoolRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `SpoolRepository` class implementing `ISpoolRepository`
- [ ] Add `GetAllAsync` — return all non-archived spools ordered by last scanned
- [ ] Add `GetByIdAsync` — return spool by ID including NFC tags and print jobs
- [ ] Add `GetActiveAsync` — return the currently active spool
- [ ] Add `CreateAsync` — insert new spool
- [ ] Add `UpdateAsync` — update existing spool
- [ ] Add `ArchiveAsync` — set `IsArchived = true`
- [ ] Add `DeleteAsync` — delete spool by ID

### Printer repository
- [ ] Create `IPrinterRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `PrinterRepository` class implementing `IPrinterRepository`
- [ ] Add `GetAllAsync` — return all printers ordered by name
- [ ] Add `GetByIdAsync` — return printer by ID
- [ ] Add `GetActiveAsync` — return all printers where `IsActive = true`
- [ ] Add `CreateAsync` — insert new printer
- [ ] Add `UpdateAsync` — update existing printer
- [ ] Add `DeleteAsync` — delete printer by ID

### PrintJob repository
- [ ] Create `IPrintJobRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `PrintJobRepository` class implementing `IPrintJobRepository`
- [ ] Add `GetBySpoolIdAsync` — return all print jobs for a spool ordered by date
- [ ] Add `GetByPrinterIdAsync` — return all print jobs for a printer ordered by date
- [ ] Add `GetRunningAsync` — return all jobs with status `"running"`
- [ ] Add `GetByIdAsync` — return job by ID
- [ ] Add `CreateAsync` — insert new print job
- [ ] Add `UpdateAsync` — update existing print job (status, grams, finished date)

### NfcTag repository
- [ ] Create `INfcTagRepository` interface inside `src/back-end/Infrastructure/Repositories/`
- [ ] Create `NfcTagRepository` class implementing `INfcTagRepository`
- [ ] Add `GetByTagUidAsync` — return NFC tag by its physical UID, includes the linked Spool
- [ ] Add `GetBySpoolIdAsync` — return all NFC tags for a spool
- [ ] Add `CreateAsync` — insert new NFC tag linked to a spool
- [ ] Add `DeleteAsync` — delete NFC tag by ID

### Dependency injection
- [ ] Register `ISpoolRepository` → `SpoolRepository` in `Program.cs`
- [ ] Register `IPrinterRepository` → `PrinterRepository` in `Program.cs`
- [ ] Register `IPrintJobRepository` → `PrintJobRepository` in `Program.cs`
- [ ] Register `INfcTagRepository` → `NfcTagRepository` in `Program.cs`

### Seed data
- [ ] Create `SeedData` class inside `src/back-end/Infrastructure/Data/`
- [ ] Add at least 2 test spools with different materials and colors
- [ ] Add 1 test NFC tag linked to one of the test spools
- [ ] Add 1 test printer with placeholder values
- [ ] Call `SeedData` on app startup only if the database is empty
- [ ] Run migrations automatically on app startup
- [ ] Handle the case where the database file already exists — do not re-seed

### Error handling
- [ ] Wrap migration on startup in try/catch — log error and continue if migration fails
- [ ] Log a clear message if the database file cannot be created or accessed

---

## Definition of Done

- [ ] `Spools`, `Printers`, `PrintJobs`, and `NfcTags` tables exist in the SQLite database
- [ ] `NfcTag.TagUid` has a unique index
- [ ] All foreign keys and cascade deletes work correctly
- [ ] All 4 repositories registered in DI and resolve without errors
- [ ] Seed data inserts test rows on first run only
- [ ] `dotnet ef migrations list` shows `InitialCreate` as applied
- [ ] Inserting a duplicate `TagUid` throws a database constraint error
- [ ] Deleting a spool also deletes its NFC tags and print jobs
- [ ] Deleting a printer also deletes its print jobs